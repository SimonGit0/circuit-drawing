Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Dot
    Inherits DO_Grafik

    Public p As Point
    Private rx As Integer
    Private ry As Integer

    Public Sub New(p As Point, rx As Integer, ry As Integer, linestyle_Color As Integer, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.p = p
        Me.rx = rx
        Me.ry = ry
        Me.lineStyle.linestyle = linestyle_Color
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim obenLinks As PointF = args.toPictureboxPoint(New Point(p.X - rx, p.Y - ry))
        Dim untenRechts As PointF = args.toPictureboxPoint(New Point(p.X + rx, p.Y + ry))

        g.FillEllipse(args.LineStyles.getLineStyle(lineStyle.linestyle).getSolidBrush(), obenLinks.X, obenLinks.Y, untenRechts.X - obenLinks.X, untenRechts.Y - obenLinks.Y)
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim obenLinks As XPoint = args.toPoint(New Point(p.X - rx, p.Y - ry))
        Dim untenRechts As XPoint = args.toPoint(New Point(p.X + rx, p.Y + ry))

        gfx.DrawEllipse(args.LineStyles.getLineStyle(lineStyle.linestyle).getSolidBrushX(), obenLinks.X, obenLinks.Y, untenRechts.X - obenLinks.X, untenRechts.Y - obenLinks.Y)
    End Sub

    Public Overrides Sub transform(t As Transform)
        Dim v As New Point(p.X + rx, p.Y + ry)
        v = t.transformPoint(v)
        p = t.transformPoint(p)
        v.X -= p.X
        v.Y -= p.Y

        rx = Math.Abs(v.X)
        ry = Math.Abs(v.Y)

    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim old As Boolean = Me.use_fillColor_from_linestyle
        Me.use_fillColor_from_linestyle = True
        Me.switchToFillstyle(writer, args)
        Me.use_fillColor_from_linestyle = old
        writer.WriteLine(args.writePoint(p) & " " & args.writeSize(New Size(Me.rx, Me.ry)) & " 0 360 ellipse")
        writer.WriteLine("fill")
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        Return New Rectangle(p.X - rx, p.Y - ry, 2 * rx, 2 * ry)
    End Function

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_Dot(p, rx, ry, lineStyle.linestyle, use_fillColor_from_linestyle)
    End Function
End Class
