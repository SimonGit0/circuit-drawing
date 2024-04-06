Imports System.Text
Imports PdfSharp.Drawing
Public Class AdvancedTextRenderer
    Private Const DRAW_DEBUG_RECTS As Boolean = False

    Public Shared Function measureString(g As Graphics, str As String, pos As PointF, alignX As Integer, alignY As Integer, myNormalFont As Font, myFontTiefHoch As Font, multiLineHAlign As Integer) As RectangleF
        Dim format As New StringFormat()
        format.FormatFlags = StringFormatFlags.NoClip

        Dim boxen As DrawStringBox
        boxen = analyseStr(g, str, pos, {myNormalFont, myFontTiefHoch}, multiLineHAlign)
        If Not boxen.isEmpty() Then
            Dim ox As Single = 0
            Dim oy As Single = 0

            Dim minX As Single = boxen.getBounds().X
            Dim minY As Single = boxen.getBounds().Y
            Dim maxX As Single = boxen.getBounds().X + boxen.getBounds().Width
            Dim maxY As Single = boxen.getBounds().Y + boxen.getBounds().Height

            If alignX = 0 Then
                ox = -((maxX + minX) / 2.0F - pos.X)
            ElseIf alignX > 0 Then
                ox = -(maxX - pos.X)
            Else
                ox = -(minX - pos.X)
            End If
            If alignY = 0 Then
                oy = -((maxY + minY) / 2.0F - pos.Y)
            ElseIf alignY > 0 Then
                oy = -(maxY - pos.Y)
            Else
                oy = -(minY - pos.Y)
            End If

            Dim r As RectangleF = boxen.getBounds()
            r.X += ox
            r.Y += oy
            Return r
        End If
    End Function

    Public Shared Sub drawString(g As Graphics, str As String, keineZerlegung As Boolean, pos As PointF, alignX As Integer, alignY As Integer, myNormalFont As Font, myFontTiefHoch As Font, brush As Brush, ByRef _out_BoundingBox As RectangleF, multiLineHAlign As Integer)
        Dim format As New StringFormat()
        format.FormatFlags = StringFormatFlags.NoClip

        Dim boxen As DrawStringBox
        If keineZerlegung Then
            Dim e1 As New DrawString_Line(False)
            For i As Integer = 0 To str.Length - 1
                e1.add(New DrawString_Char(str(i)))
            Next
            Dim formatVariable As New StringFormat()
            formatVariable.FormatFlags = StringFormatFlags.NoClip Or StringFormatFlags.NoWrap
            boxen = e1.calcBoundingBoxes(pos, 0, New CalcBoundingboxesArgs(g, {myNormalFont, myFontTiefHoch}, formatVariable))
        Else
            boxen = analyseStr(g, str, pos, {myNormalFont, myFontTiefHoch}, multiLineHAlign)
        End If
        If Not boxen.isEmpty() Then
            Dim ox As Single = 0
            Dim oy As Single = 0

            Dim minX As Single = boxen.getBounds().X
            Dim minY As Single = boxen.getBounds().Y
            Dim maxX As Single = boxen.getBounds().X + boxen.getBounds().Width
            Dim maxY As Single = boxen.getBounds().Y + boxen.getBounds().Height

            If alignX = 0 Then
                ox = -((maxX + minX) / 2.0F - pos.X)
            ElseIf alignX > 0 Then
                ox = -(maxX - pos.X)
            Else
                ox = -(minX - pos.X)
            End If
            If alignY = 0 Then
                oy = -((maxY + minY) / 2.0F - pos.Y)
            ElseIf alignY > 0 Then
                oy = -(maxY - pos.Y)
            Else
                oy = -(minY - pos.Y)
            End If

            boxen.draw(g, brush, ox, oy)
            _out_BoundingBox = boxen.getBounds()
            _out_BoundingBox.X += ox
            _out_BoundingBox.Y += oy

            'Debug.Print(minX.ToString() & " - " & minY.ToString() & " - " & maxX.ToString() & " - " & maxY.ToString())
        End If
    End Sub

    Public Shared Sub drawString(g As XGraphics, str As String, pos As XPoint, alignX As Integer, alignY As Integer, myNormalFont As XFont, myFontTiefHoch As XFont, brush As XBrush, ByRef _out_BoundingBox As RectangleF, multiLineHAlign As Integer)
        Dim boxen As DrawStringBox
        boxen = analyseStr(g, str, New PointF(CSng(pos.X), CSng(pos.Y)), {myNormalFont, myFontTiefHoch}, multiLineHAlign)
        If Not boxen.isEmpty() Then
            Dim ox As Double = 0
            Dim oy As Double = 0

            Dim minX As Double = boxen.getBounds().X
            Dim minY As Double = boxen.getBounds().Y
            Dim maxX As Double = boxen.getBounds().X + boxen.getBounds().Width
            Dim maxY As Double = boxen.getBounds().Y + boxen.getBounds().Height

            If alignX = 0 Then
                ox = -((maxX + minX) / 2.0F - pos.X)
            ElseIf alignX > 0 Then
                ox = -(maxX - pos.X)
            Else
                ox = -(minX - pos.X)
            End If
            If alignY = 0 Then
                oy = -((maxY + minY) / 2.0F - pos.Y)
            ElseIf alignY > 0 Then
                oy = -(maxY - pos.Y)
            Else
                oy = -(minY - pos.Y)
            End If

            boxen.draw(g, brush, ox, oy)
            _out_BoundingBox = boxen.getBounds()
            _out_BoundingBox.X += CSng(ox)
            _out_BoundingBox.Y += CSng(oy)
        End If
    End Sub

    Private Shared Function analyseStr(g As Graphics, str As String, startpos As PointF, fonts() As Font, multiLineHAlign As Integer) As DrawStringBox
        Dim formatVariable As New StringFormat()
        formatVariable.FormatFlags = StringFormatFlags.NoClip Or StringFormatFlags.NoWrap

        Dim el As DrawString_Element = leseStr(New getNextTokenString(str), True, False, 0, multiLineHAlign)
        'Return el.CalcBoundingBoxes(g, startpos, myNormalFont, myFontTiefHoch, format3, format2, format1, formatVariable)
        Return el.calcBoundingBoxes(startpos, 0, New CalcBoundingboxesArgs(g, fonts, formatVariable))
    End Function

    Private Shared Function analyseStr(g As XGraphics, str As String, startpos As PointF, fonts() As XFont, multiLineHAlign As Integer) As DrawStringBox
        Dim formatVariable As New XStringFormat()

        Dim el As DrawString_Element = leseStr(New getNextTokenString(str), True, False, 0, multiLineHAlign)
        'Return el.CalcBoundingBoxes(g, startpos, myNormalFont, myFontTiefHoch, format3, format2, format1, formatVariable)
        Return el.calcBoundingBoxes(startpos, 0, New CalcBoundingboxesArgsX(g, fonts, formatVariable))
    End Function

    Private Shared Function leseStr(str As getNextTokenString, readAll As Boolean, ByRef mathmode As Boolean, tiefe As Integer, multiLineHAlign As Integer) As DrawString_Element
        Dim args As New AddNextCharArgs(multiLineHAlign, mathmode)

        If readAll Then
            args.endCurrentLineAtKlammerPos = -1
        Else
            args.endCurrentLineAtKlammerPos = 0
        End If
        args.klammerpos = 0
        Dim myToken As Token
        While str.hatNochToken()
            myToken = str.getNextToken()
            If myToken.isChar Then
                If myToken.str = "$" Then
                    args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                    mathmode = Not mathmode
                    args.currentLineElement = New DrawString_Line(mathmode) 'Neues Element anfangen
                    If tiefe > 0 Then
                        Throw New Exception("Mathemodus darf hier nicht geändert werden!")
                    End If
                ElseIf myToken.str = "{" Then
                    args.klammerpos += 1
                ElseIf myToken.str = "}" Then
                    args.klammerpos -= 1
                Else
                    If mathmode Then
                        If myToken.str = "_" Then
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            args.lastLine.add(leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign), DrawString_HorizontalMode.DrawMode.Tiefgestellt) 'Tiefgestelltes Element
                            args.currentLineElement = New DrawString_Line(mathmode) 'Neues Element anfangen
                        ElseIf myToken.str = "^" Then
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            args.lastLine.add(leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign), DrawString_HorizontalMode.DrawMode.Hochgestellt) 'Hochgestelltes Element
                            args.currentLineElement = New DrawString_Line(mathmode) 'Neues Element anfangen
                        ElseIf myToken.str = "-" Then
                            addNextChar("–"c, args)
                        Else
                            addNextChar(myToken.str(0), args)
                        End If
                    Else
                        addNextChar(myToken.str(0), args)
                    End If
                End If
            Else
                If tiefe = 0 AndAlso myToken.str = "\" Then
                    args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                    args.lastLine = New DrawString_HorizontalMode() 'Neue Linie Anfangen
                    args.verticalList.add(args.lastLine) 'Neue Linie in Line-List aufnehmen!
                    args.currentLineElement = New DrawString_Line(mathmode) 'Neues Element anfgangen
                ElseIf myToken.str.Length = 1 Then
                    If myToken.str = "," Then
                        addNextChar(" "c, args)
                    Else
                        addNextChar(myToken.str(0), args)
                    End If
                Else
                    Select Case myToken.str
                        Case "alpha" : addNextChar("α"c, args)
                        Case "beta" : addNextChar("β"c, args)
                        Case "gamma" : addNextChar("γ"c, args)
                        Case "delta" : addNextChar("δ"c, args)
                        Case "epsilon" : addNextChar("ϵ"c, args)
                        Case "varepsilon" : addNextChar("ε"c, args)
                        Case "zeta" : addNextChar("ζ"c, args)
                        Case "eta" : addNextChar("η"c, args)
                        Case "theta" : addNextChar("θ"c, args)
                        Case "vartheta" : addNextChar("ϑ"c, args)
                        Case "iota" : addNextChar("ι"c, args)
                        Case "kappa" : addNextChar("κ"c, args)
                        Case "lambda" : addNextChar("λ"c, args)
                        Case "mu" : addNextChar("μ"c, args)
                        Case "nu" : addNextChar("ν"c, args)
                        Case "xi" : addNextChar("ξ"c, args)
                        Case "pi" : addNextChar("π"c, args)
                        Case "varpi" : addNextChar("ϖ"c, args)
                        Case "rho" : addNextChar("ρ"c, args)
                        Case "varrho" : addNextChar("ϱ"c, args)
                        Case "sigma" : addNextChar("σ"c, args)
                        Case "varsigma" : addNextChar("ς"c, args)
                        Case "tau" : addNextChar("τ"c, args)
                        Case "upsilon" : addNextChar("υ"c, args)
                        Case "phi" : addNextChar("φ"c, args)
                        Case "varphi" : addNextChar("φ"c, args)
                        Case "chi" : addNextChar("χ"c, args)
                        Case "psi" : addNextChar("ψ"c, args)
                        Case "omega" : addNextChar("ω"c, args)

                        Case "Gamma" : addNextChar("Γ"c, args)
                        Case "Delta" : addNextChar("Δ"c, args)
                        Case "Theta" : addNextChar("Θ"c, args)
                        Case "Lambda" : addNextChar("Λ"c, args)
                        Case "Xi" : addNextChar("Ξ"c, args)
                        Case "Pi" : addNextChar("Π"c, args)
                        Case "Sigma" : addNextChar("Σ"c, args)
                        Case "Upsilon" : addNextChar("Υ"c, args)
                        Case "Phi" : addNextChar("Φ"c, args)
                        Case "Psi" : addNextChar("Ψ"c, args)
                        Case "Omega" : addNextChar("Ω"c, args)

                        Case "geq" : addNextChar("≥"c, args)
                        Case "leq" : addNextChar("≤"c, args)

                        Case "land" : addNextChar("∧"c, args)
                        Case "lor" : addNextChar("∨"c, args)

                        Case "cdot" : addNextChar("⋅"c, args)

                        Case "text", "mathrm"
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            args.lastLine.add(leseStr(str, False, False, tiefe + 1, multiLineHAlign), DrawString_HorizontalMode.DrawMode.Normal) 'Neues TEXT Element
                            args.currentLineElement = New DrawString_Line(mathmode) 'Neue Linie anfangen
                        Case "frac"
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            Dim zähler As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            Dim nenner As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            args.lastLine.add(New DrawString_Bruch(zähler, nenner, False), DrawString_HorizontalMode.DrawMode.Normal) 'Bruch hinzufügen
                            args.currentLineElement = New DrawString_Line(mathmode) 'neue Linie anfangen
                        Case "dfrac"
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            Dim zähler As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            Dim nenner As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            args.lastLine.add(New DrawString_Bruch(zähler, nenner, True), DrawString_HorizontalMode.DrawMode.Normal) 'Bruch hinzufügen
                            args.currentLineElement = New DrawString_Line(mathmode) 'neue Linie anfangen
                        Case "overline"
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            Dim oline As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            args.lastLine.add(New DrawString_MitLinie(oline, True, False), DrawString_HorizontalMode.DrawMode.Normal)
                            args.currentLineElement = New DrawString_Line(mathmode) 'neue Linie anfangen
                        Case "underline"
                            args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal) 'altes Element abschließen
                            Dim oline As DrawString_Element = leseStr(str, False, mathmode, tiefe + 1, multiLineHAlign)
                            args.lastLine.add(New DrawString_MitLinie(oline, False, True), DrawString_HorizontalMode.DrawMode.Normal)
                            args.currentLineElement = New DrawString_Line(mathmode) 'neue Linie anfangen
                        Case "int"
                            addNextChar("∫"c, args)
                    End Select
                End If
            End If

            If args.endCurrentLineAtKlammerPos <> -1 AndAlso args.endCurrentLineAtKlammerPos = args.klammerpos Then
                If args.lastLine.isEmpty Then
                    Return args.currentLineElement
                Else
                    args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal)
                    Return args.lastLine
                End If
            End If

        End While
        args.lastLine.add(args.currentLineElement, DrawString_HorizontalMode.DrawMode.Normal)
        If args.verticalList.liste.Count > 1 Then
            Return args.verticalList
        Else
            Return args.lastLine
        End If
    End Function

    Private Shared Sub addNextChar(c As Char, args As AddNextCharArgs)
        With args
            .currentLineElement.add(New DrawString_Char(c))
        End With
    End Sub

    Private Class AddNextCharArgs
        Public currentLineElement As DrawString_Line
        Public lastLine As DrawString_HorizontalMode
        Public verticalList As DrawString_VerticalMode

        Public endCurrentLineAtKlammerPos As Integer
        Public klammerpos As Integer

        Public Sub New(multiLineHAlign As Integer, mathmode As Boolean)
            verticalList = New DrawString_VerticalMode(multiLineHAlign)
            lastLine = New DrawString_HorizontalMode
            verticalList.add(lastLine)
            currentLineElement = New DrawString_Line(mathmode)
            klammerpos = 0
            endCurrentLineAtKlammerPos = -1
        End Sub
    End Class

