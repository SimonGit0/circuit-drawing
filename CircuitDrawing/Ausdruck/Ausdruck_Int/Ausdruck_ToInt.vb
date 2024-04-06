Imports System.IO
Public Class Ausdruck_ToInt
    Inherits Ausdruck_Int

    Private ausdruck As Ausdruck
    Public Sub New(aus As Ausdruck)
        Me.ausdruck = aus
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        ausdruck = ausdruck.vereinfacheSoweitMöglich()
        If ausdruck.ist_Konstante() Then
            Return New Ausdruck_Konstante(Ausrechnen(Nothing))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        If TypeOf ausdruck Is Ausdruck_Int Then
            Return DirectCast(ausdruck, Ausdruck_Int).Ausrechnen(args)
        ElseIf TypeOf ausdruck Is Ausdruck_Boolean Then
            If DirectCast(ausdruck, Ausdruck_Boolean).Ausrechnen(args) Then
                Return 1
            Else
                Return 0
            End If
        ElseIf TypeOf ausdruck Is AusdruckString Then
            Dim str As String = DirectCast(ausdruck, AusdruckString).Ausrechnen(args)
            Dim l As Long
            Try
                l = CLng(str)
            Catch
                Throw New Exception("Der String '" & str & "' kann nicht in einen Integer konvertiert werden.")
            End Try
            Return l
        ElseIf TypeOf ausdruck Is Ausdruck_Pfeil Then
            Dim pfeil As ParamArrow = DirectCast(ausdruck, Ausdruck_Pfeil).Ausrechnen(args)
            Return pfeil.pfeilArt
        Else
            Throw New NotImplementedException("Dieser Ausdruck kann nicht in einen Integer konvertiert werden.")
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        Ausdruck.speichernAllgemein(ausdruck, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_ToInt
        Dim ausdruck As Ausdruck = Ausdruck.ladenAllgemein(reader, version)
        Return New Ausdruck_ToInt(ausdruck)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Return New Export_AusdruckInt("toInt(" & exportAllgemein(ausdruck, writer) & ")", Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function
End Class
