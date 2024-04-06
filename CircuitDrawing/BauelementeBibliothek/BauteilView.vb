Public Class BauteilView
    Public ReadOnly view As String
    Public ReadOnly template As TemplateAusDatei
    Private parent As BauteilCell

    Public Sub New(v As String, template As TemplateAusDatei, parent As BauteilCell)
        Me.view = v
        Me.template = template
        Me.parent = parent
    End Sub

    Public Function getFullName() As String
        Return parent.Name & "." & Me.view
    End Function

    Public Function sucheBauteil_Compatibility(_namespace As String, _name As String) As TemplateCompatibility
        Return template.sucheBauteil_Compatibility(_namespace, _name)
    End Function
End Class
