Public Class Pfeil_Line
    Inherits Pfeilspitze

    Private Const xh As Integer = 35
    Private Const yh As Integer = 45

    Private faktor As Single

    Private ReadOnly Verkürzung As Integer

    Public Sub New(mitVerkürzung As Boolean, faktor As Single)
        Me.faktor = faktor
        If mitVerkürzung Then
            Verkürzung = CInt(faktor * (xh + xh / 2))
        Else
            Verkürzung = 0
        End If
    End Sub

    Public Overrides Function getLineVerkürzung() As Integer
        Return Verkürzung
    End Function

    Public Overrides Function getGrafik_Basic(a As AlignPfeil) As DO_Polygon
        Dim xh_scale As Integer = CInt(xh * faktor)
        Dim yh_scale As Integer = CInt(yh * faktor)
        Dim dx As Integer = 0
        If a = AlignPfeil.Align_An_Mitte Then
            dx = xh_scale
        End If

        Dim erg As New DO_Polygon({New Point(-2 * xh_scale + dx, -yh_scale),
                                   New Point(dx, 0),
                                   New Point(-2 * xh_scale + dx, yh_scale)}, False, True, True, True, True)
        erg.closed = False
        Return erg
    End Function
End Class
