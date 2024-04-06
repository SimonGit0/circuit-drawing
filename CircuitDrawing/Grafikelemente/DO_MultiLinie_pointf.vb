Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_MultiLinie_pointf
    Inherits DO_Grafik

    Private punkte() As PointF

    Public Sub New(p() As PointF, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.punkte = p
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        If punkte.Length > 1 Then
            Dim pf(punkte.Length - 1) As PointF
            For i As Integer = 0 To punkte.Length - 1
                pf(i) = args.toPictureboxPoint(punkte(i))
            Next

            g.DrawLines(Me.getPen(args), pf)
        End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        If punkte.Length > 1 Then
            Dim pf(punkte.Length - 1) As XPoint
            For i As Integer = 0 To punkte.Length - 1
                pf(i) = args.toPoint(punkte(i))
            Next

            gfx.DrawLines(Me.getPen(args), pf)
        End If
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        If punkte.Length <= 0 Then Return New Rectangle(0, 0, 0, 0)
        Dim minX As Single = Integer.MaxValue
        Dim minY As Single = Integer.MaxValue
        Dim maxX As Single = Integer.MinValue
        Dim maxY As Single = Integer.MinValue

        For Each p As PointF In punkte
            minX = Math.Min(minX, p.X)
            minY = Math.Min(minY, p.Y)
            maxX = Math.Max(maxX, p.X)
            maxY = Math.Max(maxY, p.Y)
        Next

        Dim minX_i As Integer = CInt(Math.Floor(minX))
        Dim minY_i As Integer = CInt(Math.Floor(minY))
        Dim maxX_i As Integer = CInt(Math.Ceiling(maxX))
        Dim maxY_i As Integer = CInt(Math.Ceiling(maxY))

        Return New Rectangle(minX_i, minY_i, maxX_i - minX_i, maxY_i - minY_i)
    End Function

    Public Overrides Sub transform(t As Transform)
        For i As Integer = 0 To punkte.Length - 1
            punkte(i) = t.transformPointF(punkte(i))
        Next
    End Sub

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_MultiLinie_pointf(CType(punkte.Clone(), PointF()), use_fillColor_from_linestyle)
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        If punkte.Count >= 2 Then
            args.switchToLinestyle(writer, Me.lineStyle)
            writer.WriteLine(args.writePoint(punkte(0)) & " moveto")
            For i As Integer = 1 To punkte.Count - 1
                writer.WriteLine(args.writePoint(punkte(i)) & " lineto")
            Next
            writer.WriteLine("stroke")
        End If
    End Sub
End Class
