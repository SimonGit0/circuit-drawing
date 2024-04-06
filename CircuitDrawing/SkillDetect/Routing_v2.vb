Namespace Detektion
    Public Class Routing_v2
        Private myNets As List(Of Routing_net)
        Public Sub New(nets As List(Of Routing_net))
            Me.myNets = nets
        End Sub

#Region "Fit Fixpoints"
        Public Sub fitFixpoints()
            For Each n As Routing_net In myNets
                fitFixpoints(n)
            Next
        End Sub

        Private Sub fitFixpoints(n As Routing_net)
            For Each p As Routing_point In n.points
                If p.isFixpunkt Then
                    p.pos = p.pos_sollFixpunkt
                End If
            Next
        End Sub

        Public Sub AlignRestToFixpoints()
            For Each n As Routing_net In myNets
                AlignRestToFixpoints(n)
            Next
        End Sub

        Public Sub AlignRestToFixpoints(n As Routing_net)
            For Each p As Routing_point In n.points
                If Not p.isFixpunkt Then
                    Dim maxStufeX As Integer = -1
                    Dim anzahlKeineStufeX As Integer = 0
                    Dim maxStufeY As Integer = -1
                    Dim anzahlKeineStufeY As Integer = 0
                    For Each g As Tuple(Of Routing_point, Routing_glue) In p.nachbarn
                        If g.Item2.mymode = Routing_glue.Mode.XGleich OrElse g.Item2.mymode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                            If g.Item1.only_Ein_Constraint_Stufe_X <> -1 Then
                                maxStufeX = Math.Max(maxStufeX, g.Item1.only_Ein_Constraint_Stufe_X)
                            Else
                                anzahlKeineStufeX += 1
                            End If
                        End If
                        If g.Item2.mymode = Routing_glue.Mode.YGleich OrElse g.Item2.mymode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                            If g.Item1.only_Ein_Constraint_Stufe_Y <> -1 Then
                                maxStufeY = Math.Max(maxStufeY, g.Item1.only_Ein_Constraint_Stufe_Y)
                            Else
                                anzahlKeineStufeY += 1
                            End If
                        End If
                    Next
                    If anzahlKeineStufeX = 1 Then
                        'nur ein Knoten ist hier verbunden, der diesen Beeinflusst!
                        p.only_Ein_Constraint_Stufe_X = maxStufeX + 1
                    End If
                    If anzahlKeineStufeY = 1 Then
                        'nur ein Knoten ist hier verbunden, der diesen Beeinflusst!
                        p.only_Ein_Constraint_Stufe_Y = maxStufeY + 1
                    End If

                    Dim xNeu As Double = 0
                    Dim xNeuSumme As Integer = 0
                    Dim yNeu As Double = 0
                    Dim yNeuSumme As Integer = 0
                    For Each g As Tuple(Of Routing_point, Routing_glue) In p.nachbarn
                        If g.Item2.mymode = Routing_glue.Mode.XGleich OrElse g.Item2.mymode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                            If g.Item1.only_Ein_Constraint_Stufe_X = -1 OrElse (g.Item1.only_Ein_Constraint_Stufe_X > p.only_Ein_Constraint_Stufe_X AndAlso p.only_Ein_Constraint_Stufe_X <> -1) Then
                                xNeu += g.Item1.pos.X
                                xNeuSumme += 1
                            End If
                        End If
                        If g.Item2.mymode = Routing_glue.Mode.YGleich OrElse g.Item2.mymode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                            If g.Item1.only_Ein_Constraint_Stufe_Y = -1 OrElse (g.Item1.only_Ein_Constraint_Stufe_Y > p.only_Ein_Constraint_Stufe_Y AndAlso p.only_Ein_Constraint_Stufe_Y <> -1) Then
                                yNeu += g.Item1.pos.Y
                                yNeuSumme += 1
                            End If
                        End If
                    Next
                    If xNeuSumme > 0 Then
                        p.pos.X = CInt(xNeu / xNeuSumme)
                    End If
                    If yNeuSumme > 0 Then
                        p.pos.Y = CInt(yNeu / yNeuSumme)
                    End If
                End If
            Next
        End Sub
