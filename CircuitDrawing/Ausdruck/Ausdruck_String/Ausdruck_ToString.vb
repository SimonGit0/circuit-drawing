Imports System.IO

Public Class Ausdruck_ToString
    Inherits AusdruckString

    Private ausdruck As Ausdruck
    Public Sub New(aus As Ausdruck)
        Me.ausdruck = aus
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As AusdruckString
        ausdruck = ausdruck.vereinfacheSoweitMöglich()

        If TypeOf ausdruck Is AusdruckString Then
            Return DirectCast(ausdruck, AusdruckString)
        End If
        If TypeOf ausdruck Is Ausdruck_Konstante Then
            Return New AusdruckString_Konstante(DirectCast(ausdruck, Ausdruck_Konstante).Ausrechnen(Nothing).ToString())
        End If
        If TypeOf ausdruck Is Ausdruck_Boolean_Konstante Then
            If DirectCast(ausdruck, Ausdruck_Boolean_Konstante).Ausrechnen(Nothing) Then
                Return New AusdruckString_Konstante("True")
            Else
                Return New AusdruckString_Konstante("False")
            End If
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As String
        If TypeOf ausdruck Is Ausdruck_Int Then
            Return DirectCast(ausdruck, Ausdruck_Int).Ausrechnen(args).ToString()
        ElseIf TypeOf ausdruck Is Ausdruck_Boolean Then
            If DirectCast(ausdruck, Ausdruck_Boolean).Ausrechnen(args) Then
                Return "True"
            Else
                Return "False"
            End If
        ElseIf TypeOf ausdruck Is AusdruckString Then
            Return DirectCast(ausdruck, AusdruckString).Ausrechnen(args)
        ElseIf TypeOf ausdruck Is Ausdruck_Pfeil Then
            Dim pfeil As ParamArrow = DirectCast(ausdruck, Ausdruck_Pfeil).Ausrechnen(args)
            Return pfeil.pfeilArt.ToString()
        Else
            Throw New NotImplementedException("Dieser Ausdruck kann nicht in einen String konvertiert werden.")
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        Ausdruck.speichernAllgemein(ausdruck, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_ToString
        Dim aus As Ausdruck = Ausdruck.ladenAllgemein(reader, version)
        Return New Ausdruck_ToString(aus)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckString
        Dim str As String = "toStr("
        str &= Ausdruck.exportAllgemein(ausdruck, writer) & ")"
        Return New Export_AusdruckString(str, Export_AusdruckString.Ausdruck_Art.Atom)
    End Function
End Class
