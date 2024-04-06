Public Class BauelementeAuswahl

    Private myBib As Bibliothek

    Public Sub init(bib As Bibliothek)
        Me.myBib = bib
        For Each _namespace As KeyValuePair(Of String, BauteileNamespace) In bib
            ComboBox1.Items.Add(_namespace.Key)
        Next

        If bib.getNamespaceCount() > 0 Then
            ComboBox1.SelectedIndex = 0
        End If

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))
        If _namespace.getCellCount() > 0 Then
            Dim cells() As String = _namespace.getCellsNamen()

            ListBox1.SuspendLayout()
            ListBox1.Items.Clear()
            ListBox1.Items.AddRange(cells)
            ListBox1.ResumeLayout()

            ListBox1.SelectedIndex = 0

        Else
            ListBox1.Items.Clear()
        End If
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        If ComboBox1.SelectedIndex >= 0 AndAlso ComboBox1.Items.Count > 0 AndAlso ListBox1.Items.Count > 0 AndAlso ListBox1.SelectedIndex >= 0 Then
            Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))
            Dim _cell As BauteilCell = _namespace.getCell(CStr(ListBox1.SelectedItem))

            RaiseEvent OnTemplateChanged(Me, _cell)
        Else
            RaiseEvent OnTemplateChanged(Me, Nothing)
        End If
    End Sub

    Public Event OnTemplateChanged(sender As Object, cell As BauteilCell)

End Class
