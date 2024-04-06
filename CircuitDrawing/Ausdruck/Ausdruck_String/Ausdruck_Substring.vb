Imports System.IO
Public Class Ausdruck_Substring
    Inherits AusdruckString

    Private myStr As AusdruckString
    Private start As Ausdruck_Int
    Private len As Ausdruck_Int

    Public Sub New(myStr As AusdruckString, start As Ausdruck_Int, len As Ausdruck_Int)
        Me.myStr = myStr
        Me.start = start
        Me.len = len
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As AusdruckString
        myStr = myStr.AusrechnenSoweitMöglich()
        start = start.AusrechnenSoweitMöglich()
        len = len.AusrechnenSoweitMöglich()

        If TypeOf myStr Is AusdruckString_Konstante AndAlso TypeOf start Is Ausdruck_Konstante AndAlso TypeOf len Is Ausdruck_Konstante Then
            Return New AusdruckString_Konstante(Me.Ausrechnen(Nothing))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As String
        Dim str As String = myStr.Ausrechnen(args)
        Dim start As Long = Me.start.Ausrechnen(args)
        Dim len As Long = Me.len.Ausrechnen(args)

        If start < 0 OrElse len < 1 OrElse start + len > str.Length Then
            Throw New ArgumentException("Falsche Parameter bei 'substring(""str"", start, len)'")
        End If

        Return str.Substring(CInt(start), CInt(len))
    End Function

    Public Sub save(writer As BinaryWriter)
        AusdruckString.speichern(myStr, writer)
        Ausdruck_Int.speichern(start, writer)
        Ausdruck_Int.speichern(len, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Substring
        Dim mystr As AusdruckString = laden(reader, version)
        Dim start As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim len As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Ausdruck_Substring(mystr, start, len)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckString
        Dim str As String = "subsrt("
        str &= export(myStr, writer).str & ", "
        str &= Ausdruck_Int.export(start, writer).str & ", "
        str &= Ausdruck_Int.export(len, writer).str & ")"
        Return New Export_AusdruckString(str, Export_AusdruckString.Ausdruck_Art.Atom)
    End Function
End Class
