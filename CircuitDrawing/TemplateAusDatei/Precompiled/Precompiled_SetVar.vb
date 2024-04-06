Imports System.IO
Public Class Precompiled_SetVar
    Inherits Precompiled_Befehl

    Private varNr As Integer
    Private value As Ausdruck
    Private art As VariableEinlesen.VariableArt

    Public Sub New(varNr As Integer, value As Ausdruck, art As VariableEinlesen.VariableArt)
        Me.varNr = varNr
        Me.value = value
        Me.art = art
    End Sub

    Public Sub compile(args As AusrechnenArgs)
        Select Case art
            Case VariableEinlesen.VariableArt.Int_
                Dim value As Integer = args.ausrechnen(DirectCast(Me.value, Ausdruck_Int))
                args.vars_intern(varNr) = New VariablenWertInt(value)
            Case VariableEinlesen.VariableArt.String_
                Dim value As String = args.ausrechnen(DirectCast(Me.value, AusdruckString))
                args.vars_intern(varNr) = New VariablenWertString(value)
            Case VariableEinlesen.VariableArt.Boolean_
                Dim value As Boolean = args.ausrechnen(DirectCast(Me.value, Ausdruck_Boolean))
                args.vars_intern(varNr) = New VariablenWertBoolean(value)
        End Select
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(varNr)
        Ausdruck.speichernAllgemein(value, writer)
        writer.Write(CInt(art))
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_SetVar
        Dim nr As Integer = reader.ReadInt32()
        If version >= 19 Then
            Dim value As Ausdruck = Ausdruck.ladenAllgemein(reader, version)
            Dim art As VariableEinlesen.VariableArt = CType(reader.ReadInt32(), VariableEinlesen.VariableArt)
            If art <> value.getArt() Then
                Throw New Exception("Falscher Ausdruck ist gespeichert")
            End If
            Return New Precompiled_SetVar(nr, value, art)
        Else
            Dim value As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
            Return New Precompiled_SetVar(nr, value, VariableEinlesen.VariableArt.Int_)
        End If
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "var "
        Select Case art
            Case VariableEinlesen.VariableArt.Int_
                line &= "int "
            Case VariableEinlesen.VariableArt.Boolean_
                line &= "boolean "
            Case VariableEinlesen.VariableArt.String_
                line &= "string "
        End Select
        line &= "v" & varNr & " = "
        line &= Ausdruck.exportAllgemein(value, writer)
        writer.WriteLine(line)
    End Sub

End Class
