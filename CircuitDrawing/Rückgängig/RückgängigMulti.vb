Public Class RückgängigMulti
    Inherits Rückgängig

    Public Rück As List(Of Rückgängig)

    Public Sub New()
        MyBase.New("Grafik verändern")
        Me.Rück = New List(Of Rückgängig)
    End Sub

    Public Overrides Sub macheRückgängig(args As RückgängigArgs)
        For i As Integer = 0 To Rück.Count - 1
            Rück(i).macheRückgängig(args)
        Next
    End Sub

    Public Overrides Sub macheVorgängig(args As RückgängigArgs)
        For i As Integer = 0 To Rück.Count - 1
            Rück(i).macheVorgängig(args)
        Next
    End Sub
End Class
