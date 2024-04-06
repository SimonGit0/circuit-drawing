Public Class VirtualWindowRenderer
    Public Links As VirtualDockStack
    Public Rechts As VirtualDockStack
    Public Fenster As List(Of VirtualWindow)

    ''' <summary>
    ''' erstellt eine neuen Leeren Zustand.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        Me.Fenster = New List(Of VirtualWindow)

        Links = New VirtualDockStack
        Rechts = New VirtualDockStack
    End Sub

    ''' <summary>
    ''' Lädt einen neuen Zustand aus einer .xml Datei
    ''' Wenn NichtVorhandeneFensterEinzufügen=true werden alle nicht angegebenen Dateien in den Windowrenderer eingefügt 
    ''' (in der DockLeisteUnten, Visible=false)
    ''' Ist NichtVorhandeneFensterEinzufügen=false wird eine Exception geworfen, wenn ein Fenster fehlt. ==> Das ist z.b. für das erste Einladen aus
    ''' der Standarddatei gut. Wenn die gespeicherte Positionierung falsch ist kann so der Standard eingeladen werden!
    ''' </summary>
    ''' <param name="datei"></param>
    ''' <remarks></remarks>
    Public Sub New(datei As String, config As WindowRendererContentConfig, NichtVorhandeneFensterEinzufügen As Boolean)

        Me.Fenster = New List(Of VirtualWindow)

        Dim reader As Xml.XmlTextReader = Nothing
        Try
            reader = New Xml.XmlTextReader(datei)
            Dim doc As New Xml.XmlDocument()
            doc.Load(reader)

            Dim erfolg As Boolean = False
            If doc.HasChildNodes Then
                For i As Integer = 0 To doc.ChildNodes.Count - 1
                    If doc.ChildNodes(i).Name.ToLower() = "fenster-anordnung" Then
                        ladeAnordnung(doc.ChildNodes(i), AddressOf config.getContent)
                        erfolg = True
                        Exit For
                    End If
                Next
            End If
            If Not erfolg Then
                Throw New Exception("Kann die Datei nicht laden")
            End If

            vorlagePrüfenUndKorrigieren(AddressOf config.getContent, NichtVorhandeneFensterEinzufügen, config)

        Catch ex As Exception
            Throw ex
        Finally
            If reader IsNot Nothing Then reader.Close()
        End Try
    End Sub

    Public Sub New(node As Xml.XmlNode, config As WindowRendererContentConfig, NichtVorhandeneFensterEinzufügen As Boolean)

        Me.Fenster = New List(Of VirtualWindow)
        ladeAnordnung(node, AddressOf config.getContent)
        vorlagePrüfenUndKorrigieren(AddressOf config.getContent, NichtVorhandeneFensterEinzufügen, config)
    End Sub

    ''' <summary>
    ''' Generiert einen Zustand aus dem Stand des Windowrenderers
    ''' </summary>
    ''' <param name="WR"></param>
    ''' <remarks></remarks>
    Public Sub New(WR As WindowRenderer)

        Links = New VirtualDockStack(WR.LeisteLinks)
        Rechts = New VirtualDockStack(WR.LeisteRechts)

        Fenster = New List(Of VirtualWindow)(WR.Fenster.Count)
        For i As Integer = 0 To WR.Fenster.Count - 1
            Fenster.Add(New VirtualWindow(WR.Fenster(i)))
        Next
    End Sub

    Private Sub vorlagePrüfenUndKorrigieren(getContent As Func(Of String, Content), NichtVorhandeneFensterEinzufügen As Boolean, prüfungsConfig As WindowRendererContentConfig)
        Dim namen As New List(Of String)
        Links.prüfen(namen)
        Rechts.prüfen(namen)

        For i As Integer = 0 To Fenster.Count - 1
            Fenster(i).prüfen(namen)
        Next

        Dim anzahlen(prüfungsConfig.ANZAHL_WINDOWRENDERER_FENSTER - 1) As Integer
        For i As Integer = 0 To namen.Count - 1
            anzahlen(prüfungsConfig.getNumberOfName(namen(i))) += 1
        Next


        For i As Integer = 0 To anzahlen.Length - 1
            If anzahlen(i) = 0 Then
                'das Fenster i ist nicht vorhanden!
                'es wird erstmal pauschal unten rechts in der DockleisteH angezeigt (unsichtbar)
                If NichtVorhandeneFensterEinzufügen Then
                    Rechts.add(getContent(prüfungsConfig.getNameOfNumber(i)), False)
                Else
                    Throw New Exception("Das Fenster " & prüfungsConfig.getNameOfNumber(i) & " wurde nicht angegeben!")
                End If
            End If
            If anzahlen(i) > 1 Then
                'das Fenster kommt mehrfach vor! Schwerer Fehler!
                Dim name As String = prüfungsConfig.getNameOfNumber(i)
                Dim anzahl As Integer = 0
                Links.löschenWennAnzahlGrößer1(name, anzahl)
                Rechts.löschenWennAnzahlGrößer1(name, anzahl)

                For k As Integer = 0 To Fenster.Count - 1
                    Fenster(k).löschenWennAnzahlGrößer1(name, anzahl)
                Next

                If anzahl <> anzahlen(i) Then
                    'löschen fehlgeschlagen!
                    Throw New Exception("Fehlerhafte Positionierung kann nicht korrigiert werden.")
                End If
            End If
        Next

        '-------------------------------------------
        'Stufe 2 alle leeren Fenster, Tabelemente löschen

        For k As Integer = Fenster.Count - 1 To 0 Step -1
            If Fenster(k).istLeer() Then
                Fenster.RemoveAt(k)
            End If
        Next
        Rechts.LeereTabsLöschen()
        Links.LeereTabsLöschen()
    End Sub

