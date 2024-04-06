Imports System.IO
Public Class Precompiled_SetLineScaling
    Inherits Precompiled_Grafik

    Private scaling As Ausdruck_Int

    Public Sub New(scaling As Ausdruck_Int)
        Me.scaling = scaling
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim s As Integer = args.ausrechnen(Me.scaling)
        erg.set_scaling(s / 100.0F)
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(scaling, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_SetLineScaling
        Return New Precompiled_SetLineScaling(Ausdruck_Int.laden(reader, version))
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "lw("
        line &= Ausdruck_Int.export(scaling, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
