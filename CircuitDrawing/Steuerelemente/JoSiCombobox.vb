Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Public Class JoSiCombobox
    Inherits ComboBox

    Private _endEllipses As Boolean = False

    Protected _various As Boolean
    Public Property Various As Boolean
        Get
            Return _various
        End Get
        Set(value As Boolean)
            _various = value
            Me.Invalidate()
        End Set
    End Property

    Public Property EndEllipses As Boolean
        Get
            Return _endEllipses
        End Get
        Set(value As Boolean)
            _endEllipses = value
            Me.Invalidate()
        End Set
    End Property
    Private _RenderColorSelected As Color = Color.FromArgb(0, 75, 150)

    Public Property RenderColorSelected As Color
        Get
            Return _RenderColorSelected
        End Get
        Set(value As Color)
            _RenderColorSelected = value
            Me.Invalidate()
        End Set
    End Property

    Private _RenderColor As Color = Color.Black

    Public Property renderColor As Color
        Get
            Return _RenderColor
        End Get
        Set(value As Color)
            _RenderColor = value
            Me.Invalidate()
        End Set
    End Property

    Protected foreColor_EnabledFalse As Color = Color.Gray

    Private _FärbenStärke As Integer = 0

    Public Property FärbenStärke As Integer
        Get
            Return _FärbenStärke
        End Get
        Set(value As Integer)
            _FärbenStärke = value
            Me.Invalidate()

        End Set
    End Property

    Private _FärbenStärke_Selected As Integer = 0

    Public Property FärbenStärkeSelected As Integer
        Get
            Return _FärbenStärke_Selected
        End Get
        Set(value As Integer)
            _FärbenStärke_Selected = value
            Me.Invalidate()
        End Set
    End Property


    Protected ComboboxDropDownColorBrush As Brush = New SolidBrush(Color.White)

    Protected selectionColorBrush As Brush = New SolidBrush(Color.FromArgb(51, 153, 255))

    Protected foreColorSelected As Color = Color.Black

    Protected Text_Render_Offset As Integer = 0

    Private Sub setDesignStyle()
        _RenderColor = Color.Black
        _RenderColorSelected = Color.Black
        _FärbenStärke = 0
        _FärbenStärke_Selected = 0

        selectionColorBrush = New SolidBrush(Color.FromArgb(51, 153, 255))
        ComboboxDropDownColorBrush = New SolidBrush(Color.White)
        foreColorSelected = Color.Black

        Me.BackColor = Color.White
        Me.ForeColor = Color.Black
        Me.foreColor_EnabledFalse = Color.FromArgb(255, 128, 128, 128)
        Me.Invalidate()
    End Sub

    Public Sub New()
        Me.DoubleBuffered = True
        MyBase.DrawMode = DrawMode.OwnerDrawFixed
        MyBase.SetStyle(ControlStyles.Opaque Or ControlStyles.UserPaint Or ControlStyles.OptimizedDoubleBuffer, True)
        MyBase.DropDownStyle = ComboBoxStyle.DropDownList
        setDesignStyle()

        _various = False
    End Sub

    Protected Overrides Sub OnDropDown(e As EventArgs)
        MyBase.OnDropDown(e)
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        'Die Hintergrundfarbe des Parentcontrols nehmen (für den bereich um die abgerundeten Ecken)
        If Me.Parent IsNot Nothing Then
            Dim parentBrush As New SolidBrush(Me.Parent.BackColor)
            e.Graphics.FillRectangle(parentBrush, New Rectangle(0, 0, Me.Width, Me.Height))
        End If

        Dim isHot As Boolean = True

        Dim buttonImage As New Bitmap(Me.Width + 2, Me.Height + 2, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(buttonImage)

            'Hintergrundbutton malen
            If Me.DroppedDown Then
                'der dropdown ist ausgefahren --> button als gedrückt malen
                ButtonRenderer.DrawButton(g, New Rectangle(0, 0, Me.Width + 2, Me.Height + 2), VisualStyles.PushButtonState.Pressed)
            Else
                Dim pos As Point = Me.PointToClient(Cursor.Position)
                If pos.X >= 0 And pos.Y >= 0 And pos.X < Me.Width And pos.Y < Me.Height Then
                    'button hervorheben, wenn die maus im bereich ist
                    ButtonRenderer.DrawButton(g, New Rectangle(0, 0, Me.Width + 2, Me.Height + 2), VisualStyles.PushButtonState.Hot)
                Else
                    'sonst ganz normal malen
                    ButtonRenderer.DrawButton(g, New Rectangle(0, 0, Me.Width + 2, Me.Height + 2), VisualStyles.PushButtonState.Normal)
                    isHot = False
                End If
            End If
        End Using
        Me.abdunkeln(buttonImage, isHot)

        e.Graphics.DrawImageUnscaled(buttonImage, New Point(-1, -1))

        'Beschriftung malen (mit dem richtigen Textrenderer)
        If Me.SelectedIndex <> -1 Then
            If isHot Then
                OnDrawItemSelectedForeground(New DrawItemEventArgs(e.Graphics, Me.Font, New Rectangle(1, 0, Me.Width - 21, Me.Height), SelectedIndex, DrawItemState.Focus), Items(SelectedIndex).ToString)
            Else
                OnDrawItemSelectedForeground(New DrawItemEventArgs(e.Graphics, Me.Font, New Rectangle(1, 0, Me.Width - 21, Me.Height), SelectedIndex, DrawItemState.Default), Items(SelectedIndex).ToString)
            End If
        End If

        'malen des Pfeiles
        Dim punkte(2) As Point
        punkte(0) = New Point(Me.Width - 12, Me.Height \ 2 - 1)
        punkte(1) = New Point(Me.Width - 5, Me.Height \ 2 - 1)
        punkte(2) = New Point(Me.Width - 9, Me.Height \ 2 + 3)
        e.Graphics.FillPolygon(Brushes.Black, punkte)

    End Sub

    Protected Sub abdunkeln(img As Bitmap, isHot As Boolean)
        Dim imgdata As BitmapData = img.LockBits(New Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)
        Dim bytesMax As Integer = imgdata.Stride * imgdata.Height - 1
        Dim byt(bytesMax) As Byte
        Marshal.Copy(imgdata.Scan0, byt, 0, bytesMax + 1)
        Dim r As Integer = _RenderColor.R
        Dim g As Integer = _RenderColor.G
        Dim b As Integer = _RenderColor.B
        Dim f As Integer = FärbenStärke
        If isHot Then
            f = _FärbenStärke_Selected
            r = _RenderColorSelected.R
            g = _RenderColorSelected.G
            b = _RenderColorSelected.B
        End If
        Dim rot, grün, blau As Byte
        Dim bytes(2) As Byte
        Dim fi As Integer = 100 - f
        For index As Integer = 0 To bytesMax Step 4
            If byt(index + 3) = 255 Then
                If _RenderColorSelected.B >= _RenderColorSelected.G AndAlso _RenderColorSelected.G >= _RenderColorSelected.R Then
                    byt(index) = CByte((f * b + fi * byt(index)) \ 100)
                    byt(index + 1) = CByte((f * g + fi * byt(index + 1)) \ 100)
                    byt(index + 2) = CByte((f * r + fi * byt(index + 2)) \ 100)
                Else
                    If _RenderColorSelected.B >= _RenderColorSelected.R AndAlso _RenderColorSelected.R >= _RenderColorSelected.G Then
                        rot = CByte((f * r + fi * byt(index + 1)) \ 100)
                        grün = CByte((f * g + fi * byt(index + 2)) \ 100)
                        blau = CByte((f * b + fi * byt(index)) \ 100)
                    ElseIf _RenderColorSelected.G >= _RenderColorSelected.B AndAlso _RenderColorSelected.B >= _RenderColorSelected.R Then
                        rot = CByte((f * r + fi * byt(index + 2)) \ 100)
                        grün = CByte((f * g + fi * byt(index)) \ 100)
                        blau = CByte((f * b + fi * byt(index + 1)) \ 100)
                    ElseIf _RenderColorSelected.G >= _RenderColorSelected.R AndAlso _RenderColorSelected.R >= _RenderColorSelected.B Then
                        rot = CByte((f * r + fi * byt(index + 1)) \ 100)
                        grün = CByte((f * g + fi * byt(index)) \ 100)
                        blau = CByte((f * b + fi * byt(index + 2)) \ 100)
                    ElseIf _RenderColorSelected.R >= _RenderColorSelected.G AndAlso _RenderColorSelected.G >= _RenderColorSelected.B Then
                        rot = CByte((f * r + fi * byt(index)) \ 100)
                        grün = CByte((f * g + fi * byt(index + 1)) \ 100)
                        blau = CByte((f * b + fi * byt(index + 2)) \ 100)
                    ElseIf _RenderColorSelected.R >= _RenderColorSelected.B AndAlso _RenderColorSelected.B >= _RenderColorSelected.G Then
                        rot = CByte((f * r + fi * byt(index)) \ 100)
                        grün = CByte((f * g + fi * byt(index + 2)) \ 100)
                        blau = CByte((f * b + fi * byt(index + 1)) \ 100)
                    Else
                        Throw New Exception("Das kann eigentlich nicht sein!")
                    End If
                    byt(index + 2) = rot
                    byt(index + 1) = grün
                    byt(index) = blau
                End If
                'If changeRedAndBlue Then
                '    rot = CByte((f * b + fi * byt(index)) \ 100)
                '    grün = CByte((f * g + fi * byt(index + 1)) \ 100)
                '    blau = CByte((f * r + fi * byt(index + 2)) \ 100)
                '    byt(index + 2) = rot
                '    byt(index + 1) = grün
                '    byt(index) = blau
                'Else
                '    byt(index) = CByte((f * b + fi * byt(index)) \ 100)
                '    byt(index + 1) = CByte((f * g + fi * byt(index + 1)) \ 100)
                '    byt(index + 2) = CByte((f * r + fi * byt(index + 2)) \ 100)
                'End If
            End If
        Next
        Marshal.Copy(byt, 0, imgdata.Scan0, bytesMax + 1)
        img.UnlockBits(imgdata)
    End Sub

    Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
        MyBase.OnSelectedIndexChanged(e)
        Me.Invalidate()
    End Sub

    Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
        MyBase.OnDrawItem(e)
        If (e.State And DrawItemState.ComboBoxEdit) <> DrawItemState.ComboBoxEdit Then
            'hier darf man nur die Items in dem Dropdown malen und nicht das Item oben in der Box
            If e.Index <> -1 Then
                e.DrawBackground()
                OnDrawItemDropDownForeground(e)
            End If
        End If
    End Sub

    Protected Overridable Sub OnDrawItemDropDownForeground(e As DrawItemEventArgs)
        Dim textColor As Color = e.ForeColor
        If (e.State And DrawItemState.Selected) <> 0 Then
            e.Graphics.FillRectangle(selectionColorBrush, e.Bounds)
            textColor = foreColorSelected
        End If
        TextRenderer.DrawText(e.Graphics, Items(e.Index).ToString, e.Font, New Rectangle(e.Bounds.X + Text_Render_Offset, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height), textColor, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
    End Sub

    Protected Overridable Sub OnDrawItemSelectedForeground(e As DrawItemEventArgs, text As String)
        Dim mytext As String
        If _various Then
            mytext = ElementEinstellung.VARIOUS_STRING
        Else
            mytext = text
        End If

        Dim extraformat As TextFormatFlags = 0

        If Me.EndEllipses Then
            extraformat = TextFormatFlags.EndEllipsis
        End If

        If Me.Enabled Then
            If (e.State And DrawItemState.Focus) = 0 Then
                TextRenderer.DrawText(e.Graphics, mytext, Me.Font, New Rectangle(1 + Text_Render_Offset, 0, Me.Width - 21, Me.Height), Me.ForeColor, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or extraformat)
            Else
                TextRenderer.DrawText(e.Graphics, mytext, Me.Font, New Rectangle(1 + Text_Render_Offset, 0, Me.Width - 21, Me.Height), Color.Black, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or extraformat)
            End If
        Else
            TextRenderer.DrawText(e.Graphics, mytext, Me.Font, New Rectangle(1 + Text_Render_Offset, 0, Me.Width - 21, Me.Height), foreColor_EnabledFalse, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix Or extraformat)
        End If
    End Sub
End Class
