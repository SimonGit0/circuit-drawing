Imports System.IO
Public Class AusdruckBoolean_Variable
    Inherits Ausdruck_Boolean

    Private var_index As Integer
    Private var_intern As Boolean

    Public Sub New(var_index As Integer, var_intern As Boolean)
        Me.var_index = var_index
        Me.var_intern = var_intern
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        If var_intern Then
            If args.vars_intern(var_index) Is Nothing Then
                Return False
            End If
            If TypeOf args.vars_intern(var_index) Is VariablenWertBoolean Then
                Return DirectCast(args.vars_intern(var_index), VariablenWertBoolean).wert
            End If
            Throw New Exception("Falsche zuordnung der Variable. Es wird ein Boolean erwartet.")
        Else
            Throw New NotImplementedException("Ein param_boolean wird nicht unterstützt.")
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(var_index)
        writer.Write(var_intern)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As AusdruckBoolean_Variable
        Dim var_index As Integer = reader.ReadInt32()
        Dim var_intern As Boolean = reader.ReadBoolean()
        Return New AusdruckBoolean_Variable(var_index, var_intern)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        If var_intern Then
            Return New Export_AusdruckBoolean("v" & var_index, Export_AusdruckBoolean.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckBoolean("get(""" & writer.parameter(var_index).getName().get_ID() & """)", Export_AusdruckBoolean.Ausdruck_Art.Atom)
        End If
    End Function
End Class
