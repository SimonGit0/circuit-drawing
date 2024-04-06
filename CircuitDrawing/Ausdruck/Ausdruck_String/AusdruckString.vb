Imports System.IO
Public MustInherit Class AusdruckString
    Inherits Ausdruck

    Public MustOverride Function Ausrechnen(args As AusrechnenArgs) As String

    Public MustOverride Function AusrechnenSoweitMöglich() As AusdruckString

    Public Overrides Function vereinfacheSoweitMöglich() As Ausdruck
        Return Me.AusrechnenSoweitMöglich()
    End Function

    Public Shared Sub speichern(str As AusdruckString, writer As binaryWriter)
        If TypeOf str Is AusdruckString_Konstante Then
            writer.Write(1)
            DirectCast(str, AusdruckString_Konstante).save(writer)
        ElseIf TypeOf str Is AusdruckString_Variable Then
            writer.Write(2)
            DirectCast(str, AusdruckString_Variable).save(writer)
        ElseIf TypeOf str Is Ausdruck_Substring Then
            writer.Write(3)
            DirectCast(str, Ausdruck_Substring).save(writer)
        ElseIf TypeOf str Is Ausdruck_StringCat Then
            writer.Write(4)
            DirectCast(str, Ausdruck_StringCat).save(writer)
        Else
            Throw New NotImplementedException("Fehler P0003: Kann diesen Ausdruck nicht speichern")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As AusdruckString
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1 'AusdruckString_Konstante
                Return AusdruckString_Konstante.load(reader, version)
            Case 2 'AusdruckString_Variable
                Return AusdruckString_Variable.load(reader, version)
            Case 3 'Ausdruck_Substring
                Return Ausdruck_Substring.load(reader, version)
            Case 4 'Ausdruck_StringCat
                Return Ausdruck_StringCat.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L0003: Kann diesen Ausdruck nicht laden")
        End Select
    End Function

    Public Shared Function export(ausdruck As AusdruckString, writer As Export_StreamWriter) As Export_AusdruckString
        If TypeOf ausdruck Is AusdruckString_Konstante Then
            Return DirectCast(ausdruck, AusdruckString_Konstante).exportiere()
        ElseIf TypeOf ausdruck Is AusdruckString_Variable Then
            Return DirectCast(ausdruck, AusdruckString_Variable).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Substring Then
            Return DirectCast(ausdruck, Ausdruck_Substring).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_StringCat Then
            Return DirectCast(ausdruck, Ausdruck_StringCat).exportiere(writer)
        Else
            Throw New NotImplementedException("Export für diesen Ausdruck nicht implementiert")
        End If
    End Function
End Class

Public Class Export_AusdruckString
    Public str As String
    Public art As Ausdruck_Art

    Public Sub New(str As String, art As Ausdruck_Art)
        Me.str = str
        Me.art = art
    End Sub

    Public Enum Ausdruck_Art
        Atom
    End Enum
End Class
