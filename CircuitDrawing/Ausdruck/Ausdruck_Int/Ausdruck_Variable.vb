Imports System.IO

Public Class Ausdruck_Variable
    Inherits Ausdruck_Int

    Private var_index As Integer
    Private var_intern As Boolean

    Public Sub New(var_index As Integer, var_intern As Boolean)
        Me.var_index = var_index
        Me.var_intern = var_intern
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        If var_intern Then
            If args.vars_intern(var_index) Is Nothing Then
                Return 0
            End If
            If TypeOf args.vars_intern(var_index) Is VariablenWertInt Then
                Return DirectCast(args.vars_intern(var_index), VariablenWertInt).wert
            End If
            Throw New Exception("Falsche zuordnung der Variable. Es wird ein Integer erwartet.")
        Else
            Return DirectCast(args.params(var_index), ParamInt).myVal
        End If
    End Function

    Public Function istGleich(var2 As Ausdruck_Variable) As Boolean
        Return Me.var_intern = var2.var_intern AndAlso Me.var_index = var2.var_index
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(var_index)
        writer.Write(var_intern)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Variable
        Dim var_index As Integer = reader.ReadInt32()
        Dim var_intern As Boolean = reader.ReadBoolean()
        Return New Ausdruck_Variable(var_index, var_intern)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        If var_intern Then
            Return New Export_AusdruckInt("v" & var_index, Export_AusdruckInt.Ausdruck_Art.Atom)
        Else
            Return New Export_AusdruckInt("get(""" & writer.parameter(var_index).getName() & """)", Export_AusdruckInt.Ausdruck_Art.Atom)
        End If
    End Function

    Public Function isParameter_param(writer As Export_StreamWriter) As Boolean
        If var_intern Then Return False
        Return TypeOf writer.parameter(var_index) Is TemplateParameter_Param
    End Function

    Public Function getVarIndex() As Integer
        Return var_index
    End Function
End Class
