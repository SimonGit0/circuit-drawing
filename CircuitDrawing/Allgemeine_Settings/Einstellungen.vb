Public Class Einstellungen
    Private bib As Bibliothek
    Private FRM_BauelementeAuswahl As WR_BauelementeAuswahl
    Private pic As Vektor_Picturebox
    Public Sub New(bib As Bibliothek, FRM_BauelementeAuswahl As WR_BauelementeAuswahl, pic As Vektor_Picturebox)
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
        Me.bib = bib
        Me.FRM_BauelementeAuswahl = FRM_BauelementeAuswahl
        Me.pic = pic
    End Sub

    Private myPanels As List(Of IEinstellungspanel)
    Private selectedPanel As Integer

    Private Sub Einstellungen_Load(sender As Object, e As EventArgs) Handles Me.Load
        myPanels = New List(Of IEinstellungspanel)

        Dim p1 As New Einstellungspanel_Bauelemente()
        p1.myBib = bib
        myPanels.Add(p1)

        Dim p5 As New Einstellungspanel_ShortcutKeysSelectBauteil()
        p5.bib = bib
        myPanels.Add(p5)

        Dim p2 As New Einstellungspanel_Anzeige()
        myPanels.Add(p2)

        Dim p3 As New Einstellungspanel_Sprache()
        myPanels.Add(p3)

        Dim p4 As New Einstellungspanel_LatexExport()
        myPanels.Add(p4)

        Dim p6 As New Einstellungspanel_Tools()
        myPanels.Add(p6)

        selectedPanel = 0

        'richtige werte anzeigen
        For i As Integer = 0 To myPanels.Count - 1
            myPanels(i).InitValues()
        Next

        For i As Integer = 0 To myPanels.Count - 1
            Dim item As New ListboxItemText(myPanels(i).getName())
            item.Height = 32
            item.offsetX = 6
            UserDrawListbox1.addItem(item)
        Next
        UserDrawListbox1.SelectedIndex = selectedPanel

        show_selectedPanel()
    End Sub

    Private Sub UserDrawListbox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles UserDrawListbox1.SelectedIndexChanged
        selectedPanel = UserDrawListbox1.SelectedIndex
        show_selectedPanel()
    End Sub

    Private Sub show_selectedPanel()
        Dim pl As Panel = myPanels(selectedPanel).getPanel()
        pl.Dock = DockStyle.Fill

        PMaster.SuspendLayout()
        PMaster.Controls.Clear()
        PMaster.Controls.Add(pl)
        PMaster.ResumeLayout()

        myPanels(selectedPanel).OnShown()
    End Sub

    Private Sub set_default_of_selected()
        myPanels(selectedPanel).Set_Default()
        myPanels(selectedPanel).InitValues()
    End Sub

    Private Function speichern() As Boolean
        Dim erfolg As Boolean = True
        Dim args As New EinstellungSpeichernArgs(bib, FRM_BauelementeAuswahl, pic)
        For i As Integer = 0 To myPanels.Count - 1
            If Not myPanels(i).SpeicherValues(args) Then
                erfolg = False
            End If
        Next
        Return erfolg
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Settings.loadSettings()
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If speichern() Then
            Settings.getSettings().saveSettings()
            Me.Close()
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        set_default_of_selected()
    End Sub
End Class