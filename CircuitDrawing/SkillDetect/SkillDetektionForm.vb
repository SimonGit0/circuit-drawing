Imports CircuitDrawing.Detektion
Public Class SkillDetektionForm
    Private mySkillSchematic As SkillDetectSchematic
    Public bib As Bibliothek

    Private _step As Integer = 0
    Private lastSkalierung As Integer = -1
    Private last_cmbSelectedIndex As Integer = -1

    Private linesAlles As List(Of String)

    Private initFertig As Boolean = False

    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle

        Combobox_Sortieren2.SelectedIndex = 0

        Me.ckb_Terminals.Visible = False
        Me.Ckb_Wire.Visible = False

        Me.KeyPreview = True
    End Sub

    Public Sub setLines(lines As List(Of String), bib As Bibliothek)
        Me.bib = bib
        mySkillSchematic = New SkillDetectSchematic(lines)
        Me.linesAlles = lines
    End Sub

    Private Sub SkillDetektionForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Dim fehlendeBauteile As New List(Of MasterElemente)
        Dim zuordnungen As List(Of Skill_BauteilZuordnung) = mySkillSchematic.getBauteilZuordnungen("CadenceImportSkill", fehlendeBauteile)
        If fehlendeBauteile.Count > 0 Then
            'es fehlen noch Bauteile!!!
            Dim frm As New Skill_BauelementeAuswählen()
            frm.init(Me.bib, fehlendeBauteile, Nothing, "CadenceImportSkill")
            If frm.ShowDialog(Me) = DialogResult.OK Then
                zuordnungen.AddRange(frm.NeueZuordnungen)
            Else
                Me.DialogResult = DialogResult.Abort
                Me.Close()
                Return
            End If
        End If
        mySkillSchematic.ordneBauteileZu(bib, zuordnungen)
        mySkillSchematic.setWireConnections()
        mySkillSchematic.solveDirectConnections()
        Place()
        initFertig = True
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim fehlendeBauteile As New List(Of MasterElemente)
        Dim zuordnungen As List(Of Skill_BauteilZuordnung) = mySkillSchematic.getBauteilZuordnungen("CadenceImportSkill", fehlendeBauteile)
        For i As Integer = 0 To fehlendeBauteile.Count - 1
            zuordnungen.Insert(0, Nothing)
        Next
        For i As Integer = 0 To zuordnungen.Count - 1
            If zuordnungen(i) IsNot Nothing Then
                fehlendeBauteile.Add(mySkillSchematic.getMaster(zuordnungen(i).libName_Skill, zuordnungen(i).cellName_Skill))
            End If
        Next
        If fehlendeBauteile.Count > 0 Then
            'es fehlen noch Bauteile!!!
            Dim frm As New Skill_BauelementeAuswählen()
            Dim neueBib As Bibliothek = bib.FlatCopyOhneLocal()
            frm.init(neueBib, fehlendeBauteile, zuordnungen, "CadenceImportSkill")
            If frm.ShowDialog(Me) = DialogResult.OK Then
                Me.bib = neueBib
                zuordnungen.Clear()
                zuordnungen.AddRange(frm.NeueZuordnungen)
            Else
                Return
            End If
        End If
        mySkillSchematic = New SkillDetectSchematic(Me.linesAlles)
        mySkillSchematic.ordneBauteileZu(bib, zuordnungen)
        mySkillSchematic.setWireConnections()
        mySkillSchematic.solveDirectConnections()
        lastSkalierung = -1
        Place()
    End Sub

    Private Sub Place()
        If lastSkalierung <> CInt(Me.nudSkalierung.Value) OrElse Combobox_Sortieren2.SelectedIndex <> last_cmbSelectedIndex Then
            lastSkalierung = CInt(Me.nudSkalierung.Value)
            last_cmbSelectedIndex = Combobox_Sortieren2.SelectedIndex

            mySkillSchematic.reset_unconected_objects()
            Dim changed As Boolean = True
            While changed
                changed = False
                If mySkillSchematic.delete_unconected_devices(Settings.getSettings().Skill_Detect_removeFloatingElements) Then changed = True
                If mySkillSchematic.deleteDummys(Settings.getSettings().Skill_Detect_removeDummys) Then changed = True
                If mySkillSchematic.delete_wireStubs(Settings.getSettings().Skill_Detect_removeFloatingWires) Then changed = True
            End While

            mySkillSchematic.sortiereInsts(Combobox_Sortieren2.getSortierung())
            mySkillSchematic.placeElements(Vektor_Picturebox1, lastSkalierung)
            mySkillSchematic.routeUngefähr(Vektor_Picturebox1)
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Select Case _step
            Case 0
                _step = 1
                Place()
                mySkillSchematic.deleteBauteile_markedToDelete(Vektor_Picturebox1)
                'mySkillSchematic.routeDetail(Vektor_Picturebox1)
                mySkillSchematic.routeDetail2(Vektor_Picturebox1)
                Button1.Text = My.Resources.Strings.Skill_PostRoutingOptimierung
                nudSkalierung.Visible = False
                lblSkalierung.Visible = False
                Button2.Visible = False
                Button5.Visible = False
                Combobox_Sortieren2.Visible = False
                ckb_Terminals.Visible = True
                Ckb_Wire.Visible = True
            Case 1
                If Ckb_Wire.Checked Then
                    Vektor_Picturebox1.RoutingVerbessern(True, False, 450)
                End If
                If ckb_Terminals.Checked Then
                    mySkillSchematic.TerminalsVerschieben(Vektor_Picturebox1)
                End If
                _step = 2
                Button1.Enabled = False
                ckb_Terminals.Visible = False
                Ckb_Wire.Visible = False
        End Select
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        While _step < 2
            Button1_Click(Nothing, EventArgs.Empty)
        End While
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Combobox_Sortieren2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Combobox_Sortieren2.SelectedIndexChanged
        If initFertig Then
            Place()
        End If
    End Sub

    Private Sub nudSkalierung_ValueChanged(sender As Object, e As EventArgs) Handles nudSkalierung.ValueChanged
        If initFertig Then
            Place()
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim frm As New FRM_WeitereEinstellungen_Skill()
        If frm.ShowDialog(Me) = DialogResult.OK Then
            lastSkalierung = -1
            Place()
        End If
    End Sub

    Protected Overrides Sub OnPreviewKeyDown(e As PreviewKeyDownEventArgs)
        e.IsInputKey = True
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Dim ctr As Control = Me.ActiveControl()

        If Not TypeOf ctr Is TextBox AndAlso
           Not TypeOf ctr Is RichTextBox AndAlso
           Not TypeOf ctr Is WR_Einstellungsform AndAlso
           Not TypeOf ctr Is NumericUpDown Then

            Dim k As Key_Settings = Key_Settings.getSettings()

            If e.KeyCode = Keys.Left OrElse e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down OrElse e.KeyCode = Keys.Right Then
                If ctr.Equals(Vektor_Picturebox1) Then
                    Vektor_Picturebox1.KeyDownRaised(e)
                End If
            Else
                If k.keyFitToScreen.isDown(e) Then
                    Vektor_Picturebox1.fit_to_screen()
                End If
            End If
        End If

        MyBase.OnKeyDown(e)
    End Sub
End Class
