Public MustInherit Class Rückgängig

    Private myText As String = "Rückgängig"
    Public Sub New(myText As String)
        Me.myText = myText
    End Sub

    Public Sub setText(t As String)
        Me.myText = t
    End Sub

    Public MustOverride Sub macheRückgängig(args As RückgängigArgs)

    Public MustOverride Sub macheVorgängig(args As RückgängigArgs)

    Public Overridable Function getText() As String
        Return myText
    End Function
End Class

Public Class RückgängigArgs
    Public ElementeListe As List(Of ElementMaster)

    Public LinestyleList As LineStyleList
    Public FillstyleList As FillStyleList
    Public FontstyleList As FontList

    Public Sub New(e As List(Of ElementMaster), ls As LineStyleList, fs As FillStyleList, fonts As FontList)
        Me.ElementeListe = e
        Me.LinestyleList = ls
        Me.FillstyleList = fs
        Me.FontstyleList = fonts
    End Sub

    Public Function copy() As RückgängigArgs
        Dim neueE As New List(Of ElementMaster)(ElementeListe.Count)
        For i As Integer = 0 To Me.ElementeListe.Count - 1
            neueE.Add(Me.ElementeListe(i).Clone())
        Next
        Return New RückgängigArgs(neueE, Me.LinestyleList.clone(), FillstyleList.clone(), FontstyleList.clone())
    End Function
End Class