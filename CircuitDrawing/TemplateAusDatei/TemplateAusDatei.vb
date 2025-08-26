Imports System.IO
Imports System.Text

Public Class TemplateAusDatei

    Private Shared ReadOnly Empty_parentArgs As New CompileParentArgs()

    Public Const PENALTY0 As Integer = 0
    Public Const PENALTY1 As Integer = 2
    Public Const PENALTY2 As Integer = 5

    Private my_Precompiled_Template As Precompiled_Template
    Private currentGrafik As Precompiled_MultiGrafik 'nur zum Einlesen!

    Private TextposTypes As List(Of Integer)
    Private currentTextpos As PrecompiledTextpos 'nur zum Einlesen!
    Private DefaultName As String

    Private konstanten_CurrentBlock As Stack(Of Dictionary(Of String, Integer)) 'nur zum Einlesen!
    Private parameter_intern As List(Of ParamName) 'nur zum Einlesen!
    Private Erster_Echter_Paramter As Integer 'nur zum Einlesen!

    Private AnzahlVarsIntern As Integer
    Private vars_intern As List(Of VariableEinlesen) 'nur zum Einlesen!

    Private parameter As List(Of TemplateParameter)
    Private defaulParameterValues() As ParamValue

    Private templatesCompatibility As List(Of TemplateCompatibility)
    Private currentTemplateCompatibility As TemplateCompatibility 'nur zum Einlesen!

    Private currentPrecompiledScaling As Precompiled_Scaling 'nur zum Einlesen!

    Private _namespace As String
    Private _name As String
    Private _view As String

    Private MitNummer As Boolean

    Private isDeko As Boolean

    Public Sub __changeName(neuerName As String)
        Me._name = neuerName
    End Sub

    Public Sub __changeNamespace(neuerNamespace As String)
        Me._namespace = neuerNamespace
    End Sub

    Private Sub New()
    End Sub

    Public Sub New(dateipfad As String)
        'Dim stpw As New Stopwatch()
        'stpw.Start()
        kompilieren(dateipfad)
        'stpw.Stop()
        'Debug.Print("Einlesen von " & _namespace & "." & _name & ": " & stpw.ElapsedMilliseconds.ToString())
    End Sub

#Region "Neues Template von Skill"
    Public Sub New(master As Detektion.MasterElemente, ns As String, name As String)
        Me._namespace = ns
        Me._name = name
        Me._view = "eu"
        Me.MitNummer = True
        Me.isDeko = False
        Me.DefaultName = ""
        Me.templatesCompatibility = New List(Of TemplateCompatibility) 'ist leer. Wird nicht benötigt.
        Me.AnzahlVarsIntern = 0 'es gibt keine interen variablen

        parameter = New List(Of TemplateParameter) 'parameter sind eine leere liste
        ReDim defaulParameterValues(parameter.Count - 1)

        'Grafik einlesen
        my_Precompiled_Template = New Precompiled_Template()
        currentGrafik = New Precompiled_MultiGrafik(0)
        'currentGrafik.add(New Precompiled_Rect(, koards(1), koards(2), koards(3), False, False))

        For Each shape As Detektion.MasterShape In master.rawShapes
            If shape.layer = "device" AndAlso shape.purpose = "drawing" Then
                Dim fill As Boolean = False
                Dim fm As Drawing_FillMode = Drawing_FillMode.OnlyStroke
                Select Case shape.type
                    Case "rect"
                        currentGrafik.add(New Precompiled_Rect(getIntX_vonSkill(shape.bBox.X), getIntY_vonSkill(shape.bBox.Y), getIntBreite_vonSkill(shape.bBox.Width), getIntHöhe_vonSkill(shape.bBox.Height), fill, fm))
                    Case "ellipse"
                        currentGrafik.add(New Precompiled_Ellipse(getIntX_vonSkill(shape.bBox.X + shape.bBox.Width / 2), getIntY_vonSkill(shape.bBox.Y + shape.bBox.Height / 2), getIntBreite_vonSkill(shape.bBox.Width / 2), getIntHöhe_vonSkill(shape.bBox.Height / 2), fill, fm))
                    Case "polygon"
                        Dim points(2 * shape.points.Count - 1) As Ausdruck_Konstante
                        For i As Integer = 0 To shape.points.Count - 1
                            points(2 * i) = getIntX_vonSkill(shape.points(i).X)
                            points(2 * i + 1) = getIntY_vonSkill(shape.points(i).Y)
                        Next
                        currentGrafik.add(New Precompiled_Poly(points, fill, Not fill, fill))
                    Case "line"
                        Dim points(2 * shape.points.Count - 1) As Ausdruck_Konstante
                        For i As Integer = 0 To shape.points.Count - 1
                            points(2 * i) = getIntX_vonSkill(shape.points(i).X)
                            points(2 * i + 1) = getIntY_vonSkill(shape.points(i).Y)
                        Next
                        If Not fill Then
                            currentGrafik.add(New Precompiled_MultiLinie(points))
                        End If
                    Case "arc"
                        If Not fill Then
                            Dim cx As Ausdruck_Konstante = getIntX_vonSkill(shape.ellipseBBox.X + shape.ellipseBBox.Width / 2)
                            Dim cy As Ausdruck_Konstante = getIntY_vonSkill(shape.ellipseBBox.Y + shape.ellipseBBox.Height / 2)
                            Dim radX As Ausdruck_Konstante = getIntBreite_vonSkill(shape.ellipseBBox.Width / 2)
                            Dim radY As Ausdruck_Konstante = getIntHöhe_vonSkill(shape.ellipseBBox.Height / 2)
                            Dim startwinkel As New Ausdruck_Konstante(CInt(180.0F / Math.PI * shape.startAngle))
                            Dim deltawinkel As New Ausdruck_Konstante(CInt(180.0F / Math.PI * (shape.stopAngle - shape.startAngle)))
                            currentGrafik.add(New Precompiled_Arc(cx, cy, radX, radY, startwinkel, deltawinkel, False, False))
                        End If
                End Select
            End If
        Next

        'Selection setzen
        currentGrafik.add(New Precompiled_Selection(getIntX_vonSkill(master.bBox.X), getIntY_vonSkill(master.bBox.Y), getIntBreite_vonSkill(master.bBox.Width), getIntHöhe_vonSkill(master.bBox.Height)))

        'Pins hinzufügen
        For i As Integer = 0 To master.pins.Count - 1
            addSnappointVonSkill(master, i)
            'setze Origin auf Pin 0
            If i = 0 Then
                my_Precompiled_Template.add(New PrecompiledSetOrigin(getIntX_vonSkill(master.pins(i).pos.X), getIntY_vonSkill(master.pins(i).pos.Y)))
            End If
        Next

        my_Precompiled_Template.add(currentGrafik)

        'Textpositionen setzen
        Dim bBox As RectangleF = master.bBox
        TextposTypes = New List(Of Integer)

        'oben rechts
        addTextposVonSkill(bBox.X + bBox.Width, bBox.Y, 1, -1)
        'oben
        addTextposVonSkill(bBox.X + bBox.Width / 2, bBox.Y, 0, -1)
        'oben links
        addTextposVonSkill(bBox.X, bBox.Y, -1, -1)
        'mitte rechts
        addTextposVonSkill(bBox.X + bBox.Width, bBox.Y + bBox.Height / 2, 1, 0)
        'mitte links
        addTextposVonSkill(bBox.X, bBox.Y + bBox.Height / 2, -1, 0)
        'unten rechts
        addTextposVonSkill(bBox.X + bBox.Width, bBox.Y + bBox.Height, 1, 1)
        'unten mitte
        addTextposVonSkill(bBox.X + bBox.Width / 2, bBox.Y + bBox.Height, 0, 1)
        'unten links
        addTextposVonSkill(bBox.X, bBox.Y + bBox.Height, -1, 1)
        'mitte mitte
        addTextposVonSkill(bBox.X + bBox.Width / 2, bBox.Y + bBox.Height / 2, 0, 0)


    End Sub

    Private Sub addSnappointVonSkill(master As Detektion.MasterElemente, pinNr As Integer)
        Dim x As Ausdruck_Konstante = getIntX_vonSkill(master.pins(pinNr).pos.X)
        Dim y As Ausdruck_Konstante = getIntY_vonSkill(master.pins(pinNr).pos.Y)

        'prüfen in welche Richtung das wire routen darf!
        Dim oben_liegtImWeg As Integer = 0
        Dim unten_liegtImWeg As Integer = 0
        Dim rechts_liegtImWeg As Integer = 0
        Dim links_liegtImWeg As Integer = 0

        Dim pos As PointF = master.pins(pinNr).pos
        For Each shape In master.rawShapes
            If shape.layer = "device" AndAlso shape.purpose = "drawing" Then
                If shape.type = "rect" OrElse shape.type = "ellipse" OrElse shape.type = "polygon" OrElse shape.type = "arc" Then
                    If Skill_liegtImWeg(shape.bBox, pos, New Point(1, 0)) Then rechts_liegtImWeg += 1
                    If Skill_liegtImWeg(shape.bBox, pos, New Point(-1, 0)) Then links_liegtImWeg += 1
                    If Skill_liegtImWeg(shape.bBox, pos, New Point(0, 1)) Then unten_liegtImWeg += 1
                    If Skill_liegtImWeg(shape.bBox, pos, New Point(0, -1)) Then oben_liegtImWeg += 1
                ElseIf shape.type = "line" Then
                    For l As Integer = 0 To shape.points.Count - 2
                        Dim start As PointF = shape.points(l)
                        Dim ende As PointF = shape.points(l + 1)
                        Dim bbox As New RectangleF(Math.Min(start.X, ende.X), Math.Min(start.Y, ende.Y), Math.Max(start.X, ende.X) - Math.Min(start.X, ende.X), Math.Max(start.Y, ende.Y) - Math.Min(start.Y, ende.Y))
                        If Skill_liegtImWeg(bbox, pos, New Point(1, 0)) Then rechts_liegtImWeg += 1
                        If Skill_liegtImWeg(bbox, pos, New Point(-1, 0)) Then links_liegtImWeg += 1
                        If Skill_liegtImWeg(bbox, pos, New Point(0, 1)) Then unten_liegtImWeg += 1
                        If Skill_liegtImWeg(bbox, pos, New Point(0, -1)) Then oben_liegtImWeg += 1
                    Next
                End If
            End If
        Next

        Dim links As Integer = 0
        Dim rechts As Integer = 0
        Dim oben As Integer = 0
        Dim unten As Integer = 0
        If links_liegtImWeg > 0 Then links = PENALTY2
        If rechts_liegtImWeg > 0 Then rechts = PENALTY2
        If oben_liegtImWeg > 0 Then oben = PENALTY2
        If unten_liegtImWeg > 0 Then unten = PENALTY2

        currentGrafik.add(New Precompiled_Snappoint(x, y, New Ausdruck_Konstante(links), New Ausdruck_Konstante(oben), New Ausdruck_Konstante(rechts), New Ausdruck_Konstante(unten)))
    End Sub

    Private Function Skill_liegtImWeg(r As RectangleF, pos As PointF, richtung As Point) As Boolean
        If richtung.X = 0 Then
            If pos.X < r.X Then Return False
            If pos.X > r.X + r.Width Then Return False
            If pos.Y < r.Y + r.Height AndAlso richtung.Y > 0 Then Return True
            If pos.Y > r.Y AndAlso richtung.Y < 0 Then Return True
            Return False
        ElseIf richtung.Y = 0 Then
            If pos.Y < r.Y Then Return False
            If pos.Y > r.Y + r.Height Then Return False
            If pos.X < r.X + r.Width AndAlso richtung.X > 0 Then Return True
            If pos.X > r.X AndAlso richtung.X < 0 Then Return True
            Return False
        Else
            Throw New NotImplementedException("Diese Funktion ist nur für rechtwinklige Richtungen implementiert")
        End If
    End Function

    Private Sub addTextposVonSkill(x As Single, y As Single, dx As Integer, dy As Integer)
        currentTextpos = New PrecompiledTextpos()
        currentTextpos.setPos(getIntX_vonSkill(x), getIntY_vonSkill(y))
        currentTextpos.setVector(New Ausdruck_Konstante(dx), New Ausdruck_Konstante(dy))
        If dx = 0 AndAlso dy = 0 Then
            currentTextpos.hasAbstandVektor = True
            currentTextpos.setDist(New Ausdruck_Konstante(1), New Ausdruck_Konstante(0))
        Else
            currentTextpos.hasAbstandVektor = False 'dist = vector!!
        End If
        my_Precompiled_Template.add(currentTextpos)
        TextposTypes.Add(currentTextpos.nr)
    End Sub

    Private Function getIntX_vonSkill(posX_skill As Double) As Ausdruck_Konstante
        Dim x As Double = posX_skill * 1600
        Return New Ausdruck_Konstante(CLng(x))
    End Function
    Private Function getIntY_vonSkill(posY_skill As Double) As Ausdruck_Konstante
        Dim y As Double = posY_skill * 1600
        Return New Ausdruck_Konstante(CLng(y))
    End Function
    Private Function getIntBreite_vonSkill(breite_skill As Double) As Ausdruck_Konstante
        Dim w As Double = breite_skill * 1600
        Return New Ausdruck_Konstante(CLng(w))
    End Function
    Private Function getIntHöhe_vonSkill(höhe_skill As Double) As Ausdruck_Konstante
        Dim h As Double = höhe_skill * 1600
        Return New Ausdruck_Konstante(CLng(h))
    End Function
#End Region

