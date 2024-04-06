Imports System.IO
Public Class Ausdruck_SqrtAbs
    Inherits Ausdruck_Int

    Private Q As Ausdruck_Int
    Private myart As art
    Public Sub New(Q As Ausdruck_Int, myart As art)
        Me.Q = Q
        Me.myart = myart
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        Q = Q.AusrechnenSoweitMöglich()
        If TypeOf Q Is Ausdruck_Konstante Then
            Return New Ausdruck_Konstante(Me.Ausrechnen(Nothing))
        Else
            Return Me
        End If
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim q As Long = Me.Q.Ausrechnen(args)
        If myart = art.SQRT Then
            Return CLng(Math.Sqrt(q))
        ElseIf myart = art.ABS Then
            Return Math.Abs(q)
        End If
        Throw New NotImplementedException("Unbekannte Funktion.")
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(CInt(myart))
        speichern(Q, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_SqrtAbs
        Dim a As art = CType(reader.ReadInt32(), art)
        Return New Ausdruck_SqrtAbs(laden(reader, version), a)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        If myart = art.SQRT Then
            Return New Export_AusdruckInt("sqrt(" & export(Q, writer).str & ")", Export_AusdruckInt.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckInt("abs(" & export(Q, writer).str & ")", Export_AusdruckInt.Ausdruck_Art.Atom)
        End If
    End Function

    Public Enum art
        SQRT = 0
        ABS = 1
    End Enum
End Class
