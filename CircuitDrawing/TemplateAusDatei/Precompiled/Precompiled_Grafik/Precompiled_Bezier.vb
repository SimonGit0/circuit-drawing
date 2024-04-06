Imports System.IO
Public Class Precompiled_Bezier
    Inherits Precompiled_Grafik

    Private koards() As Ausdruck_Int

    Public Sub New(koards() As Ausdruck_Int)
        Me.koards = koards
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim punkte(koards.Length \ 2 - 1) As Point
        For i As Integer = 0 To punkte.Length - 1
            punkte(i) = New Point(args.ausrechnen(koards(2 * i)), args.ausrechnen(koards(2 * i + 1)))
        Next
        erg.addGrafik(New DO_Bezier(punkte, False, Drawing_FillMode.OnlyStroke))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(koards.Length)
        For i As Integer = 0 To koards.Length - 1
            Ausdruck_Int.speichern(koards(i), writer)
        Next
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Bezier
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Punkte in der Bezier-Kurve darf nicht negativ sein")
        Dim koards(anzahl - 1) As Ausdruck_Int
        For i As Integer = 0 To anzahl - 1
            koards(i) = Ausdruck_Int.laden(reader, version)
        Next
        Return New Precompiled_Bezier(koards)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "bezier("
        For i As Integer = 0 To koards.Length - 2
            line &= Ausdruck_Int.export(koards(i), writer).str & ", "
        Next
        line &= Ausdruck_Int.export(koards(koards.Length - 1), writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
