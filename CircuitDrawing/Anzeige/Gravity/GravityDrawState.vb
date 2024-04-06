Public Class GravityDrawState
    Private resultX As List(Of GravityResult)
    Private resultY As List(Of GravityResult)
    Private deltaX As Integer
    Private deltaY As Integer

    Public posResult As Point

    Public Sub New(resultX As List(Of GravityResult), resultY As List(Of GravityResult), deltaX As Integer, deltaY As Integer, posResult As Point)
        Me.resultX = resultX
        Me.resultY = resultY
        Me.deltaX = deltaX
        Me.deltaY = deltaY

        Me.posResult = posResult
    End Sub

    Public Sub draw(g As Graphics, sender As Vektor_Picturebox)
        For i As Integer = 0 To resultX.Count - 1
            Dim p1 As Point = resultX(i).AnzeigeLinieMax.getPointX()
            Dim p2 As Point = resultX(i).AnzeigeLinieMin.getPointX()
            Dim p3 As Point = resultX(i).refPoint.getPointX(deltaX, deltaY)

            Dim maxY As Integer = Math.Max(p1.Y, p3.Y)
            Dim minY As Integer = Math.Min(p2.Y, p3.Y)
            Dim p1f As PointF = sender.toPictureboxPoint(New Point(p3.X, minY))
            Dim p2f As PointF = sender.toPictureboxPoint(New Point(p3.X, maxY))
            g.DrawLine(Pens.Orange, p1f, p2f)
        Next
        For i As Integer = 0 To resultY.Count - 1
            Dim p1 As Point = resultY(i).AnzeigeLinieMax.getPointY()
            Dim p2 As Point = resultY(i).AnzeigeLinieMin.getPointY()
            Dim p3 As Point = resultY(i).refPoint.getPointY(deltaX, deltaY)

            Dim maxX As Integer = Math.Max(p1.X, p3.X)
            Dim minX As Integer = Math.Min(p2.X, p3.X)
            Dim p1f As PointF = sender.toPictureboxPoint(New Point(minX, p3.Y))
            Dim p2f As PointF = sender.toPictureboxPoint(New Point(maxX, p3.Y))
            g.DrawLine(Pens.Orange, p1f, p2f)
        Next
    End Sub

End Class
