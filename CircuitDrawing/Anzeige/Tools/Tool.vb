Public MustInherit Class Tool

    Public Property KannAbbrechen As Boolean
    Public Sub New()
        KannAbbrechen = True
    End Sub

    Public Overridable Sub meldeAn(sender As Vektor_Picturebox)
        sender.setToolHilfeText(Me, getDefaultHilfeText())
    End Sub

    Public Overridable Sub pause(sender As Vektor_Picturebox)
    End Sub

    Public Overridable Sub weiter(sender As Vektor_Picturebox)
        sender.setToolHilfeText(Me, getDefaultHilfeText())
    End Sub

    Public Overridable Function abortAction(sender As Vektor_Picturebox) As Boolean
        Return False
    End Function

    Public Overridable Sub meldeAb(sender As Vektor_Picturebox)
    End Sub

    Public Overridable Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
    End Sub

    Public Overridable Sub KeyUp(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
    End Sub

    Public Overridable Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
    End Sub

    Public Overridable Sub MouseMoveOffgrid(sender As Vektor_Picturebox, e As ToolMouseMoveOffgridEventArgs)
    End Sub

    Public Overridable Sub MouseDown(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
    End Sub

    Public Overridable Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
    End Sub

    Public Overridable Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
    End Sub

    Public Overridable Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
    End Sub

    Public Overridable Sub OnSelectedBauteilTemplateChanged(sender As Vektor_Picturebox, e As EventArgs)
    End Sub

    Public Overridable Sub OnGravityChanged(sender As Vektor_Picturebox, e As EventArgs)
    End Sub

    Public Overridable Sub OnMultiSelectChanged(sender As Vektor_Picturebox, e As EventArgs)
    End Sub

    Public MustOverride Function getDefaultHilfeText() As String

End Class

Public Class ToolKeyEventArgs
    Inherits EventArgs

    Public Property Handled As Boolean
    Public Property KeyCode As Keys
    Public Sub New(KeyCode As Keys)
        Handled = False
        Me.KeyCode = KeyCode
    End Sub

End Class

Public Class ToolMouseEventArgs
    Inherits EventArgs

    Public Property CursorPos As Point
    Public Property Button As MouseButtons

    Public Sub New(CursorPos As Point)
        Me.New(CursorPos, MouseButtons.None)
    End Sub

    Public Sub New(CursorPos As Point, Button As MouseButtons)
        Me.CursorPos = CursorPos
        Me.Button = Button
    End Sub

End Class

Public Class ToolMouseMoveOffgridEventArgs
    Inherits ToolMouseEventArgs

    Public Property isOnGridMove As Boolean

    Public Sub New(cursorPos As Point, isOngrid As Boolean)
        MyBase.New(cursorPos)
        Me.isOnGridMove = isOngrid
    End Sub

End Class

Public Class PaintCursorEventArgs
    Inherits PaintEventArgs

    Public CursorPosScreen As PointF
    Public CursorWidth As Single
    Public CursorHeight As Single

    Public CursorPen As Pen

    Public _Default_CursorExtensionTopLeft As PointF
    Public _Default_CursorExtensionSize As SizeF

    Public Sub New(Graphics As Graphics, ClipRect As Rectangle, cursorPos As PointF, cursorW As Single, cursorH As Single, p As Pen)
        MyBase.New(Graphics, ClipRect)

        Me.CursorPosScreen = cursorPos
        Me.CursorWidth = cursorW
        Me.CursorHeight = cursorH
        Me.CursorPen = p

        _Default_CursorExtensionTopLeft = New PointF(CursorPosScreen.X + cursorW * 0.75F, CursorPosScreen.Y - cursorH * 1.0F)
        _Default_CursorExtensionSize = New SizeF(1.2F * CursorWidth, 1.2F * CursorHeight)
    End Sub

End Class

Public Class ToolPaintEventArgs
    Inherits PaintEventArgs

    Public args_Elemente As GrafikDrawArgs
    Public args_Selection As GrafikDrawArgs

    Public Sub New(e As PaintEventArgs, args_Elemente As GrafikDrawArgs, args_Selection As GrafikDrawArgs)
        MyBase.New(e.Graphics, e.ClipRectangle)
        Me.args_Elemente = args_Elemente
        Me.args_Selection = args_Selection
    End Sub

End Class