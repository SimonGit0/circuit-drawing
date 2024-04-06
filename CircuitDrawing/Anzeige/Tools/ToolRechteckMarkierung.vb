Public Class ToolRechteckMarkierung
    Inherits Tool

    Private istDown As Boolean = False
    Private startPos As Point
    Private endPos As Point

    Private myPen As Pen = Pens.Orange
    Protected previewMarkierung As ToolHelper_PreviewMarkierung

    Public Sub New()
        previewMarkierung = New ToolHelper_PreviewMarkierung()
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
    End Sub

    Public Overrides Sub MouseDown(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            istDown = True
            startPos = e.CursorPos
            endPos = e.CursorPos
        End If
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If istDown Then
            endPos = e.CursorPos
            sender.Invalidate()
            If endPos = startPos Then
                previewMarkierung.PreviewSelect(sender, e.CursorPos)
            Else
                previewMarkierung.HighlightLöschen(sender)
            End If
        Else
            previewMarkierung.PreviewSelect(sender, e.CursorPos)
        End If
    End Sub

    Public Overrides Sub OnMultiSelectChanged(sender As Vektor_Picturebox, e As EventArgs)
        If istDown Then
            If endPos = startPos Then
                previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
            Else
                previewMarkierung.HighlightLöschen(sender)
            End If
        Else
            previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left AndAlso istDown Then
            istDown = False
            Dim mode As Vektor_Picturebox.SelectionMode = sender.getCurrentSelectionMode()

            If endPos.X = startPos.X AndAlso endPos.Y = startPos.Y Then
                'select only current pos
                sender.select_Element_At(startPos, mode)
            Else
                'Select complete rectangle
                sender.select_All_in_Rect(Mathe.getRect(startPos, endPos), mode)
                sender.Invalidate()
            End If
            previewMarkierung.PreviewSelect(sender, e.CursorPos)
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If e.KeyCode = Keys.Control OrElse e.KeyCode = Keys.ControlKey OrElse e.KeyCode = Keys.LControlKey OrElse e.KeyCode = Keys.RControlKey OrElse e.KeyCode = Keys.Shift OrElse e.KeyCode = Keys.LShiftKey OrElse e.KeyCode = Keys.RShiftKey OrElse e.KeyCode = Keys.ShiftKey Then
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub KeyUp(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If e.KeyCode = Keys.Control OrElse e.KeyCode = Keys.ControlKey OrElse e.KeyCode = Keys.LControlKey OrElse e.KeyCode = Keys.RControlKey OrElse e.KeyCode = Keys.Shift OrElse e.KeyCode = Keys.LShiftKey OrElse e.KeyCode = Keys.RShiftKey OrElse e.KeyCode = Keys.ShiftKey Then
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If istDown AndAlso (startPos.X <> endPos.X OrElse startPos.Y <> endPos.Y) Then
            Dim start As PointF = sender.toPictureboxPoint(startPos)
            Dim ende As PointF = sender.toPictureboxPoint(endPos)
            If startPos.X = endPos.X OrElse startPos.Y = endPos.Y Then
                e.Graphics.DrawLine(myPen, start.X, start.Y, ende.X, ende.Y)
            Else
                Dim minX As Single = Math.Min(start.X, ende.X)
                Dim minY As Single = Math.Min(start.Y, ende.Y)
                Dim maxX As Single = Math.Max(start.X, ende.X)
                Dim maxY As Single = Math.Max(start.Y, ende.Y)
                e.Graphics.DrawRectangle(myPen, minX, minY, maxX - minX, maxY - minY)
            End If
        Else
            previewMarkierung.OnDraw(sender, e)
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim mode As Vektor_Picturebox.SelectionMode = sender.getCurrentSelectionMode()
        If mode = Vektor_Picturebox.SelectionMode.AddSelection Then
            Dim p As PointF = e._Default_CursorExtensionTopLeft
            Dim s As SizeF = e._Default_CursorExtensionSize
            Dim myPen As New Pen(e.CursorPen.Color, 1)
            e.Graphics.DrawLine(myPen, p.X, p.Y + 0.5F * s.Height, p.X + s.Width, p.Y + 0.5F * s.Height)
            e.Graphics.DrawLine(myPen, p.X + 0.5F * s.Width, p.Y, p.X + 0.5F * s.Width, p.Y + s.Height)
        ElseIf mode = Vektor_Picturebox.SelectionMode.SubtractFromSelection Then
            Dim p As PointF = e._Default_CursorExtensionTopLeft
            Dim s As SizeF = e._Default_CursorExtensionSize
            Dim myPen As New Pen(e.CursorPen.Color, 1)
            e.Graphics.DrawLine(myPen, p.X, p.Y + 0.5F * s.Height, p.X + s.Width, p.Y + 0.5F * s.Height)
        End If
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Bereit
    End Function
End Class
