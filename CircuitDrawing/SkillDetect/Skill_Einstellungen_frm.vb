Public Class Skill_Einstellungen_frm
    Private myIntelisense As Intelisense
    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Dim default_Val As New List(Of default_Parameter)
            For i As Integer = 0 To txtDefaultParam.Lines.Count - 1
                Dim line As String = txtDefaultParam.Lines(i).Trim
                If line <> "" Then
                    Einstellungspanel_Bauelemente.readParameter(line, default_Val)
                End If
            Next
            Me.myEinstellungenClose = default_Val
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Fehler beim Übernehmen der Parameter: " & ex.Message, "Fehler beim Übernehmen")
            Me.myEinstellungenClose = Nothing
        End Try
    End Sub

    Public Sub init(einstellungen As List(Of default_Parameter), tmpl As TemplateAusDatei)
        Dim txt As String = ""
        For i As Integer = 0 To einstellungen.Count - 1
            If i <> 0 Then
                txt &= vbCrLf
            End If
            txt &= einstellungen(i).param & " = " & einstellungen(i).value
        Next
        txtDefaultParam.Text = txt

        Dim struktur As New List(Of IntelisenseEntry)
        For i As Integer = 0 To tmpl.getNrOfParams() - 1
            struktur.Add(New IntelisenseParameter(tmpl.getParameter(i)))
        Next

        Me.myIntelisense = New Intelisense(Me.txtDefaultParam, struktur)
    End Sub

    Private myEinstellungenClose As List(Of default_Parameter)

    Public Function getEinstellungen() As List(Of default_Parameter)
        Return myEinstellungenClose
    End Function
End Class