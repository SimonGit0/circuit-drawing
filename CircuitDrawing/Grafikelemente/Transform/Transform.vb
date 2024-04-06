Public MustInherit Class Transform
    Public MustOverride Function transformPoint(p As Point) As Point
    Public MustOverride Function transformPointF(p As PointF) As PointF

    Public MustOverride Sub transformMatrix(m() As Single)
End Class

Public Class Transform_translate
    Inherits Transform

    Private dx, dy As Integer

    Public Sub New(dx As Integer, dy As Integer)
        Me.dx = dx
        Me.dy = dy
    End Sub

    Public Sub New(delta As Point)
        Me.dx = delta.X
        Me.dy = delta.Y
    End Sub

    Public Overrides Function transformPoint(p As Point) As Point
        Return New Point(p.X + dx, p.Y + dy)
    End Function

    Public Overrides Function transformPointF(p As PointF) As PointF
        Return New PointF(p.X + dx, p.Y + dy)
    End Function

    Public Overrides Sub transformMatrix(m() As Single)
        'Do Nothing!
    End Sub
End Class

Public Class Transform_scale
    Inherits Transform

    Private fx, fy As Integer

    Public Sub New(fx As Integer, fy As Integer)
        Me.fx = fx
        Me.fy = fy
    End Sub

    Public Overrides Sub transformMatrix(m() As Single)
        '               (  fx     0 )   ( m(0)  m(1) )
        'Scale Matrix = (           ) * (            )
        '               (   0    fy )   ( m(2)  m(3) ) 
        m(0) *= fx
        m(1) *= fx
        m(2) *= fy
        m(3) *= fy
    End Sub

    Public Overrides Function transformPoint(p As Point) As Point
        Return New Point(p.X * fx, p.Y * fy)
    End Function

    Public Overrides Function transformPointF(p As PointF) As PointF
        Return New PointF(p.X * fx, p.Y * fy)
    End Function
End Class

Public Class Transform_rotate
    Inherits Transform

    Private drehung As Drehmatrix

    Public Sub New(d As Drehmatrix)
        Me.drehung = d
    End Sub

    Public Overrides Sub transformMatrix(m() As Single)
        '                (  a  b )   ( m(0)  m(1) )   ( a*m(0)+b*m(2)  a*m(1)+b*m(3) )
        'Rotate Matrix = (       ) * (            ) = (                              )
        '                (  c  d )   ( m(2)  m(3) )   ( c*m(0)+d*m(2)  c*m(1)+d*m(3) )

        Dim rM() As Integer = drehung.getMatrix()

        Dim mNeu(3) As Single
        mNeu(0) = rM(0) * m(0) + rM(1) * m(2)
        mNeu(1) = rM(0) * m(1) + rM(1) * m(3)
        mNeu(2) = rM(2) * m(0) + rM(3) * m(2)
        mNeu(3) = rM(2) * m(1) + rM(3) * m(3)

        m(0) = mNeu(0)
        m(1) = mNeu(1)
        m(2) = mNeu(2)
        m(3) = mNeu(3)
    End Sub

    Public Overrides Function transformPoint(p As Point) As Point
        Return Me.drehung.transformPoint(p)
    End Function

    Public Overrides Function transformPointF(p As PointF) As PointF
        Return Me.drehung.transformPointF(p)
    End Function
End Class

Public Class TransformMulti
    Inherits Transform

    Private transforms As List(Of Transform)

    Public Sub New()
        transforms = New List(Of Transform)
    End Sub

    Public Sub add(t As Transform)
        transforms.Add(t)
    End Sub

    Public Overrides Sub transformMatrix(m() As Single)
        For i As Integer = 0 To transforms.Count - 1
            transforms(i).transformMatrix(m)
        Next
    End Sub

    Public Overrides Function transformPoint(p As Point) As Point
        For i As Integer = 0 To transforms.Count - 1
            p = transforms(i).transformPoint(p)
        Next
        Return p
    End Function

    Public Overrides Function transformPointF(p As PointF) As PointF
        For i As Integer = 0 To transforms.Count - 1
            p = transforms(i).transformPointF(p)
        Next
        Return p
    End Function
End Class