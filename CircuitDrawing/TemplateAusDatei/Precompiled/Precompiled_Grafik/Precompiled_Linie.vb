Imports System.IO
Public Class Precompiled_Linie
    Inherits Precompiled_Grafik

    Private x1, y1, x2, y2 As Ausdruck_Int

    Public Sub New(x1 As Ausdruck_Int, y1 As Ausdruck_Int, x2 As Ausdruck_Int, y2 As Ausdruck_Int)
        Me.x1 = x1
        Me.y1 = y1
        Me.x2 = x2
        Me.y2 = y2
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.addGrafik(New DO_Linie(New Point(args.ausrechnen(x1), args.ausrechnen(y1)), New Point(args.ausrechnen(x2), args.ausrechnen(y2)), False))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(x1, writer)
        Ausdruck_Int.speichern(y1, writer)
        Ausdruck_Int.speichern(x2, writer)
        Ausdruck_Int.speichern(y2, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Linie
        Dim x1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim x2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Linie(x1, y1, x2, y2)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "line("
        line &= Ausdruck_Int.export(x1, writer).str & ", "
        line &= Ausdruck_Int.export(y1, writer).str & ", "
        line &= Ausdruck_Int.export(x2, writer).str & ", "
        line &= Ausdruck_Int.export(y2, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
