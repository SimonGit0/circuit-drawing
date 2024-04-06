Imports System.IO
Namespace Detektion
    Public Class SkillDetectSchematic

        Private Const Invalid_String = "@ÄÖÜ_INVALID_ÄÖÜ@"
        Private myNumberFormatInfo As Globalization.NumberFormatInfo

        Private myMaster As List(Of MasterElemente)
        Private myNets As List(Of Net_Skill)
        Private myInstances As List(Of Instance_Skill)

        Private mySkalierung As Single

#Region "Einlesen aus Textdatei"
        Public Sub New(lines As List(Of String))
            myNumberFormatInfo = New Globalization.NumberFormatInfo()
            myNumberFormatInfo.NumberDecimalSeparator = "."

            myMaster = New List(Of MasterElemente)
            myNets = New List(Of Net_Skill)
            myInstances = New List(Of Instance_Skill)
            Dim neuerMaster As MasterElemente = Nothing
            Dim neuesNet As Net_Skill = Nothing
            Dim neueInst As Instance_Skill = Nothing

            Dim mode As einleseModus = einleseModus.Start
            Dim version As Integer
            For i As Integer = 0 To lines.Count - 1
                Select Case mode
                    Case einleseModus.Start
                        If Not lines(i).StartsWith("skill-export: v") Then
                            Throw New Exception("Falscher Datei-Header!")
                        End If
                        Dim version_string As String = lines(i).Substring(15)
                        If Not Integer.TryParse(version_string, version) Then
                            Throw New Exception("Falscher Datei-Header!")
                        End If
                        mode = einleseModus.StartLeseMaster
                    Case einleseModus.StartLeseMaster
                        lineErwartet(lines(i), "####Master####;")
                        mode = einleseModus.LeseAlleMaster
                    Case einleseModus.LeseAlleMaster
                        If lines(i) = "####newMaster####;" Then
                            mode = einleseModus.LeseMaster
                            neuerMaster = New MasterElemente()
                        ElseIf lines(i) = "####Master End####;" Then
                            mode = einleseModus.StartLeseNets
                        Else
                            Throw New Exception("Falsche Eingabe")
                        End If
                    Case einleseModus.LeseMaster
                        If lines(i) = "####newMaster End####;" Then
                            neuerMaster.AnalysiereShapes()
                            Dim hatMasterSchon As Boolean = False
                            Dim name As String = neuerMaster.LibName & "@'~_~'@" & neuerMaster.CellName
                            For l As Integer = 0 To myMaster.Count - 1
                                If myMaster(l).LibName & "@'~_~'@" & myMaster(l).CellName = name Then
                                    hatMasterSchon = True
                                    Exit For
                                End If
                            Next
                            If Not hatMasterSchon Then
                                myMaster.Add(neuerMaster)
                            End If
                            mode = einleseModus.LeseAlleMaster
                        Else
                            leseMaster(lines(i), neuerMaster)
                        End If
                    Case einleseModus.StartLeseNets
                        lineErwartet(lines(i), "####Nets####;")
                        mode = einleseModus.LeseAlleNets
                    Case einleseModus.LeseAlleNets
                        If lines(i) = "####Net####;" Then
                            neuesNet = New Net_Skill()
                            mode = einleseModus.LeseNet
                        ElseIf lines(i) = "####Nets End####;" Then
                            mode = einleseModus.StartLeseInsts
                        Else
                            Throw New Exception("Falsche Eingabe")
                        End If
                    Case einleseModus.LeseNet
                        If lines(i) = "####Net End####;" Then
                            myNets.Add(neuesNet)
                            mode = einleseModus.LeseAlleNets
                        Else
                            leseNet(lines(i), neuesNet)
                        End If
                    Case einleseModus.StartLeseInsts
                        lineErwartet(lines(i), "####Insts####;")
                        mode = einleseModus.LeseAlleInsts
                    Case einleseModus.LeseAlleInsts
                        If lines(i) = "####Inst####;" Then
                            neueInst = New Instance_Skill()
                            mode = einleseModus.LeseInst
                        ElseIf lines(i) = "####Insts End####;" Then
                            mode = einleseModus.Fertig
                        Else
                            Throw New Exception("Falsche Eingabe")
                        End If
                    Case einleseModus.LeseInst
                        If lines(i) = "####Inst End####;" Then
                            neueInst.prüfe()
                            myInstances.Add(neueInst)
                            mode = einleseModus.LeseAlleInsts
                        Else
                            leseInst(lines(i), neueInst)
                        End If
                End Select
            Next
            If mode = einleseModus.Start Then
                Throw New Exception("Eingelesene Daten sind im falschen Format!")
            End If
            If mode <> einleseModus.Fertig Then
                Throw New Exception("'####Insts End####;' am Ende der Datei erwartet")
            End If

            'Transform der Koordinaten!!!
            'y=-y zum überführen in (y wird kleiner weiter oben)-Koordinaten
            For i As Integer = 0 To myMaster.Count - 1
                myMaster(i).transformY()
            Next
            For i As Integer = 0 To myNets.Count - 1
                myNets(i).transformY()
            Next
            For i As Integer = 0 To myInstances.Count - 1
                myInstances(i).transformY()
            Next

            'Netze zusammenfassen!
            For i As Integer = myNets.Count - 1 To 0 Step -1
                For j As Integer = i - 1 To 0 Step -1
                    If myNets(i).name = myNets(j).name Then
                        myNets(j).lines.AddRange(myNets(i).lines)
                        myNets.RemoveAt(i)
                        Exit For
                    End If
                Next
            Next
        End Sub

        Private Sub leseMaster(line As String, m As MasterElemente)
            If line.StartsWith("lib=") Then
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl an Parametern für newMaster")
                End If
                If kvp(0).Item1 <> "lib" Then Throw New Exception("Lib fehlt")
                If kvp(1).Item1 <> "name" Then Throw New Exception("Name fehlt")
                m.LibName = getString(kvp(0).Item2)
                m.CellName = getString(kvp(1).Item2)
            ElseIf line.StartsWith("shape:") Then
                line = line.Substring(6)
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count < 4 Then
                    Throw New Exception("Falsche Anzahl an Parametern für newMaster")
                End If
                If kvp(0).Item1 <> "lpp" Then Throw New Exception("lpp fehlt")
                If kvp(1).Item1 <> "objType" Then Throw New Exception("objType fehlt")
                If kvp(2).Item1 <> "bBox" Then Throw New Exception("bBox fehlt")
                If kvp(3).Item1 <> "pinName" Then Throw New Exception("pinName fehlt")
                Dim otype As String = getString(kvp(1).Item2)
                Dim shape As New MasterShape()
                Dim v As List(Of String) = getVector(kvp(0).Item2)
                If v.Count <> 2 Then Throw New Exception("Falsche Anzahl an Parametern für lpp")
                shape.layer = getString(v(0))
                shape.purpose = getString(v(1))
                shape.type = otype
                shape.bBox = getBBox(kvp(2).Item2)
                shape.pinName = getString(kvp(3).Item2)
                If otype = "polygon" OrElse otype = "line" Then
                    If kvp.Count <> 5 Then
                        Throw New Exception("Falsche Anzahl an Parametern für newMaster (" & otype & ")")
                    End If
                    If kvp(4).Item1 <> "points" Then Throw New Exception("points fehlt")
                    shape.points = getPointfs(kvp(4).Item2)
                ElseIf otype = "arc" Then
                    If kvp.Count <> 7 Then
                        Throw New Exception("Falsche Anzahl an Parametern für newMaster (" & otype & ")")
                    End If
                    If kvp(4).Item1 <> "ellipseBBox" Then Throw New Exception("ellipseBBox fehlt")
                    If kvp(5).Item1 <> "startAngle" Then Throw New Exception("startAngle fehlt")
                    If kvp(6).Item1 <> "stopAngle" Then Throw New Exception("stopAngle fehlt")
                    shape.ellipseBBox = getBBox(kvp(4).Item2)
                    shape.startAngle = getFloat(kvp(5).Item2)
                    shape.stopAngle = getFloat(kvp(6).Item2)
                End If
                m.rawShapes.Add(shape)
            Else
                Throw New Exception("Unbekannter Eintrag: " & line)
            End If
        End Sub

        Private Sub leseNet(line As String, n As Net_Skill)
            If line.StartsWith("name=") Then
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count <> 3 Then
                    Throw New Exception("Falsche Anzahl an Parametern für Net")
                End If
                If kvp(0).Item1 <> "name" Then Throw New Exception("Name fehlt")
                If kvp(1).Item1 <> "numBits" Then Throw New Exception("NumBits fehlt")
                If kvp(2).Item1 <> "numFigs" Then Throw New Exception("NumFigs fehlt")
                n.name = getNetName(getString(kvp(0).Item2))
                n.numBits = getInt(kvp(1).Item2)
                'numFigs ist egal
            ElseIf line.StartsWith("fig:") Then
                line = line.Substring(4)
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count > 5 OrElse kvp.Count < 3 Then
                    Throw New Exception("Falsche Anzahl an Parametern für Fig")
                End If
                If kvp(0).Item1 <> "objType" Then Throw New Exception("objType fehlt")
                If kvp(1).Item1 <> "bBox" Then Throw New Exception("bBox fehlt")
                If kvp(2).Item1 <> "numPoints" Then Throw New Exception("numPoints fehlt")
                Dim otype As String = getString(kvp(0).Item2)
                If otype = "line" OrElse otype = "path" Then
                    Dim nrPoints As Integer = getInt(kvp(2).Item2)
                    If nrPoints = 2 Then
                        If kvp(3).Item1 <> "p" Then Throw New Exception("p1 fehlt")
                        If kvp(4).Item1 <> "p" Then Throw New Exception("p2 fehlt")
                    End If
                    If nrPoints = 2 Then
                        Dim p1 As PointF = getPointf(kvp(3).Item2)
                        Dim p2 As PointF = getPointf(kvp(4).Item2)
                        If p1.X <> p2.X OrElse p1.Y <> p2.Y Then
                            n.lines.Add(New Line_Skill(p1, p2, otype = "path"))
                        End If
                    ElseIf nrPoints = 0 Then
                        'mache nichts!
                    Else
                        Throw New Exception("Falsche Anzahl an Punkten!")
                    End If
                End If
            Else
                Throw New Exception("Unbekannter Eintrag: " & line)
            End If
        End Sub

        Private Sub leseInst(line As String, inst As Instance_Skill)
            If line.StartsWith("lib=") Then
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count <> 8 Then
                    Throw New Exception("Falsche Anzahl an Parametern für Inst")
                End If
                If kvp(0).Item1 <> "lib" Then Throw New Exception("lib fehlt")
                If kvp(1).Item1 <> "name" Then Throw New Exception("name fehlt")
                If kvp(2).Item1 <> "text" Then Throw New Exception("text fehlt")
                If kvp(3).Item1 <> "xy" Then Throw New Exception("xy fehlt")
                If kvp(4).Item1 <> "orient" Then Throw New Exception("orient fehlt")
                If kvp(5).Item1 <> "bBox" Then Throw New Exception("bBox fehlt")
                If kvp(6).Item1 <> "numPin" Then Throw New Exception("numPin fehlt")
                If kvp(7).Item1 <> "pin" Then Throw New Exception("pin fehlt")

                Dim libName As String = getString(kvp(0).Item2)
                Dim cellName As String = getString(kvp(1).Item2)
                For i As Integer = 0 To myMaster.Count - 1
                    If myMaster(i).CellName = cellName AndAlso myMaster(i).LibName = libName Then
                        inst.master = myMaster(i)
                        Exit For
                    End If
                Next
                If inst.master Is Nothing Then
                    Throw New Exception("Es wurde kein Master für die Instanz '" & libName & "." & cellName & "' definiert.")
                End If
                inst.text = getString(kvp(2).Item2)
                inst.xy = getPointf(kvp(3).Item2)
                Dim rot As String = getString(kvp(4).Item2)
                Select Case rot
                    Case "R0"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.Normal)
                    Case "nil" 'falls nil (kein orient vorhanden), sollte es auch normal sein...
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.Normal)
                    Case "R90"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.Rot270)
                    Case "R180"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.Rot180)
                    Case "R270"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.Rot90)
                    Case "MY"
                        'WICHTIG: Bei Cadence ist MX = Spiegeln entlang der X-Achse. Also eigentlich MirrorY!!!
                        'Deshalb: MY bedeuted eigentlich MX nach der Vektorgrafik nomenklatur!
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorX)
                    Case "MYR90"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot270)
                    Case "MYR180"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot180)
                    Case "MYR270"
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot90)
                    Case "MX" 'wie MYR180
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot180)
                    Case "MXR90" 'wie MYR270
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot90)
                    Case "MXR180" 'wie MY
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorX)
                    Case "MXR270" 'wie MYR90
                        inst.orient = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot270)
                    Case Else
                        Throw New Exception("Unbekannte Drehrichtung: " & rot)
                End Select
                Dim numPin As Integer = getInt(kvp(6).Item2)
                Dim pinStr As String = getString(kvp(7).Item2)
                If pinStr <> "" AndAlso numPin = 0 Then
                    inst.isTerminal = True
                    inst.terminal_NetName = getNetName(pinStr)
                    inst.terminal_AnzeigeName = pinStr
                    inst.master.isTerminal = True
                Else
                    inst.isTerminal = False
                    inst.terminal_NetName = ""
                    inst.terminal_AnzeigeName = ""
                    If numPin <> inst.master.pins.Count Then
                        Throw New Exception("Anzahl an Pins zwischen Master und Instanz passt nicht!")
                    End If
                    ReDim inst.pinNets(numPin - 1)
                End If
            ElseIf line.StartsWith("pin:") Then
                line = line.Substring(4)
                Dim kvp As List(Of Tuple(Of String, String)) = leseKeyValuePairs(line)
                If kvp.Count <> 3 Then
                    Throw New Exception("Falsche Anzahl an Parametern für Inst.pin")
                End If
                If kvp(0).Item1 <> "name" Then Throw New Exception("name fehlt")
                If kvp(1).Item1 <> "numBits" Then Throw New Exception("numBits fehlt")
                If kvp(2).Item1 <> "net" Then Throw New Exception("net fehlt")
                Dim name As String = getString(kvp(0).Item2)
                Dim numBits As Integer = getInt(kvp(1).Item2)
                Dim net_ As String = getNetName(getString(kvp(2).Item2))
                Dim hatPin As Boolean = False
                For i As Integer = 0 To inst.master.pins.Count - 1
                    If inst.master.pins(i).name = name Then
                        If inst.pinNets(i) <> "" Then
                            Throw New Exception("Dieser Pin ist mehr als einmal definiert: " & name)
                        End If
                        inst.pinNets(i) = net_
                        hatPin = True
                        Exit For
                    End If
                Next
                If Not hatPin Then
                    Throw New Exception("Pin kann nicht zugeordnet werden: " & name)
                End If
            Else
                Throw New Exception("Unbekannter Eintrag: " & line)
            End If
        End Sub

