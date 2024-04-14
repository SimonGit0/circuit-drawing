Public Class WR_Einstellungsform

    Private panel_main1 As Panel
    Private panel_main2 As Panel

    Private myEinstellungen As List(Of ElementEinstellung)

    Public Sub New()
        InitializeComponent()

        Me.SetStyle(ControlStyles.ContainerControl, True)
        Me.SetStyle(ControlStyles.Selectable, False)

        panel_main1 = New Panel()
        panel_main1.Location = New Point(0, 0)
        panel_main1.Width = Me.Width
        panel_main1.Height = Button1.Location.Y - 6
        panel_main1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        Me.Controls.Add(panel_main1)
        panel_main1.AutoScroll = True

        panel_main2 = New Panel()
        panel_main2.Location = New Point(0, 0)
        panel_main2.Width = Me.Width
        panel_main2.Height = Button1.Location.Y - 6
        panel_main2.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        Me.Controls.Add(panel_main2)
        panel_main2.AutoScroll = True
    End Sub

    Public Sub setEinstellungen(einstellungen As List(Of ElementEinstellung))
        If sindEinstellungenGleich(einstellungen) Then
            'nur Einstellungen aktualisieren!
            aktualisiereEinstellungen(einstellungen)
        Else
            If myEinstellungen IsNot Nothing Then
                For i As Integer = 0 To myEinstellungen.Count - 1
                    RemoveHandler myEinstellungen(i).EinstellungLiveChanged, AddressOf Einstellungen_Changed
                Next
            End If
            myEinstellungen = einstellungen

            If einstellungen Is Nothing Then
                Label1.Visible = True
                panel_main1.Visible = False
                panel_main2.Visible = False
                Button1.Visible = False
            Else
                Dim panel_main As Panel
                Dim panel_secondary As Panel
                If panel_main1.Visible = False Then
                    panel_main = panel_main1
                    panel_secondary = panel_main2
                ElseIf panel_main2.Visible = False Then
                    panel_main = panel_main2
                    panel_secondary = panel_main1
                Else
                    Throw New Exception("Kein Panel verfügbar!")
                End If

                panel_main.AutoScroll = False 'Autoscroll ausschalten während des updates! -> ansonsten ändert sich schon die breite durch einfügen einer Scrollbar und dann ist panel_main.width nicht mehr korrekt (müsste panel_main.width - scrollbar.width dann sein!)
                Dim posY As Integer = 3
                panel_main.Controls.Clear()
                For i As Integer = 0 To einstellungen.Count - 1
                    Dim g As GroupBox = einstellungen(i).getGroupbox()
                    g.Width = panel_main.Width - 6
                    g.Location = New Point(3, posY)
                    posY += g.Height + 3
                    panel_main.Controls.Add(g)
                Next
                panel_main.AutoScroll = True
                Label1.Visible = False
                panel_main.BringToFront()
                panel_main.Visible = True
                panel_secondary.Visible = False
                Button1.Visible = True
            End If

            If myEinstellungen IsNot Nothing Then
                For i As Integer = 0 To myEinstellungen.Count - 1
                    AddHandler myEinstellungen(i).EinstellungLiveChanged, AddressOf Einstellungen_Changed
                Next
            End If
        End If
    End Sub

    Private Function sindEinstellungenGleich(einstellungen As List(Of ElementEinstellung)) As Boolean
        If einstellungen IsNot Nothing AndAlso myEinstellungen IsNot Nothing Then
            If einstellungen.Count <> myEinstellungen.Count Then Return False
            For i As Integer = 0 To einstellungen.Count - 1
                If einstellungen(i).GetType().ToString <> myEinstellungen(i).GetType().ToString Then
                    Return False
                End If
                If Not einstellungen(i).Name.is_equal(myEinstellungen(i).Name) Then
                    Return False
                End If
                If TypeOf einstellungen(i) Is Einstellung_Multi Then
                    If Not DirectCast(einstellungen(i), Einstellung_Multi).sindGleicheEinstellungen(myEinstellungen(i)) Then
                        Return False
                    End If
                End If
            Next
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub aktualisiereEinstellungen(einstellungen As List(Of ElementEinstellung))
        For i As Integer = 0 To myEinstellungen.Count - 1
            myEinstellungen(i).aktualisiere(einstellungen(i))
        Next
    End Sub

    Private Sub Einstellungen_Changed(sender As Object, e As EventArgs)
        RaiseEvent EinstellungenÜbernehmen(Me, myEinstellungen)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RaiseEvent EinstellungenÜbernehmen(Me, myEinstellungen)
    End Sub

    Public Event EinstellungenÜbernehmen(sender As Object, liste As List(Of ElementEinstellung))
End Class