#End Region

#Region "FitToGrid"
        Public Sub fitToGrid(v As Vektor_Picturebox)
            For Each n As Routing_net In myNets
                fitToGrid(v, n)
            Next
        End Sub

        Private Sub fitToGrid(v As Vektor_Picturebox, n As Routing_net)
            For Each p As Routing_point In n.points
                If Not p.isFixpunkt Then
                    p.pos = v.fitPointToGrid(p.pos)
                End If
            Next
        End Sub
#End Region

#Region "Solve Shorts"
        Public Sub UnMarkAllShorts()
            For i As Integer = 0 To myNets.Count - 1
                For Each line As Tuple(Of Routing_point, Routing_point, Routing_glue) In myNets(i).glue
                    line.Item3.istShort = False
                Next
            Next
        End Sub

        Public Function MarkAllShorts() As Integer
            Dim anzahlShorts As Integer = 0
            Dim shorts As New List(Of Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue)))
            For i As Integer = 0 To myNets.Count - 1
                For j As Integer = i + 1 To myNets.Count - 1
                    shorts.Clear()
                    shorts.AddRange(findShorts(myNets(i), myNets(j)))
                    For Each shrt In shorts
                        If Me.getMode(shrt.Item1) <> Me.getMode(shrt.Item2) Then
                            shrt.Item1.Item3.istShort = True
                        Else
                            Dim länge1 As Integer = Math.Abs(shrt.Item1.Item1.pos.X - shrt.Item1.Item2.pos.X) + Math.Abs(shrt.Item1.Item1.pos.Y - shrt.Item1.Item2.pos.Y)
                            Dim länge2 As Integer = Math.Abs(shrt.Item2.Item1.pos.X - shrt.Item2.Item2.pos.X) + Math.Abs(shrt.Item2.Item1.pos.Y - shrt.Item2.Item2.pos.Y)
                            If länge1 < länge2 Then
                                shrt.Item1.Item3.istShort = True
                            Else
                                shrt.Item2.Item3.istShort = True
                            End If
                        End If
                    Next
                    anzahlShorts += shorts.Count
                Next
            Next
            Return anzahlShorts
        End Function

        Public Function solveShorts(v As Vektor_Picturebox) As Boolean
            Dim shorts As New List(Of Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue)))
            Dim solved As Boolean = False
            For i As Integer = 0 To myNets.Count - 1
                For j As Integer = i + 1 To myNets.Count - 1
                    shorts.Clear()
                    shorts.AddRange(findShorts(myNets(i), myNets(j)))
                    For Each shrt In shorts
                        If solveShort(v, shrt) Then
                            solved = True
                            Return True
                        End If
                    Next
                Next
            Next
            Return solved
        End Function