#Region "Lese Basic Vars"
        Private Function getBBox(str As String) As RectangleF
            Dim v As List(Of String) = getVector(str)
            If v.Count <> 2 Then
                Throw New Exception("Falsche Anzahl an Koordinaten bei bBox")
            End If
            Dim ll As List(Of String) = getVector(v(0))
            Dim ur As List(Of String) = getVector(v(1))
            If ll.Count <> 2 OrElse ur.Count <> 2 Then
                Throw New Exception("Falsche Anzahl an Koordinaten bei bBox")
            End If
            Dim xmin As Single = getFloat(ll(0))
            Dim ymin As Single = getFloat(ll(1))
            Dim xmax As Single = getFloat(ur(0))
            Dim ymax As Single = getFloat(ur(1))
            Return New RectangleF(xmin, ymin, xmax - xmin, ymax - ymin)
        End Function

        Private Function getPointfs(str As String) As List(Of PointF)
            Dim vec As List(Of String) = getVector(str)
            Dim erg As New List(Of PointF)(vec.Count)
            For i As Integer = 0 To vec.Count - 1
                erg.Add(getPointf(vec(i)))
            Next
            Return erg
        End Function

        Private Function getPointf(str As String) As PointF
            Dim vec As List(Of String) = getVector(str)
            If vec.Count <> 2 Then
                Throw New Exception("Falsche Anzahl an Koordinaten bei getPointf")
            End If
            Return New PointF(getFloat(vec(0)), getFloat(vec(1)))
        End Function

        Private Function getInt(str As String) As Integer
            str = str.Trim()
            Dim erg As Integer
            If Integer.TryParse(str, erg) Then
                Return erg
            End If
            Throw New Exception("Falsche Darstellung eines integers")
        End Function

        Private Function getFloat(str As String) As Single
            str = str.Trim()
            Dim erg As Single
            If Single.TryParse(str, Globalization.NumberStyles.Any, myNumberFormatInfo, erg) Then
                Return erg
            End If
            Throw New Exception("Falsche Darstellung eines floats")
        End Function

        Private Function getVector(str As String) As List(Of String)
            str = str.Trim()
            If str.StartsWith("(") AndAlso str.EndsWith(")") Then
                str = str.Substring(1, str.Length - 2)
                Dim klammerzähler As Integer = 0
                Dim inAnführungszeichen As Boolean = False
                Dim lastString As String = ""
                Dim erg As New List(Of String)
                For i As Integer = 0 To str.Count - 1
                    If str(i) = """" Then inAnführungszeichen = Not inAnführungszeichen
                    If Not inAnführungszeichen Then
                        If str(i) = "(" Then klammerzähler += 1
                        If str(i) = ")" Then klammerzähler -= 1
                        If klammerzähler = 0 Then
                            If str(i) = " " Then
                                erg.Add(lastString.Trim())
                                lastString = ""
                            Else
                                lastString &= str(i)
                            End If
                        Else
                            lastString &= str(i)
                        End If
                    Else
                        lastString &= str(i)
                    End If
                Next
                If lastString <> "" Then
                    erg.Add(lastString)
                End If
                Return erg
            End If
            Throw New Exception("Falsche Definition eines Vektors")
        End Function

        Private Function getString(str As String) As String
            str = str.Trim()
            If str.StartsWith("""") AndAlso str.EndsWith("""") Then
                Return str.Substring(1, str.Length - 2)
            ElseIf str = "nil" Then
                Return ""
            End If
            Throw New Exception("Falsche Definition einer Zeichenkette")
        End Function

        Private Function leseKeyValuePairs(line As String) As List(Of Tuple(Of String, String))
            Dim klammerzähler As Integer = 0
            Dim inAnführungszeichen As Boolean = False
            Dim currentStr As String = ""
            Dim currentKey As String = Invalid_String
            Dim currentValue As String = Invalid_String
            Dim erg As New List(Of Tuple(Of String, String))
            For i As Integer = 0 To line.Length - 1
                If line(i) = """" Then
                    inAnführungszeichen = Not inAnführungszeichen
                End If
                If Not inAnführungszeichen Then
                    If line(i) = "(" Then klammerzähler += 1
                    If line(i) = ")" Then klammerzähler -= 1
                    If klammerzähler < 0 Then
                        Throw New Exception("Zu viele ')'")
                    End If
                    If klammerzähler = 0 Then
                        If line(i) = ";" Then
                            currentValue = currentStr
                            If currentValue <> Invalid_String AndAlso currentKey <> Invalid_String Then
                                erg.Add(New Tuple(Of String, String)(currentKey, currentValue))
                                currentKey = Invalid_String
                                currentValue = Invalid_String
                            Else
                                Throw New Exception("Falsche Definition der Werte")
                            End If
                            currentStr = ""
                        ElseIf line(i) = "=" Then
                            currentKey = currentStr
                            currentStr = ""
                        Else
                            currentStr &= line(i)
                        End If
                    Else
                        currentStr &= line(i)
                    End If
                Else
                    currentStr &= line(i)
                End If
            Next
            If currentValue <> Invalid_String OrElse currentKey <> Invalid_String OrElse currentStr <> "" Then
                Throw New Exception("';' am Ende der zeile erwartet.")
            End If
            Return erg
        End Function
#End Region

        Private Sub lineErwartet(line As String, str As String)
            If line <> str Then
                Throw New Exception("Fehler in der Datei. '" & str & "' erwartet")
            End If
        End Sub

        Private Function getNetName(pinNet As String) As String
            If pinNet.StartsWith("<*") Then
                Dim nummer As Integer = 0
                Dim nummerLength As Integer = 0
                Dim hatEnde As Boolean = False
                For i As Integer = 2 To pinNet.Length - 1
                    If pinNet(i) = ">" Then
                        hatEnde = True
                        Exit For
                    ElseIf "0123456789".Contains(pinNet(i)) Then
                        nummer = nummer * 10 + Integer.Parse(pinNet(i))
                        nummerLength += 1
                    Else
                        hatEnde = False
                        Exit For
                    End If
                Next
                If hatEnde AndAlso nummer >= 1 Then
                    pinNet = pinNet.Substring(3 + nummerLength)
                End If
            End If
            If pinNet.EndsWith(">") Then
                Dim mode As Integer = 0
                Dim istSuffix As Boolean = False
                Dim anzahlZeichen As Integer = 1
                For i As Integer = pinNet.Length - 2 To 0 Step -1
                    anzahlZeichen += 1
                    Select Case mode
                        Case 0
                            If Not "0123456789".Contains(pinNet(i)) Then
                                istSuffix = False 'Zahl erwartet!
                                Exit For
                            Else
                                mode = 1
                            End If
                        Case 1
                            If pinNet(i) = ":" Then
                                mode = 2
                            ElseIf pinNet(i) = "<" Then
                                istSuffix = True
                                Exit For
                            ElseIf Not "0123456789".Contains(pinNet(i)) Then
                                istSuffix = False 'Zahl erwartet!
                                Exit For
                            End If
                        Case 2
                            If Not "0123456789".Contains(pinNet(i)) Then
                                istSuffix = False 'Zahl erwartet!
                                Exit For
                            Else
                                mode = 3
                            End If
                        Case 3
                            If pinNet(i) = "<" Then
                                istSuffix = True
                                Exit For
                            ElseIf Not "0123456789".Contains(pinNet(i)) Then
                                istSuffix = False 'Zahl erwartet!
                                Exit For
                            End If
                    End Select
                Next
                If istSuffix Then
                    pinNet = pinNet.Substring(0, pinNet.Length - anzahlZeichen)
                End If
            End If
            Return pinNet
        End Function

        Private Enum einleseModus
            Start
            StartLeseMaster
            LeseAlleMaster
            LeseMaster
            StartLeseNets
            LeseAlleNets
            LeseNet
            StartLeseInsts
            LeseAlleInsts
            LeseInst
            Fertig
        End Enum
#End Region

#Region "Bauteile sortieren"
        Private sortVy As Integer
        Private sortVx As Integer
        Public Sub sortiereInsts(sortierung As Combobox_Sortieren.Sortieren)
            If sortierung = Combobox_Sortieren.Sortieren.LinksNachRechts_ObenNachUnten Then
                Me.sortVy = 1
                Me.sortVx = 1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_OL))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.LinksNachRechts_UntenNachOben Then
                Me.sortVy = -1
                Me.sortVx = 1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_OL))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.RechtsNachLinks_ObenNachUnten Then
                Me.sortVy = 1
                Me.sortVx = -1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_OL))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.RechtsNachLinks_UntenNachOben Then
                Me.sortVy = -1
                Me.sortVx = -1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_OL))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.ObenNachUnten_LinksNachRechts Then
                Me.sortVy = 1
                Me.sortVx = 1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_LO))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.ObenNachUnten_RechtsNachLinks Then
                Me.sortVy = 1
                Me.sortVx = -1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_LO))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.UntenNachOben_LinksNachRechts Then
                Me.sortVy = -1
                Me.sortVx = 1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_LO))
            ElseIf sortierung = Combobox_Sortieren.Sortieren.UntenNachOben_RechtsNachLinks Then
                Me.sortVy = -1
                Me.sortVx = -1
                myInstances.Sort(New Comparison(Of Instance_Skill)(AddressOf compareInsts_LO))
            End If
        End Sub

        Private Function compareInsts_LO(inst1 As Instance_Skill, inst2 As Instance_Skill) As Integer
            If inst1.getCenter().X < inst2.getCenter().X Then
                Return -sortVx
            ElseIf inst1.getCenter().X > inst2.getCenter().X Then
                Return sortVx
            Else
                If inst1.getCenter().Y < inst2.getCenter().Y Then
                    Return -sortVy
                ElseIf inst1.getCenter().Y > inst2.getCenter().Y Then
                    Return sortVy
                End If
            End If
            Return 0
        End Function

        Private Function compareInsts_OL(inst1 As Instance_Skill, inst2 As Instance_Skill) As Integer
            If inst1.getCenter().Y < inst2.getCenter().Y Then
                Return -sortVy
            ElseIf inst1.getCenter().Y > inst2.getCenter().Y Then
                Return sortVy
            Else
                If inst1.getCenter().X < inst2.getCenter().X Then
                    Return -sortVx
                ElseIf inst1.getCenter().X > inst2.getCenter().X Then
                    Return sortVx
                End If
            End If
            Return 0
        End Function

#End Region

