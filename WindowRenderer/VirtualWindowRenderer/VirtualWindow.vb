Public Class VirtualWindow
    ''' <summary>
    ''' Das TabElement in diesem Fenster
    ''' </summary>
    ''' <remarks></remarks>
    Public tab As VirtualTabElement
    ''' <summary>
    ''' Position an der das Fenster angezeigt wid
    ''' </summary>
    ''' <remarks></remarks>
    Public StartPos As Point
    ''' <summary>
    ''' Größe mit der das Fenster angezeigt wird
    ''' </summary>
    ''' <remarks></remarks>
    Public StartSize As Size

    Public Sub New(pos As Point, size As Size, t As VirtualTabElement)
        Me.StartPos = pos
        Me.StartSize = size
        Me.tab = t
    End Sub

    Public Sub New(node As Xml.XmlNode, getContent As Func(Of String, Content))
        For i As Integer = 0 To node.Attributes.Count - 1
            Select Case node.Attributes(i).Name.ToLower()
                Case "x" : StartPos.X = CInt(node.Attributes(i).Value)
                Case "y" : StartPos.Y = CInt(node.Attributes(i).Value)
                Case "breite" : StartSize.Width = CInt(node.Attributes(i).Value)
                Case "höhe" : StartSize.Height = CInt(node.Attributes(i).Value)
            End Select
        Next

        Me.tab = New VirtualTabElement(node, getContent)
    End Sub

    Public Sub New(w As Window)
        Me.tab = New VirtualTabElement(w.Tabs)
        Me.StartPos = w.Location
        Me.StartSize = w.ClientSize
    End Sub

    Public Sub speichern(writer As Xml.XmlTextWriter)
        writer.WriteAttributeString("X", StartPos.X.ToString())
        writer.WriteAttributeString("Y", StartPos.Y.ToString())
        writer.WriteAttributeString("Breite", StartSize.Width.ToString())
        writer.WriteAttributeString("Höhe", StartSize.Height.ToString())

        tab.speichern(writer, False)
    End Sub

    Public Sub prüfen(ByRef namen As List(Of String))
        tab.prüfen(namen)
        If StartSize.Width < 0 Then StartSize.Width = 0
        If StartSize.Height < 0 Then StartSize.Height = 0
    End Sub

    Public Sub löschenWennAnzahlGrößer1(name As String, ByRef anzahl As Integer)
        tab.löschenWennAnzahlGrößer1(name, anzahl)
    End Sub

    Public Function istLeer() As Boolean
        Return tab.istLeer()
    End Function
End Class