#Region "Solve short"
        Private Function solveShort(v As Vektor_Picturebox, shrt As Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue))) As Boolean
            Dim line1 As Tuple(Of Routing_point, Routing_point, Routing_glue) = shrt.Item1
            Dim line2 As Tuple(Of Routing_point, Routing_point, Routing_glue) = shrt.Item2

            Dim mode1 As Routing_glue.Mode = getMode(line1)
            Dim mode2 As Routing_glue.Mode = getMode(line2)

            Dim shortType2 As Boolean = False
            If mode1 <> mode2 Then
                shortType2 = True
                'immer nur die erste Linie verschieben!
            End If

            If mode1 = Routing_glue.Mode.Frei Then
                Return False
                'Throw New NotImplementedException("Shorts von freien Verbindern können (noch) nicht aufgelöst werden.")
            End If

            If Not shortType2 Then
                'Bei short type1 noch auswählen welches Wire verschoben wird!
                Dim anzahlMode_l1 As Integer = 0
                Dim anzahlFixpunkte1 As Integer = 0
                If line1.Item1.isFixpunkt Then
                    anzahlFixpunkte1 += 1
                End If
                If line1.Item2.isFixpunkt Then
                    anzahlFixpunkte1 += 1
                End If
                For Each g As Tuple(Of Routing_point, Routing_glue) In line1.Item1.nachbarn
                    If getMode(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item1, g.Item1, g.Item2)) = mode1 Then
                        anzahlMode_l1 += 1
                    End If
                Next
                For Each g As Tuple(Of Routing_point, Routing_glue) In line1.Item2.nachbarn
                    If getMode(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item2, g.Item1, g.Item2)) = mode1 Then
                        anzahlMode_l1 += 1
                    End If
                Next
                Dim anzahlMode_l2 As Integer = 0
                Dim anzahlFixpunkte2 As Integer = 0
                If line2.Item1.isFixpunkt Then
                    anzahlFixpunkte2 += 1
                End If
                If line2.Item2.isFixpunkt Then
                    anzahlFixpunkte2 += 1
                End If
                For Each g As Tuple(Of Routing_point, Routing_glue) In line2.Item1.nachbarn
                    If getMode(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line2.Item1, g.Item1, g.Item2)) = mode1 Then
                        anzahlMode_l2 += 1
                    End If
                Next
                For Each g As Tuple(Of Routing_point, Routing_glue) In line2.Item2.nachbarn
                    If getMode(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line2.Item2, g.Item1, g.Item2)) = mode1 Then
                        anzahlMode_l2 += 1
                    End If
                Next

                If anzahlFixpunkte2 < anzahlFixpunkte1 OrElse (anzahlFixpunkte1 = anzahlFixpunkte2 AndAlso anzahlMode_l2 < anzahlMode_l1) Then
                    'tausche line1 und line2
                    Dim z As Tuple(Of Routing_point, Routing_point, Routing_glue) = line1
                    line1 = line2
                    line2 = z
                    'tausche anzahlMode_l1/l2
                    Dim za As Integer = anzahlMode_l1
                    anzahlMode_l1 = anzahlMode_l2
                    anzahlMode_l2 = za
                End If
            End If
            '-------------------------
            'solve line1
            '-------------------------
            Dim n1 As Routing_net = line1.Item1.parent

            Dim start As Point = line1.Item1.pos
            Dim ende As Point = line1.Item2.pos
            If mode1 = Routing_glue.Mode.XGleich Then
                'verschiebe in X-Richtung
                Dim möglichkeiten As New List(Of Tuple(Of Integer, Integer)) 'dx, Bewertung (je kleiner desto besser)
                Dim liste As New List(Of Rectangle)
                Dim vorzeichenDistVorher As Integer = line1.Item1.posAlt.X - line2.Item1.posAlt.X
                For dx As Integer = -v.GridX * 10 To v.GridX * 10 Step v.GridX
                    If dx = 0 Then Continue For
                    If istWireDa(n1.netName, New Point(start.X + dx, start.Y), New Point(ende.X + dx, ende.Y)) Then
                        Continue For
                    End If
                    If istWireDa(n1.netName, start, New Point(start.X + dx, start.Y)) Then
                        Continue For
                    End If
                    If istWireDa(n1.netName, ende, New Point(ende.X + dx, ende.Y)) Then
                        Continue For
                    End If
                    'kann verschieben
                    Dim lowDyPenalty As Integer = 0
                    If Math.Abs(dx) < 2 * v.GridX Then
                        lowDyPenalty = 4 * v.GridX - 2 * Math.Abs(dx) + v.GridX \ 2
                    End If
                    '------------------------------
                    'ist Bauelement im weg?
                    '------------------------------
                    Dim anzahlImWeg As Integer = 0
                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(New Point(start.X + dx, start.Y), New Point(ende.X + dx, ende.Y), liste, False, False)
                    anzahlImWeg += liste.Count

                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(start, New Point(start.X + dx, start.Y), liste, False, False)
                    anzahlImWeg += liste.Count

                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(ende, New Point(ende.X + dx, ende.Y), liste, False, False)
                    anzahlImWeg += liste.Count
                    '------------------------------
                    Dim penalty_ausrichtungVorher As Integer = 0
                    If vorzeichenDistVorher * dx < 0 Then
                        penalty_ausrichtungVorher = 1
                    End If

                    möglichkeiten.Add(New Tuple(Of Integer, Integer)(dx, Math.Abs(dx) + lowDyPenalty + anzahlImWeg * 11 * v.GridX + penalty_ausrichtungVorher))
                Next
                If möglichkeiten.Count > 0 Then
                    möglichkeiten.Sort(New Comparison(Of Tuple(Of Integer, Integer))(AddressOf myTupleComparer))
                    If line1.Item1.isFixpunkt Then
                        'Fixpunkte können nicht verschoben werden!
                        Dim pNeu As Routing_point = line1.Item1.copy()
                        Dim gNeu As New Routing_glue(Routing_glue.Mode.YGleich, line1.Item3.istDick)
                        pNeu.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(line1.Item1, gNeu))
                        line1.Item1.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(pNeu, gNeu))
                        n1.points.Add(pNeu)
                        n1.glue.Add(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item1, pNeu, gNeu))
                        line1.Item1.isFixpunkt = False
                    End If
                    If line1.Item2.isFixpunkt Then
                        'Fixpunkte können nicht verschoben werden!
                        Dim pNeu As Routing_point = line1.Item2.copy()
                        Dim gNeu As New Routing_glue(Routing_glue.Mode.YGleich, line1.Item3.istDick)
                        pNeu.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(line1.Item2, gNeu))
                        line1.Item2.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(pNeu, gNeu))
                        n1.points.Add(pNeu)
                        n1.glue.Add(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item2, pNeu, gNeu))
                        line1.Item2.isFixpunkt = False
                    End If
                    line1.Item1.pos.X += möglichkeiten(0).Item1
                    line1.Item2.pos.X += möglichkeiten(0).Item1
                    Return True
                End If
            Else
                'verschiebe in Y-Richtung
                Dim möglichkeiten As New List(Of Tuple(Of Integer, Integer)) 'dy, Bewertung (je kleiner desto besser)
                Dim liste As New List(Of Rectangle)
                Dim vorzeichenDistVorher As Integer = line1.Item1.posAlt.Y - line2.Item1.posAlt.Y
                For dy As Integer = -v.GridY * 10 To v.GridY * 10 Step v.GridY
                    If dy = 0 Then Continue For
                    If istWireDa(n1.netName, New Point(start.X, start.Y + dy), New Point(ende.X, ende.Y + dy)) Then
                        Continue For
                    End If
                    If istWireDa(n1.netName, start, New Point(start.X, start.Y + dy)) Then
                        Continue For
                    End If
                    If istWireDa(n1.netName, ende, New Point(ende.X, ende.Y + dy)) Then
                        Continue For
                    End If
                    'kann verschieben
                    Dim lowDyPenalty As Integer = 0
                    If Math.Abs(dy) < 2 * v.GridY Then
                        lowDyPenalty = 4 * v.GridY - 2 * Math.Abs(dy) + v.GridY \ 2
                    End If
                    '------------------------------
                    'ist Bauelement im weg?
                    '------------------------------
                    Dim anzahlImWeg As Integer = 0
                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(New Point(start.X, start.Y + dy), New Point(ende.X, ende.Y + dy), liste, False, False)
                    anzahlImWeg += liste.Count

                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(start, New Point(start.X, start.Y + dy), liste, False, False)
                    anzahlImWeg += liste.Count

                    liste.Clear()
                    v.schneidet_Wire_ein_Bauteil(ende, New Point(ende.X, ende.Y + dy), liste, False, False)
                    anzahlImWeg += liste.Count
                    '------------------------------
                    Dim penalty_ausrichtungVorher As Integer = 0
                    If vorzeichenDistVorher * dy < 0 Then
                        penalty_ausrichtungVorher = 1
                    End If

                    möglichkeiten.Add(New Tuple(Of Integer, Integer)(dy, Math.Abs(dy) + lowDyPenalty + anzahlImWeg * 11 * v.GridY + penalty_ausrichtungVorher))
                Next
                If möglichkeiten.Count > 0 Then
                    möglichkeiten.Sort(New Comparison(Of Tuple(Of Integer, Integer))(AddressOf myTupleComparer))
                    If line1.Item1.isFixpunkt Then
                        'Fixpunkte können nicht verschoben werden!
                        Dim pNeu As Routing_point = line1.Item1.copy()
                        Dim gNeu As New Routing_glue(Routing_glue.Mode.XGleich, line1.Item3.istDick)
                        pNeu.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(line1.Item1, gNeu))
                        line1.Item1.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(pNeu, gNeu))
                        n1.points.Add(pNeu)
                        n1.glue.Add(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item1, pNeu, gNeu))
                        line1.Item1.isFixpunkt = False
                    End If
                    If line1.Item2.isFixpunkt Then
                        'Fixpunkte können nicht verschoben werden!
                        Dim pNeu As Routing_point = line1.Item2.copy()
                        Dim gNeu As New Routing_glue(Routing_glue.Mode.XGleich, line1.Item3.istDick)
                        pNeu.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(line1.Item2, gNeu))
                        line1.Item2.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(pNeu, gNeu))
                        n1.points.Add(pNeu)
                        n1.glue.Add(New Tuple(Of Routing_point, Routing_point, Routing_glue)(line1.Item2, pNeu, gNeu))
                        line1.Item2.isFixpunkt = False
                    End If
                    line1.Item1.pos.Y += möglichkeiten(0).Item1
                    line1.Item2.pos.Y += möglichkeiten(0).Item1
                    Return True
                End If
            End If
            Return False
        End Function

        Private Function myTupleComparer(t1 As Tuple(Of Integer, Integer), t2 As Tuple(Of Integer, Integer)) As Integer
            Return t1.Item2.CompareTo(t2.Item2)
        End Function

        Private Function istWireDa(nichtNetName As String, p1 As Point, p2 As Point) As Boolean
            For Each n As Routing_net In myNets
                If n.netName <> nichtNetName Then
                    For Each line As Tuple(Of Routing_point, Routing_point, Routing_glue) In n.glue
                        If schneidenSichLinien(line.Item1.pos, line.Item2.pos, p1, p2) Then
                            Return True
                        End If
                    Next
                End If
            Next
            Return False
        End Function

        Private Function getMode(line As Tuple(Of Routing_point, Routing_point, Routing_glue)) As Routing_glue.Mode
            If line.Item3.mymode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                Dim p1 As Point = line.Item1.pos
                Dim p2 As Point = line.Item2.pos
                If Math.Abs(p1.X - p2.X) <= Math.Abs(p1.Y - p2.Y) Then
                    Return Routing_glue.Mode.XGleich
                Else
                    Return Routing_glue.Mode.YGleich
                End If
            End If
            Return line.Item3.mymode
        End Function
