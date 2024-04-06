Imports System.IO
Public Class Ausdruck_Boolean_Konstante
    Inherits Ausdruck_Boolean

    Private value As Boolean
    Public Sub New(value As Boolean)
        Me.value = value
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        Return value
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(value)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Boolean_Konstante
        Return New Ausdruck_Boolean_Konstante(reader.ReadBoolean())
    End Function

    Public Function exportiere() As Export_AusdruckBoolean
        If value Then
            Return New Export_AusdruckBoolean("True", Export_AusdruckBoolean.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckBoolean("False", Export_AusdruckBoolean.Ausdruck_Art.Atom)
        End If
    End Function

    Public Overrides Function ist_Konstante() As Boolean
        Return True
    End Function
End Class
