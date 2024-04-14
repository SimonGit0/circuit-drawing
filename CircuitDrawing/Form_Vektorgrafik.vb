Imports System.IO
Imports WindowRenderer
Public Class Form_Vektorgrafik
    Private Const MY_TEXT As String = "Circuit Drawing"
    Private Const ALLOW_IMPORT_IMAGE As Boolean = False

    Public Const VEKTORGRAFIK_DATEIKONSTANTE As ULong = 4552427781276267190
    Public Const Speicher_Version As Integer = 33
    Private WR_NAME_Bauelemente As String = My.Resources.Strings.WR_NAME_Bauelemente
    Private WR_NAME_Einstellungen As String = My.Resources.Strings.WR_NAME_Einstellungen
    Private WR_NAME_MarkierungsEinstellungen As String = My.Resources.Strings.WR_NAME_MarkierungsEinstellungen

    Private myBibliothek As Bibliothek
    Private myRückgängigVerwaltung As RückgängigVerwaltung
    Private myWindowRendererConfig As WindowRendererContentConfig

    Private FRM_BauelementeAuswahl As WR_BauelementeAuswahl
    Private FRM_Einstellungen As WR_Einstellungsform
    Private FRM_Markierungseinstellungen As WR_MarkierungsArt

    Private _aktuellerSpeicherpfad As String

    Private _aktuellerExportPfad_TEX As String

#Region "Init"
    Public Sub New()
        InitializeComponent()

        myBibliothek = New Bibliothek(Settings.getSettings.getFull_Pfade_Bib)
        myRückgängigVerwaltung = New RückgängigVerwaltung(Vektor_Picturebox1)
        updateRückEnable()
        AktuellerSpeicherpfad = ""
        _aktuellerExportPfad_TEX = ""

        initWindowRenderer()

        Me.KeyPreview = True
        init_Menu_ShortcutKeys()

        Me.Icon = My.Resources.iconAlle

        ImportierenToolStripMenuItem.Visible = ALLOW_IMPORT_IMAGE
    End Sub

    Private Sub Form_Vektorgrafik_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Vektor_Picturebox1.GridX = Settings.getSettings().Raster
        Vektor_Picturebox1.GridY = Settings.getSettings().Raster
        Vektor_Picturebox1.crossCursor = Settings.getSettings().CrossCursor
        Vektor_Picturebox1.snappoinsImmerAnzeigen = Settings.getSettings().SnappointsImmerAnzeigen
        Vektor_Picturebox1.showBoarder = Settings.getSettings().RandAnzeigen
        Vektor_Picturebox1.TextVorschauMode = Settings.getSettings().TextVorschauMode

        Vektor_Picturebox1.Select()

        Dim tool1 As New ToolRechteckMarkierung()
        tool1.KannAbbrechen = False
        Vektor_Picturebox1.startTool(tool1)

        If Vektor_Picturebox1.myMoveRichtung = Vektor_Picturebox.MoveRichtung.AlleRichtungen Then
            ToolStripButton1.Image = My.Resources.AlleRichtungen2
        Else
            ToolStripButton1.Image = My.Resources.Rechtwinklig2
        End If
        BtnSnappointsImmerAnzeigen.Image = My.Resources.Snappoints
        BtnRandAnzeigen.Image = My.Resources.RandAnzeigen
        BtnCrossCursor.Image = My.Resources.crossCursor
        updateUI()

        Btn_DrawDots.Image = My.Resources.Punkt
        Btn_DrawZeilensprünge.Image = My.Resources.Zeilensprünge
        BtnGravity.Image = My.Resources.gravity
        BtnSelectMode.Image = My.Resources.MultiSelect

        Vektor_Picturebox1.gravityStärke = Settings.getSettings().Gravity_Stärke
        Vektor_Picturebox1.enable_gravity = Settings.getSettings().Gravity
        BtnGravity.Checked = Vektor_Picturebox1.enable_gravity

        Vektor_Picturebox1.MultiWireSelect = Settings.getSettings().MultiSelect
        BtnSelectMode.Checked = Vektor_Picturebox1.MultiWireSelect

        Dim str() As String = Environment.GetCommandLineArgs()
        If str IsNot Nothing AndAlso str.Length > 0 Then
            Dim pathLoad As String = ""
            For i As Integer = 1 To str.Length - 1 'Das erste Argument ist immer der Pfad oder Name des Programms (Vektorgrafik.exe), erst danach kommen die Argumente
                Dim fi As New FileInfo(str(i))
                If fi.Extension = ".sch" AndAlso fi.Exists Then
                    pathLoad = str(i)
                    Exit For
                End If
            Next
            If pathLoad <> "" Then
                öffneDatei(pathLoad, False)
            End If
        End If
    End Sub

    Private Sub init_Menu_ShortcutKeys()
        With Key_Settings.getSettings()
            BtnRück.Text &= " (" & .keyUndo.getMenuString() & ")"
            BtnVorgängig.Text &= " (" & .keyRedo.getMenuString() & ")"
            ToolStripButton1.Text &= " (" & .keyChangeMoveMode.getMenuString() & ")"
            BtnGravity.Text &= " (" & .keyChangeGravity.getMenuString() & ")"
            BtnSelectMode.Text &= " (" & .keyMultiSelect.getMenuString() & ")"

            ElementLöschenToolStripMenuItem.ShortcutKeyDisplayString = .keyToolDelete.getMenuString()
            MoveToolStripMenuItem.ShortcutKeyDisplayString = .keyToolMove.getMenuString()
            SkalierenToolStripMenuItem.ShortcutKeyDisplayString = .keyToolScale.getMenuString()
            WireToolStripMenuItem.ShortcutKeyDisplayString = .keyToolWire.getMenuString()
            PlaceInstanceToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddInstance.getMenuString()
            StromToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddStrom.getMenuString()
            SpannungToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddSpannung.getMenuString()
            LabelToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddLabel.getMenuString()
            BusBeschriftungToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddBusTap.getMenuString()
            ImpedanzToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddImpedanceArrow.getMenuString()

            ElementEinstellungenToolStripMenuItem.ShortcutKeyDisplayString = .keyShowElementEinstellungen.getMenuString()

            MarkierungAufhebenToolStripMenuItem.ShortcutKeyDisplayString = .keyMarkierungAufheben.getMenuString()
            AllesMarkierenToolStripMenuItem.ShortcutKeyDisplayString = .keyAllesMarkieren.getMenuString()

            SpeichernToolStripMenuItem1.ShortcutKeyDisplayString = .keyDateiSpeichern.getMenuString()

            VerbinderOptimierenToolStripMenuItem.ShortcutKeyDisplayString = .keyRoutingOptimieren.getMenuString()

            HorizontalSpiegelnToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddInstanceMirrorX.getMenuString()
            VertikalSpiegelnToolStripMenuItem.ShortcutKeyDisplayString = .keyToolAddInstanceMirrorY.getMenuString()

            InZwischenablageKopierenToolStripMenuItem.ShortcutKeyDisplayString = .keyCopy.getMenuString()
            AusZwischenablageEinfügenToolStripMenuItem.ShortcutKeyDisplayString = .keyPaste.getMenuString()

            AusZwischenablageToolStripMenuItem1.ShortcutKeyDisplayString = .keyPaste.getMenuString()
            AusZwischenablageToolStripMenuItem.ShortcutKeyDisplayString = .keyPaste.getMenuString()

            AlsPDFTEXToolStripMenuItem.ShortcutKeyDisplayString = .keyExportiereAlsTEX.getMenuString()
            AlsEMFToolStripMenuItem.ShortcutKeyDisplayString = .keyExportiereAlsEMF.getMenuString()

            EineEbeneNachHintenToolStripMenuItem.ShortcutKeyDisplayString = .keyEbeneNachHinten.getMenuString()
            EineEbeneNachVorneToolStripMenuItem.ShortcutKeyDisplayString = .keyEbeneNachVorne.getMenuString()
        End With
    End Sub
