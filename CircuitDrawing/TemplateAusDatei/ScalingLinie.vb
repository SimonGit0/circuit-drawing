Public Class ScalingLinie
    Public p1 As Point
    Public p2 As Point
    Public vec As Point
    Public min, _step, max As Integer
    Public callbackNr As Integer
    Public Sub New(p1 As Point, p2 As Point, vec As Point, min As Integer, _step As Integer, max As Integer, callbackNr As Integer)
        Me.p1 = p1
        Me.p2 = p2
        Me.vec = vec
        Me.min = min
        Me.max = max
        Me._step = _step
        Me.callbackNr = callbackNr
    End Sub

    Public Sub transform(t As Transform)
        Me.p1 = t.transformPoint(p1)
        Me.p2 = t.transformPoint(p2)

        Dim v1 As Point = t.transformPoint(New Point(0, 0))
        Dim v2 As Point = t.transformPoint(vec)
        Me.vec = New Point(v2.X - v1.X, v2.Y - v1.Y)
    End Sub

    Public Function Clone() As ScalingLinie
        Return New ScalingLinie(p1, p2, vec, min, _step, max, callbackNr)
    End Function
End Class