#Region "Bauteile zuordnen"
        Public Function getBauteilZuordnungen(pfad As String, ByVal fehlendeZuordnungen As List(Of MasterElemente)) As List(Of Skill_BauteilZuordnung)
            Dim erg As New List(Of Skill_BauteilZuordnung)(myMaster.Count - 1)
            Dim hatTerminal As Boolean = False
            Dim ersterPin As MasterElemente = Nothing
            Dim einleseFehler As String = ""
            For i As Integer = 0 To myMaster.Count - 1
                If myMaster(i).isTerminal Then
                    If ersterPin Is Nothing Then ersterPin = myMaster(i)
                    If Not hatTerminal Then
                        Try
                            If File.Exists(pfad & "/pin.skill") Then
                                erg.Add(New Skill_BauteilZuordnung(pfad & "/pin.skill", Skill_BauteilZuordnung.LIBNAME_TERMINAL, ""))
                            End If
                            hatTerminal = True 'Man muss nur einmal ein Terminal einlesen!
                        Catch ex As Exception
                            einleseFehler &= vbCrLf & vbCrLf & ex.Message
                        End Try
                    End If
                Else
                    Try
                        If File.Exists(pfad & "/" & myMaster(i).LibName & "/" & myMaster(i).CellName & ".skill") Then
                            erg.Add(New Skill_BauteilZuordnung(pfad & "/" & myMaster(i).LibName & "/" & myMaster(i).CellName & ".skill", myMaster(i).LibName, myMaster(i).CellName))
                        End If
                    Catch ex As Exception
                        einleseFehler &= vbCrLf & vbCrLf & ex.Message
                    End Try
                End If
            Next
            If einleseFehler <> "" Then
                MessageBox.Show("Beim Einlesen der Bauteilzuordnungen gab es folgende Fehler:" & einleseFehler)
            End If

            'suchen welche noch fehlen!
            If ersterPin IsNot Nothing Then
                'fehlt das Terminal?
                Dim hat As Boolean = False
                For Each z As Skill_BauteilZuordnung In erg
                    If z.libName_Skill = Skill_BauteilZuordnung.LIBNAME_TERMINAL Then
                        hat = True
                        Exit For
                    End If
                Next
                If Not hat Then
                    fehlendeZuordnungen.Add(ersterPin)
                End If
            End If
            For i As Integer = 0 To myMaster.Count - 1
                'fehlt ein anderes Bauteil?
                If Not myMaster(i).isTerminal Then
                    Dim hat As Boolean = False
                    For Each z As Skill_BauteilZuordnung In erg
                        If myMaster(i).LibName = z.libName_Skill AndAlso myMaster(i).CellName = z.cellName_Skill Then
                            hat = True
                            Exit For
                        End If
                    Next
                    If Not hat Then
                        fehlendeZuordnungen.Add(myMaster(i))
                    End If
                End If
            Next
            Return erg
        End Function

        Public Sub ordneBauteileZu(bib As Bibliothek, zuOrdnungen As List(Of Skill_BauteilZuordnung))
            Dim hatGefunden As Boolean
            For i As Integer = 0 To myMaster.Count - 1
                hatGefunden = False
                If myMaster(i).isTerminal Then
                    For Each z As Skill_BauteilZuordnung In zuOrdnungen
                        If z.libName_Skill = Skill_BauteilZuordnung.LIBNAME_TERMINAL Then
                            If z.keinBauteilZugeordnet Then
                                myMaster(i).template = Nothing
                            Else
                                myMaster(i).template = New Template_Skill(bib.getBauteil(z.libName_Hier, z.cellName_Hier, "eu"))
                                myMaster(i).template.rot = z.rotation
                                myMaster(i).template.template.__lade_defaultParameterValues(myMaster(i).template.einstellungen, z.einstellungen, False, False)

                                Dim template_Compiled As Template_Compiled = Nothing
                                myMaster(i).template.template.recompile(myMaster(i).template.einstellungen, 0, template_Compiled, Nothing)
                                Dim grafik As DO_Grafik = template_Compiled.getGrafik()
                                myMaster(i).template.boundsTemplate = grafik.getBoundingBox()
                            End If
                            hatGefunden = True
                            Exit For
                        End If
                    Next
                Else
                    For Each z As Skill_BauteilZuordnung In zuOrdnungen
                        If z.libName_Skill = myMaster(i).LibName AndAlso z.cellName_Skill = myMaster(i).CellName Then
                            If z.keinBauteilZugeordnet Then
                                myMaster(i).template = Nothing
                            Else
                                myMaster(i).template = New Template_Skill(bib.getBauteil(z.libName_Hier, z.cellName_Hier, "eu"))
                                myMaster(i).template.rot = z.rotation
                                myMaster(i).template.template.__lade_defaultParameterValues(myMaster(i).template.einstellungen, z.einstellungen, False, False)

                                ordnePinsZu(myMaster(i))
                            End If
                            hatGefunden = True
                            Exit For
                        End If
                    Next
                End If
                If Not hatGefunden Then
                    Throw New Exception("Kein Template für das Bauteil '" & myMaster(i).LibName & "." & myMaster(i).CellName & "' gefunden.")
                End If
            Next
        End Sub

        Private Sub ordnePinsZu(m As MasterElemente)
            Dim template_Compiled As Template_Compiled = Nothing
            m.template.template.recompile(m.template.einstellungen, 0, template_Compiled, Nothing)
            Dim grafik As DO_Grafik = template_Compiled.getGrafik()
            Dim boundsTemplate As Rectangle = grafik.getBoundingBox()
            Dim snappoints(template_Compiled.getNrOfSnappoints() - 1) As Snappoint
            For k As Integer = 0 To snappoints.Length - 1
                snappoints(k) = template_Compiled.getSnappoint(k)
            Next
            Dim drehung As Drehmatrix = m.template.rot
            Dim pinZuordnung() As Integer = getPinZuordnung(snappoints, boundsTemplate, m, 0, drehung)
            For i As Integer = 0 To m.pins.Count - 1
                m.pins(i).pinInVektorgrafik = pinZuordnung(i)
            Next
            m.template.boundsTemplate = boundsTemplate
        End Sub

        Public Shared Function getPinZuordnung(snappoints() As Snappoint, boundsTemplate As Rectangle, b As MasterElemente, ByRef distGesamt As Single, drehung As Drehmatrix) As Integer()
            Dim pinZuordnung(b.pins.Count - 1) As Integer
            For i As Integer = 0 To pinZuordnung.Length - 1
                pinZuordnung(i) = -1
            Next

            Dim hatSchon_j(snappoints.Count - 1) As Boolean
            Dim hatSchon_i(b.pins.Count - 1) As Boolean

            distGesamt = 0.0

            For iters As Integer = 0 To b.pins.Count - 1

                Dim minDist As Single = Single.MaxValue
                Dim minIndex_i As Integer = -1
                Dim minIndex_j As Integer = -1

                For i As Integer = 0 To b.pins.Count - 1
                    If Not hatSchon_i(i) Then
                        Dim pBauteil As New PointF(CSng((b.pins(i).pos.X - b.bBox.X) / b.bBox.Width),
                                                   CSng((b.pins(i).pos.Y - b.bBox.Y) / b.bBox.Height))

                        For j As Integer = 0 To snappoints.Length - 1
                            If Not hatSchon_j(j) Then
                                Dim pSnap As New PointF(CSng((snappoints(j).p.X - boundsTemplate.X) / boundsTemplate.Width),
                                                        CSng((snappoints(j).p.Y - boundsTemplate.Y) / boundsTemplate.Height))
                                pSnap.X -= 0.5F
                                pSnap.Y -= 0.5F
                                pSnap = drehung.transformPointF(pSnap)
                                pSnap.X += 0.5F
                                pSnap.Y += 0.5F

                                Dim dist As Single = Mathe.abstand(pSnap, pBauteil)
                                If dist < minDist Then
                                    minDist = dist
                                    minIndex_j = j
                                    minIndex_i = i
                                End If
                            End If
                        Next
                    End If
                Next

                If minIndex_i <> -1 AndAlso minIndex_j <> -1 Then
                    If pinZuordnung(minIndex_i) <> -1 Then
                        Throw New Exception("Dieser Pin wurde schon zugeordnet?!")
                    End If
                    pinZuordnung(minIndex_i) = minIndex_j
                    hatSchon_i(minIndex_i) = True
                    hatSchon_j(minIndex_j) = True
                    distGesamt += minDist
                End If
            Next

            Return pinZuordnung
        End Function

        Public Function getMaster(libName As String, cellName As String) As MasterElemente
            If libName = Skill_BauteilZuordnung.LIBNAME_TERMINAL Then
                For i As Integer = 0 To myMaster.Count - 1
                    If myMaster(i).isTerminal Then
                        Return myMaster(i)
                    End If
                Next
            Else
                For i As Integer = 0 To myMaster.Count - 1
                    If myMaster(i).LibName = libName AndAlso myMaster(i).CellName = cellName Then
                        Return myMaster(i)
                    End If
                Next
            End If
            Return Nothing
        End Function
#End Region

#Region "Wires Connectivity Festlegen"
        Public Sub setWireConnections()
            For Each n As Net_Skill In myNets
                For Each inst As Instance_Skill In myInstances
                    If inst.master.template IsNot Nothing Then
                        If inst.isTerminal Then
                            If inst.terminal_NetName = n.name Then
                                'Dim p As PointF = inst.getCenter()
                                Dim box As RectangleF = inst.getPinBBox(0)
                                connectToNextLine(n, box, New Tuple(Of Instance_Skill, Integer)(inst, 0), True)
                            End If
                        Else
                            For i As Integer = 0 To inst.pinNets.Count - 1
                                If inst.pinNets(i) = n.name AndAlso inst.master.pins(i).pinInVektorgrafik <> -1 Then
                                    'Dim p As PointF = inst.getPinPos(i)
                                    Dim box As RectangleF = inst.getPinBBox(i)
                                    connectToNextLine(n, box, New Tuple(Of Instance_Skill, Integer)(inst, i), False)
                                End If
                            Next
                        End If
                    End If
                Next
            Next
        End Sub

        Private Sub connectToNextLine(n As Net_Skill, pBox As RectangleF, value As Tuple(Of Instance_Skill, Integer), istTerminal As Boolean)
            If n.lines.Count = 0 Then
                Return
            End If
            For Each line In n.lines
                If Mathe.isInRectF(line.start, pBox) Then
                    If line.connectStartTo IsNot Nothing Then
                        Throw New Exception("Es sind zwei Bauteile an der gleichen Stelle angeschlossen?")
                    End If
                    line.connectStartTo = value
                    If istTerminal Then
                        If Math.Abs(line.start.X - line.ende.X) > Math.Abs(line.start.Y - line.ende.Y) Then
                            If line.start.X < line.ende.X Then
                                'terminal wird in richtung x-plus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(1, 0.5))
                            Else
                                'terminal wird in richtung x-minus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0, 0.5))
                            End If
                        Else
                            If line.start.Y < line.ende.Y Then
                                'terminal wird in richtung y-plus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 1))
                            Else
                                'terminal wird in richtung y-minus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 0))
                            End If
                        End If
                    End If
                End If
                If Mathe.isInRectF(line.ende, pBox) Then
                    If line.connectEndeTo IsNot Nothing Then
                        Throw New Exception("Es sind zwei Bauteile an der gleichen Stelle angeschlossen?")
                    End If
                    line.connectEndeTo = value
                    If istTerminal Then
                        If Math.Abs(line.start.X - line.ende.X) > Math.Abs(line.start.Y - line.ende.Y) Then
                            If line.ende.X < line.start.X Then
                                'terminal wird in richtung x-plus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(1, 0.5))
                            Else
                                'terminal wird in richtung x-minus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0, 0.5))
                            End If
                        Else
                            If line.ende.Y < line.start.Y Then
                                'terminal wird in richtung y-plus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 1))
                            Else
                                'terminal wird in richtung y-minus angeschlossen!
                                ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 0))
                            End If
                        End If
                    End If
                End If
            Next
        End Sub

        Private Sub connectToNextLineOLD(n As Net_Skill, p As PointF, value As Tuple(Of Instance_Skill, Integer), istTerminal As Boolean)
            If n.lines.Count = 0 Then
                Return
            End If
            Dim minLine As Line_Skill = Nothing
            Dim minIstStart As Boolean
            Dim minDist As Single = Single.MaxValue
            Dim dist As Single
            For Each line In n.lines
                dist = Mathe.abstand(p, line.start)
                If dist < minDist Then
                    minDist = dist
                    minLine = line
                    minIstStart = True
                End If
                dist = Mathe.abstand(p, line.ende)
                If dist < minDist Then
                    minDist = dist
                    minLine = line
                    minIstStart = False
                End If
            Next
            If minLine Is Nothing Then
                Throw New Exception("Kein Anschluss für diesen Pin gefunden.")
            End If
            If minIstStart Then
                If minLine.connectStartTo IsNot Nothing Then
                    Throw New Exception("Es sind zwei Bauteile an der gleichen Stelle angeschlossen?")
                End If
                minLine.connectStartTo = value
                If istTerminal Then
                    If Math.Abs(minLine.start.X - minLine.ende.X) > Math.Abs(minLine.start.Y - minLine.ende.Y) Then
                        If minLine.start.X < minLine.ende.X Then
                            'terminal wird in richtung x-plus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(1, 0.5))
                        Else
                            'terminal wird in richtung x-minus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0, 0.5))
                        End If
                    Else
                        If minLine.start.Y < minLine.ende.Y Then
                            'terminal wird in richtung y-plus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 1))
                        Else
                            'terminal wird in richtung y-minus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 0))
                        End If
                    End If
                End If
            Else
                If minLine.connectEndeTo IsNot Nothing Then
                    Throw New Exception("Es sind zwei Bauteile an der gleichen Stelle angeschlossen?")
                End If
                minLine.connectEndeTo = value
                If istTerminal Then
                    If Math.Abs(minLine.start.X - minLine.ende.X) > Math.Abs(minLine.start.Y - minLine.ende.Y) Then
                        If minLine.ende.X < minLine.start.X Then
                            'terminal wird in richtung x-plus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(1, 0.5))
                        Else
                            'terminal wird in richtung x-minus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0, 0.5))
                        End If
                    Else
                        If minLine.ende.Y < minLine.start.Y Then
                            'terminal wird in richtung y-plus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 1))
                        Else
                            'terminal wird in richtung y-minus angeschlossen!
                            ordneTerminalAnschlussZu(value.Item1, New PointF(0.5, 0))
                        End If
                    End If
                End If
            End If
        End Sub

        Private Sub ordneTerminalAnschlussZu(inst As Instance_Skill, richtung As PointF)
            Dim template_Compiled As Template_Compiled = Nothing
            inst.master.template.template.recompile(inst.master.template.einstellungen, 0, template_Compiled, Nothing)
            Dim grafik As DO_Grafik = template_Compiled.getGrafik()
            Dim boundsTemplate As Rectangle = grafik.getBoundingBox()
            Dim snappoints(template_Compiled.getNrOfSnappoints() - 1) As Snappoint
            For k As Integer = 0 To snappoints.Length - 1
                snappoints(k) = template_Compiled.getSnappoint(k)
            Next

            Dim minDist As Single = Single.MaxValue
            Dim minIndex_j As Integer = -1
            Dim drehung As Drehmatrix = inst.master.template.rot
            drehung.drehen(inst.orient)

            For j As Integer = 0 To snappoints.Length - 1
                Dim pSnap As New PointF(CSng((snappoints(j).p.X - boundsTemplate.X) / boundsTemplate.Width),
                                        CSng((snappoints(j).p.Y - boundsTemplate.Y) / boundsTemplate.Height))
                pSnap.X -= 0.5F
                pSnap.Y -= 0.5F
                pSnap = drehung.transformPointF(pSnap)
                pSnap.X += 0.5F
                pSnap.Y += 0.5F

                Dim dist As Single = Mathe.abstand(pSnap, richtung)
                If dist < minDist Then
                    minDist = dist
                    minIndex_j = j
                End If
            Next

            If minIndex_j <> -1 Then
                inst.master.pins(0).pinInVektorgrafik = minIndex_j
                ReDim inst.pinNets(1)
                inst.pinNets(0) = inst.terminal_NetName
            End If
        End Sub