#End Region

#Region "WindowRenderer"
#Region "Init"
    Public Sub initWindowRenderer()
        init_FRM_Controls()

        'Den WindowrendererConfig erstellen (Organisiert alles was mit dem Windowrenderer zu tun hat!)
        myWindowRendererConfig = New WindowRendererContentConfig()
        'Jetzt einfach ALLE Contents bekannt machen. Damit der Windowrenderer weiß, welche Fenster es gibt, wie diese heißen, welche Icons die haben, ...
        Dim c As New Content(FRM_BauelementeAuswahl, False, 200)
        c.Name = WR_NAME_Bauelemente
        c.Icon = My.Resources.Elemente
        myWindowRendererConfig.addContent(c)

        c = New Content(FRM_Einstellungen, False, 250)
        c.Name = WR_NAME_Einstellungen
        c.Icon = Nothing
        myWindowRendererConfig.addContent(c)
        AddHandler c.VisibleChanged, AddressOf FRM_Einstellungen_VisibleChanged

        c = New Content(FRM_Markierungseinstellungen, True, 250)
        c.Name = WR_NAME_MarkierungsEinstellungen
        c.Icon = Nothing
        myWindowRendererConfig.addContent(c)

        'Jetzt noch eine Default Position erstellen. Wird immer gebraucht, wenn nicht die alte Position aus einer Datei geladen werden kann.
        Dim defaultPositioning As VirtualWindowRenderer = generateDefault(myWindowRendererConfig)
        'Jetzt den Windowrenderer initialisieren mit allen wichtigen Argumenten. Die Standardposition wird nur verwendet, wenn er nicht aus einer Datei direkt was laden kann.

        Me.WR_ParentPanel.BackColor = SystemColors.ControlDark
        myWindowRendererConfig.initWindowRenderer(Me.WR_ParentPanel, Me.WR_MiddlePanel, Me, "Config/WindowPosition.xml", FensterToolStripMenuItem, False, defaultPositioning)

    End Sub

    Private Sub init_FRM_Controls()
        FRM_BauelementeAuswahl = New WR_BauelementeAuswahl()
        AddHandler FRM_BauelementeAuswahl.OnTemplateChanged, AddressOf OnTemplateChanged
        AddHandler FRM_BauelementeAuswahl.DeleteLokalSymbol, AddressOf DeleteLokalSymbol
        FRM_BauelementeAuswahl.init(myBibliothek)

        FRM_Einstellungen = New WR_Einstellungsform()
        FRM_Einstellungen.setEinstellungen(Nothing)
        AddHandler FRM_Einstellungen.EinstellungenÜbernehmen, AddressOf FRM_EinstellungenÜbernehemen

        FRM_Markierungseinstellungen = New WR_MarkierungsArt()
        FRM_Markierungseinstellungen.Select_Wires = Vektor_Picturebox1.Select_Wires
        FRM_Markierungseinstellungen.Select_Bauelemente = Vektor_Picturebox1.Select_Bauteile
        FRM_Markierungseinstellungen.Select_Beschriftung = Vektor_Picturebox1.Select_Beschriftung
        FRM_Markierungseinstellungen.Select_Drawings = Vektor_Picturebox1.Select_Drawings
        AddHandler FRM_Markierungseinstellungen.CheckedChanged, AddressOf MarkierungsEinstellungenChanged
    End Sub

    Public Function generateDefault(config As WindowRendererContentConfig) As VirtualWindowRenderer
        Dim wr As New VirtualWindowRenderer()
        With wr

            'Beispiel Tabelement Rechts
            'Dim tabElementEbenePfade As New VirtualTabElement()
            'tabElementEbenePfade.add(getContent(WR_NAME_Ebenen), True)
            'tabElementEbenePfade.add(getContent(WR_NAME_Pfade), True)
            '.Rechts.tabs.Add(tabElementEbenePfade)

            .Rechts.add(config.getContent(WR_NAME_Bauelemente), True)

            .Links.add(config.getContent(WR_NAME_Einstellungen), True)
            .Links.add(config.getContent(WR_NAME_MarkierungsEinstellungen), True)

            'Breite der DockStacks:
            .Rechts.Breite = 200
            .Links.Breite = 250
        End With
        Return wr
    End Function
#End Region

#Region "Selectionform"
    Private Sub MarkierungsEinstellungenChanged(sender As Object, e As EventArgs)
        Vektor_Picturebox1.Select_Bauteile = FRM_Markierungseinstellungen.Select_Bauelemente
        Vektor_Picturebox1.Select_Wires = FRM_Markierungseinstellungen.Select_Wires
        Vektor_Picturebox1.Select_Beschriftung = FRM_Markierungseinstellungen.Select_Beschriftung
        Vektor_Picturebox1.Select_Drawings = FRM_Markierungseinstellungen.Select_Drawings
    End Sub
#End Region

