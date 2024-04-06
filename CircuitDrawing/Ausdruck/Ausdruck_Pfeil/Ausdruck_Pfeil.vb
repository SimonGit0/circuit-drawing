Imports System.IO
Public MustInherit Class Ausdruck_Pfeil
    Inherits Ausdruck

    Public MustOverride Function Ausrechnen(args As AusrechnenArgs) As ParamArrow

    Public MustOverride Function AusrechnenSoweitMöglich() As Ausdruck_Pfeil

    Public Overrides Function vereinfacheSoweitMöglich() As Ausdruck
        Return Me.AusrechnenSoweitMöglich()
    End Function

#Region "Speichern, Laden, Exportieren"
    Public Shared Sub speichern(ausdruck As Ausdruck_Pfeil, writer As BinaryWriter)
        If TypeOf ausdruck Is Ausdruck_Pfeil_Variable Then
            writer.Write(1)
            DirectCast(ausdruck, Ausdruck_Pfeil_Variable).save(writer)
        Else
            Throw New NotImplementedException("Fehler P0004: Kann diesen Ausdruck nicht speichern")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Ausdruck_Pfeil
        Dim mode As Integer = reader.ReadInt32()
        Select Case mode
            Case 1 'Ausdruck_Pfeil_Variable
                Return Ausdruck_Pfeil_Variable.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L0004: Kann diesen Ausdruck nicht laden")
        End Select
    End Function

    Public Shared Sub speichern_kannNothingSein(ausdruck As Ausdruck_Pfeil, writer As BinaryWriter)
        If ausdruck Is Nothing Then
            writer.Write(0)
        Else
            writer.Write(1)
            speichern(ausdruck, writer)
        End If
    End Sub

    Public Shared Function laden_kannNothingSein(reader As BinaryReader, version As Integer) As Ausdruck_Pfeil
        Dim art As Integer = reader.ReadInt32()
        If art = 0 Then Return Nothing
        If art = 1 Then Return laden(reader, version)
        Throw New Exception("L0014: Kann diesen Ausdruck nicht laden")
    End Function

    Public Shared Function export(ausdruck As Ausdruck_Pfeil, writer As Export_StreamWriter) As Export_AusdruckPfeil
        If TypeOf ausdruck Is Ausdruck_Pfeil_Variable Then
            Return DirectCast(ausdruck, Ausdruck_Pfeil_Variable).exportiere(writer)
        Else
            Throw New NotImplementedException("Export für diesen Ausdruck nicht implementiert")
        End If
    End Function
#End Region

End Class

Public Class Export_AusdruckPfeil
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