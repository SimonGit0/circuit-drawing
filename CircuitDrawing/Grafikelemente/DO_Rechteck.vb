Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Rechteck
    Inherits DO_Grafik

    Public Property r As Rectangle
    Private FillMode As Drawing_FillMode = Drawing_FillMode.FillAndStroke

    Public Function splitToLines() As List(Of DO_Linie)
        Dim erg As New List(Of DO_Linie)(4)
        erg.Add(New DO_Linie(New Point(r.X, r.Y), New Point(r.X, r.Y + r.Height), False))
        erg.Add(New DO_Linie(New Point(r.X, r.Y + r.Height), New Point(r.X + r.Width, r.Y + r.Height), False))
        erg.Add(New DO_Linie(New Point(r.X + r.Width, r.Y + r.Height), New Point(r.X + r.Width, r.Y), False))
        erg.Add(New DO_Linie(New Point(r.X + r.Width, r.Y), New Point(r.X, r.Y), False))
        Return erg
    End Function

    Public Overrides Function stroke(args As GrafikDrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function fill(args As GrafikDrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikEPS_DrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return FillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Sub New(r As Rectangle, use_fillColor_from_linestyle As Boolean, fillMode As Drawing_FillMode)
        MyBase.New(use_fillColor_from_linestyle)
        Me.r = r
        Me.FillMode = fillMode
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim r As RectangleF = args.toPictureboxRect(Me.r)

        If Me.fill(args) Then
            g.FillRectangle(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            g.DrawRectangle(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim r As XRect = args.toRect(Me.r)

        If Me.fill(args) Then
            gfx.DrawRectangle(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            gfx.DrawRectangle(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
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
        Return New DO_Rechteck(r, use_fillColor_from_linestyle, Me.FillMode)
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        If _fill OrElse _stroke Then
            writer.WriteLine(args.writePoint(New Point(r.X, r.Y)) & " moveto")
            writer.WriteLine(args.writePoint(New Point(r.X + r.Width, r.Y)) & " lineto")
            writer.WriteLine(args.writePoint(New Point(r.X + r.Width, r.Y + r.Height)) & " lineto")
            writer.WriteLine(args.writePoint(New Point(r.X, r.Y + r.Height)) & " lineto")
            writer.WriteLine("closepath")
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