#End Region

#Region "Find Shorts"
        Private Function findShorts(n1 As Routing_net, n2 As Routing_net) As List(Of Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue)))
            Dim erg As New List(Of Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue)))
            For Each line1 As Tuple(Of Routing_point, Routing_point, Routing_glue) In n1.glue
                For Each line2 As Tuple(Of Routing_point, Routing_point, Routing_glue) In n2.glue
                    If istShort(line1, line2) Then
                        'line1.Item3.istShort = True
                        'line2.Item3.istShort = True
                        erg.Add(New Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue))(line1, line2))
                    ElseIf istShortType2(line1, line2) Then
                        erg.Add(New Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue))(line1, line2))
                    ElseIf istShortType2(line2, line1) Then
                        erg.Add(New Tuple(Of Tuple(Of Routing_point, Routing_point, Routing_glue), Tuple(Of Routing_point, Routing_point, Routing_glue))(line2, line1))
                    End If
                Next
            Next
            Return erg
        End Function

        Private Function istShort(line1 As Tuple(Of Routing_point, Routing_point, Routing_glue), line2 As Tuple(Of Routing_point, Routing_point, Routing_glue)) As Boolean
            Dim start1 As Point = line1.Item1.pos
            Dim ende1 As Point = line1.Item2.pos
            Dim start2 As Point = line2.Item1.pos
            Dim ende2 As Point = line2.Item2.pos
            Return schneidenSichLinien(start1, ende1, start2, ende2)
        End Function

        Private Function schneidenSichLinien(start1 As Point, ende1 As Point, start2 As Point, ende2 As Point) As Boolean
            If start1.X = ende1.X AndAlso start2.X = ende2.X AndAlso start1.X = start2.X Then
                Dim minY1 As Integer = Math.Min(start1.Y, ende1.Y)
                Dim maxY1 As Integer = Math.Max(start1.Y, ende1.Y)
                Dim minY2 As Integer = Math.Min(start2.Y, ende2.Y)
                Dim maxY2 As Integer = Math.Max(start2.Y, ende2.Y)
                If Not (maxY1 < minY2 OrElse minY1 > maxY2) Then
                    Return True
                End If
            ElseIf start1.Y = ende1.Y AndAlso start2.Y = ende2.Y AndAlso start1.Y = start2.Y Then
                Dim minX1 As Integer = Math.Min(start1.X, ende1.X)
                Dim maxX1 As Integer = Math.Max(start1.X, ende1.X)
                Dim minX2 As Integer = Math.Min(start2.X, ende2.X)
                Dim maxX2 As Integer = Math.Max(start2.X, ende2.X)
                If Not (maxX1 < minX2 OrElse minX1 > maxX2) Then
                    Return True
                End If
            End If
            Return False
        End Function

        Private Function istShortType2(line1 As Tuple(Of Routing_point, Routing_point, Routing_glue), line2 As Tuple(Of Routing_point, Routing_point, Routing_glue)) As Boolean
            Dim start1 As Point = line1.Item1.pos
            Dim ende1 As Point = line1.Item2.pos
            Dim start2 As Point = line2.Item1.pos
            Dim ende2 As Point = line2.Item2.pos
            If line2.Item1.isFixpunkt AndAlso liegtPunktAufLinie(start1, ende1, start2) Then
                Return True 'nur ein Fixpunkt kann ein short type2 sein. Wenn es im "normalen" Wire auftritt wäre es ein short type1 in der Nähe und der short type 2 ist nur eine sekundäre auswirkung!
            ElseIf line2.Item2.isFixpunkt AndAlso liegtPunktAufLinie(start1, ende1, ende2) Then
                Return True 'nur ein Fixpunkt kann ein short type2 sein. Wenn es im "normalen" Wire auftritt wäre es ein short type1 in der Nähe und der short type 2 ist nur eine sekundäre auswirkung!
            End If
            Return False
        End Function

        Private Function liegtPunktAufLinie(start As Point, ende As Point, testpos As Point) As Boolean
            If start.X = ende.X AndAlso testpos.X = start.X Then
                Return testpos.Y >= Math.Min(start.Y, ende.Y) AndAlso testpos.Y <= Math.Max(start.Y, ende.Y)
            ElseIf start.Y = ende.Y AndAlso testpos.Y = start.Y Then
                Return testpos.X >= Math.Min(start.X, ende.X) AndAlso testpos.X <= Math.Max(start.X, ende.X)
            End If
            Return False
        End Function

