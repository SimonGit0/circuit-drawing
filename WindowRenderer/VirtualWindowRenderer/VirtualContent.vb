Public Class VirtualContent
    Private myContent As Content
    ''' <summary>
    ''' Der dazugehörige Content
    ''' </summary>
    ''' <remarks></remarks>
    Public ReadOnly Property Content As Content
        Get
            Return myContent
        End Get
    End Property

    ''' <summary>
    ''' Gibt an, ob dieser Content beim Einladen dieser Vorlage sichtbar gemacht werden soll.
    ''' Dies ist natürlich abhängig vom Available des Contents. Wenn man also z.b. in der Startseite ist
    ''' gibt dieses Visible bei der BildHistorie nur an, ob die BildHistorie angezeigt wird, wenn man wieder in
    ''' den Bildmodus wechselt!
    ''' </summary>
    ''' <remarks></remarks>
    Public Property Visible As Boolean

    Public Sub New(c As Content, visible As Boolean)
        Me.myContent = c
        Me.Visible = visible
    End Sub

    Public Sub New(node As Xml.XmlNode, getContent As Func(Of String, Content))
        For i As Integer = 0 To node.Attributes.Count - 1
            Select Case node.Attributes(i).Name.ToLower()
                Case "name"
                    myContent = getContent(node.Attributes(i).Value)
                Case "sichtbar"
                    If node.Attributes(i).Value.ToLower = "false" Then
                        Visible = False
                    Else
                        Visible = True
                    End If
            End Select
        Next
        If myContent Is Nothing Then Throw New Exception("Kein Name für den Content zugeordnet!")
    End Sub

    Public Sub writeAttributes(writer As Xml.XmlTextWriter)
        writer.WriteAttributeString("Name", myContent.Name)
        writer.WriteAttributeString("Sichtbar", Visible.ToString)
    End Sub

    Public Sub prüfen(ByRef namen As List(Of String))
        namen.Add(myContent.Name)
    End Sub
End Class
