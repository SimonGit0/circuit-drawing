Imports System.IO

Public Class Precompiled_Dot
    Inherits Precompiled_Grafik

    Private x, y As Ausdruck_Int
    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.x = x
        Me.y = y
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.addGrafik(New DO_Dot(New Point(args.ausrechnen(x), args.ausrechnen(y)), Vektor_Picturebox.RADIUS_DOT, Vektor_Picturebox.RADIUS_DOT, 0, True))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(x, writer)
        Ausdruck_Int.speichern(y, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Dot
        Dim x As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Dot(x, y)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "dot("
        line &= Ausdruck_Int.export(x, writer).str & ", "
        line &= Ausdruck_Int.export(y, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
