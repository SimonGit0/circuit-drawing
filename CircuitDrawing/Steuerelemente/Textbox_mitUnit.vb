Public Class Textbox_mitUnit
    Inherits Textbox_Einstellungen

    Public unit As String = ""

    Public Sub setVarious()
        Me.Text = ElementEinstellung.VARIOUS_STRING
    End Sub

    Public Sub setText_ohneUnit(str As String)
        Me.Text = str & unit
    End Sub

    Public Function getText_ohneUnit() As String
        Dim str As String = Me.Text
        If str.TrimEnd().EndsWith(unit) Then
            str = str.TrimEnd()
            str = str.Substring(0, str.Length - unit.Length)
        End If
        Return str
    End Function

End Class
