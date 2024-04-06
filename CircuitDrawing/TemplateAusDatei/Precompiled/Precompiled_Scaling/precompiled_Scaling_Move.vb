Imports System.IO

Public Class precompiled_Scaling_Move
    Inherits Precompiled_ScalingBefehl

    Private dx As Ausdruck_Int
    Private dy As Ausdruck_Int

    Public Sub New(dx As Ausdruck_Int, dy As Ausdruck_Int)
        Me.dx = dx
        Me.dy = dy
    End Sub

    Public Overrides Function Compile(args As AusrechnenArgs, erg As Precompiled_Scaling_CompileArgs) As Boolean
        Dim dx As Integer = args.ausrechnen(Me.dx)
        Dim dy As Integer = args.ausrechnen(Me.dy)
        erg.move(dx, dy)
        Return False
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(dx, writer)
        Ausdruck_Int.speichern(dy, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As precompiled_Scaling_Move
        Dim dx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim dy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New precompiled_Scaling_Move(dx, dy)
    End Function

    Public Overrides Sub export(writer As Export_StreamWriter)
        writer.WriteLine("move(" & Ausdruck_Int.export(dx, writer).str & ", " & Ausdruck_Int.export(dy, writer).str & ")")
    End Sub
End Class
