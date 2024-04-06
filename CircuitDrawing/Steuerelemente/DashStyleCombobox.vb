Public Class DashStyleCombobox
    Inherits JoSiCombobox

    Private dashStyleScale As Integer = 300
    Private PixelPerMM As Single = 3.0

    Public Sub New()
        For i As Integer = 0 To DashStyle.getDashStyleCount() - 1
            Me.Items.Add(i)
        Next
        Me.SelectedIndex = 0
    End Sub

    Protected Overrides Sub OnDrawItemDropDownForeground(e As DrawItemEventArgs)
        Dim lineColor As Color = e.ForeColor
        If (e.State And DrawItemState.Selected) <> 0 Then
            e.Graphics.FillRectangle(selectionColorBrush, e.Bounds)
            lineColor = foreColorSelected
        End If

        Dim p1 As New Point(e.Bounds.X + 5, e.Bounds.Y + e.Bounds.Height \ 2)
        Dim p2 As New Point(e.Bounds.X + e.Bounds.Width - 5, e.Bounds.Y + e.Bounds.Height \ 2)

        Dim dashStyle As Integer = e.Index
        Dim myPen As Pen = New LineStyle(New Farbe(255, lineColor.R, lineColor.G, lineColor.B), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 1.0F, New DashStyle(dashStyle, dashStyleScale)).getPen(PixelPerMM, 1.0F, False)

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        e.Graphics.DrawLine(myPen, p1, p2)

    End Sub

    Protected Overrides Sub OnDrawItemSelectedForeground(e As DrawItemEventArgs, text As String)
        If _various Then
            MyBase.OnDrawItemSelectedForeground(e, ElementEinstellung.VARIOUS_STRING)
        Else
            Dim lineColor As Color
            If Me.Enabled Then
                If (e.State And DrawItemState.Focus) = 0 Then
                    lineColor = Me.ForeColor
                Else
                    lineColor = Color.Black
                End If
            Else
                lineColor = foreColor_EnabledFalse
            End If

            Dim p1 As New Point(e.Bounds.X + 5, e.Bounds.Y + e.Bounds.Height \ 2)
            Dim p2 As New Point(e.Bounds.X + e.Bounds.Width - 5, e.Bounds.Y + e.Bounds.Height \ 2)

            Dim dashStyle As Integer = e.Index
            Dim myPen As Pen = New LineStyle(New Farbe(255, lineColor.R, lineColor.G, lineColor.B), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 1.0F, New DashStyle(dashStyle, dashStyleScale)).getPen(PixelPerMM, 1.0F, False)

            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            e.Graphics.DrawLine(myPen, p1, p2)
        End If
    End Sub

End Class
