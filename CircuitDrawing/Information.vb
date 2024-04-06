Public Class Information

    Private erstelltVon As String = "Simon Buhr"
    Private erstelltMit As String = "Visual Basic .Net Framework"
    Private version As String = "1.3"
    Private datum As String = AutoGenDate.LastBuildData.ToString("dd.MM.yyyy")
    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
    End Sub

    Private Sub Information_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label3.Text = vbCrLf & vbCrLf &
                      erstelltVon & vbCrLf &
                      erstelltMit & vbCrLf &
                      version & vbCrLf &
                      datum
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start(LinkLabel1.Text)
    End Sub
End Class