Imports System.IO

Public Class Dot
    Inherits ElementLinestyle

    Private radius As Integer

    Public Sub New(ID As ULong, pos As Point, radius As Integer, linestyle_Color As Integer)
        MyBase.New(ID, 0)
        Me.position = pos
        Me.radius = radius
        Me.linestyle = linestyle_Color
    End Sub

    Public Overrides Sub speichern(writer As BinaryWriter)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub drehe(drehpunkt As Point, d As Drehmatrix)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        Dim e As New DO_Dot(position, radius, radius, Me.linestyle, False)
        Return e
    End Function

    Public Overrides Function getSelection() As Selection
        Throw New NotImplementedException()
    End Function

    Public Overrides Function NrOfSnappoints() As Integer
        Return 0
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Throw New NotImplementedException()
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Return New Dot(newID, position, radius, Me.linestyle)
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Dot Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Dot)
            If Me.position <> .position Then
                Return False
            End If
            If Me.radius <> .radius Then Return False
        End With
        Return True
    End Function
End Class
