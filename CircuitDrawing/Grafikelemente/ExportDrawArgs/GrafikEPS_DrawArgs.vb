Imports System.IO
Public Class GrafikEPS_DrawArgs
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

    Private currentFarbe As Farbe
    Private currentLW As Single
    Private currentLineCap As Integer
    Private currentLinejoin As Integer
    Private currentDashstyle() As Single

    Private myNumberFormatInfo As Globalization.NumberFormatInfo

    Public Sub New(maßeinheit As Single, offset As Point, offsetTEX As Point, LineStyles As LineStyleList, FillStyles As FillStyleList, fonts As FontList)
        Me.maßeinheit = maßeinheit
        Me.offset = offset
        Me.offsetTEX = offsetTEX
        Me.LineStyles = LineStyles
        Me.FillStyles = FillStyles
        Me.Fonts = fonts
        Me.warnings = New List(Of String)
        currentFarbe = New Farbe(0, 0, 0, 0)
        currentLW = -1
        currentLineCap = -1
        currentLinejoin = -1
        currentDashstyle = Nothing

        myNumberFormatInfo = New Globalization.NumberFormatInfo()
        myNumberFormatInfo.NumberDecimalSeparator = "."
    End Sub

    Public Sub switchToLinestyle(writer As StreamWriter, ls As ScaleableLinestyle)
        LineStyles.getLineStyle(ls.linestyle).writeEPS(writer, ls.scaling, Me)
    End Sub

    Public Sub switchToFillStyle(writer As StreamWriter, fs As Integer)
        FillStyles.getFillStyle(fs).writeEPS(writer, Me)
    End Sub

    Public Sub switchToFillColorFromLinestyle(writer As StreamWriter, ls As Integer)
        switchToColor(writer, LineStyles.getLineStyle(ls).farbe)
    End Sub

    Public Sub switchToColor(writer As StreamWriter, c As Farbe)
        If currentFarbe.Color_A = 0 OrElse c.Color_R <> currentFarbe.Color_R OrElse c.Color_G <> currentFarbe.Color_G OrElse c.Color_B <> currentFarbe.Color_B Then
            writer.WriteLine(toStringF(c.Color_R / 255.0F) & " " & toStringF(c.Color_G / 255.0F) & " " & toStringF(c.Color_B / 255.0F) & " setrgbcolor")
            currentFarbe = c
            currentFarbe.Color_A = 255
        End If
    End Sub

    Public Sub switchToLinewidth(writer As StreamWriter, dicke As Single)
        If currentLW <> dicke Then
            writer.WriteLine(writeValue_in_mm(dicke) & " setlinewidth")
            currentLW = dicke
        End If
    End Sub

    Public Sub switchToLinecap(writer As StreamWriter, linecap As Integer)
        If linecap <> currentLineCap Then
            writer.WriteLine(linecap & " setlinecap")
            currentLineCap = linecap
        End If
    End Sub

    Public Sub switchToDashstyle(writer As StreamWriter, dashstyle() As Single)
        Dim neu As Boolean = False
        If dashstyle Is Nothing AndAlso currentDashstyle IsNot Nothing Then
            neu = True
        ElseIf dashstyle IsNot Nothing AndAlso currentDashstyle Is Nothing Then
            neu = True
        ElseIf dashstyle IsNot Nothing AndAlso currentDashstyle IsNot Nothing Then
            If dashstyle.Length <> currentDashstyle.Length Then
                neu = True
            Else
                For i As Integer = 0 To dashstyle.Length - 1
                    If dashstyle(i) <> currentDashstyle(i) Then
                        neu = True
                        Exit For
                    End If
                Next
            End If
        End If

        If neu Then
            If dashstyle Is Nothing Then
                writer.WriteLine("[] 0 setdash")
                currentDashstyle = Nothing
            Else
                Dim str As String = "["
                For i As Integer = 0 To dashstyle.Length - 1
                    If i = dashstyle.Length - 1 Then
                        str &= toStringF(dashstyle(i)) & "]"
                    Else
                        str &= toStringF(dashstyle(i)) & " "
                    End If
                Next
                str &= " 0 setdash"
                writer.WriteLine(str)
                ReDim currentDashstyle(dashstyle.Length - 1)
                For i As Integer = 0 To dashstyle.Length - 1
                    currentDashstyle(i) = dashstyle(i)
                Next
            End If
        End If
    End Sub

    Public Sub switchToLinejoin(writer As StreamWriter, linejoin As Integer)
        If linejoin <> currentLinejoin Then
            writer.WriteLine(linejoin & " setlinejoin")
            currentLinejoin = linejoin
        End If
    End Sub

    Public Function writePoint(p1 As Point) As String
        Dim pF As PointF = toPoint(p1)
        Return toStringF(pF.X) & " " & toStringF(pF.Y)
    End Function

    Public Function writePoint(p1 As PointF) As String
        Dim pF As PointF = toPoint(p1)
        Return toStringF(pF.X) & " " & toStringF(pF.Y)
    End Function

    Public Function writePoint_converted(pF As PointF) As String
        Return toStringF(pF.X) & " " & toStringF(pF.Y)
    End Function

    Public Function writeValue_in_mm(value As Single) As String
        Return toStringF(mmToPoint(value))
    End Function

    Public Function writeSize(s As Size) As String
        Dim sF As SizeF = New SizeF(mmToPoint(s.Width * maßeinheit), mmToPoint(s.Height * maßeinheit))
        Return toStringF(sF.Width) & " " & toStringF(sF.Height)
    End Function

    Public Function writeSize(s As SizeF) As String
        Dim sF As SizeF = New SizeF(mmToPoint(s.Width * maßeinheit), mmToPoint(s.Height * maßeinheit))
        Return toStringF(sF.Width) & " " & toStringF(sF.Height)
    End Function

    Public Function writeScale(x As Integer) As String
        Dim xf As Single = mmToPoint(x * maßeinheit)
        Return toStringF(xf)
    End Function

    Public Function toPoint(p1 As PointF) As PointF
        Return New PointF(mmToPoint((p1.X + offset.X) * maßeinheit), mmToPoint((-p1.Y + offset.Y) * maßeinheit))
    End Function

    Public Function toStringF(value As Single) As String
        Return value.ToString(myNumberFormatInfo)
    End Function

    Public Shared Function mmToPoint(x As Single) As Single
        Return x * 72.0F / 25.4F
    End Function
End Class
