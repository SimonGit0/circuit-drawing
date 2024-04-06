Public Class MarginEinstellen

    Public Property Value As Padding
        Get
            Return New Padding(CInt(NumericUpDown1.Value), CInt(NumericUpDown3.Value), CInt(NumericUpDown2.Value), CInt(NumericUpDown4.Value))
        End Get
        Set(value As Padding)
            NumericUpDown1.Value = value.Left
            NumericUpDown2.Value = value.Right
            NumericUpDown3.Value = value.Top
            NumericUpDown4.Value = value.Bottom
        End Set
    End Property

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class