#End Region

#Region "Direkt Connection: Wires mit Länge 0 hinzufügen"
        Public Sub solveDirectConnections()
            For i As Integer = 0 To myInstances.Count - 1
                For j As Integer = i + 1 To myInstances.Count - 1
                    solveDirectConnections(myInstances(i), myInstances(j))
                Next
            Next
        End Sub

        Private Sub solveDirectConnections(el1 As Instance_Skill, el2 As Instance_Skill)
            If el1.isTerminal OrElse el2.isTerminal Then Return
            For i As Integer = 0 To el1.pinNets.Count - 1
                For j As Integer = 0 To el2.pinNets.Count - 1
                    solveDirectConnections_Pins(el1, i, el2, j)
                Next
            Next
        End Sub

        Private Sub solveDirectConnections_Pins(el1 As Instance_Skill, pin1 As Integer, el2 As Instance_Skill, pin2 As Integer)
            If el1.pinNets(pin1) <> el2.pinNets(pin2) Then
                'verschiedene Netze! Können nicht verbunden sein!
                Return
            End If
            'prüfe ob Pin Positionen gleich sind!
            Dim rect1 As RectangleF = el1.getPinBBox(pin1)
            Dim rect2 As RectangleF = el2.getPinBBox(pin2)
            If Mathe.rectFIneinander(rect1, rect2) Then
                'diese zwei Pins sollten wahrscheinlich verbunden sein!
                'erst aber noch prüfen ob schon andere netze hier angeschlossen sind. Nur wenn keine anderen Netze hier sind fehlt die Verbindung wirklich noch!
                Dim netname As String = el1.pinNets(pin1)
                Dim hatPin1_Anschluss As Boolean = False
                Dim hatPin2_Anschluss As Boolean = False
                For i As Integer = 0 To myNets.Count - 1
                    If myNets(i).name = netname Then
                        For Each line As Line_Skill In myNets(i).lines
                            If line.connectStartTo IsNot Nothing Then
                                If line.connectStartTo.Item1.Equals(el1) AndAlso line.connectStartTo.Item2 = pin1 Then
                                    'diese Linie wird an el1 angeschlossen!
                                    hatPin1_Anschluss = True
                                End If
                                If line.connectStartTo.Item1.Equals(el2) AndAlso line.connectStartTo.Item2 = pin2 Then
                                    hatPin2_Anschluss = True
                                End If
                            End If
                            If line.connectEndeTo IsNot Nothing Then
                                If line.connectEndeTo.Item1.Equals(el1) AndAlso line.connectEndeTo.Item2 = pin1 Then
                                    hatPin1_Anschluss = True
                                End If
                                If line.connectEndeTo.Item1.Equals(el2) AndAlso line.connectEndeTo.Item2 = pin2 Then
                                    hatPin2_Anschluss = True
                                End If
                            End If
                        Next
                    End If
                Next
                If Not (hatPin1_Anschluss AndAlso hatPin2_Anschluss) Then
                    'hier fehlt noch die Verbindung!
                    Dim pMitte1 As PointF = Mathe.getCenter(rect1)
                    Dim pMitte2 As PointF = Mathe.getCenter(rect2)
                    Dim pMitte As New PointF((pMitte1.X + pMitte2.X) * 0.5F, (pMitte1.Y + pMitte2.Y) * 0.5F)
                    For i As Integer = 0 To myNets.Count - 1
                        If myNets(i).name = netname Then
                            'Linie hinzufügen
                            Dim lneu As New Line_Skill(pMitte, pMitte, False)
                            lneu.connectStartTo = New Tuple(Of Instance_Skill, Integer)(el1, pin1)
                            lneu.connectEndeTo = New Tuple(Of Instance_Skill, Integer)(el2, pin2)
                            myNets(i).lines.Add(lneu)
                            Exit For
                        End If
                    Next
                End If
            End If
        End Sub
#End Region

#Region "Optimierungen (Löschen von Dummys, etc.)"
        Public Sub reset_unconected_objects()
            For Each inst As Instance_Skill In myInstances
                inst.OptimierungLöschen = False
            Next
            For Each n As Net_Skill In myNets
                For Each l As Line_Skill In n.lines
                    l.OptimierungLöschen = False
                Next
            Next
        End Sub
#Region "Delete Unconnected devices"
        Public Function delete_unconected_devices(deleteDevices As Boolean) As Boolean
            Dim changed As Boolean = False
            If deleteDevices Then
                For i As Integer = myInstances.Count - 1 To 0 Step -1
                    If myInstances(i).OptimierungLöschen = False AndAlso is_unconnected(myInstances(i)) Then
                        myInstances(i).OptimierungLöschen = True
                        changed = True
                    End If
                Next
            End If
            Return changed
        End Function

        Private Function is_unconnected(inst As Instance_Skill) As Boolean
            For i As Integer = 0 To inst.pinNets.Count - 1
                If Not is_unconnected(inst, i) Then Return False
            Next
            Return True
        End Function

        Private Function is_unconnected(inst As Instance_Skill, pin As Integer) As Boolean
            Dim netName As String = inst.pinNets(pin)
            For i As Integer = 0 To myNets.Count - 1
                If myNets(i).name = netName Then
                    For lineNr As Integer = 0 To myNets(i).lines.Count - 1
                        Dim line As Line_Skill = myNets(i).lines(lineNr)
                        If (line.connectStartTo IsNot Nothing AndAlso line.connectStartTo.Item1.Equals(inst) AndAlso line.connectStartTo.Item2 = pin) OrElse
                           (line.connectEndeTo IsNot Nothing AndAlso line.connectEndeTo.Item1.Equals(inst) AndAlso line.connectEndeTo.Item2 = pin) Then
                            If anzahl_connected_devices(myNets(i), lineNr) > 1 Then
                                Return False
                            End If
                        End If
                    Next
                End If
            Next
            Return True
        End Function

        Private Function anzahl_connected_devices(n As Net_Skill, lineNr As Integer) As Integer
            Dim lines As List(Of Line_Skill) = get_all_Lines_connected_to(n, lineNr)
            Dim insts As New List(Of Instance_Skill)
            For i As Integer = 0 To lines.Count - 1
                If lines(i).connectStartTo IsNot Nothing AndAlso lines(i).connectStartTo.Item1.OptimierungLöschen = False Then
                    If Not insts.Contains(lines(i).connectStartTo.Item1) Then
                        insts.Add(lines(i).connectStartTo.Item1)
                    End If
                End If
                If lines(i).connectEndeTo IsNot Nothing AndAlso lines(i).connectEndeTo.Item1.OptimierungLöschen = False Then
                    If Not insts.Contains(lines(i).connectEndeTo.Item1) Then
                        insts.Add(lines(i).connectEndeTo.Item1)
                    End If
                End If
            Next
            Return insts.Count
        End Function

        Private Function get_all_Lines_connected_to(n As Net_Skill, lineNr As Integer) As List(Of Line_Skill)
            Dim hatSchon(n.lines.Count - 1) As Boolean
            hatSchon(lineNr) = True

            Dim lines As New List(Of Line_Skill)
            lines.Add(n.lines(lineNr))
            Dim index As Integer = 0
            While index < lines.Count
                Dim start As PointF = lines(index).start
                Dim ende As PointF = lines(index).ende

                For i As Integer = 0 To n.lines.Count - 1
                    If Not hatSchon(i) Then
                        If n.lines(i).start = start OrElse n.lines(i).start = ende OrElse
                           n.lines(i).ende = start OrElse n.lines(i).ende = ende Then
                            hatSchon(i) = True
                            lines.Add(n.lines(i))
                        End If
                    End If
                Next

                index += 1
            End While
            Return lines
        End Function
#End Region

#Region "Delete Wire subts"
        Public Function delete_wireStubs(deleteWireStubs As Boolean) As Boolean
            Dim changed As Boolean = False
            If deleteWireStubs Then
                For i As Integer = 0 To myNets.Count - 1
                    While delete_wireStubs(myNets(i))
                        changed = True
                    End While
                Next
            End If
            Return changed
        End Function

        Private Function delete_wireStubs(n As Net_Skill) As Boolean
            Dim changed As Boolean = False
            For i As Integer = n.lines.Count - 1 To 0 Step -1
                If Not n.lines(i).OptimierungLöschen Then
                    If Not hatMehrAls1WireOderElementeAnPosition(n.lines(i).start, n) OrElse
                       Not hatMehrAls1WireOderElementeAnPosition(n.lines(i).ende, n) Then
                        n.lines(i).OptimierungLöschen = True
                        changed = True
                    End If
                End If
            Next
            Return changed
        End Function

        Private Function hatMehrAls1WireOderElementeAnPosition(pos As PointF, n As Net_Skill) As Boolean
            Dim anzahl As Integer = 0
            For i As Integer = 0 To n.lines.Count - 1
                If Not n.lines(i).OptimierungLöschen Then
                    If n.lines(i).start = pos Then
                        anzahl += 1
                        If n.lines(i).connectStartTo IsNot Nothing AndAlso Not n.lines(i).connectStartTo.Item1.OptimierungLöschen Then
                            anzahl += 1
                        End If
                        If anzahl > 1 Then Return True
                    End If
                    If n.lines(i).ende = pos Then
                        anzahl += 1
                        If n.lines(i).connectEndeTo IsNot Nothing AndAlso Not n.lines(i).connectEndeTo.Item1.OptimierungLöschen Then
                            anzahl += 1
                        End If
                        If anzahl > 1 Then Return True
                    End If
                End If
            Next
            Return anzahl > 1
        End Function
#End Region

#Region "Delete Dummys"
        Public Function deleteDummys(delete As Boolean) As Boolean
            Dim changed As Boolean = False
            If delete Then
                For i As Integer = 0 To myInstances.Count - 1
                    If myInstances(i).OptimierungLöschen = False AndAlso myInstances(i).pinNets.Count > 1 Then
                        Dim istDummy As Boolean = True
                        Dim name As String = myInstances(i).pinNets(0)
                        For j As Integer = 1 To myInstances(i).pinNets.Count - 1
                            If name <> myInstances(i).pinNets(j) Then
                                istDummy = False
                                Exit For
                            End If
                        Next
                        If istDummy Then
                            myInstances(i).OptimierungLöschen = True
                            changed = True
                        End If
                    End If
                Next
            End If
            Return changed
        End Function
