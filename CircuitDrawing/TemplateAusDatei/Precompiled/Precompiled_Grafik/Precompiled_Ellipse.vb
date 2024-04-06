Imports System.IO
Public Class Precompiled_Ellipse
    Inherits Precompiled_Grafik

    Private use_fillColor_from_linestyle As Boolean
    Private myFillMode As Drawing_FillMode
    Private cx, cy, rx, ry As Ausdruck_Int

    Public Sub New(cx As Ausdruck_Int, cy As Ausdruck_Int, rx As Ausdruck_Int, ry As Ausdruck_Int, use_fillColor_from_linestyle As Boolean, fillMode As Drawing_FillMode)
        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
        Me.myFillMode = fillMode
        Me.cx = cx
        Me.cy = cy
        Me.rx = rx
        Me.ry = ry
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim cx As Integer = args.ausrechnen(Me.cx)
        Dim cy As Integer = args.ausrechnen(Me.cy)
        Dim rx As Integer = args.ausrechnen(Me.rx)
        Dim ry As Integer = args.ausrechnen(Me.ry)
        erg.addGrafik(New DO_Ellipse(New Rectangle(cx - rx, cy - ry, 2 * rx, 2 * ry), use_fillColor_from_linestyle And erg.use_forecolor_fill, myFillMode))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(use_fillColor_from_linestyle)
        writer.Write(CInt(myFillMode))
        Ausdruck_Int.speichern(cx, writer)
        Ausdruck_Int.speichern(cy, writer)
        Ausdruck_Int.speichern(rx, writer)
        Ausdruck_Int.speichern(ry, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Ellipse
        Dim use_fillColor_from_linestyle As Boolean = reader.ReadBoolean()
        Dim fm As Drawing_FillMode
        If version >= 28 Then
            fm = CType(reader.ReadInt32(), Drawing_FillMode)
        Else
            Dim onlyFill As Boolean = reader.ReadBoolean()
            If onlyFill Then
                fm = Drawing_FillMode.OnlyFill
            Else
                fm = Drawing_FillMode.OnlyStroke
            End If
        End If
        Dim cx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim cy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim rx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim ry As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Ellipse(cx, cy, rx, ry, use_fillColor_from_linestyle, fm)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If myFillMode = Drawing_FillMode.OnlyFill AndAlso use_fillColor_from_linestyle Then
            line = "circfill("
        Else
            line = "circ("
        End If
        line &= Ausdruck_Int.export(cx, writer).str & ", "
        line &= Ausdruck_Int.export(cy, writer).str & ", "
        line &= Ausdruck_Int.export(rx, writer).str & ", "
        line &= Ausdruck_Int.export(ry, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
