Imports System.IO
Public Class Ausdruck_Konstante
    Inherits Ausdruck_Int

    Private konstante As Long

    Public Sub New(konst As Long)
        Me.konstante = konst
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Return konstante
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(konstante)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Konstante
        Return New Ausdruck_Konstante(reader.ReadInt64())
    End Function

    Public Function exportiere() As Export_AusdruckInt
        Return New Export_AusdruckInt(konstante.ToString(), Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function

    Public Overrides Function ist_Konstante() As Boolean
        Return True
    End Function
End Class
