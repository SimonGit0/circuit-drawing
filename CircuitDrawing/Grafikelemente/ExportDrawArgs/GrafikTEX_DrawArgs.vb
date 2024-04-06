Public Class GrafikTEX_DrawArgs
    'X Anzeige im PDF = (x + offset.x)
    'Y Anzeige im PDF = (y + offset.y)
    Public maßeinheit As Single
    Public offset As Point
    Private fonts As FontList
    Public Sub New(maßeinheit As Single, offset As Point, fonts As FontList)
        Me.maßeinheit = maßeinheit
        Me.offset = offset
        Me.fonts = fonts
    End Sub

    Public Function toTEXpoint(p As Point) As Point
        Return New Point(p.X + offset.X, offset.Y - p.Y)
    End Function

    Public Function getFont(font As Integer) As FontStyle
        Return fonts.getFontStyle(font)
    End Function

End Class

Public Class CustomStringBuilder
    Private myNumberFormatInfo As Globalization.NumberFormatInfo
    Private myStringbuilder As System.Text.StringBuilder
    Public Sub New()
        myStringbuilder = New System.Text.StringBuilder()
        myNumberFormatInfo = New Globalization.NumberFormatInfo()
        myNumberFormatInfo.NumberDecimalSeparator = "."
    End Sub

    Public Sub Append(str As String)
        myStringbuilder.Append(str)
    End Sub

    Public Sub Append(wert As Double)
        myStringbuilder.Append("""")
        myStringbuilder.Append(wert.ToString(myNumberFormatInfo))
        myStringbuilder.Append("""")
    End Sub

    Public Sub Append(wert As Single)
        myStringbuilder.Append("""")
        myStringbuilder.Append(wert.ToString(myNumberFormatInfo))
        myStringbuilder.Append("""")
    End Sub

    Public Sub Append(farbe As Color)
        myStringbuilder.Append("""rgb(")
        myStringbuilder.Append(farbe.R)
        myStringbuilder.Append(",")
        myStringbuilder.Append(farbe.G)
        myStringbuilder.Append(",")
        myStringbuilder.Append(farbe.B)
        myStringbuilder.Append(")""")
    End Sub

    Public Sub AppendPlain(wert As Double)
        myStringbuilder.Append(wert.ToString(myNumberFormatInfo))
    End Sub

    Public Sub AppendPlain(wert As Single)
        myStringbuilder.Append(wert.ToString(myNumberFormatInfo))
    End Sub

    Public Sub AppendPlain(wert As Integer)
        myStringbuilder.Append(wert.ToString(myNumberFormatInfo))
    End Sub

    Public Overrides Function ToString() As String
        Return myStringbuilder.ToString()
    End Function
End Class