#End Region

#End Region

#Region "Zeichne"
        Public Sub zeichne(v As Vektor_Picturebox, ImmerDiagonal As Boolean, ls_dick As Integer)
            Dim ls_Löschen As LineStyle = v.myLineStyles.getLineStyle(0).copy()
            ls_Löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_index_löschen As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_Löschen)

            Dim ls_dick_löschen As LineStyle = v.myLineStyles.getLineStyle(ls_dick).copy()
            ls_dick_löschen.farbe = New CircuitDrawing.Farbe(255, 255, 0, 0)
            Dim ls_dick_löschen_index As Integer = v.myLineStyles.getNumberOfNewLinestyle(ls_dick_löschen)

            For Each n As Routing_net In myNets
                zeichne(v, n, ImmerDiagonal, ls_index_löschen, ls_dick, ls_dick_löschen_index)
            Next
        End Sub

        Private Sub zeichne(v As Vektor_Picturebox, n As Routing_net, ImmerDiagonal As Boolean, ls_index_löschen As Integer, ls_dick As Integer, ls_dick_löschen As Integer)
            For Each p As Routing_point In n.points
                For Each g As Tuple(Of Routing_point, Routing_glue) In p.nachbarn
                    g.Item2.hatSchonGezeichnet = False
                Next
            Next

            For Each p As Routing_point In n.points
                For Each g As Tuple(Of Routing_point, Routing_glue) In p.nachbarn
                    If Not g.Item2.hatSchonGezeichnet Then
                        g.Item2.hatSchonGezeichnet = True
                        Dim start As Point = p.pos
                        Dim ende As Point = g.Item1.pos
                        If start.X = ende.X AndAlso start.Y = ende.Y Then
                            'mache nichts
                        ElseIf start.X = ende.X OrElse start.Y = ende.Y Then
                            Dim w As New CircuitDrawing.Wire(v.getNewID(), start, ende)
                            If g.Item2.istDick Then
                                If g.Item2.istShort Then
                                    w.linestyle = ls_dick_löschen
                                Else
                                    w.linestyle = ls_dick
                                End If
                            Else
                                If g.Item2.istShort Then
                                    w.linestyle = ls_index_löschen
                                End If
                            End If
                            v.addElement_OHNE_SIMPLIFY_WIRES(w)
                        Else
                            If g.Item2.mymode = Routing_glue.Mode.Frei OrElse ImmerDiagonal Then
                                Dim w As New CircuitDrawing.WireLuftlinie(v.getNewID(), start, ende)
                                If g.Item2.istDick Then
                                    If g.Item2.istShort Then
                                        w.linestyle = ls_dick_löschen
                                    Else
                                        w.linestyle = ls_dick
                                    End If
                                Else
                                    If g.Item2.istShort Then
                                        w.linestyle = ls_index_löschen
                                    End If
                                End If
                                v.addElement_OHNE_SIMPLIFY_WIRES(w)
                            Else
                                zeichneKonfliktWire(v, p, g.Item1, g.Item2.mymode)
                            End If
                        End If
                    End If
                Next
            Next

            v.simplifyWires()
        End Sub

        Private Sub zeichneKonfliktWire(v As Vektor_Picturebox, start As Routing_point, ende As Routing_point, mode As Routing_glue.Mode)
            Dim istXSpezial As Boolean = False
            Dim istYSpezial As Boolean = False
            If mode = Routing_glue.Mode.XGleich_ODER_YGleich Then
                istXSpezial = Math.Abs(ende.pos.X - start.pos.X) >= Math.Abs(ende.pos.Y - start.pos.Y)
                istYSpezial = Not istXSpezial
            End If

            Dim points(3) As Point
            points(0) = start.pos
            points(3) = ende.pos
            If mode = Routing_glue.Mode.XGleich OrElse istXSpezial Then
                Dim yMitte As Integer = (start.pos.Y + ende.pos.Y) \ 2
                yMitte = v.GridY * v.fitToGridY(yMitte)
                points(1) = New Point(start.pos.X, yMitte)
                points(2) = New Point(ende.pos.X, yMitte)
            ElseIf mode = Routing_glue.Mode.YGleich OrElse istYSpezial Then
                Dim xMitte As Integer = (start.pos.X + ende.pos.X) \ 2
                xMitte = v.GridX * v.fitToGridX(xMitte)
                points(1) = New Point(xMitte, start.pos.Y)
                points(2) = New Point(xMitte, ende.pos.Y)
            Else
                Throw New Exception("Falscher Glue-Modus!")
            End If
            Dim w1 As New CircuitDrawing.Wire(v.getNewID(), points(0), points(1))
            Dim w2 As New CircuitDrawing.Wire(v.getNewID(), points(1), points(2))
            Dim w3 As New CircuitDrawing.Wire(v.getNewID(), points(2), points(3))
            v.addElement_OHNE_SIMPLIFY_WIRES(w1)
            v.addElement_OHNE_SIMPLIFY_WIRES(w2)
            v.addElement_OHNE_SIMPLIFY_WIRES(w3)
        End Sub
