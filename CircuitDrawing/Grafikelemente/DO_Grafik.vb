Imports System.IO
Imports PdfSharp.Drawing
Public MustInherit Class DO_Grafik

    Public fillstyle As Integer
    Public lineStyle As ScaleableLinestyle

    Public use_fillColor_from_linestyle As Boolean = False

    Public Sub New(use_fillColor_from_linestyle As Boolean)
        fillstyle = 0
        lineStyle = New ScaleableLinestyle(0)

        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
    End Sub

    Public Overridable Function fill(args As GrafikDrawArgs) As Boolean
        If use_fillColor_from_linestyle Then
            Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
        Else
            Return args.FillStyles.getFillStyle(fillstyle).farbe.Color_A > 0
        End If
    End Function

    Public Overridable Function fill(args As GrafikPDFSharp_DrawArgs) As Boolean
        If use_fillColor_from_linestyle Then
            Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
        Else
            Return args.FillStyles.getFillStyle(fillstyle).farbe.Color_A > 0
        End If
    End Function

    Public Overridable Function fill(args As GrafikEPS_DrawArgs) As Boolean
        If use_fillColor_from_linestyle Then
            Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
        Else
            Return args.FillStyles.getFillStyle(fillstyle).farbe.Color_A > 0
        End If
    End Function

    Public Overridable Function stroke(args As GrafikDrawArgs) As Boolean
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
    End Function

    Public Overridable Function stroke(args As GrafikPDFSharp_DrawArgs) As Boolean
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
    End Function

    Public Overridable Function stroke(args As GrafikEPS_DrawArgs) As Boolean
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).farbe.Color_A > 0
    End Function

    Public Function getBrush(args As GrafikDrawArgs) As Brush
        If Me.use_fillColor_from_linestyle Then
            Return args.LineStyles.getLineStyle(Me.lineStyle.linestyle).getSolidBrush()
        Else
            Return args.getBrush(fillstyle)
        End If
    End Function

    Public Function getBrush(args As GrafikPDFSharp_DrawArgs) As XBrush
        If Me.use_fillColor_from_linestyle Then
            Return args.LineStyles.getLineStyle(Me.lineStyle.linestyle).getSolidBrushX()
        Else
            Return args.getBrush(fillstyle)
        End If
    End Function

    Public Sub switchToFillstyle(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        If Me.use_fillColor_from_linestyle Then
            args.switchToFillColorFromLinestyle(writer, Me.lineStyle.linestyle)
        Else
            args.switchToFillStyle(writer, fillstyle)
        End If
    End Sub

    Public Function getPen(args As GrafikDrawArgs) As Pen
        Return Me.getPen(args, False)
    End Function

    Public Function getPen(args As GrafikDrawArgs, noDash As Boolean) As Pen
        Return args.getPen(lineStyle.linestyle, lineStyle.scaling, noDash)
    End Function

    Public Function getPen(args As GrafikPDFSharp_DrawArgs) As XPen
        Return Me.getPen(args, False)
    End Function

    Public Function getPen(args As GrafikPDFSharp_DrawArgs, noDash As Boolean) As XPen
        Return args.getPen(lineStyle.linestyle, lineStyle.scaling, noDash)
    End Function

    Public Function getLineWidth(args As GrafikDrawArgs) As Single
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).Dicke * lineStyle.scaling
    End Function

    Public Function getLineWidth(args As GrafikPDFSharp_DrawArgs) As Single
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).Dicke * lineStyle.scaling
    End Function

    Public Function getLineWidth(args As GrafikEPS_DrawArgs) As Single
        Return args.LineStyles.getLineStyle(lineStyle.linestyle).Dicke * lineStyle.scaling
    End Function

    Public MustOverride Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)

    Public MustOverride Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)

    Public MustOverride Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)

    Public Overridable Sub drawTEX_Text(writer As StreamWriter, args As GrafikTEX_DrawArgs)
    End Sub

    Public MustOverride Function getBoundingBox() As Rectangle

    Public Function Clone() As DO_Grafik
        Dim erg As DO_Grafik = Me.Clone_intern()
        erg.lineStyle = Me.lineStyle
        erg.fillstyle = Me.fillstyle
        Return erg
    End Function

    Protected MustOverride Function Clone_intern() As DO_Grafik

    Public MustOverride Sub transform(t As Transform)

    Public Overridable Sub markAllUsedLinestyles(usedLinestyles() As Boolean)
        usedLinestyles(lineStyle.linestyle) = True
    End Sub

    Public Overridable Sub markAllUsedFillstyles(usedFillstyles() As Boolean)
        usedFillstyles(fillstyle) = True
    End Sub

    Public Overridable Sub markAllUsedFontStyles(usedFontstyles() As Boolean)
    End Sub
End Class

Public Structure ScaleableLinestyle
    Public linestyle As Integer
    Public scaling As Single

    Public Sub New(ls As Integer)
        Me.linestyle = ls
        Me.scaling = 1.0
    End Sub
End Structure

Public Enum Drawing_FillMode
    FillAndStroke = 0
    OnlyFill = 1
    OnlyStroke = 2
End Enum