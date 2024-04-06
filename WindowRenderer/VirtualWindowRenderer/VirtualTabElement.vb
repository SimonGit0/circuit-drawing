Public Class VirtualTabElement
    ''' <summary>
    ''' Items im TabElement
    ''' </summary>
    ''' <remarks></remarks>
    Public Items As List(Of VirtualContent)
    ''' <summary>
    ''' Anzeige Höhe dieses Tabelements. Ist nur entscheidend, wenn das Tabelement in einem DockStack ist. 
    ''' Normalerweise ist diese Höhe -1. Dies gibt an, dass die Standardpositionierung des DockStacks verwendet wird.
    ''' Nur wenn die Höhe speziell angepasst wird kann man eine zusätzliche anzeigeHeight vergeben.
    ''' </summary>
    ''' <remarks></remarks>
    Public anzeigeHeight As Integer
    Public selectedIndex As Integer

    Public Sub New(anzeigeHeight As Integer)
        Items = New List(Of VirtualContent)
        Me.anzeigeHeight = anzeigeHeight
    End Sub

    ''' <summary>
    ''' Standardfall: anzeigeHeight = -1
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        Me.New(-1)
    End Sub

    Public Sub New(node As Xml.XmlNode, getContent As Func(Of String, Content))
        Dim aktiv As String = ""
        anzeigeHeight = -1
        For i As Integer = 0 To node.Attributes.Count - 1
            Select Case node.Attributes(i).Name.ToLower()
                Case "höhe" : anzeigeHeight = CInt(node.Attributes(i).Value)
                Case "aktiv" : aktiv = node.Attributes(i).Value
            End Select
        Next
        Items = New List(Of VirtualContent)
        If node.HasChildNodes Then
            For i As Integer = 0 To node.ChildNodes.Count - 1
                If node.ChildNodes(i).Name.ToLower() = "content" Then
                    Items.Add(New VirtualContent(node.ChildNodes(i), getContent))
                End If
            Next
        End If

        selectedIndex = 0
        For i As Integer = 0 To Items.Count - 1
            If Items(i).Content.Name.ToLower() = aktiv.ToLower() Then
                selectedIndex = i
                Exit For
            End If
        Next
    End Sub

    Public Sub New(tab As TabElement)
        Me.anzeigeHeight = tab.anzeigeheight
        Me.selectedIndex = tab.selectedIndex

        Items = New List(Of VirtualContent)
        If tab.ContentList IsNot Nothing Then
            For i As Integer = 0 To tab.ContentList.Count - 1
                Items.Add(New VirtualContent(tab.ContentList(i), tab.ContentList(i).VisibleInternal))
            Next
        End If
    End Sub

    Public Sub add(content As Content, visible As Boolean)
        Me.Items.Add(New VirtualContent(content, visible))
    End Sub

    Public Sub speichern(writer As Xml.XmlTextWriter, mitHöhe As Boolean)
        If mitHöhe Then writer.WriteAttributeString("Höhe", anzeigeHeight.ToString())
        If selectedIndex >= 0 AndAlso selectedIndex < Items.Count Then
            writer.WriteAttributeString("Aktiv", Items(selectedIndex).Content.Name)
        End If
        For i As Integer = 0 To Items.Count - 1
            writer.WriteStartElement("Content")
            Items(i).writeAttributes(writer)
            writer.WriteEndElement()
        Next
    End Sub

    Public Sub prüfen(ByRef namen As List(Of String))
        If anzeigeHeight <> -1 Then
            If anzeigeHeight < 0 Then anzeigeHeight = -1
        End If

        For i As Integer = 0 To Items.Count - 1
            Items(i).prüfen(namen)
        Next
    End Sub

    Public Sub löschenWennAnzahlGrößer1(name As String, ByRef anzahl As Integer)
        'Rückwärts durchgehen, damit man schneller löschen kann!
        For i As Integer = Items.Count - 1 To 0 Step -1
            If Items(i).Content.Name = name Then
                anzahl += 1
                If anzahl > 1 Then
                    Items.RemoveAt(i)
                End If
            End If
        Next
    End Sub

    Public Function istLeer() As Boolean
        Return Items.Count = 0
    End Function
End Class