#Region "Einstellungsform"
    Private Sub Vektor_Picturebox1_SelectionChanged(sender As Vektor_Picturebox, e As EventArgs) Handles Vektor_Picturebox1.SelectionChanged
        FRM_lade_Einstellungen()
    End Sub

    Private Sub FRM_Einstellungen_VisibleChanged(sender As Object, e As VisibleChangedEventArgs)
        FRM_lade_Einstellungen()
    End Sub

    Private Sub FRM_lade_Einstellungen()
        Dim c As Content = myWindowRendererConfig.getContent(WR_NAME_Einstellungen)
        If c.Visible Then
            If Vektor_Picturebox1.has_selection() Then
                Dim einstellungen As List(Of ElementEinstellung) = Vektor_Picturebox1.getEinstellungenOfSelectedElements()
                If einstellungen.Count > 0 Then
                    FRM_Einstellungen.setEinstellungen(einstellungen)
                Else
                    FRM_Einstellungen.setEinstellungen(Nothing)
                End If
            Else
                FRM_Einstellungen.setEinstellungen(Nothing)
            End If
        End If
    End Sub

    Private Sub FRM_EinstellungenÜbernehemen(sender As Object, liste As List(Of ElementEinstellung))
        If liste IsNot Nothing Then
            Vektor_Picturebox1.setEinstellungenSelected(liste)
        End If
    End Sub
#End Region

#Region "Bauelementeform"
    Private Sub DeleteLokalSymbol(sender As Object, cell As BauteilCell)
        Dim tmpl As TemplateAusDatei = cell.getFirst().template
        Dim name As String = tmpl.getNameSpace() & "." & tmpl.getName()

        Dim hatBauteileNoch As Boolean = False
        For Each e As ElementMaster In Vektor_Picturebox1.ElementListe
            If TypeOf e Is BauteilAusDatei Then
                With DirectCast(e, BauteilAusDatei)
                    Dim name1 As String = .getTemplate().getNameSpace() & "." & .getTemplate().getName()
                    If name1 = name Then
                        hatBauteileNoch = True
                        Exit For
                    End If

                    Dim deko As List(Of Deko_Bauteil) = .getDeko()
                    If deko IsNot Nothing Then
                        For i As Integer = 0 To deko.Count - 1
                            name1 = deko(i).getTemplate().getNameSpace() & "." & deko(i).getTemplate().getName()
                            If name1 = name Then
                                hatBauteileNoch = True
                                Exit For
                            End If
                        Next
                    End If
                    If hatBauteileNoch Then
                        Exit For
                    End If
                End With
            End If
        Next

        If hatBauteileNoch Then
            MessageBox.Show("Dieses Symbol wird in dem Dokument noch verwendet! Sie können es daher nicht löschen. Löschen Sie erst alle Instanzen dieses Symbols und löschen das Symbol dann erneut.", "Symbol kann nicht gelöscht werden")
            Exit Sub
        End If

        'Löschen
        myBibliothek.deleteLocalBauteil(tmpl)
        OnBibliothekChanged()
    End Sub

    Private Sub OnBibliothekChanged()
        Me.FRM_BauelementeAuswahl.refresh_Liste(myBibliothek)
    End Sub

    Private Sub OnTemplateChanged(sender As Object, cell As BauteilCell)
        Vektor_Picturebox1.currentPlaceBauteil = cell
    End Sub

    Private Sub selectBauteil(_namespace As String, _name As String)
        Me.FRM_BauelementeAuswahl.selectBauteil(_namespace, _name)
    End Sub
#End Region
#End Region

#Region "Datei"
#Region "Laden Speichern"
    Private Sub TsBTN_Öffnen_Click(sender As Object, e As EventArgs) Handles TsBTN_Öffnen.Click
        öffnen(False)
    End Sub

    Private Sub TsBTN_Save_Click(sender As Object, e As EventArgs) Handles TsBTN_Save.Click
        speichern()
    End Sub

    Private Sub SpeichernToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SpeichernToolStripMenuItem.Click
        speichernUnter()
    End Sub

    Private Sub SpeichernToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles SpeichernToolStripMenuItem1.Click
        speichern()
    End Sub

    Private Sub ÖffnenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ÖffnenToolStripMenuItem.Click
        öffnen(False)
    End Sub

    Private Sub DateiImKompatibilitätsmodusÖffnenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DateiImKompatibilitätsmodusÖffnenToolStripMenuItem.Click
        öffnen(True)
    End Sub

    Private Sub öffnen(kompatibilität As Boolean)
        Dim o As New OpenFileDialog()
        o.Filter = "Schematic (*.sch)|*.sch|Alle Dateien (*.*)|*.*"
        If o.ShowDialog() = DialogResult.OK Then
            öffneDatei(o.FileName, kompatibilität)
        End If
    End Sub

    Private Sub öffneDatei(pfad As String, kompatibilität As Boolean)
        Dim stream As FileStream = Nothing
        Try
            stream = New FileStream(pfad, FileMode.Open, FileAccess.Read)
            Dim reader As New BinaryReader(stream, System.Text.Encoding.UTF8)

            Dim konst As ULong = reader.ReadUInt64()
            If konst <> VEKTORGRAFIK_DATEIKONSTANTE Then
                Throw New Exception("Falsches Dateiformat (Fehler A1000)")
            End If
            Dim version As Integer = reader.ReadInt32()
            If version < 0 Then
                Throw New Exception("Falsches Dateiformat (Fehler A1001)")
            End If
            If version > Speicher_Version Then
                Throw New Exception("Diese Datei wurde in einer neueren Version dieses Programms erstellt und kann daher nicht geöffnet werden. Bitte aktualisieren Sie das Programm auf die neueste Version.")
            End If

            If version >= 21 Then
                _aktuellerExportPfad_TEX = reader.ReadString()
            Else
                _aktuellerExportPfad_TEX = ""
            End If

            Dim myNeueBib As Bibliothek = myBibliothek.FlatCopyOhneLocal()
            myNeueBib.ladeLokaleTemplates(reader, version)
            Dim myLokaleBib As LokaleBibliothek = Bibliothek.ladeGespeicherteSymboleAlsNeueBib(reader, version)
            Vektor_Picturebox1.ladeGrafik(reader, version, myNeueBib, myLokaleBib, kompatibilität)
            myBibliothek = myNeueBib 'erst jetzt die Bibliothek übernehmen! So ist (falls die Datei nicht geöffnet werden konnte) die alte Bib nicht überschrieben!

            OnBibliothekChanged()

            stream.Close()

            myRückgängigVerwaltung.löscheAlleRückgängig()
            updateRückEnable()

            AktuellerSpeicherpfad = pfad

            updateUI()

        Catch ex As Exception
            MessageBox.Show("Öffnen fehlgeschlagen: " + ex.Message)
        Finally
            If stream IsNot Nothing Then
                stream.Close()
            End If
        End Try
    End Sub

    Private Sub speichern()
        If File.Exists(AktuellerSpeicherpfad) Then
            speichernUnter(AktuellerSpeicherpfad)
        Else
            speichernUnter()
        End If
    End Sub

    Private Sub speichernUnter()
        Dim s As New SaveFileDialog()
        s.Filter = "Schematic (*.sch)|*.sch"
        If s.ShowDialog() = DialogResult.OK Then
            speichernUnter(s.FileName)
        End If
    End Sub

    Private Sub speichernUnter(pfad As String)
        Dim stream As FileStream = Nothing
        Try
            stream = New FileStream(pfad, FileMode.Create, FileAccess.Write)
            Dim writer As New BinaryWriter(stream, System.Text.Encoding.UTF8)

            writer.Write(VEKTORGRAFIK_DATEIKONSTANTE)
            writer.Write(Speicher_Version)

            writer.Write(_aktuellerExportPfad_TEX)

            myBibliothek.speicherLokaleTemplates(writer)
            myBibliothek.speicherVerwendeteTemplates(writer, Vektor_Picturebox1.ElementListe, False, True) 'nur nichtlokale Bauteile speichern (die anderen sind schon oben)
            Vektor_Picturebox1.speicherGrafik(writer)

            AktuellerSpeicherpfad = pfad

            stream.Close()
        Catch ex As Exception
            MessageBox.Show("Speichern fehlgeschlagen: " + ex.Message)
        Finally
            If stream IsNot Nothing Then
                stream.Close()
            End If
        End Try
    End Sub

    Private Property AktuellerSpeicherpfad As String
        Get
            Return _aktuellerSpeicherpfad
        End Get
        Set(value As String)
            _aktuellerSpeicherpfad = value
            If _aktuellerSpeicherpfad <> "" Then
                Me.Text = MY_TEXT & " - " + _aktuellerSpeicherpfad
            Else
                Me.Text = MY_TEXT
            End If
        End Set
    End Property
