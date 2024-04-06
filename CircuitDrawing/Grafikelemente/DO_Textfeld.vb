Imports System.IO
Imports System.Text
Imports PdfSharp.Drawing

Public Class DO_Textfeld
    Inherits DO_Grafik

    Public Property r As Rectangle
    Public Property Text As String
    Public Property font As Integer
    Private av As DO_Text.AlignV
    Private ah As AlignH

    Public Sub New(r As Rectangle, t As String, font As Integer, av As DO_Text.AlignV, ah As AlignH, use_fillColor_from_linestyle As Boolean)
        MyBase.New(use_fillColor_from_linestyle)
        Me.r = r
        Me.Text = t
        Me.font = font

        Me.ah = ah
        Me.av = av
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        Dim r As RectangleF = args.toPictureboxRect(Me.r)

        If Me.fill(args) Then
            g.FillRectangle(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            g.DrawRectangle(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
        End If

        Dim format As New StringFormat()
        If ah = AlignH.Links OrElse ah = AlignH.Blocksatz Then
            format.Alignment = StringAlignment.Near
        ElseIf ah = AlignH.Mitte Then
            format.Alignment = StringAlignment.Center
        ElseIf ah = AlignH.Rechts Then
            format.Alignment = StringAlignment.Far
        End If
        If av = DO_Text.AlignV.Oben Then
            format.LineAlignment = StringAlignment.Near
        ElseIf av = DO_Text.AlignV.Mitte Then
            format.LineAlignment = StringAlignment.Center
        ElseIf av = DO_Text.AlignV.Unten Then
            format.LineAlignment = StringAlignment.Far
        End If

        g.DrawString(Text, args.getFont(font), args.getFontBrush(font), r, format)

    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        Dim r As XRect = args.toRect(Me.r)

        If Me.fill(args) Then
            gfx.DrawRectangle(Me.getBrush(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Me.stroke(args) Then
            gfx.DrawRectangle(Me.getPen(args), r.X, r.Y, r.Width, r.Height)
        End If
        If Not args.OhneText Then
            Dim format As New XStringFormat()
            If ah = AlignH.Links Then
                format.Alignment = XStringAlignment.Near
                args.tf.Alignment = Layout.XParagraphAlignment.Left
            ElseIf ah = AlignH.Mitte Then
                format.Alignment = XStringAlignment.Near
                args.tf.Alignment = Layout.XParagraphAlignment.Center
            ElseIf ah = AlignH.Rechts Then
                format.Alignment = XStringAlignment.Near
                args.tf.Alignment = Layout.XParagraphAlignment.Right
            ElseIf ah = AlignH.Blocksatz Then
                format.Alignment = XStringAlignment.Near
                args.tf.Alignment = Layout.XParagraphAlignment.Justify
            End If
            If av = DO_Text.AlignV.Oben Then
                format.LineAlignment = XLineAlignment.Near
            ElseIf av = DO_Text.AlignV.Mitte Then
                format.LineAlignment = XLineAlignment.Center
            ElseIf av = DO_Text.AlignV.Unten Then
                format.LineAlignment = XLineAlignment.Far
            End If

            args.tf.DrawString(Text, args.getFont(font), args.getFontBrush(font), r, format)

            'gfx.DrawString(Text, args.getFont(font), args.getFontBrush(font), r, format)
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
        Throw New NotImplementedException()
    End Function

    Public Overrides Sub drawTEX_Text(writer As StreamWriter, args As GrafikTEX_DrawArgs)
        If Text <> "" Then
            Dim p1 As Point = args.toTEXpoint(New Point(Me.r.X, Me.r.Y + Me.r.Height))
            Dim p2 As Point = args.toTEXpoint(New Point(Me.r.X + Me.r.Width, Me.r.Y))

            Dim line As New CustomStringBuilder()
            line.Append("\put(")
            line.AppendPlain(p1.X)
            line.Append(",")
            line.AppendPlain(p1.Y)
            line.Append("){")
            'If winkel <> 0 Then
            'line.Append("\rotatebox[origin=c]{")
            'line.AppendPlain(winkel)
            'line.Append("}{")
            'End If
            line.Append("\makebox(")
            line.AppendPlain(p2.X - p1.X)
            line.Append(",")
            line.AppendPlain(p2.Y - p1.Y)
            line.Append(")")
            If Me.av = DO_Text.AlignV.Oben Then
                line.Append("[tl]")
            ElseIf Me.av = DO_Text.AlignV.Mitte Then
                line.Append("[cl]")
            Else
                line.Append("[bl]")
            End If
            line.Append("{\parbox{")
            line.AppendPlain(p2.X - p1.X)
            line.Append("\unitlength}{")

            If args.getFont(Me.font).farbe.Color_B <> 0 OrElse
               args.getFont(Me.font).farbe.Color_G <> 0 OrElse
               args.getFont(Me.font).farbe.Color_R <> 0 Then
                line.Append("\color[RGB]{")
                line.AppendPlain(args.getFont(Me.font).farbe.Color_R)
                line.Append(",")
                line.AppendPlain(args.getFont(Me.font).farbe.Color_G)
                line.Append(",")
                line.AppendPlain(args.getFont(Me.font).farbe.Color_B)
                line.Append("}")
            End If

            If Me.ah = DO_Text.AlignH.Mitte Then
                line.Append("\centering")
            ElseIf Me.ah = AlignH.Links Then
                line.Append("\raggedright")
            ElseIf Me.ah = AlignH.Rechts Then
                line.Append("\raggedleft")
            End If
            line.Append("\strut{}")
            line.Append(ReplaceVBCRLF_For_TEX(Text))
            line.Append("}") 'parbox end
            line.Append("}") 'makebox end
            'If winkel <> 0 Then
            'line.Append("}")
            'End If
            line.Append("}%") 'put end
            writer.WriteLine(line)
        End If
    End Sub

    Public Shared Function ReplaceVBCRLF_For_TEX(str As String) As String
        Dim erg As New StringBuilder()
        For i As Integer = 0 To str.Length - 1
            If str(i) = vbCrLf Then
                erg.Append("\\{}\strut{}")
            ElseIf str(i) = vbCr Then
                erg.Append("\\{}\strut{}")
            ElseIf str(i) <> vbLf Then
                erg.Append(str(i))
            End If
        Next
        Return erg.ToString()
    End Function

    Public Overrides Sub markAllUsedFontStyles(usedFontstyles() As Boolean)
        usedFontstyles(font) = True
    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        'mache nichts
    End Sub

    Public Enum AlignH
        Links
        Mitte
        Rechts
        Blocksatz
    End Enum
End Class
