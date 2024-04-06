Imports System.IO
Public Class Snappoint
    Public p As Point

    Public Xminus As Integer
    Public Xplus As Integer
    Public Yminus As Integer
    Public Yplus As Integer

    Public ReadOnly KeinWireDot As Boolean

    Public Sub New(p As Point, Xminus As Integer, Xplus As Integer, Yminus As Integer, Yplus As Integer)
        Me.New(p, Xminus, Xplus, Yminus, Yplus, False)
    End Sub

    Public Sub New(p As Point, Xminus As Integer, Xplus As Integer, Yminus As Integer, Yplus As Integer, keinWireDot As Boolean)
        Me.p = p
        Me.KeinWireDot = keinWireDot
        Me.Xminus = Xminus
        Me.Xplus = Xplus
        Me.Yminus = Yminus
        Me.Yplus = Yplus
    End Sub

    Public Function isEqual(s As Snappoint) As Boolean
        If s.Xminus <> Xminus Then Return False
        If s.Xplus <> Xplus Then Return False
        If s.Yminus <> Yminus Then Return False
        If s.Yplus <> Yplus Then Return False
        Return Me.p.X = s.p.X AndAlso Me.p.Y = s.p.Y
    End Function

    Public Function Clone() As Snappoint
        Return New Snappoint(Me.p, Me.Xminus, Me.Xplus, Me.Yminus, Me.Yplus, Me.KeinWireDot)
    End Function

    Public Function isBestRichtung(vector As Point) As Boolean
        If vector.X = 0 Then
            If vector.Y > 0 Then
                Return Yplus <= Yminus AndAlso Yplus <= Xplus AndAlso Yplus <= Xminus
            ElseIf vector.Y < 0 Then
                Return Yminus <= Yplus AndAlso Yminus <= Xplus AndAlso Yminus <= Xminus
            Else
                'Vektor 0 ist komisch...
                Return False
            End If
        ElseIf vector.Y = 0 Then
            If vector.X > 0 Then
                Return Xplus <= Xminus AndAlso Xplus <= Yplus AndAlso Xplus <= Yminus
            ElseIf vector.X < 0 Then
                Return Xminus <= Xplus AndAlso Xminus <= Yplus AndAlso Xminus <= Yminus
            Else
                'Vektor 0 ist komisch...
                Return False
            End If
        Else
            'schräger Vektor ist nicht optimal!!
            Return False
        End If
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(p.X)
        writer.Write(p.Y)
        writer.Write(Xminus)
        writer.Write(Xplus)
        writer.Write(Yminus)
        writer.Write(Yplus)
        writer.Write(KeinWireDot)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As Snappoint
        Dim x As Integer = reader.ReadInt32()
        Dim y As Integer = reader.ReadInt32()
        Dim xminus As Integer = reader.ReadInt32()
        Dim xplus As Integer = reader.ReadInt32()
        Dim yminus As Integer = reader.ReadInt32()
        Dim yplus As Integer = reader.ReadInt32()
        Dim keinWireDot As Boolean = False
        If version >= 9 Then
            keinWireDot = reader.ReadBoolean()
        End If
        Return New Snappoint(New Point(x, y), xminus, xplus, yminus, yplus, keinWireDot)
    End Function
End Class