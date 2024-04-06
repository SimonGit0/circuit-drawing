''' <summary>
''' T sollte ein Enum sein. Kann man leider in VB nicht spezifizieren, aber man kann sich ja dran
''' halten...
''' </summary>
''' <typeparam name="T"></typeparam>
''' <remarks></remarks>
Public Class MultiPerspektivenManager(Of T)
    Private perspektiven() As T
    Private Ansichten() As VirtualWindowRenderer
    Private aktivePerspektiveIndex As Integer

    Public Sub New(perspektiven() As T)
        Me.perspektiven = perspektiven
        ReDim Ansichten(Me.perspektiven.Length - 1)
    End Sub

    Public Sub initPerspektive(perspektive As T, vorlage As VirtualWindowRenderer)
        Ansichten(getIndex(perspektive)) = vorlage
    End Sub

    Public Sub initAusDatei(pfad As String, config As WindowRendererContentConfig, NichtVorhandeneFensterEinzufügen As Boolean)
        Dim reader As Xml.XmlTextReader = Nothing
        Try
            reader = New Xml.XmlTextReader(pfad)
            Dim doc As New Xml.XmlDocument()
            doc.Load(reader)

            Dim erfolg As Boolean = False
            If doc.HasChildNodes Then
                For i As Integer = 0 To doc.ChildNodes.Count - 1
                    If doc.ChildNodes(i).Name.ToLower() = "perspektiven" Then
                        ladePerspektiven(doc.ChildNodes(i), config, NichtVorhandeneFensterEinzufügen)
                        erfolg = True
                        Exit For
                    End If
                Next
            End If
            If Not erfolg Then
                Throw New Exception("Laden der Perspektiven fehlgeschlagen")
            End If

        Catch ex As Exception
            Throw ex
        Finally
            If reader IsNot Nothing Then reader.Close()
        End Try
    End Sub

    Private Sub ladePerspektiven(node As Xml.XmlNode, config As WindowRendererContentConfig, NichtVorhandeneFensterEinzufügen As Boolean)
        Dim hat(perspektiven.Count - 1) As Boolean
        If node.HasChildNodes Then
            For i As Integer = 0 To node.ChildNodes.Count - 1
                If node.ChildNodes(i).Name.ToLower() = "fenster-anordnung" Then
                    ladeAnordnung(hat, node.ChildNodes(i), config, NichtVorhandeneFensterEinzufügen)
                End If
            Next
        End If

        For i As Integer = 0 To hat.Count - 1
            If Not hat(i) Then
                Throw New Exception("Nicht alle Ansichten initialisiert!")
            End If
        Next
    End Sub

    Private Sub ladeAnordnung(ByRef erfolg() As Boolean, node As Xml.XmlNode, config As WindowRendererContentConfig, NichtVorhandeneFensterEinzufügen As Boolean)
        Dim _ansichten As String() = Nothing
        Dim loadDefault As Boolean = False
        For i As Integer = 0 To node.Attributes.Count - 1
            If node.Attributes(i).Name.ToLower = "ansicht" Then
                _ansichten = node.Attributes(i).Value.Split(","c)
            ElseIf node.Attributes(i).Name.ToLower = "default" Then
                If node.Attributes(i).Value.ToLower = "true" Then
                    loadDefault = True
                End If
            End If
        Next
        If _ansichten IsNot Nothing Then
            Dim nr As Integer = -1
            Dim ansicht As String
            For k As Integer = 0 To _ansichten.Length - 1
                nr = -1
                ansicht = _ansichten(k).Trim.ToLower()
                For i As Integer = 0 To perspektiven.Count - 1
                    If perspektiven(i).ToString.ToLower() = ansicht Then
                        nr = i
                        Exit For
                    End If
                Next
                If nr = -1 Then Continue For

                If erfolg(nr) Then
                    Throw New Exception("Diese Ansicht kommt doppelt vor!")
                End If

                If loadDefault Then
                    'Ansichten(nr) = VirtualWindowRenderer.GeneriereStandardvorlage(perspektiven(nr).ToString(), setgetContent)
                Else
                    Ansichten(nr) = New VirtualWindowRenderer(node, config, NichtVorhandeneFensterEinzufügen)
                End If

                erfolg(nr) = True
            Next
        End If
    End Sub

    Public Sub setzeStartPerspektive(perspektive As T)
        aktivePerspektiveIndex = getIndex(perspektive)
    End Sub

    Public Function getAktivePerspektive() As VirtualWindowRenderer
        Return Ansichten(aktivePerspektiveIndex)
    End Function

    Public Sub WechselPerspektive(perspektive As T, w As WindowRenderer)
        'alte Ansicht speichern
        Ansichten(aktivePerspektiveIndex) = New VirtualWindowRenderer(w)

        'neue Ansicht anzeigen
        Dim index As Integer = getIndex(perspektive)
        Dim ansicht As VirtualWindowRenderer = Ansichten(index)
        If ansicht Is Nothing Then Throw New ArgumentException("Die Perspektive wurde nicht initialisiert.")
        ansicht.LadeEin(w)

        'aktuelle Ansicht ändern
        aktivePerspektiveIndex = index
    End Sub

    Private Function getIndex(perspektive As T) As Integer
        Dim index As Integer = Array.IndexOf(perspektiven, perspektive)
        If index = -1 Then Throw New ArgumentException("Die Perspektive ist nicht vorhanden.")
        Return index
    End Function

    Public Sub speichern(w As WindowRenderer, pfad As String)
        'alte Ansicht speichern
        Ansichten(aktivePerspektiveIndex) = New VirtualWindowRenderer(w)

        Dim xmlWriter As New Xml.XmlTextWriter(pfad, New System.Text.UnicodeEncoding())
        With xmlWriter
            .Formatting = Xml.Formatting.Indented
            .Indentation = 4

            .WriteStartDocument()
            .WriteComment("Fensteranordnung erstellt mit Vektorgrafik 1.0 am " & Now.ToString())
            .WriteStartElement("Perspektiven")
            For i As Integer = 0 To perspektiven.Count - 1
                .WriteStartElement("Fenster-Anordnung")
                .WriteAttributeString("Ansicht", perspektiven(i).ToString())

                Ansichten(i).speicherVorlageIntern(xmlWriter)

                .WriteEndElement()
            Next
            .WriteEndElement()
            .WriteEndDocument()
            .Close()
        End With
    End Sub
End Class
