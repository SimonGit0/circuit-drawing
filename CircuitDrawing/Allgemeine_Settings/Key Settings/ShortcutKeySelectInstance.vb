Public Class ShortcutKeySelectInstance
    Public key As ShortcutKey
    Public _namespace As String
    Public _cell As String

    Public Sub New(key As ShortcutKey, _namespace As String, cell As String)
        Me.key = key
        Me._namespace = _namespace
        Me._cell = cell
    End Sub
End Class
