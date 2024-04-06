Imports System.IO
Public Class Precompiled_SetVarGrafik
    Inherits Precompiled_Grafik

    Private myBefehl As Precompiled_SetVar

    Public Sub New(varNr As Integer, value As Ausdruck, art As VariableEinlesen.VariableArt)
        Me.myBefehl = New Precompiled_SetVar(varNr, value, art)
    End Sub

    Private Sub New(b As Precompiled_SetVar)
        Me.myBefehl = b
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Me.myBefehl.compile(args)
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Me.myBefehl.speichern(writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_SetVarGrafik
        If version >= 19 Then
            Dim b As Precompiled_SetVar = Precompiled_SetVar.laden(reader, version)
            Return New Precompiled_SetVarGrafik(b)
        Else
            Dim nr As Integer = reader.ReadInt32()
            Dim value As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
            Return New Precompiled_SetVarGrafik(nr, value, VariableEinlesen.VariableArt.Int_)
        End If
    End Function

    Public Sub export(writer As Export_StreamWriter)
        myBefehl.export(writer)
    End Sub
End Class