#Region "Token einlesen"
    Public Class getNextTokenString
        Private str As String
        Private pos As Integer

        Public Sub New(str As String)
            Me.str = str
            Me.pos = 0
        End Sub

        Public Function getNextToken() As Token
            If isLeer(str(pos)) Then
                While pos < str.Length - 1 AndAlso isLeer(str(pos + 1))
                    pos += 1
                End While
            End If
            If str(pos) = "\" AndAlso pos < str.Length - 1 Then
                Dim tokenName As New StringBuilder()
                Dim pNeu As Integer = pos + 1
                For i As Integer = pos + 1 To str.Length - 1
                    If Not Char.IsLetter(str(i)) Then
                        If tokenName.Length = 0 Then
                            tokenName.Append(str(i))
                        End If
                        If str(i) = " " OrElse tokenName.Length = 1 Then
                            pNeu += 1
                        End If
                        Exit For
                    Else
                        tokenName.Append(str(i))
                    End If
                    pNeu += 1
                Next
                Dim t As New Token(False, tokenName.ToString())
                pos = pNeu
                Return t
            Else
                Dim t As New Token(True, str(pos))
                pos += 1
                Return t
            End If
        End Function

        Private Function isLeer(c As Char) As Boolean
            Return c = " " OrElse c = vbCrLf OrElse c = vbCr OrElse c = vbLf
        End Function

        Public Function hatNochToken() As Boolean
            Return pos < str.Length
        End Function

    End Class

    Public Class Token
        Public isChar As Boolean
        Public str As String
        Public Sub New(isChar As Boolean, str As String)
            Me.isChar = isChar
            Me.str = str
        End Sub
    End Class
