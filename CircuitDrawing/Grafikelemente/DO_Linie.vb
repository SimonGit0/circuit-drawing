Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Linie
    Inherits DO_Grafik

    Public Property p1 As Point
    Public Property p2 As Point

    Public Sub New(p1 As Point, p2 As Point, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.p1 = p1
        Me.p2 = p2
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim p1 As PointF = args.toPictureboxPoint(Me.p1)
        Dim p2 As PointF = args.toPictureboxPoint(Me.p2)

        g.DrawLine(Me.getPen(args), p1, p2)
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        Dim minX As Integer = Math.Min(p1.X, p2.X)
        Dim minY As Integer = Math.Min(p1.Y, p2.Y)
        Dim breite As Integer = Math.Abs(p1.X - p2.X)
        Dim höhe As Integer = Math.Abs(p1.Y - p2.Y)

        Return New Rectangle(minX, minY, breite, höhe)
    End Function

    Public Overrides Sub transform(t As Transform)
        Me.p1 = t.transformPoint(Me.p1)
        Me.p2 = t.transformPoint(Me.p2)
    End Sub

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_Linie(p1, p2, use_fillColor_from_linestyle)
    End Function

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim p1 As XPoint = args.toPoint(Me.p1)
        Dim p2 As XPoint = args.toPoint(Me.p2)
        gfx.DrawLine(Me.getPen(args), p1, p2)
    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        args.switchToLinestyle(writer, Me.lineStyle)
        writer.WriteLine(args.writePoint(p1) & " moveto")
        writer.WriteLine(args.writePoint(p2) & " lineto stroke")
    End Sub
End Class
