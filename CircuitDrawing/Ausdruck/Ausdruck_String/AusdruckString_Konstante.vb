Imports System.IO
Public Class AusdruckString_Konstante
    Inherits AusdruckString

    Private myString As String

    Public Sub New(str As String)
        Me.myString = str
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As AusdruckString
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As String
        Return myString
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(myString)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As AusdruckString_Konstante
        Return New AusdruckString_Konstante(reader.ReadString())
    End Function

    Public Function exportiere() As Export_AusdruckString
        Return New Export_AusdruckString("""" & myString & """", Export_AusdruckString.Ausdruck_Art.Atom)
    End Function

    Public Overrides Function ist_Konstante() As Boolean
        Return True
    End Function
End Class