#End Region

    End Class

    Public Class Routing_net
        Public netName As String
        Public points As List(Of Routing_point)
        Public glue As List(Of Tuple(Of Routing_point, Routing_point, Routing_glue))

        Public Sub New(netName As String)
            Me.netName = netName
            Me.points = New List(Of Routing_point)
            Me.glue = New List(Of Tuple(Of Routing_point, Routing_point, Routing_glue))
        End Sub

        Public Sub initFromNet(net As Net_Skill, transform As Func(Of PointF, Point))
            For Each line As Line_Skill In net.lines
                If Not line.OptimierungLöschen Then
                    Dim rStart As Routing_point = initPoint(transform(line.start), line.connectStartTo)
                    Dim rEnde As Routing_point = initPoint(transform(line.ende), line.connectEndeTo)
                    Dim mode As Routing_glue.Mode
                    If line.start.X = line.ende.X AndAlso line.start.Y = line.ende.Y Then
                        'line mit länge Null. Kann man die Richtung nicht sagen!
                        mode = Routing_glue.Mode.XGleich_ODER_YGleich
                    ElseIf line.start.X = line.ende.X Then
                        mode = Routing_glue.Mode.XGleich
                    ElseIf line.start.Y = line.ende.Y Then
                        mode = Routing_glue.Mode.YGleich
                    Else
                        mode = Routing_glue.Mode.Frei
                    End If
                    Dim glue As New Routing_glue(mode, line.istDickeLinie)
                    rStart.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(rEnde, glue))
                    rEnde.nachbarn.Add(New Tuple(Of Routing_point, Routing_glue)(rStart, glue))
                    Me.glue.Add(New Tuple(Of Routing_point, Routing_point, Routing_glue)(rStart, rEnde, glue))
                End If
            Next
        End Sub

        Private Function initPoint(pos As Point, connectTo As Tuple(Of Instance_Skill, Integer)) As Routing_point
            If connectTo IsNot Nothing Then
                Dim nrSnap As Integer = connectTo.Item1.master.pins(connectTo.Item2).pinInVektorgrafik
                Dim posSoll As Point = connectTo.Item1.BauelementInVektorgrafik.getSnappoint(nrSnap).p
                points.Add(New Routing_point(pos, posSoll, Me))
                Return points(points.Count - 1)
            Else
                For i As Integer = 0 To points.Count - 1
                    If Not points(i).isFixpunkt AndAlso points(i).pos = pos Then
                        Return points(i)
                    End If
                Next
                points.Add(New Routing_point(pos, Me))
                Return points(points.Count - 1)
            End If
        End Function
    End Class

    Public Class Routing_point
        Public parent As Routing_net
        Public posAlt As Point
        Public pos As Point
        Public pos_sollFixpunkt As Point
        Public isFixpunkt As Boolean
        Public nachbarn As List(Of Tuple(Of Routing_point, Routing_glue))

        Public only_Ein_Constraint_Stufe_X As Integer = -1
        Public only_Ein_Constraint_Stufe_Y As Integer = -1

        Public Sub New(pos As Point, parent As Routing_net)
            Me.posAlt = pos
            Me.parent = parent
            Me.pos = pos
            Me.isFixpunkt = False
            Me.nachbarn = New List(Of Tuple(Of Routing_point, Routing_glue))
        End Sub

        Public Sub New(pos As Point, posSoll As Point, parent As Routing_net)
            Me.parent = parent
            Me.posAlt = pos
            Me.pos = pos
            Me.pos_sollFixpunkt = posSoll
            Me.isFixpunkt = True
            Me.nachbarn = New List(Of Tuple(Of Routing_point, Routing_glue))
        End Sub

        Public Function copy() As Routing_point
            Dim erg As New Routing_point(Me.pos, Me.parent)
            erg.posAlt = Me.posAlt
            erg.pos = Me.pos
            erg.pos_sollFixpunkt = Me.pos_sollFixpunkt
            erg.isFixpunkt = Me.isFixpunkt
            Return erg
        End Function
    End Class

    Public Class Routing_glue
        Public mymode As Mode
        Public hatSchonGezeichnet As Boolean 'temporärer Wert während des Zeichnes
        Public istShort As Boolean = False 'wurde dieses Wire als Short detektiert, welches ein Problem darstellt?
        Public istDick As Boolean = False
        Public Sub New(mymode As Mode, istDick As Boolean)
            Me.mymode = mymode
            Me.istDick = istDick
        End Sub
        Public Enum Mode
            Frei
            XGleich
            YGleich
            XGleich_ODER_YGleich
        End Enum
    End Class
End Namespace