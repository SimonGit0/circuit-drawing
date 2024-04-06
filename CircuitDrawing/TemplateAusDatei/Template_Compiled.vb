Public Class Template_Compiled
    Private paramVisible() As Boolean
    Private grafik As DO_MultiGrafik
    Private snaps As List(Of Snappoint)
    Private selectionRect As Rectangle
    Private textpos As List(Of TextPoint)
    Private scalings As List(Of ScalingLinie)
    Private _benötigt_Fillstil As Boolean

    Public Sub New(NrOfParams As Integer)
        Me.snaps = New List(Of Snappoint)
        Me.textpos = New List(Of TextPoint)

        ReDim paramVisible(NrOfParams - 1)
    End Sub

    Public Sub AllParamsVisible()
        For i As Integer = 0 To paramVisible.Length - 1
            paramVisible(i) = True
        Next
    End Sub

    Public Sub snaps_Clear()
        snaps.Clear()
    End Sub

    Public Sub textpos_Clear()
        Me.textpos.Clear()
    End Sub

    Public Sub moveTextpos(dx As Integer, dy As Integer)
        For i As Integer = 0 To textpos.Count - 1
            textpos(i).pos.X += dx
            textpos(i).pos.Y += dy
        Next
    End Sub

    Public Sub set_grafik(g As DO_MultiGrafik)
        Me.grafik = g
    End Sub

    Public Sub set_Scaling(s As List(Of ScalingLinie))
        Me.scalings = s
    End Sub

    Public Sub set_benötigt_Fillstil(fillstile As Boolean)
        Me._benötigt_Fillstil = fillstile
    End Sub

    Public Sub add_Snap(s As Snappoint)
        Me.snaps.Add(s)
    End Sub

    Public Sub add_textpos(t As TextPoint)
        Me.textpos.Add(t)
    End Sub

    Public Sub set_selectionRect(r As Rectangle)
        If r.Width < 0 Then
            r.X += r.Width
            r.Width *= -1
        End If
        If r.Height < 0 Then
            r.Y += r.Height
            r.Height *= -1
        End If
        Me.selectionRect = r
    End Sub

#Region "Get Compiled Grafik"
    Public Function getNrOfSnappoints() As Integer
        Return snaps.Count
    End Function

    Public Function getSnappoint(index As Integer) As Snappoint
        Return snaps(index).Clone()
    End Function

    Public Function getGrafik() As DO_MultiGrafik
        Return DirectCast(grafik.Clone(), DO_MultiGrafik)
    End Function

    Public Function getSelectionRect() As Rectangle
        Return selectionRect
    End Function

    Public Function getNrOfTextpos() As Integer
        Return textpos.Count
    End Function

    Public Function getTextpos(index As Integer) As TextPoint
        If index >= 0 AndAlso index < textpos.Count Then
            Return textpos(index)
        Else
            Return New TextPoint(New Point(0, 0), 0, 0, New Point(0, 0))
        End If
    End Function

    Public Function getParamsVisible() As Boolean()
        Return Me.paramVisible
    End Function

    Public Function getParamVisible(i As Integer) As Boolean
        If i >= 0 AndAlso i < Me.paramVisible.Count Then
            Return Me.paramVisible(i)
        Else
            Return True
        End If
    End Function

    Public Function getNrOfScalingLinien() As Integer
        Return scalings.Count
    End Function

    Public Function getScalingLinie(index As Integer) As ScalingLinie
        Return scalings(index).Clone()
    End Function

    Public Function benötigt_Fillstil() As Boolean
        Return _benötigt_Fillstil
    End Function
#End Region
End Class
