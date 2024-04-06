Public Class Netze
    Public netze As List(Of Netz_System)
    Private myWiresLuftlinie As List(Of WireLuftlinieMitConstraints)

    Public Sub New(wires As List(Of WireMitConstraints), wiresLuftlinie As List(Of WireLuftlinieMitConstraints))
        netze = New List(Of Netz_System)
        Me.myWiresLuftlinie = wiresLuftlinie

        For i As Integer = 0 To wires.Count - 1
            If Not wires(i).abgearbeitet Then
                If wires(i).startAlign IsNot Nothing Then
                    'starte von hier
                    netze.Add(New Netz_System(wires, i, True, wiresLuftlinie))
                ElseIf wires(i).endeAlign IsNot Nothing Then
                    netze.Add(New Netz_System(wires, i, False, wiresLuftlinie))
                End If
            End If
        Next
    End Sub

    Public Sub zeichnen(v As Vektor_Picturebox)
        For i As Integer = 0 To netze.Count - 1
            netze(i).zeichnen(v)
        Next

        For i As Integer = 0 To myWiresLuftlinie.Count - 1
            If myWiresLuftlinie(i).hatNeuenStart AndAlso myWiresLuftlinie(i).hatNeuesEnde Then
                Dim start As Point = myWiresLuftlinie(i).neuerStart
                Dim ende As Point = myWiresLuftlinie(i).neuesEnde
                Dim w As New WireLuftlinie(v.getNewID(), start, ende)
                v.addElement_OHNE_SIMPLIFY_WIRES(w)
            End If
        Next
    End Sub
End Class

Public Class Netz_System
    Public startPos As Netz_AlignAt
    Public startWire As Netz_WireElement

    Public Sub New(AlleWires As List(Of WireMitConstraints), start As Integer, vonStartNachEnde As Boolean, wiresLuftlinie As List(Of WireLuftlinieMitConstraints))
        If vonStartNachEnde Then
            startPos = AlleWires(start).startAlign
        Else
            startPos = AlleWires(start).endeAlign
        End If
        startWire = New Netz_WireElement(AlleWires(start), vonStartNachEnde, wiresLuftlinie)

        startWire.erweitern(AlleWires, wiresLuftlinie)
        startWire.propagateConstraints()
    End Sub

    Public Sub zeichnen(v As Vektor_Picturebox)
        startWire.zeichnen(v, startPos.getPos(v))
    End Sub
End Class

