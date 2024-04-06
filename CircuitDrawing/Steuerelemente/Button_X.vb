Public Class Button_X
    Inherits Control

    Private Shared myImg As Bitmap
    Private Shared myImgHover As Bitmap
    Private Shared myImgDown As Bitmap

    Private mOver As Boolean = False
    Private mDown As Boolean = False
    Private mDown_Over As Boolean = False

    Public Sub New()
        If myImg Is Nothing Then
            myImg = My.Resources.X
            myImgDown = My.Resources.X_focus
            myImgHover = My.Resources.X_hover
        End If
        Me.BackgroundImage = myImg
        Me.Size = New Size(16, 16)
    End Sub

    Private Sub updateImg()
        If mDown Then
            If mDown_Over Then
                Me.BackgroundImage = myImgDown
            Else
                Me.BackgroundImage = myImgHover
            End If
        Else
            If mOver Then
                Me.BackgroundImage = myImgHover
            Else
                Me.BackgroundImage = myImg
            End If
        End If
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)
        mOver = True
        updateImg()
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)
        mOver = False
        updateImg()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left Then
            mDown = True
            mDown_Over = True
            updateImg()
        End If
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If mDown Then
            Dim mo As Boolean
            If e.X < 0 OrElse e.Y < 0 OrElse e.X > Me.Width OrElse e.Y > Me.Height Then
                mo = False
            Else
                mo = True
            End If
            If mo <> mDown_Over Then
                mDown_Over = mo
                updateImg()
            End If
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        If e.Button = MouseButtons.Left Then
            mDown = False
            updateImg()
        End If
    End Sub

End Class
