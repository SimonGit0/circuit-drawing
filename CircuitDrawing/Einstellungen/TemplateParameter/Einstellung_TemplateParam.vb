Public MustInherit Class Einstellung_TemplateParam
    Inherits ElementEinstellung

    Public Sub New(name As String)
        MyBase.New(name)
    End Sub

    Public MustOverride Function isSameParameter(e2 As Einstellung_TemplateParam) As Boolean

    Public MustOverride Function getControlListe() As List(Of List(Of Control))

End Class
