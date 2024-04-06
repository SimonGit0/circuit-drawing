Imports System.IO
Public Class Precompiled_Poly
    Inherits Precompiled_Grafik

    Private koards() As Ausdruck_Int
    Private fill, stroke, use_fillColor_from_linestyle As Boolean

    Public Sub New(koards() As Ausdruck_Int, fill As Boolean, stroke As Boolean, use_fillColor_from_linestyle As Boolean)
        Me.koards = koards
        Me.fill = fill
        Me.stroke = stroke
        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim punkte(koards.Length \ 2 - 1) As Point
        For i As Integer = 0 To punkte.Length - 1
            punkte(i) = New Point(args.ausrechnen(koards(2 * i)), args.ausrechnen(koards(2 * i + 1)))
        Next
        erg.addGrafik(New DO_Polygon(punkte, fill, stroke, use_fillColor_from_linestyle AndAlso erg.use_forecolor_fill, False, False))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(fill)
        writer.Write(stroke)
        writer.Write(use_fillColor_from_linestyle)
        writer.Write(koards.Length)
        For i As Integer = 0 To koards.Length - 1
            Ausdruck_Int.speichern(koards(i), writer)
        Next
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Poly
        Dim fill As Boolean = reader.ReadBoolean()
        Dim stroke As Boolean = reader.ReadBoolean()
        Dim use_fillColor_from_linestyle As Boolean = reader.ReadBoolean()

        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Koordinaten beim Laden des Polygons darf nicht negativ sein")
        Dim koards(anzahl - 1) As Ausdruck_Int
        For i As Integer = 0 To anzahl - 1
            koards(i) = Ausdruck_Int.laden(reader, version)
        Next

        Return New Precompiled_Poly(koards, fill, stroke, use_fillColor_from_linestyle)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If fill Then
            line = "polyfill("
        Else
            line = "poly("
        End If
        For i As Integer = 0 To koards.Count - 2
            line &= Ausdruck_Int.export(koards(i), writer).str & ", "
        Next
        line &= Ausdruck_Int.export(koards(koards.Count - 1), writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
