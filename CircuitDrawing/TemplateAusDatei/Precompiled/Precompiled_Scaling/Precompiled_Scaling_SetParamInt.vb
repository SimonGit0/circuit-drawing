Imports System.IO

Public Class Precompiled_Scaling_SetParamInt
    Inherits Precompiled_ScalingBefehl

    Private paramNr As Integer
    Private value As Ausdruck_Int

    Public Sub New(paramNr As Integer, value As Ausdruck_Int)
        Me.paramNr = paramNr
        Me.value = value
    End Sub

    Public Overrides Function Compile(args As AusrechnenArgs, erg As Precompiled_Scaling_CompileArgs) As Boolean
        Dim neuerWert As Integer = args.ausrechnen(value)
        If TypeOf erg.paramEinstellungen(paramNr) Is ParamInt Then
            Dim änderung As Boolean = DirectCast(erg.paramEinstellungen(paramNr), ParamInt).myVal <> neuerWert
            DirectCast(erg.paramEinstellungen(paramNr), ParamInt).myVal = neuerWert
            Return änderung
        End If
        Return False
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(paramNr)
        Ausdruck_Int.speichern(value, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Scaling_SetParamInt
        Dim paramNr As Integer = reader.ReadInt32()
        Dim value As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Precompiled_Scaling_SetParamInt(paramNr, value)
    End Function

    Public Overrides Sub export(writer As Export_StreamWriter)
        Dim line As String = "set_param("
        line &= """" & writer.parameter(paramNr + writer.extraParameterAmAnfang).getName().get_ID() & """, "
        line &= Ausdruck_Int.export(value, writer).str & ")"
        writer.WriteLine(line)
    End Sub
End Class
