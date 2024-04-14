Imports System.IO

Public Class SnapableImpedanceArrow
    Inherits SnapableElement
    Implements IElementWithFont
    Implements IElementWithLinestyle

    Public Const DEFAULT_ABSTAND_PFEIL As Integer = 50
    Public Const DEFAULT_LAENGE_X As Integer = 150
    Public Const DEFAULT_LAENGE_Y As Integer = 130

    Public Const DEFAULT_ABSTAND_BESCHRIFTUNG As Integer = 40
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0

    Private pos As WireSnappoint
    Private beschriftung As Beschriftung
    Public linestyle As Integer
    Private fontstyle As Integer
    Private abstandPfeil As Integer
    Private längePfeilX As Integer
    Private längePfeilY As Integer

    Private pfeilspitze As ParamArrow


    Public Sub New(ID As ULong, pfeilspitze As ParamArrow, p As WireSnappoint, beschriftung As Beschriftung, linestyle As Integer, fontstyle As Integer, abstandPfeil As Integer, längePfeilX As Integer, längePfeilY As Integer)
        MyBase.New(ID)
        Me.pfeilspitze = pfeilspitze
        Me.pos = p
        Me.beschriftung = beschriftung
        Me.linestyle = linestyle
        Me.fontstyle = fontstyle

        Me.abstandPfeil = abstandPfeil
        Me.längePfeilX = längePfeilX
        Me.längePfeilY = längePfeilY
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

        Dim p As PointF = Me.pos.getMitteF()
        Dim normal As PointF = SnapableCurrentArrow.getNormal(Me.pos.wireStart, Me.pos.wireEnd, Me.beschriftung.positionIndex)
        normal.X *= -1
        normal.Y *= -1
        Dim normal_n As PointF = SnapableCurrentArrow.getn_Normal(normal)
        If Me.beschriftung.positionIndex = 0 Then
            normal_n.X *= -1
            normal_n.Y *= -1
        End If

        Dim p0 As New PointF(p.X + normal.X * abstandPfeil + normal_n.X * längePfeilX / 2.0F, p.Y + normal.Y * abstandPfeil + normal_n.Y * längePfeilX / 2)
        Dim p1 As New PointF(p.X + normal.X * (abstandPfeil + längePfeilY) + normal_n.X * längePfeilX / 2.0F, p.Y + normal.Y * (abstandPfeil + längePfeilY) + normal_n.Y * längePfeilX / 2)
        Dim p2 As New PointF(p.X + normal.X * (abstandPfeil + längePfeilY) - normal_n.X * längePfeilX / 2.0F, p.Y + normal.Y * (abstandPfeil + längePfeilY) - normal_n.Y * längePfeilX / 2)

        g.childs.Add(New DO_Linie(New Point(CInt(p0.X), CInt(p0.Y)), New Point(CInt(p1.X), CInt(p1.Y)), False))
        g.childs.Add(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(New Point(CInt(p1.X), CInt(p1.Y)), New Point(CInt(p2.X), CInt(p2.Y)), New ParamArrow(-1, 100), pfeilspitze))
        SnapableCurrentArrow.AddBeschriftungToGrafik(g, beschriftung, pos.wireStart, pos.wireEnd, pos.alpha, beschriftung.abstand + abstandPfeil + längePfeilY, beschriftung.abstandQuer, Me.fontstyle, normal)

        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
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

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim s As New SnapableImpedanceArrow(newID, pfeilspitze.CopyPfeil(), pos.clone(), beschriftung, linestyle, fontstyle, abstandPfeil, längePfeilX, längePfeilY)
        Return s
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        pos.drehe(drehpunkt, drehung)
    End Sub

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot SnapableImpedanceArrow Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, SnapableImpedanceArrow)
            If Not .pfeilspitze.isEqual(Me.pfeilspitze) Then Return False
            If Not .pos.isEqualWithoutSelection(Me.pos) Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If .linestyle <> Me.linestyle Then Return False
            If .abstandPfeil <> Me.abstandPfeil Then Return False
            If .längePfeilX <> Me.längePfeilX Then Return False
            If .längePfeilY <> Me.längePfeilY Then Return False
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
        writer.Write(fontstyle)
        writer.Write(CInt(abstandPfeil))
        writer.Write(CInt(längePfeilX))
        writer.Write(CInt(längePfeilY))
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As SnapableImpedanceArrow
        Dim pos As WireSnappoint = WireSnappoint.Einlesen(sender, reader, version)
        Dim beschriftung As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim ls As Integer = reader.ReadInt32()
        Dim pfeil As New ParamArrow(-1, 100)
        pfeil.pfeilArt = CShort(reader.ReadInt32())
        pfeil.pfeilSize = CUShort(reader.ReadInt32())
        Dim fontstyle As Integer = reader.ReadInt32()
        Dim abstand As Integer = reader.ReadInt32()
        Dim längeX As Integer = reader.ReadInt32()
        Dim längeY As Integer = reader.ReadInt32()
        Dim res As New SnapableImpedanceArrow(sender.getNewID(), pfeil, pos, beschriftung, ls, fontstyle, abstand, längeX, längeY)
        Return res
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        Dim e1 As New Einstellung_Multi("Parameter", False)

        e1.add(New Einstellung_TemplateParameter_Int(New TemplateParameter_Int(New Multi_Lang_String("Abstand Pfeil", Nothing), New Intervall(0, Integer.MaxValue, 1, False, True, Intervall.OutOfRangeMode.ClipToBounds), DEFAULT_ABSTAND_PFEIL, ""), abstandPfeil))
        e1.add(New Einstellung_TemplateParameter_Int(New TemplateParameter_Int(New Multi_Lang_String("Länge (parallel)", Nothing), New Intervall(0, Integer.MaxValue, 1, False, True, Intervall.OutOfRangeMode.ClipToBounds), DEFAULT_ABSTAND_PFEIL, ""), längePfeilX))
        e1.add(New Einstellung_TemplateParameter_Int(New TemplateParameter_Int(New Multi_Lang_String("Länge (senkrecht)", Nothing), New Intervall(0, Integer.MaxValue, 1, False, True, Intervall.OutOfRangeMode.ClipToBounds), DEFAULT_ABSTAND_PFEIL, ""), längePfeilY))

        l.Add(e1)
        l.AddRange(MyBase.getEinstellungen(sender))
        beschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.fontstyle, sender.myFonts))
        l.Add(New Einstellung_SinglePfeilspitze(Element.EINSTELLUNG_SINGLEPFEILSPITZE, pfeilspitze))
        l.Add(New Einstellung_Linienstil(Element.EINSTELLUNG_LINESTYLE, linestyle, sender.myLineStyles))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Linienstil AndAlso e.Name.get_ID() = Element.EINSTELLUNG_LINESTYLE Then
                Me.linestyle = DirectCast(e, Einstellung_Linienstil).getNewLinienstil(Me.linestyle, sender.myLineStyles, changed, False)
            ElseIf TypeOf e Is Einstellung_SinglePfeilspitze AndAlso e.Name.get_ID() = Element.EINSTELLUNG_SINGLEPFEILSPITZE Then
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
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = Element.EINSTELLUNG_FONTSTYLE Then
                Me.fontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.fontstyle, sender.myFonts, changed, False)
            ElseIf TypeOf e Is Einstellung_Multi AndAlso e.Name.get_ID() = "Parameter" Then
                For Each e1 As Einstellung_TemplateParam In DirectCast(e, Einstellung_Multi).getListe()
                    If TypeOf e1 Is Einstellung_TemplateParameter_Int AndAlso e1.Name.get_ID() = "Abstand Pfeil" Then
                        With DirectCast(e1, Einstellung_TemplateParameter_Int)
                            If .myNr <> Me.abstandPfeil Then
                                Me.abstandPfeil = .myNr
                                changed = True
                            End If
                        End With
                    ElseIf TypeOf e1 Is Einstellung_TemplateParameter_Int AndAlso e1.Name.get_ID() = "Länge (parallel)" Then
                        With DirectCast(e1, Einstellung_TemplateParameter_Int)
                            If .myNr <> Me.längePfeilX Then
                                Me.längePfeilX = .myNr
                                changed = True
                            End If
                        End With
                    ElseIf TypeOf e1 Is Einstellung_TemplateParameter_Int AndAlso e1.Name.get_ID() = "Länge (senkrecht)" Then
                        With DirectCast(e1, Einstellung_TemplateParameter_Int)
                            If .myNr <> Me.längePfeilY Then
                                Me.längePfeilY = .myNr
                                changed = True
                            End If
                        End With
                    End If
                Next
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
End Class