#End Region

#Region "Exportieren"
    Private Sub AlsPDFTEXToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlsPDFTEXToolStripMenuItem.Click
        Dim s As New SaveFileDialog()
        s.Filter = "*.tex|*.tex"
        preset_FileName_SaveFileDialog(s, True)
        If s.ShowDialog = DialogResult.OK Then
            Dim pfad As String = s.FileName
            _aktuellerExportPfad_TEX = pfad
            Vektor_Picturebox1.exportierenAlsTEX(pfad, False)
        End If
    End Sub

    Private Sub AlsEPSToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlsEPSToolStripMenuItem.Click
        Dim s As New SaveFileDialog()
        s.Filter = "*.tex|*.tex"
        preset_FileName_SaveFileDialog(s, True)
        If s.ShowDialog = DialogResult.OK Then
            Dim pfad As String = s.FileName
            _aktuellerExportPfad_TEX = pfad
            Vektor_Picturebox1.exportierenAlsTEX(pfad, True)
        End If
    End Sub

    Private Sub AlsPDFToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlsPDFToolStripMenuItem.Click
        Dim s As New SaveFileDialog()
        s.Filter = "*.pdf|*.pdf"
        preset_FileName_SaveFileDialog(s, False)
        If s.ShowDialog = DialogResult.OK Then
            Dim pfad As String = s.FileName
            Vektor_Picturebox1.exportierenAlsPDF(pfad)
        End If
    End Sub

    Private Sub AlsEMFToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlsEMFToolStripMenuItem.Click
        Dim s As New SaveFileDialog()
        s.Filter = "*.emf|*.emf"
        preset_FileName_SaveFileDialog(s, False)
        If s.ShowDialog = DialogResult.OK Then
            Dim pfad As String = s.FileName
            Vektor_Picturebox1.exportierenAlsEMF(pfad)
        End If
    End Sub

    Private Sub AlsBildPNGJPEGToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AlsBildPNGJPEGToolStripMenuItem.Click
        Dim bbox As Size = Vektor_Picturebox1.getBoundingBoxWithMarginExport().Size
        Dim size_selector As New FRM_ImgSizeSelection(bbox, 0.1)
        If size_selector.ShowDialog = DialogResult.OK Then
            Dim exportSize As Size = New Size(size_selector.W, size_selector.H)

            Dim s As New SaveFileDialog()
            s.Filter = "*.png|*.png|*.tiff|*.tiff|*.jpg|*.jpg|*.gif|*.gif"
            preset_FileName_SaveFileDialog(s, False)
            If s.ShowDialog = DialogResult.OK Then
                Dim pfad As String = s.FileName
                Dim ext As String = New FileInfo(s.FileName).Extension
                Dim format As Imaging.ImageFormat = Imaging.ImageFormat.Png
                Dim transparent As Boolean = size_selector.transparent
                Select Case ext
                    Case ".png"
                        format = Imaging.ImageFormat.Png
                    Case ".tiff"
                        format = Imaging.ImageFormat.Tiff
                    Case ".jpg"
                        format = Imaging.ImageFormat.Jpeg
                        transparent = False
                    Case ".gif"
                        format = Imaging.ImageFormat.Gif
                        transparent = False
                End Select
                Vektor_Picturebox1.exportierenAlsIMG(pfad, format, transparent, exportSize)
            End If
        End If
    End Sub

    Private Sub preset_FileName_SaveFileDialog(s As SaveFileDialog, use_aktuellerExportPfadTex As Boolean)
        If use_aktuellerExportPfadTex AndAlso _aktuellerExportPfad_TEX <> "" AndAlso (New FileInfo(_aktuellerExportPfad_TEX).Exists) Then
            Dim name As String = New FileInfo(_aktuellerExportPfad_TEX).Name
            Dim ext As String = New FileInfo(_aktuellerExportPfad_TEX).Extension
            If ext.Length < name.Length AndAlso ext.Length > 0 Then
                name = name.Substring(0, name.Length - ext.Length)
            End If
            s.InitialDirectory = New FileInfo(_aktuellerExportPfad_TEX).DirectoryName
            s.FileName = name
        ElseIf AktuellerSpeicherpfad <> "" Then
            Dim name As String = New FileInfo(AktuellerSpeicherpfad).Name
            Dim ext As String = New FileInfo(AktuellerSpeicherpfad).Extension
            If ext.Length < name.Length AndAlso ext.Length > 0 Then
                name = name.Substring(0, name.Length - ext.Length)
            End If
            s.InitialDirectory = New FileInfo(AktuellerSpeicherpfad).DirectoryName
            s.FileName = name
        End If
    End Sub
#End Region

