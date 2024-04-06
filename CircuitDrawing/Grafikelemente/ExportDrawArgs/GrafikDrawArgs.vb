Public Class GrafikDrawArgs
    'Y-Anzeige = Y_Quelle * faktorY + offsetY
    'X-Anzeige = X_Quelle * faktorX + offsetX
    Public Property faktorX As Single
    Public Property faktorY As Single
    Public Property offsetX As Single
    Public Property offsetY As Single

    Public Property clip_range As Rectangle

    Public Property Fonts As FontList
    Public Property LineStyles As LineStyleList
    Public Property FillStyles As FillStyleList

    Public Property PixelPerMM As Single

    Public Property TextVorschauMode As Boolean = False

    Public Sub New(linestyles As LineStyleList, fillstyles As FillStyleList, fonts As FontList, pixelPerMM As Single, textVorschauMode As Boolean)
        Me.LineStyles = linestyles
        Me.FillStyles = fillstyles
        Me.Fonts = fonts
        Me.PixelPerMM = pixelPerMM
        Me.TextVorschauMode = textVorschauMode
    End Sub

    Public Function getPen(style As Integer, scaling As Single, noDash As Boolean) As Pen
        Return LineStyles.getLineStyle(style).getPen(PixelPerMM, scaling, noDash)
    End Function

    Public Function getBrush(style As Integer) As Brush
        Return FillStyles.getFillStyle(style).getBrush(PixelPerMM)
    End Function

    Public Function getFont(font As Integer) As Font
        Return Fonts.getFontStyle(font).getFont(PixelPerMM)
    End Function

    Public Function getTiefHochFont(font As Integer) As Font
        Return Fonts.getFontStyle(font).getTiefHochFont(PixelPerMM)
    End Function

    Public Function getFontBrush(font As Integer) As Brush
        Return Fonts.getFontStyle(font).getBrush(PixelPerMM)
    End Function

    Public Function toPictureboxScale(s As Integer) As Single
        Return s * (faktorX + faktorY) * 0.5F
    End Function

    Public Function toPictureboxPoint(p As Point) As PointF
        Return New PointF(p.X * faktorX + offsetX, p.Y * faktorY + offsetY)
    End Function

    Public Function toPictureboxPoint(p As PointF) As PointF
        Return New PointF(p.X * faktorX + offsetX, p.Y * faktorY + offsetY)
    End Function

    Public Function toPictureboxRect(r As Rectangle) As RectangleF
        Dim p As PointF = toPictureboxPoint(r.Location)
        Return New RectangleF(p.X, p.Y, r.Width * faktorX, r.Height * faktorY)
    End Function

    Public Function toIntRect(r As RectangleF) As Rectangle
        Return New Rectangle(CInt((r.X - offsetX) / faktorX), CInt((r.Y - offsetY) / faktorY), CInt(r.Width / faktorX), CInt(r.Height / faktorY))
    End Function
End Class