#End Region
#End Region

#Region "Place Elements"
        Public Sub placeElements(v As Vektor_Picturebox, skalierung As Single)
            v.deleteAll()

            Me.mySkalierung = skalierung

            Dim ls_Löschen As LineStyle = v.myLineStyles.getLineStyle(0).copy()
            ls_Löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_index_löschen As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_Löschen)

            For i As Integer = 0 To myInstances.Count - 1
                If myInstances(i).master.template IsNot Nothing Then
                    Dim ls As Integer = -1
                    If myInstances(i).OptimierungLöschen Then
                        ls = ls_index_löschen
                    End If
                    If Not myInstances(i).isTerminal Then
                        addBauteil(v, myInstances(i), skalierung, ls)
                    Else
                        addTerminal(v, myInstances(i), skalierung, ls)
                    End If
                End If
            Next

            v.removeNichtBenötigteStyles()

            Dim constraints As List(Of AusrichtungsContstraint) = getConstraints()
            BauelementeAusrichten_NachPins(v, constraints)

            v.WORKAROUND_FitToScreen_AFTERLOAD()
        End Sub

        Private Sub addBauteil(v As Vektor_Picturebox, b As Instance_Skill, f As Single, ls As Integer)
            If b.master.template IsNot Nothing Then
                Dim tmpl As TemplateAusDatei = b.master.template.template
                Dim einstellungen(b.master.template.einstellungen.Length - 1) As ParamValue
                For i As Integer = 0 To einstellungen.Length - 1
                    einstellungen(i) = b.master.template.einstellungen(i).Copy()
                Next

                Dim drehung As Drehmatrix = b.master.template.rot
                drehung.drehen(b.orient)

                'DEBUG_AUSGABE: BOUNDING BOX
                'Dim box As New Rectangle(CInt(b.master.bBox.X * f), CInt(b.master.bBox.Y * f), CInt(b.master.bBox.Width * f), CInt(b.master.bBox.Height * f))
                'box = b.orient.transformRect(box)
                'box = b.master.template.rot.transformRect(box)
                'box.X += CInt(b.xy.X * f)
                'box.Y += CInt(b.xy.Y * f)
                'v.addElement(New Element_Rect(v.getNewID(), box.Location, box.Width, box.Height, 0, 0))

                Dim pos As PointF = b.getCenter()
                Dim position As Point = New Point(CInt(f * pos.X), CInt(f * pos.Y))
                Dim mitte As Point = position
                position.X -= b.master.template.boundsTemplate.X + b.master.template.boundsTemplate.Width \ 2
                position.Y -= b.master.template.boundsTemplate.Y + b.master.template.boundsTemplate.Height \ 2

                Dim beschriftung As String
                If tmpl.getDefaultNaming() <> "" Then
                    beschriftung = v.getNeuerName(tmpl.getDefaultNaming(), tmpl.getNameMitNummer())
                Else
                    beschriftung = ""
                End If

                Dim el As New BauteilAusDatei(v.getNewID(), position, tmpl, Drehmatrix.getIdentity(), New Beschriftung(beschriftung, 0, DO_Text.TextRotation.Normal, 0, 0), 0, BauteilAusDatei.DefaultFillstyle)
                If ls <> -1 Then
                    el.linestyle = ls
                End If
                el.drehe(mitte, drehung)

                'Auf Grid fitten!
                el.position = New Point(v.fitToGridX(el.position.X) * v.GridX,
                                        v.fitToGridY(el.position.Y) * v.GridY)

                el.setParams(einstellungen)
                b.BauelementInVektorgrafik = el

                v.addElement(el)
            Else
                Dim rect As New Element_Rect(v.getNewID(), New Point(CInt(b.xy.X * f - 300), CInt(b.xy.Y * f - 300)), 600, 600, 0, 0, Element_Rect.Modus.EinRect, New Point(Element_Rect.DefaultMultiRectVec_X, Element_Rect.DefaultMultiRectVec_Y), Element_Rect.DefaultMultiRectAnzahlStart)
                v.addElement(rect)
            End If
        End Sub

        Private Sub addTerminal(v As Vektor_Picturebox, b As Instance_Skill, f As Single, ls As Integer)
            If b.master.template IsNot Nothing Then
                Dim tmpl As TemplateAusDatei = b.master.template.template
                Dim einstellungen(b.master.template.einstellungen.Length - 1) As ParamValue
                For i As Integer = 0 To einstellungen.Length - 1
                    einstellungen(i) = b.master.template.einstellungen(i).Copy()
                Next

                Dim drehung As Drehmatrix = b.master.template.rot
                drehung.drehen(b.orient)

                Dim pos As PointF = b.getCenter()
                Dim position As Point = New Point(CInt(f * pos.X), CInt(f * pos.Y))
                Dim mitte As Point = position
                position.X -= b.master.template.boundsTemplate.X + b.master.template.boundsTemplate.Width \ 2
                position.Y -= b.master.template.boundsTemplate.Y + b.master.template.boundsTemplate.Height \ 2

                Dim beschriftung As String
                If tmpl.getDefaultNaming() <> "" Then
                    'beschriftung = v.getNeuerName(tmpl.getDefaultNaming(), tmpl.getNameMitNummer())
                    beschriftung = b.terminal_AnzeigeName
                Else
                    beschriftung = ""
                End If

                Dim el As New BauteilAusDatei(v.getNewID(), position, tmpl, Drehmatrix.getIdentity(), New Beschriftung(beschriftung, 0, DO_Text.TextRotation.Normal, 0, 0), 0, BauteilAusDatei.DefaultFillstyle)
                If ls <> -1 Then
                    el.linestyle = ls
                End If
                el.drehe(mitte, drehung)

                'Auf Grid fitten!
                el.position = New Point(v.fitToGridX(el.position.X) * v.GridX,
                                        v.fitToGridY(el.position.Y) * v.GridY)

                el.setParams(einstellungen)
                b.BauelementInVektorgrafik = el

                v.addElement(el)
            Else
                Dim rect As New Element_Rect(v.getNewID(), New Point(CInt(b.xy.X * f - 300), CInt(b.xy.Y * f - 300)), 600, 600, 0, 0, Element_Rect.Modus.EinRect, New Point(Element_Rect.DefaultMultiRectVec_X, Element_Rect.DefaultMultiRectVec_Y), Element_Rect.DefaultMultiRectAnzahlStart)
                v.addElement(rect)
            End If
        End Sub

        Public Sub deleteBauteile_markedToDelete(v As Vektor_Picturebox)
            Dim ls_Löschen As LineStyle = v.myLineStyles.getLineStyle(0).copy()
            ls_Löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_index_löschen As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_Löschen)

            For i As Integer = v.ElementListe.Count - 1 To 0 Step -1
                If TypeOf v.ElementListe(i) Is BauteilAusDatei Then
                    If DirectCast(v.ElementListe(i), BauteilAusDatei).linestyle = ls_index_löschen Then
                        v.ElementListe.RemoveAt(i)
                    End If
                End If
            Next
        End Sub
#End Region

#Region "Bauelemente Ausrichten nach Constraints"
        Private Sub BauelementeAusrichten_NachPins(v As Vektor_Picturebox, constraints As List(Of AusrichtungsContstraint))
            'constraints zusammenfassen
            Dim hatZusammengefasst As Boolean = True
            While hatZusammengefasst
                hatZusammengefasst = False
                For i As Integer = 0 To constraints.Count - 1
                    For j As Integer = i + 1 To constraints.Count - 1
                        Dim delta As Integer = 0
                        If kannZusammenfassen(constraints(i), constraints(j), delta) Then
                            constraints(i).add(constraints(j), delta)
                            constraints.RemoveAt(j)
                            hatZusammengefasst = True
                            Exit For
                        End If
                    Next
                    If hatZusammengefasst Then
                        Exit For
                    End If
                Next
            End While

            'reihenfolge sortieren!
            For Each c As AusrichtungsContstraint In constraints
                c.sortieren()
            Next

            'constraints anwenden
            Dim hatIDX As New List(Of ULong)
            Dim hatIDY As New List(Of ULong)
            For Each c As AusrichtungsContstraint In constraints
                If c.art = AusrichtungsContstraint.ArtConst.Xequal Then
                    Dim refPos As Integer = c.inst(0).Item1.getSnappoint(c.inst(0).Item2).p.X
                    Dim deltaRef As Integer = c.inst(0).Item3
                    hatIDX.Add(c.inst(0).Item1.ID)
                    For i As Integer = 1 To c.inst.Count - 1
                        If Not hatIDX.Contains(c.inst(i).Item1.ID) Then
                            Dim posLocal As Integer = c.inst(i).Item1.getSnappoint(c.inst(i).Item2).p.X
                            posLocal -= c.inst(i).Item3 - deltaRef 'dies ist posSoll: posSoll = posLocal - (delta - deltaRef)
                            Dim dx As Integer = refPos - posLocal
                            c.inst(i).Item1.position = New Point(c.inst(i).Item1.position.X + dx, c.inst(i).Item1.position.Y)
                            hatIDX.Add(c.inst(i).Item1.ID)
                        End If
                    Next
                Else
                    Dim refPos As Integer = c.inst(0).Item1.getSnappoint(c.inst(0).Item2).p.Y
                    Dim deltaRef As Integer = c.inst(0).Item3
                    hatIDY.Add(c.inst(0).Item1.ID)
                    For i As Integer = 1 To c.inst.Count - 1
                        If Not hatIDY.Contains(c.inst(i).Item1.ID) Then
                            Dim posLocal As Integer = c.inst(i).Item1.getSnappoint(c.inst(i).Item2).p.Y
                            posLocal -= c.inst(i).Item3 - deltaRef 'dies ist posSoll: posSoll = posLocal - (delta - deltaRef)
                            Dim dy As Integer = refPos - posLocal
                            c.inst(i).Item1.position = New Point(c.inst(i).Item1.position.X, c.inst(i).Item1.position.Y + dy)
                            hatIDY.Add(c.inst(i).Item1.ID)
                        End If
                    Next
                End If
            Next
        End Sub

        Private Function kannZusammenfassen(c1 As AusrichtungsContstraint, c2 As AusrichtungsContstraint, ByRef delta As Integer) As Boolean
            If c1.art <> c2.art Then Return False
            For i As Integer = 0 To c1.inst.Count - 1
                For j As Integer = 0 To c2.inst.Count - 1
                    If c1.inst(i).Item1.ID = c2.inst(j).Item1.ID Then
                        Dim p1 As Point = c1.inst(i).Item1.getSnappoint(c1.inst(i).Item2).p
                        Dim p2 As Point = c2.inst(j).Item1.getSnappoint(c2.inst(j).Item2).p
                        If c1.art = AusrichtungsContstraint.ArtConst.Xequal Then
                            delta = p2.X - p1.X + c1.inst(i).Item3
                        Else
                            delta = p2.Y - p1.Y + c1.inst(i).Item3
                        End If
                        Return True
                    End If
                Next
            Next
            Return False
        End Function

        Private Function getConstraints() As List(Of AusrichtungsContstraint)
            Dim erg As New List(Of AusrichtungsContstraint)
            For Each n As Net_Skill In myNets
                addConstraints(erg, n)
            Next
            Return erg
        End Function

        Private Sub addConstraints(erg As List(Of AusrichtungsContstraint), n As Net_Skill)
            Dim anschlüsse As New List(Of Tuple(Of PointF, Tuple(Of Instance_Skill, Integer)))
            For Each l As Line_Skill In n.lines
                If l.connectStartTo IsNot Nothing Then 'AndAlso Not l.connectStartTo.Item1.isTerminal Then
                    anschlüsse.Add(New Tuple(Of PointF, Tuple(Of Instance_Skill, Integer))(l.start, l.connectStartTo))
                End If
                If l.connectEndeTo IsNot Nothing Then 'AndAlso Not l.connectEndeTo.Item1.isTerminal Then
                    anschlüsse.Add(New Tuple(Of PointF, Tuple(Of Instance_Skill, Integer))(l.ende, l.connectEndeTo))
                End If
            Next
            If anschlüsse.Count >= 2 Then
                'mehr als 2 Pins verbunden in dem Netz -> es kann constraints geben!
                For i As Integer = 0 To anschlüsse.Count - 1
                    For j As Integer = i + 1 To anschlüsse.Count - 1
                        'prüfen ob zwischen i und j ein Constraint hinzugefügt werden muss!
                        If anschlüsse(i).Item1.X = anschlüsse(j).Item1.X Then
                            If direkterPfad(anschlüsse(i).Item1, anschlüsse(j).Item1, n) Then
                                Dim inst1 As Instance_Skill = anschlüsse(i).Item2.Item1
                                Dim pin1InSkill As Integer = anschlüsse(i).Item2.Item2
                                Dim inst2 As Instance_Skill = anschlüsse(j).Item2.Item1
                                Dim pin2InSkill As Integer = anschlüsse(j).Item2.Item2
                                erg.Add(New AusrichtungsContstraint(inst1.BauelementInVektorgrafik, inst1.isTerminal, inst1.master.pins(pin1InSkill).pinInVektorgrafik,
                                                                    inst2.BauelementInVektorgrafik, inst2.isTerminal, inst2.master.pins(pin2InSkill).pinInVektorgrafik, AusrichtungsContstraint.ArtConst.Xequal))
                            End If
                        ElseIf anschlüsse(i).Item1.Y = anschlüsse(j).Item1.Y Then
                            If direkterPfad(anschlüsse(i).Item1, anschlüsse(j).Item1, n) Then
                                Dim inst1 As Instance_Skill = anschlüsse(i).Item2.Item1
                                Dim pin1InSkill As Integer = anschlüsse(i).Item2.Item2
                                Dim inst2 As Instance_Skill = anschlüsse(j).Item2.Item1
                                Dim pin2InSkill As Integer = anschlüsse(j).Item2.Item2
                                erg.Add(New AusrichtungsContstraint(inst1.BauelementInVektorgrafik, inst1.isTerminal, inst1.master.pins(pin1InSkill).pinInVektorgrafik,
                                                                    inst2.BauelementInVektorgrafik, inst2.isTerminal, inst2.master.pins(pin2InSkill).pinInVektorgrafik, AusrichtungsContstraint.ArtConst.Yequal))

                            End If
                        End If
                    Next
                Next
            End If
        End Sub

        Private Function direkterPfad(p1 As PointF, p2 As PointF, n As Net_Skill) As Boolean
            Dim unitVector As PointF = New PointF(p2.X - p1.X, p2.Y - p1.Y)
            If unitVector.X = 0 AndAlso unitVector.Y = 0 Then Return True

            Dim pos As PointF = p1
            Dim hatÄnderung As Boolean = True
            While hatÄnderung
                hatÄnderung = False
                For Each l As Line_Skill In n.lines
                    If Not l.istLeereLinie() Then 'wichtig, wenn die linie eine länge von Null hat bringt sie einen nicht weiter und man ist in einer endlosschleife gefangen!!!
                        If pos = l.start Then
                            If istGleicheRichtung(unitVector, New PointF(l.ende.X - l.start.X, l.ende.Y - l.start.Y)) Then
                                pos = l.ende
                                hatÄnderung = True
                                Exit For
                            End If
                        ElseIf pos = l.ende Then
                            If istGleicheRichtung(unitVector, New PointF(l.start.X - l.ende.X, l.start.Y - l.ende.Y)) Then
                                pos = l.start
                                hatÄnderung = True
                                Exit For
                            End If
                        End If
                    End If
                Next
                If pos = p2 Then
                    Return True
                End If
            End While
            Return False
        End Function

        Private Function istGleicheRichtung(v1 As PointF, v2 As PointF) As Boolean
            Dim skalar As Double = v1.X * v2.X + v1.Y * v2.Y 'v1*v2
            Dim l1l2 As Double = Math.Sqrt((v1.X * v1.X + v1.Y * v1.Y) * (v2.X * v2.X + v2.Y * v2.Y)) '|v1|*|v2|
            If skalar = 0.0 AndAlso l1l2 = 0 Then Return True
            Dim f As Double = skalar / l1l2
            If f > 0.9999 AndAlso f < 1.0001 Then
                Return True
            Else
                Return False
            End If
        End Function
