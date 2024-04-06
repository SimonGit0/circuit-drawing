Public Class Ctr_BauelementeAuswahl_Skill

    Private myBib As Bibliothek
    Private myLinestylelist As LineStyleList
    Private myFillstylelist As FillStyleList
    Private myFontList As FontList

    Private myElement As Detektion.MasterElemente

    Private CellName As String
    Private LibName As String
    Private einstellungen As New List(Of default_Parameter)
    Private rotation As Drehmatrix
    Private keinElement As Boolean
    Private alsLokalesElement As Boolean

    Private nichtAufUIreagieren As Boolean = False
    Private drehungManuellGewählt As Boolean = False

    Public Sub New()
        InitializeComponent()

        myLinestylelist = New LineStyleList()
        Dim ls As New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 0.2, New DashStyle(0))
        ls.alwaysUsePenWidthOfOne = True
        myLinestylelist.add(ls)

        myFillstylelist = New FillStyleList()
        Dim fs As New FillStyle(New Farbe(0, 0, 0, 0))
        myFillstylelist.add(fs)

        myFontList = New FontList()
        myFontList.add(New FontStyle(New Farbe(255, 0, 0, 0), "Times", 10.0, False, False))

        ComboBox3.Items.Add(My.Resources.Strings.Skill_Drehung0)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_Drehung90)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_Drehung180)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_Drehung270)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_MX)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_MX_Rot90)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_MY)
        ComboBox3.Items.Add(My.Resources.Strings.Skill_MY_Rot90)
    End Sub

    Private Function getCmb3Index(drehung As Drehmatrix) As Integer
        Select Case drehung.getDrehung
            Case Drehmatrix.Drehungen.Normal
                Return 0
            Case Drehmatrix.Drehungen.Rot90
                Return 1
            Case Drehmatrix.Drehungen.Rot180
                Return 2
            Case Drehmatrix.Drehungen.Rot270
                Return 3
            Case Drehmatrix.Drehungen.MirrorX
                Return 4
            Case Drehmatrix.Drehungen.MirrorXRot90
                Return 5
            Case Drehmatrix.Drehungen.MirrorXRot180
                Return 6
            Case Drehmatrix.Drehungen.MirrorXRot270
                Return 7
        End Select
        Throw New Exception("Unbekannte Drehung")
    End Function

    Private Function getDrehungVonCmb3Index(index As Integer) As Drehmatrix
        Select Case index
            Case 0
                Return Drehmatrix.getIdentity()
            Case 1
                Return Drehmatrix.Drehen90Grad
            Case 2
                Dim erg = Drehmatrix.Drehen90Grad
                erg.um90GradDrehen()
                Return erg
            Case 3
                Return Drehmatrix.DrehenMinus90Grad
            Case 4
                Return Drehmatrix.MirrorX
            Case 5
                Dim erg = Drehmatrix.MirrorX
                erg.um90GradDrehen()
                Return erg
            Case 6
                Return Drehmatrix.MirrorY
            Case 7
                Dim erg = Drehmatrix.MirrorY
                erg.um90GradDrehen()
                Return erg
        End Select
        Throw New Exception("Unbekannte Drehung")
    End Function

    Public Sub init(bib As Bibliothek, master As Detektion.MasterElemente, zuOrdnungInit As Detektion.Skill_BauteilZuordnung)
        Me.myBib = bib
        Me.myElement = master

        If master.isTerminal Then
            lblHeader.Text = "Terminal"
        Else
            lblHeader.Text = master.LibName & " -> " & master.CellName
        End If

        'Bild anzeigen
        '------------------
        PictureBox1.Image = master.getBitmap(PictureBox1.Width, PictureBox1.Height, New Pen(Color.FromArgb(0, 204, 102)), False)
        '-------------------

        ComboBox1.Items.Add(My.Resources.Strings.Skill_Kein_Bauteil_Zuordnen)
        ComboBox1.Items.Add(My.Resources.Strings.Skill_Symbol_importieren)

        For Each _namespace As KeyValuePair(Of String, BauteileNamespace) In bib

            Dim cellsNamen() As String = _namespace.Value.getCellsNamen()
            Dim cells As New List(Of String)()
            For i As Integer = 0 To cellsNamen.Length - 1
                If Not _namespace.Value.getCell(cellsNamen(i)).getFirst().template.getIsDeko() Then
                    cells.Add(cellsNamen(i))
                End If
            Next
            If cells.Count > 0 Then
                ComboBox1.Items.Add(_namespace.Key)
            End If
        Next

        ComboBox1.SelectedIndex = 1
        updateBauteil(False)

        If zuOrdnungInit IsNot Nothing Then
            Dim libName As String = zuOrdnungInit.libName_Hier
            Dim cellName As String = zuOrdnungInit.cellName_Hier
            If zuOrdnungInit.keinBauteilZugeordnet Then
                ComboBox1.SelectedIndex = 0
                updateBauteil(True)
            Else
                Dim weiter As Boolean = False
                For i As Integer = 2 To ComboBox1.Items.Count - 1
                    If ComboBox1.Items(i).ToString() = libName Then
                        ComboBox1.SelectedIndex = i
                        weiter = True
                        Exit For
                    End If
                Next
                If weiter Then
                    weiter = False
                    For i As Integer = 0 To ComboBox2.Items.Count - 1
                        If ComboBox2.Items(i).ToString() = cellName Then
                            ComboBox2.SelectedIndex = i
                            weiter = True
                            Exit For
                        End If
                    Next
                    If weiter Then
                        ComboBox3.SelectedIndex = getCmb3Index(zuOrdnungInit.rotation)
                        Me.einstellungen.Clear()
                        Me.einstellungen.AddRange(zuOrdnungInit.einstellungen)
                        updateBauteil(True)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.SelectedIndex = 0 Then
            ComboBox2.Enabled = False
            ComboBox3.Enabled = False
            Button1.Enabled = False

            updateBauteil(False)
        ElseIf ComboBox1.SelectedIndex = 1 Then
            ComboBox2.Enabled = False
            ComboBox3.Enabled = False
            Button1.Enabled = False

            updateBauteil(False)
        Else
            ComboBox2.Enabled = True
            ComboBox3.Enabled = True

            Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))
            If _namespace.getCellCount() > 0 Then
                Dim cellsNamen() As String = _namespace.getCellsNamen()
                Dim cells As New List(Of String)()
                For i As Integer = 0 To cellsNamen.Length - 1
                    If Not _namespace.getCell(cellsNamen(i)).getFirst().template.getIsDeko() Then
                        cells.Add(cellsNamen(i))
                    End If
                Next
                ComboBox2.SuspendLayout()
                ComboBox2.Items.Clear()
                ComboBox2.Items.AddRange(cells.ToArray())
                ComboBox2.SelectedIndex = 0
                ComboBox2.ResumeLayout()

                updateBauteil(False)

            End If

        End If
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        updateBauteil(False)
    End Sub

    Private Sub updateBauteil(NurEinstellungenChanged As Boolean)
        If ComboBox1.SelectedIndex <> -1 Then
            If ComboBox1.SelectedIndex = 0 Then
                Me.einstellungen.Clear()
                Me.LibName = ""
                Me.CellName = ""
                Me.rotation = Me.getDrehungVonCmb3Index(0)
                Me.drehungManuellGewählt = False
                keinElement = True
                alsLokalesElement = False
                PictureBox2.Image = Nothing
                ckb_speichern.Visible = True
            ElseIf ComboBox1.SelectedIndex = 1 Then
                Me.einstellungen.Clear()
                Me.LibName = ""
                Me.CellName = ""
                Me.rotation = Me.getDrehungVonCmb3Index(0)
                Me.drehungManuellGewählt = False
                keinElement = False
                alsLokalesElement = True
                PictureBox2.Image = Me.myElement.getBitmap(PictureBox2.Width, PictureBox2.Height, Pens.Black, True)
                ckb_speichern.Visible = False
            Else
                ckb_speichern.Visible = True
                keinElement = False
                alsLokalesElement = False
                If ComboBox2.SelectedIndex <> -1 Then
                    Dim bauteil As BauteilCell = myBib.getNamespace(CStr(ComboBox1.SelectedItem)).getCell(CStr(ComboBox2.SelectedItem))

                    Dim tmpl As TemplateAusDatei = bauteil.getFirst().template
                    Dim tmpl_compiled As Template_Compiled = Nothing

                    If Not NurEinstellungenChanged Then
                        Me.einstellungen.Clear()

                        'ReDim myBauteil.einstellungen(tmpl.getNrOfParams() - 1)
                        Dim einstellungen() As ParamValue = tmpl.getDefaultParameters_copy()

                        Dim anzahlSnappoints As Integer = myElement.pins.Count
                        tmpl.recompile(einstellungen, 0, tmpl_compiled, Nothing)
                        Dim anzahl As Integer = tmpl_compiled.getNrOfSnappoints()
                        If anzahl <> anzahlSnappoints Then
                            'Suche "beste" Konfiguration, die am besten passt hinsichtlich der Anzahl an Snappoints!
                            Dim hat_Gefunden As Boolean
                            For i As Integer = 0 To einstellungen.Count - 1
                                If TypeOf tmpl.getParameter(i) Is TemplateParameter_Param Then
                                    hat_Gefunden = False
                                    For k As Integer = 0 To DirectCast(tmpl.getParameter(i), TemplateParameter_Param).options.Count - 1
                                        einstellungen(i) = New ParamInt(k)
                                        tmpl.recompile(einstellungen, 0, tmpl_compiled, Nothing)
                                        anzahl = tmpl_compiled.getNrOfSnappoints()

                                        If anzahl = anzahlSnappoints Then
                                            Dim name As String = DirectCast(tmpl.getParameter(i), TemplateParameter_Param).name
                                            Dim value As String = DirectCast(tmpl.getParameter(i), TemplateParameter_Param).options(k)
                                            Me.einstellungen.Add(New default_Parameter(Mathe.strToLower(name), Mathe.strToLower(value)))
                                            hat_Gefunden = True
                                            Exit For
                                        End If
                                    Next
                                    If hat_Gefunden Then
                                        Exit For
                                    Else
                                        einstellungen(i) = tmpl.getDefaultParameter_copy(i)
                                    End If
                                End If
                            Next
                        End If
                    End If
                    Dim Current_einstellungen() As ParamValue = getCurrentEinstellungen(tmpl)
                    tmpl.recompile(Current_einstellungen, 0, tmpl_compiled, Nothing)

                    Dim grafik As DO_Grafik = tmpl_compiled.getGrafik()
                    Dim boundsTemplate As Rectangle = grafik.getBoundingBox()

                    Dim snappoints(tmpl_compiled.getNrOfSnappoints() - 1) As Snappoint
                    For k As Integer = 0 To snappoints.Length - 1
                        snappoints(k) = tmpl_compiled.getSnappoint(k)
                    Next

                    If Not drehungManuellGewählt Then
                        'Optimiere Ausrichtung
                        Dim bestPinZuordnung() As Integer = Nothing
                        Dim bestDrehung As Drehmatrix
                        Dim minDistGesamt As Single = Single.MaxValue
                        Dim drehung As Drehmatrix = Drehmatrix.getIdentity()
                        For i As Integer = 0 To 7
                            Dim distGesamt As Single
                            Dim pinZuordnung() As Integer = Detektion.SkillDetectSchematic.getPinZuordnung(snappoints, boundsTemplate, myElement, distGesamt, drehung)
                            distGesamt -= 0.1F * Skill_BauelementeAuswählen.getBewertungVonDrehung(drehung)
                            If distGesamt < minDistGesamt Then
                                minDistGesamt = distGesamt
                                bestPinZuordnung = pinZuordnung
                                bestDrehung = drehung
                            End If
                            drehung.um90GradDrehen()
                            If i = 3 Then
                                drehung.SpielgenX()
                            End If
                        Next
                        Me.rotation = bestDrehung

                        nichtAufUIreagieren = True
                        ComboBox3.SelectedIndex = getCmb3Index(bestDrehung)
                        nichtAufUIreagieren = False
                    Else
                        Me.rotation = getDrehungVonCmb3Index(ComboBox3.SelectedIndex)
                    End If

                    If tmpl.getDefaultParameters_copy().Length > 0 Then
                        Button1.Enabled = True
                    Else
                        Button1.Enabled = False
                    End If

                    Dim bmp As Bitmap = getBitmap(bauteil, Current_einstellungen)
                    PictureBox2.Image = bmp

                    CellName = CStr(ComboBox2.SelectedItem)
                    LibName = CStr(ComboBox1.SelectedItem)
                End If
            End If
        End If
    End Sub

    Private Function getBitmap(cell As BauteilCell, einstellungen() As ParamValue) As Bitmap
        Dim breite As Integer = PictureBox2.Width
        Dim höhe As Integer = PictureBox2.Height
        Dim rand As Integer = 4

        Dim view As BauteilView = cell.getFirst()
        Dim template As TemplateAusDatei = view.template
        Dim tmpl_compiled As Template_Compiled = Nothing
        template.recompile(einstellungen, 0, tmpl_compiled, Nothing)
        Dim grafik As DO_Grafik = tmpl_compiled.getGrafik()
        Dim faktor As Single
        Dim offsetX As Single
        Dim offsetY As Single

        Dim t As New Transform_rotate(Me.rotation)
        grafik.transform(t)

        Dim bounds As Rectangle = grafik.getBoundingBox()

        faktor = CSng(Math.Min((breite - rand) / bounds.Width, (höhe - rand) / bounds.Height))
        offsetX = CSng(breite / 2 - (bounds.X + bounds.Width / 2) * faktor)
        offsetY = CSng(höhe / 2 - (bounds.Y + bounds.Height / 2) * faktor)

        Dim bild As New Bitmap(breite, höhe)

        Using g As Graphics = Graphics.FromImage(bild)
            g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
            g.Clear(Color.Transparent)
            g.CompositingMode = Drawing2D.CompositingMode.SourceOver
            g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            Dim args As New GrafikDrawArgs(myLinestylelist, myFillstylelist, myFontList, faktor / Vektor_Picturebox.DEFAULT_MM_PER_INT, False)
            args.faktorX = faktor
            args.faktorY = faktor
            args.offsetX = offsetX
            args.offsetY = offsetY
            grafik.drawGraphics(g, args)
        End Using
        Return bild
    End Function

    Private Function getCurrentEinstellungen(tmpl As TemplateAusDatei) As ParamValue()
        Dim erg() As ParamValue = tmpl.getDefaultParameters_copy()
        tmpl.__lade_defaultParameterValues(erg, Me.einstellungen.ToArray(), False, False)
        Return erg
    End Function

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        If Not nichtAufUIreagieren Then
            If ComboBox3.SelectedIndex <> -1 Then
                Me.rotation = getDrehungVonCmb3Index(ComboBox3.SelectedIndex)
                Me.drehungManuellGewählt = True
                updateBauteil(True)
            End If
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim template As TemplateAusDatei = myBib.getBauteil(Me.LibName, Me.CellName, "eu")

        Dim frm As New Skill_Einstellungen_frm()
        frm.init(einstellungen, template)
        If frm.ShowDialog(Me) = DialogResult.OK Then
            Me.einstellungen = frm.getEinstellungen()
            updateBauteil(True)
        End If
    End Sub

    Public Function getZuordnung(SpeicherPfad As String) As Detektion.Skill_BauteilZuordnung
        Dim erg As Detektion.Skill_BauteilZuordnung

        If Me.alsLokalesElement Then
            createTemplateFromSkill(myBib)
        End If

        If Me.myElement.isTerminal Then
            erg = New Detektion.Skill_BauteilZuordnung(Detektion.Skill_BauteilZuordnung.LIBNAME_TERMINAL, Me.myElement.CellName, Me.LibName, Me.CellName, Me.einstellungen.ToArray(), Me.rotation, Me.keinElement)
        Else
            erg = New Detektion.Skill_BauteilZuordnung(Me.myElement.LibName, Me.myElement.CellName, Me.LibName, Me.CellName, Me.einstellungen.ToArray(), Me.rotation, Me.keinElement)
        End If
        If Me.ckb_speichern.Checked AndAlso Not Me.alsLokalesElement Then
            erg.speichern(SpeicherPfad)
        End If
        Return erg
    End Function

    Private Sub createTemplateFromSkill(bib As Bibliothek)
        Me.LibName = Bibliothek.NAMESPACE_LOKAL
        Me.CellName = myElement.LibName & "." & myElement.CellName

        Dim tmpl As New TemplateAusDatei(Me.myElement, Me.LibName, Me.CellName)
        bib.add_local_bauteil(tmpl)
    End Sub
End Class
