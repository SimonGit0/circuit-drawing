Imports System.IO
Public Class Ausdruck_Trigonometry
    Inherits Ausdruck_Int

    Private alpha As Ausdruck_Int
    Private len As Ausdruck_Int
    Private myArt As Art
    Public Sub New(alpha As Ausdruck_Int, len As Ausdruck_Int, myArt As Art)
        Me.alpha = alpha
        Me.len = len
        Me.myArt = myArt
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        alpha = alpha.AusrechnenSoweitMöglich()
        len = len.AusrechnenSoweitMöglich()
        If TypeOf alpha Is Ausdruck_Konstante AndAlso TypeOf len Is Ausdruck_Konstante Then
            Return New Ausdruck_Konstante(Me.Ausrechnen(Nothing))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim alpha As Long = Me.alpha.Ausrechnen(args)
        Dim len As Long = Me.len.Ausrechnen(args)
        Select Case myArt
            Case Art.Sin
                Return CLng(Math.Round(Math.Sin(alpha * Math.PI / 180) * len))
            Case Art.Cos
                Return CLng(Math.Round(Math.Cos(alpha * Math.PI / 180) * len))
            Case Art.Tan
                Return CLng(Math.Round(Math.Cos(alpha * Math.PI / 180) * len))
            Case Art.Cot
                Return CLng(Math.Round(len / Math.Tan(alpha * Math.PI / 180)))
            Case Art.Sec
                Return CLng(Math.Round(len / Math.Cos(alpha * Math.PI / 180)))
            Case Art.Csc
                Return CLng(Math.Round(len / Math.Sin(alpha * Math.PI / 180)))
        End Select
        Throw New NotImplementedException("Unbekannte Funktion.")
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(CInt(myArt))
        speichern(alpha, writer)
        speichern(len, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Trigonometry
        Dim a As Art = CType(reader.ReadInt32(), Art)
        Dim alpha As Ausdruck_Int = laden(reader, version)
        Dim len As Ausdruck_Int = laden(reader, version)
        Return New Ausdruck_Trigonometry(alpha, len, a)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Dim erg As String
        Select Case myArt
            Case Art.Sin
                erg = "sin("
            Case Art.Cos
                erg = "cos("
            Case Art.Tan
                erg = "tan("
            Case Art.Cot
                erg = "cot("
            Case Art.Csc
                erg = "csc("
            Case Art.Sec
                erg = "sec("
            Case Else
                Throw New NotImplementedException("Dieser Ausdruck kann nicht exportiert werden")
        End Select
        erg &= export(alpha, writer).str & ", " & export(len, writer).str & ")"
        Return New Export_AusdruckInt(erg, Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function

    Public Enum Art
        Sin = 0
        Cos = 1
        Tan = 2
        Cot = 3
        Csc = 4
        Sec = 5
    End Enum
End Class
