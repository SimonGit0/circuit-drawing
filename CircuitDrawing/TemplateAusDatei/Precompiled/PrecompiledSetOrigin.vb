Imports System.IO
Public Class PrecompiledSetOrigin
    Inherits Precompiled_Befehl

    Public originX As Ausdruck_Int
    Public originY As Ausdruck_Int

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.originX = x
        Me.originY = y
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(originX, writer)
        Ausdruck_Int.speichern(originY, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As PrecompiledSetOrigin
        Dim x As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New PrecompiledSetOrigin(x, y)
    End Function

    Public Sub exportiere(writer As Export_StreamWriter)
        Dim line As String = "origin("
        line &= Ausdruck_Int.export(originX, writer).str & ", "
        line &= Ausdruck_Int.export(originY, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