#Region "Kompilieren"
    Public Sub kompilieren(pfad As String)
        Dim reader As New Stack(Of Tuple(Of FileInfo, StreamReader))
        Dim zeilenNr As New Stack(Of Integer)

        my_Precompiled_Template = New Precompiled_Template()
        DefaultName = "?"

        _view = "eu"

        konstanten_CurrentBlock = New Stack(Of Dictionary(Of String, Integer))
        konstanten_CurrentBlock.Push(New Dictionary(Of String, Integer))
        konstanten_CurrentBlock.Peek().Add("#alignhead", 0)
        konstanten_CurrentBlock.Peek().Add("#aligncenter", 1)
        konstanten_CurrentBlock.Peek().Add("#penalty0", PENALTY0)
        konstanten_CurrentBlock.Peek().Add("#penalty1", PENALTY1)
        konstanten_CurrentBlock.Peek().Add("#penalty2", PENALTY2)
        konstanten_CurrentBlock.Peek().Add("#inf", Integer.MaxValue)

        parameter_intern = New List(Of ParamName)
        vars_intern = New List(Of VariableEinlesen)

        parameter_intern.Add(New ParamName(ParamName.paramArt.Int, "#textpos"))

        Me.MitNummer = True
        Me.isDeko = False
        Dim firstLine As Boolean = True

        TextposTypes = New List(Of Integer)

        Me.templatesCompatibility = New List(Of TemplateCompatibility)

        parameter = New List(Of TemplateParameter)
        Dim nameKopf As String = New FileInfo(pfad).Name

        Try
            reader.Push(New Tuple(Of FileInfo, StreamReader)(New FileInfo(pfad), New StreamReader(pfad, Text.Encoding.UTF8)))
            zeilenNr.Push(0)

            Dim mode As EinleseModus = EinleseModus.IDLE
            While reader.Count >= 1
                While reader.Count >= 1 AndAlso reader.Peek().Item2.EndOfStream()
                    reader.Peek().Item2.Close()
                    reader.Pop()
                    zeilenNr.Pop()
                End While
                If reader.Count = 0 Then
                    Exit While
                End If

                Dim line As String = reader.Peek().Item2.ReadLine()
                zeilenNr.Push(zeilenNr.Pop() + 1)

                If Not line.TrimStart().StartsWith("%") Then
                    'suche nach Kommentar
                    line = entferneKommentar(line)
                    Dim lineLarge As String = line.Trim()
                    line = Mathe.strToLower(lineLarge)
                    If line <> "" Then

                        If firstLine Then
                            firstLine = False
                            If line = "use_as_child" Then
                                Me.isDeko = True
                                'parameter_intern.Add("parent.width")
                                'parameter_intern.Add("parent.height")
                                CompileParentArgs.addChildParameter(parameter_intern)
                                Erster_Echter_Paramter = parameter_intern.Count
                                Continue While
                            ElseIf line = "use_as_parent" Then
                                Me.isDeko = False
                                Erster_Echter_Paramter = parameter_intern.Count
                                Continue While
                            Else
                                Erster_Echter_Paramter = parameter_intern.Count
                            End If
                        End If

                        If line.StartsWith("input(") AndAlso line.EndsWith(")") Then
                            Dim pfad1 As String = line.Substring(6)
                            pfad1 = pfad1.Substring(0, pfad1.Length - 1).Trim()
                            pfad1 = reader.Peek().Item1.DirectoryName & "\" & pfad1
                            If Not File.Exists(pfad1) Then
                                Throw New Exception("Die Datei '" & pfad1 & "' existiert nicht!")
                            End If
                            reader.Push(New Tuple(Of FileInfo, StreamReader)(New FileInfo(pfad1), New StreamReader(pfad1, Text.Encoding.UTF8)))
                            zeilenNr.Push(0)
                        Else
                            If line.StartsWith("def ") OrElse line.StartsWith("defvar ") Then
                                If mode = EinleseModus.LeseScaling Then
                                    Throw New Exception("Definition von Konstanten ist innerhalb eines scaling blocks nicht möglich")
                                End If
                                'definiere Konstante
                                definiereKonstanteGesamt(line, Me.konstanten_CurrentBlock.Peek(), mode)
                            ElseIf line.StartsWith("var ") Then
                                If mode = EinleseModus.LeseScaling Then
                                    Throw New Exception("Definition von Variablen ist innerhalb eines scaling blocks nicht möglich")
                                End If
                                definiereVariable(line, mode)
                            Else
                                Select Case mode
                                    Case EinleseModus.IDLE
                                        If line = "drawing:" OrElse line = "drawing" Then
                                            mode = EinleseModus.LeseGrafik
                                            currentGrafik = New Precompiled_MultiGrafik(vars_intern.Count)
                                            begingroup()
                                        ElseIf line = "info:" OrElse line = "info" Then
                                            mode = EinleseModus.LeseInfo
                                            begingroup()
                                        ElseIf line = "textpos:" OrElse line = "textpos" Then
                                            mode = EinleseModus.LeseTextPos
                                            currentTextpos = New PrecompiledTextpos()
                                            begingroup()
                                        ElseIf line = "compatibility:" OrElse line = "compatibility" Then
                                            mode = EinleseModus.Compatibility
                                            currentTemplateCompatibility = New TemplateCompatibility(Me)
                                            begingroup()
                                        ElseIf line.StartsWith("scaling ") Then
                                            mode = EinleseModus.LeseScaling
                                            leseScalingStart(line.Substring(7).Trim())
                                        Else
                                            readIDLE(line, lineLarge)
                                        End If
                                    Case EinleseModus.LeseGrafik
                                        If line = "drawing end" Then
                                            currentGrafik.add_End()
                                            For i As Integer = currentGrafik.getVarCountAtStart() To vars_intern.Count - 1
                                                vars_intern(i) = New VariableEinlesen(vars_intern(i).art, "") 'ab jetzt kann man nichtmehr darauf zugreifen!
                                            Next
                                            If Not currentGrafik.fertig() Then
                                                Throw New Exception("'end' erwartet vor 'drawing end'")
                                            End If
                                            Dim g_simplified As Precompiled_Grafik = currentGrafik.simplifyBlock()
                                            If g_simplified IsNot Nothing Then
                                                my_Precompiled_Template.add(DirectCast(g_simplified, Precompiled_MultiGrafik))
                                            End If
                                            mode = EinleseModus.IDLE
                                            endgroup()
                                        Else
                                            leseGrafik(line, lineLarge)
                                        End If
                                    Case EinleseModus.LeseInfo
                                        If line = "info end" Then
                                            mode = EinleseModus.IDLE
                                            endgroup()
                                        Else
                                            leseInfo(line, lineLarge)
                                        End If
                                    Case EinleseModus.LeseTextPos
                                        If line = "textpos end" Then
                                            mode = EinleseModus.IDLE
                                            my_Precompiled_Template.add(currentTextpos)
                                            TextposTypes.Add(currentTextpos.nr)
                                            endgroup()
                                        Else
                                            leseTextPos(line)
                                        End If
                                    Case EinleseModus.Compatibility
                                        If line = "compatibility end" Then
                                            mode = EinleseModus.IDLE
                                            currentTemplateCompatibility.einladenFertig()
                                            Me.templatesCompatibility.Add(currentTemplateCompatibility)
                                            endgroup()
                                        Else
                                            leseCompatibilit(line, lineLarge)
                                        End If
                                    Case EinleseModus.LeseScaling
                                        If line = "scaling end" Then
                                            mode = EinleseModus.IDLE
                                            my_Precompiled_Template.add_Scaling(currentPrecompiledScaling)
                                            For i As Integer = currentPrecompiledScaling.varNr_Scale To vars_intern.Count - 1
                                                vars_intern(i) = New VariableEinlesen(vars_intern(i).art, "") 'Ab jetzt kann man nicht mehr darauf zugreifen
                                            Next
                                        Else
                                            leseScaling(line, lineLarge)
                                        End If
                                End Select
                            End If
                        End If
                    End If
                End If
            End While

        Catch ex As Exception
            If reader IsNot Nothing AndAlso reader.Count > 0 Then
                Dim fehlerMsg As String = ex.Message
                Dim fehlerOrt As String = ""
                While zeilenNr.Count > reader.Count
                    zeilenNr.Pop()
                End While

                If reader.Count = zeilenNr.Count Then
                    While reader.Count > 0
                        Dim reader1 = reader.Pop()
                        Dim nr As Integer = zeilenNr.Pop()
                        If reader1 IsNot Nothing AndAlso reader1.Item2 IsNot Nothing Then
                            reader1.Item2.Close()
                            If fehlerOrt.Length > 0 Then
                                fehlerOrt &= vbCrLf
                            End If
                            fehlerOrt &= "Zeile " & nr & " in Datei " & reader1.Item1.Name & " (" & reader1.Item1.FullName & ")"
                        End If
                    End While
                Else
                    fehlerOrt = "In Datei " & nameKopf
                End If
                'Throw New Exception(fehlerMsg)
                Throw New CompileException(fehlerMsg, fehlerOrt)
            Else
                Throw New CompileException("Allgemeiner Fehler beim Übersetzen: " & ex.Message, "In Datei: " & nameKopf)
            End If
        Finally
            If reader IsNot Nothing Then
                While reader.Count > 0
                    If reader.Peek() IsNot Nothing Then
                        reader.Pop().Item2.Close()
                    End If
                End While
            End If
        End Try

        ReDim defaulParameterValues(parameter.Count - 1)
        reload_defaultParameterValues()

        'Aufräumen
        konstanten_CurrentBlock.Clear()
        konstanten_CurrentBlock = Nothing
        parameter_intern.Clear()
        parameter_intern = Nothing
        AnzahlVarsIntern = vars_intern.Count
        vars_intern.Clear()
        vars_intern = Nothing
    End Sub

    Private Sub begingroup()
        Dim newKonst As New Dictionary(Of String, Integer)
        Dim oldKonst As Dictionary(Of String, Integer) = konstanten_CurrentBlock.Peek()
        For i As Integer = 0 To oldKonst.Count - 1
            newKonst.Add(oldKonst.Keys(i), oldKonst.Values(i))
        Next
        konstanten_CurrentBlock.Push(newKonst)
    End Sub

    Private Sub endgroup()
        konstanten_CurrentBlock.Peek().Clear() 'löschen
        konstanten_CurrentBlock.Pop()
    End Sub

    Private Sub definiereKonstanteGesamt(line As String, konstantenListe As Dictionary(Of String, Integer), einlese_Modus As EinleseModus)
        Dim defVar As Boolean = False
        If line.StartsWith("def ") Then
            line = line.Substring(4)
            defVar = False
        ElseIf line.StartsWith("defvar ") Then
            line = line.Substring(7)
            defVar = True
        Else
            Throw New Exception("Fehler bei der Definition der Konstante")
        End If
        Dim aktuelleVar As New StringBuilder()
        Dim aktuellerWert As New StringBuilder()
        Dim mode As Integer = 0
        Dim inAnführungszeichen As Boolean = False
        Dim klammerpos As Integer = 0
        For i As Integer = 0 To line.Length - 1
            If line(i) = """" Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                If line(i) = "(" Then
                    klammerpos += 1
                ElseIf line(i) = ")" Then
                    klammerpos -= 1
                End If
            End If
            If klammerpos < 0 Then
                Throw New Exception("Zu viele ')'.")
            End If
            If inAnführungszeichen OrElse klammerpos > 0 Then
                Select Case mode
                    Case 0
                        aktuelleVar.Append(line(i))
                    Case 1
                        aktuelleVar.Append(line(i))
                    Case 2
                        Throw New Exception("'=' erwartet")
                    Case 3
                        aktuellerWert.Append(line(i))
                    Case 4
                        aktuellerWert.Append(line(i))
                    Case 5
                        Throw New Exception("',' erwartet")
                End Select
            Else
                Select Case mode
                    Case 0 'IDLE
                        If line(i) <> " "c Then
                            aktuelleVar.Append(line(i))
                            mode = 1
                        End If
                    Case 1 'Var Einlesen
                        If line(i) = "="c Then
                            mode = 3
                        ElseIf line(i) = " "c Then
                            mode = 2
                        Else
                            aktuelleVar.Append(line(i))
                        End If
                    Case 2 'Warten auf =
                        If line(i) = "="c Then
                            mode = 3
                        ElseIf line(i) <> " "c Then
                            Throw New Exception("'=' erwartet")
                        End If
                    Case 3 'Warten auf Wert
                        If line(i) <> " "c Then
                            aktuellerWert.Append(line(i))
                            mode = 4
                        End If
                    Case 4 'Wert einlesen
                        If line(i) = "," Then
                            defKonst(aktuelleVar.ToString(), aktuellerWert.ToString(), konstantenListe, einlese_Modus, defVar)
                            aktuellerWert.Clear()
                            aktuelleVar.Clear()
                            mode = 0
                            'ElseIf line(i) = " "c Then
                            'mode = 5
                        Else
                            aktuellerWert.Append(line(i))
                        End If
                    Case 5 'Warten auf ","
                        If line(i) = "," Then
                            defKonst(aktuelleVar.ToString(), aktuellerWert.ToString(), konstantenListe, einlese_Modus, defVar)
                            aktuellerWert.Clear()
                            aktuelleVar.Clear()
                            mode = 0
                        ElseIf line(i) <> " "c Then
                            Throw New Exception("',' erwartet")
                        End If
                End Select
            End If
        Next
        If mode >= 4 Then
            defKonst(aktuelleVar.ToString(), aktuellerWert.ToString(), konstantenListe, einlese_Modus, defVar)
        End If
    End Sub

    Private Sub definiereVariable(line As String, myMode As EinleseModus)
        line = line.Substring(4)

        line = line.TrimStart()
        Dim art As VariableEinlesen.VariableArt
        If line.StartsWith("int ") Then
            art = VariableEinlesen.VariableArt.Int_
            line = line.Substring(4)
        ElseIf line.StartsWith("string ") Then
            art = VariableEinlesen.VariableArt.String_
            line = line.Substring(7)
        ElseIf line.StartsWith("boolean ") Then
            art = VariableEinlesen.VariableArt.Boolean_
            line = line.Substring(8)
        Else
            art = VariableEinlesen.VariableArt.Int_
        End If

        Dim aktuelleVar As New StringBuilder()
        Dim aktuellerWert As New StringBuilder()
        Dim mode As Integer = 0
        Dim inAnführungszeichen As Boolean = False
        Dim klammerpos As Integer = 0
        For i As Integer = 0 To line.Length - 1
            If line(i) = """" Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                If line(i) = "(" Then
                    klammerpos += 1
                ElseIf line(i) = ")" Then
                    klammerpos -= 1
                End If
            End If
            If klammerpos < 0 Then
                Throw New Exception("Zu viele ')'.")
            End If
            If inAnführungszeichen OrElse klammerpos > 0 Then
                Select Case mode
                    Case 0
                        aktuelleVar.Append(line(i))
                    Case 1
                        aktuelleVar.Append(line(i))
                    Case 2
                        Throw New Exception("'=' erwartet")
                    Case 3
                        aktuellerWert.Append(line(i))
                    Case 4
                        aktuellerWert.Append(line(i))
                    Case 5
                        Throw New Exception("',' erwartet")
                End Select
            Else
                Select Case mode
                    Case 0 'IDLE
                        If line(i) <> " "c Then
                            aktuelleVar.Append(line(i))
                            mode = 1
                        End If
                    Case 1 'Var Einlesen
                        If line(i) = "="c Then
                            mode = 3
                        ElseIf line(i) = " "c Then
                            mode = 2
                        Else
                            aktuelleVar.Append(line(i))
                        End If
                    Case 2 'Warten auf =
                        If line(i) = "="c Then
                            mode = 3
                        ElseIf line(i) <> " "c Then
                            Throw New Exception("'=' erwartet")
                        End If
                    Case 3 'Warten auf Wert
                        If line(i) <> " "c Then
                            aktuellerWert.Append(line(i))
                            mode = 4
                        End If
                    Case 4 'Wert einlesen
                        If line(i) = "," Then
                            defVar(aktuelleVar.ToString(), aktuellerWert.ToString(), myMode, art)
                            aktuellerWert.Clear()
                            aktuelleVar.Clear()
                            mode = 0
                            'ElseIf line(i) = " "c Then
                            'mode = 5
                        Else
                            aktuellerWert.Append(line(i))
                        End If
                    Case 5 'Warten auf ","
                        If line(i) = "," Then
                            defVar(aktuelleVar.ToString(), aktuellerWert.ToString(), myMode, art)
                            aktuellerWert.Clear()
                            aktuelleVar.Clear()
                            mode = 0
                        ElseIf line(i) <> " "c Then
                            Throw New Exception("',' erwartet")
                        End If
                End Select
            End If
        Next
        If mode >= 4 Then
            defVar(aktuelleVar.ToString(), aktuellerWert.ToString(), myMode, art)
        End If
    End Sub

    Private Sub readIDLE(line As String, lineLarge As String)
        If line.StartsWith("param_int ") Then
            lineLarge = lineLarge.Substring(10).Trim()

            lineLarge &= " "
            line &= " " 'Ein Leerzeichen am Ende um eine einfachere Detektion von Parametern zu haben

            Dim mode As Integer = 0
            Dim name As String = ""
            Dim defaultVal As String = ""
            Dim intervalStart_geschlossen As Boolean = False
            Dim intervalEnde_geschlossen As Boolean = False
            Dim intervalString As String = ""
            Dim unit As String = ""
            Dim interval_klammerzähler As Integer = 0
            Dim outOfRangeMode As Intervall.OutOfRangeMode = Intervall.OutOfRangeMode.ClipToBounds
            Dim hasClipOrMod As Boolean = False
            Dim hasUnit As Boolean = False
            Dim hasFrom As Boolean = False
            Dim hat_str As Boolean = False
            For i As Integer = 0 To lineLarge.Length - 1
                Select Case mode
                    Case 0
                        If lineLarge(i) = """"c Then
                            mode = 1
                        ElseIf lineLarge(i) = "["c Then
                            mode = 101
                            name = "["
                            hat_str = False
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 1
                        If lineLarge(i) = """"c Then
                            mode = 2
                        Else
                            name &= lineLarge(i)
                        End If
                    Case 2
                        If lineLarge(i) = "="c Then
                            mode = 3
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'=' erwartet!")
                        End If
                    Case 3
                        If i <= lineLarge.Length - 6 AndAlso lineLarge.Substring(i, 6) = " from " Then
                            If hasFrom Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'from' ist zu viel.")
                            End If
                            mode = 4
                            i += 4
                        ElseIf i <= lineLarge.Length - 6 AndAlso lineLarge.Substring(i, 6) = " unit " Then
                            If hasUnit Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'unit' ist zu viel.")
                            End If
                            mode = 7
                            i += 4
                        ElseIf i <= lineLarge.Length - 6 AndAlso lineLarge.Substring(i, 6) = " clip " Then
                            If hasClipOrMod Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'clip' ist zu viel.")
                            End If
                            mode = 3
                            i += 4
                            hasClipOrMod = True
                            outOfRangeMode = Intervall.OutOfRangeMode.ClipToBounds
                        ElseIf i <= lineLarge.Length - 5 AndAlso lineLarge.Substring(i, 5) = " mod " Then
                            If hasClipOrMod Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'mod' ist zu viel.")
                            End If
                            mode = 3
                            i += 3
                            hasClipOrMod = True
                            outOfRangeMode = Intervall.OutOfRangeMode.Modulo
                        Else
                            defaultVal &= lineLarge(i)
                        End If
                    Case 4
                        If lineLarge(i) = "(" Then
                            intervalStart_geschlossen = False
                            mode = 5
                        ElseIf lineLarge(i) = "[" Then
                            intervalStart_geschlossen = True
                            mode = 5
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'(' oder '[' erwartet!")
                        End If
                    Case 5
                        If lineLarge(i) = "(" Then
                            interval_klammerzähler += 1
                        End If
                        If lineLarge(i) = ")" Then
                            interval_klammerzähler -= 1
                        End If
                        If interval_klammerzähler = -1 OrElse (interval_klammerzähler = 0 AndAlso lineLarge(i) = "]") Then
                            hasFrom = True
                            mode = 3
                        End If
                        intervalString &= lineLarge(i)
                    Case 7
                        If lineLarge(i) = """"c Then
                            mode = 8
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet")
                        End If
                    Case 8
                        If lineLarge(i) = """"c Then
                            mode = 3
                            hasUnit = True
                        Else
                            unit &= lineLarge(i)
                        End If
                    Case 101
                        name &= lineLarge(i)
                        If lineLarge(i) = """"c Then
                            hat_str = Not hat_str
                        ElseIf lineLarge(i) = "]"c And Not hat_str Then
                            mode = 2
                        End If
                End Select
            Next
            If mode <> 3 Then
                Throw New Exception("Ungültige Definition eines Parameters bei '" & lineLarge & "'")
            End If
            Dim myIntervall As Intervall
            If hasFrom Then
                If intervalString = "" Then
                    myIntervall = New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, outOfRangeMode)
                Else
                    intervalString = Mathe.strToLower(intervalString)
                    If intervalString.EndsWith(")") Then
                        intervalEnde_geschlossen = False
                    ElseIf intervalString.EndsWith("]") Then
                        intervalEnde_geschlossen = True
                    Else
                        Throw New Exception("')' oder ']' erwartet!")
                    End If
                    intervalString = intervalString.Substring(0, intervalString.Length - 1)

                    Dim values() As Ausdruck = Mathe.readSeperatedAusdrücke(intervalString, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
                    If values.Length = 2 Then
                        If TypeOf values(0) IsNot Ausdruck_Konstante Then
                            Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ").")
                        End If
                        If TypeOf values(1) IsNot Ausdruck_Konstante Then
                            Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ").")
                        End If
                        Dim _start As Integer = CInt(DirectCast(values(0), Ausdruck_Konstante).Ausrechnen(Nothing))
                        Dim _ende As Integer = CInt(DirectCast(values(1), Ausdruck_Konstante).Ausrechnen(Nothing))
                        Dim _step As Integer = 1
                        If istIntervalUngültig(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen) Then
                            Throw New Exception("Ungültiges (leeres) Interval bei '" & lineLarge & "'")
                        End If
                        myIntervall = New Intervall(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen, outOfRangeMode)
                    ElseIf values.Length = 3 Then
                        If TypeOf values(0) IsNot Ausdruck_Konstante Then
                            Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
                        End If
                        If TypeOf values(1) IsNot Ausdruck_Konstante Then
                            Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
                        End If
                        If TypeOf values(2) IsNot Ausdruck_Konstante Then
                            Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
                        End If

                        Dim _start As Integer = CInt(DirectCast(values(0), Ausdruck_Konstante).Ausrechnen(Nothing))
                        Dim _step As Integer = CInt(DirectCast(values(1), Ausdruck_Konstante).Ausrechnen(Nothing))
                        If _step < 1 Then
                            Throw New Exception("Die Schrittweite muss >=1 sein (Bei " & lineLarge & ")")
                        End If
                        Dim _ende As Integer = CInt(DirectCast(values(2), Ausdruck_Konstante).Ausrechnen(Nothing))
                        If istIntervalUngültig(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen) Then
                            Throw New Exception("Ungültiges (leeres) Interval bei '" & lineLarge & "'")
                        End If
                        myIntervall = New Intervall(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen, outOfRangeMode)
                    Else
                        Throw New Exception("Falsche Definition des Intervals bei '" & lineLarge & "'")
                    End If
                End If
            Else
                'hat nur defaultval (param_int "value1"=10)
                myIntervall = New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, outOfRangeMode)
                If hasClipOrMod Then
                    Throw New Exception("Die Angabe der Einschränkung 'clip' oder 'mod' kann nur verwendet werden, wenn ein Interval angegeben wird.")
                End If
            End If
            Dim valueDefault As Ausdruck = Ausdruck.EinlesenAusdruck(defaultVal, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
            If TypeOf valueDefault IsNot Ausdruck_Konstante Then
                Throw New Exception("Der Default-Wert muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
            End If
            Dim name_str As New Multi_Lang_String(name)
            parameter.Add(New TemplateParameter_Int(name_str, myIntervall, CInt(DirectCast(valueDefault, Ausdruck_Konstante).Ausrechnen(Nothing)), unit))
            parameter_intern.Add(New ParamName(ParamName.paramArt.Int, name_str.get_ID()))
        ElseIf line.StartsWith("param_arrow ") Then
            lineLarge = lineLarge.Substring(12).Trim()

            Dim mode As Integer = 0
            Dim name As String = ""
            Dim defaultVal As String = ""
            Dim intervalStart_geschlossen As Boolean = False
            Dim intervalEnde_geschlossen As Boolean = False
            Dim intervalString As String = ""
            Dim hatFrom As Boolean = False
            Dim hasClipOrMod As Boolean = False
            Dim outOfBounds As Intervall.OutOfRangeMode = Intervall.OutOfRangeMode.ClipToBounds
            Dim hat_str As Boolean = False
            For i As Integer = 0 To lineLarge.Length - 1
                Select Case mode
                    Case 0
                        If lineLarge(i) = """"c Then
                            mode = 1
                        ElseIf lineLarge(i) = "["c Then
                            mode = 101
                            name = "["
                            hat_str = False
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 1
                        If lineLarge(i) = """"c Then
                            mode = 2
                        Else
                            name &= lineLarge(i)
                        End If
                    Case 2
                        If lineLarge(i) = "="c Then
                            mode = 3
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'=' erwartet!")
                        End If
                    Case 3
                        If i <= lineLarge.Length - 6 AndAlso lineLarge.Substring(i, 6) = " from " Then
                            If hatFrom Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'from' ist zu viel.")
                            End If
                            mode = 4
                            i += 4
                        ElseIf i <= lineLarge.Length - 6 AndAlso lineLarge.Substring(i, 6) = " clip " Then
                            If hasClipOrMod Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'clip' ist zu viel.")
                            End If
                            mode = 3
                            i += 4
                            hasClipOrMod = True
                            outOfBounds = Intervall.OutOfRangeMode.ClipToBounds
                        ElseIf i <= lineLarge.Length - 5 AndAlso lineLarge.Substring(i, 5) = " mod " Then
                            If hasClipOrMod Then
                                Throw New Exception("Zu viele Einschränkungen für diesen Parameter, 'mod' ist zu viel.")
                            End If
                            mode = 3
                            i += 3
                            hasClipOrMod = True
                            outOfBounds = Intervall.OutOfRangeMode.Modulo
                        Else
                            defaultVal &= lineLarge(i)
                        End If
                    Case 4
                        If lineLarge(i) = "(" Then
                            intervalStart_geschlossen = False
                            mode = 5
                        ElseIf lineLarge(i) = "[" Then
                            intervalStart_geschlossen = True
                            mode = 5
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'(' oder '[' erwartet!")
                        End If
                    Case 5
                        intervalString &= lineLarge(i)
                        If lineLarge(i) = ")" OrElse lineLarge(i) = "]" Then
                            mode = 3
                            hatFrom = True
                        End If
                    Case 101
                        name &= lineLarge(i)
                        If lineLarge(i) = """"c Then
                            hat_str = Not hat_str
                        ElseIf lineLarge(i) = "]"c And Not hat_str Then
                            mode = 2
                        End If
                End Select
            Next
            If mode <> 3 Then
                Throw New Exception("Ungültige Definition eines Parameters bei '" & lineLarge & "'")
            End If

            Dim myIntervall As Intervall
            If hatFrom Then
                intervalString = Mathe.strToLower(intervalString)
                If intervalString.EndsWith(")") Then
                    intervalEnde_geschlossen = False
                ElseIf intervalString.EndsWith("]") Then
                    intervalEnde_geschlossen = True
                Else
                    Throw New Exception("')' oder ']' erwartet!")
                End If
                intervalString = intervalString.Substring(0, intervalString.Length - 1)

                Dim values() As Ausdruck = Mathe.readSeperatedAusdrücke(intervalString, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
                If values.Length = 2 Then
                    If TypeOf values(0) IsNot Ausdruck_Konstante Then
                        Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
                    End If
                    If TypeOf values(1) IsNot Ausdruck_Konstante Then
                        Throw New Exception("Die Definition des Intervalls muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
                    End If
                    Dim _start As Integer = CInt(DirectCast(values(0), Ausdruck_Konstante).Ausrechnen(Nothing))
                    Dim _ende As Integer = CInt(DirectCast(values(1), Ausdruck_Konstante).Ausrechnen(Nothing))
                    Dim _step As Integer = 1
                    If istIntervalUngültig(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen) Then
                        Throw New Exception("Ungültiges (leeres) Interval bei '" & lineLarge & "'")
                    End If
                    myIntervall = New Intervall(_start, _ende, _step, intervalStart_geschlossen, intervalEnde_geschlossen, outOfBounds)
                Else
                    Throw New Exception("Falsche Definition des Intervals bei '" & lineLarge & "'")
                End If
            Else
                'hat nur defaultval (param_arrow "value1"=10)
                myIntervall = New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, outOfBounds)
            End If

            Dim valueDefault As Ausdruck = Ausdruck.EinlesenAusdruck(defaultVal, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
            If TypeOf valueDefault IsNot Ausdruck_Konstante Then
                Throw New Exception("Der Default-Wert muss ein Integer sein und darf nicht von Variablen abhängen (Bei " & lineLarge & ")")
            End If

            Dim name_str As New Multi_Lang_String(name)
            parameter.Add(New TemplateParameter_Arrow(name_str, New Intervall(myIntervall.min, myIntervall.max, 1, intervalStart_geschlossen, intervalEnde_geschlossen, myIntervall.outOfRange), CInt(DirectCast(valueDefault, Ausdruck_Konstante).Ausrechnen(Nothing))))
            parameter_intern.Add(New ParamName(ParamName.paramArt.Arrow, name_str.get_ID()))

        ElseIf line.StartsWith("param ") Then
            lineLarge = lineLarge.Substring(6).Trim()
            Dim mode As Integer = 0
            Dim name As String = ""
            Dim neueOption As String = ""
            Dim optionen As New List(Of String)()
            Dim oldOptionStr As String = ""
            Dim hat_str As Boolean = False
            For i As Integer = 0 To lineLarge.Length - 1
                Select Case mode
                    Case 0
                        If lineLarge(i) = """"c Then
                            mode = 1
                        ElseIf lineLarge(i) = "["c Then
                            mode = 101
                            name = "["
                            hat_str = False
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 1
                        If lineLarge(i) = """"c Then
                            mode = 2
                        Else
                            name &= lineLarge(i)
                        End If
                    Case 2
                        If lineLarge(i) = "="c Then
                            mode = 3
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'=' erwartet!")
                        End If
                    Case 3
                        If lineLarge(i) = "{" Then
                            mode = 4
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'{' erwartet!")
                        End If
                    Case 4
                        If lineLarge(i) = """" Then
                            mode = 5
                        ElseIf lineLarge(i) = "[" Then
                            mode = 105
                            hat_str = False
                            neueOption = "["
                        ElseIf lineLarge(i) = "}" Then
                            mode = 6
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 5
                        If lineLarge(i) = """" Then
                            mode = 4
                            optionen.Add(neueOption)
                            neueOption = ""
                        Else
                            neueOption &= lineLarge(i)
                        End If
                    Case 6
                        If i < lineLarge.Length - 5 AndAlso lineLarge.Substring(i, 5) = " old " Then
                            mode = 7
                            i += 4
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("Zeilenende erwartet.")
                        End If
                    Case 7
                        If lineLarge(i) = """"c Then
                            mode = 8
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'""' erwartet.")
                        End If
                    Case 8
                        If lineLarge(i) = """"c Then
                            mode = 9
                        Else
                            oldOptionStr &= lineLarge(i)
                        End If
                    Case 9
                        If lineLarge(i) <> " "c Then
                            Throw New Exception("Zeilenende erwartet.")
                        End If
                    Case 100
                        If lineLarge(i) = """"c Then
                            mode = 101
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 101
                        name &= lineLarge(i)
                        If lineLarge(i) = """"c Then
                            hat_str = Not hat_str
                        ElseIf lineLarge(i) = "]"c And Not hat_str Then
                            mode = 2
                        End If
                    Case 105
                        neueOption &= lineLarge(i)
                        If lineLarge(i) = """"c Then
                            hat_str = Not hat_str
                        ElseIf lineLarge(i) = "]"c And Not hat_str Then
                            mode = 4
                            optionen.Add(neueOption)
                            neueOption = ""
                        End If
                End Select
            Next
            If Not (mode = 6 OrElse mode = 9) Then
                Throw New Exception("Ungültige Definition eines Parameters bei '" & lineLarge & "'")
            End If
            Dim optionen_parse(optionen.Count - 1) As Multi_Lang_String
            For i As Integer = 0 To optionen.Count - 1
                optionen_parse(i) = New Multi_Lang_String(optionen(i))
            Next
            Dim oldoption As Integer = -1
            If mode = 9 Then
                For i As Integer = 0 To optionen_parse.Length - 1
                    If oldOptionStr = optionen_parse(i).get_ID() Then
                        oldoption = i
                    End If
                Next
                If oldoption = -1 Then
                    Throw New Exception("Die Option '" & oldOptionStr & "' gibt es nicht.")
                End If
            End If
            Dim name_str As New Multi_Lang_String(name)
            parameter.Add(New TemplateParameter_Param(name_str, optionen_parse, oldoption))
            parameter_intern.Add(New ParamName(ParamName.paramArt.ParamListe, name_str.get_ID())) 'dieser Parameter blockiert eine Position in der Liste. Aber mit dem "@param#" wird sichergestellt, dass er nicht als param_int oder param_var überschrieben wird!
        ElseIf line.StartsWith("param_string ") Then
            lineLarge = lineLarge.Substring(13).Trim()

            Dim mode As Integer = 0
            Dim name As String = ""
            Dim defaultVal As String = ""
            Dim fromVal As String = ""
            Dim hat_str As Boolean = False
            For i As Integer = 0 To lineLarge.Length - 1
                Select Case mode
                    Case 0
                        If lineLarge(i) = """"c Then
                            mode = 1
                        ElseIf lineLarge(i) = "["c Then
                            mode = 101
                            name = "["
                            hat_str = False
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 1
                        If lineLarge(i) = """"c Then
                            mode = 2
                        Else
                            name &= lineLarge(i)
                        End If
                    Case 2
                        If lineLarge(i) = "="c Then
                            mode = 3
                        ElseIf lineLarge(i) <> " " Then
                            Throw New Exception("'=' erwartet!")
                        End If
                    Case 3
                        If lineLarge(i) = """"c Then
                            mode = 4
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 4
                        If lineLarge(i) = """"c Then
                            mode = 5
                        Else
                            defaultVal &= lineLarge(i)
                        End If
                    Case 5
                        If lineLarge(i) = "f" Then
                            mode = 6
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("Ende der Zeile oder 'from' erwartet!")
                        End If
                    Case 6
                        If lineLarge(i) = "r" Then
                            mode = 7
                        Else
                            Throw New Exception("'from' erwartet!")
                        End If
                    Case 7
                        If lineLarge(i) = "o" Then
                            mode = 8
                        Else
                            Throw New Exception("'from' erwartet!")
                        End If
                    Case 8
                        If lineLarge(i) = "m" Then
                            mode = 9
                        Else
                            Throw New Exception("'from' erwartet!")
                        End If
                    Case 9
                        If lineLarge(i) = """"c Then
                            mode = 10
                        ElseIf lineLarge(i) <> " "c Then
                            Throw New Exception("'""' erwartet!")
                        End If
                    Case 10
                        If lineLarge(i) = """"c Then
                            mode = 11
                        Else
                            fromVal &= lineLarge(i)
                        End If
                    Case 11
                        If lineLarge(i) <> " "c Then
                            Throw New Exception("Ende der Zeile erwartet!")
                        End If
                    Case 101
                        name &= lineLarge(i)
                        If lineLarge(i) = """"c Then
                            hat_str = Not hat_str
                        ElseIf lineLarge(i) = "]"c And Not hat_str Then
                            mode = 2
                        End If
                End Select
            Next
            Dim name_str As New Multi_Lang_String(name)
            If mode = 5 Then
                parameter.Add(New TemplateParameter_String(name_str, defaultVal, False, ""))
                parameter_intern.Add(New ParamName(ParamName.paramArt.Str, name_str.get_ID()))
            ElseIf mode = 11 Then
                parameter.Add(New TemplateParameter_String(name_str, defaultVal, True, fromVal))
                parameter_intern.Add(New ParamName(ParamName.paramArt.Str, name_str.get_ID()))
            Else
                Throw New Exception("Falsche Definition eines param_string (Erwartet wird: 'param_string ""Name"" = ""Default-Wert"" [from ""Zeichen""]'")
            End If
        Else
            Throw New Exception("Ungültiger Befehl: " & lineLarge.ToString())
        End If
    End Sub

    Private Function istIntervalUngültig(min As Integer, max As Integer, _step As Integer, intervalStart_geschlossen As Boolean, intervalEnde_geschlossen As Boolean) As Boolean
        If intervalEnde_geschlossen AndAlso intervalStart_geschlossen Then
            '[2 1] ist falsch; [1 1] == "nur 1" ist ok
            'Step=2: [1 5] = {1 3 5}
            'Step=3: [1 5] = {1 4}
            'Unabhängig vom Step die gleichen Bedingungen: [1 1] ist immer gültig egal wie groß der Step ist!
            Return max < min
        ElseIf Not intervalStart_geschlossen AndAlso Not intervalEnde_geschlossen Then
            '(1 2) ist falsch; (1 3) == "nur 2" ist ok 
            'Step=2: (1 5) = {3} ; (1 3) -> ungültig, da min+step>=max
            Return max <= min + _step
        ElseIf intervalStart_geschlossen Then
            '[1 1) ist falsch; [1 2) == "nur 1" ist ok
            'ist unabhängig vom Step, da der erste Wert immer dabei ist, solange es kleiner ist als der zweite Wert.
            Return max <= min
        Else
            '(1 1] ist falsch; (1 2] == "nur 2" ist ok
            'Step=2: (1 5] = {3 5} ; (1 3] = {3} ; (1 2] -> ungültig, da min+step>max
            Return max < min + _step
        End If
    End Function

    Private Sub defKonst(name As String, wert As String, konstantenListe As Dictionary(Of String, Integer), mode As EinleseModus, vardef As Boolean)
        If name.Length > 0 AndAlso wert.Length > 0 Then
            prüfeObVarKonstanteKorrektIst(name)

            Dim aus As Ausdruck = Ausdruck.EinlesenAusdruck(wert, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
            If TypeOf aus IsNot Ausdruck_Konstante Then
                If vardef Then
                    defVar(name, wert, mode, VariableEinlesen.VariableArt.Int_)
                    Exit Sub
                Else
                    Throw New Exception("Bei der Definition von Konstanten (def) sind nur Konstante Integer erlaubt.")
                End If
            End If
            Dim value As Integer = CInt(DirectCast(aus, Ausdruck_Int).Ausrechnen(Nothing))
            If konstantenListe.ContainsKey(name) Then
                konstantenListe(name) = value
            Else
                konstantenListe.Add(name, value)
            End If
        Else
            Throw New Exception("Ungültige Definition von Konstanter")
        End If
    End Sub

    Private Sub defVar(name As String, wert As String, mode As EinleseModus, art As VariableEinlesen.VariableArt)
        If mode = EinleseModus.IDLE OrElse mode = EinleseModus.LeseGrafik Then
            If name.Length > 0 AndAlso wert.Length > 0 Then
                prüfeObVarKonstanteKorrektIst(name)

                Dim aus_ As Ausdruck = evaluateAusdruck(wert)
                If art = VariableEinlesen.VariableArt.Int_ AndAlso TypeOf aus_ IsNot Ausdruck_Int Then
                    Throw New Exception("Der Variablen '" & name & "' muss ein Integer zugeordnet werden!")
                End If
                If art = VariableEinlesen.VariableArt.String_ AndAlso TypeOf aus_ IsNot AusdruckString Then
                    Throw New Exception("Der Variablen '" & name & "' muss ein String zugeordnet werden!")
                End If
                If art = VariableEinlesen.VariableArt.Boolean_ AndAlso TypeOf aus_ IsNot Ausdruck_Boolean Then
                    Throw New Exception("Der Variablen '" & name & "' muss ein Boolean zugeordnet werden!")
                End If

                For i As Integer = vars_intern.Count - 1 To 0 Step -1
                    If vars_intern(i).name = name Then
                        If vars_intern(i).art <> art Then
                            Throw New Exception("Der Variable '" & name & "' muss ein " & vars_intern(i).getStringArt() & " zugeordnet werden!")
                        End If
                        If mode = EinleseModus.IDLE Then
                            Me.my_Precompiled_Template.add(New Precompiled_SetVar(i, aus_, art))
                        ElseIf mode = EinleseModus.LeseGrafik Then
                            currentGrafik.add(New Precompiled_SetVarGrafik(i, aus_, art))
                        Else
                            Throw New Exception("Falscher Modus bei Definition von Variablen")
                        End If
                        Exit Sub
                    End If
                Next
                'Neue Variable
                Me.vars_intern.Add(New VariableEinlesen(art, name))
                If mode = EinleseModus.IDLE Then
                    Me.my_Precompiled_Template.add(New Precompiled_SetVar(Me.vars_intern.Count - 1, aus_, art))
                Else
                    currentGrafik.add(New Precompiled_SetVarGrafik(Me.vars_intern.Count - 1, aus_, art))
                End If
            Else
                Throw New Exception("Ungültige Definition der Variable")
            End If
        Else
            Throw New Exception("Definition von Variablen ist hier nicht erlaubt")
        End If
    End Sub

    Private Sub prüfeObVarKonstanteKorrektIst(name As String)
        If name = "" Then
            Throw New Exception("Leere Variablen oder Konstanten ('') sind nicht erlaubt")
        End If
        If Not (Char.IsLetter(name(0)) OrElse name(0) = "_"c) Then
            Throw New Exception("Falscher Name '" & name & "' --> Konstanten und Variablen müssen mit Buchstaben oder '_' anfangen!")
        End If
        For i As Integer = 0 To name.Length - 1
            If Not (Char.IsLetterOrDigit(name(i)) OrElse name(i) = "_"c) Then
                Throw New Exception("Falscher Name: '" & name & "' --> Konstanten und Variablen dürfen nur aus Buchstaben und Zahlen und Unterstrichen bestehen!")
            End If
        Next
    End Sub

    Private Sub leseGrafik(line As String, lineLarge As String)
        If line.StartsWith("if(") OrElse line.StartsWith("if ") Then
            begingroup()
            If line.EndsWith(":") Then
                'Throw New Exception(": am Ende der If-Anweisung erwartet")
                line = line.Substring(0, line.Length - 1)
            End If
            line = line.Substring(2, line.Length - 2).Trim() 'remove 'if'
            Dim ausdruckBool As Ausdruck = Ausdruck.EinlesenAusdruck(line, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)

            If TypeOf ausdruckBool Is Ausdruck_Boolean Then
                currentGrafik.add(New Precompiled_IF(vars_intern.Count, DirectCast(ausdruckBool, Ausdruck_Boolean)))
            Else
                Throw New Exception("Bedingung in if-Abfrage erwartet.")
            End If
        ElseIf line.StartsWith("elseif ") OrElse line.StartsWith("elseif(") Then
            endgroup()
            begingroup()
            If line.EndsWith(":") Then
                line = line.Substring(0, line.Length - 1)
            End If
            line = line.Substring(6, line.Length - 6).Trim() 'remove 'elseif'
            Dim ausdruckBool As Ausdruck = Ausdruck.EinlesenAusdruck(line, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
            Dim myIf As Precompiled_MultiGrafik = currentGrafik.getCurrentSubElement()
            If TypeOf myIf IsNot Precompiled_IF AndAlso TypeOf myIf IsNot Precompiled_ELSE_IF Then
                Throw New Exception("'elseif' muss nach einem 'if' kommen")
            End If
            If TypeOf ausdruckBool Is Ausdruck_Boolean Then
                For i As Integer = myIf.getVarCountAtStart_Direkt() To vars_intern.Count - 1
                    vars_intern(i) = New VariableEinlesen(vars_intern(i).art, "") 'ab jetzt kann man nichtmehr darauf zugreifen!
                Next
                If TypeOf myIf Is Precompiled_IF Then
                    currentGrafik.add(New Precompiled_ELSE_IF(vars_intern.Count, DirectCast(ausdruckBool, Ausdruck_Boolean)))
                ElseIf TypeOf myIf Is Precompiled_ELSE_IF Then
                    'jetzt den neuen Block nicht an das ineinander verschachtelte elseif machen sondern an das haupt-if
                    myIf = myIf.parent
                    If myIf Is Nothing OrElse TypeOf myIf IsNot Precompiled_IF Then
                        Throw New Exception("Allgemeiner Fehler beim übersetzen des 'elseif'")
                    End If
                    myIf.add_Direct(New Precompiled_ELSE_IF(vars_intern.Count, DirectCast(ausdruckBool, Ausdruck_Boolean)))
                End If
            Else
                Throw New Exception("Bedingung in elseif-Abfrage erwartet.")
            End If
        ElseIf line = "else" OrElse line = "else:" OrElse line.StartsWith("else ") Then
            endgroup()
            begingroup()
            If line.EndsWith(":") Then
                line = line.Substring(0, line.Length - 1)
                If line.Trim() <> "else" Then
                    Throw New Exception("Ungültige Definition von else: '" & line & ":'")
                End If
            End If
            Dim myIf As Precompiled_MultiGrafik = currentGrafik.getCurrentSubElement()
            If TypeOf myIf IsNot Precompiled_IF AndAlso TypeOf myIf IsNot Precompiled_ELSE_IF Then
                Throw New Exception("'else' muss nach einem 'if' kommen")
            End If
            For i As Integer = myIf.getVarCountAtStart_Direkt() To vars_intern.Count - 1
                vars_intern(i) = New VariableEinlesen(vars_intern(i).art, "") 'ab jetzt kann man nichtmehr darauf zugreifen!
            Next
            If TypeOf myIf Is Precompiled_IF Then
                currentGrafik.add(New Precompiled_ELSE(vars_intern.Count))
            Else
                'jetzt den neuen Block nicht an das ineinander verschachtelte elseif machen sondern an das haupt-if
                myIf = myIf.parent
                If myIf Is Nothing OrElse TypeOf myIf IsNot Precompiled_IF Then
                    Throw New Exception("Allgemeiner Fehler beim übersetzen des 'else'")
                End If
                myIf.add_Direct(New Precompiled_ELSE(vars_intern.Count))
            End If
        ElseIf line.StartsWith("for ") Then
            begingroup()
            If line.EndsWith(":") Then
                line = line.Substring(0, line.Length - 1)
                'Throw New Exception(": am Ende der For-Anweisung erwartet")
            End If
            line = line.Substring(3, line.Length - 3).Trim() 'remove 'for'
            Dim start As Ausdruck_Int = Nothing
            Dim ende As Ausdruck_Int = Nothing
            Dim start_geschlossen As Boolean = False
            Dim ende_geschlossen As Boolean = False
            Dim name As String = ""

            readForDefinition(line, name, start, start_geschlossen, ende, ende_geschlossen)
            prüfeObVarKonstanteKorrektIst(name)

            vars_intern.Add(New VariableEinlesen(VariableEinlesen.VariableArt.Int_, name))
            currentGrafik.add(New Precompiled_FOR(vars_intern.Count, vars_intern.Count - 1, start, ende, start_geschlossen, ende_geschlossen))
        ElseIf line = "end" Then
            endgroup()
            Dim myBlock As Precompiled_MultiGrafik = currentGrafik.getCurrentSubElement()
            currentGrafik.add_End()
            For i As Integer = currentGrafik.getVarCountAtStart() To vars_intern.Count - 1
                vars_intern(i) = New VariableEinlesen(vars_intern(i).art, "") 'ab jetzt kann man nichtmehr darauf zugreifen!
            Next
            If TypeOf myBlock Is Precompiled_ELSE OrElse TypeOf myBlock Is Precompiled_ELSE_IF Then
                currentGrafik.add_End() 'bei einem else muss es zweimal enden! Einmal fürs if und einmal fürs else
            End If
            If currentGrafik.fertig() Then
                Throw New Exception("'end' ist zu viel.")
            End If
        ElseIf line.StartsWith("rect(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 4 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'rect(x, y, breite, höhe)'")
            End If
            currentGrafik.add(New Precompiled_Rect(koards(0), koards(1), koards(2), koards(3), False, Drawing_FillMode.OnlyStroke))
        ElseIf line.StartsWith("rectfill") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 4 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'rectfill(x, y, breite, höhe)'")
            End If
            currentGrafik.add(New Precompiled_Rect(koards(0), koards(1), koards(2), koards(3), True, Drawing_FillMode.OnlyFill))
        ElseIf line.StartsWith("circ(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 4 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'circ(centerX, centerY, radX, radY)'")
            End If
            currentGrafik.add(New Precompiled_Ellipse(koards(0), koards(1), koards(2), koards(3), False, Drawing_FillMode.OnlyStroke))
        ElseIf line.StartsWith("circfill(") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 4 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'circFill(centerX, centerY, radX, radY)'")
            End If
            currentGrafik.add(New Precompiled_Ellipse(koards(0), koards(1), koards(2), koards(3), True, Drawing_FillMode.OnlyFill))
        ElseIf line.StartsWith("line(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If Not (koards.Length >= 4 AndAlso (koards.Length Mod 2 = 0)) Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'line(x1, y1, x2, y2, ... xn, yn)'")
            End If
            If koards.Length = 4 Then
                currentGrafik.add(New Precompiled_Linie(koards(0), koards(1), koards(2), koards(3)))
            Else
                currentGrafik.add(New Precompiled_MultiLinie(koards))
            End If
        ElseIf line.StartsWith("linearrow(") AndAlso line.EndsWith(")") Then
            line = line.Substring(10)
            line = line.Substring(0, line.Length - 1)
            Dim params() As String = readSeperatedIntsAsString(line)
            If params.Length <> 6 AndAlso params.Length <> 7 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'arrow(x1, y1, x2, y2, start, ende)'")
            End If
            If params.Length = 6 Then
                currentGrafik.add(New Precompiled_LineArrow(evaluateInt(params(0)), evaluateInt(params(1)), evaluateInt(params(2)), evaluateInt(params(3)), evaluateAusdruck(params(4)), evaluateAusdruck(params(5))))
            ElseIf params.Length = 7 Then
                currentGrafik.add(New Precompiled_LineArrow(evaluateInt(params(0)), evaluateInt(params(1)), evaluateInt(params(2)), evaluateInt(params(3)), evaluateAusdruck(params(4)), evaluateAusdruck(params(5)), evaluateInt(params(6))))
            End If
        ElseIf line.StartsWith("arrow(") AndAlso line.EndsWith(")") Then
            line = line.Substring(6)
            line = line.Substring(0, line.Length - 1)
            Dim params() As String = readSeperatedIntsAsString(line)
            If params.Length <> 6 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'arrow(x, y, pfeilIndex, pfeilAlign, vx, vy)'")
            End If
            Dim pfeil As Ausdruck = evaluateAusdruck(params(2))
            If TypeOf pfeil Is Ausdruck_Pfeil Then
                currentGrafik.add(New Precompiled_ArrowHead(evaluateInt(params(0)), evaluateInt(params(1)), DirectCast(pfeil, Ausdruck_Pfeil), evaluateInt(params(3)), evaluateInt(params(4)), evaluateInt(params(5))))
            ElseIf TypeOf pfeil Is Ausdruck_Int Then
                currentGrafik.add(New Precompiled_ArrowHead(evaluateInt(params(0)), evaluateInt(params(1)), DirectCast(pfeil, Ausdruck_Int), evaluateInt(params(3)), evaluateInt(params(4)), evaluateInt(params(5))))
            End If
        ElseIf line.StartsWith("poly(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length Mod 2 = 0 AndAlso params.Length > 5 Then
                currentGrafik.add(New Precompiled_Poly(params, False, True, False))
            Else
                Throw New Exception("Falsche Anzahl an Parametern für 'fill(x1, y1, x2, y2, x3, y3 ...)'")
            End If
        ElseIf line.StartsWith("polyfill(") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length Mod 2 = 0 AndAlso params.Length > 5 Then
                currentGrafik.add(New Precompiled_Poly(params, True, False, True))
            Else
                Throw New Exception("Falsche Anzahl an Parametern für 'polyfill(x1, y1, x2, y2, x3, y3 ...)'")
            End If
        ElseIf line.StartsWith("bezier(") AndAlso line.EndsWith(")") Then
            line = line.Substring(7)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length Mod 2 = 0 AndAlso (params.Length \ 2 - 1) Mod 3 = 0 AndAlso params.Length >= 8 Then
                currentGrafik.add(New Precompiled_Bezier(params))
            Else
                Throw New Exception("Falsche Anzahl an Parametern für 'bezier(x1, y1, x2, y2, x3, y3, x4, y4...)'")
            End If
        ElseIf line.StartsWith("dot(") AndAlso line.EndsWith(")") Then
            line = line.Substring(4)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'dot(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Dot(params(0), params(1)))
        ElseIf line.StartsWith("arc(") AndAlso line.EndsWith(")") Then
            line = line.Substring(4)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length = 5 Then
                currentGrafik.add(New Precompiled_Arc(params(0), params(1), params(2), params(3), params(4), False, False))
            ElseIf params.Length = 6 Then
                currentGrafik.add(New Precompiled_Arc(params(0), params(1), params(2), params(3), params(4), params(5), False, False))
            Else
                Throw New Exception("Falsche Anzahl an Parametern bei 'arc(x, y, radius, startwinkel, deltawinkel)'")
            End If
        ElseIf line.StartsWith("pie(") AndAlso line.EndsWith(")") Then
            line = line.Substring(4)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length = 5 Then
                currentGrafik.add(New Precompiled_Arc(params(0), params(1), params(2), params(3), params(4), True, False))
            ElseIf params.Length = 6 Then
                currentGrafik.add(New Precompiled_Arc(params(0), params(1), params(2), params(3), params(4), params(5), True, False))
            Else
                Throw New Exception("Falsche Anzahl an Parametern bei 'pie(x, y, radius, startwinkel, deltawinkel)'")
            End If
        ElseIf line.StartsWith("text(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(5)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            Dim params() As String = readSeperatedIntsAsString(lineLarge)
            If params.Length <> 5 AndAlso params.Length <> 6 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'text(x, y, vx, vy, ""text"")'")
            End If

            Dim px As Ausdruck_Int = evaluateInt(Mathe.strToLower(params(0)))
            Dim py As Ausdruck_Int = evaluateInt(Mathe.strToLower(params(1)))
            Dim vx As Ausdruck_Int = evaluateInt(Mathe.strToLower(params(2)))
            Dim vy As Ausdruck_Int = evaluateInt(Mathe.strToLower(params(3)))
            Dim txt As AusdruckString = evaluateStr(Mathe.strToLower(params(4)))
            Dim ausrichtung As Ausdruck_Int
            If params.Length = 6 Then
                ausrichtung = evaluateInt(Mathe.strToLower(params(5)))
            Else
                ausrichtung = New Ausdruck_Konstante(0)
            End If
            currentGrafik.add(New Precompiled_Text(txt, 0, px, py, vx, vy, ausrichtung))
        ElseIf line.StartsWith("select(") AndAlso line.EndsWith(")") Then
            line = line.Substring(7)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 4 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'select(x, y, breite, höhe)'")
            End If
            currentGrafik.add(New Precompiled_Selection(koards(0), koards(1), koards(2), koards(3)))
        ElseIf line.StartsWith("snap(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            leseSnap_intern(line)
        ElseIf line.StartsWith("snapleft(") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapLeft(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY0, PENALTY1, PENALTY2, PENALTY1))
        ElseIf line.StartsWith("snapright(") AndAlso line.EndsWith(")") Then
            line = line.Substring(10)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapRight(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY2, PENALTY1, PENALTY0, PENALTY1))
        ElseIf line.StartsWith("snaptop(") AndAlso line.EndsWith(")") Then
            line = line.Substring(8)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapTop(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY1, PENALTY0, PENALTY1, PENALTY2))
        ElseIf line.StartsWith("snapbottom(") AndAlso line.EndsWith(")") Then
            line = line.Substring(11)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapBottom(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY1, PENALTY2, PENALTY1, PENALTY0))
        ElseIf line.StartsWith("snaponlyleft(") AndAlso line.EndsWith(")") Then
            line = line.Substring(13)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapOnlyLeft(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY0, PENALTY2, PENALTY2, PENALTY2))
        ElseIf line.StartsWith("snaponlyright(") AndAlso line.EndsWith(")") Then
            line = line.Substring(14)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapOnlyRight(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY2, PENALTY2, PENALTY0, PENALTY2))
        ElseIf line.StartsWith("snaponlytop(") AndAlso line.EndsWith(")") Then
            line = line.Substring(12)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapOnlyTop(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY2, PENALTY0, PENALTY2, PENALTY2))
        ElseIf line.StartsWith("snaponlybottom(") AndAlso line.EndsWith(")") Then
            line = line.Substring(15)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'snapOnlyBottom(x, y)'")
            End If
            currentGrafik.add(New Precompiled_Snappoint(koards(0), koards(1), PENALTY2, PENALTY2, PENALTY2, PENALTY0))
        ElseIf line.StartsWith("invisible(") AndAlso line.EndsWith(")") Then
            line = line.Substring(10)
            line = line.Substring(0, line.Length - 1)
            Dim ops() As String = readSeperatedIntsAsString(line)
            If ops.Count <> 1 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'invisible(""param"")'")
            End If
            Dim aus As Ausdruck = evaluateAusdruck(ops(0))
            If TypeOf aus IsNot AusdruckString_Konstante Then
                Throw New Exception("Der Parametername muss ein konstanter String sein (Bei 'invisible(""param"")'.")
            End If
            Dim param As String = DirectCast(aus, AusdruckString).Ausrechnen(Nothing).ToLower()
            Dim nr As Integer = -1
            For i As Integer = Erster_Echter_Paramter To Me.parameter_intern.Count - 1
                If parameter_intern(i).name.ToLower() = param Then
                    nr = i
                    Exit For
                End If
            Next
            If nr = -1 Then
                Throw New Exception("Der Parameter " & DirectCast(aus, AusdruckString).Ausrechnen(Nothing).ToLower() & " wurde nicht definiert.")
            End If
            currentGrafik.add(New Precompiled_ParamInvisible(nr - Erster_Echter_Paramter, False))
        ElseIf line.StartsWith("lw(") AndAlso line.EndsWith(")") Then
            line = line.Substring(3)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length <> 1 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'lw(scale)'")
            End If
            currentGrafik.add(New Precompiled_SetLineScaling(params(0)))
        ElseIf line.StartsWith("scale_line(") AndAlso line.EndsWith(")") Then
            line = line.Substring(11)
            line = line.Substring(0, line.Length - 1)
            Dim params() As String = readSeperatedIntsAsString(line)
            If params.Length <> 10 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'scale_line(x1,y1,x2,y2,vx,vy,min,step,max,scaling)'")
            End If
            Dim aus(8) As Ausdruck_Int
            For i As Integer = 0 To 8
                Dim ausA = evaluateAusdruck(params(i))
                If TypeOf ausA IsNot Ausdruck_Int Then
                    Throw New Exception("Es wird ein Integer als " & (i + 1).ToString & ". Paramter erwartet.")
                End If
                aus(i) = DirectCast(ausA, Ausdruck_Int)
            Next
            Dim nrScaling As Integer = my_Precompiled_Template.findScaling(params(9))

            currentGrafik.add(New Precompiled_ScaleLine(aus(0), aus(1), aus(2), aus(3), aus(4), aus(5), aus(6), aus(7), aus(8), nrScaling))
        ElseIf line = "fill_backcolor()" Then
            currentGrafik.add(New Precompiled_SetFillColor(False))
        ElseIf line = "fill_forecolor()" Then
            currentGrafik.add(New Precompiled_SetFillColor(True))
        Else
            Throw New Exception("Unbekannter Befehl:  " & line)
        End If
    End Sub

    Private Sub leseScalingStart(line As String)
        If line.EndsWith(":") Then line = line.Substring(0, line.Length - 1).TrimEnd()

        Dim name As String = ""
        Dim var As String = ""
        Dim mode As Integer = 0
        For i As Integer = 0 To line.Length - 1
            If mode = 0 Then
                If line(i) = "(" Then
                    mode = 1
                Else
                    name &= line(i)
                End If
            ElseIf mode = 1 Then
                If line(i) = ")" Then
                    mode = 2
                Else
                    var &= line(i)
                End If
            ElseIf mode = 2 Then
                Throw New Exception("Ende der Zeile erwartet")
            End If
        Next

        name = name.Trim()
        var = var.Trim()

        prüfeObVarKonstanteKorrektIst(name)

        If my_Precompiled_Template.hatScaling(name) Then
            Throw New Exception("Es wurde bereits ein Scaling-Block mit dem Namen '" & name & "' definiert.")
        End If

        Dim vars() As String = readSeperatedIntsAsString(var)
        If vars.Length = 1 Then
            prüfeObVarKonstanteKorrektIst(vars(0))
            Me.vars_intern.Add(New VariableEinlesen(VariableEinlesen.VariableArt.Int_, vars(0)))
            currentPrecompiledScaling = New Precompiled_Scaling(name, Me.vars_intern.Count - 1)
        ElseIf vars.Length = 2 Then
            prüfeObVarKonstanteKorrektIst(vars(0))
            prüfeObVarKonstanteKorrektIst(vars(1))
            Me.vars_intern.Add(New VariableEinlesen(VariableEinlesen.VariableArt.Int_, vars(0)))
            Me.vars_intern.Add(New VariableEinlesen(VariableEinlesen.VariableArt.Int_, vars(1)))
            currentPrecompiledScaling = New Precompiled_Scaling(name, Me.vars_intern.Count - 2, Me.vars_intern.Count - 1)
        End If
    End Sub

    Private Sub leseScaling(line As String, lineLarge As String)
        If line.StartsWith("set_param(") AndAlso line.EndsWith(")") Then
            line = line.Substring(10)
            line = line.Substring(0, line.Length - 1)
            Dim params() As String = readSeperatedIntsAsString(line)
            If params.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'set_param(""param"", value)'")
            End If
            Dim param_str As Ausdruck = evaluateAusdruck(params(0))
            If TypeOf param_str IsNot AusdruckString_Konstante Then
                Throw New Exception("Für den Namen des Parameters wird ein konstanter String erwartet.")
            End If
            Dim param As String = DirectCast(param_str, AusdruckString_Konstante).Ausrechnen(Nothing)
            Dim nr As Integer = -1
            For i As Integer = Erster_Echter_Paramter To parameter_intern.Count - 1
                If parameter_intern(i).name = param Then
                    nr = i
                    Exit For
                End If
            Next
            If nr <= 0 Then
                Throw New Exception("Der Paramter '" & param & "' wurde nicht definiert.")
            End If
            Dim p As ParamName.paramArt = parameter_intern(nr).art

            Select Case p
                Case ParamName.paramArt.Int
                    Dim aus As Ausdruck = evaluateAusdruck(params(1))
                    If TypeOf aus IsNot Ausdruck_Int Then
                        Throw New Exception("Es wird ein Integer erwartet.")
                    End If
                    currentPrecompiledScaling.add(New Precompiled_Scaling_SetParamInt(nr - Erster_Echter_Paramter, DirectCast(aus, Ausdruck_Int)))
                Case Else
                    Throw New Exception("'set_param' ist für diesen Parameter-Typ nicht definiert.")
            End Select
        ElseIf line.StartsWith("move(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim params() As Ausdruck_Int = readSeperatedInts(line)
            If params.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'move(dx, dy)'")
            End If
            currentPrecompiledScaling.add(New precompiled_Scaling_Move(DirectCast(params(0), Ausdruck_Int), DirectCast(params(1), Ausdruck_Int)))
        Else
            Throw New Exception("Unbekannter Befehl:  " & line)
        End If
    End Sub

    Private Sub leseSnap_intern(line As String)
        Dim koards() As String = readSeperatedIntsAsString(line)
        Dim ausdrücke(koards.Length - 1) As Ausdruck
        Dim str_header As String = ""
        For i As Integer = 0 To koards.Length - 1
            ausdrücke(i) = Ausdruck.EinlesenAusdruck(koards(i), konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
            If TypeOf ausdrücke(i) Is Ausdruck_Int Then
                str_header &= "i"
            ElseIf TypeOf ausdrücke(i) Is Ausdruck_Boolean Then
                str_header &= "b"
            Else
                Throw New Exception("Ungültiger Parameter '" & str_header & "' bei Funktion 'snap'")
            End If
        Next
        If ausdrücke.Length = 2 AndAlso str_header = "ii" Then
            currentGrafik.add(New Precompiled_Snappoint(DirectCast(ausdrücke(0), Ausdruck_Int), DirectCast(ausdrücke(1), Ausdruck_Int)))
        ElseIf koards.Length = 4 AndAlso str_header = "iiii" Then
            currentGrafik.add(New Precompiled_Snappoint(DirectCast(ausdrücke(0), Ausdruck_Int), DirectCast(ausdrücke(1), Ausdruck_Int), DirectCast(ausdrücke(2), Ausdruck_Int), DirectCast(ausdrücke(3), Ausdruck_Int), New Ausdruck_Boolean_Konstante(True)))
        ElseIf koards.Length = 5 AndAlso str_header = "iiiib" Then
            currentGrafik.add(New Precompiled_Snappoint(DirectCast(ausdrücke(0), Ausdruck_Int), DirectCast(ausdrücke(1), Ausdruck_Int), DirectCast(ausdrücke(2), Ausdruck_Int), DirectCast(ausdrücke(3), Ausdruck_Int), DirectCast(ausdrücke(4), Ausdruck_Boolean)))
        ElseIf koards.Length = 6 AndAlso str_header = "iiiiii" Then
            currentGrafik.add(New Precompiled_Snappoint(DirectCast(ausdrücke(0), Ausdruck_Int), DirectCast(ausdrücke(1), Ausdruck_Int),
                                                        DirectCast(ausdrücke(2), Ausdruck_Int), DirectCast(ausdrücke(3), Ausdruck_Int), DirectCast(ausdrücke(4), Ausdruck_Int), DirectCast(ausdrücke(5), Ausdruck_Int)))
        ElseIf koards.Length = 7 AndAlso str_header = "iiiiiib" Then
            currentGrafik.add(New Precompiled_Snappoint(DirectCast(ausdrücke(0), Ausdruck_Int), DirectCast(ausdrücke(1), Ausdruck_Int),
                                                        DirectCast(ausdrücke(2), Ausdruck_Int), DirectCast(ausdrücke(3), Ausdruck_Int), DirectCast(ausdrücke(4), Ausdruck_Int), DirectCast(ausdrücke(5), Ausdruck_Int), DirectCast(ausdrücke(6), Ausdruck_Boolean)))
        Else
            Throw New Exception("Der Befehl snap(...) ist für diese Parameter nicht definiert.")
        End If
    End Sub

    Private Sub leseTextPos(line As String)
        If line.StartsWith("pos(") AndAlso line.EndsWith(")") Then
            line = line.Substring(4)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'pos(x y)'")
            End If
            currentTextpos.setPos(koards(0), koards(1))
        ElseIf line.StartsWith("vector(") AndAlso line.EndsWith(")") Then
            line = line.Substring(7)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'vector(x y)'")
            End If
            currentTextpos.setVector(koards(0), koards(1))
        ElseIf line.StartsWith("dist(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'dist(x y)'")
            End If

            currentTextpos.setDist(koards(0), koards(1))
            currentTextpos.hasAbstandVektor = True
        ElseIf line.StartsWith("type(") AndAlso line.EndsWith(")") Then
            line = line.Substring(5)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 1 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'type(nr)'")
            End If
            If TypeOf koards(0) Is Ausdruck_Konstante Then
                currentTextpos.nr = CInt(koards(0).Ausrechnen(Nothing))
            Else
                Throw New Exception("Der Parameter 'nr' bei 'type(nr)' muss eine Konstante sein!")
            End If
        Else
            Throw New Exception("Unbekannter Befehl: " & line)
        End If
    End Sub

    Private Sub leseInfo(line As String, lineLarge As String)
        If line.StartsWith("origin(") AndAlso line.EndsWith(")") Then
            line = line.Substring(7)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'origin(x y)'")
            End If
            Me.my_Precompiled_Template.setOrigin(New PrecompiledSetOrigin(koards(0), koards(1)))
        ElseIf line.StartsWith("name(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(5)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            Me._name = lineLarge.Trim()
        ElseIf line.StartsWith("namespace(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(10)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            If lineLarge.ToLower() = Bibliothek.NAMESPACE_LOKAL.ToLower() Then
                Throw New Exception("Der Name '" & Bibliothek.NAMESPACE_LOKAL & "' ist als Name eines 'namespace' nicht erlaubt")
            End If
            Me._namespace = lineLarge.Trim()
            'ElseIf line.StartsWith("view(") AndAlso line.EndsWith(")") Then
            '    lineLarge = lineLarge.Substring(5)
            '    lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            '    Me._view = lineLarge.Trim()
        ElseIf line.StartsWith("text(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(5)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            Me.DefaultName = lineLarge.Trim()
        ElseIf line.StartsWith("nonumber(") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim koards() As Ausdruck_Int = readSeperatedInts(line)
            If koards.Length <> 0 Then
                Throw New Exception("Falsche Anzahl an Parametern bei 'nonumber()'")
            End If
            Me.MitNummer = False
        Else
            Throw New Exception("Unbekannter Befehl: " & line)
        End If
    End Sub

    Private Sub leseCompatibilit(line As String, lineLarge As String)
        If line.StartsWith("name(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(5)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            currentTemplateCompatibility._name = lineLarge.Trim()
        ElseIf line.StartsWith("namespace(") AndAlso line.EndsWith(")") Then
            lineLarge = lineLarge.Substring(10)
            lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
            currentTemplateCompatibility._Namespace = lineLarge.Trim()
        ElseIf line.StartsWith("setparam(") AndAlso line.EndsWith(")") Then
            line = line.Substring(9)
            line = line.Substring(0, line.Length - 1)
            Dim ops() As String = readSeperatedIntsAsString(line)
            If ops.Count <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern für 'setparams(""param"", value)'.")
            End If
            Dim a1 As Ausdruck = evaluateAusdruck(ops(0))
            If TypeOf a1 IsNot AusdruckString_Konstante Then
                Throw New Exception("Falscher Parameter bei 'setparams(""param"", value)': ""param"" muss ein konstanter String sein!")
            End If
            Dim p As String = DirectCast(a1, AusdruckString).Ausrechnen(Nothing)
            Dim val As String = ops(1).Trim()
            currentTemplateCompatibility.listPreset.Add(New default_Parameter(p, val))
        ElseIf line.StartsWith("offset(") AndAlso line.EndsWith(")") Then
            line = line.Substring(7)
            line = line.Substring(0, line.Length - 1)
            Dim ops() As Ausdruck_Int = readSeperatedInts(line)
            If ops.Count <> 2 Then
                Throw New Exception("Falsche Anzahl an Parametern für 'offset(x, y)'")
            End If
            If TypeOf ops(0) IsNot Ausdruck_Konstante OrElse TypeOf ops(1) IsNot Ausdruck_Konstante Then
                Throw New Exception("Der Offset bei 'offset(x, y)' muss eine Konstante sein!")
            End If
            currentTemplateCompatibility.setOffset(CInt(ops(0).Ausrechnen(Nothing)), CInt(ops(1).Ausrechnen(Nothing)))
        End If
    End Sub

    Private Function readSeperatedInts(s As String) As Ausdruck_Int()
        Dim strings() As String = Mathe.splitString(s, ","c)
        Dim ergI(strings.Length - 1) As Ausdruck_Int
        For i As Integer = 0 To strings.Length - 1
            ergI(i) = evaluateInt(strings(i))
        Next
        Return ergI
    End Function

    Private Function readSeperatedIntsAsString(s As String) As String()
        Return Mathe.readSeperatedStrings(s)
    End Function

    Private Function evaluateInt(s As String) As Ausdruck_Int
        Dim aus As Ausdruck = Ausdruck.EinlesenAusdruck(s, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
        If TypeOf aus Is Ausdruck_Int Then
            Return DirectCast(aus, Ausdruck_Int)
        End If
        Throw New Exception("Der Ausdruck '" & s & "' kann nicht in einen Integer umgewandelt werden.")
    End Function

    Private Function evaluateAusdruck(s As String) As Ausdruck
        Return Ausdruck.EinlesenAusdruck(s, konstanten_CurrentBlock.Peek(), parameter_intern, vars_intern, parameter)
    End Function

    Private Function evaluateStr(s As String) As AusdruckString
        Dim aus As Ausdruck = evaluateAusdruck(s)
        If TypeOf aus Is AusdruckString Then
            Return DirectCast(aus, AusdruckString)
        End If
        Throw New Exception("Zeichenkette erwartet bei " & s & ".")
    End Function

    Private Sub readForDefinition(line As String, ByRef varName As String, ByRef intervallStart As Ausdruck_Int, ByRef startGeschlossen As Boolean, ByRef intervallEnd As Ausdruck_Int, ByRef endeGeschlossen As Boolean)
        Dim mode As Integer = 0
        Dim name As String = ""
        Dim intervalStart_geschlossen As Boolean = False
        Dim intervalEnde_geschlossen As Boolean = False
        Dim intervalString As String = ""
        For i As Integer = 0 To line.Length - 1
            Select Case mode
                Case 0
                    If line(i) = "="c Then
                        mode = 1
                    Else
                        name &= line(i)
                    End If
                Case 1
                    If line(i) = "(" Then
                        intervalStart_geschlossen = False
                        mode = 2
                    ElseIf line(i) = "[" Then
                        intervalStart_geschlossen = True
                        mode = 2
                    ElseIf line(i) <> " " Then
                        Throw New Exception("'(' oder '[' erwartet!")
                    End If
                Case 2
                    intervalString &= line(i)
            End Select
        Next
        intervalString = Mathe.strToLower(intervalString)
        If mode <> 2 Then
            Throw New Exception("Ungültige Definition der for-Schleife bei '" & line & "'")
        End If
        If intervalString.EndsWith(")") Then
            intervalEnde_geschlossen = False
        ElseIf intervalString.EndsWith("]") Then
            intervalEnde_geschlossen = True
        Else
            Throw New Exception("')' oder ']' erwartet!")
        End If
        intervalString = intervalString.Substring(0, intervalString.Length - 1)

        'Dim values() As Integer = Mathe.readSeperatedInts(intervalString, konstanten, konstanten_CurrentBlock)
        Dim values() As Ausdruck_Int = readSeperatedInts(intervalString)
        If values.Length <> 2 Then
            Throw New Exception("Falsche Definition des Intervals bei '" & line & "'")
        End If

        varName = name.Trim()
        intervallStart = values(0)
        startGeschlossen = intervalStart_geschlossen
        intervallEnd = values(1)
        endeGeschlossen = intervalEnde_geschlossen
    End Sub

    Private Function entferneKommentar(line As String) As String
        Dim inAnführungszeichen As Boolean = False
        For i As Integer = 0 To line.Length - 1
            If line(i) = """"c Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                If line(i) = "%"c Then
                    If i = 0 Then
                        Return ""
                    Else
                        Return line.Substring(0, i)
                    End If
                End If
            End If
        Next
        Return line
    End Function

    Private Enum EinleseModus
        IDLE
        LeseGrafik
        LeseInfo
        LeseTextPos
        Compatibility
        LeseScaling
    End Enum
#End Region

#Region "Template Compiled erstellen"
    Public Sub recompile(paramEinstellungen() As ParamValue, indexBeschriftung As Integer, ByRef compiled_out As Template_Compiled, parentArgs As CompileParentArgs)
        Try
            If parentArgs Is Nothing Then
                parentArgs = Empty_parentArgs
            End If

            If paramEinstellungen Is Nothing Then
                paramEinstellungen = defaulParameterValues
            End If
            Dim textpos_type As Integer = getTextpos(indexBeschriftung)

            Dim PARAMS_INTERN As Integer = 1 '#Textpos
            If Me.isDeko Then
                PARAMS_INTERN += CompileParentArgs.NrChildParams
            End If
            Dim vars As New List(Of ParamValue)(parameter.Count + PARAMS_INTERN)
            vars.Add(New ParamInt(textpos_type)) '#textpos ist parameter 0
            If Me.isDeko Then
                parentArgs.addParams(vars)
            End If
            For i As Integer = 0 To parameter.Count - 1
                If TypeOf parameter(i) Is TemplateParameter_Arrow Then
                    vars.Add(paramEinstellungen(i))
                ElseIf TypeOf parameter(i) Is TemplateParameter_Int Then
                    vars.Add(paramEinstellungen(i))
                ElseIf TypeOf parameter(i) Is TemplateParameter_Param Then
                    vars.Add(paramEinstellungen(i))
                ElseIf TypeOf parameter(i) Is TemplateParameter_String Then
                    vars.Add(paramEinstellungen(i))
                Else
                    Throw New Exception("Unbekannte ParameterArt.")
                End If
            Next
            Dim vars_intern As New List(Of VariablenWert)(AnzahlVarsIntern)
            For i As Integer = 0 To AnzahlVarsIntern - 1
                vars_intern.Add(Nothing)
            Next
            Dim args As New AusrechnenArgs(vars, vars_intern)

            my_Precompiled_Template.recompile(args, compiled_out, Me.parameter.Count, parentArgs)
        Catch ex As Exception
            MessageBox.Show("Fehler beim übersetzen des Symbols '" & _namespace & "." & _name & "'" & vbCrLf & vbCrLf & "Fehlermeldung:" & vbCrLf & ex.Message)

            compiled_out = New Template_Compiled(0)
            Dim fehler_grafik As New DO_MultiGrafik()
            fehler_grafik.childs.Add(New DO_Linie(New Point(-300, -300), New Point(300, 300), False))
            fehler_grafik.childs.Add(New DO_Linie(New Point(-300, 300), New Point(300, -300), False))

            For i As Integer = 0 To Me.TextposTypes.Count - 1
                compiled_out.add_textpos(New TextPoint(New Point(0, 0), 0, 0, New Point(0, 0)))
            Next

            compiled_out.set_selectionRect(New Rectangle(-300, -300, 600, 600))

            compiled_out.set_grafik(fehler_grafik)
            compiled_out.set_Scaling(New List(Of ScalingLinie))
        End Try
    End Sub

    Private Function getTextpos(indexBeschriftung As Integer) As Integer
        If TextposTypes.Count = 0 Then Return 0
        If indexBeschriftung < 0 AndAlso indexBeschriftung > TextposTypes.Count - 1 Then
            Throw New IndexOutOfRangeException("Positionsindex außerhalb des gültigen Bereiches.")
        End If
        Return Me.TextposTypes(indexBeschriftung)
    End Function

    Public Function recompileScaling(CallbackNr As Integer, scale As Integer, scale_normiert As Integer, bauteil As BauteilAusDatei, paramEinstellungen() As ParamValue, indexBeschriftung As Integer, parentArgs As CompileParentArgs) As Boolean
        If parentArgs Is Nothing Then
            parentArgs = Empty_parentArgs
        End If

        Dim textpos_type As Integer = getTextpos(indexBeschriftung)

        Dim PARAMS_INTERN As Integer = 1 '#Textpos
        If Me.isDeko Then
            PARAMS_INTERN += CompileParentArgs.NrChildParams
        End If
        Dim vars As New List(Of ParamValue)(parameter.Count + PARAMS_INTERN)
        vars.Add(New ParamInt(textpos_type)) '#textpos ist parameter 0
        If Me.isDeko Then
            parentArgs.addParams(vars)
        End If
        For i As Integer = 0 To parameter.Count - 1
            If TypeOf parameter(i) Is TemplateParameter_Arrow Then
                vars.Add(paramEinstellungen(i))
            ElseIf TypeOf parameter(i) Is TemplateParameter_Int Then
                vars.Add(paramEinstellungen(i))
            ElseIf TypeOf parameter(i) Is TemplateParameter_Param Then
                vars.Add(paramEinstellungen(i))
            ElseIf TypeOf parameter(i) Is TemplateParameter_String Then
                vars.Add(paramEinstellungen(i))
            Else
                Throw New Exception("Unbekannte ParameterArt.")
            End If
        Next
        Dim vars_intern As New List(Of VariablenWert)(AnzahlVarsIntern)
        For i As Integer = 0 To AnzahlVarsIntern - 1
            vars_intern.Add(Nothing)
        Next
        Dim args As New AusrechnenArgs(vars, vars_intern)

        Return my_Precompiled_Template.recompileScaling(CallbackNr, scale, scale_normiert, args, bauteil, paramEinstellungen, parentArgs)
    End Function

#End Region

#Region "Parameter, Default Parameter"
    Public Function getNrOfParams() As Integer
        Return parameter.Count
    End Function

    Public Function getParameter(index As Integer) As TemplateParameter
        Return parameter(index)
    End Function

    Public Sub reload_defaultParameterValues()
        Dim paramsQuelle() As default_Parameter = Settings.getSettings().default_params
        __lade_defaultParameterValues(defaulParameterValues, paramsQuelle, True, True)
    End Sub

    Public Sub __lade_defaultParameterValues(paramsZiel() As ParamValue, paramsQuelle() As default_Parameter, useNameSpaceUndName As Boolean, AnsonstenDefaultWert As Boolean)
        Dim paramName As String
        Dim param As TemplateParameter_Param
        Dim param_a As TemplateParameter_Arrow
        Dim param_i As TemplateParameter_Int
        Dim param_s As TemplateParameter_String
        For i As Integer = 0 To parameter.Count - 1
            If TypeOf parameter(i) Is TemplateParameter_Param Then
                param = DirectCast(parameter(i), TemplateParameter_Param)
                If useNameSpaceUndName Then
                    paramName = (_namespace & "." & _name & "." & param.name.get_ID()).ToLower()
                Else
                    paramName = param.name.get_ID().ToLower()
                End If
                Dim hat_geändert As Boolean = False
                For j As Integer = 0 To paramsQuelle.Length - 1
                    If paramName Like paramsQuelle(j).param Then

                        For k As Integer = 0 To param.options.Length - 1
                            If param.options(k).get_ID().ToLower() = paramsQuelle(j).value Then
                                paramsZiel(i) = New ParamInt(k)
                                hat_geändert = True
                                Exit For
                            End If
                        Next

                    End If
                Next
                If Not hat_geändert AndAlso AnsonstenDefaultWert Then
                    'default-wert = 0
                    paramsZiel(i) = New ParamInt(0)
                End If
            ElseIf TypeOf parameter(i) Is TemplateParameter_Arrow Then
                param_a = DirectCast(parameter(i), TemplateParameter_Arrow)
                If useNameSpaceUndName Then
                    paramName = (_namespace & "." & _name & "." & param_a.name.get_ID()).ToLower()
                Else
                    paramName = param_a.name.get_ID().ToLower()
                End If
                Dim hat_geändert As Boolean = False
                For j As Integer = 0 To paramsQuelle.Length - 1
                    If paramName Like paramsQuelle(j).param Then
                        Dim wert As Integer
                        If Integer.TryParse(paramsQuelle(j).value, wert) Then
                            If wert >= param_a.intervall.min AndAlso wert <= param_a.intervall.max Then
                                paramsZiel(i) = New ParamArrow(CShort(wert), 100)
                                hat_geändert = True
                            End If
                        End If
                    End If
                Next
                If Not hat_geändert AndAlso AnsonstenDefaultWert Then
                    'default-wert = min-wert!
                    paramsZiel(i) = New ParamArrow(CShort(param_a.defaultVal), 100)
                End If
            ElseIf TypeOf parameter(i) Is TemplateParameter_Int Then
                param_i = DirectCast(parameter(i), TemplateParameter_Int)
                If useNameSpaceUndName Then
                    paramName = (_namespace & "." & _name & "." & param_i.name.get_ID()).ToLower()
                Else
                    paramName = param_i.name.get_ID().ToLower()
                End If
                Dim hat_geändert As Boolean = False
                For j As Integer = 0 To paramsQuelle.Length - 1
                    If paramName Like paramsQuelle(j).param Then

                        Dim wert As Integer
                        If Integer.TryParse(paramsQuelle(j).value, wert) Then
                            If wert >= param_i.intervall.min AndAlso wert <= param_i.intervall.max AndAlso (wert - param_i.intervall.min) Mod param_i.intervall._step = 0 Then
                                paramsZiel(i) = New ParamInt(wert)
                                hat_geändert = True
                            End If
                        End If

                    End If
                Next
                If Not hat_geändert AndAlso AnsonstenDefaultWert Then
                    'default-wert = min-wert!
                    paramsZiel(i) = New ParamInt(param_i.defaultVal)
                End If
            ElseIf TypeOf parameter(i) Is TemplateParameter_String Then
                param_s = DirectCast(parameter(i), TemplateParameter_String)
                If useNameSpaceUndName Then
                    paramName = (_namespace & "." & _name & "." & param_s.name.get_ID()).ToLower()
                Else
                    paramName = param_s.name.get_ID().ToLower()
                End If
                Dim hat_geändert As Boolean = False
                For j As Integer = 0 To paramsQuelle.Length - 1
                    If paramName Like paramsQuelle(j).param Then
                        Dim str_c As String = paramsQuelle(j).value.Trim()
                        If str_c.StartsWith("""") AndAlso str_c.EndsWith("""") Then
                            str_c = str_c.Substring(1, str_c.Length - 2)
                            If param_s.istErlaubt(str_c) Then
                                paramsZiel(i) = New ParamString(str_c)
                                hat_geändert = True
                            End If
                        End If
                    End If
                Next
                If Not hat_geändert AndAlso AnsonstenDefaultWert Then
                    'default-wert = min-wert!
                    paramsZiel(i) = New ParamString(param_s.defaultval)
                End If
            Else
                Throw New Exception("Unbekannte Parameter-Art!")
            End If
        Next
    End Sub

    Public Function try_translate_param_value(_param As String, _value As String, str_to_ID As Boolean) As KeyValuePair(Of String, String)?
        Dim paramName As String
        Dim param As TemplateParameter_Param
        Dim param_a As TemplateParameter
        For i As Integer = 0 To parameter.Count - 1
            If TypeOf parameter(i) Is TemplateParameter_Param Then
                param = DirectCast(parameter(i), TemplateParameter_Param)
                If str_to_ID Then
                    paramName = (_namespace & "." & _name & "." & param.name.get_str()).ToLower()
                Else
                    paramName = (_namespace & "." & _name & "." & param.name.get_ID()).ToLower()
                End If
                If paramName Like _param Then
                    For k As Integer = 0 To param.options.Length - 1
                        If str_to_ID Then
                            If param.options(k).get_str().ToLower() = _value Then
                                Return New KeyValuePair(Of String, String)(replaceLast(_param, param.getName().get_ID), param.options(k).get_ID().ToLower())
                            End If
                        Else
                            If param.options(k).get_ID().ToLower() = _value Then
                                Return New KeyValuePair(Of String, String)(replaceLast(_param, param.getName().get_str), param.options(k).get_str().ToLower())
                            End If
                        End If
                    Next
                End If
            ElseIf TypeOf parameter(i) Is TemplateParameter_Arrow OrElse
                   TypeOf parameter(i) Is TemplateParameter_Int OrElse
                   TypeOf parameter(i) Is TemplateParameter_String Then
                param_a = DirectCast(parameter(i), TemplateParameter)
                If str_to_ID Then
                    paramName = (_namespace & "." & _name & "." & param_a.getName().get_str()).ToLower()
                Else
                    paramName = (_namespace & "." & _name & "." & param_a.getName().get_ID()).ToLower()
                End If
                If paramName Like _param Then
                    If str_to_ID Then
                        Return New KeyValuePair(Of String, String)(replaceLast(_param, param_a.getName().get_ID), _value)
                    Else
                        Return New KeyValuePair(Of String, String)(replaceLast(_param, param_a.getName().get_str), _value)
                    End If
                End If
            Else
                Throw New Exception("Unbekannte Parameter-Art!")
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function replaceLast(str As String, replace As String) As String
        For k As Integer = str.Length - 1 To 1 Step -1
            If str(k) = "." Then
                Dim last_str As String = str.Substring(k + 1)
                If last_str <> "" AndAlso Not last_str.Contains("*") Then
                    Return str.Substring(0, k + 1) & replace.ToLower()
                End If
            End If
        Next
        Return str
    End Function

    Public Function getDefaultParameter_copy(index As Integer) As ParamValue
        Return defaulParameterValues(index).Copy()
    End Function

    Public Function getDefaultParameters_copy() As ParamValue()
        Dim erg(defaulParameterValues.Length - 1) As ParamValue
        For i As Integer = 0 To defaulParameterValues.Length - 1
            erg(i) = defaulParameterValues(i).Copy()
        Next
        Return erg
    End Function
#End Region

#Region "Einfache Standardwerte zurückgeben"
    Public Function getNrOfTextPoints() As Integer
        Return Me.TextposTypes.Count
    End Function

    Public Function getIsDeko() As Boolean
        Return isDeko
    End Function

    Public Function getNameMitNummer() As Boolean
        Return MitNummer
    End Function

    Public Function getDefaultNaming() As String
        Return Me.DefaultName
    End Function

    Public Function getNameSpace() As String
        Return _namespace
    End Function

    Public Function getName() As String
        Return _name
    End Function

    Public Function getView() As String
        Return _view
    End Function
#End Region

    Public Function sucheBauteil_Compatibility(_namespace As String, _name As String) As TemplateCompatibility
        For i As Integer = 0 To templatesCompatibility.Count - 1
            If templatesCompatibility(i)._name = _name AndAlso templatesCompatibility(i)._Namespace = _namespace Then
                Return templatesCompatibility(i)
            End If
        Next
        Return Nothing
    End Function

#Region "Speichern, Laden und Exportieren, Vergleich"
    Public Sub speichern(writer As BinaryWriter)
        Me.speichern(writer, False)
    End Sub

    Public Sub speichern(writer As BinaryWriter, ohneNameNamespace As Boolean)
        my_Precompiled_Template.speichern(writer)

        writer.Write(TextposTypes.Count)
        For i As Integer = 0 To TextposTypes.Count - 1
            writer.Write(TextposTypes(i))
        Next

        writer.Write(DefaultName)
        writer.Write(AnzahlVarsIntern)

        writer.Write(parameter.Count)
        For i As Integer = 0 To parameter.Count - 1
            TemplateParameter.speichern(parameter(i), writer)
            ParamValue.speichern(defaulParameterValues(i), writer)
        Next

        If Not ohneNameNamespace Then
            writer.Write(_namespace)
            writer.Write(_name)
            writer.Write(_view)
        End If
        writer.Write(MitNummer)
        writer.Write(isDeko)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As TemplateAusDatei
        Dim erg As New TemplateAusDatei()
        erg.my_Precompiled_Template = Precompiled_Template.laden(reader, version)

        Dim anzahlTextposTypes As Integer = reader.ReadInt32()
        If anzahlTextposTypes < 0 Then Throw New Exception("Die Anzahl der Textpositionen darf nicht negativ sein!")
        erg.TextposTypes = New List(Of Integer)(anzahlTextposTypes)
        For i As Integer = 0 To anzahlTextposTypes - 1
            erg.TextposTypes.Add(reader.ReadInt32())
        Next

        erg.DefaultName = reader.ReadString()
        erg.AnzahlVarsIntern = reader.ReadInt32()

        Dim anzahlParameter As Integer = reader.ReadInt32()
        If anzahlParameter < 0 Then Throw New Exception("Die Anzahl der Parameter darf nicht negativ sein!")
        erg.parameter = New List(Of TemplateParameter)(anzahlParameter)
        ReDim erg.defaulParameterValues(anzahlParameter - 1)
        For i As Integer = 0 To anzahlParameter - 1
            erg.parameter.Add(TemplateParameter.laden(reader, version))
            erg.defaulParameterValues(i) = ParamValue.laden(reader, version)
        Next

        If version = 13 Then 'nur zu Anfang wurde anzahltemplatesCompatibility mit gespeichert (aber immer nur für templates mit einer leeren liste)
            Dim anzahltemplatesCompatibility As Integer = reader.ReadInt32()
            If anzahltemplatesCompatibility <> 0 Then Throw New Exception("Die Anzahl der Elemente muss hier Null sein!")
        End If
        erg.templatesCompatibility = New List(Of TemplateCompatibility)(0) 'Keine Compatibilitys (braucht es nicht für gespeicherte Symbole!)

        erg._namespace = reader.ReadString()
        erg._name = reader.ReadString()
        erg._view = reader.ReadString()
        erg.MitNummer = reader.ReadBoolean()
        erg.isDeko = reader.ReadBoolean()

        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter, alternateNamespace As String)
        If Me.isDeko Then
            writer.WriteLine("use_as_child")
            writer.WriteLine("")
        End If

        writer.precompiled_template = my_Precompiled_Template

        'Parameter exportieren
        For i As Integer = 0 To parameter.Count - 1
            TemplateParameter.export(parameter(i), writer)
        Next

        Dim extraParameterAmAnfang As Integer = 1 '#textpos
        'Parameter-Namen an den writer übergeben, sodass man weiß wie die Parameter heißen!

        If Me.isDeko Then
            extraParameterAmAnfang += CompileParentArgs.NrChildParams
        End If

        Dim paramExport As New List(Of TemplateParameter)(Me.parameter.Count + extraParameterAmAnfang)
        '#textpos ist intern parameter0, der aber nicht in der parameterliste ist!
        paramExport.Add(New TemplateParameter_Int(New Multi_Lang_String("#textpos"), New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), 0, ""))
        If Me.isDeko Then
            CompileParentArgs.addParamsExport(paramExport)
        End If
        paramExport.AddRange(Me.parameter)
        writer.parameter = paramExport
        writer.extraParameterAmAnfang = extraParameterAmAnfang

        'exportiere Scalings
        my_Precompiled_Template.exportScalings(writer)

        'Schreibe precompiled Template (Variablen und Drawing-Blöcke)
        my_Precompiled_Template.export(writer)

        'Info Block
        writer.WriteLine("Info:")
        writer.increase_Indend()
        If writer.lastSetOrigin IsNot Nothing Then
            writer.lastSetOrigin.exportiere(writer)
            writer.lastSetOrigin = Nothing
        End If
        writer.WriteLine("text(" & DefaultName & ")")
        If MitNummer = False Then
            writer.WriteLine("nonumber()")
        End If
        writer.WriteLine("namespace(" & alternateNamespace & ")")
        writer.WriteLine("name(" & _name & ")")
        writer.decrease_Indend()
        writer.WriteLine("Info End")
    End Sub

    Public Function istGleich(tmpl As TemplateAusDatei) As Boolean
        Dim ms As New MemoryStream()
        Dim writer As New BinaryWriter(ms)
        Me.speichern(writer, True)
        writer.Flush()
        Dim bytMe() As Byte = ms.GetBuffer()
        ms.Close()

        ms = New MemoryStream()
        writer = New BinaryWriter(ms)
        tmpl.speichern(writer, True)
        writer.Flush()
        Dim bytOther() As Byte = ms.GetBuffer()
        ms.Close()

        If bytMe.Length <> bytOther.Length Then
            Return False
        End If
        For i As Integer = 0 To bytMe.Length - 1
            If bytMe(i) <> bytOther(i) Then Return False
        Next
        Return True
    End Function
#End Region
End Class

Public Class TextPoint
    Public pos As Point
    Public xDir As Integer
    Public yDir As Integer
    Public vektorAbstand As Point
    Public Sub New(p As Point, x As Integer, y As Integer, vektorAbstand As Point)
        Me.pos = p
        Me.xDir = x
        Me.yDir = y
        Me.vektorAbstand = vektorAbstand
    End Sub
End Class

Public MustInherit Class TemplateParameter
    Public Shared Sub speichern(param As TemplateParameter, writer As BinaryWriter)
        If TypeOf param Is TemplateParameter_Param Then
            writer.Write(1)
            DirectCast(param, TemplateParameter_Param).save(writer)
        ElseIf TypeOf param Is TemplateParameter_String Then
            writer.Write(2)
            DirectCast(param, TemplateParameter_String).save(writer)
        ElseIf TypeOf param Is TemplateParameter_Int Then
            writer.Write(3)
            DirectCast(param, TemplateParameter_Int).save(writer)
        ElseIf TypeOf param Is TemplateParameter_Arrow Then
            writer.Write(4)
            DirectCast(param, TemplateParameter_Arrow).save(writer)
        ElseIf TypeOf param Is TemplateParameter_Double Then
            writer.Write(5)
            DirectCast(param, TemplateParameter_Double).save(writer)
        Else
            Throw New NotImplementedException("Fehler P1000: Dieser Parameter kann nicht gespeichert werden")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As TemplateParameter
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1
                Return TemplateParameter_Param.load(reader, version)
            Case 2
                Return TemplateParameter_String.load(reader, version)
            Case 3
                Return TemplateParameter_Int.load(reader, version)
            Case 4
                Return TemplateParameter_Arrow.load(reader, version)
            Case 5
                Return TemplateParameter_Double.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L1000: Dieser Parameter kann nicht geladen werden")
        End Select
    End Function

    Public Shared Sub export(param As TemplateParameter, writer As Export_StreamWriter)
        If TypeOf param Is TemplateParameter_Param Then
            DirectCast(param, TemplateParameter_Param).exportiere(writer)
        ElseIf TypeOf param Is TemplateParameter_String Then
            DirectCast(param, TemplateParameter_String).exportiere(writer)
        ElseIf TypeOf param Is TemplateParameter_Int Then
            DirectCast(param, TemplateParameter_Int).exportiere(writer)
        ElseIf TypeOf param Is TemplateParameter_Arrow Then
            DirectCast(param, TemplateParameter_Arrow).exportiere(writer)
        ElseIf TypeOf param Is TemplateParameter_Double Then
            DirectCast(param, TemplateParameter_Double).exportiere(writer)
        Else
            Throw New Exception("Der Parameter kann nicht exportiert werden")
        End If
    End Sub

    Public MustOverride Function getName() As Multi_Lang_String
End Class

Public Class TemplateParameter_Arrow
    Inherits TemplateParameter

    Public name As Multi_Lang_String
    Public intervall As Intervall
    Public defaultVal As Integer

    Public Sub New(name As Multi_Lang_String, intervall As Intervall, defaultVal As Integer)
        Me.name = name
        Me.intervall = intervall
        If Me.intervall.min < -1 Then Me.intervall.min = -1
        If Me.intervall.max < -1 Then Me.intervall.max = -1
        If Me.intervall.min > Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile() - 1 Then
            Me.intervall.min = Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile() - 1
        End If
        If Me.intervall.max > Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile() - 1 Then
            Me.intervall.max = Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile() - 1
        End If

        Me.defaultVal = Me.intervall.fitToRange(defaultVal)
    End Sub

    Public Overrides Function getName() As Multi_Lang_String
        Return name
    End Function

    Public Sub save(writer As BinaryWriter)
        name.speichern(writer)
        writer.Write(defaultVal)
        intervall.speichern(writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As TemplateParameter_Arrow
        Dim name As Multi_Lang_String = Multi_Lang_String.Einlesen(reader, version)
        Dim defaultVal As Integer = reader.ReadInt32()
        Dim intervall As Intervall = Intervall.laden(reader, version)
        Return New TemplateParameter_Arrow(name, intervall, defaultVal)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "param_arrow "
        line &= name.exportieren() & " = "
        line &= defaultVal
        If Not intervall.ist_unendlich() Then
            line &= " from ["
            line &= intervall.min & ", "
            If intervall._step <> 1 Then
                line &= intervall._step & ", "
            End If
            line &= intervall.max & "]"
        End If
        'Bei param_arrow gibts kein clip oder mod. Macht einfach keinen Sinn
        writer.WriteLine(line)
    End Sub
End Class

Public Class TemplateParameter_Int
    Inherits TemplateParameter
    Public name As Multi_Lang_String
    Public intervall As Intervall
    Public defaultVal As Integer
    Public unit As String

    Public Sub New(name As Multi_Lang_String, intervall As Intervall, defaultVal As Integer, unit As String)
        Me.name = name
        Me.intervall = intervall
        Me.unit = unit
        Me.defaultVal = intervall.fitToRange(defaultVal)
    End Sub

    Public Overrides Function getName() As Multi_Lang_String
        Return name
    End Function

    Public Sub save(writer As BinaryWriter)
        name.speichern(writer)
        writer.Write(defaultVal)
        writer.Write(unit)
        intervall.speichern(writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As TemplateParameter_Int
        Dim name As Multi_Lang_String = Multi_Lang_String.Einlesen(reader, version)
        Dim defaultVal As Integer = reader.ReadInt32()
        Dim unit As String = reader.ReadString()
        Dim intervall As Intervall = Intervall.laden(reader, version)
        Return New TemplateParameter_Int(name, intervall, defaultVal, unit)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "param_int "
        line &= name.exportieren() & " = "
        line &= defaultVal
        If Not intervall.ist_unendlich() Then
            line &= " from ["
            line &= intervall.min & ", "
            If intervall._step <> 1 Then
                line &= intervall._step & ", "
            End If
            line &= intervall.max & "]"
        End If
        If unit <> "" Then
            line &= " unit """ & unit & """"
        End If
        If intervall.outOfRange = Intervall.OutOfRangeMode.Modulo Then
            line &= " mod"
        Else
            line &= " clip"
        End If
        writer.WriteLine(line)
    End Sub
End Class

Public Class TemplateParameter_String
    Inherits TemplateParameter
    Public name As Multi_Lang_String
    Public defaultval As String
    Public hatFrom As Boolean
    Public fromVal As String
    Public Sub New(name As Multi_Lang_String, defaultVal As String, hatfrom As Boolean, fromVal As String)
        Me.name = name
        Me.defaultval = defaultVal
        If hatfrom Then
            Me.fromVal = fromVal
        Else
            Me.fromVal = ""
        End If
        Me.hatFrom = hatfrom
        If Not istErlaubt(Me.defaultval) Then
            Throw New Exception("Der Default-Wert '" & defaultVal & "' ist ein nicht erlaubter Wert für den Parameter '" & name.get_ID() & "'.")
        End If
    End Sub

    Public Function istErlaubt(str As String) As Boolean
        If hatFrom Then
            For i As Integer = 0 To str.Length - 1
                If Not fromVal.Contains(str(i)) Then
                    Return False
                End If
            Next
        End If
        Return True
    End Function

    Public Overrides Function getName() As Multi_Lang_String
        Return name
    End Function

    Public Sub save(writer As BinaryWriter)
        name.speichern(writer)
        writer.Write(defaultval)
        writer.Write(hatFrom)
        If hatFrom Then
            writer.Write(fromVal)
        End If
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As TemplateParameter_String
        Dim name As Multi_Lang_String = Multi_Lang_String.Einlesen(reader, version)
        Dim defaultVal As String = reader.ReadString()
        Dim hatFrom As Boolean = reader.ReadBoolean()
        Dim fromVal As String = ""
        If hatFrom Then
            fromVal = reader.ReadString()
        End If
        Return New TemplateParameter_String(name, defaultVal, hatFrom, fromVal)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "param_string "
        line &= name.exportieren() & " = "
        line &= """" & defaultval & """"
        If hatFrom Then
            line &= " from """
            line &= fromVal & """"
        End If
        writer.WriteLine(line)
    End Sub
End Class

Public Class TemplateParameter_Double
    Inherits TemplateParameter
    Public name As Multi_Lang_String
    Public defaultVal As Double
    Public unit As String

    Public Sub New(name As Multi_Lang_String, defaultVal As Double, unit As String)
        Me.name = name
        Me.unit = unit
        Me.defaultVal = defaultVal
    End Sub

    Public Overrides Function getName() As Multi_Lang_String
        Return name
    End Function

    Public Sub save(writer As BinaryWriter)
        name.speichern(writer)
        writer.Write(defaultVal)
        writer.Write(unit)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As TemplateParameter_Double
        Dim name As Multi_Lang_String = Multi_Lang_String.Einlesen(reader, version)
        Dim defaultVal As Double = reader.ReadDouble()
        Dim unit As String = reader.ReadString()
        Return New TemplateParameter_Double(name, defaultVal, unit)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "param_double "
        line &= name.exportieren() & " = "
        line &= defaultVal
        If unit <> "" Then
            line &= " unit """ & unit & """"
        End If
        writer.WriteLine(line)
    End Sub
End Class

Public Class Intervall
    Public min As Integer
    Public max As Integer
    Public _step As Integer
    Public outOfRange As OutOfRangeMode

    Private Sub New()
    End Sub

    Public Sub New(min_val As Integer, max_val As Integer, _step As Integer, min_geschlossenens_Intervall As Boolean, max_geschlossenes_Intervall As Boolean, outOfRange As OutOfRangeMode)
        Me.outOfRange = outOfRange
        If min_geschlossenens_Intervall Then
            min = min_val
        Else
            min = min_val + _step
        End If
        If max_geschlossenes_Intervall Then
            'max = max_val
            '[1 2 5] = {1 3 5}
            '[1 2 6] = {1 3 5}
            Dim max_lng As Long = ((CLng(max_val) - min) \ _step) * _step + min
            While max_lng > Integer.MaxValue
                max_lng -= _step
            End While
            max = CInt(max_lng)
        Else
            Dim max_lng As Long = ((CLng(max_val) - min) \ _step) * _step + min
            If max_lng = max_val Then
                max_lng -= _step
            End If
            While max_lng > Integer.MaxValue
                max_lng -= _step
            End While
            max = CInt(max_lng)
        End If

        Me._step = _step
    End Sub

    Public Function ist_unendlich() As Boolean
        Return min = Integer.MinValue AndAlso max = Integer.MaxValue AndAlso _step = 1
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(min)
        writer.Write(max)
        writer.Write(_step)
        writer.Write(CInt(outOfRange))
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Intervall
        Dim min As Integer = reader.ReadInt32()
        Dim max As Integer = reader.ReadInt32()
        Dim _step As Integer = reader.ReadInt32()
        Dim erg As New Intervall()
        erg.min = min
        erg.max = max
        erg._step = _step
        If version >= 20 Then
            erg.outOfRange = CType(reader.ReadInt32(), OutOfRangeMode)
        End If
        Return erg
    End Function

    Public Function fitToRange(val As Integer) As Integer
        If val < Me.min Then
            If Me.outOfRange = Intervall.OutOfRangeMode.ClipToBounds Then
                val = Me.min
            Else
                val = CInt((CLng(val) - Me.min) Mod (CLng(Me.max) - Me.min + 1) + Me.max + 1)
            End If
        End If
        If val > Me.max Then
            If Me.outOfRange = Intervall.OutOfRangeMode.ClipToBounds Then
                val = Me.max
            Else
                val = CInt((CLng(val) - Me.min) Mod (CLng(Me.max) - Me.min + 1) + Me.min)
            End If
        End If
        If val - Me.min Mod Me._step <> 0 Then
            val = CInt(((CLng(val) - CLng(Me.min) + CLng(Me._step) \ 2) \ Me._step) * Me._step + Me.min)
            If val < Me.min Then
                val = Me.min
            End If
            If val > Me.max Then
                val = Me.max
            End If
        End If
        Return val
    End Function

    Public Enum OutOfRangeMode
        ClipToBounds = 0
        Modulo = 1
    End Enum
End Class

Public Class TemplateParameter_Param
    Inherits TemplateParameter
    Public name As Multi_Lang_String
    Public options() As Multi_Lang_String
    Public oldOption As Integer

    Public Sub New(name As Multi_Lang_String, options() As Multi_Lang_String, oldOption As Integer)
        Me.name = name
        Me.options = options
        Me.oldOption = oldOption
    End Sub

    Public Overrides Function getName() As Multi_Lang_String
        Return name
    End Function

    Public Sub save(writer As BinaryWriter)
        name.speichern(writer)
        writer.Write(oldOption)
        writer.Write(options.Length)
        For i As Integer = 0 To options.Length - 1
            options(i).speichern(writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As TemplateParameter_Param
        Dim name As Multi_Lang_String = Multi_Lang_String.Einlesen(reader, version)
        Dim oldOption As Integer = -1
        If version >= 15 Then
            oldOption = reader.ReadInt32()
        End If
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Optionen darf nicht negativ sein!")
        Dim options(anzahl - 1) As Multi_Lang_String
        For i As Integer = 0 To anzahl - 1
            options(i) = Multi_Lang_String.Einlesen(reader, version)
        Next
        Return New TemplateParameter_Param(name, options, oldOption)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "param " & name.exportieren() & " = {"
        For i As Integer = 0 To options.Length - 2
            line &= options(i).exportieren() & " "
        Next
        line &= options(options.Length - 1).exportieren & "}"
        If oldOption >= 0 AndAlso oldOption < options.Length - 1 Then
            line &= " old """ & options(oldOption).get_ID & """"
        End If
        writer.WriteLine(line)
    End Sub
End Class

Public Class CompileException
    Inherits Exception

    Public art As String
    Public fehler As String
    Public fehlerOrt As String
    Public Sub New(art As String, fehler As String, fehlerOrt As String)
        Me.art = art
        Me.fehler = fehler
        Me.fehlerOrt = fehlerOrt
    End Sub

    Public Sub New(fehler As String, fehlerOrt As String)
        Me.New("Fehler", fehler, fehlerOrt)
    End Sub

End Class

Public Class CompileParentArgs

    Private sw As Integer
    Private sh As Integer
    Private sx As Integer
    Private sy As Integer

    Public list_of_parent_lines As List(Of DO_Linie)
    Public list_of_parent_ellipse As List(Of DO_Ellipse)
    Public list_of_parent_Arcs As List(Of DO_Arc)
    Public list_of_parent_bezier As List(Of DO_Bezier)
    Public Sub New()
        Me.sw = 600
        Me.sh = 600
        list_of_parent_lines = New List(Of DO_Linie)
        list_of_parent_ellipse = New List(Of DO_Ellipse)
        list_of_parent_Arcs = New List(Of DO_Arc)
        list_of_parent_bezier = New List(Of DO_Bezier)
    End Sub
    Public Sub New(p As BauteilAusDatei)
        Me.sw = p.template_compiled.getSelectionRect().Width
        Me.sh = p.template_compiled.getSelectionRect().Height
        Me.sx = p.template_compiled.getSelectionRect().X
        Me.sy = p.template_compiled.getSelectionRect().Y

        list_of_parent_lines = New List(Of DO_Linie)
        list_of_parent_ellipse = New List(Of DO_Ellipse)
        list_of_parent_Arcs = New List(Of DO_Arc)
        list_of_parent_bezier = New List(Of DO_Bezier)
        Dim grafik As DO_MultiGrafik = p.template_compiled.getGrafik()
        add_parent_grafik(grafik)
    End Sub

    Private Sub add_parent_grafik(g As DO_MultiGrafik)
        For i As Integer = 0 To g.childs.Count - 1
            If TypeOf g.childs(i) Is DO_Linie Then
                list_of_parent_lines.Add(DirectCast(g.childs(i), DO_Linie))
            ElseIf TypeOf g.childs(i) Is DO_MultiLinie Then
                list_of_parent_lines.AddRange(DirectCast(g.childs(i), DO_MultiLinie).splitToLines)
            ElseIf TypeOf g.childs(i) Is DO_Polygon Then
                If DirectCast(g.childs(i), DO_Polygon).use_fillColor_from_linestyle = False Then
                    list_of_parent_lines.AddRange(DirectCast(g.childs(i), DO_Polygon).splitToLines)
                End If
            ElseIf TypeOf g.childs(i) Is DO_Rechteck Then
                If DirectCast(g.childs(i), DO_Rechteck).use_fillColor_from_linestyle = False Then
                    list_of_parent_lines.AddRange(DirectCast(g.childs(i), DO_Rechteck).splitToLines)
                End If
            ElseIf TypeOf g.childs(i) Is DO_Ellipse Then
                If DirectCast(g.childs(i), DO_Ellipse).use_fillColor_from_linestyle = False Then
                    list_of_parent_ellipse.Add(DirectCast(g.childs(i), DO_Ellipse))
                End If
            ElseIf TypeOf g.childs(i) Is DO_Arc Then
                If DirectCast(g.childs(i), DO_Arc).use_fillColor_from_linestyle = False Then
                    list_of_parent_Arcs.Add(DirectCast(g.childs(i), DO_Arc))
                End If
            ElseIf TypeOf g.childs(i) Is DO_Bezier Then
                list_of_parent_bezier.Add(DirectCast(g.childs(i), DO_Bezier))
            ElseIf TypeOf g.childs(i) Is DO_MultiGrafik Then
                add_parent_grafik(DirectCast(g.childs(i), DO_MultiGrafik))
            End If
        Next
    End Sub

    Public Const NrChildParams As Integer = 4

    Public Shared Sub addChildParameter(params As List(Of ParamName))
        params.Add(New ParamName(ParamName.paramArt.Int, "parent.select.width"))
        params.Add(New ParamName(ParamName.paramArt.Int, "parent.select.height"))
        params.Add(New ParamName(ParamName.paramArt.Int, "parent.select.x"))
        params.Add(New ParamName(ParamName.paramArt.Int, "parent.select.y"))
    End Sub

    Public Sub addParams(vars As List(Of ParamValue))
        vars.Add(New ParamInt(sw))
        vars.Add(New ParamInt(sh))
        vars.Add(New ParamInt(sx))
        vars.Add(New ParamInt(sy))
    End Sub

    Public Shared Sub addParamsExport(paramExport As List(Of TemplateParameter))
        paramExport.Add(New TemplateParameter_Int(New Multi_Lang_String("parent.select.width"), New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), 0, ""))
        paramExport.Add(New TemplateParameter_Int(New Multi_Lang_String("parent.select.height"), New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), 0, ""))
        paramExport.Add(New TemplateParameter_Int(New Multi_Lang_String("parent.select.x"), New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), 0, ""))
        paramExport.Add(New TemplateParameter_Int(New Multi_Lang_String("parent.select.y"), New Intervall(Integer.MinValue, Integer.MaxValue, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), 0, ""))
    End Sub
End Class

Public Class ParamName
    Public art As paramArt
    Public name As String

    Public Sub New(art As paramArt, name As String)
        Me.art = art
        Me.name = name
    End Sub

    Public Enum paramArt
        Int
        Arrow
        ParamListe
        Str
    End Enum
End Class

Public Class Export_StreamWriter
    Inherits StreamWriter

    Public lastSetOrigin As PrecompiledSetOrigin

    Public parameter As List(Of TemplateParameter)
    Public extraParameterAmAnfang As Integer

    Public precompiled_template As Precompiled_Template

    Private myIndendAnzahl As Integer
    Private myIndend As String
    Public Sub New(stream As Stream, encoding As System.Text.Encoding)
        MyBase.New(stream, encoding)
        myIndend = ""
        myIndendAnzahl = 0
        extraParameterAmAnfang = 0
        precompiled_template = Nothing
    End Sub

    Public Sub increase_Indend()
        Me.increase_Indend(4)
    End Sub

    Public Sub decrease_Indend()
        Me.decrease_Indend(4)
    End Sub

    Public Sub increase_Indend(anzahl As Integer)
        myIndendAnzahl += anzahl
        updateIndend()
    End Sub

    Public Sub decrease_Indend(anzahl As Integer)
        myIndendAnzahl -= anzahl
        updateIndend()
    End Sub

    Private Sub updateIndend()
        myIndend = ""
        For i As Integer = 0 To myIndendAnzahl - 1
            myIndend &= " "
        Next
    End Sub

    Public Overrides Sub WriteLine(line As String)
        MyBase.WriteLine(myIndend & line)
    End Sub

End Class

Public Class VariableEinlesen
    Public ReadOnly art As VariableArt
    Public ReadOnly name As String

    Public Sub New(art As VariableArt, name As String)
        Me.art = art
        Me.name = name
    End Sub

    Public Function getStringArt() As String
        Select Case art
            Case VariableArt.Int_ : Return "Integer"
            Case VariableArt.String_ : Return "String"
            Case VariableArt.Boolean_ : Return "Boolean"
        End Select
        Throw New NotImplementedException("Falsche Variablen Art")
    End Function

    Public Enum VariableArt
        Int_
        String_
        Boolean_
    End Enum
End Class