#Region "Importieren"
    Private Sub AusDateiToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AusDateiToolStripMenuItem1.Click
        Dim o As New OpenFileDialog()
        o.Filter = "*.txt|*.txt"
        If o.ShowDialog = DialogResult.OK Then
            Dim lines As New List(Of String)
            Dim reader As StreamReader = Nothing
            Try
                reader = New StreamReader(o.FileName, System.Text.Encoding.UTF8)
                While Not reader.EndOfStream()
                    Dim line As String = reader.ReadLine()
                    line = line.Trim()
                    If line <> "" Then
                        lines.Add(line)
                    End If
                End While
            Catch ex As Exception
                MessageBox.Show("Fehler beim Öffnen der Datei." & vbCrLf & "Fehler: " & ex.Message)
                Exit Sub
            Finally
                If reader IsNot Nothing Then
                    reader.Close()
                    reader.Dispose()
                End If
            End Try
            If lines.Count > 0 Then
                importiereVonSkill(lines)
            End If
        End If
    End Sub

    Private Sub AusZwischenablageToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles AusZwischenablageToolStripMenuItem1.Click
        If My.Computer.Clipboard.ContainsText() Then
            Dim txt As String
            Try
                txt = My.Computer.Clipboard.GetText()
            Catch ex As Exception
                MessageBox.Show("Fehler beim Laden des Textes aus der Zwischenablage: " & ex.Message)
                Exit Sub
            End Try

            Dim currentLine As New System.Text.StringBuilder("")
            Dim lines As New List(Of String)
            For i As Integer = 0 To txt.Count - 1
                If txt(i) = vbCrLf OrElse txt(i) = vbLf OrElse txt(i) = vbCr Then
                    If currentLine.Length > 0 Then
                        lines.Add(currentLine.ToString())
                    End If
                    currentLine.Clear()
                Else
                    currentLine.Append(txt(i))
                End If
            Next

            If lines.Count = 0 Then
                MessageBox.Show("Kann nicht aus Zwischenablage einfügen: Es sind keine richtigen Daten in der Zwischenablage")
                Return
            End If
            If Not lines(0).StartsWith("skill-export: v") Then
                MessageBox.Show("Kann nicht aus Zwischenablage einfügen: Es sind keine richtigen Daten in der Zwischenablage")
                Return
            End If
            importiereVonSkill(lines)
        Else
            MessageBox.Show("Kein Text in der Zwischenablage vorhanden.")
        End If
    End Sub

    Private Sub importiereVonSkill(lines As List(Of String))
        Dim neueBib As Bibliothek = Me.myBibliothek.FlatCopyOhneLocal()
        Dim frm As New SkillDetektionForm()
        Try
            frm.setLines(lines, neueBib)
        Catch ex As Exception
            MessageBox.Show("Fehler beim Einlesen der Datei: " & ex.Message)
            Exit Sub
        End Try
        If frm.ShowDialog(Me) = DialogResult.OK Then
            myBibliothek = frm.bib
            Dim ms As New MemoryStream()
            Dim writer As New BinaryWriter(ms)
            frm.Vektor_Picturebox1.speicherGrafik(writer)
            writer.Flush()
            ms.Position = 0
            Dim reader1 As New BinaryReader(ms)
            Vektor_Picturebox1.ladeGrafik(reader1, Speicher_Version, myBibliothek, New LokaleBibliothek(), False)
            ms.Close()
            ms.Dispose()

            OnBibliothekChanged()
            myRückgängigVerwaltung.löscheAlleRückgängig()
            updateRückEnable()

            AktuellerSpeicherpfad = ""
        End If
    End Sub
#End Region
#End Region

#Region "Bearbeiten (MenuStrip)"
    Private Sub BearbeitenToolStripMenuItem_DropDownOpening(sender As Object, e As EventArgs) Handles BearbeitenToolStripMenuItem.DropDownOpening
        GruppeErstellenToolStripMenuItem.Enabled = Vektor_Picturebox1.kannGruppieren()
        GruppeAuflösenToolStripMenuItem.Enabled = Vektor_Picturebox1.kannGruppeAuflösen()
    End Sub

    Private Sub MoveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MoveToolStripMenuItem.Click
        Vektor_Picturebox1.startToolMove()
    End Sub

    Private Sub SkalierenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SkalierenToolStripMenuItem.Click
        Vektor_Picturebox1.startToolScale()
    End Sub

    Private Sub WireToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WireToolStripMenuItem.Click
        Vektor_Picturebox1.startToolWire()
    End Sub

    Private Sub PlaceInstanceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PlaceInstanceToolStripMenuItem.Click
        Vektor_Picturebox1.startToolPlace()
    End Sub

    Private Sub ElementLöschenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ElementLöschenToolStripMenuItem.Click
        Vektor_Picturebox1.deleteOrStartToolDelete()
    End Sub

#Region "Hinzufügen Beschriftung"
    Private Sub StromToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StromToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddCurrentArrow()
    End Sub

    Private Sub SpannungToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SpannungToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddVoltageArrow()
    End Sub

    Private Sub BusBeschriftungToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BusBeschriftungToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddBusTap()
    End Sub

    Private Sub LabelToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LabelToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddLabel()
    End Sub

    Private Sub ImpedanzToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImpedanzToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddImpedanceArrow()
    End Sub
#End Region

#Region "Hinzufügen Drawing"
    Private Sub LinieToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LinieToolStripMenuItem.Click
        Vektor_Picturebox1.startToolDrawLine()
    End Sub

    Private Sub BezierkurveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BezierkurveToolStripMenuItem.Click
        Vektor_Picturebox1.startToolDrawBezier()
    End Sub

    Private Sub BezierkurveZeichnenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles BezierkurveZeichnenToolStripMenuItem.Click
        Vektor_Picturebox1.startToolDrawBezierFreihand()
    End Sub

    Private Sub RechteckToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RechteckToolStripMenuItem.Click
        startToolRect(ToolDrawRect.FormArt.Rect)
    End Sub

    Private Sub EllipseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EllipseToolStripMenuItem.Click
        startToolRect(ToolDrawRect.FormArt.Ellipse)
    End Sub

    Private Sub TextfeldToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TextfeldToolStripMenuItem.Click
        startToolRect(ToolDrawRect.FormArt.Textfeld)
    End Sub

    Private Sub AbgerundetesRechteckToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AbgerundetesRechteckToolStripMenuItem.Click
        startToolRect(ToolDrawRect.FormArt.RoundRect)
    End Sub

    Private Sub GraphToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GraphToolStripMenuItem.Click
        startToolRect(ToolDrawRect.FormArt.Graph)
    End Sub

    Private Sub startToolRect(art As ToolDrawRect.FormArt)
        Dim ct As Tool = Vektor_Picturebox1.getCurrentTool()
        If ct Is Nothing OrElse 'Wenn man noch kein Tool hat
           TypeOf ct IsNot ToolDrawRect OrElse 'Oder wenn man kein DrawRect Tool hat
           (TypeOf ct Is ToolDrawRect AndAlso DirectCast(ct, ToolDrawRect).form <> art) Then 'Oder wenn ein anderes DrawRect tool da ist

            Dim tr As New ToolDrawRect(art)
            Vektor_Picturebox1.startTool(tr)
        End If
    End Sub

    Private Sub SnappointToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SnappointToolStripMenuItem.Click
        Vektor_Picturebox1.startToolAddSnappoint()
    End Sub

    Private Sub SnappointLöschenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SnappointLöschenToolStripMenuItem.Click
        Vektor_Picturebox1.startToolDeleteSnappoint()
    End Sub
