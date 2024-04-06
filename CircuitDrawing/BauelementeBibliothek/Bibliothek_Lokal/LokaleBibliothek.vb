Public Class LokaleBibliothek

    Private namespaces As Dictionary(Of String, LokalerNamespace)

    Public Sub New()
        Me.namespaces = New Dictionary(Of String, LokalerNamespace)
    End Sub

    Public Sub add(tmpl As TemplateAusDatei)
        If Not namespaces.ContainsKey(tmpl.getNameSpace()) Then
            namespaces.Add(tmpl.getNameSpace(), New LokalerNamespace())
        End If
        namespaces(tmpl.getNameSpace()).add(tmpl)
    End Sub

    Public Function hatBauteil(ns As String, name As String) As Boolean
        If namespaces.ContainsKey(ns) Then
            Return namespaces(ns).hatBauteil(name)
        Else
            Return False
        End If
    End Function

    Public Function getBauteil(ns As String, name As String) As LokalesBauteil
        Return namespaces(ns).getBauteil(name)
    End Function
End Class

Public Class LokalerNamespace
    Private bauteile As Dictionary(Of String, LokalesBauteil)

    Public Sub New()
        Me.bauteile = New Dictionary(Of String, LokalesBauteil)
    End Sub

    Public Sub add(tmpl As TemplateAusDatei)
        If Not bauteile.ContainsKey(tmpl.getName()) Then
            bauteile.Add(tmpl.getName(), New LokalesBauteil(tmpl))
        Else
            Throw New Exception("Das Bauteil mit dem Namen " & tmpl.getName() & " existiert schon.")
        End If
    End Sub

    Public Function hatBauteil(name As String) As Boolean
        Return bauteile.ContainsKey(name)
    End Function

    Public Function getBauteil(name As String) As LokalesBauteil
        Return bauteile(name)
    End Function
End Class

Public Class LokalesBauteil

    Public tmpl As TemplateAusDatei
    Public istSchonGeladen As Boolean
    Public neuerNamespaceName As String
    Public neuerName As String

    Public Sub New(tmpl As TemplateAusDatei)
        Me.tmpl = tmpl
        Me.istSchonGeladen = False
        Me.neuerNamespaceName = ""
        Me.neuerName = ""
    End Sub

End Class
