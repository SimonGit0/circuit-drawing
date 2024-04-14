Imports System.IO

Public Class SnapableBusTap
    Inherits SnapableElement
    Implements IElementWithFont
    Implements IElementWithLinestyle

    Public Const DEFAULT_ABSTAND_BESCHRIFTUNG As Integer = 150
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0

    Private pos As WireSnappoint
    Private beschriftung As Beschriftung
    Private fontstyle As Integer
    Private linestyle As Integer

    Public Sub New(ID As ULong, p As WireSnappoint, beschriftung As Beschriftung, fontstyle As Integer, linestyle As Integer)
        MyBase.New(ID)
        Me.pos = p
        Me.beschriftung = beschriftung
        Me.fontstyle = fontstyle
        Me.linestyle = linestyle
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

        Dim delta As Point = New Point(pos.wireEnd.X - pos.wireStart.X, pos.wireEnd.Y - pos.wireStart.Y)
        If Math.Abs(delta.X) > 0 OrElse Math.Abs(delta.Y) > 0 Then
            Dim länge As Double = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y)
            Dim vec As New PointF(CSng(delta.X / länge), CSng(delta.Y / länge))
            Dim norm As New PointF(-Math.Abs(vec.Y), Math.Abs(vec.X))
            Dim mitte As PointF = pos.getMitteF()
            Dim vecGesamt As New PointF(vec.X * 50 + norm.X * 150, vec.Y * 50 + norm.Y * 150)
            Dim p1 As New Point(CInt(mitte.X - vecGesamt.X), CInt(mitte.Y - vecGesamt.Y))
            Dim p2 As New Point(CInt(mitte.X + vecGesamt.X), CInt(mitte.Y + vecGesamt.Y))
            g.childs.Add(New DO_Linie(p1, p2, False))
        End If
        SnapableCurrentArrow.AddBeschriftungToGrafik(g, beschriftung, pos.wireStart, pos.wireEnd, pos.alpha, beschriftung.abstand, beschriftung.abstandQuer, Me.fontstyle)
        pos.setLastVectorGroeßerNull_DuringDraw()

        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)

        Return g
    End Function

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
        Dim s As New SnapableBusTap(newID, pos.clone(), beschriftung, fontstyle, linestyle)
        Return s
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot SnapableBusTap Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, SnapableBusTap)
            If Not .pos.isEqualWithoutSelection(Me.pos) Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If .fontstyle <> Me.fontstyle Then Return False
            If .linestyle <> Me.linestyle Then Return False
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
        writer.Write(fontstyle)
        writer.Write(linestyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As SnapableBusTap
        Dim pos As WireSnappoint = WireSnappoint.Einlesen(sender, reader, version)
        Dim beschriftung As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim fontstyle As Integer = 0
        Dim linestyle As Integer = 0
        If version >= 11 Then
            fontstyle = reader.ReadInt32()
        End If
        If version >= 29 Then
            linestyle = reader.ReadInt32()
        End If
        Dim res As New SnapableBusTap(sender.getNewID(), pos, beschriftung, fontstyle, linestyle)
        Return res
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As List(Of ElementEinstellung) = MyBase.getEinstellungen(sender)
        beschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.fontstyle, sender.myFonts))
        l.Add(New Einstellung_Linienstil(Element.EINSTELLUNG_LINESTYLE, linestyle, sender.myLineStyles))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If beschriftung.setEinstellung(e) Then
                changed = True
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = Element.EINSTELLUNG_FONTSTYLE Then
                Me.fontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.fontstyle, sender.myFonts, changed, False)
            ElseIf TypeOf e Is Einstellung_Linienstil AndAlso e.Name.get_ID() = Element.EINSTELLUNG_LINESTYLE Then
                Me.linestyle = DirectCast(e, Einstellung_Linienstil).getNewLinienstil(Me.linestyle, sender.myLineStyles, changed, False)
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
End Class
