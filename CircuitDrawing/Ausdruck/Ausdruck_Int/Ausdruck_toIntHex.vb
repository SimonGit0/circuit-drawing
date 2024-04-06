Imports System.IO
Public Class Ausdruck_ToIntHex
    Inherits Ausdruck_Int

    Private ausdruck As AusdruckString
    Public Sub New(aus As AusdruckString)
        Me.ausdruck = aus
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        ausdruck = ausdruck.AusrechnenSoweitMöglich()
        If TypeOf ausdruck Is AusdruckString_Konstante Then
            Return New Ausdruck_Konstante(Ausrechnen(Nothing))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim str As String = DirectCast(ausdruck, AusdruckString).Ausrechnen(args)
        Dim l As Long
        Try
            l = Convert.ToInt64(str, 16)
        Catch
            Throw New Exception("Der String '" & str & "' ist keine gültige Hexadezimalzahl.")
        End Try
        Return l
    End Function

    Public Sub save(writer As BinaryWriter)
        AusdruckString.speichern(ausdruck, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_ToIntHex
        Dim ausdruck As AusdruckString = AusdruckString.laden(reader, version)
        Return New Ausdruck_ToIntHex(ausdruck)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Return New Export_AusdruckInt("toIntHex(" & AusdruckString.export(ausdruck, writer).str & ")", Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function
End Class
