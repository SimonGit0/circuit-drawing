Imports System.IO

Public Class AusdruckString_Variable
    Inherits AusdruckString

    Private var_index As Integer
    Private var_intern As Boolean

    Public Sub New(var_index As Integer, var_intern As Boolean)
        Me.var_index = var_index
        Me.var_intern = var_intern
    End Sub

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As String
        If var_intern Then
            If args.vars_intern(var_index) Is Nothing Then
                Return ""
            End If
            If TypeOf args.vars_intern(var_index) Is VariablenWertString Then
                Return DirectCast(args.vars_intern(var_index), VariablenWertString).wert
            End If
            Throw New Exception("Falsche zuordnung der Variable. Es wird ein String erwartet.")
        Else
            Return DirectCast(args.params(var_index), ParamString).myVal
        End If
    End Function

    Public Overrides Function AusrechnenSoweitMöglich() As AusdruckString
        Return Me
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(var_index)
        writer.Write(var_intern)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As AusdruckString_Variable
        Dim var_index As Integer = reader.ReadInt32()
        Dim var_intern As Boolean = reader.ReadBoolean()
        Return New AusdruckString_Variable(var_index, var_intern)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckString
        If var_intern Then
            Return New Export_AusdruckString("v" & var_index, Export_AusdruckString.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckString("get(""" & writer.parameter(var_index).getName().get_ID() & """)", Export_AusdruckString.Ausdruck_Art.Atom)
        End If
    End Function
End Class