#End Region

#Region "Drehen, Spiegeln"
    Private Sub ImUhrzeigersinnDrehenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ImUhrzeigersinnDrehenToolStripMenuItem.Click
        Vektor_Picturebox1.dreheSelectedUm90Grad(False, False)
    End Sub

    Private Sub GegenDenUhrzeigersinnDrehenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles GegenDenUhrzeigersinnDrehenToolStripMenuItem.Click
        Vektor_Picturebox1.dreheSelectedUmMinus90Grad(False, False)
    End Sub

    Private Sub HorizontalSpiegelnToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles HorizontalSpiegelnToolStripMenuItem.Click
        Vektor_Picturebox1.dreheSelectedSpiegelnHorizontal(False, False)
    End Sub

    Private Sub VertikalSpiegelnToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VertikalSpiegelnToolStripMenuItem.Click
        Vektor_Picturebox1.dreheSelectedSpiegelnVertikal(False, False)
    End Sub
#End Region

#Region "Reihenfolge"
    Private Sub InDenVordergrundToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles InDenVordergrundToolStripMenuItem1.Click
        Vektor_Picturebox1.Selected_Elemente_to_fore_back(False)
    End Sub

    Private Sub InDenHintergrundToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles InDenHintergrundToolStripMenuItem1.Click
        Vektor_Picturebox1.Selected_Elemente_to_fore_back(True)
    End Sub

    Private Sub EineEbeneNachVorneToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EineEbeneNachVorneToolStripMenuItem.Click
        Vektor_Picturebox1.Selected_Elemente_one_level_fore_back(False)
    End Sub

    Private Sub EineEbeneNachHintenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles EineEbeneNachHintenToolStripMenuItem.Click
        Vektor_Picturebox1.Selected_Elemente_one_level_fore_back(True)
    End Sub
#End Region

#Region "Copy Paste"
    Private Sub general_paste_from_keyboard()
        If My.Computer.Clipboard.ContainsText() Then
            Try
                Dim txt As String = My.Computer.Clipboard.GetText()
                If txt.StartsWith("skill-export: v") Then
                    AusZwischenablageToolStripMenuItem1_Click(Nothing, EventArgs.Empty)
                    Exit Sub
                End If
            Catch ex As Exception
            End Try
        End If

        AusZwischenablageEinfügenToolStripMenuItem_Click(Nothing, EventArgs.Empty)
    End Sub

    Private Sub InZwischenablageKopierenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InZwischenablageKopierenToolStripMenuItem.Click
        Vektor_Picturebox1.inZwischenablageKopieren(myBibliothek)
    End Sub

    Private Sub AusZwischenablageEinfügenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AusZwischenablageEinfügenToolStripMenuItem.Click
        If Vektor_Picturebox1.ausZwischenablageEinfügen(myBibliothek) Then
            OnBibliothekChanged()
        End If
    End Sub
#End Region

    Private Sub GruppeErstellenToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles GruppeErstellenToolStripMenuItem.Click
        Vektor_Picturebox1.gruppeErstellen()
    End Sub

    Private Sub GruppeAuflösenToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles GruppeAuflösenToolStripMenuItem.Click
        Vektor_Picturebox1.gruppeAuflösen()
    End Sub
#End Region

#Region "Markierung"
    Private Sub MarkierungAufhebenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MarkierungAufhebenToolStripMenuItem.Click
        Vektor_Picturebox1.deselect_All()
    End Sub

    Private Sub AllesMarkierenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AllesMarkierenToolStripMenuItem.Click
        Vektor_Picturebox1.selectAll()
    End Sub
#End Region

#Region "Optionen"
    Private Sub StandardstileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StandardstileToolStripMenuItem.Click
        Dim einstellungen As New List(Of ElementEinstellung)
        einstellungen.Add(New Einstellung_Linienstil(My.Resources.Strings.DefaultEinstellung_Bauteile, 0, Vektor_Picturebox1.myLineStyles))
        einstellungen.Add(New Einstellung_Linienstil(My.Resources.Strings.DefaultEinstellung_Verbinder, 1, Vektor_Picturebox1.myLineStyles))
        einstellungen.Add(New Einstellung_Linienstil(My.Resources.Strings.DefaultEinstellung_StromSpannungspfeile, 3, Vektor_Picturebox1.myLineStyles))
        einstellungen.Add(New Einstellung_Linienstil(My.Resources.Strings.DefaultEinstellung_Grafikelemente, 2, Vektor_Picturebox1.myLineStyles))
        einstellungen.Add(New Einstellung_Fontstyle(My.Resources.Strings.DefaultEinstellung_Font, 0, Vektor_Picturebox1.myFonts))

        Dim frm As New FRM_ElementEinstellungen()
        frm.Height = 720
        frm.init(einstellungen)
        frm.Text = My.Resources.Strings.DefaultEinstellung
        If frm.ShowDialog() = DialogResult.OK Then
            Dim rück As New RückgängigMulti()
            rück.setText("Standardstile geändert")
            Dim rück1 As New RückgängigLineStyle()
            rück1.speicherVorher(Vektor_Picturebox1.getRückArgs())
            Dim rück2 As New RückgängigFillStyles()
            rück2.speicherVorher(Vektor_Picturebox1.getRückArgs())
            Dim rück3 As New RückgängigFontStyle()
            rück3.speicherVorher(Vektor_Picturebox1.getRückArgs())

            Dim changed As Boolean = False
            DirectCast(einstellungen(0), Einstellung_Linienstil).getNewLinienstil(0, Vektor_Picturebox1.myLineStyles, changed, True)
            DirectCast(einstellungen(1), Einstellung_Linienstil).getNewLinienstil(1, Vektor_Picturebox1.myLineStyles, changed, True)
            DirectCast(einstellungen(2), Einstellung_Linienstil).getNewLinienstil(3, Vektor_Picturebox1.myLineStyles, changed, True)
            DirectCast(einstellungen(3), Einstellung_Linienstil).getNewLinienstil(2, Vektor_Picturebox1.myLineStyles, changed, True)
            DirectCast(einstellungen(4), Einstellung_Fontstyle).getNewFontstyle(0, Vektor_Picturebox1.myFonts, changed, True)

            If changed Then
                rück1.speicherNachher(Vektor_Picturebox1.getRückArgs())
                rück2.speicherNachher(Vektor_Picturebox1.getRückArgs())
                rück3.speicherNachher(Vektor_Picturebox1.getRückArgs())

                If rück1.RückBenötigt() Then rück.Rück.Add(rück1)
                If rück2.RückBenötigt() Then rück.Rück.Add(rück2)
                If rück3.RückBenötigt() Then rück.Rück.Add(rück3)

                If rück.Rück.Count > 0 Then
                    Vektor_Picturebox1.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
                End If
                Vektor_Picturebox1.Invalidate()
            End If
        End If
    End Sub

    Private Sub ElementEinstellungenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ElementEinstellungenToolStripMenuItem.Click
        showEinstellungen()
    End Sub

    Private Sub showEinstellungen()
        If Vektor_Picturebox1.has_selection() Then
            Dim einstellungen As List(Of ElementEinstellung) = Vektor_Picturebox1.getEinstellungenOfSelectedElements()
            If einstellungen.Count > 0 Then
                Dim frm As New FRM_ElementEinstellungen()
                frm.init(einstellungen)
                If frm.ShowDialog() = DialogResult.OK Then
                    Vektor_Picturebox1.setEinstellungenSelected(einstellungen)
                End If
            End If
        End If
    End Sub

    Private Sub VoreinstellungenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VoreinstellungenToolStripMenuItem.Click
        Dim eins As New Einstellungen(Me.myBibliothek, FRM_BauelementeAuswahl, Vektor_Picturebox1)
        eins.ShowDialog()
        BtnGravity.Checked = Settings.getSettings().Gravity
        Vektor_Picturebox1.enable_gravity = Settings.getSettings().Gravity
        Vektor_Picturebox1.gravityStärke = Settings.getSettings().Gravity_Stärke
        Vektor_Picturebox1.crossCursor = Settings.getSettings().CrossCursor
        Vektor_Picturebox1.snappoinsImmerAnzeigen = Settings.getSettings().SnappointsImmerAnzeigen
        Vektor_Picturebox1.showBoarder = Settings.getSettings().RandAnzeigen
        Vektor_Picturebox1.TextVorschauMode = Settings.getSettings().TextVorschauMode
        Vektor_Picturebox1.MultiWireSelect = Settings.getSettings().MultiSelect

        BtnSnappointsImmerAnzeigen.Checked = Vektor_Picturebox1.snappoinsImmerAnzeigen
        BtnRandAnzeigen.Checked = Vektor_Picturebox1.showBoarder
        BtnCrossCursor.Checked = Vektor_Picturebox1.crossCursor
        BtnSelectMode.Checked = Vektor_Picturebox1.MultiWireSelect
    End Sub

    Private Sub InformationToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles InformationToolStripMenuItem1.Click
        Information.ShowDialog()
    End Sub
