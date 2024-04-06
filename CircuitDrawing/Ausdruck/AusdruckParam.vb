Public Class AusdruckParam
    Inherits Ausdruck

    Public ReadOnly ParamNr As Integer
    Public ReadOnly VarNr As Integer
    Public Sub New(ParamNr As Integer, varNr As Integer)
        Me.ParamNr = ParamNr
        Me.VarNr = varNr
    End Sub

    Public Overrides Function vereinfacheSoweitMöglich() As Ausdruck
        Return Me
    End Function
End Class