Public Class Netz_WireElement
    Public posEndeOriginal As Point

    Public vector_Default As Point
    Public Align_Ende_FIX As Netz_AlignAt
    Public Align_Ende_Var As Netz_AlignAt_Var
    Public childs As List(Of Netz_WireElement)

    Public AmEndeWireLuftLinie As List(Of Tuple(Of WireLuftlinieMitConstraints, Boolean)) 'wire und true=start; false=ende

    Public Sub New(wire As WireMitConstraints, vonStartNachEnde As Boolean, wiresLuftlinie As List(Of WireLuftlinieMitConstraints))
        wire.abgearbeitet = True

        childs = New List(Of Netz_WireElement)
        vector_Default = wire.wire.vector
        If vonStartNachEnde Then
            Align_Ende_FIX = wire.endeAlign
            posEndeOriginal = wire.wire.getEnde()
        Else
            Align_Ende_FIX = wire.startAlign
            posEndeOriginal = wire.wire.getStart()
            vector_Default.X *= -1
            vector_Default.Y *= -1
        End If

        AmEndeWireLuftLinie = New List(Of Tuple(Of WireLuftlinieMitConstraints, Boolean))
        For i As Integer = 0 To wiresLuftlinie.Count - 1
            If wiresLuftlinie(i).start = posEndeOriginal Then
                AmEndeWireLuftLinie.Add(New Tuple(Of WireLuftlinieMitConstraints, Boolean)(wiresLuftlinie(i), True))
            End If
            If wiresLuftlinie(i).ende = posEndeOriginal Then
                AmEndeWireLuftLinie.Add(New Tuple(Of WireLuftlinieMitConstraints, Boolean)(wiresLuftlinie(i), False))
            End If
        Next

        Align_Ende_Var = Nothing
    End Sub

    Public Sub erweitern(wires As List(Of WireMitConstraints), wiresLuftlinie As List(Of WireLuftlinieMitConstraints))
        For i As Integer = 0 To wires.Count - 1
            If Not wires(i).abgearbeitet Then
                Dim wiresAmEnde As List(Of WireMitConstraints) = getWiresAnPosition(wires, posEndeOriginal)
                For j As Integer = 0 To wiresAmEnde.Count - 1
                    addWire(wiresAmEnde(j), wiresLuftlinie)
                Next
            End If
        Next
        For i As Integer = 0 To childs.Count - 1
            childs(i).erweitern(wires, wiresLuftlinie)
        Next
    End Sub

    Private Sub addWire(wire As WireMitConstraints, wiresLuftlinie As List(Of WireLuftlinieMitConstraints))
        If wire.wire.getStart() = posEndeOriginal Then
            childs.Add(New Netz_WireElement(wire, True, wiresLuftlinie))
        ElseIf wire.wire.getEnde() = posEndeOriginal Then
            childs.Add(New Netz_WireElement(wire, False, wiresLuftlinie))
        End If
    End Sub

    Private Function getWiresAnPosition(AlleWires As List(Of WireMitConstraints), p As Point) As List(Of WireMitConstraints)
        Dim erg As New List(Of WireMitConstraints)
        For Each w As WireMitConstraints In AlleWires
            If Not w.abgearbeitet Then
                If w.wire.getStart() = p Then
                    erg.Add(w)
                ElseIf w.wire.getEnde() = p Then
                    erg.Add(w)
                End If
            End If
        Next
        Return erg
    End Function

    Public Sub propagateConstraints()
        If Me.Align_Ende_FIX Is Nothing Then
            If childs.Count = 1 AndAlso Me.childs(0).Align_Ende_FIX IsNot Nothing Then
                'wenn man nur 1 Child hat, dann kann man fixed aligns direkt weiterreichen!
                childs(0).propagateConstraints()
                Me.Align_Ende_FIX = Me.childs(0).Align_Ende_FIX.clone()
                Me.Align_Ende_FIX.offset = New Point(Me.Align_Ende_FIX.offset.X - childs(0).vector_Default.X, Me.Align_Ende_FIX.offset.Y - childs(0).vector_Default.Y)

                'Zusätzlich noch als Fallback im Falle, dass das Align_Ende_Fix nicht angewendet werden kann:
                If childs(0).vector_Default.X = 0 Then
                    'constraint der X-Koordinate kann gesetzt werden!

                    If Me.Align_Ende_Var Is Nothing Then
                        Me.Align_Ende_Var = New Netz_AlignAt_Var(childs(0).Align_Ende_FIX, Nothing)
                    ElseIf Me.Align_Ende_Var.align_X_At Is Nothing Then
                        Me.Align_Ende_Var.align_X_At = childs(0).Align_Ende_FIX
                    End If

                ElseIf childs(0).vector_Default.Y = 0 Then
                    'constraint der Y-Koordinate kann gesetzt werden!

                    If Me.Align_Ende_Var Is Nothing Then
                        Me.Align_Ende_Var = New Netz_AlignAt_Var(Nothing, childs(0).Align_Ende_FIX)
                    ElseIf Me.Align_Ende_Var.align_Y_At Is Nothing Then
                        Me.Align_Ende_Var.align_Y_At = childs(0).Align_Ende_FIX
                    End If
                End If


            Else
                For i As Integer = 0 To childs.Count - 1
                    childs(i).propagateConstraints()

                    If childs(i).Align_Ende_FIX IsNot Nothing Then
                        If childs(i).vector_Default.X = 0 Then
                            'constraint der X-Koordinate kann gesetzt werden!

                            If Me.Align_Ende_Var Is Nothing Then
                                Me.Align_Ende_Var = New Netz_AlignAt_Var(childs(i).Align_Ende_FIX, Nothing)
                            ElseIf Me.Align_Ende_Var.align_X_At Is Nothing Then
                                Me.Align_Ende_Var.align_X_At = childs(i).Align_Ende_FIX
                            End If

                        ElseIf childs(i).vector_Default.Y = 0 Then
                            'constraint der Y-Koordinate kann gesetzt werden!

                            If Me.Align_Ende_Var Is Nothing Then
                                Me.Align_Ende_Var = New Netz_AlignAt_Var(Nothing, childs(i).Align_Ende_FIX)
                            ElseIf Me.Align_Ende_Var.align_Y_At Is Nothing Then
                                Me.Align_Ende_Var.align_Y_At = childs(i).Align_Ende_FIX
                            End If
                        End If
                    ElseIf childs(i).Align_Ende_Var IsNot Nothing Then
                        If childs(i).vector_Default.X = 0 AndAlso childs(i).Align_Ende_Var.align_X_At IsNot Nothing Then
                            'constraint der X-Koordinate kann gesetzt werden!

                            If Me.Align_Ende_Var Is Nothing Then
                                Me.Align_Ende_Var = New Netz_AlignAt_Var(childs(i).Align_Ende_Var.align_X_At, Nothing)
                            ElseIf Me.Align_Ende_Var.align_X_At Is Nothing Then
                                Me.Align_Ende_Var.align_X_At = childs(i).Align_Ende_Var.align_X_At
                            End If
                        ElseIf childs(i).vector_Default.Y = 0 AndAlso childs(i).Align_Ende_Var.align_Y_At IsNot Nothing Then
                            'constraint der Y-Koordinate kann gesetzt werden!

                            If Me.Align_Ende_Var Is Nothing Then
                                Me.Align_Ende_Var = New Netz_AlignAt_Var(Nothing, childs(i).Align_Ende_Var.align_Y_At)
                            ElseIf Me.Align_Ende_Var.align_Y_At Is Nothing Then
                                Me.Align_Ende_Var.align_Y_At = childs(i).Align_Ende_Var.align_Y_At
                            End If
                        End If
                    End If
                Next
            End If

        End If
    End Sub

    Public Sub zeichnen(v As Vektor_Picturebox, startPos As Point)
        Dim ende As Point
        If Align_Ende_FIX IsNot Nothing Then
            If Align_Ende_FIX.offset.X = 0 AndAlso Align_Ende_FIX.offset.Y = 0 Then
                Dim ende_snap As Snappoint = Align_Ende_FIX.getSnappoint(v)
                v.DoRouting(New Snappoint(startPos, 0, 0, 0, 0), ende_snap, New ParamArrow(-1, 100), True)
                ende = ende_snap.p
            Else
                'Wenn es nur ein weitergegebenes alignment ist, muss man kein "komisches" routing machen!
                Dim ende_snap As Snappoint = Align_Ende_FIX.getSnappoint(v)
                If ende_snap.p.X = startPos.X OrElse ende_snap.p.Y = startPos.Y Then
                    'man kann es durch eine gerade linie verbinden!
                    v.DoRouting(New Snappoint(startPos, 0, 0, 0, 0), ende_snap, New ParamArrow(-1, 100), True)
                    ende = ende_snap.p
                Else
                    'man sollte in diesem fall den constraint vernachlässigen
                    Me.Align_Ende_FIX = Nothing
                    Me.zeichnen(v, startPos)
                    Return
                End If
            End If
        ElseIf Align_Ende_Var IsNot Nothing Then
            If vector_Default.X = 0 AndAlso Align_Ende_Var.align_Y_At IsNot Nothing Then
                ende = New Point(startPos.X, Align_Ende_Var.align_Y_At.getPos(v).Y)
            ElseIf vector_Default.Y = 0 AndAlso Align_Ende_Var.align_X_At IsNot Nothing Then
                ende = New Point(Align_Ende_Var.align_X_At.getPos(v).X, startPos.Y)
            Else
                Dim vx As Integer = fitToGridX(v, vector_Default.X)
                Dim vy As Integer = fitToGridY(v, vector_Default.Y)
                ende = New Point(startPos.X + vx, startPos.Y + vy)
            End If
            v.addElement(New Wire(v.getNewID(), startPos, ende))
        Else
            Dim vx As Integer = fitToGridX(v, vector_Default.X)
            Dim vy As Integer = fitToGridY(v, vector_Default.Y)
            ende = New Point(startPos.X + vx, startPos.Y + vy)
            v.addElement(New Wire(v.getNewID(), startPos, ende))
        End If

        For i As Integer = 0 To childs.Count - 1
            childs(i).zeichnen(v, ende)
        Next

        'am ende die wire_luftlinie anschließen!
        For i As Integer = 0 To AmEndeWireLuftLinie.Count - 1
            If AmEndeWireLuftLinie(i).Item2 Then
                'wird am Startpunkt von wireLuftlinie angeschlossen
                AmEndeWireLuftLinie(i).Item1.neuerStart = ende
                AmEndeWireLuftLinie(i).Item1.hatNeuenStart = True
            Else
                'wird am Endpunkt von wireLuftlinie angeschlossen
                AmEndeWireLuftLinie(i).Item1.neuesEnde = ende
                AmEndeWireLuftLinie(i).Item1.hatNeuesEnde = True
            End If
        Next
    End Sub

    Public Shared Function fitToGridX(v As Vektor_Picturebox, value As Integer) As Integer
        Dim ofit As Integer = v.fitToGridX(value) * v.GridX
        If ofit = 0 AndAlso value <> 0 Then
            If value < 0 Then
                ofit = -v.GridX
            ElseIf value > 0 Then
                ofit = v.GridX
            End If
        End If
        If ofit > 0 AndAlso ofit < 2 * v.GridX Then
            ofit = 2 * v.GridX
        End If
        If ofit < 0 AndAlso ofit > -2 * v.GridX Then
            ofit = -2 * v.GridX
        End If
        Return ofit
    End Function

    Public Shared Function fitToGridY(v As Vektor_Picturebox, value As Integer) As Integer
        Dim ofit As Integer = v.fitToGridY(value) * v.GridY
        If ofit = 0 AndAlso value <> 0 Then
            If value < 0 Then
                ofit = -v.GridY
            ElseIf value > 0 Then
                ofit = v.GridY
            End If
        End If
        If ofit > 0 AndAlso ofit < 2 * v.GridY Then
            ofit = 2 * v.GridY
        End If
        If ofit < 0 AndAlso ofit > -2 * v.GridY Then
            ofit = -2 * v.GridY
        End If
        Return ofit
    End Function
