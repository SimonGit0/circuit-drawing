Public Class VirtualDockStack
    ''' <summary>
    ''' Alle Tabs die in diesem DockStack sind.
    ''' </summary>
    ''' <remarks></remarks>
    Public tabs As List(Of VirtualTabElement)
    ''' <summary>
    ''' Breite des DockStacks
    ''' </summary>
    ''' <remarks></remarks>
    Public Breite As Integer

    Public Sub New()
        Me.tabs = New List(Of VirtualTabElement)
    End Sub

    Public Sub New(node As Xml.XmlNode, getContent As Func(Of String, Content))
        For i As Integer = 0 To node.Attributes.Count - 1
            If node.Attributes(i).Name.ToLower = "breite" Then
                Me.Breite = CInt(node.Attributes(i).Value)
            End If
        Next
        tabs = New List(Of VirtualTabElement)
        If node.HasChildNodes Then
            For i As Integer = 0 To node.ChildNodes.Count - 1
                If node.ChildNodes(i).Name.ToLower = "tab" Then
                    tabs.Add(New VirtualTabElement(node.ChildNodes(i), getContent))
                End If
            Next
        End If
    End Sub

    Public Sub New(dockstack As DockStack)
        Me.Breite = dockstack.breite
        Me.tabs = New List(Of VirtualTabElement)
        If dockstack.Invisiblecollection IsNot Nothing Then
            For i As Integer = 0 To dockstack.Invisiblecollection.Count - 1
                Me.tabs.Add(New VirtualTabElement(dockstack.Invisiblecollection(i)))
            Next
        End If
    End Sub

    ''' <summary>
    ''' Schnelle Funktion, wenn man ein Tabelement mit nur einem Content hinzufügen will.
    ''' </summary>
    ''' <param name="content"></param>
    ''' <param name="visible"></param>
    ''' <remarks></remarks>
    Public Sub add(content As Content, visible As Boolean, anzeigeHeight As Integer)
        Dim tabNeu As New VirtualTabElement(anzeigeHeight)
        tabNeu.selectedIndex = 0
        tabNeu.add(content, visible)
        tabs.Add(tabNeu)
    End Sub

    ''' <summary>
    ''' Schnelle Funktion, wenn man ein Tabelement mit nur einem Content hinzufügen will.
    ''' anzeigeHeight = -1
    ''' </summary>
    ''' <param name="content"></param>
    ''' <param name="visible"></param>
    ''' <remarks></remarks>
    Public Sub add(content As Content, visible As Boolean)
        add(content, visible, -1)
    End Sub

    Public Sub speichern(writer As Xml.XmlTextWriter)
        writer.WriteAttributeString("Breite", Breite.ToString)
        For i As Integer = 0 To tabs.Count - 1
            writer.WriteStartElement("Tab")
            tabs(i).speichern(writer, True)
            writer.WriteEndElement()
        Next
    End Sub

    Public Sub prüfen(ByRef namen As List(Of String))
        If Breite < WindowRenderer.minGröße Then
            Breite = WindowRenderer.minGröße
        End If
        For i As Integer = 0 To tabs.Count - 1
            tabs(i).prüfen(namen)
        Next
    End Sub

    Public Sub löschenWennAnzahlGrößer1(name As String, ByRef anzahl As Integer)
        For i As Integer = 0 To tabs.Count - 1
            tabs(i).löschenWennAnzahlGrößer1(name, anzahl)
        Next
    End Sub

    Public Sub LeereTabsLöschen()
        For i As Integer = tabs.Count - 1 To 0 Step -1
            If tabs(i).istLeer Then
                tabs.RemoveAt(i)
            End If
        Next
    End Sub
End Class
