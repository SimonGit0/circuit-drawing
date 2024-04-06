Public MustInherit Class VariablenWert
End Class

Public Class VariablenWertInt
    Inherits VariablenWert
    Public wert As Integer
    Public Sub New(wert As Integer)
        Me.wert = wert
    End Sub
End Class

Public Class VariablenWertString
    Inherits VariablenWert
    Public wert As String
    Public Sub New(wert As String)
        Me.wert = wert
    End Sub
End Class

Public Class VariablenWertBoolean
    Inherits VariablenWert
    Public wert As Boolean
    Public Sub New(wert As Boolean)
        Me.wert = wert
    End Sub
End Class