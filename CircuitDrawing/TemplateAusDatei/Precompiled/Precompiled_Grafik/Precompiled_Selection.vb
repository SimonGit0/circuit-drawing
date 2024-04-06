Imports System.IO
Public Class Precompiled_Selection
    Inherits Precompiled_Grafik

    Public x, y, w, h As Ausdruck_Int

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, w As Ausdruck_Int, h As Ausdruck_Int)
        Me.x = x
        Me.y = y
        Me.w = w
        Me.h = h
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.selection_Rect = New Rectangle(args.ausrechnen(x), args.ausrechnen(y), args.ausrechnen(w), args.ausrechnen(h))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(x, writer)
        Ausdruck_Int.speichern(y, writer)
        Ausdruck_Int.speichern(w, writer)
        Ausdruck_Int.speichern(h, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Selection
        Dim x As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim w As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim h As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Selection(x, y, w, h)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "select("
        line &= Ausdruck_Int.export(x, writer).str & ", "
        line &= Ausdruck_Int.export(y, writer).str & ", "
        line &= Ausdruck_Int.export(w, writer).str & ", "
        line &= Ausdruck_Int.export(h, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