#End Region

#Region "DrawStringBox"
    Private MustInherit Class DrawStringBox
        Public MustOverride Sub draw(g As Graphics, brush As Brush, ox As Single, oy As Single)

        Public MustOverride Sub draw(g As XGraphics, brush As XBrush, ox As Double, oy As Double)

        Public MustOverride Function getBounds() As RectangleF

        Public MustOverride Function isEmpty() As Boolean

        Public MustOverride Sub verschiebe(dx As Single, dy As Single)
    End Class

    Private Class DrawStringCharBox
        Inherits DrawStringBox

        Public str As String
        Public box As RectangleF
        Public drawPoint As PointF
        Public f As Font
        Public fX As XFont
        Public Sub New(str As String, box As RectangleF, drawPoint As PointF, f As Font)
            Me.str = str
            Me.box = box
            Me.drawPoint = drawPoint
            Me.f = f
        End Sub

        Public Sub New(str As String, box As RectangleF, drawPoint As PointF, fx As XFont)
            Me.str = str
            Me.box = box
            Me.drawPoint = drawPoint
            Me.fX = fx
        End Sub

        Public Overrides Sub draw(g As Graphics, brush As Brush, ox As Single, oy As Single)
            If DRAW_DEBUG_RECTS Then
                g.DrawRectangle(Pens.Red, box.X + ox, box.Y + oy, box.Width, box.Height)
            End If
            g.DrawString(str, f, brush, New PointF(drawPoint.X + ox, drawPoint.Y + oy))
        End Sub

        Public Overrides Sub draw(g As XGraphics, brush As XBrush, ox As Double, oy As Double)
            If DRAW_DEBUG_RECTS Then
                g.DrawRectangle(XPens.Red, box.X + ox, box.Y + oy, box.Width, box.Height)
            End If
            g.DrawString(str, fX, brush, New XPoint(drawPoint.X + ox, drawPoint.Y + oy), XStringFormats.TopCenter)
        End Sub

        Public Overrides Sub verschiebe(dx As Single, dy As Single)
            Me.box.X += dx
            Me.box.Y += dy
            Me.drawPoint.X += dx
            Me.drawPoint.Y += dy
        End Sub

        Public Overrides Function getBounds() As RectangleF
            Return box
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return False
        End Function
    End Class

    Private Class DrawStringEmptyBox
        Inherits DrawStringBox

        Private pos As PointF

        Public Sub New(pos As PointF)
            Me.pos = pos
        End Sub

        Public Overrides Sub draw(g As Graphics, brush As Brush, ox As Single, oy As Single)
            'mache nichts!
        End Sub

        Public Overrides Sub draw(g As XGraphics, brush As XBrush, ox As Double, oy As Double)
            'mache nichts!
        End Sub

        Public Overrides Sub verschiebe(dx As Single, dy As Single)
            pos.X += dx
            pos.Y += dy
        End Sub

        Public Overrides Function getBounds() As RectangleF
            Return New RectangleF(pos, New SizeF(0, 0))
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return True
        End Function
    End Class

    Private Class DrawStringLineBox
        Inherits DrawStringBox

        Private start As PointF
        Private ende As PointF
        Private breite As Single

        Public Sub New(start As PointF, ende As PointF, breite As Single)
            Me.start = start
            Me.ende = ende
            Me.breite = breite
        End Sub

        Public Overrides Sub draw(g As Graphics, brush As Brush, ox As Single, oy As Single)
            g.DrawLine(New Pen(brush, breite), New PointF(start.X + ox, start.Y + oy), New PointF(ende.X + ox, ende.Y + oy))
        End Sub

        Public Overrides Sub draw(g As XGraphics, brush As XBrush, ox As Double, oy As Double)
            If TypeOf brush IsNot XSolidBrush Then
                Throw New NotImplementedException("Kann in dieser Farbe den Text nicht darstellen")
            End If
            Dim c As XColor = DirectCast(brush, XSolidBrush).Color
            g.DrawLine(New XPen(c, breite), New XPoint(start.X + ox, start.Y + oy), New XPoint(ende.X + ox, ende.Y + oy))
        End Sub

        Public Overrides Sub verschiebe(dx As Single, dy As Single)
            Me.start.X += dx
            Me.start.Y += dy
            Me.ende.X += dx
            Me.ende.Y += dy
        End Sub

        Public Overrides Function getBounds() As RectangleF
            Return New RectangleF(Math.Min(start.X, ende.X), Math.Min(start.Y, ende.Y), Math.Max(start.X, ende.X) - Math.Min(start.X, ende.X), Math.Max(start.Y, ende.Y) - Math.Min(start.Y, ende.Y))
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return False
        End Function
    End Class

    Private Class DrawStringMultiBox
        Inherits DrawStringBox

        Private liste As List(Of DrawStringBox)
        Private boundingBox As RectangleF
        Public Sub New(liste As List(Of DrawStringBox), boundsOnlyFrom1stBox As Boolean)
            Me.liste = liste
            If boundsOnlyFrom1stBox Then
                Me.boundingBox = liste(0).getBounds()
            Else
                Dim minX As Single = Single.MaxValue
                Dim minY As Single = Single.MaxValue
                Dim maxX As Single = Single.MinValue
                Dim maxY As Single = Single.MinValue

                Dim r As RectangleF
                For Each b In liste
                    r = b.getBounds()
                    minX = Math.Min(minX, r.X)
                    minY = Math.Min(minY, r.Y)
                    maxX = Math.Max(maxX, r.X + r.Width)
                    maxY = Math.Max(maxY, r.Y + r.Height)
                Next
                Me.boundingBox = New RectangleF(minX, minY, maxX - minX, maxY - minY)
            End If
        End Sub

        Public Overrides Sub draw(g As Graphics, brush As Brush, ox As Single, oy As Single)
            If DRAW_DEBUG_RECTS Then
                g.DrawRectangle(Pens.Blue, boundingBox.X + ox, boundingBox.Y + oy, boundingBox.Width, boundingBox.Height)
            End If
            For Each b In liste
                b.draw(g, brush, ox, oy)
            Next
        End Sub

        Public Overrides Sub draw(g As XGraphics, brush As XBrush, ox As Double, oy As Double)
            If DRAW_DEBUG_RECTS Then
                g.DrawRectangle(XPens.Blue, boundingBox.X + ox, boundingBox.Y + oy, boundingBox.Width, boundingBox.Height)
            End If
            For Each b In liste
                b.draw(g, brush, ox, oy)
            Next
        End Sub

        Public Overrides Sub verschiebe(dx As Single, dy As Single)
            For Each b In liste
                b.verschiebe(dx, dy)
            Next
            boundingBox.X += dx
            boundingBox.Y += dy
        End Sub

        Public Overrides Function getBounds() As RectangleF
            Return boundingBox
        End Function

        Public Overrides Function isEmpty() As Boolean
            If liste.Count = 0 Then Return True
            For Each b In liste
                If Not b.isEmpty() Then Return False
            Next
            Return True
        End Function
    End Class
