Public Class RückgängigFillStyles
    Inherits Rückgängig

    Public Sub New()
        MyBase.New("Füllstil geändert")
    End Sub

    Private vor() As FillStyle
    Private nach() As FillStyle

    Public Sub speicherVorher(args As RückgängigArgs)
        speichern(vor, args)
    End Sub

    Public Sub speicherNachher(args As RückgängigArgs)
        speichern(nach, args)
    End Sub

    Private Sub speichern(ByRef s() As FillStyle, args As RückgängigArgs)
        ReDim s(args.FillstyleList.Count - 1)
        For i As Integer = 0 To s.Count - 1
            If args.FillstyleList.getFillStyle(i) Is Nothing Then
                s(i) = Nothing
            Else
                s(i) = args.FillstyleList.getFillStyle(i)
            End If
        Next
    End Sub

    Private Sub laden(vonS() As FillStyle, toArgs As RückgängigArgs)
        toArgs.FillstyleList.clear()
        For i As Integer = 0 To vonS.Length - 1
            toArgs.FillstyleList.add(vonS(i))
        Next
    End Sub

    Public Function RückBenötigt() As Boolean
        If vor.Length <> nach.Length Then
            Return True
        End If
        For i As Integer = 0 To vor.Length - 1
            If vor(i) IsNot Nothing AndAlso nach(i) Is Nothing Then
                Return True
            End If
            If vor(i) Is Nothing AndAlso nach(i) IsNot Nothing Then
                Return True
            End If
            If vor(i) IsNot Nothing AndAlso nach(i) IsNot Nothing AndAlso Not vor(i).isSameAs(nach(i)) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Overrides Sub macheRückgängig(args As RückgängigArgs)
        laden(vor, args)
    End Sub

    Public Overrides Sub macheVorgängig(args As RückgängigArgs)
        laden(nach, args)
    End Sub
End Class
