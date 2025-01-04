Imports System.IO

Public Class Wire
    Inherits ElementLinestyle
    Implements IWire

    Public vector As Point
    Public pfeilStart As ParamArrow
    Public pfeilEnde As ParamArrow

    Public Sub New(ID As ULong, start As Point, ende As Point)
        MyBase.New(ID, 1)
        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
        If vector.X <> 0 AndAlso vector.Y <> 0 Then
            If Math.Abs(vector.X) > Math.Abs(vector.Y) Then
                vector.Y = 0
            Else
                vector.X = 0
            End If
            Debug.Print("ACHTUNG: WIRE IST NICHT GERADE!")
        End If
        Me.pfeilEnde = New ParamArrow(-1, 100)
        Me.pfeilStart = New ParamArrow(-1, 100)
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        If Not args.mitZeilensprüngen Then
            Return Me.getGrafikBasic()
        Else
            Return Me.getGrafikMitZeilensprüngen(args.radiusZeilensprünge, args.wires, New Point(args.deltaX, args.deltaY))
        End If
    End Function

    Private Function getGrafikBasic() As DO_Grafik
        Dim g As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(getStart(), getEnde(), pfeilStart, pfeilEnde)
        If TypeOf g Is DO_MultiGrafik Then
            g.lineStyle.linestyle = Me.linestyle
            DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(Me.linestyle)
        Else
            g.lineStyle.linestyle = Me.linestyle
        End If
        Return g
    End Function

    Private Function getGrafikMitZeilensprüngen(radius As Integer, allWires As List(Of Tuple(Of Point, Point)), delta As Point) As DO_Grafik
        If vector.X = 0 Then
            'Zeilensprünge nur bei horizontalen wires
            Return getGrafikBasic()
        End If

        Dim myEnde As Point = Me.getEnde()
        Dim myStart As Point = Me.getStart()

        Dim yVal As Integer = Me.position.Y
        Dim xMin As Integer = Math.Min(Me.position.X, Me.position.X + vector.X)
        Dim xMax As Integer = Math.Max(Me.position.X, Me.position.X + vector.X)
        Dim det As Long
        Dim alpha, beta As Double
        Dim zeilensprünge As New List(Of Double)
        For i As Integer = 0 To allWires.Count - 1
            Dim start As Point = allWires(i).Item1
            Dim ende As Point = allWires(i).Item2
            start.X -= delta.X
            start.Y -= delta.Y
            ende.X -= delta.X
            ende.Y -= delta.Y
            Dim yMin As Integer = Math.Min(start.Y, ende.Y)
            Dim yMax As Integer = Math.Max(start.Y, ende.Y)
            If yVal >= yMin AndAlso yVal <= yMax AndAlso yMin <> yMax Then
                'nur wenn y im Bereich liegt kann es einen Schnittpunkt geben!
                'Wenn das zweite wire auch horizontal liegt, kann es keinen sinnvollen Zeilensprung geben!
                Dim xMin2 As Integer = Math.Min(start.X, ende.X)
                Dim xMax2 As Integer = Math.Max(start.X, ende.X)
                If Not (xMax2 < xMin OrElse xMin2 > xMax) Then
                    'nur wenn x im bereich liegt kann es einen schnittpunkt geben!

                    'Schnittpunkt berechnen (alpha = 0 --> Schnittpunkt bei i1; alpha = 1 --> Schnittpunkt bei i2)
                    det = CLng(myEnde.Y - myStart.Y) * (ende.X - start.X) - CLng(myEnde.X - myStart.X) * (ende.Y - start.Y)
                    If det <> 0 Then
                        alpha = (CLng(ende.X - start.X) * (start.Y - myStart.Y) - (ende.Y - start.Y) * (start.X - myStart.X)) / det
                        If alpha > 0 AndAlso alpha < 1 Then
                            'Prüfen ob auch Linie2 bei beta > 0 und beta < 1 den Schnittpunkt hat!
                            beta = ((myEnde.X - myStart.X) * (start.Y - myStart.Y) - (myEnde.Y - myStart.Y) * (start.X - myStart.X)) / det
                            If beta > 0 AndAlso beta < 1 Then
                                zeilensprünge.Add(alpha)
                            End If
                        End If
                    End If

                End If
            End If
        Next

        Return Me.getGrafikMitZeilensprüngen(radius, zeilensprünge)
    End Function

    Private Function getGrafikMitZeilensprüngen(radius As Integer, sprünge As List(Of Double)) As DO_Grafik
        If sprünge Is Nothing OrElse sprünge.Count < 1 Then
            Return getGrafikBasic()
        End If

        sprünge.Sort()
        Dim g As New DO_MultiGrafik()
        Dim p1, p2 As Point
        Dim start As Point = getStart()
        Dim ende As Point = getEnde()
        Dim len As Integer = Math.Abs(ende.X - start.X) + Math.Abs(ende.Y - start.Y)
        If len = 0 Then Return getGrafikBasic()
        Dim faktor As Double = radius / len

        'Linie vom Start zum ersten Zwischenpunkt
        p1 = start
        If sprünge(0) > faktor Then
            p2 = New Point(CInt(start.X + (sprünge(0) - faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(0) - faktor) * (ende.Y - start.Y)))
        Else
            p2 = p1
        End If
        g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, pfeilStart, New ParamArrow(-1, 100)))
        g.childs.Add(addZeilensprung(radius, sprünge(0)))
        'Linien zwischendrinnen
        For i As Integer = 1 To sprünge.Count - 1
            If sprünge(i) - sprünge(i - 1) > 2 * faktor Then
                p1 = New Point(CInt(start.X + (sprünge(i - 1) + faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(i - 1) + faktor) * (ende.Y - start.Y)))
                p2 = New Point(CInt(start.X + (sprünge(i) - faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(i) - faktor) * (ende.Y - start.Y)))
                g.childs.Add(New DO_Linie(p1, p2, False))
            End If
            g.childs.Add(addZeilensprung(radius, sprünge(i)))
        Next
        'Linie am Ende
        p2 = ende
        If 1 - sprünge(sprünge.Count - 1) > faktor Then
            p1 = New Point(CInt(start.X + (sprünge(sprünge.Count - 1) + faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(sprünge.Count - 1) + faktor) * (ende.Y - start.Y)))
        Else
            p1 = p2
        End If
        g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, New ParamArrow(-1, 100), pfeilEnde))
        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
        Return g
    End Function

    Private Function addZeilensprung(radius As Integer, stelle As Double) As DO_Arc
        Dim pos As New Point(CInt(position.X + stelle * vector.X), CInt(position.Y + stelle * vector.Y))
        Return New DO_Arc(pos, radius, radius, 180, 180, False, False, Drawing_FillMode.OnlyStroke)
    End Function

    Public Overrides Function getSelection() As Selection
        Return New SelectionLine(position, New Point(position.X + vector.X, position.Y + vector.Y))
    End Function

    Public Overrides Function NrOfSnappoints() As Integer
        Return 2
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        If vector.Y = 0 Then
            If vector.X >= 0 Then
                If i = 0 Then
                    Return New Snappoint(position, 0, 5, 0, 0)
                Else
                    Return New Snappoint(New Point(position.X + vector.X, position.Y + vector.Y), 5, 0, 0, 0)
                End If
            Else
                If i = 0 Then
                    Return New Snappoint(position, 5, 0, 0, 0)
                Else
                    Return New Snappoint(New Point(position.X + vector.X, position.Y + vector.Y), 0, 5, 0, 0)
                End If
            End If
        ElseIf vector.X = 0 Then
            If vector.Y >= 0 Then
                If i = 0 Then
                    Return New Snappoint(position, 0, 0, 0, 5)
                Else
                    Return New Snappoint(New Point(position.X + vector.X, position.Y + vector.Y), 0, 0, 5, 0)
                End If
            Else
                If i = 0 Then
                    Return New Snappoint(position, 0, 0, 5, 0)
                Else
                    Return New Snappoint(New Point(position.X + vector.X, position.Y + vector.Y), 0, 0, 0, 5)
                End If
            End If
        Else
            If i = 0 Then
                Return New Snappoint(position, 0, 0, 0, 0)
            Else
                Return New Snappoint(New Point(position.X + vector.X, position.Y + vector.Y), 0, 0, 0, 0)
            End If
        End If
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim w As New Wire(newID, getStart(), getEnde())
        w.isSelected = Me.isSelected
        w.linestyle = Me.linestyle
        w.pfeilStart = Me.pfeilStart.CopyPfeil()
        w.pfeilEnde = Me.pfeilEnde.CopyPfeil()
        Return w
    End Function

    Public Function getStart() As Point Implements IWire.getStart
        Return position
    End Function
    Public Function getStart(delta As Point) As Point Implements IWire.getStart
        Return New Point(position.X + delta.X, position.Y + delta.Y)
    End Function

    Public Function getEnde() As Point Implements IWire.getEnde
        Return New Point(position.X + vector.X, position.Y + vector.Y)
    End Function
    Public Function getEnde(delta As Point) As Point Implements IWire.getEnde
        Return New Point(position.X + vector.X + delta.X, position.Y + vector.Y + delta.Y)
    End Function

    Public Function liegtInMitteVonWire(p As Point) As Boolean
        If vector.X = 0 Then
            Return p.X = position.X AndAlso p.Y > Math.Min(position.Y, position.Y + vector.Y) AndAlso p.Y < Math.Max(position.Y, position.Y + vector.Y)
        ElseIf vector.Y = 0 Then
            Return p.Y = position.Y AndAlso p.X > Math.Min(position.X, position.X + vector.X) AndAlso p.X < Math.Max(position.X, position.X + vector.X)
        Else
            Throw New Exception("Schräge Wires sind nicht erlaubt!")
        End If
    End Function

    Public Function liegtAufWire(p As Point) As Boolean
        If vector.X = 0 Then
            Return p.X = position.X AndAlso p.Y >= Math.Min(position.Y, position.Y + vector.Y) AndAlso p.Y <= Math.Max(position.Y, position.Y + vector.Y)
        ElseIf vector.Y = 0 Then
            Return p.Y = position.Y AndAlso p.X >= Math.Min(position.X, position.X + vector.X) AndAlso p.X <= Math.Max(position.X, position.X + vector.X)
        Else
            Throw New Exception("Schräge Wires sind nicht erlaubt!")
        End If
    End Function

    Public Function AddIfPossible(w As Wire, ecken As Dictionary(Of Point, Integer)) As Boolean
        If Me.vector.X = 0 AndAlso w.vector.X = 0 AndAlso w.position.X = position.X Then
            Dim minY As Integer = Math.Min(position.Y, position.Y + vector.Y)
            Dim maxY As Integer = Math.Max(position.Y, position.Y + vector.Y)
            If (w.position.Y >= minY AndAlso w.position.Y <= maxY) OrElse
               (w.position.Y + w.vector.Y >= minY AndAlso w.position.Y + w.vector.Y <= maxY) Then

                minY = Math.Min(minY, Math.Min(w.position.Y, w.position.Y + w.vector.Y))
                maxY = Math.Max(maxY, Math.Max(w.position.Y, w.position.Y + w.vector.Y))


                If position.Y > minY AndAlso position.Y < maxY AndAlso (ecken(position) > 2 OrElse pfeilStart.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If position.Y + vector.Y > minY AndAlso position.Y + vector.Y < maxY AndAlso (ecken(New Point(position.X, position.Y + vector.Y)) > 2 OrElse pfeilEnde.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If w.position.Y > minY AndAlso w.position.Y < maxY AndAlso (ecken(w.position) > 2 OrElse w.pfeilStart.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If w.position.Y + w.vector.Y > minY AndAlso w.position.Y + w.vector.Y < maxY AndAlso (ecken(New Point(w.position.X, w.position.Y + w.vector.Y)) > 2 OrElse w.pfeilEnde.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If

                Dim pS, pE As ParamArrow
                pS = New ParamArrow(-1, 100) : pE = New ParamArrow(-1, 100)
                If w.position.Y = minY Then
                    pS = w.pfeilStart
                ElseIf w.position.Y = maxY Then
                    pE = w.pfeilStart
                End If
                If w.position.Y + w.vector.Y = minY Then
                    pS = w.pfeilEnde
                ElseIf w.position.Y + w.vector.Y = maxY Then
                    pE = w.pfeilEnde
                End If

                If Me.position.Y = minY Then
                    pS = Me.pfeilStart
                ElseIf Me.position.Y = maxY Then
                    pE = Me.pfeilStart
                End If
                If Me.position.Y + Me.vector.Y = minY Then
                    pS = Me.pfeilEnde
                ElseIf Me.position.Y + Me.vector.Y = maxY Then
                    pE = Me.pfeilEnde
                End If

                Me.position = New Point(Me.position.X, minY)
                Me.vector.Y = maxY - minY
                Me.pfeilStart = pS.CopyPfeil()
                Me.pfeilEnde = pE.CopyPfeil()

                Return True
            End If
        ElseIf Me.vector.Y = 0 AndAlso w.vector.Y = 0 AndAlso w.position.Y = position.Y Then
            Dim minX As Integer = Math.Min(position.X, position.X + vector.X)
            Dim maxX As Integer = Math.Max(position.X, position.X + vector.X)
            If (w.position.X >= minX AndAlso w.position.X <= maxX) OrElse
               (w.position.X + w.vector.X >= minX AndAlso w.position.X + w.vector.X <= maxX) Then

                minX = Math.Min(minX, Math.Min(w.position.X, w.position.X + w.vector.X))
                maxX = Math.Max(maxX, Math.Max(w.position.X, w.position.X + w.vector.X))

                If position.X > minX AndAlso position.X < maxX AndAlso (ecken(position) > 2 OrElse pfeilStart.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If position.X + vector.X > minX AndAlso position.X + vector.X < maxX AndAlso (ecken(New Point(position.X + vector.X, position.Y)) > 2 OrElse pfeilEnde.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If w.position.X > minX AndAlso w.position.X < maxX AndAlso (ecken(w.position) > 2 OrElse w.pfeilStart.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If
                If w.position.X + w.vector.X > minX AndAlso w.position.X + w.vector.X < maxX AndAlso (ecken(New Point(w.position.X + w.vector.X, w.position.Y)) > 2 OrElse w.pfeilEnde.pfeilArt >= 0) Then
                    '3+ Ecken an einem Mittleren Punkt ODER Pfeilende! Nicht zusammenfügen.
                    Return False
                End If

                Dim pS, pE As ParamArrow
                pS = New ParamArrow(-1, 100) : pE = New ParamArrow(-1, 100)
                If w.position.X = minX Then
                    pS = w.pfeilStart
                ElseIf w.position.X = maxX Then
                    pE = w.pfeilStart
                End If
                If w.position.X + w.vector.X = minX Then
                    pS = w.pfeilEnde
                ElseIf w.position.X + w.vector.X = maxX Then
                    pE = w.pfeilEnde
                End If

                If Me.position.X = minX Then
                    pS = Me.pfeilStart
                ElseIf Me.position.X = maxX Then
                    pE = Me.pfeilStart
                End If
                If Me.position.X + Me.vector.X = minX Then
                    pS = Me.pfeilEnde
                ElseIf Me.position.X + Me.vector.X = maxX Then
                    pE = Me.pfeilEnde
                End If

                Me.position = New Point(minX, Me.position.Y)
                Me.vector.X = maxX - minX
                Me.pfeilStart = pS.CopyPfeil()
                Me.pfeilEnde = pE.CopyPfeil()

                Return True
            End If
        End If
        Return False
    End Function

    Public Sub moveStart(neuerStart As Point, vectorVorher As Point)
        Dim neuesEnde As Point
        Dim v As Point = Me.vector
        If v.X = 0 AndAlso v.Y = 0 Then
            v = vectorVorher
        End If
        If v.X = 0 Then
            neuesEnde = New Point(neuerStart.X, getEnde().Y)
        ElseIf v.Y = 0 Then
            neuesEnde = New Point(getEnde().X, neuerStart.Y)
        Else
            Throw New Exception("Schiefe Wires sind nicht erlaubt!")
        End If
        Me.position = neuerStart
        Me.vector = New Point(neuesEnde.X - neuerStart.X, neuesEnde.Y - neuerStart.Y)
    End Sub

    Public Sub moveEnde(neuesEnde As Point, vectorVorher As Point)
        Dim neuerStart As Point
        Dim v As Point = Me.vector
        If v.X = 0 AndAlso v.Y = 0 Then
            v = vectorVorher
        End If
        If v.X = 0 Then
            neuerStart = New Point(neuesEnde.X, getStart().Y)
        ElseIf v.Y = 0 Then
            neuerStart = New Point(getStart().X, neuesEnde.Y)
        Else
            Throw New Exception("Schiefe Wires sind nicht erlaubt!")
        End If
        Me.position = neuerStart
        Me.vector = New Point(neuesEnde.X - neuerStart.X, neuesEnde.Y - neuerStart.Y)
    End Sub

    Public Overrides Sub drehe(drehpunkt As Point, d As Drehmatrix)
        Dim p1 As New Point(position.X - drehpunkt.X, position.Y - drehpunkt.Y)
        Dim p2 As New Point(position.X + vector.X - drehpunkt.X, position.Y + vector.Y - drehpunkt.Y)
        p1 = d.transformPoint(p1)
        p2 = d.transformPoint(p2)

        Me.vector = New Point(p2.X - p1.X, p2.Y - p1.Y)
        Me.position = New Point(drehpunkt.X + p1.X, drehpunkt.Y + p1.Y)
    End Sub

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(isSelected)
        writer.Write(position.X)
        writer.Write(position.Y)
        writer.Write(vector.X)
        writer.Write(vector.Y)
        writer.Write(Me.linestyle)
        writer.Write(CInt(pfeilStart.pfeilArt))
        writer.Write(CInt(pfeilStart.pfeilSize))
        writer.Write(CInt(pfeilEnde.pfeilArt))
        writer.Write(CInt(pfeilEnde.pfeilSize))
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Wire
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim vecX As Integer = reader.ReadInt32()
        Dim vecY As Integer = reader.ReadInt32()
        Dim linestyle As Integer = reader.ReadInt32()
        Dim pStart As ParamArrow = New ParamArrow(-1, 100)
        Dim pEnde As ParamArrow = New ParamArrow(-1, 100)
        If version >= 10 Then
            pStart.pfeilArt = CShort(reader.ReadInt32())
            pStart.pfeilSize = CUShort(reader.ReadInt32())
            pEnde.pfeilArt = CShort(reader.ReadInt32())
            pEnde.pfeilSize = CUShort(reader.ReadInt32())
        ElseIf version >= 2 Then
            pStart.pfeilArt = CShort(reader.ReadInt32())
            pEnde.pfeilArt = CShort(reader.ReadInt32())
        End If
        If vecX <> 0 AndAlso vecY <> 0 Then
            Throw New Exception("Falsches Dateiformat (Fehler W1000)")
        End If

        Dim w As New Wire(sender.getNewID(), New Point(posX, posY), New Point(posX + vecX, posY + vecY))
        w.linestyle = linestyle
        w.isSelected = isSelected
        w.pfeilStart = pStart
        w.pfeilEnde = pEnde

        Return w
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Wire Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Wire)
            If .position <> Me.position Then Return False
            If .vector <> Me.vector Then Return False
            If .linestyle <> Me.linestyle Then Return False
            If Not .pfeilStart.isEqual(Me.pfeilStart) Then Return False
            If Not .pfeilEnde.isEqual(Me.pfeilEnde) Then Return False
        End With
        Return True
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As List(Of ElementEinstellung) = MyBase.getEinstellungen(sender)
        l.Add(New Einstellung_Pfeilspitze(EINSTELLUNG_PFEILSPITZEN, pfeilStart, pfeilEnde))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Pfeilspitze AndAlso e.Name.get_ID() = EINSTELLUNG_PFEILSPITZEN Then
                With DirectCast(e, Einstellung_Pfeilspitze)
                    If .startChanged Then
                        pfeilStart.pfeilArt = .pfeilStart.pfeilArt
                        changed = True
                    End If
                    If .endeChanged Then
                        pfeilEnde.pfeilArt = .pfeilEnde.pfeilArt
                        changed = True
                    End If
                    If .startSizeChanged Then
                        pfeilStart.pfeilSize = .pfeilStart.pfeilSize
                        changed = True
                    End If
                    If .endeSizeChanged Then
                        pfeilEnde.pfeilSize = .pfeilEnde.pfeilSize
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function
End Class

Public Structure PfeilWirePos
    Public pos_relativ As Single
    Public align As Pfeilspitze.AlignPfeil
    Public pfeilArt As Integer
End Structure