#End Region

#Region "DrawString_Element"
    Private Class DrawString_Char
        Public c As Char
        Public Sub New(c As Char)
            Me.c = c
        End Sub
    End Class

    Private MustInherit Class DrawString_Element

        Public MustOverride Function isEmpty() As Boolean

        Public MustOverride Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox

        Public MustOverride Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
    End Class

    Private Class DrawString_Line
        Inherits DrawString_Element

        Private liste As New List(Of DrawString_Char)
        Public modus As DrawMode = DrawMode.Normal
        Private matheModus As Boolean
        Public Sub New(matheModus As Boolean)
            Me.matheModus = matheModus
            liste = New List(Of DrawString_Char)
        End Sub

        Public Sub add(e As DrawString_Char)
            Me.liste.Add(e)
        End Sub

        Public Function Count() As Integer
            Return liste.Count
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox
            If matheModus AndAlso Settings.getSettings().kursiverTextImMatheModus Then
                Return CalcBoundingBoxesSingleStr(args.g, startpos, args.fontsKursiv(fontIndex), args.RenderFormat_Variabel)
            Else
                Return CalcBoundingBoxesSingleStr(args.g, startpos, args.fonts(fontIndex), args.RenderFormat_Variabel)
            End If
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
            If matheModus AndAlso Settings.getSettings().kursiverTextImMatheModus Then
                Return CalcBoundingBoxesSingleStr(args.g, startpos, args.fontsKursiv(fontIndex), args.RenderFormat_Variabel)
            Else
                Return CalcBoundingBoxesSingleStr(args.g, startpos, args.fonts(fontIndex), args.RenderFormat_Variabel)
            End If
        End Function

        Private Function CalcBoundingBoxesSingleStr(g As Graphics, startpos As PointF, myFont As Font, RenderFormat_Variabel As StringFormat) As DrawStringBox
            RenderFormat_Variabel.SetMeasurableCharacterRanges({New CharacterRange(0, Me.liste.Count)})
            Dim str As New StringBuilder()
            For i As Integer = 0 To liste.Count - 1
                str.Append(liste(i).c)
            Next
            Dim str1 As String = str.ToString()
            Dim regionen() As Region = g.MeasureCharacterRanges(str1 & "I", myFont, New RectangleF(0, 0, 0, 0), RenderFormat_Variabel)
            Dim rect As RectangleF = regionen(0).GetBounds(g)
            Dim xKomp As Single = -rect.X 'Kompensieren, dess Startpunktes bei x>0, indem man um diese Anzahl weiter links malt!
            rect.X = startpos.X
            rect.Y += startpos.Y
            Return New DrawStringCharBox(str1, rect, New PointF(rect.X + xKomp, rect.Y), myFont)
        End Function

        Private Function CalcBoundingBoxesSingleStr(g As XGraphics, startpos As PointF, myFont As XFont, RenderFormat_Variabel As XStringFormat) As DrawStringBox
            Dim str As New StringBuilder()
            For i As Integer = 0 To liste.Count - 1
                str.Append(liste(i).c)
            Next
            Dim str1 As String = str.ToString()
            Dim size As XSize = g.MeasureString(str1, myFont, RenderFormat_Variabel)
            Dim rect As New RectangleF(startpos.X, startpos.Y, CSng(size.Width), CSng(size.Height))
            Return New DrawStringCharBox(str1, rect, New PointF(rect.X + rect.Width / 2, rect.Y), myFont)
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return liste.Count = 0
        End Function
    End Class

    Private Class DrawString_VerticalMode
        Inherits DrawString_Element

        Public liste As List(Of DrawString_Element)
        Private multiLineHAlign As Integer
        Public Sub New(multiLineHAlign As Integer)
            liste = New List(Of DrawString_Element)
            Me.multiLineHAlign = multiLineHAlign
        End Sub

        Public Sub add(line As DrawString_Element)
            'If Not line.isEmpty() Then
            Me.liste.Add(line) 'Auch leere Linien hinzufügen
            'End If
        End Sub

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox
            Dim erg As New List(Of DrawStringBox)
            Dim pos As PointF = startpos
            Dim breite As Single = 0
            For i As Integer = 0 To liste.Count - 1
                Dim neueBox As DrawStringBox = liste(i).calcBoundingBoxes(pos, fontIndex, args)
                If TypeOf neueBox Is DrawStringEmptyBox Then
                    Dim line As New DrawString_Line(False)
                    line.add(New DrawString_Char(" "c))
                    neueBox = line.calcBoundingBoxes(pos, fontIndex, args)
                    breite = Math.Max(breite, neueBox.getBounds().Width)
                End If
                erg.Add(neueBox)

                pos.Y += neueBox.getBounds().Height
            Next

            If multiLineHAlign = 0 Then
                'Align center
                For i As Integer = 0 To erg.Count - 1
                    Dim dx As Single = (breite - erg(i).getBounds().Width) / 2
                    erg(i).verschiebe(dx, 0)
                Next
            ElseIf multiLineHAlign > 0 Then
                'Align right
                For i As Integer = 0 To erg.Count - 1
                    Dim dx As Single = breite - erg(i).getBounds().Width
                    erg(i).verschiebe(dx, 0)
                Next
            End If

            Return New DrawStringMultiBox(erg, False)
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
            Dim erg As New List(Of DrawStringBox)
            Dim pos As PointF = startpos
            Dim breite As Single = 0
            For i As Integer = 0 To liste.Count - 1
                Dim neueBox As DrawStringBox = liste(i).calcBoundingBoxes(pos, fontIndex, args)
                If TypeOf neueBox Is DrawStringEmptyBox Then
                    Dim line As New DrawString_Line(False)
                    line.add(New DrawString_Char(" "c))
                    neueBox = line.calcBoundingBoxes(pos, fontIndex, args)
                    breite = Math.Max(breite, neueBox.getBounds().Width)
                End If
                erg.Add(neueBox)

                pos.Y += neueBox.getBounds().Height
            Next

            If multiLineHAlign = 0 Then
                'Align center
                For i As Integer = 0 To erg.Count - 1
                    Dim dx As Single = (breite - erg(i).getBounds().Width) / 2
                    erg(i).verschiebe(dx, 0)
                Next
            ElseIf multiLineHAlign > 0 Then
                'Align right
                For i As Integer = 0 To erg.Count - 1
                    Dim dx As Single = breite - erg(i).getBounds().Width
                    erg(i).verschiebe(dx, 0)
                Next
            End If

            Return New DrawStringMultiBox(erg, False)
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return liste.Count = 0
        End Function
    End Class

    Private Class DrawString_HorizontalMode
        Inherits DrawString_Element

        Public liste As List(Of Tuple(Of DrawMode, DrawString_Element))

        Public Sub New()
            liste = New List(Of Tuple(Of DrawMode, DrawString_Element))()
        End Sub

        Public Sub add(line As DrawString_Element, mode As DrawMode)
            If Not line.isEmpty() Then
                Me.liste.Add(New Tuple(Of DrawMode, DrawString_Element)(mode, line))
            End If
        End Sub

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox
            If liste.Count = 0 Then
                Return New DrawStringEmptyBox(startpos)
            End If

            Dim erg As New List(Of DrawStringBox)
            Dim pos As PointF = startpos
            Dim nextPosRight As Single = -1
            Dim hasNextPosRight As Boolean = False

            For i As Integer = 0 To liste.Count - 1
                Dim neueBox As DrawStringBox
                If liste(i).Item1 = DrawMode.Tiefgestellt Then
                    Dim newFontIndex As Integer
                    If fontIndex < args.fonts.Length - 1 Then
                        newFontIndex = fontIndex + 1
                    Else
                        newFontIndex = fontIndex
                    End If

                    neueBox = liste(i).Item2.calcBoundingBoxes(New PointF(pos.X, pos.Y), newFontIndex, args)
                    Dim neuesOben As Single = startpos.Y + args.fonts(fontIndex).Size * 0.5F
                    Dim dy As Single = neuesOben - neueBox.getBounds().Y
                    neueBox.verschiebe(0, dy)
                ElseIf liste(i).Item1 = DrawMode.Hochgestellt Then
                    Dim newFontIndex As Integer
                    If fontIndex < args.fonts.Length - 1 Then
                        newFontIndex = fontIndex + 1
                    Else
                        newFontIndex = fontIndex
                    End If

                    neueBox = liste(i).Item2.calcBoundingBoxes(New PointF(pos.X, pos.Y), newFontIndex, args)
                    Dim neuesOben As Single = startpos.Y + args.fonts(fontIndex).Size * 0.5F - neueBox.getBounds().Height
                    Dim dy As Single = neuesOben - neueBox.getBounds().Y
                    neueBox.verschiebe(0, dy)
                Else
                    neueBox = liste(i).Item2.calcBoundingBoxes(pos, fontIndex, args)
                End If
                erg.Add(neueBox)

                If Not hasNextPosRight AndAlso (liste(i).Item1 = DrawMode.Tiefgestellt AndAlso i < liste.Count - 1 AndAlso liste(i + 1).Item1 = DrawMode.Hochgestellt) OrElse
                                               (liste(i).Item1 = DrawMode.Hochgestellt AndAlso i < liste.Count - 1 AndAlso liste(i + 1).Item1 = DrawMode.Tiefgestellt) Then

                    nextPosRight = erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width
                    hasNextPosRight = True
                Else
                    If hasNextPosRight Then
                        pos = New PointF(Math.Max(nextPosRight, erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width), pos.Y)
                    Else
                        pos = New PointF(erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width, pos.Y)
                    End If
                    hasNextPosRight = False
                End If
            Next
            Return New DrawStringMultiBox(erg, False)
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
            If liste.Count = 0 Then
                Return New DrawStringEmptyBox(startpos)
            End If

            Dim erg As New List(Of DrawStringBox)
            Dim pos As PointF = startpos
            Dim nextPosRight As Single = -1
            Dim hasNextPosRight As Boolean = False

            For i As Integer = 0 To liste.Count - 1
                Dim neueBox As DrawStringBox
                If liste(i).Item1 = DrawMode.Tiefgestellt Then
                    Dim newFontIndex As Integer
                    If fontIndex < args.fonts.Length - 1 Then
                        newFontIndex = fontIndex + 1
                    Else
                        newFontIndex = fontIndex
                    End If

                    neueBox = liste(i).Item2.calcBoundingBoxes(New PointF(pos.X, pos.Y), newFontIndex, args)
                    Dim neuesOben As Single = startpos.Y + CSng(args.fonts(fontIndex).Size) * 0.5F
                    Dim dy As Single = neuesOben - neueBox.getBounds().Y
                    neueBox.verschiebe(0, dy)
                ElseIf liste(i).Item1 = DrawMode.Hochgestellt Then
                    Dim newFontIndex As Integer
                    If fontIndex < args.fonts.Length - 1 Then
                        newFontIndex = fontIndex + 1
                    Else
                        newFontIndex = fontIndex
                    End If

                    neueBox = liste(i).Item2.calcBoundingBoxes(New PointF(pos.X, pos.Y), newFontIndex, args)
                    Dim neuesOben As Single = startpos.Y + CSng(args.fonts(fontIndex).Size) * 0.5F - neueBox.getBounds().Height
                    Dim dy As Single = neuesOben - neueBox.getBounds().Y
                    neueBox.verschiebe(0, dy)
                Else
                    neueBox = liste(i).Item2.calcBoundingBoxes(pos, fontIndex, args)
                End If
                erg.Add(neueBox)

                If Not hasNextPosRight AndAlso (liste(i).Item1 = DrawMode.Tiefgestellt AndAlso i < liste.Count - 1 AndAlso liste(i + 1).Item1 = DrawMode.Hochgestellt) OrElse
                                               (liste(i).Item1 = DrawMode.Hochgestellt AndAlso i < liste.Count - 1 AndAlso liste(i + 1).Item1 = DrawMode.Tiefgestellt) Then

                    nextPosRight = erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width
                    hasNextPosRight = True
                Else
                    If hasNextPosRight Then
                        pos = New PointF(Math.Max(nextPosRight, erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width), pos.Y)
                    Else
                        pos = New PointF(erg(erg.Count - 1).getBounds().X + erg(erg.Count - 1).getBounds().Width, pos.Y)
                    End If
                    hasNextPosRight = False
                End If
            Next
            Return New DrawStringMultiBox(erg, False)
        End Function

        Public Overrides Function isEmpty() As Boolean
            Return liste.Count = 0
        End Function

        Public Enum DrawMode
            Normal
            Tiefgestellt
            Hochgestellt
        End Enum
    End Class

    Private Class DrawString_Bruch
        Inherits DrawString_Element

        Private z As DrawString_Element
        Private n As DrawString_Element
        Private größeNichtÄndern As Boolean
        Public Sub New(z As DrawString_Element, n As DrawString_Element, größeNichtÄndern As Boolean)
            Me.z = z
            Me.n = n
            Me.größeNichtÄndern = größeNichtÄndern
        End Sub

        Public Overrides Function isEmpty() As Boolean
            Return False
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox
            Dim neuerfontIndex = fontIndex + 1
            If neuerfontIndex > args.fonts.Length - 1 Then
                neuerfontIndex = fontIndex
            End If
            If Me.größeNichtÄndern Then
                neuerfontIndex = fontIndex
            End If
            Dim boxZähler As DrawStringBox = z.calcBoundingBoxes(startpos, neuerfontIndex, args)
            Dim boxNenner As DrawStringBox = n.calcBoundingBoxes(startpos, neuerfontIndex, args)

            Dim neuePosY As Single = startpos.Y + args.fonts(fontIndex).Size * 0.5F - boxZähler.getBounds().Height
            Dim dy1 As Single = neuePosY - boxZähler.getBounds().Y

            neuePosY = startpos.Y + args.fonts(fontIndex).Size * 0.5F
            Dim dy2 As Single = neuePosY - boxNenner.getBounds().Y

            Dim dx1, dx2 As Single

            If boxZähler.getBounds().Width > boxNenner.getBounds().Width Then
                dx1 = 0.0F
                dx2 = (boxZähler.getBounds().Width - boxNenner.getBounds().Width) * 0.5F
            Else
                dx2 = 0.0F
                dx1 = (boxNenner.getBounds().Width - boxZähler.getBounds().Width) * 0.5F
            End If

            boxZähler.verschiebe(dx1, dy1)
            boxNenner.verschiebe(dx2, dy2)
            Dim liste As New List(Of DrawStringBox)(3)
            liste.Add(boxZähler)
            liste.Add(boxNenner)
            liste.Add(New DrawStringLineBox(New PointF(startpos.X, neuePosY), New PointF(startpos.X + Math.Max(boxZähler.getBounds().Width, boxNenner.getBounds().Width), neuePosY), args.fonts(neuerfontIndex).Size * 0.05F))

            Return New DrawStringMultiBox(liste, False)
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
            Dim neuerfontIndex = fontIndex + 1
            If neuerfontIndex > args.fonts.Length - 1 Then
                neuerfontIndex = fontIndex
            End If
            If Me.größeNichtÄndern Then
                neuerfontIndex = fontIndex
            End If
            Dim boxZähler As DrawStringBox = z.calcBoundingBoxes(startpos, neuerfontIndex, args)
            Dim boxNenner As DrawStringBox = n.calcBoundingBoxes(startpos, neuerfontIndex, args)

            Dim neuePosY As Single = startpos.Y + CSng(args.fonts(fontIndex).Size) * 0.5F - boxZähler.getBounds().Height
            Dim dy1 As Single = neuePosY - boxZähler.getBounds().Y

            neuePosY = startpos.Y + CSng(args.fonts(fontIndex).Size) * 0.5F
            Dim dy2 As Single = neuePosY - boxNenner.getBounds().Y

            Dim dx1, dx2 As Single

            If boxZähler.getBounds().Width > boxNenner.getBounds().Width Then
                dx1 = 0.0F
                dx2 = (boxZähler.getBounds().Width - boxNenner.getBounds().Width) * 0.5F
            Else
                dx2 = 0.0F
                dx1 = (boxNenner.getBounds().Width - boxZähler.getBounds().Width) * 0.5F
            End If

            boxZähler.verschiebe(dx1, dy1)
            boxNenner.verschiebe(dx2, dy2)
            Dim liste As New List(Of DrawStringBox)(3)
            liste.Add(boxZähler)
            liste.Add(boxNenner)
            liste.Add(New DrawStringLineBox(New PointF(startpos.X, neuePosY), New PointF(startpos.X + Math.Max(boxZähler.getBounds().Width, boxNenner.getBounds().Width), neuePosY), CSng(args.fonts(neuerfontIndex).Size) * 0.05F))

            Return New DrawStringMultiBox(liste, False)
        End Function
    End Class

    Private Class DrawString_MitLinie
        Inherits DrawString_Element

        Private intern As DrawString_Element
        Private overline As Boolean
        Private underline As Boolean
        Public Sub New(intern As DrawString_Element, overline As Boolean, underline As Boolean)
            Me.intern = intern
            Me.overline = overline
            Me.underline = underline
        End Sub

        Public Overrides Function isEmpty() As Boolean
            Return False
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgs) As DrawStringBox
            Dim boxIntern As DrawStringBox = intern.calcBoundingBoxes(startpos, fontIndex, args)

            Dim liste As New List(Of DrawStringBox)(2)
            Dim rect As RectangleF = boxIntern.getBounds()

            liste.Add(boxIntern)

            Dim EM_Heigth As Single = args.fonts(fontIndex).FontFamily.GetEmHeight(args.fonts(fontIndex).Style)
            Dim descend As Single = args.fonts(fontIndex).FontFamily.GetCellDescent(args.fonts(fontIndex).Style) * args.fonts(fontIndex).Size / EM_Heigth

            If overline Then
                liste.Add(New DrawStringLineBox(New PointF(rect.X, rect.Y + descend * 0.5F), New PointF(rect.X + rect.Width, rect.Y + descend * 0.5F), args.fonts(fontIndex).Size * 0.05F))
            End If
            If underline Then
                liste.Add(New DrawStringLineBox(New PointF(rect.X, rect.Y + rect.Height - descend * 0.5F), New PointF(rect.X + rect.Width, rect.Y + rect.Height - descend * 0.5F), args.fonts(fontIndex).Size * 0.05F))
            End If
            Dim erg As New DrawStringMultiBox(liste, True)

            Return erg
        End Function

        Public Overrides Function calcBoundingBoxes(startpos As PointF, fontIndex As Integer, args As CalcBoundingboxesArgsX) As DrawStringBox
            Dim boxIntern As DrawStringBox = intern.calcBoundingBoxes(startpos, fontIndex, args)

            Dim liste As New List(Of DrawStringBox)(2)
            Dim rect As RectangleF = boxIntern.getBounds()

            liste.Add(boxIntern)

            Dim EM_Heigth As Single = args.fonts(fontIndex).FontFamily.GetEmHeight(args.fonts(fontIndex).Style)
            Dim descend As Single = args.fonts(fontIndex).FontFamily.GetCellDescent(args.fonts(fontIndex).Style) * CSng(args.fonts(fontIndex).Size) / EM_Heigth

            If overline Then
                liste.Add(New DrawStringLineBox(New PointF(rect.X, rect.Y + descend * 0.5F), New PointF(rect.X + rect.Width, rect.Y + descend * 0.5F), CSng(args.fonts(fontIndex).Size) * 0.05F))
            End If
            If underline Then
                liste.Add(New DrawStringLineBox(New PointF(rect.X, rect.Y + rect.Height - descend * 0.5F), New PointF(rect.X + rect.Width, rect.Y + rect.Height - descend * 0.5F), CSng(args.fonts(fontIndex).Size) * 0.05F))
            End If
            Dim erg As New DrawStringMultiBox(liste, True)

            Return erg
        End Function
    End Class

    Private Class CalcBoundingboxesArgs
        Public g As Graphics
        Public fonts() As Font
        Public fontsKursiv() As Font
        Public RenderFormat_Variabel As StringFormat

        Public Sub New(g As Graphics, f() As Font, renderFormat As StringFormat)
            Me.g = g
            Me.fonts = f

            If Settings.getSettings().kursiverTextImMatheModus Then
                ReDim Me.fontsKursiv(f.Length - 1)
                For i As Integer = 0 To f.Length - 1
                    fontsKursiv(i) = New Font(f(i), Drawing.FontStyle.Italic)
                Next
            End If

            Me.RenderFormat_Variabel = renderFormat
        End Sub
    End Class

    Private Class CalcBoundingboxesArgsX
        Public g As XGraphics
        Public fonts() As XFont
        Public fontsKursiv() As XFont
        Public RenderFormat_Variabel As XStringFormat

        Public Sub New(g As XGraphics, f() As XFont, renderFormat As XStringFormat)
            Me.g = g
            Me.fonts = f

            If Settings.getSettings().kursiverTextImMatheModus Then
                ReDim Me.fontsKursiv(f.Length - 1)
                For i As Integer = 0 To f.Length - 1
                    fontsKursiv(i) = New XFont(f(i).FontFamily.ToString(), f(i).Size, f(i).Style Or XFontStyle.Italic)
                Next
            End If

            Me.RenderFormat_Variabel = renderFormat
        End Sub
    End Class
#End Region
End Class