#End Region

#Region "Tools (MenuItem)"
    Private Sub VerbinderOptimierenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VerbinderOptimierenToolStripMenuItem.Click
        Vektor_Picturebox1.RoutingVerbessern(True, True, Integer.MaxValue)
    End Sub

    Private Sub LatexBeschriftungstexttextEinfügenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LatexBeschriftungstexttextEinfügenToolStripMenuItem.Click
        Vektor_Picturebox1.beschriftungNichtKursiv()
    End Sub
#End Region

#Region "Rückgängig"
    Private Sub macheRück()
        Vektor_Picturebox1.VorRückVorToolAbbrechen()
        myRückgängigVerwaltung.macheRück()
        FRM_lade_Einstellungen()
        updateRückEnable()
    End Sub

    Private Sub macheVor()
        Vektor_Picturebox1.VorRückVorToolAbbrechen()
        myRückgängigVerwaltung.macheVorgängig()
        FRM_lade_Einstellungen()
        updateRückEnable()
    End Sub

    Private Sub Vektor_Picturebox1_NeuesRückgängigElement(sender As Vektor_Picturebox, e As NeuesRückgängigEventArgs) Handles Vektor_Picturebox1.NeuesRückgängigElement
        myRückgängigVerwaltung.addRückElement(e.R)
        FRM_lade_Einstellungen()
        updateRückEnable()
    End Sub

    Private Sub BtnRück_Click(sender As Object, e As EventArgs) Handles BtnRück.Click
        macheRück()
    End Sub

    Private Sub BtnVorgängig_Click(sender As Object, e As EventArgs) Handles BtnVorgängig.Click
        macheVor()
    End Sub

    Private Sub updateRückEnable()
        BtnRück.Enabled = myRückgängigVerwaltung.kannRückgängig()
        BtnVorgängig.Enabled = myRückgängigVerwaltung.kannVorgängig()
    End Sub
#End Region

#Region "Sonstige Toolstrip-Funktionen"
    Private Sub ToolStripButton1_Click(sender As Object, e As EventArgs) Handles ToolStripButton1.Click
        If Vektor_Picturebox1.myMoveRichtung = Vektor_Picturebox.MoveRichtung.AlleRichtungen Then
            Vektor_Picturebox1.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig
            ToolStripButton1.Image = My.Resources.Rechtwinklig2
        Else
            Vektor_Picturebox1.myMoveRichtung = Vektor_Picturebox.MoveRichtung.AlleRichtungen
            ToolStripButton1.Image = My.Resources.AlleRichtungen2
        End If
    End Sub

    Private Sub BtnGravity_Click(sender As Object, e As EventArgs) Handles BtnGravity.Click
        Settings.getSettings().Gravity = Not Settings.getSettings().Gravity
        Vektor_Picturebox1.enable_gravity = Settings.getSettings().Gravity
        BtnGravity.Checked = Vektor_Picturebox1.enable_gravity
    End Sub

    Private Sub BtnSelectMode_Click(sender As Object, e As EventArgs) Handles BtnSelectMode.Click
        Settings.getSettings().MultiSelect = Not Settings.getSettings().MultiSelect
        Vektor_Picturebox1.MultiWireSelect = Settings.getSettings().MultiSelect
        Vektor_Picturebox1.Invalidate()
        BtnSelectMode.Checked = Vektor_Picturebox1.MultiWireSelect
    End Sub

    Private Sub Btn_DrawDots_Click(sender As Object, e As EventArgs) Handles Btn_DrawDots.Click
        Btn_DrawDots.Checked = Not Btn_DrawDots.Checked
        Vektor_Picturebox1.drawDotsOnIntersectingWires = Btn_DrawDots.Checked
        Vektor_Picturebox1.Invalidate()
    End Sub

    Private Sub Btn_DrawZeilensprünge_Click(sender As Object, e As EventArgs) Handles Btn_DrawZeilensprünge.Click
        Btn_DrawZeilensprünge.Checked = Not Btn_DrawZeilensprünge.Checked
        Vektor_Picturebox1.drawZeilensprünge = Btn_DrawZeilensprünge.Checked
        Vektor_Picturebox1.Invalidate()
    End Sub

    Private Sub BtnSnappointsImmerAnzeigen_Click(sender As Object, e As EventArgs) Handles BtnSnappointsImmerAnzeigen.Click
        BtnSnappointsImmerAnzeigen.Checked = Not BtnSnappointsImmerAnzeigen.Checked
        Vektor_Picturebox1.snappoinsImmerAnzeigen = BtnSnappointsImmerAnzeigen.Checked
        Vektor_Picturebox1.Invalidate()
        Settings.getSettings().SnappointsImmerAnzeigen = Vektor_Picturebox1.snappoinsImmerAnzeigen
    End Sub

    Private Sub BtnRandAnzeigen_Click(sender As Object, e As EventArgs) Handles BtnRandAnzeigen.Click
        BtnRandAnzeigen.Checked = Not BtnRandAnzeigen.Checked
        Vektor_Picturebox1.showBoarder = BtnRandAnzeigen.Checked
        Vektor_Picturebox1.Invalidate()
        Settings.getSettings().RandAnzeigen = Vektor_Picturebox1.showBoarder
    End Sub

    Private Sub BtnCrossCursor_Click(sender As Object, e As EventArgs) Handles BtnCrossCursor.Click
        BtnCrossCursor.Checked = Not BtnCrossCursor.Checked
        Vektor_Picturebox1.crossCursor = BtnCrossCursor.Checked
        Vektor_Picturebox1.Invalidate()
        Settings.getSettings().CrossCursor = Vektor_Picturebox1.crossCursor
    End Sub
