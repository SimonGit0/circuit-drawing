Imports System.IO
Public Class BauteilCell
    Public ReadOnly Name As String
    Private ReadOnly views As Dictionary(Of String, BauteilView)
    Private parent As BauteileNamespace

    Public Sub New(name As String, parent As BauteileNamespace)
        Me.Name = name
        Me.parent = parent
        views = New Dictionary(Of String, BauteilView)
    End Sub

    Public Sub add(v As TemplateAusDatei)
        If views.ContainsKey(v.getView()) Then
            'Throw New Exception("Der view '" + v.getView() + "' der Zelle '" + getFullName() + "' existiert schon.")
            Throw New Exception("Die Zelle '" & getFullName() & "' existiert schon.")
        Else
            views.Add(v.getView(), New BauteilView(v.getView(), v, Me))
        End If
    End Sub

    Public Function getViewCount() As Integer
        Return views.Count()
    End Function

    Public Function getFullName() As String
        Return parent.Name & "." & Me.Name
    End Function

    Public Function hasView(name As String) As Boolean
        Return views.ContainsKey(name)
    End Function

    Public Function getView(name As String) As BauteilView
        Return views(name)
    End Function

    Public Function getFirst() As BauteilView
        Return views.Values(0)
    End Function

    Public Sub reload_default_params()
        For Each v As BauteilView In views.Values
            v.template.reload_defaultParameterValues()
        Next
    End Sub

    Public Function sucheBauteil_Compatibility(_namespace As String, _name As String) As TemplateCompatibility
        For Each pair As KeyValuePair(Of String, BauteilView) In views
            Dim erg As TemplateCompatibility = pair.Value.sucheBauteil_Compatibility(_namespace, _name)
            If erg IsNot Nothing Then
                Return erg
            End If
        Next
        Return Nothing
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(Name)
        views("eu").template.speichern(writer)
    End Sub

    Public Shared Function laden(parent As BauteileNamespace, reader As BinaryReader, version As Integer) As BauteilCell
        Dim name As String = reader.ReadString()
        Dim erg As New BauteilCell(name, parent)
        erg.add(TemplateAusDatei.laden(reader, version))
        Return erg
    End Function
End Class
