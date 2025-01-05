Imports System.IO

Public Class Basic_Bezier
    Inherits ElementWithStrokeFill

    '4 Punkte definieren eine Bezier Kurve
    'mybase.position definiert den Punkt 1 und mybase.position + vec<i> definiert den Punkt 2 bis 4
    Private vec2, vec3, vec4 As Point
    Private pfeilStart As ParamArrow
    Private pfeilEnde As ParamArrow

    Public Sub New(ID As ULong, linestyle As Integer, p1 As Point, p2 As Point, p3 As Point, p4 As Point, pfeilStart As ParamArrow, pfeilEnde As ParamArrow)
        MyBase.New(ID, linestyle, 0)
        Me.position = p1
        Me.vec2 = New Point(p2.X - p1.X, p2.Y - p1.Y)
        Me.vec3 = New Point(p3.X - p1.X, p3.Y - p1.Y)
        Me.vec4 = New Point(p4.X - p1.X, p4.Y - p1.Y)

        Me.pfeilStart = pfeilStart
        Me.pfeilEnde = pfeilEnde
    End Sub

    Public Function getStart() As Point
        Return Me.position
    End Function

    Public Function getP2() As Point
        Return New Point(Me.position.X + vec2.X, Me.position.Y + vec2.Y)
    End Function

    Public Function getP3() As Point
        Return New Point(Me.position.X + vec3.X, Me.position.Y + vec3.Y)
    End Function

    Public Function getEnde() As Point
        Return New Point(Me.position.X + vec4.X, Me.position.Y + vec4.Y)
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        Dim start As Point = Me.position
        Dim p2 As Point = getP2()
        Dim p3 As Point = getP3()
        Dim ende As Point = getEnde()

        start.X -= drehpunkt.X
        start.Y -= drehpunkt.Y
        p2.X -= drehpunkt.X
        p2.Y -= drehpunkt.Y
        p3.X -= drehpunkt.X
        p3.Y -= drehpunkt.Y
        ende.X -= drehpunkt.X
        ende.Y -= drehpunkt.Y

        start = drehung.transformPoint(start)
        p2 = drehung.transformPoint(p2)
        p3 = drehung.transformPoint(p3)
        ende = drehung.transformPoint(ende)

        start.X += drehpunkt.X
        start.Y += drehpunkt.Y
        p2.X += drehpunkt.X
        p2.Y += drehpunkt.Y
        p3.X += drehpunkt.X
        p3.Y += drehpunkt.Y
        ende.X += drehpunkt.X
        ende.Y += drehpunkt.Y

        Me.position = start
        Me.vec2 = New Point(p2.X - start.X, p2.Y - start.Y)
        Me.vec3 = New Point(p3.X - start.X, p3.Y - start.Y)
        Me.vec4 = New Point(ende.X - start.X, ende.Y - start.Y)
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim g As DO_Grafik
        Dim p1 As Point = Me.getStart()
        Dim p2 As Point = Me.getP2()
        Dim p3 As Point = Me.getP3()
        Dim p4 As Point = Me.getEnde()

        g = Pfeil_Verwaltung.getVerwaltung().getBezierWithPfeil(p1, p2, p3, p4, pfeilStart, pfeilEnde)
        g.lineStyle.linestyle = Me.linestyle
        If TypeOf g Is DO_MultiGrafik Then
            DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(Me.linestyle)
        End If
        Return g
    End Function

    Public Overrides Function getSelection() As Selection
        Dim p1 As Point = getStart()
        Dim p2 As Point = Me.getP2()
        Dim p3 As Point = Me.getP3()
        Dim p4 As Point = getEnde()
        Return New SelectionBezier(p1, p2, p3, p4)
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim v As New Basic_Bezier(newID, linestyle, Me.getStart(), Me.getP2(), Me.getP3(), Me.getEnde(), pfeilStart.CopyPfeil(), pfeilEnde.CopyPfeil())
        v.isSelected = Me.isSelected
        Return v
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Basic_Bezier Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Basic_Bezier)
            If .linestyle <> Me.linestyle Then Return False
            If .position <> Me.position Then Return False
            If .vec2 <> Me.vec2 Then Return False
            If .vec3 <> Me.vec3 Then Return False
            If .vec4 <> Me.vec4 Then Return False
            If Not .pfeilEnde.isEqual(Me.pfeilEnde) Then Return False
            If Not .pfeilStart.isEqual(Me.pfeilStart) Then Return False
        End With
        Return True
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(getStart().X)
        writer.Write(getStart().Y)
        writer.Write(getP2().X)
        writer.Write(getP2().Y)
        writer.Write(getP3().X)
        writer.Write(getP3().Y)
        writer.Write(getEnde().X)
        writer.Write(getEnde().Y)

        writer.Write(CInt(pfeilEnde.pfeilArt))
        writer.Write(CInt(pfeilEnde.pfeilSize))
        writer.Write(CInt(pfeilStart.pfeilArt))
        writer.Write(CInt(pfeilStart.pfeilSize))

        writer.Write(linestyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Basic_Bezier
        Dim sx As Integer = reader.ReadInt32()
        Dim sy As Integer = reader.ReadInt32()
        Dim p2x As Integer = reader.ReadInt32()
        Dim p2y As Integer = reader.ReadInt32()
        Dim p3x As Integer = reader.ReadInt32()
        Dim p3y As Integer = reader.ReadInt32()
        Dim ex As Integer = reader.ReadInt32()
        Dim ey As Integer = reader.ReadInt32()

        Dim pfeilEnde As New ParamArrow(-1, 100)
        Dim pfeilStart As New ParamArrow(-1, 100)
        pfeilEnde.pfeilArt = CShort(reader.ReadInt32())
        pfeilEnde.pfeilSize = CUShort(reader.ReadInt32())
        pfeilStart.pfeilArt = CShort(reader.ReadInt32())
        pfeilStart.pfeilSize = CUShort(reader.ReadInt32())

        Dim ls As Integer = reader.ReadInt32()

        Dim erg As New Basic_Bezier(sender.getNewID(), ls, New Point(sx, sy), New Point(p2x, p2y), New Point(p3x, p3y), New Point(ex, ey), pfeilStart, pfeilEnde)

        Return erg
    End Function

#Region "Scale"
    Public Overrides Function getScaleKantenCount() As Integer
        Return 4
    End Function

    Public Overrides Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        Dim gespiegelt As Boolean = False
        If alteKante IsNot Nothing AndAlso alteKante.gespiegeltXY Then
            gespiegelt = True
        End If
        If index = 0 Then
            Return New Kante(Me.getStart(), New Point(0, 0), 0, True, False, Me, True)
        ElseIf index = 1 Then
            Return New Kante(Me.getEnde(), New Point(0, 0), 1, True, False, Me, True)
        ElseIf index = 2 Then
            Dim k As New Kante(Me.getP2(), Me.getStart(), 2, True, False, Me, True, True)
            k.gespiegeltXY = gespiegelt
            Return k
        ElseIf index = 3 Then
            Dim k As New Kante(Me.getP3(), Me.getEnde(), 3, True, False, Me, True, True)
            k.gespiegeltXY = gespiegelt
            Return k
        End If
        Throw New IndexOutOfRangeException()
    End Function

    Public Overrides Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        If kante.gespiegeltXY Then
            dx = -dx
            dy = -dy
        End If
        'Index 0 und 1 muss immer der Start und Endpunkt bleiben! Wird bei ToolScale (bzw. Vectorpicturebox.getNextScaleKante()) genutzt, um benachbarte bezierkurven mitzuverschieben (bei start und ende)
        If kante.KantenIndex = 0 Then
            'start
            Me.position = New Point(Me.position.X + dx, Me.position.Y + dy)
            'Vec2 gleich lassen, sodass sich der Startpunkt immer zusammen mit dem ersten Kontrollpunkt verschiebt!!
            Me.vec3 = New Point(Me.vec3.X - dx, Me.vec3.Y - dy)
            Me.vec4 = New Point(Me.vec4.X - dx, Me.vec4.Y - dy)
        ElseIf kante.KantenIndex = 2 Then
            'p2
            Me.vec2 = New Point(Me.vec2.X + dx, Me.vec2.Y + dy)
        ElseIf kante.KantenIndex = 3 Then
            'p3
            Me.vec3 = New Point(Me.vec3.X + dx, Me.vec3.Y + dy)
        ElseIf kante.KantenIndex = 1 Then
            'ende
            Me.vec4 = New Point(Me.vec4.X + dx, Me.vec4.Y + dy)
            Me.vec3 = New Point(Me.vec3.X + dx, Me.vec3.Y + dy)
            'Vec3 zusammen mit vec4 verschieben, sodass sich der endpunkt immer zusammen mit dem zweiten Kontrollpunkt verschiebt!!
        Else
            Return False
        End If
        out_invalidate_screen = True
        Return True
    End Function
#End Region

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox, mode As ElementEinstellung.combineModus) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        l.Add(New Einstellung_Pfeilspitze(Element.EINSTELLUNG_PFEILSPITZEN, pfeilStart, pfeilEnde))
        MyBase.addEinstellungenStrokeFill(sender, l)
        l.AddRange(MyBase.getEinstellungen(sender, mode))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Pfeilspitze AndAlso e.Name.get_ID() = Element.EINSTELLUNG_PFEILSPITZEN Then
                With DirectCast(e, Einstellung_Pfeilspitze)
                    If .startChanged Then
                        Me.pfeilStart.pfeilArt = .pfeilStart.pfeilArt
                        changed = True
                    End If
                    If .endeChanged Then
                        Me.pfeilEnde.pfeilArt = .pfeilEnde.pfeilArt
                        changed = True
                    End If
                    If .startSizeChanged Then
                        Me.pfeilStart.pfeilSize = .pfeilStart.pfeilSize
                        changed = True
                    End If
                    If .endeSizeChanged Then
                        Me.pfeilEnde.pfeilSize = .pfeilEnde.pfeilSize
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function

    Public Overrides Function Hat_Fillstyle() As Boolean
        Return False
    End Function
End Class
