Imports System.IO
Public Class Precompiled_MultiLinie
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
        erg.addGrafik(New DO_MultiLinie(punkte, False))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(koards.Count)
        For i As Integer = 0 To koards.Count - 1
            Ausdruck_Int.speichern(koards(i), writer)
        Next
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_MultiLinie
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl darf nicht kleiner als 0 sein")

        Dim koards(anzahl - 1) As Ausdruck_Int
        For i As Integer = 0 To anzahl - 1
            koards(i) = Ausdruck_Int.laden(reader, version)
        Next

        Return New Precompiled_MultiLinie(koards)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "line("
        For i As Integer = 0 To koards.Count - 2
            line &= Ausdruck_Int.export(koards(i), writer).str & ", "
        Next
        line &= Ausdruck_Int.export(koards(koards.Length - 1), writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
