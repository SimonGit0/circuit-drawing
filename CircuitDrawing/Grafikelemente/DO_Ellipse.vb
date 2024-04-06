Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Ellipse
    Inherits DO_Grafik

    Public Property r As Rectangle
    Private myFillMode As Drawing_FillMode

    Public Overrides Function stroke(args As GrafikDrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function fill(args As GrafikDrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikEPS_DrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return myFillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Sub New(r As Rectangle, use_fillColor_from_linestyle As Boolean, fillMode As Drawing_FillMode)
        MyBase.New(use_fillColor_from_linestyle)
        Me.r = r
        Me.myFillMode = fillMode
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim r As RectangleF = args.toPictureboxRect(Me.r)

        If Me.fill(args) Then
            g.FillEllipse(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            g.DrawEllipse(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim r As XRect = args.toRect(Me.r)

        If Me.fill(args) Then
            gfx.DrawEllipse(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            gfx.DrawEllipse(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
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
        Return New DO_Ellipse(r, use_fillColor_from_linestyle, myFillMode)
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        If _fill = False AndAlso _stroke = False Then
            Exit Sub
        End If

        writer.WriteLine(args.writePoint(New PointF(r.X + r.Width / 2.0F, r.Y + r.Height / 2.0F)) & " " & args.writeSize(New SizeF(r.Width / 2.0F, r.Height / 2.0F)) & " 0 360 ellipse")
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