End Class

Public Class Netz_AlignAt
    Private bauelement As Bauteil
    Private snapIndex As Integer
    Public offset As Point

    Public Sub New(b As Bauteil, snapIndex As Integer)
        Me.bauelement = b
        Me.snapIndex = snapIndex
        offset = New Point(0, 0)
    End Sub

    Public Function clone() As Netz_AlignAt
        Dim erg As New Netz_AlignAt(Me.bauelement, Me.snapIndex)
        erg.offset = Me.offset
        Return erg
    End Function

    Public Function getPos(v As Vektor_Picturebox) As Point
        Dim p As Point = bauelement.getSnappoint(snapIndex).p
        p.X += Netz_WireElement.fitToGridX(v, offset.X)
        p.Y += Netz_WireElement.fitToGridY(v, offset.Y)
        Return p
    End Function

    Public Function getSnappoint(v As Vektor_Picturebox) As Snappoint
        If offset.X = 0 AndAlso offset.Y = 0 Then
            Return bauelement.getSnappoint(snapIndex)
        Else
            Return New Snappoint(getPos(v), 0, 0, 0, 0)
        End If
    End Function
End Class

Public Class Netz_AlignAt_Var
    Public align_X_At As Netz_AlignAt
    Public align_Y_At As Netz_AlignAt

    Public Sub New(align_X_At As Netz_AlignAt, align_Y_At As Netz_AlignAt)
        Me.align_X_At = align_X_At
        Me.align_Y_At = align_Y_At
    End Sub

End Class

Public Class WireMitConstraints
    Public wire As Wire
    Public startAlign As Netz_AlignAt
    Public endeAlign As Netz_AlignAt
    Public abgearbeitet As Boolean
    Public Sub New(wire As Wire)
        Me.wire = wire
        startAlign = Nothing
        endeAlign = Nothing
        abgearbeitet = False
    End Sub
End Class

Public Class WireLuftlinieMitConstraints
    Public start As Point
    Public ende As Point

    Public neuesEnde As Point
    Public neuerStart As Point
    Public hatNeuesEnde As Boolean
    Public hatNeuenStart As Boolean

    Public Sub New(p1 As Point, p2 As Point)
        Me.start = p1
        Me.ende = p2
        hatNeuenStart = False
        hatNeuesEnde = False
    End Sub
End Class