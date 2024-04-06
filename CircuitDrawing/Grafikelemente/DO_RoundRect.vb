Imports System.IO
Imports PdfSharp.Drawing
Public Class DO_RoundRect
    Inherits DO_Grafik

    Public Property r As Rectangle
    Public Property radius As Integer

    Public Sub New(r As Rectangle, radius As Integer, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.r = r
        Me.radius = radius
    End Sub

    Private Function getRadius() As Integer
        Return Math.Min(radius, Math.Min(r.Width, r.Height) \ 2)
    End Function

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim r As RectangleF = args.toPictureboxRect(Me.r)
        Dim rad As Single = args.toPictureboxScale(getRadius())

        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        Dim p As Drawing2D.GraphicsPath = Nothing
        If _fill OrElse _stroke Then
            p = Mathe.getRoundRect(r.X, r.Y, r.Width, r.Height, rad)
        End If
        If _fill Then
            g.FillPath(Me.getBrush(args), p)
        End If
        If _stroke Then
            g.DrawPath(Me.getPen(args), p)
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim r As XRect = args.toRect(Me.r)
        Dim rad As Double = args.toScale(getRadius())

        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)

        If _fill And _stroke Then
            gfx.DrawRoundedRectangle(Me.getPen(args), Me.getBrush(args), r, New XSize(rad, rad))
        ElseIf _fill Then
            gfx.DrawRoundedRectangle(Me.getBrush(args), r, New XSize(rad, rad))
        ElseIf _stroke Then
            gfx.DrawRoundedRectangle(Me.getPen(args), r, New XSize(rad, rad))
        End If
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        Return r
    End Function

    Public Overrides Sub transform(t As Transform)
        Dim p1 As New Point(r.X, r.Y)
        Dim p2 As New Point(r.X + r.Width, r.Y + r.Height)
        p1 = t.transformPoint(p1)
        p2 = t.transformPoint(p2)

        Dim minX As Integer = Math.Min(p1.X, p2.X)
        Dim minY As Integer = Math.Min(p1.Y, p2.Y)
        Dim maxX As Integer = Math.Max(p1.X, p2.X)
        Dim maxY As Integer = Math.Max(p1.Y, p2.Y)

        r = New Rectangle(minX, minY, maxX - minX, maxY - minY)

    End Sub

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_RoundRect(r, radius, use_fillColor_from_linestyle)
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        If _fill OrElse _stroke Then

            Dim xw As Integer = r.X + r.Width
            Dim yh As Integer = r.Y + r.Height
            Dim xwr As Integer = xw - radius
            Dim yhr As Integer = yh - radius
            Dim xr As Integer = r.X + radius
            Dim yr As Integer = r.Y + radius
            Dim r2 As Integer = radius + radius
            Dim xwr2 As Integer = xw - r2
            Dim yhr2 As Integer = yh - r2

            If r.Width = 0 AndAlso r.Height = 0 Then
                'Nichts malen!
                Exit Sub
            ElseIf r.Width = 0 Then
                writer.WriteLine(args.writePoint(New Point(r.X, r.Y)) & " moveto " & args.writePoint(New Point(r.X, r.Y + r.Height)) & " lineto")
                'wenn w=0, dann vertikale Linie!
            ElseIf r.Height = 0 Then
                writer.WriteLine(args.writePoint(New Point(r.X, r.Y)) & " moveto " & args.writePoint(New Point(r.X + r.Width, r.Y)) & " lineto")
                'wenn h=0, dann horizontale Linie!
            ElseIf radius = 0 Then 'wenn radius = 0, dann rechteck malen!
                writer.WriteLine(args.writePoint(New Point(r.X, r.Y)) & " moveto")
                writer.WriteLine(args.writePoint(New Point(r.X + r.Width, r.Y)) & " lineto")
                writer.WriteLine(args.writePoint(New Point(r.X + r.Width, r.Y + r.Height)) & " lineto")
                writer.WriteLine(args.writePoint(New Point(r.X, r.Y + r.Height)) & " lineto")
                writer.WriteLine("closepath")
            Else
                'Top Left Corner
                writer.WriteLine(args.writePoint(New Point(xr, yr)) & " " & args.writeScale(radius) & " 180 90 arcn")

                'Top Right Corner
                writer.WriteLine(args.writePoint(New Point(xwr, yr)) & " " & args.writeScale(radius) & " 90 0 arcn")

                'Bottom Right Corner
                writer.WriteLine(args.writePoint(New Point(xwr, yhr)) & " " & args.writeScale(radius) & " 0 270 arcn")

                'Bottom Left Corner
                writer.WriteLine(args.writePoint(New Point(xr, yhr)) & " " & args.writeScale(radius) & " 270 180 arcn")

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
        End If
    End Sub
End Class
