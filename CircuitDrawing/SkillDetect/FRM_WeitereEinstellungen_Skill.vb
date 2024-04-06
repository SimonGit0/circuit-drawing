Public Class FRM_WeitereEinstellungen_Skill
    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle

        CheckBox1.Checked = Settings.getSettings().Skill_Detect_removeFloatingElements
        CheckBox2.Checked = Settings.getSettings().Skill_Detect_removeFloatingWires
        CheckBox3.Checked = Settings.getSettings().Skill_Detect_removeDummys
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Settings.getSettings().Skill_Detect_removeFloatingElements = CheckBox1.Checked
        Settings.getSettings().Skill_Detect_removeFloatingWires = CheckBox2.Checked
        Settings.getSettings().Skill_Detect_removeDummys = CheckBox3.Checked
        Settings.getSettings().saveSettings()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class