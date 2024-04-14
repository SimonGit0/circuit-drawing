Imports System.IO

Public Class SnapableLabel
    Inherits SnapableElement
    Implements IElementWithFont

    Public Const DEFAULT_ABSTAND_BESCHRIFTUNG As Integer = 40
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0

    Private pos As WireSnappoint
    Private beschriftung As Beschriftung
    Private fontstyle As Integer

    Public Sub New(ID As ULong, p As WireSnappoint, beschriftung As Beschriftung, fontstyle As Integer)
        MyBase.New(ID)
        Me.pos = p
        Me.beschriftung = beschriftung
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
        SnapableCurrentArrow.AddBeschriftungToGrafik(g, beschriftung, pos.wireStart, pos.wireEnd, pos.alpha, beschriftung.abstand, beschriftung.abstandQuer, fontstyle)
        pos.setLastVectorGroeßerNull_DuringDraw()
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
        Dim s As New SnapableLabel(newID, pos.clone(), beschriftung, fontstyle)
        Return s
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot SnapableLabel Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, SnapableLabel)
            If Not .pos.isEqualWithoutSelection(Me.pos) Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If Me.fontstyle <> .fontstyle Then Return False
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
        writer.Write(Me.fontstyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As SnapableLabel
        Dim pos As WireSnappoint = WireSnappoint.Einlesen(sender, reader, version)
        Dim beschriftung As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim fontstyle As Integer = 0
        If version >= 11 Then
            fontstyle = reader.ReadInt32()
        End If
        Dim res As New SnapableLabel(sender.getNewID(), pos, beschriftung, fontstyle)
        Return res
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As List(Of ElementEinstellung) = MyBase.getEinstellungen(sender)
        beschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.fontstyle, sender.myFonts))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If beschriftung.setEinstellung(e) Then
                changed = True
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = Element.EINSTELLUNG_FONTSTYLE Then
                Me.fontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.fontstyle, sender.myFonts, changed, False)
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
End Class
