Public Class GravityPoint
    Implements IComparable(Of GravityPoint)

    Public posSnap As Integer 'Die Koordinate an der es gesnappt werden soll
    Public posAndereKoordinate As Integer 'Die andere Koordinate

    Public Xplus, Xminus, Yplus, Yminus As Integer 'je größer desto schlechter ist es in diese richtung zu snappen

    Public Sub New(posSnap As Integer, posAndere As Integer, Xplus As Integer, Xminus As Integer, Yplus As Integer, Yminus As Integer)
        Me.posSnap = posSnap
        Me.posAndereKoordinate = posAndere
        Me.Xplus = Xplus
        Me.Xminus = Xminus
        Me.Yplus = Yplus
        Me.Yminus = Yminus
    End Sub

    Public Function CompareTo(other As GravityPoint) As Integer Implements IComparable(Of GravityPoint).CompareTo
        Return Me.posSnap.CompareTo(other.posSnap)
    End Function

    Public Function copy() As GravityPoint
        Return New GravityPoint(posSnap, posAndereKoordinate, Xplus, Xminus, Yplus, Yminus)
    End Function

    Public Function getPointX() As Point
        Return New Point(posSnap, posAndereKoordinate)
    End Function

    Public Function getPointX(dx As Integer, dy As Integer) As Point
        Return New Point(posSnap + dx, posAndereKoordinate + dy)
    End Function

    Public Function getPointY() As Point
        Return New Point(posAndereKoordinate, posSnap)
    End Function

    Public Function getPointY(dx As Integer, dy As Integer) As Point
        Return New Point(posAndereKoordinate + dx, posSnap + dy)
    End Function

    Public Function calcMalus(other As GravityPoint, malusFaktor As Integer) As Integer
        Dim malus, malusMirror As Integer
        malus = Math.Min(Me.Xplus + other.Xplus, Math.Min(Me.Xminus + other.Xminus, Math.Min(Me.Yminus + other.Yminus, Me.Yplus + other.Yplus)))
        If malus > 0 Then malus = 1
        malusMirror = Math.Min(Me.Xplus + other.Xminus, Math.Min(Me.Xminus + other.Xplus, Math.Min(Me.Yminus + other.Yplus, Me.Yplus + other.Yminus)))
        If malusMirror > 0 Then malusMirror = 1
        Return Math.Max(0, Math.Min(malus, malusMirror)) * malusFaktor
    End Function
End Class
