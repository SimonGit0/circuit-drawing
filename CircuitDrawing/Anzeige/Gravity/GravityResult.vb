Public Class GravityResult
    Public score As Integer
    Public delta As Integer
    Public malus As Integer
    Public AnzeigeLinieMin As GravityPoint
    Public AnzeigeLinieMax As GravityPoint
    Public refPoint As GravityPoint
    Public Sub New(score As Integer, malus As Integer, delta As Integer, refPoint As GravityPoint, minPoint As GravityPoint, maxPoint As GravityPoint)
        Me.score = score
        Me.malus = malus
        Me.delta = delta
        Me.AnzeigeLinieMin = minPoint
        Me.AnzeigeLinieMax = maxPoint
        Me.refPoint = refPoint
    End Sub
End Class
