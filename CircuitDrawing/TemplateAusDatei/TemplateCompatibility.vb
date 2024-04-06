Public Class TemplateCompatibility
    Public _name As String
    Public _Namespace As String

    Public listPreset As List(Of default_Parameter) 'nur zum Einlesen
    Private paramsQuelle() As default_Parameter

    Public ReadOnly parent As TemplateAusDatei

    Public dx As Integer
    Public dy As Integer

    Public Sub New(parent As TemplateAusDatei)
        Me.parent = parent
        Me._name = ""
        Me._Namespace = ""
        Me.listPreset = New List(Of default_Parameter)
        Me.dx = 0
        Me.dy = 0
    End Sub

    Public Sub einladenFertig()
        ReDim paramsQuelle(listPreset.Count - 1)
        For i As Integer = 0 To listPreset.Count - 1
            paramsQuelle(i) = New default_Parameter(listPreset(i).param.ToLower(), Mathe.strToLower(listPreset(i).value))
        Next
        listPreset.ToArray()
        listPreset.Clear()
        listPreset = Nothing
    End Sub

    Public Sub LoadParams(paramZiel() As ParamValue)
        Dim template As TemplateAusDatei = Me.parent
        template.__lade_defaultParameterValues(paramZiel, paramsQuelle, False, False)
    End Sub

    Public Sub setOffset(x As Integer, y As Integer)
        Me.dx = x
        Me.dy = y
    End Sub
End Class
