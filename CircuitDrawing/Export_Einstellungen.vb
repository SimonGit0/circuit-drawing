Public Class Export_Einstellungen

    Public Sub New()
        InitializeComponent()
        Me.Textbox_mitUnit1.unit = "mm"
    End Sub

    Private myScale As Single
    Public Property skalierung As Single
        Get
            Return myScale
        End Get
        Set(value As Single)
            Textbox_mitUnit1.setText_ohneUnit(value.ToString)
            myScale = value
        End Set
    End Property

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim str As String = Textbox_mitUnit1.getText_ohneUnit()
        If Not Single.TryParse(str, myScale) OrElse myScale <= 0.0F Then
            MessageBox.Show("Fehler, keine gültige Skalierung. Geben Sie eine positive Zahl ein.", "Fehler")
        Else
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub
End Class