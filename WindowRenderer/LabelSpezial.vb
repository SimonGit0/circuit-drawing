Public Class LabelSpezial
    Inherits Label

    Private ganzerText As String

    Public Shadows Property Text As String
        Get
            Return ganzerText
        End Get
        Set(value As String)
            ganzerText = value
            anpassen()
        End Set
    End Property

    Protected Overrides Sub OnSizeChanged(e As System.EventArgs)
        MyBase.OnSizeChanged(e)
        anpassen()
    End Sub

    Private Sub anpassen()
        Using g As Graphics = Graphics.FromHwnd(IntPtr.Zero)
            Dim str As String = ganzerText
            Dim strPunkt As String = str
            While g.MeasureString(strPunkt, Me.Font, Integer.MaxValue, StringFormat.GenericDefault).Width > Me.Width
                If str.Length = 0 Then
                    strPunkt = ""
                    Exit While
                End If
                str = str.Substring(0, str.Length - 1)
                strPunkt = str & "..."
            End While
            MyBase.Text = strPunkt
        End Using
    End Sub

End Class