#End Region

#Region "Route"
        Public Sub routeUngefähr(v As Vektor_Picturebox)
            'Line style für Dicke Linien
            Dim ls_dicke_Linie_index As Integer = getLineStyleDickeLinie(v)

            'Line style für zu löschende Elemente (Rot)
            Dim ls_Löschen As LineStyle = v.myLineStyles.getLineStyle(0).copy()
            ls_Löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_index_löschen As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_Löschen)

            'Line style für zu löschende Dicke Elemente (Rot)
            Dim ls_dick_löschen As LineStyle = v.myLineStyles.getLineStyle(ls_dicke_Linie_index).copy()
            ls_dick_löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_dick_löschen_Index As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_dick_löschen)

            Dim skalierung As Single = Me.mySkalierung
            For i As Integer = 0 To myNets.Count - 1
                For Each line As Line_Skill In myNets(i).lines
                    Dim p1 As Point = New Point(CInt(line.start.X * skalierung), CInt(line.start.Y * skalierung))
                    Dim p2 As Point = New Point(CInt(line.ende.X * skalierung), CInt(line.ende.Y * skalierung))
                    If p1.X = p2.X OrElse p1.Y = p2.Y Then
                        Dim w As New CircuitDrawing.Wire(v.getNewID(), p1, p2)
                        If line.istDickeLinie Then
                            If line.OptimierungLöschen Then
                                w.linestyle = ls_dick_löschen_Index
                            Else
                                w.linestyle = ls_dicke_Linie_index
                            End If
                        Else
                            If line.OptimierungLöschen Then
                                w.linestyle = ls_index_löschen
                            End If
                        End If
                        v.addElement_OHNE_SIMPLIFY_WIRES(w)
                    Else
                        Dim w As New CircuitDrawing.WireLuftlinie(v.getNewID(), p1, p2)
                        If line.istDickeLinie Then
                            If line.OptimierungLöschen Then
                                w.linestyle = ls_dick_löschen_Index
                            Else
                                w.linestyle = ls_dicke_Linie_index
                            End If
                        Else
                            If line.OptimierungLöschen Then
                                w.linestyle = ls_index_löschen
                            End If
                        End If

                        v.addElement_OHNE_SIMPLIFY_WIRES(w)
                    End If
                Next
            Next
            v.simplifyWires()

            v.removeNichtBenötigteStyles()
        End Sub

        Public Sub routeDetail(v As Vektor_Picturebox)
            Dim wires As New List(Of WireMitConstraints)
            Dim wiresL As New List(Of WireLuftlinieMitConstraints)

            For i As Integer = 0 To myNets.Count - 1
                For Each line As Line_Skill In myNets(i).lines
                    If Not line.OptimierungLöschen Then
                        Dim p1 As Point = New Point(CInt(line.start.X * mySkalierung), CInt(line.start.Y * mySkalierung))
                        Dim p2 As Point = New Point(CInt(line.ende.X * mySkalierung), CInt(line.ende.Y * mySkalierung))
                        If p1.X = p2.X OrElse p1.Y = p2.Y Then
                            Dim w As New CircuitDrawing.Wire(v.getNewID(), p1, p2)
                            wires.Add(New WireMitConstraints(w))
                            If line.connectStartTo IsNot Nothing Then
                                wires(wires.Count - 1).startAlign = getNetzAlign(line.connectStartTo)
                            End If
                            If line.connectEndeTo IsNot Nothing Then
                                wires(wires.Count - 1).endeAlign = getNetzAlign(line.connectEndeTo)
                            End If
                        Else
                            wiresL.Add(New WireLuftlinieMitConstraints(p1, p2))
                        End If
                    End If
                Next
            Next

            Dim netze As New Netze(wires, wiresL)

            'Alle Wires löschen
            For i As Integer = v.ElementListe.Count - 1 To 0 Step -1
                If TypeOf v.ElementListe(i) Is CircuitDrawing.Wire Then
                    v.ElementListe.RemoveAt(i)
                ElseIf TypeOf v.ElementListe(i) Is CircuitDrawing.WireLuftlinie Then
                    v.ElementListe.RemoveAt(i)
                End If
            Next

            netze.zeichnen(v)

            v.removeNichtBenötigteStyles()
            v.simplifyWires()
            v.Invalidate()
        End Sub

        Private Function getNetzAlign(connectAt As Tuple(Of Instance_Skill, Integer)) As Netz_AlignAt
            If connectAt.Item1 IsNot Nothing AndAlso connectAt.Item1.BauelementInVektorgrafik IsNot Nothing Then
                Return New Netz_AlignAt(connectAt.Item1.BauelementInVektorgrafik, connectAt.Item1.master.pins(connectAt.Item2).pinInVektorgrafik)
            End If
            Return Nothing
        End Function

        Public Sub routeDetail2(v As Vektor_Picturebox)
            'Line style für Dicke Linien
            Dim ls_dicke_Linie_index As Integer = getLineStyleDickeLinie(v)

            Dim netze As New List(Of Routing_net)
            For Each n As Net_Skill In myNets
                Dim n2 As New Routing_net(n.name)
                n2.initFromNet(n, AddressOf myTransform)
                netze.Add(n2)
            Next

            Dim mitDEBUG_AUSGABE As Boolean = True
            Dim mitDEBUG_AUSGABE_DETAIL As Boolean = False

            Dim r As New Routing_v2(netze)
            r.fitFixpoints()

            If mitDEBUG_AUSGABE Then
                ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
            End If
            For iters As Integer = 0 To 99
                r.AlignRestToFixpoints()
                If mitDEBUG_AUSGABE_DETAIL Then
                    If iters < 10 OrElse iters Mod 10 = 0 Then
                        ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
                    End If
                End If
            Next
            If mitDEBUG_AUSGABE Then
                ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
            End If
            r.fitToGrid(v)
            r.UnMarkAllShorts()
            If r.MarkAllShorts() > 0 Then
                If mitDEBUG_AUSGABE Then
                    ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
                End If
                For i As Integer = 0 To 99
                    If Not r.solveShorts(v) Then
                        Exit For
                    Else
                        ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
                    End If
                Next
                r.UnMarkAllShorts()
                r.MarkAllShorts()
            End If
            If mitDEBUG_AUSGABE Then
                ZeichneCurrentResults(v, r, True, ls_dicke_Linie_index)
            End If
            ZeichneCurrentResults(v, r, False, ls_dicke_Linie_index)

            v.removeNichtBenötigteStyles()
            v.simplifyWires()
            v.Invalidate()
        End Sub

        Private Sub ZeichneCurrentResults(v As Vektor_Picturebox, r As Routing_v2, schräg As Boolean, ls_dick As Integer)
            'Alle Wires löschen
            For i As Integer = v.ElementListe.Count - 1 To 0 Step -1
                If TypeOf v.ElementListe(i) Is IWire Then
                    v.ElementListe.RemoveAt(i)
                End If
            Next
            r.zeichne(v, schräg, ls_dick)
            v.Invalidate()
            v.Update()
        End Sub

        Private Function myTransform(pos As PointF) As Point
            Return New Point(CInt(pos.X * mySkalierung), CInt(pos.Y * mySkalierung))
        End Function

        Private Function getLineStyleDickeLinie(v As Vektor_Picturebox) As Integer
            Dim ls As LineStyle = v.myLineStyles.getLineStyle(0).copy()
            ls.Dicke *= 4
            Return v.myLineStyles.getNumberOfNewLinestyle(ls)
        End Function
#End Region

