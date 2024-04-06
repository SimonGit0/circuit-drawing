Imports System.IO
Public MustInherit Class Precompiled_Grafik
    Inherits Precompiled_Befehl

    Public MustOverride Sub compile(args As AusrechnenArgs, erg As CompileArgs)

    Public Overridable Function simplifyBlock() As Precompiled_Grafik
        Return Me
    End Function

    Public Shared Sub speicherGrafik(grafik As Precompiled_Grafik, writer As BinaryWriter)
        If TypeOf grafik Is Precompiled_MultiLinie Then
            writer.Write(2)
            DirectCast(grafik, Precompiled_MultiLinie).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Ellipse Then
            writer.Write(3)
            DirectCast(grafik, Precompiled_Ellipse).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Rect Then
            writer.Write(4)
            DirectCast(grafik, Precompiled_Rect).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Arc Then
            writer.Write(5)
            DirectCast(grafik, Precompiled_Arc).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Poly Then
            writer.Write(6)
            DirectCast(grafik, Precompiled_Poly).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Selection Then
            writer.Write(7)
            DirectCast(grafik, Precompiled_Selection).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Snappoint Then
            writer.Write(8)
            DirectCast(grafik, Precompiled_Snappoint).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_SetLineScaling Then
            writer.Write(9)
            DirectCast(grafik, Precompiled_SetLineScaling).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_ArrowHead Then
            writer.Write(10)
            DirectCast(grafik, Precompiled_ArrowHead).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Bezier Then
            writer.Write(11)
            DirectCast(grafik, Precompiled_Bezier).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Dot Then
            writer.Write(12)
            DirectCast(grafik, Precompiled_Dot).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_LineArrow Then
            writer.Write(13)
            DirectCast(grafik, Precompiled_LineArrow).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Linie Then
            writer.Write(14)
            DirectCast(grafik, Precompiled_Linie).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_ParamInvisible Then
            writer.Write(15)
            DirectCast(grafik, Precompiled_ParamInvisible).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_SetVarGrafik Then
            writer.Write(16)
            DirectCast(grafik, Precompiled_SetVarGrafik).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_Text Then
            writer.Write(17)
            DirectCast(grafik, Precompiled_Text).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_FOR Then
            writer.Write(18)
            DirectCast(grafik, Precompiled_FOR).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_IF Then
            writer.Write(19)
            DirectCast(grafik, Precompiled_IF).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_ELSE Then
            writer.Write(20)
            DirectCast(grafik, Precompiled_ELSE).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_ELSE_IF Then
            writer.Write(21)
            DirectCast(grafik, Precompiled_ELSE_IF).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_ScaleLine Then
            writer.Write(22)
            DirectCast(grafik, Precompiled_ScaleLine).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_SetFillColor Then
            writer.Write(23)
            DirectCast(grafik, Precompiled_SetFillColor).speichern(writer)
        ElseIf TypeOf grafik Is Precompiled_MultiGrafik Then 'MultiGrafik als letztes, weil andere Elemente (For, etc.) auch davon erben!!
            writer.Write(1)
            DirectCast(grafik, Precompiled_MultiGrafik).speichernMULTI(writer)
        Else
            Throw New Exception("Fehler P0001: Kann diesen Grafik-Typ nicht speichern")
        End If
    End Sub

    Public Shared Function ladeGrafik(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_Grafik
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1 'Precompiled_MultiGrafik
                Return Precompiled_MultiGrafik.ladenMULTI(parent, reader, version)
            Case 2 'Precompiled_MultiLinie
                Return Precompiled_MultiLinie.laden(reader, version)
            Case 3 'Precompiled_Ellipse
                Return Precompiled_Ellipse.laden(reader, version)
            Case 4 'Precompiled_Rect
                Return Precompiled_Rect.laden(reader, version)
            Case 5 'Precompiled_Arc
                Return Precompiled_Arc.laden(reader, version)
            Case 6 'Precompiled_Poly
                Return Precompiled_Poly.laden(reader, version)
            Case 7 'Precompiled_Selection
                Return Precompiled_Selection.laden(reader, version)
            Case 8 'Precompiled_Snappoint
                Return Precompiled_Snappoint.laden(reader, version)
            Case 9 'Precompiled_SetLineScaling
                Return Precompiled_SetLineScaling.laden(reader, version)
            Case 10 'Precompiled_ArrowHead
                Return Precompiled_ArrowHead.laden(reader, version)
            Case 11 'Precompiled_Bezier
                Return Precompiled_Bezier.laden(reader, version)
            Case 12 'Precompiled_Dot
                Return Precompiled_Dot.laden(reader, version)
            Case 13 'Precompiled_LineArrow
                Return Precompiled_LineArrow.laden(reader, version)
            Case 14 'Precompiled_Linie
                Return Precompiled_Linie.laden(reader, version)
            Case 15 'Precompiled_ParamInvisible
                Return Precompiled_ParamInvisible.laden(reader, version)
            Case 16 'Precompiled_SetVarGrafik
                Return Precompiled_SetVarGrafik.laden(reader, version)
            Case 17 'Precompiled_Text
                Return Precompiled_Text.laden(reader, version)
            Case 18 'Precompiled_FOR
                Return Precompiled_FOR.laden(parent, reader, version)
            Case 19 'Precompiled_IF
                Return Precompiled_IF.laden(parent, reader, version)
            Case 20 'Precompiled_ELSE
                Return Precompiled_ELSE.laden(parent, reader, version)
            Case 21 'Precompiled_ELSE_IF
                Return Precompiled_ELSE_IF.laden(parent, reader, version)
            Case 22 'Precompiled_ScaleLine
                Return Precompiled_ScaleLine.laden(reader, version)
            Case 23 'Precompiled_SetFillColor
                Return Precompiled_SetFillColor.laden(reader, version)
            Case Else
                Throw New Exception("Fehler L0001: Kann diesen Grafik-Typ nicht laden")
        End Select
    End Function

    Public Shared Sub exportiereGrafik(grafik As Precompiled_Grafik, writer As Export_StreamWriter)
        If TypeOf grafik Is Precompiled_MultiLinie Then
            DirectCast(grafik, Precompiled_MultiLinie).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Ellipse Then
            DirectCast(grafik, Precompiled_Ellipse).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Rect Then
            DirectCast(grafik, Precompiled_Rect).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Arc Then
            DirectCast(grafik, Precompiled_Arc).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Poly Then
            DirectCast(grafik, Precompiled_Poly).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Selection Then
            DirectCast(grafik, Precompiled_Selection).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Snappoint Then
            DirectCast(grafik, Precompiled_Snappoint).export(writer)
        ElseIf TypeOf grafik Is Precompiled_SetLineScaling Then
            DirectCast(grafik, Precompiled_SetLineScaling).export(writer)
        ElseIf TypeOf grafik Is Precompiled_ArrowHead Then
            DirectCast(grafik, Precompiled_ArrowHead).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Bezier Then
            DirectCast(grafik, Precompiled_Bezier).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Dot Then
            DirectCast(grafik, Precompiled_Dot).export(writer)
        ElseIf TypeOf grafik Is Precompiled_LineArrow Then
            DirectCast(grafik, Precompiled_LineArrow).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Linie Then
            DirectCast(grafik, Precompiled_Linie).export(writer)
        ElseIf TypeOf grafik Is Precompiled_ParamInvisible Then
            DirectCast(grafik, Precompiled_ParamInvisible).export(writer)
        ElseIf TypeOf grafik Is Precompiled_SetVarGrafik Then
            DirectCast(grafik, Precompiled_SetVarGrafik).export(writer)
        ElseIf TypeOf grafik Is Precompiled_Text Then
            DirectCast(grafik, Precompiled_Text).export(writer)
        ElseIf TypeOf grafik Is Precompiled_FOR Then
            DirectCast(grafik, Precompiled_FOR).export(writer)
        ElseIf TypeOf grafik Is Precompiled_IF Then
            DirectCast(grafik, Precompiled_IF).export(writer)
        ElseIf TypeOf grafik Is Precompiled_ELSE Then
            DirectCast(grafik, Precompiled_ELSE).export(writer)
        ElseIf TypeOf grafik Is Precompiled_ELSE_IF Then
            DirectCast(grafik, Precompiled_ELSE_IF).export(writer)
        ElseIf TypeOf grafik Is Precompiled_ScaleLine Then
            DirectCast(grafik, Precompiled_ScaleLine).export(writer)
        ElseIf TypeOf grafik Is Precompiled_SetFillColor Then
            DirectCast(grafik, Precompiled_SetFillColor).export(writer)

        ElseIf TypeOf grafik Is Precompiled_MultiGrafik Then 'MultiGrafik als letztes, weil andere Elemente (For, etc.) auch davon erben!!
            DirectCast(grafik, Precompiled_MultiGrafik).exportMULTI(writer)
        Else
            Throw New Exception("Fehler E0001: Kann diesen Grafik-Typ nicht exportieren")
        End If
    End Sub
End Class

Public Class CompileArgs
    Private grafik As DO_MultiGrafik
    Public selection_Rect As Rectangle
    Public snaps As List(Of Snappoint)
    Public paramVisible() As Boolean

    Private current_linewithScaling As Single = 1.0

    Public parentArgs As CompileParentArgs

    Public callbackAfterOrigin As List(Of Action(Of CompileArgs, Point))

    Private scalings As List(Of ScalingLinie)
    Public use_forecolor_fill As Boolean = True
    Public benötigt_fillStil As Boolean = False

    Public Sub New(paramVisible() As Boolean, parentArgs As CompileParentArgs)
        Me.grafik = New DO_MultiGrafik()
        Me.selection_Rect = New Rectangle(0, 0, 0, 0)
        snaps = New List(Of Snappoint)()
        Me.scalings = New List(Of ScalingLinie)

        Me.paramVisible = paramVisible
        Me.parentArgs = parentArgs
        callbackAfterOrigin = New List(Of Action(Of CompileArgs, Point))
    End Sub

    Public Sub set_scaling(scaling As Single)
        If scaling < 0 Then
            scaling = 0
        End If
        Me.current_linewithScaling = scaling
    End Sub

    Public Sub addGrafik(g As DO_Grafik)
        g.lineStyle.scaling = current_linewithScaling
        If TypeOf g Is DO_MultiGrafik Then
            DirectCast(g, DO_MultiGrafik).setLineScalingRekursiv(current_linewithScaling)
        End If
        grafik.childs.Add(g)
    End Sub

    Public Sub addScaling(s As ScalingLinie)
        Me.scalings.Add(s)
    End Sub

    Public Sub TransformScalings(t As Transform)
        For i As Integer = 0 To scalings.Count - 1
            scalings(i).transform(t)
        Next
    End Sub

    Public Sub insertGrafik(index As Integer, g As DO_Grafik)
        g.lineStyle.scaling = current_linewithScaling
        If TypeOf g Is DO_MultiGrafik Then
            DirectCast(g, DO_MultiGrafik).setLineScalingRekursiv(current_linewithScaling)
        End If
        grafik.childs.Insert(index, g)
    End Sub

    Public Sub removeGrafik(index As Integer)
        grafik.childs.RemoveAt(index)
    End Sub

    Public Function getIndexOfGrafik(g As DO_Grafik) As Integer
        For i As Integer = 0 To grafik.childs.Count - 1
            If grafik.childs(i).Equals(g) Then
                Return i
            End If
        Next
        Return -1
    End Function

    Public Function get_grafik() As DO_MultiGrafik
        Return grafik
    End Function

    Public Function get_Scalings() As List(Of ScalingLinie)
        Return scalings
    End Function
End Class
