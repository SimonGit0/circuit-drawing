Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Polygon
    Inherits DO_Grafik

    Private punkte() As Point
    Private _ActivateFill As Boolean
    Private _ActivateStroke As Boolean

    Public matrix() As Single 'zeile1: 0 1; zeile 2: 2 3
    Private offset As Point
    Public closed As Boolean
    Private scale_as_arrow As Boolean
    Private noDash As Boolean

    Public Sub New(p() As Point, fill As Boolean, stroke As Boolean, use_fillColor_from_linestyle As Boolean, scale_as_arrow As Boolean, noDash As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.scale_as_arrow = scale_as_arrow
        Me.punkte = p
        Me._ActivateFill = fill
        Me._ActivateStroke = stroke
        ReDim Me.matrix(3)
        matrix(0) = 1
        matrix(1) = 0
        matrix(2) = 0
        matrix(3) = 1
        Me.offset = New Point(0, 0)
        closed = True
        Me.noDash = noDash
    End Sub

    Public Function splitToLines() As List(Of DO_Linie)
        If scale_as_arrow Then Return New List(Of DO_Linie)(0)
        Dim erg As New List(Of DO_Linie)(punkte.Count)
        For i As Integer = 0 To punkte.Count - 2
            erg.Add(New DO_Linie(New Point(punkte(i).X + offset.X, punkte(i).Y + offset.Y), New Point(punkte(i + 1).X + offset.X, punkte(i + 1).Y + offset.Y), False))
        Next
        If closed Then
            erg.Add(New DO_Linie(New Point(punkte(punkte.Count - 1).X + offset.X, punkte(punkte.Count - 1).Y + offset.Y), New Point(punkte(0).X + offset.X, punkte(0).Y + offset.Y), False))
        End If
        Return erg
    End Function

    Public Sub SetMatrix(v As Point)
        Dim alpha As Double = Math.Atan2(v.Y, v.X)
        matrix(0) = CSng(Math.Cos(alpha))
        matrix(1) = CSng(-Math.Sin(alpha))
        matrix(2) = CSng(Math.Sin(alpha))
        matrix(3) = CSng(Math.Cos(alpha))
    End Sub

    Private Function drehen(p As Point) As PointF
        Dim faktor As Single = 1.0F
        Return New PointF(p.X * faktor * matrix(0) + p.Y * faktor * matrix(1) + offset.X, p.X * faktor * matrix(2) + p.Y * faktor * matrix(3) + offset.Y)
    End Function

    Public Overrides Function fill(args As GrafikDrawArgs) As Boolean
        Return _ActivateFill AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return _ActivateFill AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikEPS_DrawArgs) As Boolean
        Return _ActivateFill AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function stroke(args As GrafikDrawArgs) As Boolean
        Return _ActivateStroke AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return _ActivateStroke AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return _ActivateStroke AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim pf(punkte.Length - 1) As PointF
        For i As Integer = 0 To punkte.Length - 1
            pf(i) = args.toPictureboxPoint(drehen(punkte(i)))
        Next
        If Me.fill(args) Then
            g.FillPolygon(Me.getBrush(args), pf, Drawing2D.FillMode.Alternate)
        End If
        If Me.stroke(args) Then
            If closed Then
                g.DrawPolygon(Me.getPen(args, noDash), pf)
            Else
                g.DrawLines(Me.getPen(args, noDash), pf)
            End If
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim pf(punkte.Length - 1) As XPoint
        For i As Integer = 0 To punkte.Length - 1
            pf(i) = args.toPoint(drehen(punkte(i)))
        Next
        If Me.fill(args) Then
            gfx.DrawPolygon(Me.getBrush(args), pf, XFillMode.Alternate)
        End If
        If Me.stroke(args) Then
            If closed Then
                gfx.DrawPolygon(Me.getPen(args, noDash), pf)
            Else
                gfx.DrawLines(Me.getPen(args, noDash), pf)
            End If
        End If
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        If punkte.Count = 0 Then
            Return New Rectangle(0, 0, 0, 0)
        End If

        Dim minX As Integer = Integer.MaxValue
        Dim minY As Integer = Integer.MaxValue
        Dim maxX As Integer = Integer.MinValue
        Dim maxY As Integer = Integer.MinValue
        Dim p As PointF
        For i As Integer = 0 To punkte.Count - 1
            p = drehen(punkte(i))
            minX = Math.Min(minX, CInt(p.X))
            minY = Math.Min(minY, CInt(p.Y))
            maxX = Math.Max(maxX, CInt(p.X))
            maxY = Math.Max(maxY, CInt(p.Y))
        Next

        Return New Rectangle(minX, minY, maxX - minX, maxY - minY)
    End Function

    Public Overrides Sub transform(t As Transform)
        t.transformMatrix(matrix)
        offset = t.transformPoint(offset)
    End Sub

    Protected Overrides Function Clone_intern() As DO_Grafik
        Dim p As New DO_Polygon(CType(Me.punkte.Clone(), Point()), Me._ActivateFill, Me._ActivateStroke, use_fillColor_from_linestyle, scale_as_arrow, noDash)
        p.matrix(0) = Me.matrix(0)
        p.matrix(1) = Me.matrix(1)
        p.matrix(2) = Me.matrix(2)
        p.matrix(3) = Me.matrix(3)
        p.offset = offset
        p.closed = closed
        Return p
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim pf(punkte.Length - 1) As PointF
        For i As Integer = 0 To punkte.Length - 1
            pf(i) = args.toPoint(drehen(punkte(i)))
        Next

        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        If _stroke = False AndAlso _fill = False Then
            Return
        End If

        writer.WriteLine(args.writePoint_converted(pf(0)) & " moveto")
        For i As Integer = 1 To punkte.Count - 1
            writer.WriteLine(args.writePoint_converted(pf(i)) & " lineto")
        Next

        If closed Then
            writer.WriteLine("closepath")
        End If
        If _fill AndAlso _stroke Then
            writer.WriteLine("gsave")
            switchToFillstyle(writer, args)
            writer.WriteLine("fill")
            writer.WriteLine("grestore")
            args.switchToLinestyle(writer, lineStyle)
            writer.WriteLine("stroke")
        ElseIf _stroke Then
            args.switchToLinestyle(writer, lineStyle)
            writer.WriteLine("stroke")
        ElseIf _fill Then
            switchToFillstyle(writer, args)
            writer.WriteLine("fill")
        End If
    End Sub
End Class
