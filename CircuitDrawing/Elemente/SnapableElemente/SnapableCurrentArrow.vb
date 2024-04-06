Imports System.IO

Public Class SnapableCurrentArrow
    Inherits SnapableElement
    Implements IElementWithFont
    Implements IElementWithLinestyle

    Public Const DEFAULT_ART As StromPfeilArt = StromPfeilArt.OnWire

    Private Const ABSTAND_PFEIL As Integer = 150
    Private Const RADIUS_PFEIL As Integer = 225

    Public Const DEFAULT_ABSTAND_BESCHRIFTUNG As Integer = 40
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0

    Private pos As WireSnappoint
    Private beschriftung As Beschriftung
    Public linestyle As Integer
    Private fontstyle As Integer

    Private pfeilspitze As ParamArrow

    Private myArt As StromPfeilArt

    Public Sub New(ID As ULong, pfeilspitze As ParamArrow, p As WireSnappoint, beschriftung As Beschriftung, linestyle As Integer, art As StromPfeilArt, fontstyle As Integer)
        MyBase.New(ID)
        Me.pfeilspitze = pfeilspitze
        Me.pos = p
        Me.beschriftung = beschriftung
        Me.linestyle = linestyle
        Me.myArt = art
        Me.fontstyle = fontstyle
    End Sub

    Public Overrides Function getNrOfSnappoints() As Integer
        Return 1
    End Function

    Public Overrides Function getSnappoint(index As Integer) As WireSnappoint
        Return pos
    End Function

    Public Overrides Sub setSnappoint(index As Integer, s As WireSnappoint)
        pos = s
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        Dim g As New DO_MultiGrafik()
        If myArt = StromPfeilArt.OnWire Then
            g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getPfeilOnLine(pos, pfeilspitze))

            AddBeschriftungToGrafik(g, beschriftung, pos.wireStart, pos.wireEnd, pos.alpha, beschriftung.abstand, beschriftung.abstandQuer, Me.fontstyle)
        Else
            Dim p As PointF = Me.pos.getMitteF()
            Dim normal As PointF = getNormal(Me.pos.wireStart, Me.pos.wireEnd, Me.beschriftung.positionIndex)
            Dim normal_n As PointF = getn_Normal(normal)
            If Me.beschriftung.positionIndex = 1 Then
                normal_n.X *= -1
                normal_n.Y *= -1
            End If

            Dim start As New PointF(p.X + normal.X * ABSTAND_PFEIL + normal_n.X * RADIUS_PFEIL, p.Y + normal.Y * ABSTAND_PFEIL + normal_n.Y * RADIUS_PFEIL)
            Dim ende As New PointF(p.X + normal.X * ABSTAND_PFEIL - normal_n.X * RADIUS_PFEIL, p.Y + normal.Y * ABSTAND_PFEIL - normal_n.Y * RADIUS_PFEIL)

            g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(New Point(CInt(start.X), CInt(start.Y)), New Point(CInt(ende.X), CInt(ende.Y)), New ParamArrow(-1, 100), pfeilspitze))
            AddBeschriftungToGrafik(g, beschriftung, pos.wireStart, pos.wireEnd, pos.alpha, beschriftung.abstand + ABSTAND_PFEIL, beschriftung.abstandQuer, Me.fontstyle, normal)
        End If

        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
        pos.setLastVectorGroeßerNull_DuringDraw()
        Return g
    End Function

    Public Shared Function getNormal(start As Point, ende As Point, positionsindex As Integer) As PointF
        Dim normal As New PointF(start.Y - ende.Y, ende.X - start.X)
        Dim laengeNormal As Single = CSng(Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y))
        If laengeNormal = 0 Then
            normal = New PointF(0.0F, 1.0F)
            laengeNormal = 1.0F
        End If
        normal.X /= laengeNormal
        normal.Y /= laengeNormal
        If positionsindex = 1 Then
            normal.X *= -1.0F
            normal.Y *= -1.0F
        End If
        Return normal
    End Function

    Public Shared Function getn_Normal(normal As PointF) As PointF
        Return New PointF(-normal.Y, normal.X)
    End Function

    Public Shared Sub AddBeschriftungToGrafik(g As DO_MultiGrafik, b As Beschriftung, start As Point, ende As Point, alpha As Single, dist As Integer, distQuer As Integer, fontstyle As Integer, normal As PointF)
        If b.positionIndex <= 1 AndAlso b.positionIndex >= 0 Then

            Dim pos_mitte As New PointF(start.X + alpha * (ende.X - start.X), start.Y + alpha * (ende.Y - start.Y))

            Dim pos_text As New PointF(pos_mitte.X + normal.X * dist, pos_mitte.Y + normal.Y * dist)
            Dim n_normal As PointF = getn_Normal(normal)
            pos_text.X += n_normal.X * distQuer
            pos_text.Y += n_normal.Y * distQuer

            Dim ah As DO_Text.AlignH
            Dim av As DO_Text.AlignV
            Dim winkel As Double = Math.Atan2(normal.Y, normal.X) * 180 / Math.PI
            If winkel < 0 Then
                winkel += 360
            End If
            winkel = 360 - winkel
            If winkel < 22.5 OrElse winkel > 337.5 Then
                ah = DO_Text.AlignH.Links
                av = DO_Text.AlignV.Mitte
            ElseIf winkel < 67.5 Then
                ah = DO_Text.AlignH.Links
                av = DO_Text.AlignV.Unten
            ElseIf winkel < 112.5 Then
                ah = DO_Text.AlignH.Mitte
                av = DO_Text.AlignV.Unten
            ElseIf winkel < 157.5 Then
                ah = DO_Text.AlignH.Rechts
                av = DO_Text.AlignV.Unten
            ElseIf winkel < 202.5 Then
                ah = DO_Text.AlignH.Rechts
                av = DO_Text.AlignV.Mitte
            ElseIf winkel < 247.5 Then
                ah = DO_Text.AlignH.Rechts
                av = DO_Text.AlignV.Oben
            ElseIf winkel < 292.5 Then
                ah = DO_Text.AlignH.Mitte
                av = DO_Text.AlignV.Oben
            Else
                ah = DO_Text.AlignH.Links
                av = DO_Text.AlignV.Oben
            End If

            Dim gText As New DO_Text(b.text, fontstyle, New Point(CInt(pos_text.X), CInt(pos_text.Y)), ah, av, b.textRot, False)

            g.childs.Add(gText)
        End If
    End Sub

    Public Shared Sub AddBeschriftungToGrafik(g As DO_MultiGrafik, b As Beschriftung, start As Point, ende As Point, alpha As Single, dist As Integer, distQuer As Integer, fontstyle As Integer)
        Dim normal As PointF = getNormal(start, ende, b.positionIndex)
        AddBeschriftungToGrafik(g, b, start, ende, alpha, dist, distQuer, fontstyle, normal)
    End Sub

    Public Function schalteBeschriftungsPosDurch() As Boolean
        beschriftung.positionIndex += 1
        If beschriftung.positionIndex >= 2 Then
            beschriftung.positionIndex = 0
        End If
        Return True
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        pos.drehe(drehpunkt, drehung)
    End Sub

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim s As New SnapableCurrentArrow(newID, pfeilspitze.CopyPfeil(), pos.clone(), beschriftung, linestyle, myArt, fontstyle)
        Return s
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot SnapableCurrentArrow Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, SnapableCurrentArrow)
            If Not .pfeilspitze.isEqual(Me.pfeilspitze) Then Return False
            If Not .pos.isEqualWithoutSelection(Me.pos) Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If .linestyle <> Me.linestyle Then Return False
            If .myArt <> Me.myArt Then Return False
            If .fontstyle <> Me.fontstyle Then Return False
        End With
        Return True
    End Function

    Public Function getBeschriftung_Text() As String
        Return beschriftung.text
    End Function

    Public Sub setBeschriftung_Text(val As String)
        beschriftung.text = val
    End Sub

    Public Overrides Sub speichern(writer As BinaryWriter)
        pos.speichern(writer)
        beschriftung.speichern(writer)
        writer.Write(linestyle)
        writer.Write(CInt(pfeilspitze.pfeilArt))
        writer.Write(CInt(pfeilspitze.pfeilSize))
        writer.Write(CInt(Me.myArt))
        writer.Write(fontstyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As SnapableCurrentArrow
        Dim pos As WireSnappoint = WireSnappoint.Einlesen(sender, reader, version)
        Dim beschriftung As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim ls As Integer = reader.ReadInt32()
        Dim pfeil As New ParamArrow(-1, 100)
        If version >= 10 Then
            pfeil.pfeilArt = CShort(reader.ReadInt32())
            pfeil.pfeilSize = CUShort(reader.ReadInt32())
        Else
            pfeil.pfeilArt = CShort(reader.ReadInt32())
        End If

        Dim myArt As StromPfeilArt = CType(reader.ReadInt32(), StromPfeilArt)
        Dim fontstyle As Integer = 0
        If version >= 11 Then
            fontstyle = reader.ReadInt32()
        End If
        Dim res As New SnapableCurrentArrow(sender.getNewID(), pfeil, pos, beschriftung, ls, myArt, fontstyle)
        Return res
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As List(Of ElementEinstellung) = MyBase.getEinstellungen(sender)
        l.Add(New EinstellungStrompfeilArt(My.Resources.Strings.Einstellung_ArtDesStrompfeiles, myArt))
        beschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.fontstyle, sender.myFonts))
        l.Add(New Einstellung_SinglePfeilspitze(Element.EINSTELLUNG_SINGLEPFEILSPITZE, pfeilspitze))
        l.Add(New Einstellung_Linienstil(Element.EINSTELLUNG_LINESTYLE, linestyle, sender.myLineStyles))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Linienstil AndAlso e.Name = Element.EINSTELLUNG_LINESTYLE Then
                Me.linestyle = DirectCast(e, Einstellung_Linienstil).getNewLinienstil(Me.linestyle, sender.myLineStyles, changed, False)
            ElseIf TypeOf e Is Einstellung_SinglePfeilspitze AndAlso e.Name = Element.EINSTELLUNG_SINGLEPFEILSPITZE Then
                With DirectCast(e, Einstellung_SinglePfeilspitze)
                    If .Changed Then
                        Me.pfeilspitze.pfeilArt = .pfeil.pfeilArt
                        changed = True
                    End If
                    If .changedSize Then
                        Me.pfeilspitze.pfeilSize = .pfeil.pfeilSize
                        changed = True
                    End If
                End With
            ElseIf TypeOf e Is EinstellungStrompfeilArt AndAlso e.Name = My.Resources.Strings.Einstellung_ArtDesStrompfeiles Then
                With DirectCast(e, EinstellungStrompfeilArt)
                    If .changedArt Then
                        Me.myArt = .myArt
                        changed = True
                    End If
                End With
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name = Element.EINSTELLUNG_FONTSTYLE Then
                Me.fontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.fontstyle, sender.myFonts, changed, False)
            ElseIf beschriftung.setEinstellung(e) Then
                changed = True
            End If
        Next
        Return changed
    End Function

    Public Function get_fontstyle() As Integer Implements IElementWithFont.get_fontstyle
        Return Me.fontstyle
    End Function

    Public Sub set_fontstyle(fs As Integer) Implements IElementWithFont.set_fontstyle
        Me.fontstyle = fs
    End Sub

    Public Function get_mylinestyle() As Integer Implements IElementWithLinestyle.get_mylinestyle
        Return Me.linestyle
    End Function

    Public Sub set_mylinestyle(ls As Integer) Implements IElementWithLinestyle.set_mylinestyle
        Me.linestyle = ls
    End Sub

    Public Enum StromPfeilArt
        OnWire
        NextToWire
    End Enum

End Class
