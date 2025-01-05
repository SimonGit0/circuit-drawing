Public Class Einstellung_Textausrichtung
    Inherits ElementEinstellung

    Public ah As DO_Textfeld.AlignH
    Public av As DO_Text.AlignV

    Public ah_changed As Boolean = False
    Private ah_various As Boolean = False

    Public av_changed As Boolean = False
    Private av_various As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private cmb_ah, cmb_av As JoSiCombobox

    Public Sub New(name As String, ah As DO_Textfeld.AlignH, av As DO_Text.AlignV)
        MyBase.New(SortierTyp.ElementEinstellung_Textfeld_Rotation, New Multi_Lang_String(name, Nothing))
        Me.ah = ah
        Me.av = av
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung, mode As combineModus)
        Dim e As Einstellung_Textausrichtung = DirectCast(e2, Einstellung_Textausrichtung)
        If e.ah <> Me.ah Then
            ah_various = True
        End If
        If e.av <> Me.av Then
            av_various = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Textausrichtung)
            Me.ah = .ah
            Me.av = .av

            Me.ah_changed = .ah_changed
            Me.ah_various = .ah_various

            Me.av_changed = .av_changed
            Me.av_various = .av_various

            If cmb_ah IsNot Nothing Then
                cmb_ah.Various = ah_various
                If ah = DO_Textfeld.AlignH.Links Then
                    cmb_ah.SelectedIndex = 0
                ElseIf ah = DO_Textfeld.AlignH.Mitte Then
                    cmb_ah.SelectedIndex = 1
                ElseIf ah = DO_Textfeld.AlignH.Rechts Then
                    cmb_ah.SelectedIndex = 2
                Else
                    cmb_ah.SelectedIndex = 3
                End If
            End If

            If cmb_av IsNot Nothing Then
                cmb_av.Various = av_various
                If av = DO_Text.AlignV.Oben Then
                    cmb_av.SelectedIndex = 0
                ElseIf av = DO_Text.AlignV.Mitte Then
                    cmb_av.SelectedIndex = 1
                Else
                    cmb_av.SelectedIndex = 2
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(2)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New JoSiCombobox()
        createLabelCombobox(lbl1, cmb1)
        lbl1.Text = My.Resources.Strings.Einstellung_Textausrichtung_Horizontal
        cmb1.Items.Add(My.Resources.Strings.Linksbündig)
        cmb1.Items.Add(My.Resources.Strings.Zentriert)
        cmb1.Items.Add(My.Resources.Strings.Rechtsbündig)
        cmb1.Items.Add(My.Resources.Strings.Blocksatz)
        cmb1.Various = ah_various
        If ah = DO_Textfeld.AlignH.Links Then
            cmb1.SelectedIndex = 0
        ElseIf ah = DO_Textfeld.AlignH.Mitte Then
            cmb1.SelectedIndex = 1
        ElseIf ah = DO_Textfeld.AlignH.Rechts Then
            cmb1.SelectedIndex = 2
        Else
            cmb1.SelectedIndex = 3
        End If
        cmb_ah = cmb1
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        liste.Add(zeile1)
        AddHandler cmb1.SelectedIndexChanged, AddressOf ahChanged

        Dim lbl2 As Label = Nothing
        Dim cmb2 As New JoSiCombobox()
        createLabelCombobox(lbl2, cmb2)
        lbl2.Text = My.Resources.Strings.Einstellung_Textausrichtung_Vertikal
        cmb2.Items.Add(My.Resources.Strings.Ausrichtung_Oben)
        cmb2.Items.Add(My.Resources.Strings.Ausrichtung_Mitte)
        cmb2.Items.Add(My.Resources.Strings.Ausrichtung_Unten)
        cmb2.Various = av_various
        If av = DO_Text.AlignV.Oben Then
            cmb2.SelectedIndex = 0
        ElseIf av = DO_Text.AlignV.Mitte Then
            cmb2.SelectedIndex = 1
        Else
            cmb2.SelectedIndex = 2
        End If
        cmb_av = cmb2
        Dim zeile2 As New List(Of Control)(2)
        zeile2.Add(lbl2)
        zeile2.Add(cmb2)
        liste.Add(zeile2)
        AddHandler cmb2.SelectedIndexChanged, AddressOf avChanged

        Return createGroupbox(liste)
    End Function

    Private Sub avChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex = 0 Then
                av = DO_Text.AlignV.Oben
                av_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 1 Then
                av = DO_Text.AlignV.Mitte
                av_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 2 Then
                av = DO_Text.AlignV.Unten
                av_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Private Sub ahChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex = 0 Then
                ah = DO_Textfeld.AlignH.Links
                ah_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 1 Then
                ah = DO_Textfeld.AlignH.Mitte
                ah_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 2 Then
                ah = DO_Textfeld.AlignH.Rechts
                ah_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 3 Then
                ah = DO_Textfeld.AlignH.Blocksatz
                ah_changed = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub
End Class
