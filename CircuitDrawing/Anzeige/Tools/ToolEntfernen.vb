Public Class ToolEntfernen
    Inherits ToolRechteckMarkierung

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        sender.noSelectionChangedEvents_ForSelectionBeforeDelete = True
        MyBase.MouseUp(sender, e)
        sender.noSelectionChangedEvents_ForSelectionBeforeDelete = False
        If e.Button = MouseButtons.Left Then
            If sender.has_selection() Then
                sender.delete_selected(True)
                previewMarkierung.PreviewSelect(sender, e.CursorPos)
            End If
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        Dim pen As New Pen(e.CursorPen.Color, 2)

        e.Graphics.DrawLine(pen, p, New PointF(p.X + s.Width, p.Y + s.Height))
        e.Graphics.DrawLine(pen, New PointF(p.X, p.Y + s.Height), New PointF(p.X + s.Width, p.Y))
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tool_Delete
    End Function
End Class