#Region "Terminals verschieben"
        Public Sub TerminalsVerschieben(v As Vektor_Picturebox)
            Dim hatChange As Boolean
            For iters As Integer = 0 To v.ElementListe.Count - 1
                hatChange = False
                For i As Integer = 0 To v.ElementListe.Count - 1
                    If TypeOf v.ElementListe(i) Is BauteilAusDatei Then
                        With DirectCast(v.ElementListe(i), BauteilAusDatei)
                            Dim anzahlVerbunden As Integer = 0
                            Dim snappointNr As Integer = -1
                            For nr As Integer = 0 To .NrOfSnappoints() - 1
                                Dim p As Point = .getSnappoint(nr).p

                                If getWiresAnPosition(v, p).Count > 0 Then
                                    anzahlVerbunden += 1
                                    snappointNr = nr
                                End If
                                If anzahlVerbunden >= 2 Then
                                    Exit For
                                    'Abbrechen!
                                End If
                            Next
                            If anzahlVerbunden = 1 Then
                                'Element mit genau 1 Wire angeschlossen!
                                If verschiebeElementMitGenau1Wire(v, DirectCast(v.ElementListe(i), BauteilAusDatei), snappointNr) Then
                                    hatChange = True
                                    Debug.Print("Verschiebe Terminal (iters = " & iters & ")")
                                    Exit For
                                End If
                            End If
                        End With
                    End If
                Next
                If Not hatChange Then
                    Debug.Print("Keine weitere Verschiebung möglich (iters = " & iters & ")")
                    Exit For
                End If
            Next
        End Sub

        Private Function verschiebeElementMitGenau1Wire(v As Vektor_Picturebox, b As Bauteil, snap_nr As Integer) As Boolean
            Dim p1 As Point = b.getSnappoint(snap_nr).p
            Dim wires As List(Of CircuitDrawing.Wire) = getWiresAnPosition(v, p1)
            If wires.Count = 1 Then
                Dim wire1 As CircuitDrawing.Wire = wires(0)
                Dim w1_SnapStart As Boolean = wire1.getStart() = p1
                Dim p2 As Point
                If w1_SnapStart Then
                    p2 = wire1.getEnde()
                Else
                    p2 = wire1.getStart()
                End If

                wires = getWiresAnPosition(v, p2)
                If wires.Count = 2 Then 'Einmal wire1 (von wo man kommt!) und einmal das neue Wire!
                    Dim wire2 As CircuitDrawing.Wire
                    If wires(0).Equals(wire1) Then
                        wire2 = wires(1)
                    ElseIf wires(1).Equals(wire1) Then
                        wire2 = wires(0)
                    Else
                        Return False
                    End If

                    Dim w2_SnapStart As Boolean = wire2.getStart() = p2
                    Dim p3 As Point
                    If w2_SnapStart Then
                        p3 = wire2.getEnde()
                    Else
                        p3 = wire2.getStart()
                    End If

                    wires = getWiresAnPosition(v, p3)
                    If wires.Count = 2 Then 'Einmal wire1 (von wo man kommt!) und einmal das neue Wire!

                        'jetzt kann man wire2 auf Länge Null reduzieren und damit wire1 und wire3 = wires(0) zu einem machen!

                        Dim dx As Integer = wire2.vector.X
                        Dim dy As Integer = wire2.vector.Y
                        If Not w2_SnapStart Then
                            dx = -dx
                            dy = -dy
                        End If
                        If Math.Abs(dx) + Math.Abs(dy) > 0 Then
                            Dim pStartNeu As New Point(wire1.getStart().X + dx, wire1.getStart().Y + dy)
                            Dim pEndeNeu As New Point(wire1.getEnde().X + dx, wire1.getEnde().Y + dy)
                            If Not v.hatWireAnPosition(pStartNeu, pEndeNeu, Not w1_SnapStart, w1_SnapStart) Then
                                If darfVerschieben(v, b, dx, dy) Then
                                    v.deselect_All()
                                    b.isSelected = True
                                    v.moveSelectedElements(dx, dy)
                                    v.deselect_All()
                                    Return True
                                End If
                            End If
                        End If
                    ElseIf wires.Count = 1 Then 'hier hört es auf! Also ist es nur ein Stub (bzw. verbunden mit einem Bauteil). Wenn die neue Richtung besser ist kann man evtl. den bogen löschen

                        Dim snappoints As List(Of CircuitDrawing.Snappoint) = getBauteileSnappointsAnPosition(v, p3)
                        If snappoints.Count = 1 Then
                            Dim delta_wire1 As Point = New Point(p1.X - p2.X, p1.Y - p2.Y)

                            If snappoints(0).isBestRichtung(delta_wire1) Then
                                'Es ist optimal dieses Bauteil in Richtung von wire1 anzuschließen! Daher kann man sich wire2 auch sparen!
                                'jetzt kann man wire2 auf Länge Null reduzieren und damit wire1 und wire3 = wires(0) zu einem machen!


                                Dim dx As Integer = wire2.vector.X
                                Dim dy As Integer = wire2.vector.Y
                                If Not w2_SnapStart Then
                                    dx = -dx
                                    dy = -dy
                                End If
                                If Math.Abs(dx) + Math.Abs(dy) > 0 Then
                                    Dim pStartNeu As New Point(wire1.getStart().X + dx, wire1.getStart().Y + dy)
                                    Dim pEndeNeu As New Point(wire1.getEnde().X + dx, wire1.getEnde().Y + dy)
                                    If Not v.hatWireAnPosition(pStartNeu, pEndeNeu, Not w1_SnapStart, w1_SnapStart) Then
                                        If darfVerschieben(v, b, dx, dy) Then
                                            v.deselect_All()
                                            b.isSelected = True
                                            v.moveSelectedElements(dx, dy)
                                            v.deselect_All()
                                            Return True
                                        End If
                                    End If
                                End If


                            End If
                        End If

                    End If
                End If
            End If
            Return False
        End Function

        Private Function darfVerschieben(v As Vektor_Picturebox, el As Bauteil, dx As Integer, dy As Integer) As Boolean
            Dim r_ref As Rectangle = DirectCast(el.getSelection(), SelectionRect).r
            r_ref.X += dx
            r_ref.Y += dy

            For Each element As ElementMaster In v.ElementListe
                If Not element.Equals(el) Then
                    If TypeOf element Is Bauteil Then
                        Dim s As Selection = DirectCast(element, Bauteil).getSelection()
                        If TypeOf s Is SelectionRect Then
                            Dim r1 As Rectangle = DirectCast(s, SelectionRect).r
                            If Mathe.rectIneinander(r1, r_ref) Then
                                Return False
                            End If
                        End If
                    End If
                End If
            Next
            Return True
        End Function

        Private Function getWiresAnPosition(v As Vektor_Picturebox, p As Point) As List(Of CircuitDrawing.Wire)
            Dim erg As New List(Of CircuitDrawing.Wire)
            For Each e In v.ElementListe
                If TypeOf e Is CircuitDrawing.Wire Then
                    If DirectCast(e, CircuitDrawing.Wire).getStart() = p Then
                        erg.Add(DirectCast(e, CircuitDrawing.Wire))
                    ElseIf DirectCast(e, CircuitDrawing.Wire).getEnde() = p Then
                        erg.Add(DirectCast(e, CircuitDrawing.Wire))
                    End If
                End If
            Next
            Return erg
        End Function

        Private Function getBauteileSnappointsAnPosition(v As Vektor_Picturebox, p As Point) As List(Of CircuitDrawing.Snappoint)
            Dim erg As New List(Of CircuitDrawing.Snappoint)
            For Each e In v.ElementListe
                If TypeOf e Is CircuitDrawing.Bauteil Then
                    For snap_nr As Integer = 0 To DirectCast(e, Bauteil).NrOfSnappoints() - 1
                        Dim snap As Snappoint = DirectCast(e, Bauteil).getSnappoint(snap_nr)
                        If snap.p = p Then
                            erg.Add(snap)
                        End If
                    Next
                End If
            Next
            Return erg
        End Function
#End Region

    End Class

#Region "Einlese Klassen"
    Public Class Instance_Skill
        Public master As MasterElemente
        Public text As String
        Public xy As PointF
        Public orient As Drehmatrix
        Public isTerminal As Boolean
        Public terminal_NetName As String
        Public terminal_AnzeigeName As String
        Public pinNets() As String 'zuordnung des i-ten Pins mit dem dazugehörigen Netz

        Public BauelementInVektorgrafik As BauteilAusDatei
        Public OptimierungLöschen As Boolean = False

        Public Sub prüfe()
            If isTerminal Then
                'keine Tests
                If pinNets Is Nothing Then
                    ReDim pinNets(-1)
                End If
            Else
                If pinNets.Length <> master.pins.Count Then
                    Throw New Exception("Falsche Pinanzahl in Prüfung festgestellt")
                End If
                For i As Integer = 0 To pinNets.Length - 1
                    If pinNets(i) = "" Then
                        Throw New Exception("Net ohne Name detektiert (nicht zugeordnet?)")
                    End If
                Next
            End If
        End Sub

        Public Sub transformY()
            xy.Y *= -1
        End Sub

        Public Function getCenter() As PointF
            Dim pos As New PointF(master.bBox.X + master.bBox.Width / 2, master.bBox.Y + master.bBox.Height / 2)
            pos = orient.transformPointF(pos) 'nur um b.orient drehen, da die Boundingbox ja schon in Cadence Maßstab gedreht ist (Drehung Circuit Drawing nach Cadence ist nicht relevant!)

            pos.X += xy.X
            pos.Y += xy.Y
            Return pos
        End Function

        Public Function getPinPos(pinNr As Integer) As PointF
            Dim pos As PointF = master.pins(pinNr).pos
            pos = orient.transformPointF(pos)

            pos.X += xy.X
            pos.Y += xy.Y
            Return pos
        End Function

        Public Function getPinBBox(pinNr As Integer) As RectangleF
            Dim bBox As RectangleF = master.pins(pinNr).bBox
            bBox = orient.transformRectF(bBox)

            bBox.X += xy.X
            bBox.Y += xy.Y
            Return bBox
        End Function
    End Class

    Public Class Net_Skill
        Public name As String
        Public numBits As Integer
        Public lines As List(Of Line_Skill)
        Public Sub New()
            name = ""
            numBits = 0
            lines = New List(Of Line_Skill)
        End Sub

        Public Sub transformY()
            For i As Integer = 0 To lines.Count - 1
                lines(i).transformY()
            Next
        End Sub
    End Class

    Public Class Line_Skill
        Public start As PointF
        Public ende As PointF
        Public connectStartTo As Tuple(Of Instance_Skill, Integer)
        Public connectEndeTo As Tuple(Of Instance_Skill, Integer)
        Public OptimierungLöschen As Boolean
        Public istDickeLinie As Boolean

        Public Sub New(p1 As PointF, p2 As PointF, dickeLinie As Boolean)
            start = p1
            ende = p2
            Me.istDickeLinie = dickeLinie
            connectEndeTo = Nothing
            connectStartTo = Nothing
            Me.OptimierungLöschen = False
        End Sub
        Public Sub transformY()
            start.Y *= -1
            ende.Y *= -1
        End Sub

        Public Function istLeereLinie() As Boolean
            Return start.X = ende.X AndAlso start.Y = ende.Y
        End Function
    End Class

    Public Class MasterElemente
        Public LibName As String
        Public CellName As String
        Public rawShapes As List(Of MasterShape)
        Public pins As List(Of MasterPins)
        Public bBox As RectangleF
        Public isTerminal As Boolean

        Public template As Template_Skill

        Public Sub New()
            LibName = ""
            CellName = ""
            rawShapes = New List(Of MasterShape)
            isTerminal = False
        End Sub

        Public Sub AnalysiereShapes()
            For i As Integer = 0 To rawShapes.Count - 1
                If rawShapes(i).purpose.StartsWith("drawing") Then
                    rawShapes(i).purpose = "drawing"
                End If
                If rawShapes(i).layer = "annotate" Then
                    rawShapes(i).layer = "device"
                End If
            Next
            bBox = New Rectangle(0, 0, 0, 0)
            pins = New List(Of MasterPins)
            For i As Integer = 0 To rawShapes.Count - 1
                If rawShapes(i).layer = "instance" AndAlso rawShapes(i).purpose = "drawing" Then
                    'bBox = Mathe.Union(bBox, rawShapes(i).bBox)
                ElseIf rawShapes(i).layer = "device" AndAlso rawShapes(i).purpose = "drawing" Then
                    bBox = Mathe.Union(bBox, rawShapes(i).bBox)
                ElseIf rawShapes(i).layer = "pin" AndAlso rawShapes(i).purpose = "drawing" AndAlso rawShapes(i).pinName <> "" Then
                    Dim p As New MasterPins()
                    p.name = rawShapes(i).pinName
                    p.bBox = rawShapes(i).bBox
                    p.pos = Mathe.getCenter(rawShapes(i).bBox)
                    pins.Add(p)
                End If
            Next
        End Sub

        Public Sub transformY()
            For i As Integer = 0 To rawShapes.Count - 1
                rawShapes(i).transformY()
            Next
            For i As Integer = 0 To pins.Count - 1
                pins(i).transformY()
            Next
            bBox = Mathe.MirrorY_Rect(bBox)
        End Sub

        Public Function getBitmap(w As Integer, h As Integer, pDrawing As Pen, ohnePins As Boolean) As Bitmap
            Dim erg As New Bitmap(w, h)
            Dim bbox As RectangleF = Me.bBox
            If bbox.Width = 0 Then bbox.Width = 0.4
            If bbox.Height = 0 Then bbox.Height = 0.4
            Dim f As Single = 0.8F * Math.Min(w / bbox.Width, h / bbox.Height)
            Dim ox As Single = w * 0.5F - f * (bbox.Location.X + bbox.Width * 0.5F)
            Dim oy As Single = h * 0.5F - f * (bbox.Location.Y + bbox.Height * 0.5F)

            Using g As Graphics = Graphics.FromImage(erg)
                g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                g.Clear(Color.Transparent)
                g.CompositingMode = Drawing2D.CompositingMode.SourceOver
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

                'g.DrawRectangle(pRahmen, bbox.X * f + ox, bbox.Y * f + oy, bbox.Width * f, bbox.Height * f)

                For Each shape As MasterShape In rawShapes
                    If (shape.layer = "device" AndAlso shape.purpose = "drawing") OrElse (shape.layer = "pin" AndAlso shape.purpose = "drawing" AndAlso Not ohnePins) Then
                        Dim p As Pen = pDrawing
                        Dim b As Brush = Brushes.Red
                        Dim fill As Boolean = False
                        If shape.layer = "pin" Then
                            fill = True
                        End If
                        Select Case shape.type
                            Case "rect"
                                If fill Then
                                    g.FillRectangle(b, f * shape.bBox.X + ox, f * shape.bBox.Y + oy, f * shape.bBox.Width, f * shape.bBox.Height)
                                Else
                                    g.DrawRectangle(p, f * shape.bBox.X + ox, f * shape.bBox.Y + oy, f * shape.bBox.Width, f * shape.bBox.Height)
                                End If
                            Case "ellipse"
                                If fill Then
                                    g.FillEllipse(b, f * shape.bBox.X + ox, f * shape.bBox.Y + oy, f * shape.bBox.Width, f * shape.bBox.Height)
                                Else
                                    g.DrawEllipse(p, f * shape.bBox.X + ox, f * shape.bBox.Y + oy, f * shape.bBox.Width, f * shape.bBox.Height)
                                End If
                            Case "polygon"
                                Dim points(shape.points.Count - 1) As PointF
                                For i As Integer = 0 To shape.points.Count - 1
                                    points(i) = New PointF(f * shape.points(i).X + ox, f * shape.points(i).Y + oy)
                                Next
                                If fill Then
                                    g.FillPolygon(b, points)
                                Else
                                    g.DrawPolygon(p, points)
                                End If
                            Case "line"
                                Dim points(shape.points.Count - 1) As PointF
                                For i As Integer = 0 To shape.points.Count - 1
                                    points(i) = New PointF(f * shape.points(i).X + ox, f * shape.points(i).Y + oy)
                                Next
                                If Not fill Then
                                    g.DrawLines(p, points)
                                End If
                            Case "arc"
                                If Not fill Then
                                    g.DrawArc(p, New RectangleF(f * shape.ellipseBBox.X + ox, f * shape.ellipseBBox.Y + oy, f * shape.ellipseBBox.Width, f * shape.ellipseBBox.Height), CSng(180.0F / Math.PI * shape.startAngle), CSng(180.0F / Math.PI * (shape.stopAngle - shape.startAngle)))
                                End If
                        End Select
                    End If
                Next
            End Using
            Return erg
        End Function
    End Class

    Public Class MasterShape
        Public layer As String
        Public purpose As String
        Public type As String
        Public bBox As RectangleF
        Public pinName As String

        Public points As List(Of PointF)
        Public ellipseBBox As RectangleF
        Public startAngle As Single
        Public stopAngle As Single

        Public Sub transformY()
            bBox = Mathe.MirrorY_Rect(bBox)
            ellipseBBox = Mathe.MirrorY_Rect(ellipseBBox)
            If points IsNot Nothing Then
                For i As Integer = 0 To points.Count - 1
                    points(i) = New PointF(points(i).X, -points(i).Y)
                Next
            End If
            startAngle = -startAngle
            stopAngle = -stopAngle
        End Sub
    End Class

    Public Class MasterPins
        Public name As String
        Public pos As PointF
        Public bBox As RectangleF
        Public pinInVektorgrafik As Integer
        Public Sub New()
            name = ""
            pos = New PointF(0, 0)
            pinInVektorgrafik = -1
        End Sub

        Public Sub transformY()
            pos.Y *= -1
            bBox = Mathe.MirrorY_Rect(bBox)
        End Sub
    End Class
