''' <summary>
''' Stellt eine nxm matrix da. 
''' Verfügt über die Möglichkeit die Matrix nach dem Gauss-Verfahren aufzulösen
''' </summary>
''' <remarks></remarks>
Public Class Matrix
    'X-Richtung, Y-Richtung
    Private matrix(,) As Double
    Public Sub New(ByVal _matrix(,) As Double)
        matrix = _matrix
    End Sub
    Public Function Auflösen() As Double()
        If matrix.Length = 0 Then Return Nothing
        For i As Integer = 0 To matrix.GetLength(1) - 2
            If matrix(i, i) = 0 Then
                For j As Integer = i + 1 To matrix.GetLength(1) - 1
                    If matrix(i, j) <> 0 Then
                        Zeilentauschen(i, j)
                        Exit For
                    End If
                Next
                If matrix(i, i) = 0 Then
                    Return Nothing
                End If
            End If
            For k As Integer = i + 1 To matrix.GetLength(1) - 1
                Dim zahl As Double = -matrix(i, k) / matrix(i, i)
                For j As Integer = 0 To matrix.GetLength(0) - 1
                    matrix(j, k) += matrix(j, i) * zahl
                Next
            Next
        Next
        Dim lösungen(matrix.GetLength(1) - 1) As Double
        lösungen(lösungen.Length - 1) = matrix(matrix.GetLength(0) - 1, matrix.GetLength(1) - 1) / matrix(matrix.GetLength(0) - 2, matrix.GetLength(1) - 1)
        For i As Integer = lösungen.Length - 2 To 0 Step -1
            lösungen(i) = matrix(matrix.GetLength(0) - 1, i)
            For j As Integer = 0 To matrix.GetLength(0) - 2
                If j <> i Then
                    lösungen(i) -= lösungen(j) * matrix(j, i)
                End If
            Next
            lösungen(i) /= matrix(i, i)
        Next

        Return lösungen
    End Function
    Private Sub Zeilentauschen(ByVal zeile1 As Integer, ByVal zeile2 As Integer)
        Dim zeile(matrix.GetLength(0) - 1) As Double
        For i As Integer = 0 To zeile.Length - 1
            zeile(i) = matrix(i, zeile2)
        Next
        For i As Integer = 0 To zeile.Length - 1
            matrix(i, zeile2) = matrix(i, zeile1)
        Next
        For i As Integer = 0 To zeile.Length - 1
            matrix(i, zeile1) = zeile(i)
        Next
    End Sub
End Class