#Region "In Windowrenderer einladen"
    ''' <summary>
    ''' Lädt diese Vorlage in den Windowrenderer ein. Achtung was noch nicht gemacht wird ist das setzen der
    ''' Checked Eigenschaft in dem Menustrip! Dies muss das aufrufende Programm im Anschluss tun. Optimalerweise ohne irgendwelche Events etc.
    ''' an den Windowrenderer zu senden, da der nach dieser Sub schon auf dem Optimalen Stand ist!
    ''' </summary>
    ''' <param name="w"></param>
    ''' <remarks></remarks>
    Public Sub LadeEin(w As WindowRenderer)
        Dim alteFenster As New List(Of Window)(w.Fenster.Count)
        alteFenster.AddRange(w.Fenster)

        w.Clear(True)
        Dim style As DesignStyle = w.DesignStyle

        For i As Integer = 0 To Me.Fenster.Count - 1
            w.addWindowOhneEvents(getTab(Me.Fenster(i).tab, style, Me.Fenster(i).StartSize.Width), Me.Fenster(i).StartPos, Me.Fenster(i).StartSize)
        Next

        w.LeisteLinks.breite = Links.Breite
        For i As Integer = 0 To Me.Links.tabs.Count - 1
            w.AddTabElementOhneEvents(getTab(Me.Links.tabs(i), style, Me.Links.Breite), Aufenthaltsort.Links)
        Next

        w.LeisteRechts.breite = Rechts.Breite
        For i As Integer = 0 To Me.Rechts.tabs.Count - 1
            w.AddTabElementOhneEvents(getTab(Me.Rechts.tabs(i), style, Me.Rechts.Breite), Aufenthaltsort.Rechts)
        Next


        w.anzeigen()

        For Each f As Window In alteFenster
            f.Close()
        Next
        w.FensterAktivieren()
    End Sub

    Private Function getTab(v As VirtualTabElement, style As DesignStyle, breite As Integer) As TabElement
        Dim t As New TabElement(style)
        t.setBreiteOhneEvents(breite)

        Dim visible As Boolean = False

        For i As Integer = 0 To v.Items.Count - 1
            v.Items(i).Content.setVisibleInit(v.Items(i).Visible)
            v.Items(i).Content.FavoriteWidth = breite

            t.add(v.Items(i).Content)
            If v.Items(i).Content.Visible Then
                visible = True
            End If
        Next
        t.anzeigeheight = v.anzeigeHeight
        If v.selectedIndex >= 0 AndAlso v.selectedIndex < t.ContentList.Count Then
            t.selectedIndex = v.selectedIndex
        Else
            t.selectedIndex = 0
        End If
        If t.ContentList(t.selectedIndex).Visible = False Then
            For i As Integer = 0 To t.ContentList.Count - 1
                If t.ContentList(i).Visible Then
                    t.selectedIndex = i
                    Exit For
                End If
            Next
        End If

        'Damit es noch zusammen passt!
        t.setVisibleOhneEvents(visible)

        Return t
    End Function
#End Region

#Region "Als .xml speichern und laden"
    Public Sub SpeicherVorlage(pfad As String)
        Dim xmlWriter As New Xml.XmlTextWriter(pfad, New System.Text.UnicodeEncoding())
        With xmlWriter
            .Formatting = Xml.Formatting.Indented
            .Indentation = 4

            .WriteStartDocument()
            .WriteComment("Fensteranordnung erstellt mit Vektorgrafik am " & Now.ToString())
            .WriteStartElement("Fenster-Anordnung")

            speicherVorlageIntern(xmlWriter)

            .WriteEndElement()

            .WriteEndDocument()
            .Close()
        End With

    End Sub

    Public Sub speicherVorlageIntern(xmlwriter As Xml.XmlTextWriter)
        With xmlwriter
            .WriteStartElement("Links")
            Links.speichern(xmlwriter)
            .WriteEndElement()
            .WriteStartElement("Rechts")
            Rechts.speichern(xmlwriter)
            .WriteEndElement()

            For i As Integer = 0 To Fenster.Count - 1
                .WriteStartElement("Fenster")
                Fenster(i).speichern(xmlwriter)
                .WriteEndElement()
            Next
        End With
    End Sub

    Private Sub ladeAnordnung(node As Xml.XmlNode, getContent As Func(Of String, Content))
        If node.HasChildNodes Then
            For i As Integer = 0 To node.ChildNodes.Count - 1
                Select Case node.ChildNodes(i).Name.ToLower
                    Case "links"
                        If Links Is Nothing Then
                            Links = New VirtualDockStack(node.ChildNodes(i), getContent)
                        End If
                    Case "rechts"
                        If Rechts Is Nothing Then
                            Rechts = New VirtualDockStack(node.ChildNodes(i), getContent)
                        End If
                    Case "fenster"
                        Fenster.Add(New VirtualWindow(node.ChildNodes(i), getContent))
                End Select
            Next
        End If
        If Links Is Nothing Then Links = New VirtualDockStack()
        If Rechts Is Nothing Then Rechts = New VirtualDockStack()
    End Sub
#End Region

End Class
