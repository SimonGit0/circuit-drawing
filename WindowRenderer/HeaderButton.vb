
Public Class HeaderButton
    Inherits UserControl

    Protected lbl As LabelSpezial
    Protected btn As Button
    Protected Pic As PictureBox

    Public Property HeaderName As String
        Get
            Return lbl.Text
        End Get
        Set(ByVal value As String)
            lbl.Text = value
        End Set
    End Property
    Public Property headerIcon As Bitmap
        Get
            Return DirectCast(Pic.Image, Bitmap)
        End Get
        Set(ByVal value As Bitmap)
            Pic.Image = value
        End Set
    End Property
    Private _sichtbarkeit As Boolean
    Public Property Sichtbar As Boolean
        Get
            Return _sichtbarkeit
        End Get
        Set(ByVal value As Boolean)
            _sichtbarkeit = value
            If Not _sichtbarkeit Then
                lbl.Visible = False
                Pic.Visible = False
                btn.Visible = False
                Me.BackColor = style.ToolStripHeaderBackcolor
            Else
                lbl.Visible = True
                Pic.Visible = True
                btn.Visible = True
                DesignStyleChanged()
            End If
        End Set
    End Property

    Public contentName As String

    ''' <summary>
    ''' Setzt vorübergehend die Farbe als prozentanteil zwischen der SelectedColor und der 
    ''' Defaultcolor
    ''' </summary>
    ''' <param name="prozent"></param>
    ''' <remarks></remarks>
    Public Sub SetactivateProzent(prozent As Double)
        'für die hintergrundfarbe
        Dim r As Integer = CInt(prozent * Me.style.SelectedColor.R + (1 - prozent) * Me.style.DefaultColor.R)
        Dim g As Integer = CInt(prozent * Me.style.SelectedColor.G + (1 - prozent) * Me.style.DefaultColor.G)
        Dim b As Integer = CInt(prozent * Me.style.SelectedColor.B + (1 - prozent) * Me.style.DefaultColor.B)
        Me.BackColor = Color.FromArgb(r, g, b)
        lbl.BackColor = Color.FromArgb(r, g, b)

        'für das schließenkreuz
        r = CInt(prozent * Me.style.ButtonBackgroundcolorSelected.R + (1 - prozent) * Me.style.ButtonBackgroundcolorDefault.R)
        g = CInt(prozent * Me.style.ButtonBackgroundcolorSelected.G + (1 - prozent) * Me.style.ButtonBackgroundcolorDefault.G)
        b = CInt(prozent * Me.style.ButtonBackgroundcolorSelected.B + (1 - prozent) * Me.style.ButtonBackgroundcolorDefault.B)
        btn.BackColor = Color.FromArgb(r, g, b)

        'für das schließenkreuz_Rahmen
        r = CInt(prozent * Me.style.ButtonBorderColorSelected.R + (1 - prozent) * Me.style.ButtonBorderColor.R)
        g = CInt(prozent * Me.style.ButtonBorderColorSelected.G + (1 - prozent) * Me.style.ButtonBorderColor.G)
        b = CInt(prozent * Me.style.ButtonBorderColorSelected.B + (1 - prozent) * Me.style.ButtonBorderColor.B)
        btn.FlatAppearance.BorderColor = Color.FromArgb(r, g, b)

        'für die Textfarbe
        r = CInt(prozent * Me.style.FontColorSelected.R + (1 - prozent) * Me.style.FontColorDefault.R)
        g = CInt(prozent * Me.style.FontColorSelected.G + (1 - prozent) * Me.style.FontColorDefault.G)
        b = CInt(prozent * Me.style.FontColorSelected.B + (1 - prozent) * Me.style.FontColorDefault.B)
        lbl.ForeColor = Color.FromArgb(r, g, b)

    End Sub

    Public Sub New(style As DesignStyle)
        'Me.BorderStyle = Windows.Forms.BorderStyle.FixedSingle
        Me.style = style

        lbl = New LabelSpezial()
        lbl.Text = "Header"
        lbl.Parent = Me
        lbl.Margin = New Padding(0)
        lbl.Padding = New Padding(0)
        btn = New Button
        btn.Size = New Size(16, 16)
        btn.Text = "X"
        btn.Font = New Font("Microsoft sans serif", 6)
        btn.Parent = Me
        Pic = New PictureBox
        Pic.Image = SystemIcons.Application.ToBitmap
        Pic.Parent = Me
        Pic.Size = New Size(16, 16)
        Me.Size = New Size(150, 20)
        Pic.Location = New Point(1, 2)
        btn.Location = New Point(132, 2)
        btn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btn.FlatStyle = FlatStyle.Flat
        btn.ForeColor = Color.Red
        btn.BackColor = style.ButtonBackgroundcolorDefault
        btn.FlatAppearance.BorderColor = style.ButtonBorderColor
        btn.FlatAppearance.BorderSize = style.ButtonBorderWidth
        btn.FlatAppearance.MouseOverBackColor = style.ButtonBackgroundcolorMouseOver
        btn.FlatAppearance.MouseDownBackColor = style.ButtonBackgroundcolorMouseDown
        Pic.Anchor = AnchorStyles.Top Or AnchorStyles.Left
        btn.TextAlign = ContentAlignment.MiddleCenter
        Pic.SizeMode = PictureBoxSizeMode.StretchImage
        lbl.Location = New Point(18, 3)
        lbl.AutoSize = False
        lbl.Size = New Size(114, 13)
        'lbl.AutoEllipsis = True
        lbl.TextAlign = ContentAlignment.MiddleLeft
        lbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right Or AnchorStyles.Bottom
        lbl.Font = New Font("Microsoft Sans Serif", 8)
        Me.BackColor = style.DefaultColor
        lbl.ForeColor = style.FontColorDefault

        AddHandler btn.MouseEnter, AddressOf SchließenMausEnter
        AddHandler btn.MouseLeave, AddressOf schließenMausleave
        AddHandler btn.GotFocus, AddressOf SchließenBtnGotFocus
        AddHandler lbl.MouseDown, AddressOf headerMouseDown
        AddHandler Me.MouseDown, AddressOf headerMouseDown
        AddHandler Pic.MouseDown, AddressOf headerMouseDown
        AddHandler btn.Click, AddressOf SchließenButtonClick
        AddHandler MyBase.MouseMove, AddressOf OnMouseMoving
        AddHandler lbl.MouseMove, AddressOf OnMouseMoving
        AddHandler Pic.MouseMove, AddressOf OnMouseMoving
        AddHandler MyBase.MouseUp, AddressOf MouseUp1
        AddHandler lbl.MouseUp, AddressOf MouseUp1
        AddHandler Pic.MouseUp, AddressOf MouseUp1

        AddHandler Pic.MouseLeave, AddressOf _mouseLeaveAll
        AddHandler lbl.MouseLeave, AddressOf _mouseLeaveAll
        AddHandler btn.MouseLeave, AddressOf _mouseLeaveAll
        AddHandler Me.MouseLeave, AddressOf _mouseLeaveAll

        _sichtbarkeit = True
    End Sub

    Protected style As DesignStyle

    Public Overridable Sub DesignStyleChanged()
        If Not _sichtbarkeit Then
            Me.BackColor = style.ToolStripHeaderBackcolor
        Else
            If Me.Selected Then
                Me.BackColor = Me.style.SelectedColor
                lbl.BackColor = Me.style.SelectedColor
                btn.BackColor = Me.style.ButtonBackgroundcolorSelected
                lbl.ForeColor = Me.style.FontColorSelected
                btn.FlatAppearance.BorderColor = style.ButtonBorderColorSelected
            Else
                Me.BackColor = Me.style.DefaultColor
                lbl.BackColor = Me.style.DefaultColor
                btn.BackColor = Me.style.ButtonBackgroundcolorDefault
                lbl.ForeColor = Me.style.FontColorDefault
                btn.FlatAppearance.BorderColor = style.ButtonBorderColor
            End If
        End If
        btn.FlatAppearance.BorderSize = style.ButtonBorderWidth
        btn.FlatAppearance.MouseOverBackColor = style.ButtonBackgroundcolorMouseOver
        btn.FlatAppearance.MouseDownBackColor = style.ButtonBackgroundcolorMouseDown
    End Sub

    Private _s As Boolean = False
    Public Property Selected As Boolean
        Get
            Return _s
        End Get
        Set(ByVal value As Boolean)
            _s = value
            If _s Then
                Me.BackColor = style.SelectedColor
                btn.BackColor = style.ButtonBackgroundcolorSelected
                lbl.BackColor = style.SelectedColor
                lbl.ForeColor = style.FontColorSelected
                btn.FlatAppearance.BorderColor = style.ButtonBorderColorSelected
            Else
                Me.BackColor = style.DefaultColor
                btn.BackColor = style.ButtonBackgroundcolorDefault
                lbl.BackColor = style.DefaultColor
                lbl.ForeColor = style.FontColorDefault
                btn.FlatAppearance.BorderColor = style.ButtonBorderColor
            End If
            Me.Invalidate()
        End Set
    End Property

    Public Sub SetSelectedOhneFarben(selection As Boolean)
        _s = selection
    End Sub

    Private Sub SchließenBtnGotFocus(ByVal sender As Object, ByVal e As EventArgs)
        lbl.Focus()
    End Sub
    Private Sub SchließenMausEnter(sender As Object, e As EventArgs)
        btn.ForeColor = Color.White
    End Sub
    Private Sub schließenMausleave(sender As Object, e As EventArgs)
        btn.ForeColor = Color.Red()
    End Sub
    Private Sub SchließenButtonClick(ByVal sender As Object, ByVal e As EventArgs)
        RaiseEvent Schließen(Me, EventArgs.Empty)
    End Sub
    Private Sub headerMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not Me.Selected Then
            'Me.Selected = True
            RaiseEvent GotSelection(Me, EventArgs.Empty)
        End If
        Dim p As Point = Me.PointToClient(Cursor.Position)

        If e.Button = MouseButtons.Left Then
            RaiseEvent DragDropStart(Me, New MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta))
        End If
    End Sub

    Private Sub OnMouseMoving(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim p As Point = Me.PointToClient(Cursor.Position)
        RaiseEvent MouseMove(Me, New MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta))
    End Sub
    Private Sub MouseUp1(ByVal sender As Object, ByVal e As MouseEventArgs)
        Dim p As Point = Me.PointToClient(Cursor.Position)
        RaiseEvent MouseUp(Me, New MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta))
    End Sub

    Private Sub _mouseLeaveAll(ByVal sender As Object, ByVal e As EventArgs)
        Dim p As Point = Me.PointToClient(Cursor.Position)
        If p.X < 0 Or p.Y < 0 Or p.X >= Me.Width Or p.Y >= Me.Height Then
            RaiseEvent MouseLeaveMe(Me, EventArgs.Empty)
        End If
    End Sub

    Public Event GotSelection(ByVal sender As Object, ByVal e As EventArgs)
    Public Event Schließen(ByVal sender As Object, ByVal e As EventArgs)
    Public Event DragDropStart(ByVal sender As Object, ByVal e As MouseEventArgs)
    Public Shadows Event MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
    Public Shadows Event MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
    Public Event MouseLeaveMe(ByVal sender As Object, ByVal e As EventArgs)
End Class
