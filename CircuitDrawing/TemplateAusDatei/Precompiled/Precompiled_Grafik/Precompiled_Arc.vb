Imports System.IO
Public Class Precompiled_Arc
    Inherits Precompiled_Grafik

    Private cx, cy, rad, startwinkel, deltawinkel As Ausdruck_Int
    Private radY As Ausdruck_Int 'kann nothing sein!!
    Private line_around, use_fillColor_from_linestyle As Boolean

    Public Sub New(cx As Ausdruck_Int, cy As Ausdruck_Int, rad As Ausdruck_Int, startwinkel As Ausdruck_Int, deltawinkel As Ausdruck_Int, line_around As Boolean, use_fillColor_from_linestyle As Boolean)
        Me.cx = cx
        Me.cy = cy
        Me.rad = rad
        radY = Nothing
        Me.startwinkel = startwinkel
        Me.deltawinkel = deltawinkel
        Me.line_around = line_around
        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
    End Sub

    Public Sub New(cx As Ausdruck_Int, cy As Ausdruck_Int, radX As Ausdruck_Int, radY As Ausdruck_Int, startwinkel As Ausdruck_Int, deltawinkel As Ausdruck_Int, line_around As Boolean, use_fillColor_from_linestyle As Boolean)
        Me.cx = cx
        Me.cy = cy
        Me.rad = radX
        Me.radY = radY
        Me.startwinkel = startwinkel
        Me.deltawinkel = deltawinkel
        Me.line_around = line_around
        Me.use_fillColor_from_linestyle = use_fillColor_from_linestyle
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        If radY Is Nothing Then
            Dim x As Integer = args.ausrechnen(cx)
            Dim y As Integer = args.ausrechnen(cy)
            Dim r As Integer = args.ausrechnen(rad)
            erg.addGrafik(New DO_Arc(New Point(x, y), r, r, args.ausrechnen(startwinkel), args.ausrechnen(deltawinkel), line_around, use_fillColor_from_linestyle, Drawing_FillMode.OnlyStroke))
        Else
            Dim x As Integer = args.ausrechnen(cx)
            Dim y As Integer = args.ausrechnen(cy)
            Dim rx As Integer = args.ausrechnen(rad)
            Dim ry As Integer = args.ausrechnen(radY)
            erg.addGrafik(New DO_Arc(New Point(x, y), rx, ry, args.ausrechnen(startwinkel), args.ausrechnen(deltawinkel), line_around, use_fillColor_from_linestyle, Drawing_FillMode.OnlyStroke))
        End If
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(line_around)
        writer.Write(use_fillColor_from_linestyle)
        Ausdruck_Int.speichern(cx, writer)
        Ausdruck_Int.speichern(cy, writer)
        Ausdruck_Int.speichern(rad, writer)
        Ausdruck_Int.speichern(startwinkel, writer)
        Ausdruck_Int.speichern(deltawinkel, writer)
        If radY Is Nothing Then
            writer.Write(0)
        Else
            writer.Write(1)
            Ausdruck_Int.speichern(radY, writer)
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Arc
        Dim line_around As Boolean = reader.ReadBoolean()
        Dim use_fillColor_from_linestyle As Boolean = reader.ReadBoolean()
        Dim cx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim cy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim rad As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim startwinkel As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim deltawinkel As Ausdruck_Int = Ausdruck_Int.laden(reader, version)

        Dim radYmode As Integer = reader.ReadInt32()
        If radYmode = 0 Then
            Return New Precompiled_Arc(cx, cy, rad, startwinkel, deltawinkel, line_around, use_fillColor_from_linestyle)
        ElseIf radYmode = 1 Then
            Dim radY As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
            Return New Precompiled_Arc(cx, cy, rad, radY, startwinkel, deltawinkel, line_around, use_fillColor_from_linestyle)
        Else
            Throw New Exception("Fehler L0001: Falscher 'radYmode' beim Laden des Kreisbogens")
        End If
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If line_around Then
            line = "pie("
        Else
            line = "arc("
        End If
        line &= Ausdruck_Int.export(cx, writer).str & ", "
        line &= Ausdruck_Int.export(cy, writer).str & ", "
        line &= Ausdruck_Int.export(rad, writer).str & ", "
        If radY IsNot Nothing Then
            line &= Ausdruck_Int.export(radY, writer).str & ", "
        End If
        line &= Ausdruck_Int.export(startwinkel, writer).str & ", "
        line &= Ausdruck_Int.export(deltawinkel, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
