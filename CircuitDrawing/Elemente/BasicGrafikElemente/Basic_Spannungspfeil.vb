Imports System.IO

Public Class Basic_Spannungspfeil
    Inherits ElementWithStrokeFill

    Public Const DEFAULT_ABSTAND_BESCHRIFTUNG As Integer = 40
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0

    Public Const DEFAULT_KRÜMMUNG As Integer = 10
    Public Const DEFAULT_ECKIGKEIT As Integer = 25

    Public Const DEFAULT_LINESTYLE As Integer = 3

    Private vector As Point
    Private beschriftung As Beschriftung
    Private pfeilStart As ParamArrow
    Private pfeilEnde As ParamArrow

    Private krümmung As Integer
    Private eckigkeit As Integer
    Private fontstyle As Integer

    Public Sub New(ID As ULong, linestyle As Integer, start As Point, ende As Point, pfeilStart As ParamArrow, pfeilEnde As ParamArrow, beschriftung As Beschriftung, krümmung As Integer, eckigkeit As Integer, fontstyle As Integer)
        MyBase.New(ID, linestyle, 0)
        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
        Me.beschriftung = beschriftung
        Me.pfeilStart = pfeilStart
        Me.pfeilEnde = pfeilEnde
        Me.krümmung = krümmung
        Me.eckigkeit = eckigkeit
        Me.fontstyle = fontstyle
    End Sub

    Public Function getStart() As Point
        Return Me.position
    End Function

    Public Function getEnde() As Point
        Return New Point(Me.position.X + vector.X, Me.position.Y + vector.Y)
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        Dim start As Point = Me.position
        Dim ende As Point = getEnde()
        start.X -= drehpunkt.X
        start.Y -= drehpunkt.Y
        ende.X -= drehpunkt.X
        ende.Y -= drehpunkt.Y

        Dim norm As New Point(ende.Y - start.Y, start.X - ende.X)
        start = drehung.transformPoint(start)
        ende = drehung.transformPoint(ende)
        norm = drehung.transformPoint(norm)

        Dim norm_ist As New Point(ende.Y - start.Y, start.X - ende.X)

        If norm_ist.X <> norm.X OrElse norm_ist.Y <> norm.Y Then
            If beschriftung.positionIndex = 0 Then
                beschriftung.positionIndex = 1
            Else
                beschriftung.positionIndex = 0
            End If
        End If

        start.X += drehpunkt.X
        start.Y += drehpunkt.Y
        ende.X += drehpunkt.X
        ende.Y += drehpunkt.Y

        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim g As New DO_MultiGrafik()
        Dim start As Point = Me.getStart()
        Dim ende As Point = Me.getEnde()
        Dim längeSQ As Long = Mathe.abstandQuadrat(start, ende)
        If krümmung = 0 OrElse längeSQ = 0 Then
            g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(start, ende, pfeilStart, pfeilEnde))
            SnapableCurrentArrow.AddBeschriftungToGrafik(g, beschriftung, start, ende, 0.5, beschriftung.abstand, beschriftung.abstandQuer, fontstyle)
        Else
            Dim p2, p3 As Point
            Dim dist_normal As Double
            getBezierP2P3(p2, p3, dist_normal)
            g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getBezierWithPfeil(start, p2, p3, ende, pfeilStart, pfeilEnde))
            SnapableCurrentArrow.AddBeschriftungToGrafik(g, beschriftung, start, ende, 0.5, CInt(beschriftung.abstand + dist_normal), beschriftung.abstandQuer, fontstyle)
        End If
        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
        Return g
    End Function

    Private Sub getBezierP2P3(ByRef p2 As Point, ByRef p3 As Point, ByRef dist_normal As Double)
        Dim start As Point = Me.getStart()
        Dim ende As Point = Me.getEnde()
        Dim längeSQ As Long = Mathe.abstandQuadrat(start, ende)

        Dim normal As PointF = SnapableCurrentArrow.getNormal(start, ende, beschriftung.positionIndex)
        Dim länge As Double = Math.Sqrt(längeSQ)
        Dim vec As New PointF(CSng((ende.X - start.X) / länge), CSng((ende.Y - start.Y) / länge))

        dist_normal = länge * krümmung / 100
        Dim dist_Quer As Double = länge * eckigkeit / 100
        p2 = New Point(CInt(start.X + 4 / 3 * (normal.X * dist_normal + vec.X * dist_Quer)),
                       CInt(start.Y + 4 / 3 * (normal.Y * dist_normal + vec.Y * dist_Quer)))
        p3 = New Point(CInt(ende.X + 4 / 3 * (normal.X * dist_normal - vec.X * dist_Quer)),
                       CInt(ende.Y + 4 / 3 * (normal.Y * dist_normal - vec.Y * dist_Quer)))
    End Sub

    Public Overrides Function getSelection() As Selection
        Dim p1 As Point = getStart()
        Dim p4 As Point = getEnde()
        If krümmung = 0 OrElse p1 = p4 Then
            Return New SelectionLine(p1, p4)
        Else
            Dim p2, p3 As Point
            getBezierP2P3(p2, p3, 0.0)
            Return New SelectionBezier(p1, p2, p3, p4)
        End If
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim v As New Basic_Spannungspfeil(newID, linestyle, Me.getStart(), Me.getEnde(), pfeilStart.CopyPfeil(), pfeilEnde.CopyPfeil(), Me.beschriftung, Me.krümmung, Me.eckigkeit, Me.fontstyle)
        v.isSelected = Me.isSelected
        Return v
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Basic_Spannungspfeil Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Basic_Spannungspfeil)
            If .linestyle <> Me.linestyle Then Return False
            If .position <> Me.position Then Return False
            If .vector <> Me.vector Then Return False
            If .krümmung <> Me.krümmung Then Return False
            If .eckigkeit <> Me.eckigkeit Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If Not .pfeilEnde.isEqual(Me.pfeilEnde) Then Return False
            If Not .pfeilStart.isEqual(Me.pfeilStart) Then Return False
            If .fontstyle <> Me.fontstyle Then Return False
        End With
        Return True
    End Function

    Public Function schalteBeschriftungsPosDurch() As Boolean
        beschriftung.positionIndex += 1
        If beschriftung.positionIndex >= 2 Then
            beschriftung.positionIndex = 0
        End If
        Return True
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(getStart().X)
        writer.Write(getStart().Y)
        writer.Write(getEnde().X)
        writer.Write(getEnde().Y)
        beschriftung.speichern(writer)
        writer.Write(CInt(pfeilEnde.pfeilArt))
        writer.Write(CInt(pfeilEnde.pfeilSize))
        writer.Write(CInt(pfeilStart.pfeilArt))
        writer.Write(CInt(pfeilStart.pfeilSize))
        writer.Write(krümmung)
        writer.Write(eckigkeit)

        writer.Write(linestyle)
        writer.Write(fontstyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Basic_Spannungspfeil
        Dim sx As Integer = reader.ReadInt32()
        Dim sy As Integer = reader.ReadInt32()
        Dim ex As Integer = reader.ReadInt32()
        Dim ey As Integer = reader.ReadInt32()
        Dim b As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim pfeilEnde As New ParamArrow(-1, 100)
        Dim pfeilStart As New ParamArrow(-1, 100)
        If version >= 10 Then
            pfeilEnde.pfeilArt = CShort(reader.ReadInt32())
            pfeilEnde.pfeilSize = CUShort(reader.ReadInt32())
            pfeilStart.pfeilArt = CShort(reader.ReadInt32())
            pfeilStart.pfeilSize = CUShort(reader.ReadInt32())
        Else
            pfeilEnde.pfeilArt = CShort(reader.ReadInt32())
            pfeilStart.pfeilArt = CShort(reader.ReadInt32())
        End If
        Dim krümmung As Integer = reader.ReadInt32()
        Dim eckigkeit As Integer = reader.ReadInt32()

        Dim ls As Integer = 0
        If version >= 7 Then
            ls = reader.ReadInt32()
        End If
        Dim fs As Integer = 0
        If version >= 11 Then
            fs = reader.ReadInt32()
        End If
        Return New Basic_Spannungspfeil(sender.getNewID(), ls, New Point(sx, sy), New Point(ex, ey), pfeilStart, pfeilEnde, b, krümmung, eckigkeit, fs)
    End Function

#Region "Scale"
    Public Overrides Function getScaleKantenCount() As Integer
        Return 2
    End Function

    Public Overrides Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        If index = 0 Then
            Return New Kante(Me.getStart(), Me.getEnde(), 0, True, True, Me, False)
        ElseIf index = 1 Then
            Return New Kante(Me.getEnde(), Me.getStart(), 1, True, True, Me, False)
        End If
        Throw New IndexOutOfRangeException()
    End Function

    Public Overrides Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        If kante.KantenIndex = 0 Then
            'start
            Me.position = New Point(Me.position.X + dx, Me.position.Y + dy)
            Me.vector = New Point(Me.vector.X - dx, Me.vector.Y - dy)
        ElseIf kante.KantenIndex = 1 Then
            'ende
            Me.vector = New Point(Me.vector.X + dx, Me.vector.Y + dy)
        Else
            Return False
        End If
        out_invalidate_screen = True
        Return True
    End Function
#End Region

    Public Function getBeschriftung_Text() As String
        Return beschriftung.text
    End Function

    Public Sub setBeschriftung_Text(val As String)
        beschriftung.text = val
    End Sub

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox, mode As ElementEinstellung.combineModus) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        beschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.fontstyle, sender.myFonts))
        'l.Add(New Einstellung_SinglePfeilspitze(Element.EINSTELLUNG_SINGLEPFEILSPITZE, Me.pfeil))
        l.Add(New Einstellung_Pfeilspitze(Element.EINSTELLUNG_PFEILSPITZEN, pfeilStart, pfeilEnde))
        l.Add(New Einstellung_Pos(ElementEinstellung.SortierTyp.ElementEinstellung_Speziell, My.Resources.Strings.Einstellung_Krümmung, New Point(krümmung, eckigkeit), My.Resources.Strings.Einstellung_KrümmungAbstand, My.Resources.Strings.Einstellung_KrümmungRundheit, "%", "%"))
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
            ElseIf TypeOf e Is Einstellung_Pos AndAlso e.Name.get_ID() = My.Resources.Strings.Einstellung_Krümmung Then
                With DirectCast(e, Einstellung_Pos)
                    If .changedX Then
                        Me.krümmung = .pos.X
                        changed = True
                    End If
                    If .changedY Then
                        Me.eckigkeit = .pos.Y
                        changed = True
                    End If
                End With
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = EINSTELLUNG_FONTSTYLE Then
                Me.fontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.fontstyle, sender.myFonts, changed, False)
            ElseIf beschriftung.setEinstellung(e) Then
                changed = True
            End If
        Next
        Return changed
    End Function

    Public Overrides Function Hat_Fillstyle() As Boolean
        Return False
    End Function
End Class
