Imports System.IO
Public Class Precompiled_ParamInvisible
    Inherits Precompiled_Grafik

    Private paramNr As Integer
    Private visible As Boolean

    Public Sub New(paramNr As Integer, visible As Boolean)
        Me.paramNr = paramNr
        Me.visible = visible
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.paramVisible(paramNr) = visible
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(paramNr)
        writer.Write(visible)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_ParamInvisible
        Dim nr As Integer = reader.ReadInt32()
        Dim visible As Boolean = reader.ReadBoolean()
        Return New Precompiled_ParamInvisible(nr, visible)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If visible = False Then
            line = "invisible("
        Else
            line = "visible("
        End If
        line &= """" & writer.parameter(paramNr).getName().get_ID() & """)"
        writer.WriteLine(line)
    End Sub
End Class
