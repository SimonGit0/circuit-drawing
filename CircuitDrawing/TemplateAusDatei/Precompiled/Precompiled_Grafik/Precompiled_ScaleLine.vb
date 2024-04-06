Imports System.IO

Public Class Precompiled_ScaleLine
    Inherits Precompiled_Grafik

    Private x1, y1, x2, y2, vx, vy, min, step_, max As Ausdruck_Int
    Private callbackNr As Integer

    Public Sub New(x1 As Ausdruck_Int, y1 As Ausdruck_Int, x2 As Ausdruck_Int, y2 As Ausdruck_Int, vx As Ausdruck_Int, vy As Ausdruck_Int, min As Ausdruck_Int, step_ As Ausdruck_Int, max As Ausdruck_Int, callbackNr As Integer)
        Me.x1 = x1
        Me.y1 = y1
        Me.x2 = x2
        Me.y2 = y2
        Me.vx = vx
        Me.vy = vy
        Me.min = min
        Me.max = max
        Me.step_ = step_
        Me.callbackNr = callbackNr
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim p1 As New Point(args.ausrechnen(x1), args.ausrechnen(y1))
        Dim p2 As New Point(args.ausrechnen(x2), args.ausrechnen(y2))
        Dim vec As New Point(args.ausrechnen(vx), args.ausrechnen(vy))
        Dim s As New ScalingLinie(p1, p2, vec, args.ausrechnen(min), args.ausrechnen(step_), args.ausrechnen(max), callbackNr)
        erg.addScaling(s)
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(callbackNr)
        Ausdruck_Int.speichern(x1, writer)
        Ausdruck_Int.speichern(y1, writer)
        Ausdruck_Int.speichern(x2, writer)
        Ausdruck_Int.speichern(y2, writer)
        Ausdruck_Int.speichern(vx, writer)
        Ausdruck_Int.speichern(vy, writer)
        Ausdruck_Int.speichern(min, writer)
        Ausdruck_Int.speichern(step_, writer)
        Ausdruck_Int.speichern(max, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_ScaleLine
        Dim callbackNr As Integer = reader.ReadInt32()
        Dim x1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim x2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim min As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim step_ As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim max As Ausdruck_Int = Ausdruck_Int.laden(reader, version)

        Return New Precompiled_ScaleLine(x1, y1, x2, y2, vx, vy, min, step_, max, callbackNr)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "scale_line("
        line &= Ausdruck_Int.export(x1, writer).str & ", "
        line &= Ausdruck_Int.export(y1, writer).str & ", "
        line &= Ausdruck_Int.export(x2, writer).str & ", "
        line &= Ausdruck_Int.export(y2, writer).str & ", "
        line &= Ausdruck_Int.export(vx, writer).str & ", "
        line &= Ausdruck_Int.export(vy, writer).str & ", "
        line &= Ausdruck_Int.export(min, writer).str & ", "
        line &= Ausdruck_Int.export(step_, writer).str & ", "
        line &= Ausdruck_Int.export(max, writer).str & ", "

        line &= writer.precompiled_template.getScaling(callbackNr).name & ")"
        writer.WriteLine(line)
    End Sub
End Class
