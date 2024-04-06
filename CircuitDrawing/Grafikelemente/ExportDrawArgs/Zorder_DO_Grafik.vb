Public Class Zorder_DO_Grafik
    Implements IComparable(Of Zorder_DO_Grafik)

    Public myGrafik As DO_Grafik
    Public ZOrder As Integer

    Public Sub New(g As DO_Grafik, z As Integer)
        Me.myGrafik = g
        Me.ZOrder = z
    End Sub

    Public Function CompareTo(other As Zorder_DO_Grafik) As Integer Implements IComparable(Of Zorder_DO_Grafik).CompareTo
        Return Me.ZOrder.CompareTo(other.ZOrder)
    End Function
End Class