#End Region

    Private Sub Vektor_Picturebox1_ToolInfoTextChanged(sender As Vektor_Picturebox, e As ToolInfoTextEventArgs) Handles Vektor_Picturebox1.ToolInfoTextChanged
        ToolStripStatusLabel1.Text = e.Text
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        Dim ctr As Control = Me.ActiveControl()
        If ctr.Equals(Vektor_Picturebox1) Then
            Vektor_Picturebox1.KeyUpRaised(e)
        End If
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        Dim ctr As Control = Me.ActiveControl()

        If Not TypeOf ctr Is TextBox AndAlso
           Not TypeOf ctr Is RichTextBox AndAlso
           Not TypeOf ctr Is WR_Einstellungsform Then

            Dim k As Key_Settings = Key_Settings.getSettings()

            If e.KeyCode = Keys.Left OrElse e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down OrElse e.KeyCode = Keys.Right Then
                If ctr.Equals(Vektor_Picturebox1) Then
                    Vektor_Picturebox1.KeyDownRaised(e)
                End If
            Else
                If k.keyUndo.isDown(e) Then
                    macheRück()
                ElseIf k.keyRedo.isDown(e) Then
                    macheVor()
                ElseIf k.keyDateiSpeichern.isDown(e) Then
                    speichern()
                ElseIf k.keyShowElementEinstellungen.isDown(e) Then
                    showEinstellungen()
                ElseIf k.keyChangeMoveMode.isDown(e) Then
                    ToolStripButton1_Click(ToolStripButton1, EventArgs.Empty)
                ElseIf k.keyChangeGravity.isDown(e) Then
                    BtnGravity_Click(BtnGravity, EventArgs.Empty)
                ElseIf k.keyMultiSelect.isDown(e) Then
                    BtnSelectMode_Click(BtnSelectMode, EventArgs.Empty)
                ElseIf k.keyCopy.isDown(e) Then
                    InZwischenablageKopierenToolStripMenuItem_Click(Nothing, EventArgs.Empty)
                ElseIf k.keyPaste.isDown(e) Then
                    general_paste_from_keyboard()
                ElseIf k.keyExportiereAlsTEX.isDown(e) Then
                    AlsPDFTEXToolStripMenuItem_Click(Nothing, EventArgs.Empty)
                ElseIf k.keyExportiereAlsEMF.isDown(e) Then
                    AlsEMFToolStripMenuItem_Click(Nothing, EventArgs.Empty)
                ElseIf k.keyEbeneNachVorne.isDown(e) Then
                    EineEbeneNachVorneToolStripMenuItem_Click(Nothing, EventArgs.Empty)
                ElseIf k.keyEbeneNachHinten.isDown(e) Then
                    EineEbeneNachHintenToolStripMenuItem_Click(Nothing, EventArgs.Empty)
                Else
                    Dim hatAbgearbeitet As Boolean = False
                    For i As Integer = 0 To Settings.getSettings().KeysSelectInstance.Count - 1
                        If Settings.getSettings().KeysSelectInstance(i).key.isDown(e) Then
                            selectBauteil(Settings.getSettings().KeysSelectInstance(i)._namespace, Settings.getSettings().KeysSelectInstance(i)._cell)
                            hatAbgearbeitet = True
                            Exit For
                        End If
                    Next
                    If Not hatAbgearbeitet Then
                        Vektor_Picturebox1.KeyDownRaised(e)
                    End If
                End If
            End If
        End If

        MyBase.OnKeyDown(e)

    End Sub

    Private Sub updateUI()
        Btn_DrawZeilensprünge.Checked = Vektor_Picturebox1.drawZeilensprünge
        Btn_DrawDots.Checked = Vektor_Picturebox1.drawDotsOnIntersectingWires
        BtnSnappointsImmerAnzeigen.Checked = Vektor_Picturebox1.snappoinsImmerAnzeigen
        Settings.getSettings().SnappointsImmerAnzeigen = Vektor_Picturebox1.snappoinsImmerAnzeigen
        BtnRandAnzeigen.Checked = Vektor_Picturebox1.showBoarder
        Settings.getSettings().RandAnzeigen = Vektor_Picturebox1.showBoarder
        BtnCrossCursor.Checked = Vektor_Picturebox1.crossCursor
        Settings.getSettings().CrossCursor = Vektor_Picturebox1.crossCursor
    End Sub

    Private Sub RandEinstellenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles RandEinstellenToolStripMenuItem.Click
        Dim mf As New MarginEinstellen()
        mf.Value = Vektor_Picturebox1.MarginExport
        If mf.ShowDialog = DialogResult.OK Then
            Vektor_Picturebox1.MarginExport = mf.Value
        End If
    End Sub

    Private Sub ExportEinstellungenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportEinstellungenToolStripMenuItem.Click
        Dim ee As New Export_Einstellungen()
        ee.skalierung = Vektor_Picturebox1.MM_PER_INT
        If ee.ShowDialog = DialogResult.OK Then
            Vektor_Picturebox1.MM_PER_INT = ee.skalierung
        End If
    End Sub
End Class