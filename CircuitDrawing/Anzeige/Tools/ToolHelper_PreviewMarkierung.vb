Public Class ToolHelper_PreviewMarkierung

    Private elementPreviewSelect As List(Of Object)

    Public Sub HighlightLöschen(sender As Vektor_Picturebox)
        elementPreviewSelect = Nothing
        sender.Invalidate()
    End Sub

    Public Sub PreviewSelect(sender As Vektor_Picturebox, p As Point)
        elementPreviewSelect = sender.getElementAt_forSelect(p)
        sender.Invalidate()
    End Sub

    Public Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If elementPreviewSelect IsNot Nothing Then
            For Each o As Object In elementPreviewSelect
                If TypeOf o Is Element Then
                    Dim g As DO_Grafik = DirectCast(o, Element).getSelection().getGrafik()
                    g.lineStyle.linestyle = 1
                    g.lineStyle.scaling = 1.0F / 4.0F
                    If TypeOf g Is DO_MultiGrafik Then
                        DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(1)
                        DirectCast(g, DO_MultiGrafik).setLineScalingRekursiv(1.0F / 4.0F)
                    End If
                    g.drawGraphics(e.Graphics, e.args_Selection)
                ElseIf TypeOf o Is WireSnappoint Then
                    Dim g As DO_Grafik = DirectCast(o, WireSnappoint).getSelection().getGrafik()
                    g.lineStyle.linestyle = 1
                    g.lineStyle.scaling = 1.0F / 4.0F
                    If TypeOf g Is DO_MultiGrafik Then
                        DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(1)
                        DirectCast(g, DO_MultiGrafik).setLineScalingRekursiv(1.0F / 4.0F)
                    End If

                    g.drawGraphics(e.Graphics, e.args_Selection)
                End If
            Next
        End If
    End Sub

End Class
