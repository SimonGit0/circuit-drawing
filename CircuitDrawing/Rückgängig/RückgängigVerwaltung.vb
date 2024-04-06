Public Class RückgängigVerwaltung

    Private myRück As List(Of Rückgängig)
    Private myRückIndex As Integer

    Private v As Vektor_Picturebox

    Public Sub New(v As Vektor_Picturebox)
        Me.v = v
        myRück = New List(Of Rückgängig)
        myRückIndex = -1
    End Sub

    Public Sub löscheAlleRückgängig()
        myRück.Clear()
        myRückIndex = -1
    End Sub

    Public Sub addRückElement(r As Rückgängig)
        'Alle Vorgängigelemente löschen!
        If kannVorgängig() Then
            myRück.RemoveRange(myRückIndex + 1, myRück.Count - myRückIndex - 1)
        End If
        'Neues Element hinzufügen
        myRück.Add(r)
        myRückIndex += 1
    End Sub

    Public Function kannRückgängig() As Boolean
        Return myRückIndex >= 0
    End Function

    Public Function kannVorgängig() As Boolean
        Return myRück.Count - myRückIndex > 1
    End Function

    Public Sub macheRück()
        If kannRückgängig() Then
            myRück(myRückIndex).macheRückgängig(createArgs())
            myRückIndex -= 1
            v.Invalidate()
        End If
    End Sub

    Public Sub macheVorgängig()
        If kannVorgängig() Then
            myRück(myRückIndex + 1).macheVorgängig(createArgs())
            myRückIndex += 1
            v.Invalidate()
        End If
    End Sub

    Private Function createArgs() As RückgängigArgs
        Return v.getRückArgs()
    End Function

End Class
