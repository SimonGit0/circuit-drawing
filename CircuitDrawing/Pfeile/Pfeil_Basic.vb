Public Class Pfeil_Basic
    Inherits Pfeilspitze

    Private Const xh As Integer = 40
    Private Const yh As Integer = 50

    Private faktor As Single

    Public Sub New(f As Single)
        Me.faktor = f
    End Sub

    Public Overrides Function getGrafik_Basic(a As AlignPfeil) As DO_Polygon
        Dim yh_scale As Integer = CInt(yh * faktor)
        Dim xh_scale As Integer = CInt(xh * faktor)

        Dim dx As Integer = 0
        If a = AlignPfeil.Align_An_Mitte Then
            dx = xh_scale
        End If

        Dim erg As New DO_Polygon({New Point(dx, 0),
                                   New Point(-2 * xh_scale + dx, -yh_scale),
                                   New Point(-2 * xh_scale + dx, yh_scale)}, True, False, True, True, False)
        Return erg
    End Function

    Public Overrides Function getLineVerkürzung() As Integer
        Return CInt(xh * faktor)
    End Function
End Class
