Public Class FRM_ElementEinstellungen

    Public Sub New()
        InitializeComponent()

        Me.KeyPreview = True
        Me.Icon = My.Resources.iconAlle
    End Sub

    Public Sub init(einstellungen As List(Of ElementEinstellung))
        Me.DialogResult = DialogResult.Abort 'falls man über das schließen Kreuz schließt...

        Dim posY As Integer = 3
        For i As Integer = 0 To einstellungen.Count - 1
            Dim g As GroupBox = einstellungen(i).getGroupbox()
            g.Width = Panel_Main.Width - 6
            g.Location = New Point(3, posY)
            posY += g.Height + 3
            Panel_Main.Controls.Add(g)
        Next

    End Sub

    Private Sub Btn_Abbrechen_Click(sender As Object, e As EventArgs) Handles Btn_Abbrechen.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Btn_Ok_Click(sender As Object, e As EventArgs) Handles Btn_Ok.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Dim ctr As Control = Me.ActiveControl()
        If Not TypeOf ctr Is RichTextBox Then
            If TypeOf ctr IsNot TextBox OrElse DirectCast(ctr, TextBox).Multiline = False Then
                If e.KeyCode = Keys.Enter Then
                    Btn_Ok_Click(Me, EventArgs.Empty)
                End If
            End If
        End If
        MyBase.OnKeyDown(e)
    End Sub
End Class