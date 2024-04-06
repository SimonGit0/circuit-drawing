Imports System.IO
Public Class Ausdruck_StringLength
    Inherits Ausdruck_Int

    Private mystr As AusdruckString
    Public Sub New(mystr As AusdruckString)
        Me.mystr = mystr
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        mystr = mystr.AusrechnenSoweitMöglich()
        If TypeOf mystr Is AusdruckString_Konstante Then
            Return New Ausdruck_Konstante(Me.Ausrechnen(Nothing))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Return mystr.Ausrechnen(args).Length
    End Function

    Public Sub save(writer As BinaryWriter)
        AusdruckString.speichern(mystr, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_StringLength
        Return New Ausdruck_StringLength(AusdruckString.laden(reader, version))
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Return New Export_AusdruckInt("length(" & AusdruckString.export(mystr, writer).str & ")", Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function
End Class
