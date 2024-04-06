Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Bezier
    Inherits DO_Grafik

    Public punkte() As Point
    Private fillmode As Drawing_FillMode

    Public Overrides Function fill(args As GrafikDrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikEPS_DrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function stroke(args As GrafikDrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return fillmode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Sub New(punkte() As Point, use_fillColor_from_linestyle As Boolean, fm As Drawing_FillMode)
        MyBase.New(use_fillColor_from_linestyle)
        Me.fillmode = fm
        If punkte.Length < 4 Then
            Throw New Exception("Zu wenige Punkte für eine Bezier-Kurve")
        End If
        If (punkte.Length - 4) Mod 3 <> 0 Then
            Throw New Exception("Falsche Anzahl an Punkten für eine Bezier-Kurve")
        End If
        Me.punkte = punkte
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim pF(punkte.Length - 1) As PointF
        For i As Integer = 0 To punkte.Length - 1
            pF(i) = args.toPictureboxPoint(punkte(i))
        Next
        If Me.fill(args) Then
            Dim p As New Drawing2D.GraphicsPath()
            p.StartFigure()
            p.AddBeziers(pF)
            p.CloseFigure()
            g.FillPath(Me.getBrush(args), p)
        End If
        If Me.stroke(args) Then
            g.DrawBeziers(Me.getPen(args), pF)
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim pF(punkte.Length - 1) As XPoint
        For i As Integer = 0 To punkte.Length - 1
            pF(i) = args.toPoint(punkte(i))
        Next
        If Me.fill(args) Then
            Dim p As New XGraphicsPath()
            p.StartFigure()
            p.AddBeziers(pF)
            p.CloseFigure()
            gfx.DrawPath(Me.getBrush(args), p)
        End If
        If Me.stroke(args) Then
            gfx.DrawBeziers(Me.getPen(args), pF)
        End If
    End Sub

    Public Overrides Sub transform(t As Transform)
        For i As Integer = 0 To punkte.Length - 1
            punkte(i) = t.transformPoint(punkte(i))
        Next
    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        If punkte.Count >= 4 Then
            args.switchToLinestyle(writer, Me.lineStyle)
            writer.WriteLine(args.writePoint(punkte(0)) & " moveto")
            For i As Integer = 0 To punkte.Count - 4 Step 4
                writer.WriteLine(args.writePoint(punkte(i + 1)) & " " & args.writePoint(punkte(i + 2)) & " " & args.writePoint(punkte(i + 3)) & " curveto")
            Next
            writer.WriteLine("stroke")
        End If
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        Dim box As Rectangle = Mathe.getBezierBoundingBox(punkte(0), punkte(1), punkte(2), punkte(3))
        For i As Integer = 3 To punkte.Length - 4 Step 3
            box = Mathe.Union(box, Mathe.getBezierBoundingBox(punkte(i), punkte(i + 1), punkte(i + 2), punkte(i + 3)))
        Next
        Return box
    End Function

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_Bezier(CType(punkte.Clone(), Point()), use_fillColor_from_linestyle, fillmode)
    End Function
End Class
