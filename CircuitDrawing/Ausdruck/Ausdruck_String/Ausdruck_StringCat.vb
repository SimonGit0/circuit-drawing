Imports System.IO
Imports System.Text
Public Class Ausdruck_StringCat
    Inherits AusdruckString

    Private strs() As AusdruckString

    Public Sub New(strs() As AusdruckString)
        Me.strs = strs
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As AusdruckString
        Dim kannDirektZusammenfassen As Boolean = True
        For i As Integer = 0 To strs.Length - 1
            strs(i) = strs(i).AusrechnenSoweitMöglich()
            If TypeOf strs(i) IsNot AusdruckString_Konstante Then
                kannDirektZusammenfassen = False
            End If
        Next
        If kannDirektZusammenfassen Then
            Return New AusdruckString_Konstante(Me.Ausrechnen(Nothing))
        Else
            Return Me
        End If
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As String
        Dim erg As New StringBuilder()
        For i As Integer = 0 To strs.Length - 1
            erg.Append(strs(i).Ausrechnen(args))
        Next
        Return erg.ToString()
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(strs.Length)
        For i As Integer = 0 To strs.Length - 1
            speichern(strs(i), writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_StringCat
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Parameter bei 'strcat' muss größer als Null sein")
        Dim strs(anzahl - 1) As AusdruckString
        For i As Integer = 0 To anzahl - 1
            strs(i) = AusdruckString.laden(reader, version)
        Next
        Return New Ausdruck_StringCat(strs)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckString
        Dim str As String = "strcat("
        For i As Integer = 0 To strs.Length - 2
            str &= export(strs(i), writer).str & ", "
        Next
        str &= export(strs(strs.Length - 1), writer).str & ")"
        Return New Export_AusdruckString(str, Export_AusdruckString.Ausdruck_Art.Atom)
    End Function
End Class
