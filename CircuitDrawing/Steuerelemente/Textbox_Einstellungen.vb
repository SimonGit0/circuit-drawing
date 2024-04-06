Public Class Textbox_Einstellungen
    Inherits TextBox

    Protected Overrides Sub OnClick(e As EventArgs)
        MyBase.OnClick(e)
        If Me.Text = ElementEinstellung.VARIOUS_STRING Then
            Me.SelectAll()
        End If
    End Sub
End Class

Public Class TextBox_EinstellungParentEvent
    Inherits Textbox_Einstellungen

    Private oldParent As Control = Nothing
    Private oldParentWidth As Integer = -1

    Protected Overrides Sub OnParentChanged(e As EventArgs)
        MyBase.OnParentChanged(e)
        If oldParent IsNot Nothing Then
            RemoveHandler oldParent.SizeChanged, AddressOf parentSizeChanged
        End If
        Me.oldParent = Me.Parent
        If Me.oldParent IsNot Nothing Then
            AddHandler oldParent.SizeChanged, AddressOf parentSizeChanged
        End If
    End Sub

    Private Sub parentSizeChanged(sender As Object, e As EventArgs)
        If sender IsNot Nothing AndAlso TypeOf sender Is Control Then
            Dim width As Integer = DirectCast(sender, Control).Width
            If width <> oldParentWidth Then
                RaiseEvent ParentWidthChanged(Me, width)
                oldParentWidth = width
            End If
        End If
    End Sub

    Public Event ParentWidthChanged(sender As Object, new_width As Integer)

End Class
