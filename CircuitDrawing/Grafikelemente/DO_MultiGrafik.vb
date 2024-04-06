Imports System.IO
Imports PdfSharp.Drawing

Public Class DO_MultiGrafik
    Inherits DO_Grafik

    Public childs As New List(Of DO_Grafik)

    Public Sub New()
        Me.New(New List(Of DO_Grafik))
    End Sub

    Public Sub New(childs As List(Of DO_Grafik))
        MyBase.New(False)
        Me.childs = childs
    End Sub

    Public Overrides Sub drawGraphics(g As Graphics, args As GrafikDrawArgs)
        For Each g1 As DO_Grafik In childs
            g1.drawGraphics(g, args)
        Next
    End Sub

    Public Overrides Sub drawPDFSharp(gfx As XGraphics, args As GrafikPDFSharp_DrawArgs)
        For i As Integer = 0 To childs.Count - 1
            childs(i).drawPDFSharp(gfx, args)
        Next
    End Sub

    Public Overrides Sub drawTEX_Text(writer As StreamWriter, args As GrafikTEX_DrawArgs)
        For i As Integer = 0 To childs.Count - 1
            childs(i).drawTEX_Text(writer, args)
        Next
    End Sub


    Public Overrides Function getBoundingBox() As Rectangle
        Dim box As New Rectangle(0, 0, 0, 0)

        For Each g1 As DO_Grafik In childs
            box = Mathe.Union(box, g1.getBoundingBox())
        Next
        Return box
    End Function

    Public Overrides Sub transform(t As Transform)
        For i As Integer = 0 To childs.Count - 1
            childs(i).transform(t)
        Next
    End Sub

    Protected Overrides Function Clone_intern() As DO_Grafik
        Dim cn As New List(Of DO_Grafik)(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            cn.Add(childs(i).Clone())
        Next
        Return New DO_MultiGrafik(cn)
    End Function

    Public Overrides Sub markAllUsedLinestyles(usedLinestyles() As Boolean)
        MyBase.markAllUsedLinestyles(usedLinestyles)
        For i As Integer = 0 To childs.Count - 1
            childs(i).markAllUsedLinestyles(usedLinestyles)
        Next
    End Sub

    Public Overrides Sub markAllUsedFillstyles(usedFillstyles() As Boolean)
        MyBase.markAllUsedFillstyles(usedFillstyles)
        For i As Integer = 0 To childs.Count - 1
            childs(i).markAllUsedFillstyles(usedFillstyles)
        Next
    End Sub

    Public Overrides Sub markAllUsedFontStyles(usedFontstyles() As Boolean)
        MyBase.markAllUsedFontStyles(usedFontstyles)
        For i As Integer = 0 To childs.Count - 1
            childs(i).markAllUsedFontStyles(usedFontstyles)
        Next
    End Sub

    Public Sub setLineStyleRekursiv(linestyle As Integer)
        For i As Integer = 0 To childs.Count - 1
            childs(i).lineStyle.linestyle = linestyle
            If TypeOf childs(i) Is DO_MultiGrafik Then
                DirectCast(childs(i), DO_MultiGrafik).setLineStyleRekursiv(linestyle)
            End If
        Next
    End Sub

    Public Sub setLineScalingRekursiv(scaling As Single)
        For i As Integer = 0 To childs.Count - 1
            childs(i).lineStyle.scaling = scaling
            If TypeOf childs(i) Is DO_MultiGrafik Then
                DirectCast(childs(i), DO_MultiGrafik).setLineScalingRekursiv(scaling)
            End If
        Next
    End Sub

    Public Sub setFillStyleRekursiv(fillstyle As Integer)
        For i As Integer = 0 To childs.Count - 1
            childs(i).fillstyle = fillstyle
            If TypeOf childs(i) Is DO_MultiGrafik Then
                DirectCast(childs(i), DO_MultiGrafik).setFillStyleRekursiv(fillstyle)
            End If
        Next
    End Sub

    Public Sub setFontstyleRekursiv(fontstyle As Integer)
        For i As Integer = 0 To childs.Count - 1
            If TypeOf childs(i) Is DO_MultiGrafik Then
                DirectCast(childs(i), DO_MultiGrafik).setFontstyleRekursiv(fontstyle)
            ElseIf TypeOf childs(i) Is DO_Text Then
                DirectCast(childs(i), DO_Text).fontIndex = fontstyle
            ElseIf TypeOf childs(i) Is DO_Textfeld Then
                DirectCast(childs(i), DO_Textfeld).font = fontstyle
            End If
        Next
    End Sub

    Public Overrides Sub drawEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        For i As Integer = 0 To childs.Count - 1
            childs(i).drawEPS(writer, args)
        Next
    End Sub
End Class
