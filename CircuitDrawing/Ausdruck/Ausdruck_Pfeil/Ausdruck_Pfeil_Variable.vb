Imports System.IO
Public Class Ausdruck_Pfeil_Variable
    Inherits Ausdruck_Pfeil

    Private var_index As Integer
    Private var_intern As Boolean

    Public Sub New(var_index As Integer, var_intern As Boolean)
        Me.var_index = var_index
        Me.var_intern = var_intern
    End Sub

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As ParamArrow
        If var_intern Then
            Throw New NotImplementedException()
        Else
            Return DirectCast(args.params(var_index), ParamArrow).CopyPfeil()
        End If
    End Function

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Pfeil
        Return Me
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(var_index)
        writer.Write(var_intern)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Pfeil_Variable
        Dim var_index As Integer = reader.ReadInt32()
        Dim var_intern As Boolean = reader.ReadBoolean()
        Return New Ausdruck_Pfeil_Variable(var_index, var_intern)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckPfeil
        If var_intern Then
            Return New Export_AusdruckPfeil("v" & var_index, Export_AusdruckPfeil.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckPfeil("get(""" & writer.parameter(var_index).getName().get_ID() & """)", Export_AusdruckPfeil.Ausdruck_Art.Atom)
        End If
    End Function
End Class
