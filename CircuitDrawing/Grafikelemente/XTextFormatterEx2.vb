' Authors:
'   Stefan Lange
'   Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
' Modifications by:
'   Thomas Hövel
'   Copyright (c) 2015 TH Software, Troisdorf (Germany), http://developer.th-soft.com/developer/
'
' http://www.pdfsharp.com
' http://sourceforge.net/projects/pdfsharp
'
' Permission Is hereby granted, free of charge, to any person obtaining a
' copy of this software And associated documentation files (the "Software"),
' to deal in the Software without restriction, including without limitation
' the rights to use, copy, modify, merge, publish, distribute, sublicense,
' And/Or sell copies of the Software, And to permit persons to whom the
' Software Is furnished to do so, subject to the following conditions:
'
' The above copyright notice And this permission notice shall be included
' in all copies Or substantial portions of the Software.
'
' THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS Or
' IMPLIED, INCLUDING BUT Not LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS Or COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES Or OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT Or OTHERWISE, ARISING
' FROM, OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER 
' DEALINGS IN THE SOFTWARE.

Imports PdfSharp.Drawing
Imports PdfSharp.Drawing.Layout
''' <summary>
''' Represents a very simple text formatter.
''' If this class does Not satisfy your needs on formatting paragraphs I recommend to take a look
''' at MigraDoc Foundation. Alternatively you should copy this class in your own source code And modify it.
''' </summary>
Public Class XTextFormatterEx2

    Public Enum SpacingMode
        ''' <summary>
        ''' With Relative, the value of Spacing will be added to the default line space.
        ''' With 0 you get the default behaviour.
        ''' With 5 the line spacing will be 5 points larger than the default spacing.
        ''' </summary>
        Relative
        ''' <summary>
        ''' With Absolute you set the absolute line spacing.
        ''' With 0 all the text will be written at the same line.
        ''' </summary>
        Absolute
        ''' <summary>
        ''' With Percentage, you can specify larger Or smaller line spacing.
        ''' With 100 you get the default behaviour.
        ''' With 200 you get double line spacing.
        ''' With 90 you get 90% of the default line spacing.
        ''' </summary>
        Percentage
    End Enum

    Public Structure LayoutOptions
        Public Spacing As Single
        Public SpacingMode As SpacingMode
        Public Sub New(mode As SpacingMode, spacing As Single)
            Me.Spacing = spacing
            Me.SpacingMode = mode
        End Sub
    End Structure

    ''' <summary>
    ''' Initializes a New instance of the <see cref="XTextFormatter"/> class.
    ''' </summary>
    Public Sub New(gfx As XGraphics)
        Me.New(gfx, New LayoutOptions(SpacingMode.Relative, 0))
    End Sub

    ''' <summary>
    ''' Initializes a New instance of the <see cref="XTextFormatter"/> class.
    ''' </summary>
    Public Sub New(gfx As XGraphics, options As LayoutOptions)
        If gfx Is Nothing Then
            Throw New ArgumentNullException("gfx")
        End If
        _gfx = gfx
        _layoutOptions = options
    End Sub

    Private ReadOnly _gfx As XGraphics
    Private ReadOnly _layoutOptions As LayoutOptions

    ''' <summary>
    ''' Gets Or sets the text.
    ''' </summary>
    ''' <value>The text.</value>
    Public Property Text As String
        Get
            Return _text
        End Get
        Set(value As String)
            _text = value
        End Set
    End Property
    Private _text As String

    ''' <summary>
    ''' Gets Or sets the font.
    ''' </summary>
    Public Property Font As XFont
        Get
            Return _font
        End Get
        Set(value As XFont)
            If value Is Nothing Then
                Throw New ArgumentNullException("Font")
            End If
            _font = value

            _lineSpace = _font.GetHeight()
            _cyAscent = _lineSpace * _font.CellAscent / _font.CellSpace
            _cyDescent = _lineSpace * _font.CellDescent / _font.CellSpace

            'HACK in XTextFormatter
            _spaceWidth = _gfx.MeasureString("x x", value).Width
            _spaceWidth -= _gfx.MeasureString("xx", value).Width

            CalculateLineSpace()
        End Set
    End Property
    Private _font As XFont

    Private _lineSpace As Double
    Private _effectiveLineSpace As Double
    Private _cyAscent As Double
    Private _cyDescent As Double
    Private _spaceWidth As Double

    Private _preparedText As Boolean

    Private Function GetLineSpace() As Double
        Return _effectiveLineSpace
    End Function

    Private Sub CalculateLineSpace()
        Select Case (_layoutOptions.SpacingMode)
            Case SpacingMode.Absolute
                _effectiveLineSpace = _layoutOptions.Spacing
            Case SpacingMode.Relative
                _effectiveLineSpace = _lineSpace + _layoutOptions.Spacing
            Case SpacingMode.Percentage
                _effectiveLineSpace = _lineSpace * _layoutOptions.Spacing / 100
        End Select
    End Sub

    ''' <summary>
    ''' Gets Or sets the bounding box of the layout.
    ''' </summary>
    Public Property LayoutRectangle As XRect
        Get
            Return _layoutRectangle
        End Get
        Set(value As XRect)
            _layoutRectangle = value
        End Set
    End Property
    Private _layoutRectangle As XRect

    ''' <summary>
    ''' Gets Or sets the alignment of the text.
    ''' </summary>
    Public Property Alignment As XParagraphAlignment
        Get
            Return _alignment
        End Get
        Set(value As XParagraphAlignment)
            _alignment = value
        End Set
    End Property
    Private _alignment As XParagraphAlignment = XParagraphAlignment.Left

    ''' <summary>
    ''' Prepares a given text for drawing, performs the layout, returns the index of the last fitting char And the needed height.
    ''' </summary>
    ''' <param name="text">The text to be drawn.</param>
    ''' <param name="font">The font to be used.</param>
    ''' <param name="layoutRectangle">The layout rectangle. Set the correct width.
    ''' Either set the available height to find how many chars will fit.
    ''' Or set height to double.MaxValue to find which height will be needed to draw the whole text.</param>
    ''' <param name="lastFittingChar">Index of the last fitting character. Can be -1 if the character was Not determined. Will be -1 if the whole text can be drawn.</param>
    ''' <param name="neededHeight">The needed height - either for the complete text Or the used height of the given rect.</param>
    ''' <exception cref="ArgumentNullException"></exception>
    Public Sub PrepareDrawString(text As String, font As XFont, layoutRectangle As XRect, ByRef lastFittingChar As Integer, ByRef neededHeight As Double)
        If text Is Nothing Then Throw New ArgumentNullException("text")
        If font Is Nothing Then Throw New ArgumentNullException("font")

        Me.Text = text
        Me.Font = font
        Me.LayoutRectangle = layoutRectangle

        lastFittingChar = -1
        neededHeight = Double.MinValue

        If text.Length = 0 Then
            Return
        End If

        CreateBlocks()

        CreateLayout()

        _preparedText = True

        Dim dy As Double = _cyDescent + _cyAscent
        Dim count As Integer = _blocks.Count
        For idx As Integer = 0 To count - 1
            Dim block As Block = _blocks(idx)
            If (block._Stop) Then
                'We have a Stop block, so only part of the text will fit. We return the index of the last fitting char (And the height of the block, if available).
                lastFittingChar = 0
                Dim idx2 As Integer = idx - 1
                While (idx2 >= 0)
                    Dim block2 As Block = _blocks(idx2)
                    If (block2.EndIndex >= 0) Then
                        lastFittingChar = block2.EndIndex
                        neededHeight = dy + block2.Location.Y 'Test this!!!!!
                        Return
                    End If
                    idx2 -= 1
                End While
                Return
            End If
            If block.Type = BlockType.LineBreak Then
                Continue For
            End If
            'gfx.DrawString(block.Text, font, brush, dx + block.Location.x, dy + block.Location.y);
            neededHeight = dy + block.Location.Y 'Test this!!!!! Performance optimization?
        Next
    End Sub


    ''' <summary>
    ''' Draws the text that was previously prepared by calling PrepareDrawString Or by passing a text to DrawString.
    ''' </summary>
    ''' <param name="brush">The brush used for drawing the text.</param>
    Public Sub DrawString(brush As XBrush)
        DrawString(brush, XStringFormats.TopLeft)
    End Sub


    ''' <summary>
    ''' Draws the text that was previously prepared by calling PrepareDrawString Or by passing a text to DrawString.
    ''' </summary>
    ''' <param name="brush">The brush used for drawing the text.</param>
    ''' <param name="format">Not yet implemented.</param>
    ''' <exception cref="ArgumentException"></exception>
    ''' <exception cref="ArgumentNullException"></exception>
    Public Sub DrawString(brush As XBrush, format As XStringFormat)
        ' TODO Do we need "XStringFormat format" at PrepareDrawString Or at DrawString? Not yet used anyway, but probably already needed at PrepareDrawString.
        If Not _preparedText Then Throw New ArgumentException("PrepareDrawString must be called first.")
        If brush Is Nothing Then Throw New ArgumentNullException("brush")
        If format.Alignment <> XStringAlignment.Near OrElse format.LineAlignment <> XLineAlignment.Near Then
            Throw New ArgumentException("Only TopLeft alignment is currently implemented.")
        End If

        If _text.Length = 0 Then
            Return
        End If

        Dim dx As Double = _layoutRectangle.Location.X
        Dim dy As Double = _layoutRectangle.Location.Y + _cyAscent
        Dim count As Integer = _blocks.Count
        For idx As Integer = 0 To count - 1
            Dim block As Block = _blocks(idx)
            If block._Stop Then Exit For
            If block.Type = BlockType.LineBreak Then Continue For
            _gfx.DrawString(block.Text, _font, brush, dx + block.Location.X, dy + block.Location.Y)
        Next
    End Sub

    ''' <summary>
    ''' Draws the text.
    ''' </summary>
    ''' <param name="text">The text to be drawn.</param>
    ''' <param name="font">The font.</param>
    ''' <param name="brush">The text brush.</param>
    ''' <param name="layoutRectangle">The layout rectangle.</param>
    Public Sub DrawString(text As String, font As XFont, brush As XBrush, layoutRectangle As XRect)
        DrawString(text, font, brush, layoutRectangle, XStringFormats.TopLeft)
    End Sub

    ''' <summary>
    ''' Draws the text.
    ''' </summary>
    ''' <param name="text">The text to be drawn.</param>
    ''' <param name="font">The font.</param>
    ''' <param name="brush">The text brush.</param>
    ''' <param name="layoutRectangle">The layout rectangle.</param>
    ''' <param name="format">The format. Must be <c>XStringFormat.TopLeft</c></param>
    Public Sub DrawString(text As String, font As XFont, brush As XBrush, layoutRectangle As XRect, format As XStringFormat)
        Dim dummy1 As Integer
        Dim dummy2 As Double
        PrepareDrawString(text, font, layoutRectangle, dummy1, dummy2)

        Dim offset As Double = 0
        If format.LineAlignment = XLineAlignment.Center Then
            offset = (layoutRectangle.Height - dummy2) / 2
        ElseIf format.LineAlignment = XLineAlignment.Far Then
            offset = layoutRectangle.Height - dummy2
        End If
        _layoutRectangle.Y += offset

        DrawString(brush)
    End Sub

    Private Sub CreateBlocks()
        _blocks.Clear()
        Dim length As Integer = _text.Length
        Dim inNonWhiteSpace As Boolean = False
        Dim startIndex As Integer = 0
        Dim blockLength As Integer = 0
        For idx As Integer = 0 To length - 1
            Dim ch As Char = _text(idx)
            ' Treat CR And CRLF as LF
            If ch = ControlChars.Cr Then
                If idx < length - 1 AndAlso _text(idx + 1) = ControlChars.Lf Then
                    idx += 1
                End If
                ch = ControlChars.Lf
            End If
            If ch = ControlChars.Lf Then
                If blockLength <> 0 Then
                    Dim token As String = _text.Substring(startIndex, blockLength)
                    _blocks.Add(New Block(token, BlockType.Text, _gfx.MeasureString(token, _font).Width, startIndex, startIndex + blockLength - 1))
                End If
                startIndex = idx + 1
                blockLength = 0
                _blocks.Add(New Block(BlockType.LineBreak))
            ElseIf Char.IsWhiteSpace(ch) Then
                If (inNonWhiteSpace) Then
                    Dim token As String = _text.Substring(startIndex, blockLength)
                    _blocks.Add(New Block(token, BlockType.Text, _gfx.MeasureString(token, _font).Width, startIndex, startIndex + blockLength - 1))
                    startIndex = idx + 1
                    blockLength = 0
                Else
                    blockLength += 1
                End If
            Else
                inNonWhiteSpace = True
                blockLength += 1
            End If
        Next
        If blockLength <> 0 Then
            Dim token As String = _text.Substring(startIndex, blockLength)
            _blocks.Add(New Block(token, BlockType.Text, _gfx.MeasureString(token, _font).Width, startIndex, startIndex + blockLength - 1))
        End If
    End Sub

    Private Sub CreateLayout()
        Dim rectWidth As Double = _layoutRectangle.Width
        Dim rectHeight As Double = _layoutRectangle.Height - _cyAscent - _cyDescent

        Dim firstIndex As Integer = 0
        Dim x As Double = 0
        Dim y As Double = 0
        Dim count As Integer = _blocks.Count
        For idx As Integer = 0 To count - 1
            Dim block As Block = _blocks(idx)
            If block.Type = BlockType.LineBreak Then
                If Alignment = XParagraphAlignment.Justify Then
                    _blocks(firstIndex).Alignment = XParagraphAlignment.Left
                End If
                AlignLine(firstIndex, idx - 1, rectWidth)
                firstIndex = idx + 1
                x = 0
                y += GetLineSpace()
                If y > rectHeight Then
                    block._Stop = True
                    Exit For
                End If
            Else
                Dim width As Double = block.Width '!!!modTHHO 19.11.09 don't add this.spaceWidth here
                If (x + width <= rectWidth OrElse x = 0) AndAlso block.Type <> BlockType.LineBreak Then
                    block.Location = New XPoint(x, y)
                    x += width + _spaceWidth '!!!modTHHO 19.11.09 add this.spaceWidth here
                Else
                    AlignLine(firstIndex, idx - 1, rectWidth)
                    firstIndex = idx
                    y += GetLineSpace()
                    If (y > rectHeight) Then
                        block._Stop = True
                        Exit For
                    End If
                    block.Location = New XPoint(0, y)
                    x = width + _spaceWidth '!!!modTHHO 19.11.09 add this.spaceWidth here
                End If
            End If
        Next
        If firstIndex < count AndAlso Alignment <> XParagraphAlignment.Justify Then
            AlignLine(firstIndex, count - 1, rectWidth)
        End If
    End Sub

    ''' <summary>
    ''' Align center, right Or justify.
    ''' </summary>
    Private Sub AlignLine(firstIndex As Integer, lastIndex As Integer, layoutWidth As Double)
        Dim blockAlignment As XParagraphAlignment = _blocks(firstIndex).Alignment
        If _alignment = XParagraphAlignment.Left OrElse blockAlignment = XParagraphAlignment.Left Then
            Return
        End If

        Dim count As Integer = lastIndex - firstIndex + 1
        If count = 0 Then
            Return
        End If

        Dim totalWidth As Double = -_spaceWidth
        For idx As Integer = firstIndex To lastIndex
            totalWidth += _blocks(idx).Width + _spaceWidth
        Next
        Dim dx As Double = Math.Max(layoutWidth - totalWidth, 0)
        'Debug.Assert(dx >= 0);
        If _alignment <> XParagraphAlignment.Justify Then
            If _alignment = XParagraphAlignment.Center Then
                dx /= 2
            End If
            For idx As Integer = firstIndex To lastIndex
                Dim Block As Block = _blocks(idx)
                Block.Location += New XSize(dx, 0)
            Next
        ElseIf (count > 1) Then 'case: justify
            dx = dx / (count - 1)
            Dim i As Integer = 1
            For idx As Integer = firstIndex + 1 To lastIndex
                Dim Block As Block = _blocks(idx)
                Block.Location += New XSize(dx * i, 0)
                i += 1
            Next
        End If
    End Sub

    ReadOnly _blocks As List(Of Block) = New List(Of Block)()

    Public Enum BlockType
        Text
        Space
        Hyphen
        LineBreak
    End Enum

    ''' <summary>
    ''' Represents a single word.
    ''' </summary>
    Class Block
        ''' <summary>
        ''' Initializes a New instance of the <see cref="Block"/> class.
        ''' </summary>
        ''' <param name="text">The text of the block.</param>
        ''' <param name="type">The type of the block.</param>
        ''' <param name="width">The width of the text.</param>
        ''' <param name="startIndex"></param>
        ''' <param name="endIndex"></param>
        Public Sub New(text As String, type As BlockType, width As Double, startIndex As Integer, endIndex As Integer)
            Me.Text = text
            Me.Type = type
            Me.Width = width
            Me.StartIndex = startIndex
            Me.EndIndex = endIndex
        End Sub

        ''' <summary>
        ''' Initializes a New instance of the <see cref="Block"/> class.
        ''' </summary>
        ''' <param name="type">The type.</param>
        Public Sub New(type As BlockType)
            Me.Type = type
        End Sub

        ''' <summary>
        ''' The text represented by this block.
        ''' </summary>
        Public ReadOnly Text As String

        Public ReadOnly StartIndex As Integer = -1
        Public ReadOnly EndIndex As Integer = -1

        ''' <summary>
        ''' The type of the block.
        ''' </summary>
        Public ReadOnly Type As BlockType

        ''' <summary>
        ''' The width of the text.
        ''' </summary>
        Public ReadOnly Width As Double

        ''' <summary>
        ''' The location relative to the upper left corner of the layout rectangle.
        ''' </summary>
        Public Location As XPoint

        ''' <summary>
        ''' The alignment of this line.
        ''' </summary>
        Public Alignment As XParagraphAlignment

        ''' <summary>
        ''' A flag indicating that this Is the last block that fits in the layout rectangle.
        ''' </summary>
        Public _Stop As Boolean
    End Class
End Class