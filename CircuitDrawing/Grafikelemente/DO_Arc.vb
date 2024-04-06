Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Arc
    Inherits DO_Grafik

    Public Mitte As Point
    Public radiusX As Integer
    Public radiusY As Integer

    Public startwinkel As Single
    Public deltawinkel As Single
    Public line_around As Boolean

    Private hasFillLastDrawTime As Boolean = False
    Private fillMode As Drawing_FillMode

    Public Overrides Function fill(args As GrafikDrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikEPS_DrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyStroke AndAlso MyBase.fill(args)
    End Function

    Public Overrides Function stroke(args As GrafikDrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Overrides Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return fillMode <> Drawing_FillMode.OnlyFill AndAlso MyBase.stroke(args)
    End Function

    Public Sub New(mitte As Point, radiusX As Integer, radiusY As Integer, startwinkel As Single, deltawinkel As Single, line_around As Boolean, use_fillColor_from_linestyle As Boolean, fm As Drawing_FillMode)
        MyBase.New(use_fillColor_from_linestyle)

        Me.fillMode = fm

        Me.Mitte = mitte
        Me.radiusX = radiusX
        Me.radiusY = radiusY
        Me.startwinkel = startwinkel
        Me.deltawinkel = deltawinkel
        Me.line_around = line_around

        normalisiere()
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim m As PointF = args.toPictureboxPoint(Mitte)
        Dim rx As Single = args.toPictureboxScale(radiusX)
        Dim ry As Single = args.toPictureboxScale(radiusY)

        If Me.fill(args) Then
            g.FillPie(Me.getBrush(args), m.X - rx, m.Y - ry, 2 * rx, 2 * ry, startwinkel, deltawinkel)
            hasFillLastDrawTime = True
        Else
            hasFillLastDrawTime = False
        End If
        If Me.stroke(args) Then
            Dim p As Pen = Me.getPen(args)
            g.DrawArc(p, m.X - rx, m.Y - ry, 2 * rx, 2 * ry, startwinkel, deltawinkel)
            If line_around Then
                Dim xend, yend As Single
                Dim sinV As Double = Math.Sin((startwinkel + deltawinkel) * Math.PI / 180)
                Dim cosV As Double = Math.Cos((startwinkel + deltawinkel) * Math.PI / 180)
                Dim r As Double = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
                xend = CSng(cosV * r + m.X)
                yend = CSng(sinV * r + m.Y)

                Dim xstart, ystart As Single
                sinV = Math.Sin(startwinkel * Math.PI / 180)
                cosV = Math.Cos(startwinkel * Math.PI / 180)
                r = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
                xstart = CSng(cosV * r + m.X)
                ystart = CSng(sinV * r + m.Y)

                g.DrawLines(p, {New PointF(xstart, ystart), New PointF(m.X, m.Y), New PointF(xend, yend)})
            End If
        End If

        'Dim box As RectangleF = args.toPictureboxRect(getBoundingBox())
        'g.DrawRectangle(Pens.Red, box.X, box.Y, box.Width, box.Height)

    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim m As XPoint = args.toPoint(Mitte)
        Dim rx As Double = args.toScale(radiusX)
        Dim ry As Double = args.toScale(radiusY)

        If Me.fill(args) Then
            gfx.DrawPie(Me.getBrush(args), m.X - rx, m.Y - ry, 2 * rx, 2 * ry, startwinkel, deltawinkel)
        End If
        If Me.stroke(args) Then
            Dim p As XPen = Me.getPen(args)
            gfx.DrawArc(p, m.X - rx, m.Y - ry, 2 * rx, 2 * ry, startwinkel, deltawinkel)
            If line_around Then
                Dim xend, yend As Double
                Dim sinV As Double = Math.Sin((startwinkel + deltawinkel) * Math.PI / 180)
                Dim cosV As Double = Math.Cos((startwinkel + deltawinkel) * Math.PI / 180)
                Dim r As Double = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
                xend = cosV * r + m.X
                yend = sinV * r + m.Y

                Dim xstart, ystart As Double
                sinV = Math.Sin(startwinkel * Math.PI / 180)
                cosV = Math.Cos(startwinkel * Math.PI / 180)
                r = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
                xstart = cosV * r + m.X
                ystart = sinV * r + m.Y

                gfx.DrawLines(p, {New XPoint(xstart, ystart), New XPoint(m.X, m.Y), New XPoint(xend, yend)})
            End If
        End If
    End Sub

    Public Overrides Sub transform(t As Transform)
        Mitte = t.transformPoint(Mitte)
        normalisiere()

        Dim null As Point = t.transformPoint(New Point(0, 0))

        Dim zeigerStart As New PointF(CSng(Math.Cos(startwinkel * Math.PI / 180)), CSng(Math.Sin(startwinkel * Math.PI / 180)))
        zeigerStart = t.transformPointF(zeigerStart)
        zeigerStart.X -= null.X
        zeigerStart.Y -= null.Y

        Dim zeigerMitte As New PointF(CSng(Math.Cos((startwinkel + deltawinkel / 2) * Math.PI / 180)), CSng(Math.Sin((startwinkel + deltawinkel / 2) * Math.PI / 180)))
        zeigerMitte = t.transformPointF(zeigerMitte)
        zeigerMitte.X -= null.X
        zeigerMitte.Y -= null.Y

        Dim s As Single = CSng(180 / Math.PI * Math.Atan2(zeigerStart.Y, zeigerStart.X))
        Dim m As Single = CSng(180 / Math.PI * Math.Atan2(zeigerMitte.Y, zeigerMitte.X))

        Dim mitte_start As Single = m - s
        While mitte_start < 0
            mitte_start += 360
        End While

        Me.startwinkel = s
        If mitte_start > deltawinkel Then
            deltawinkel = -deltawinkel
        End If

        Dim obenRechts As New Point(radiusX, radiusY)
        obenRechts = t.transformPoint(obenRechts)
        radiusX = Math.Abs(obenRechts.X - null.X)
        radiusY = Math.Abs(obenRechts.Y - null.Y)

        normalisiere()
    End Sub

    Private Sub normalisiere()
        If deltawinkel < 0 Then
            startwinkel = startwinkel + deltawinkel
            deltawinkel = -deltawinkel
        End If
        startwinkel = startwinkel Mod 360
        If startwinkel < 0 Then
            startwinkel += 360
        End If
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        Dim rx As Double = radiusX
        Dim ry As Double = radiusY

        Dim ende As PointF
        Dim sinV As Double = Math.Sin((startwinkel + deltawinkel) * Math.PI / 180)
        Dim cosV As Double = Math.Cos((startwinkel + deltawinkel) * Math.PI / 180)
        Dim r As Double = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
        ende.X = CSng(cosV * r + Mitte.X)
        ende.Y = CSng(sinV * r + Mitte.Y)
        Dim start As PointF
        sinV = Math.Sin(startwinkel * Math.PI / 180)
        cosV = Math.Cos(startwinkel * Math.PI / 180)
        r = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)
        start.X = CSng(cosV * r + Mitte.X)
        start.Y = CSng(sinV * r + Mitte.Y)

        Dim minX As Single = Math.Min(ende.X, start.X)
        Dim maxX As Single = Math.Max(ende.X, start.X)
        Dim minY As Single = Math.Min(ende.Y, start.Y)
        Dim maxY As Single = Math.Max(ende.Y, start.Y)

        'zusätzliche Punkte beachten!
        'Wenn fill oder around, dann auch mittelpunkt
        If hasFillLastDrawTime OrElse line_around Then
            minX = Math.Min(minX, Mitte.X)
            minY = Math.Min(minY, Mitte.Y)
            maxX = Math.Max(maxX, Mitte.X)
            maxY = Math.Max(maxY, Mitte.Y)
        End If
        'Wenn +90°, 180°, oder 270° umschlossen wird
        For winkel As Integer = 0 To 270 Step 90
            If norm(winkel - startwinkel) < deltawinkel Then
                Dim vecNeu As PointF
                sinV = Math.Sin(winkel * Math.PI / 180)
                cosV = Math.Cos(winkel * Math.PI / 180)
                r = rx * ry / Math.Sqrt(rx * rx * sinV * sinV + ry * ry * cosV * cosV)

                vecNeu.X = CSng(Math.Cos(winkel * Math.PI / 180) * r + Mitte.X)
                vecNeu.Y = CSng(Math.Sin(winkel * Math.PI / 180) * r + Mitte.Y)
                minX = Math.Min(minX, vecNeu.X)
                minY = Math.Min(minY, vecNeu.Y)
                maxX = Math.Max(maxX, vecNeu.X)
                maxY = Math.Max(maxY, vecNeu.Y)
            End If
        Next
        'Return New Rectangle(Mitte.X - radius, Mitte.Y - radius, 2 * radius, 2 * radius)
        Return New Rectangle(CInt(minX), CInt(minY), CInt(maxX) - CInt(minX), CInt(maxY) - CInt(minY))
    End Function

    Private Function norm(w As Single) As Single
        w = w Mod 360
        If w < 0 Then w += 360
        Return w
    End Function

    Protected Overrides Function Clone_intern() As DO_Grafik
        Return New DO_Arc(Me.Mitte, Me.radiusX, Me.radiusY, Me.startwinkel, Me.deltawinkel, line_around, Me.use_fillColor_from_linestyle, fillMode)
    End Function

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        Dim _fill As Boolean = Me.fill(args)
        Dim _stroke As Boolean = Me.stroke(args)
        If _fill = False AndAlso _stroke = False Then
            Exit Sub
        End If

        Dim t As New Transform_scale(1, -1)

        Dim zeigerStart As New PointF(CSng(Math.Cos(startwinkel * Math.PI / 180)), CSng(Math.Sin(startwinkel * Math.PI / 180)))
        zeigerStart = t.transformPointF(zeigerStart)

        Dim zeigerMitte As New PointF(CSng(Math.Cos((startwinkel + deltawinkel / 2) * Math.PI / 180)), CSng(Math.Sin((startwinkel + deltawinkel / 2) * Math.PI / 180)))
        zeigerMitte = t.transformPointF(zeigerMitte)

        Dim s As Single = CSng(180 / Math.PI * Math.Atan2(zeigerStart.Y, zeigerStart.X))
        Dim m As Single = CSng(180 / Math.PI * Math.Atan2(zeigerMitte.Y, zeigerMitte.X))

        Dim mitte_start As Single = m - s
        While mitte_start < 0
            mitte_start += 360
        End While

        Dim w1 As Single = s
        Dim delta As Single = deltawinkel
        If mitte_start > deltawinkel Then
            delta = -delta
        End If

        w1 = w1 Mod 360
        If w1 < 0 Then w1 += 360
        Dim w2 As Single = (w1 + delta) Mod 360
        If w2 < 0 Then w2 = w2 + 360

        If delta < 0 Then
            Dim z As Single = w1
            w1 = w2
            w2 = z
        End If

        If line_around Then
            writer.WriteLine(args.writePoint(Mitte) & " moveto")
        End If
        writer.WriteLine(args.writePoint(Me.Mitte) & " " & args.writeSize(New Size(Me.radiusX, Me.radiusY)) & " " & args.toStringF(w1) & " " & args.toStringF(w2) & " ellipse")
        If line_around Then
            writer.WriteLine(args.writePoint(Mitte) & " lineto")
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
