Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_Text
    Inherits DO_Grafik

    Private Const OFFSET_LINKS As Integer = 50
    Private Const OFFSET_RECHTS As Integer = 50
    Private Const OFFSET_OBEN_GDI As Integer = 20
    Private Const OFFSET_UNTEN_GDI As Integer = 20
    Private Const OFFSET_OBEN_TEX As Integer = 20
    Private Const OFFSET_UNTEN_TEX As Integer = 20

    Private text As String
    Private pos As Point
    Private ah As AlignH
    Private av As AlignV
    Public fontIndex As Integer

    Private boundingBox_lastDrawing As Rectangle

    Private myRot As TextRotation

    Public Sub New(text As String, fontIndex As Integer, pos As Point, vector As Point, rot As TextRotation, use_fillColor_from_linestyle As Boolean)
        Me.New(text, fontIndex, pos, get_ah_from_Vector(vector.X), get_av_from_Vector(vector.Y), rot, use_fillColor_from_linestyle)
    End Sub

    Public Sub New(text As String, fontIndex As Integer, pos As Point, ah As AlignH, av As AlignV, rot As TextRotation, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.text = text
        Me.fontIndex = fontIndex
        Me.pos = pos
        Me.ah = ah
        Me.av = av
        Me.myRot = rot
    End Sub

    Private Shared Function get_ah_from_Vector(x As Integer) As AlignH
        If x < 0 Then
            Return AlignH.Rechts
        ElseIf x > 0 Then
            Return AlignH.Links
        Else
            Return AlignH.Mitte
        End If
    End Function

    Private Shared Function get_av_from_Vector(y As Integer) As AlignV
        If y > 0 Then
            Return AlignV.Oben
        ElseIf y < 0 Then
            Return AlignV.Unten
        Else
            Return AlignV.Mitte
        End If
    End Function

    Private Shared Function getVector(ah As AlignH, av As AlignV) As Point
        Dim v As New Point
        If ah = AlignH.Links Then
            v.X = 1
        ElseIf ah = AlignH.Rechts Then
            v.X = -1
        Else
            v.X = 0
        End If
        If av = AlignV.Oben Then
            v.Y = 1
        ElseIf av = AlignV.Unten Then
            v.Y = -1
        Else
            v.Y = 0
        End If
        Return v
    End Function

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim ax As Integer = 0
        Dim ay As Integer = 0
        Dim deltaX As Integer = 0
        Dim deltaY As Integer = 0
        Dim myNormalFont As Font = args.getFont(fontIndex)
        Dim myTiefHochFont As Font = args.getTiefHochFont(fontIndex)

        Dim ah_Rot As AlignH = Me.ah
        Dim av_Rot As AlignV = Me.av
        If myRot <> TextRotation.Normal Then
            rechneAH_AV_FuerDrehungUm(ah_Rot, av_Rot)
        End If
        '-----------------------------------------------
        'Korrektur für etwas größere Abstände!
        '-----------------------------------------------
        Dim deltaV As New Point(0, 0)
        If ah_Rot = AlignH.Links Then
            deltaV.X = OFFSET_LINKS
        ElseIf ah_Rot = AlignH.Rechts Then
            deltaV.X = -OFFSET_RECHTS
        End If
        If av_Rot = AlignV.Oben Then
            deltaV.Y = OFFSET_OBEN_GDI
        ElseIf av_Rot = AlignV.Unten Then
            deltaV.Y = -OFFSET_UNTEN_GDI
        End If
        If myRot = TextRotation.Normal Then
            deltaX = deltaV.X
            deltaY = deltaV.Y
        ElseIf myRot = TextRotation.Rot180 Then
            deltaX = -deltaV.X
            deltaY = -deltaV.Y
        ElseIf myRot = TextRotation.Rot90 Then
            deltaY = -deltaV.X
            deltaX = deltaV.Y
        ElseIf myRot = TextRotation.Rot270 Then
            deltaY = deltaV.X
            deltaX = -deltaV.Y
        Else
            Throw New Exception("Unbekannte Text-Rotation")
        End If

        Dim posDraw As PointF = args.toPictureboxPoint(New Point(pos.X + deltaX, pos.Y + deltaY))
        Dim _out_BoundingBox As RectangleF

        '-----------------------------------------------
        'Drehung berücksichtigen
        '-----------------------------------------------
        Dim winkel As Integer = 0

        If myRot <> TextRotation.Normal Then
            g.TranslateTransform(posDraw.X, posDraw.Y)
            Select Case myRot
                Case TextRotation.Rot90
                    g.RotateTransform(270)
                    winkel = 270
                Case TextRotation.Rot180
                    g.RotateTransform(180)
                    winkel = 180
                Case TextRotation.Rot270
                    g.RotateTransform(90)
                    winkel = 90
            End Select
            g.TranslateTransform(-posDraw.X, -posDraw.Y)
        End If

        '-----------------------------------------------
        'Align-Enums in verschiebungswerte umrechnen!
        '-----------------------------------------------
        If ah_Rot = AlignH.Links Then
            ax = -1
        ElseIf ah_Rot = AlignH.Mitte Then
            ax = 0
        ElseIf ah_Rot = AlignH.Rechts Then
            ax = 1
        End If
        If av_Rot = AlignV.Oben Then
            ay = -1
        ElseIf av_Rot = AlignV.Mitte Then
            ay = 0
        ElseIf av_Rot = AlignV.Unten Then
            ay = 1
        End If

        '-----------------------------------------------
        'Malen!
        '-----------------------------------------------
        Dim multiLineHAlign As Integer = ax
        Try
            AdvancedTextRenderer.drawString(g, Me.text, args.TextVorschauMode, posDraw, ax, ay, myNormalFont, myTiefHochFont, args.getFontBrush(fontIndex), _out_BoundingBox, multiLineHAlign)
            If Single.IsNaN(_out_BoundingBox.X) OrElse Single.IsNaN(_out_BoundingBox.Y) OrElse Single.IsNaN(_out_BoundingBox.Width) OrElse Single.IsNaN(_out_BoundingBox.Height) Then
                Throw New Exception("Fehler T1001")
            End If
            If _out_BoundingBox.Width < 0 OrElse _out_BoundingBox.Height < 0 Then
                Throw New Exception("Fehler T1000")
            End If
        Catch ex As Exception
            AdvancedTextRenderer.drawString(g, "ERROR", False, posDraw, ax, ay, myNormalFont, myTiefHochFont, args.getFontBrush(fontIndex), _out_BoundingBox, multiLineHAlign)
        End Try

        If myRot <> TextRotation.Normal Then
            g.ResetTransform()
        End If

        '-----------------------------------------------
        'Drehen der Bounding Box, damit es auch passt!
        '-----------------------------------------------
        If winkel <> 0 Then
            Dim p1 As PointF = New PointF(_out_BoundingBox.X, _out_BoundingBox.Y)
            Dim p2 As PointF = New PointF(_out_BoundingBox.X + _out_BoundingBox.Width, _out_BoundingBox.Y + _out_BoundingBox.Height)
            p1 = Mathe.pointFDrehen(p1, posDraw, winkel)
            p2 = Mathe.pointFDrehen(p2, posDraw, winkel)
            Dim minX As Single = Math.Min(p1.X, p2.X)
            Dim minY As Single = Math.Min(p1.Y, p2.Y)
            Dim maxX As Single = Math.Max(p1.X, p2.X)
            Dim maxY As Single = Math.Max(p1.Y, p2.Y)
            _out_BoundingBox = New RectangleF(minX, minY, maxX - minX, maxY - minY)
        End If
        'g.DrawRectangle(Pens.Red, _out_BoundingBox.X, _out_BoundingBox.Y, _out_BoundingBox.Width, _out_BoundingBox.Height)

        Me.boundingBox_lastDrawing = args.toIntRect(_out_BoundingBox)
        '-----------------------------------------------
        'Alter Algorithmus
        '-----------------------------------------------

        'Else
        '    Dim format As New StringFormat()

        '    If ah = AlignH.Links Then
        '        format.Alignment = StringAlignment.Near
        '    ElseIf ah = AlignH.Mitte Then
        '        format.Alignment = StringAlignment.Center
        '    Else
        '        format.Alignment = StringAlignment.Far
        '    End If
        '    If av = AlignV.Unten Then
        '        format.LineAlignment = StringAlignment.Far
        '    ElseIf av = AlignV.Mitte Then
        '        format.LineAlignment = StringAlignment.Center
        '    Else
        '        format.LineAlignment = StringAlignment.Near
        '    End If

        '    g.DrawString(text, args.getFont(fontIndex), args.getFontBrush(fontIndex), args.toPictureboxPoint(pos), format)
        'End If
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        If Not args.OhneText AndAlso text <> "" Then
            gfx.Save()

            Dim ax As Integer = 0
            Dim ay As Integer = 0
            Dim deltaX As Integer = 0
            Dim deltaY As Integer = 0
            Dim myNormalFont As XFont = args.getFont(fontIndex)
            Dim myTiefHochFont As XFont = args.getTiefHochFont(fontIndex)

            Dim ah_Rot As AlignH = Me.ah
            Dim av_Rot As AlignV = Me.av
            If myRot <> TextRotation.Normal Then
                rechneAH_AV_FuerDrehungUm(ah_Rot, av_Rot)
            End If
            '-----------------------------------------------
            'Korrektur für etwas größere Abstände!
            '-----------------------------------------------
            Dim deltaV As New Point(0, 0)
            If ah_Rot = AlignH.Links Then
                deltaV.X = OFFSET_LINKS
            ElseIf ah_Rot = AlignH.Rechts Then
                deltaV.X = -OFFSET_RECHTS
            End If
            If av_Rot = AlignV.Oben Then
                deltaV.Y = OFFSET_OBEN_GDI
            ElseIf av_Rot = AlignV.Unten Then
                deltaV.Y = -OFFSET_UNTEN_GDI
            End If
            If myRot = TextRotation.Normal Then
                deltaX = deltaV.X
                deltaY = deltaV.Y
            ElseIf myRot = TextRotation.Rot180 Then
                deltaX = -deltaV.X
                deltaY = -deltaV.Y
            ElseIf myRot = TextRotation.Rot90 Then
                deltaY = -deltaV.X
                deltaX = deltaV.Y
            ElseIf myRot = TextRotation.Rot270 Then
                deltaY = deltaV.X
                deltaX = -deltaV.Y
            Else
                Throw New Exception("Unbekannte Text-Rotation")
            End If

            Dim posDraw As XPoint = args.toPoint(New Point(pos.X + deltaX, pos.Y + deltaY))
            Dim _out_BoundingBox As RectangleF

            '-----------------------------------------------
            'Drehung berücksichtigen
            '-----------------------------------------------
            Dim winkel As Integer = 0

            If myRot <> TextRotation.Normal Then
                gfx.TranslateTransform(posDraw.X, posDraw.Y)
                Select Case myRot
                    Case TextRotation.Rot90
                        gfx.RotateTransform(270)
                        winkel = 270
                    Case TextRotation.Rot180
                        gfx.RotateTransform(180)
                        winkel = 180
                    Case TextRotation.Rot270
                        gfx.RotateTransform(90)
                        winkel = 90
                End Select
                gfx.TranslateTransform(-posDraw.X, -posDraw.Y)
            End If

            '-----------------------------------------------
            'Align-Enums in verschiebungswerte umrechnen!
            '-----------------------------------------------
            If ah_Rot = AlignH.Links Then
                ax = -1
            ElseIf ah_Rot = AlignH.Mitte Then
                ax = 0
            ElseIf ah_Rot = AlignH.Rechts Then
                ax = 1
            End If
            If av_Rot = AlignV.Oben Then
                ay = -1
            ElseIf av_Rot = AlignV.Mitte Then
                ay = 0
            ElseIf av_Rot = AlignV.Unten Then
                ay = 1
            End If

            '-----------------------------------------------
            'Malen!
            '-----------------------------------------------
            Dim multiLineHAlign As Integer = ax
            Try
                AdvancedTextRenderer.drawString(gfx, Me.text, posDraw, ax, ay, myNormalFont, myTiefHochFont, args.getFontBrush(fontIndex), _out_BoundingBox, multiLineHAlign)
                If Single.IsNaN(_out_BoundingBox.X) OrElse Single.IsNaN(_out_BoundingBox.Y) OrElse Single.IsNaN(_out_BoundingBox.Width) OrElse Single.IsNaN(_out_BoundingBox.Height) Then
                    Throw New Exception("Fehler T1001")
                End If
                If _out_BoundingBox.Width < 0 OrElse _out_BoundingBox.Height < 0 Then
                    Throw New Exception("Fehler T1000")
                End If
            Catch ex As Exception
                AdvancedTextRenderer.drawString(gfx, "ERROR", posDraw, ax, ay, myNormalFont, myTiefHochFont, args.getFontBrush(fontIndex), _out_BoundingBox, multiLineHAlign)
            End Try

            gfx.Restore()
        End If
    End Sub

    Public Overrides Sub drawTEX_Text(writer As StreamWriter, args As GrafikTEX_DrawArgs)
        If text <> "" Then
            Dim pos As Point = args.toTEXpoint(Me.pos)
            Dim ah_rot As AlignH = ah
            Dim av_rot As AlignV = av
            Me.rechneAH_AV_FuerDrehungUm(ah_rot, av_rot)

            Dim deltaX As Integer
            Dim deltaY As Integer
            Dim deltaV As New Point(0, 0)
            If ah_rot = AlignH.Links Then
                deltaV.X = OFFSET_LINKS
            ElseIf ah_rot = AlignH.Rechts Then
                deltaV.X = -OFFSET_RECHTS
            End If
            If av_rot = AlignV.Oben Then
                deltaV.Y = -OFFSET_OBEN_TEX
            ElseIf av_rot = AlignV.Unten Then
                deltaV.Y = OFFSET_UNTEN_TEX
            End If
            If myRot = TextRotation.Normal Then
                deltaX = deltaV.X
                deltaY = deltaV.Y
            ElseIf myRot = TextRotation.Rot180 Then
                deltaX = -deltaV.X
                deltaY = -deltaV.Y
            ElseIf myRot = TextRotation.Rot90 Then
                deltaY = deltaV.X 'For Tex is y-axis flipped (positive to top!) So here the sign is flipped compared to gdi+ drawing!
                deltaX = -deltaV.Y 'same here...
            ElseIf myRot = TextRotation.Rot270 Then
                deltaY = -deltaV.X 'same here...
                deltaX = deltaV.Y 'same here...
            Else
                Throw New Exception("Unbekannte Text-Rotation")
            End If
            pos.X += deltaX
            pos.Y += deltaY

            Dim winkel As Integer = 0
            If myRot = TextRotation.Normal Then
                winkel = 0
            ElseIf myRot = TextRotation.Rot90 Then
                winkel = 90
            ElseIf myRot = TextRotation.Rot180 Then
                winkel = 180
            ElseIf myRot = TextRotation.Rot270 Then
                winkel = 270
            End If

            Dim line As New CustomStringBuilder()
            line.Append("\put(")
            line.AppendPlain(pos.X)
            line.Append(",")
            line.AppendPlain(pos.Y)
            line.Append("){")
            If winkel <> 0 Then
                line.Append("\rotatebox[origin=c]{")
                line.AppendPlain(winkel)
                line.Append("}{")
            End If
            line.Append("\makebox(0,0)")
            If av_rot = AlignV.Oben Then
                If ah_rot = AlignH.Links Then
                    line.Append("[tl]")
                ElseIf ah_rot = AlignH.Mitte Then
                    line.Append("[t]")
                Else
                    line.Append("[tr]")
                End If
            ElseIf av_rot = AlignV.Mitte Then
                If ah_rot = AlignH.Links Then
                    line.Append("[l]")
                ElseIf ah_rot = AlignH.Rechts Then
                    line.Append("[r]")
                End If
            Else
                If ah_rot = AlignH.Links Then
                    line.Append("[bl]")
                ElseIf ah_rot = AlignH.Mitte Then
                    line.Append("[b]")
                Else
                    line.Append("[br]")
                End If
            End If
            line.Append("{")

            If args.getFont(Me.fontIndex).farbe.Color_B <> 0 OrElse
               args.getFont(Me.fontIndex).farbe.Color_G <> 0 OrElse
               args.getFont(Me.fontIndex).farbe.Color_R <> 0 Then
                line.Append("\color[RGB]{")
                line.AppendPlain(args.getFont(Me.fontIndex).farbe.Color_R)
                line.Append(",")
                line.AppendPlain(args.getFont(Me.fontIndex).farbe.Color_G)
                line.Append(",")
                line.AppendPlain(args.getFont(Me.fontIndex).farbe.Color_B)
                line.Append("}")
            End If

            addTEXText(text, line, ah_rot)
            If winkel <> 0 Then
                line.Append("}")
            End If
            line.Append("}}%")
            writer.WriteLine(line)
        End If
    End Sub

    Private Sub addTEXText(text As String, line As CustomStringBuilder, ah As AlignH)
        Dim klammerpos As Integer = 0
        Dim currentLine As String = ""
        Dim lines As New List(Of String)
        For i As Integer = 0 To text.Length - 1
            If text(i) = "{" Then
                klammerpos += 1
            End If
            If text(i) = "}" Then
                klammerpos -= 1
            End If
            If klammerpos = 0 Then
                If i < text.Length - 1 AndAlso text.Substring(i, 2) = "\\" Then
                    lines.Add(currentLine)
                    currentLine = ""
                    i = i + 1
                    Continue For
                End If
            End If
            currentLine &= text(i)
        Next
        lines.Add(currentLine)

        If lines.Count = 0 Then
            Throw New Exception("Keine Zeilen vorhanden?")
        ElseIf lines.Count = 1 Then
            line.Append("\strut{}")
            line.Append(text)
        Else
            line.Append("\vbox{\setlength\hsize{500cm}") '500cm is near maxdimen! It is assumed that the textwidth is (much) smaller than this!
            If ah = AlignH.Links Then
                line.Append("\raggedright")
            ElseIf ah = AlignH.Mitte Then
                line.Append("\centering")
            Else
                line.Append("\raggedleft")
            End If
            For i As Integer = 0 To lines.Count - 1
                If i >= 1 AndAlso lines(i).StartsWith("[") Then
                    'Konstruktion a la \\[1mm]
                    Dim klammerEnde As Integer = -1
                    For j As Integer = 0 To lines(i).Length - 1
                        If lines(i)(j) = "]" Then
                            klammerEnde = j
                            Exit For
                        End If
                    Next
                    If klammerEnde = -1 Then
                        line.Append("\strut{}")
                        line.Append(lines(i))
                    Else
                        line.Append(lines(i).Substring(0, klammerEnde + 1))
                        line.Append("\strut{}")
                        line.Append(lines(i).Substring(klammerEnde + 1))
                    End If
                Else
                    line.Append("\strut{}")
                    line.Append(lines(i))
                End If
                If i <> lines.Count - 1 Then line.Append("\\")
            Next
            line.Append("}")
        End If
    End Sub

    Public Overrides Sub transform(t As Transform)
        Me.pos = t.transformPoint(pos)

        Dim v As Point = getVector(ah, av)
        v = t.transformPoint(v)
        Dim null As Point = t.transformPoint(New Point(0, 0))
        v.X -= null.X
        v.Y -= null.Y
        ah = get_ah_from_Vector(v.X)
        av = get_av_from_Vector(v.Y)
    End Sub

    Public Overrides Function getBoundingBox() As Rectangle
        'Dim ax As Integer = 0
        'Dim ay As Integer = 0
        'If ah = AlignH.Links Then
        '    ax = -1
        'ElseIf ah = AlignH.Mitte Then
        '    ax = 0
        'ElseIf ah = AlignH.Rechts Then
        '    ax = 1
        'End If
        'If av = AlignV.Oben Then
        '    ay = -1
        'ElseIf av = AlignV.Mitte Then
        '    ay = 0
        'ElseIf av = AlignV.Unten Then
        '    ay = 1
        'End If

        'Dim myNormalFont As Font = args.getFont(fontIndex)
        'Dim myTiefHochFont As Font = args.getTiefHochFont(fontIndex)

        'Dim deltaX As Integer
        'If ax = -1 Then
        '    deltaX = 20
        'End If

        'Dim r As RectangleF = AdvancedTextRenderer.measureString(g, Me.text, args.toPictureboxPoint(New Point(pos.X + deltaX, pos.Y)), ax, ay, myNormalFont, myTiefHochFont)

        'Return args.toIntRect(r)
        Return boundingBox_lastDrawing
    End Function

    Protected Overrides Function Clone_intern() As DO_Grafik
        Dim erg As New DO_Text(Me.text, Me.fontIndex, Me.pos, Me.ah, Me.av, Me.myRot, Me.use_fillColor_from_linestyle)
        erg.boundingBox_lastDrawing = Me.boundingBox_lastDrawing
        Return erg
    End Function

    Public Overrides Sub markAllUsedFontStyles(usedFontstyles() As Boolean)
        usedFontstyles(fontIndex) = True
    End Sub

    Private Sub rechneAH_AV_FuerDrehungUm(ByRef _out_ah As AlignH, ByRef _out_av As AlignV)
        'Für die Drehung anpassen

        Dim vector As Point
        If ah = AlignH.Links Then
            vector.X = 1
        ElseIf ah = AlignH.Rechts Then
            vector.X = -1
        Else
            vector.X = 0
        End If
        If av = AlignV.Oben Then
            vector.Y = 1
        ElseIf av = AlignV.Unten Then
            vector.Y = -1
        Else
            vector.Y = 0
        End If
        If myRot = TextRotation.Rot90 Then
            vector = Drehmatrix.Drehen90Grad.transformPoint(vector)
        ElseIf myRot = TextRotation.Rot180 Then
            vector = Drehmatrix.Drehen90Grad.transformPoint(vector)
            vector = Drehmatrix.Drehen90Grad.transformPoint(vector)
        ElseIf myRot = TextRotation.Rot270 Then
            vector = Drehmatrix.DrehenMinus90Grad.transformPoint(vector)
        End If
        _out_ah = get_ah_from_Vector(vector.X)
        _out_av = get_av_from_Vector(vector.Y)
    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        'mache nichts
    End Sub

    Public Enum AlignH
        Links
        Mitte
        Rechts
    End Enum
    Public Enum AlignV
        Oben
        Mitte
        Unten
    End Enum
    Public Enum TextRotation
        Normal
        Rot90
        Rot180
        Rot270
    End Enum
End Class
