Imports System.IO

Public Class WireLuftlinie
    Inherits ElementLinestyle
    Implements IWire

    Public vector As Point
    Public pfeilStart As ParamArrow
    Public pfeilEnde As ParamArrow

    Public Sub New(ID As ULong, start As Point, ende As Point)
        MyBase.New(ID, 1)
        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
        Me.pfeilEnde = New ParamArrow(-1, 100)
        Me.pfeilStart = New ParamArrow(-1, 100)
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        Dim g As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(getStart(), getEnde(), pfeilStart, pfeilEnde)
        If TypeOf g Is DO_MultiGrafik Then
            g.lineStyle.linestyle = Me.linestyle
            DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(Me.linestyle)
        Else
            g.lineStyle.linestyle = Me.linestyle
        End If
        Return g
    End Function

    Public Function getGrafikMitZeilensprüngen(radius As Integer, allWires As List(Of IWire)) As DO_Grafik Implements IWire.getGrafikMitZeilensprüngen
        If vector.X = 0 Then
            'Zeilensprünge nicht bei vertikalen wires
            Return getGrafik()
        End If

        Dim myEnde As Point = Me.getEnde()
        Dim myStart As Point = Me.getStart()

        Dim yMin As Integer = Math.Min(myStart.Y, myEnde.Y)
        Dim yMax As Integer = Math.Max(myStart.Y, myEnde.Y)
        Dim xMin As Integer = Math.Min(myStart.X, myEnde.X)
        Dim xMax As Integer = Math.Max(myStart.X, myEnde.X)

        Dim det As Long
        Dim alpha, beta As Double
        Dim myWinkel As Double = Math.Atan2(Math.Abs(myEnde.Y - myStart.Y), Math.Abs(myEnde.X - myStart.X))

        Dim zeilensprünge As New List(Of Double)
        For i As Integer = 0 To allWires.Count - 1
            If Not allWires(i).Equals(Me) Then
                Dim start As Point = allWires(i).getStart()
                Dim ende As Point = allWires(i).getEnde()
                Dim yMin2 As Integer = Math.Min(start.Y, ende.Y)
                Dim yMax2 As Integer = Math.Max(start.Y, ende.Y)
                Dim xMin2 As Integer = Math.Min(start.X, ende.X)
                Dim xMax2 As Integer = Math.Max(start.X, ende.X)
                If yMax2 >= yMin AndAlso yMin2 <= yMax AndAlso xMax2 >= xMin AndAlso xMin2 <= xMax Then
                    'nur wenn das wire im Bereich liegt kann es einen Schnittpunkt geben!

                    'Schauen welches Wire "steiler" ist
                    beta = Math.Atan2(Math.Abs(ende.Y - start.Y), Math.Abs(ende.X - start.X))
                    If myWinkel < beta Then
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
            End If
        Next

        Return Me.getGrafikMitZeilensprüngen(radius, zeilensprünge)
    End Function

    Public Function getGrafikMitZeilensprüngen(radius As Integer, sprünge As List(Of Double)) As DO_Grafik
        If sprünge Is Nothing OrElse sprünge.Count < 1 Then
            Return getGrafik()
        End If

        sprünge.Sort()
        Dim g As New DO_MultiGrafik()
        Dim p1, p2 As Point
        Dim start As Point = getStart()
        Dim ende As Point = getEnde()
        Dim len As Double = Math.Sqrt(CDbl(ende.X - start.X) * (ende.X - start.X) + CDbl(ende.Y - start.Y) * (ende.Y - start.Y))
        If len = 0 Then Return getGrafik()
        Dim faktor As Double = radius / len

        Dim startwinkel As Single = CSng(Math.Atan2(vector.Y, vector.X) * 180 / Math.PI)
        If startwinkel < 0 Then startwinkel += 360
        If Math.Abs(startwinkel - 180) > 90 Then
            startwinkel = startwinkel + 180
            If startwinkel >= 360 Then startwinkel -= 360
        End If

        'Linie vom Start zum ersten Zwischenpunkt
        p1 = start
        If sprünge(0) > faktor Then
            p2 = New Point(CInt(start.X + (sprünge(0) - faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(0) - faktor) * (ende.Y - start.Y)))
        Else
            p2 = p1
        End If
        g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, pfeilStart, New ParamArrow(-1, 100)))
        g.childs.Add(addZeilensprung(radius, sprünge(0), startwinkel))
        'Linien zwischendrinnen
        For i As Integer = 1 To sprünge.Count - 1
            If sprünge(i) - sprünge(i - 1) > 2 * faktor Then
                p1 = New Point(CInt(start.X + (sprünge(i - 1) + faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(i - 1) + faktor) * (ende.Y - start.Y)))
                p2 = New Point(CInt(start.X + (sprünge(i) - faktor) * (ende.X - start.X)), CInt(start.Y + (sprünge(i) - faktor) * (ende.Y - start.Y)))
                g.childs.Add(New DO_Linie(p1, p2, False))
            End If
            g.childs.Add(addZeilensprung(radius, sprünge(i), startwinkel))
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

    Private Function addZeilensprung(radius As Integer, stelle As Double, startwinkel As Single) As DO_Arc
        Dim pos As New Point(CInt(position.X + stelle * vector.X), CInt(position.Y + stelle * vector.Y))
        Return New DO_Arc(pos, radius, radius, startwinkel, 180, False, False, Drawing_FillMode.OnlyStroke)
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
        Dim w As New WireLuftlinie(newID, getStart(), getEnde())
        w.isSelected = Me.isSelected
        w.linestyle = Me.linestyle
        w.pfeilStart = Me.pfeilStart.CopyPfeil()
        w.pfeilEnde = Me.pfeilEnde.CopyPfeil()
        Return w
    End Function

    Public Function abstand(p As PointF) As Double
        Return Mathe.distLiniePoint(getStart(), getEnde(), p)
    End Function

    Public Function getStart() As Point Implements IWire.getStart
        Return position
    End Function

    Public Function getEnde() As Point Implements IWire.getEnde
        Return New Point(position.X + vector.X, position.Y + vector.Y)
    End Function

    Public Sub moveStart(neuerStart As Point)
        Dim ende As Point = getEnde()
        Me.position = neuerStart
        Me.vector = New Point(ende.X - neuerStart.X, ende.Y - neuerStart.Y)
    End Sub

    Public Sub moveEnde(neuesEnde As Point)
        Me.vector = New Point(neuesEnde.X - position.X, neuesEnde.Y - position.Y)
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
        writer.Write(linestyle)
        writer.Write(CInt(pfeilStart.pfeilArt))
        writer.Write(CInt(pfeilStart.pfeilSize))
        writer.Write(CInt(pfeilEnde.pfeilArt))
        writer.Write(CInt(pfeilEnde.pfeilSize))
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As WireLuftlinie
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

        Dim w As New WireLuftlinie(sender.getNewID(), New Point(posX, posY), New Point(posX + vecX, posY + vecY))
        w.isSelected = isSelected
        w.linestyle = linestyle
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
