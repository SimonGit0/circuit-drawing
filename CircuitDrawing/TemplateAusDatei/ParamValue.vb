Imports System.IO
Public MustInherit Class ParamValue

    Public Shared Sub speichern(value As ParamValue, writer As binarywriter)
        If TypeOf value Is ParamInt Then
            writer.Write(1)
            DirectCast(value, ParamInt).save(writer)
        ElseIf TypeOf value Is ParamArrow Then
            writer.Write(2)
            DirectCast(value, ParamArrow).save(writer)
        ElseIf TypeOf value Is ParamString Then
            writer.Write(3)
            DirectCast(value, ParamString).save(writer)
        Else
            Throw New NotImplementedException("Fehler P2000: Dieser Parameterwert kann nicht gespeichert werden.")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As ParamValue
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1
                Return ParamInt.load(reader, version)
            Case 2
                Return ParamArrow.load(reader, version)
            Case 3
                Return ParamString.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L2000: Dieser Parameterwert kann nicht geladen werden.")
        End Select
    End Function

    Public MustOverride Function Copy() As ParamValue

    Public MustOverride Function isEqual(param2 As ParamValue) As Boolean
End Class

Public Class ParamInt
    Inherits ParamValue

    Public myVal As Integer
    Public Sub New(v As Integer)
        Me.myVal = v
    End Sub

    Public Overrides Function Copy() As ParamValue
        Return New ParamInt(myVal)
    End Function

    Public Overrides Function isEqual(param2 As ParamValue) As Boolean
        If TypeOf param2 IsNot ParamInt Then Return False
        Return myVal = DirectCast(param2, ParamInt).myVal
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(myVal)
    End Sub
    Public Shared Function load(reader As BinaryReader, version As Integer) As ParamInt
        Return New ParamInt(reader.ReadInt32())
    End Function
End Class

Public Class ParamArrow
    Inherits ParamValue

    Public pfeilArt As Short
    Public pfeilSize As UShort
    Public Sub New(pfeilArt As Short, pfeilSize As UShort)
        Me.pfeilArt = pfeilArt
        Me.pfeilSize = pfeilSize
    End Sub

    Public Overrides Function Copy() As ParamValue
        Return Me.CopyPfeil()
    End Function

    Public Function CopyPfeil() As ParamArrow
        Return New ParamArrow(pfeilArt, pfeilSize)
    End Function

    Public Overrides Function isEqual(param2 As ParamValue) As Boolean
        If TypeOf param2 IsNot ParamArrow Then Return False
        Return pfeilArt = DirectCast(param2, ParamArrow).pfeilArt AndAlso pfeilSize = DirectCast(param2, ParamArrow).pfeilSize
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(pfeilArt)
        writer.Write(pfeilSize)
    End Sub
    Public Shared Function load(reader As BinaryReader, version As Integer) As ParamArrow
        Dim art As Short = reader.ReadInt16()
        Dim size As UShort = reader.ReadUInt16()
        Return New ParamArrow(art, size)
    End Function
End Class

Public Class ParamString
    Inherits ParamValue

    Public myVal As String
    Public Sub New(v As String)
        Me.myVal = v
    End Sub

    Public Overrides Function Copy() As ParamValue
        Return New ParamString(myVal)
    End Function

    Public Overrides Function isEqual(param2 As ParamValue) As Boolean
        If TypeOf param2 IsNot ParamString Then Return False
        Return myVal = DirectCast(param2, ParamString).myVal
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(myVal)
    End Sub
    Public Shared Function load(reader As BinaryReader, version As Integer) As ParamString
        Return New ParamString(reader.ReadString())
    End Function
End Class