#End Region

    Public Class Template_Skill
        Public template As TemplateAusDatei
        Public rot As Drehmatrix
        Public einstellungen() As ParamValue
        Public boundsTemplate As Rectangle

        Public Sub New(t As TemplateAusDatei)
            Me.template = t
            einstellungen = Me.template.getDefaultParameters_copy()
        End Sub

    End Class

    Public Class AusrichtungsContstraint
        Public inst As List(Of Tuple(Of BauteilAusDatei, Integer, Integer, Boolean)) 'instance + pinNr + delta + isTerminal
        Public art As ArtConst

        Public Sub New(inst1 As BauteilAusDatei, isTerminal1 As Boolean, inst1PinNr As Integer, inst2 As BauteilAusDatei, isTerminal2 As Boolean, inst2PinNr As Integer, art As ArtConst)
            Me.inst = New List(Of Tuple(Of BauteilAusDatei, Integer, Integer, Boolean))(2)
            inst.Add(New Tuple(Of BauteilAusDatei, Integer, Integer, Boolean)(inst1, inst1PinNr, 0, isTerminal1))
            inst.Add(New Tuple(Of BauteilAusDatei, Integer, Integer, Boolean)(inst2, inst2PinNr, 0, isTerminal2))
            Me.art = art
        End Sub

        Public Sub add(c2 As AusrichtungsContstraint, delta As Integer)
            If Me.art <> c2.art Then
                Throw New Exception("Diese Constraints kann man nicht zusammenfassen!")
            End If
            For i As Integer = 0 To c2.inst.Count - 1
                Me.inst.Add(New Tuple(Of BauteilAusDatei, Integer, Integer, Boolean)(c2.inst(i).Item1, c2.inst(i).Item2, c2.inst(i).Item3 + delta, c2.inst(i).Item4))
            Next
        End Sub

        Public Sub sortieren()
            Dim maxPins As Integer = 0
            Dim maxIndex As Integer = -1
            For i As Integer = 0 To inst.Count - 1
                Dim pins As Integer
                If inst(i).Item4 Then 'isterminal
                    pins = -1 'Terminals haben immer die niedrigste Prio, da sie am einfachsten hin und her geschoben werden können
                Else
                    pins = inst(i).Item1.NrOfSnappoints()
                End If
                If pins > maxPins Then
                    maxPins = pins
                    maxIndex = i
                End If
            Next
            If maxIndex <> -1 Then
                Dim inst0 As Tuple(Of BauteilAusDatei, Integer, Integer, Boolean) = inst(maxIndex)
                inst.RemoveAt(maxIndex)
                inst.Insert(0, inst0)
            End If
        End Sub

        Public Enum ArtConst
            Xequal
            Yequal
        End Enum
    End Class

    Public Class Skill_BauteilZuordnung
        Public Const LIBNAME_TERMINAL As String = "BUHR_ÖÜÄ_@@@TERMINAL@@@ÄÖÜ$%&/()[]{}=?\\//ÖÄÜ" 'String der als Libname normalerweise nicht vorkommt ;)
        Public libName_Skill As String
        Public cellName_Skill As String
        Public libName_Hier As String
        Public cellName_Hier As String
        Public einstellungen() As default_Parameter
        Public rotation As Drehmatrix
        Public keinBauteilZugeordnet As Boolean

        Public Sub New(libName_Skill As String, cellName_Skill As String, libName_Hier As String, cellName_Hier As String, einstellungen() As default_Parameter, rotation As Drehmatrix, keinBauteil_zuordnen As Boolean)
            Me.libName_Skill = libName_Skill
            Me.cellName_Skill = cellName_Skill
            Me.libName_Hier = libName_Hier
            Me.cellName_Hier = cellName_Hier
            Me.einstellungen = einstellungen
            Me.rotation = rotation
            Me.keinBauteilZugeordnet = keinBauteil_zuordnen
        End Sub

        Public Sub New(ladeVonDatei As String, libNameSkill As String, cellNameSkill As String)
            Me.libName_Skill = libNameSkill
            Me.cellName_Skill = cellNameSkill
            Me.keinBauteilZugeordnet = False

            Dim mode As EinleseModus = EinleseModus.IDLE
            Dim reader As StreamReader = Nothing

            Dim einstellungenList As New List(Of default_Parameter)
            Try
                reader = New StreamReader(ladeVonDatei, Text.Encoding.UTF8)
                While Not reader.EndOfStream()
                    Dim line As String = reader.ReadLine().Trim()
                    If line <> "" Then
                        Dim lineLarge As String = line
                        line = line.ToLower()
                        Select Case mode
                            Case EinleseModus.IDLE
                                If line = "info:" Then
                                    mode = EinleseModus.INFO
                                ElseIf line = "params:" Then
                                    mode = EinleseModus.PARAMS
                                Else
                                    Throw New Exception("Falscher Befehl: " & line)
                                End If
                            Case EinleseModus.INFO
                                If line = "info end" Then
                                    mode = EinleseModus.IDLE
                                Else
                                    leseInfo(line, lineLarge)
                                End If
                            Case EinleseModus.PARAMS
                                If line = "params end" Then
                                    mode = EinleseModus.IDLE
                                Else
                                    leseParam(line, lineLarge, einstellungenList)
                                End If
                        End Select
                    End If
                End While
            Catch ex As Exception
                Throw New Exception("Fehler beim Einlesen der Zuordnung '" & libNameSkill & "/" & cellNameSkill & "': " & ex.Message)
            Finally
                If reader IsNot Nothing Then
                    reader.Close()
                    reader.Dispose()
                End If
            End Try

            Me.einstellungen = einstellungenList.ToArray()
        End Sub

        Private Sub leseInfo(line As String, lineLarge As String)
            If line.StartsWith("name(") AndAlso line.EndsWith(")") Then
                lineLarge = lineLarge.Substring(5)
                lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
                Me.cellName_Hier = lineLarge.Trim()
            ElseIf line.StartsWith("namespace(") AndAlso line.EndsWith(")") Then
                lineLarge = lineLarge.Substring(10)
                lineLarge = lineLarge.Substring(0, lineLarge.Length - 1)
                Me.libName_Hier = lineLarge.Trim()
            ElseIf line.StartsWith("rot(") AndAlso line.EndsWith(")") Then
                line = line.Substring(4)
                line = line.Substring(0, line.Length - 1).Trim()
                Select Case line
                    Case "r0"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.Normal)
                    Case "r90"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.Rot90)
                    Case "r180"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.Rot180)
                    Case "r270"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.Rot270)
                    Case "mx0"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.MirrorX)
                    Case "mx90"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot90)
                    Case "mx180"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot180)
                    Case "mx270"
                        Me.rotation = New Drehmatrix(Drehmatrix.Drehungen.MirrorXRot270)
                    Case Else
                        Throw New Exception("Falsche Drehrichtung '" & line & "'")
                End Select
            ElseIf line.StartsWith("placenothing(") AndAlso line.EndsWith(")") Then
                line = line.Substring(13)
                line = line.Substring(0, line.Length - 1).Trim()
                If line <> "" Then
                    Throw New Exception("Falsche Anzahl an Parametern für 'placeNothing()'")
                End If
                Me.keinBauteilZugeordnet = True
            Else
                Throw New Exception("Unbekannter Befehl: " & lineLarge)
            End If
        End Sub

        Private Sub leseParam(line As String, lineLarge As String, einstellungenList As List(Of default_Parameter))
            Einstellungspanel_Bauelemente.readParameter(lineLarge, einstellungenList)
        End Sub

        Public Enum EinleseModus
            IDLE
            INFO
            PARAMS
        End Enum

        Public Sub speichern(speicherpfad As String)
            Dim pfad As String
            If Me.libName_Skill = LIBNAME_TERMINAL Then
                pfad = speicherpfad & "/" & "pin.skill"
            Else
                pfad = speicherpfad & "/" & libName_Skill & "/" & cellName_Skill & ".skill"
                If Not Directory.Exists(speicherpfad & "/" & libName_Skill) Then
                    Directory.CreateDirectory(speicherpfad & "/" & libName_Skill)
                End If
            End If
            Dim outStream As FileStream = Nothing
            Try
                outStream = New FileStream(pfad, FileMode.Create, FileAccess.Write)
                Dim writer = New StreamWriter(outStream, System.Text.Encoding.UTF8)
                writer.WriteLine("Info:")
                If keinBauteilZugeordnet Then
                    writer.WriteLine("placeNothing()")
                Else
                    writer.WriteLine("namespace(" & Me.libName_Hier & ")")
                    writer.WriteLine("name(" & Me.cellName_Hier & ")")
                    writer.WriteLine("rot(" & getRotString(Me.rotation) & ")")
                End If
                writer.WriteLine("Info End")
                If Me.einstellungen.Count > 0 Then
                    writer.WriteLine("Params:")
                    For i As Integer = 0 To Me.einstellungen.Count - 1
                        writer.WriteLine(Me.einstellungen(i).param & " = " & Me.einstellungen(i).value)
                    Next
                    writer.WriteLine("Params End")
                End If
                writer.Flush()
            Catch ex As Exception
                MessageBox.Show("Fehler beim Speichern der Datei: " & pfad)
            Finally
                If outStream IsNot Nothing Then
                    outStream.Close()
                    outStream.Dispose()
                End If
            End Try
        End Sub

        Private Function getRotString(rot As Drehmatrix) As String
            Select Case rot.getDrehung()
                Case Drehmatrix.Drehungen.Normal
                    Return "R0"
                Case Drehmatrix.Drehungen.Rot90
                    Return "R90"
                Case Drehmatrix.Drehungen.Rot180
                    Return "R180"
                Case Drehmatrix.Drehungen.Rot270
                    Return "R270"
                Case Drehmatrix.Drehungen.MirrorX
                    Return "MX0"
                Case Drehmatrix.Drehungen.MirrorXRot90
                    Return "MX90"
                Case Drehmatrix.Drehungen.MirrorXRot180
                    Return "MX180"
                Case Drehmatrix.Drehungen.MirrorXRot270
                    Return "MX270"
            End Select
            Throw New NotImplementedException("Falsche Drehrichtung!")
        End Function

    End Class
End Namespace
