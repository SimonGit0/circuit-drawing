Imports PdfSharp.Drawing
Public Class GrafikPDFSharp_DrawArgs
    'X Anzeige im PDF = (x + offset.x) * maßeinheit
    'Y Anzeige im PDF = (y + offset.y) * maßeinheit
    Public maßeinheit As Single
    Public offset As Point
    Public offsetTEX As Point

    Public Property Fonts As FontList
    Public Property LineStyles As LineStyleList
    Public Property FillStyles As FillStyleList
    Public Property OhneText As Boolean = False

    Public breitePDF_inINT As Integer
    Public höhePDF_inINT As Integer

    Public tf As XTextFormatterEx2

    Public warnings As List(Of String)

    Public Sub New(maßeinheit As Single, offset As Point, offsetTEX As Point, LineStyles As LineStyleList, FillStyles As FillStyleList, fonts As FontList)
        Me.maßeinheit = maßeinheit
        Me.offset = offset
        Me.offsetTEX = offsetTEX
        Me.LineStyles = LineStyles
        Me.FillStyles = FillStyles
        Me.Fonts = fonts
        Me.warnings = New List(Of String)
    End Sub

    Public Sub initTextFormatter(gfx As XGraphics)
        tf = New XTextFormatterEx2(gfx)
    End Sub

    Public Function toPoint(p1 As Point) As XPoint
        Return New XPoint(mmToPoint((p1.X + offset.X) * maßeinheit), mmToPoint((p1.Y + offset.Y) * maßeinheit))
    End Function

    Public Function toPoint(p1 As PointF) As XPoint
        Return New XPoint(mmToPoint((p1.X + offset.X) * maßeinheit), mmToPoint((p1.Y + offset.Y) * maßeinheit))
    End Function

    Public Function toScale(scale As Integer) As Double
        Return mmToPoint(scale * maßeinheit)
    End Function

    Public Function toRect(r As Rectangle) As XRect
        Return New XRect(toPoint(r.Location), New XSize(toScale(r.Width), toScale(r.Height)))
    End Function

    Public Function getPen(style As Integer, scaling As Single, noDash As Boolean) As XPen
        Return LineStyles.getLineStyle(style).getXPen(scaling, noDash)
    End Function

    Public Function getBrush(style As Integer) As XBrush
        Return FillStyles.getFillStyle(style).getXBrush()
    End Function

    Public Function getFont(font As Integer) As XFont
        Return Fonts.getFontStyle(font).getXFont(Me)
    End Function

    Public Function getTiefHochFont(font As Integer) As XFont
        Return Fonts.getFontStyle(font).getTiefHochXFont(Me)
    End Function

    Public Function getFontBrush(font As Integer) As XBrush
        Return Fonts.getFontStyle(font).getXBrush()
    End Function

    Public Shared Function mmToPoint(x As Double) As Double
        Return XUnit.FromMillimeter(x).Point
    End Function

    Public Sub showWarnings()
        If Me.warnings.Count > 0 Then
            Dim erg As String = ""
            For i As Integer = 0 To warnings.Count - 2
                erg &= "WARNUNG: " & warnings(i) & vbCrLf & vbCrLf
            Next
            erg &= "WARNUNG: " & warnings(warnings.Count - 1)
            MessageBox.Show("Beim Speichern der PDF-Datei sind folgende Warnungen aufgetreten: " & vbCrLf & vbCrLf & erg, "Warnungen beim Export", MessageBoxButtons.OK)
        End If
    End Sub
End Class
