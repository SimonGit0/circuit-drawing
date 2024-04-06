Imports System.IO
Public Class Precompiled_Rect
    Inherits Precompiled_Grafik

    Private use_fillColor_from_linestyle As Boolean
    Private myFillMode As Drawing_FillMode
    Private x, y, breite, höhe As Ausdruck_Int

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, breite As Ausdruck_Int, höhe As Ausdruck_Int, use_fillColor_from_linestyle As Boolean, fm As Drawing_FillMode)
        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
        Me.myFillMode = fm
        Me.x = x
        Me.y = y
        Me.breite = breite
        Me.höhe = höhe
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.addGrafik(New DO_Rechteck(New Rectangle(args.ausrechnen(x), args.ausrechnen(y), args.ausrechnen(breite), args.ausrechnen(höhe)), use_fillColor_from_linestyle And erg.use_forecolor_fill, myFillMode))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(use_fillColor_from_linestyle)
        writer.Write(CInt(myFillMode))
        Ausdruck_Int.speichern(x, writer)
        Ausdruck_Int.speichern(y, writer)
        Ausdruck_Int.speichern(breite, writer)
        Ausdruck_Int.speichern(höhe, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Rect
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
        Dim x As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim breite As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim höhe As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Rect(x, y, breite, höhe, use_fillColor_from_linestyle, fm)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If myFillMode = Drawing_FillMode.OnlyFill AndAlso use_fillColor_from_linestyle Then
            line = "rectfill("
        Else
            line = "rect("
        End If
        line &= Ausdruck_Int.export(x, writer).str & ", "
        line &= Ausdruck_Int.export(y, writer).str & ", "
        line &= Ausdruck_Int.export(breite, writer).str & ", "
        line &= Ausdruck_Int.export(höhe, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
