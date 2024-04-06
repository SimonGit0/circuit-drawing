Public Class ShortcutKey
    Public hatCtrl As Boolean
    Public hatShift As Boolean
    Public hatAlt As Boolean
    Public keyValue As Keys

    Public Sub New(keyValue As Keys, hatCtrl As Boolean, hatShift As Boolean, hatAlt As Boolean)
        Me.keyValue = keyValue
        Me.hatCtrl = hatCtrl
        Me.hatShift = hatShift
        Me.hatAlt = hatAlt
    End Sub

    Public Sub New(keyValue As Keys)
        Me.New(keyValue, False, False, False)
    End Sub

    Public Sub New(keyValue As Keys, ctrl As Boolean)
        Me.New(keyValue, ctrl, False, False)
    End Sub

    Public Sub New(keyValue As Keys, ctrl As Boolean, shift As Boolean)
        Me.New(keyValue, ctrl, shift, False)
    End Sub

    Public Function isDown(e As KeyEventArgs) As Boolean
        If e.KeyCode = keyValue OrElse (keyValue = Keys.Oemplus AndAlso e.KeyCode = Keys.Add) OrElse (keyValue = Keys.OemMinus AndAlso e.KeyCode = Keys.Subtract) Then
            If hatCtrl Xor e.Control Then Return False
            If hatAlt Xor e.Alt Then Return False
            If hatShift Xor e.Shift Then Return False
            Return True
        End If
        Return False
    End Function

    Public Function isDown(key As Keys) As Boolean
        If key = keyValue OrElse (keyValue = Keys.Oemplus AndAlso key = Keys.Add) OrElse (keyValue = Keys.OemMinus AndAlso key = Keys.Subtract) Then
            If hatCtrl Xor My.Computer.Keyboard.CtrlKeyDown Then Return False
            If hatAlt Xor My.Computer.Keyboard.AltKeyDown Then Return False
            If hatShift Xor My.Computer.Keyboard.ShiftKeyDown Then Return False
            Return True
        End If
        Return False
    End Function

    Public Function getStatusStripString() As String
        Return "'" & getMenuString() & "'"
    End Function

    Public Function getMenuString() As String
        Dim erg As String = ""
        If hatCtrl Then
            erg &= "Strg + "
        End If
        If hatShift Then
            erg &= My.Resources.Strings.Key_shift & " + "
        End If
        If hatAlt Then
            erg &= "Alt + "
        End If
        Return erg & getKeyValueString()
    End Function

    Private Function getKeyValueString() As String
        Select Case keyValue
            Case Keys.Delete
                Return My.Resources.Strings.Key_Entf
            Case Keys.Space
                Return My.Resources.Strings.Key_Leertaste
            Case Keys.Oemplus
                Return "Plus"
            Case Keys.OemMinus
                Return "Minus"
            Case Else
                Return keyValue.ToString()
        End Select
    End Function

    Public Function isEqual(k As ShortcutKey) As Boolean
        If Me.keyValue <> k.keyValue Then Return False
        If Me.hatCtrl <> k.hatCtrl Then Return False
        If Me.hatAlt <> k.hatAlt Then Return False
        If Me.hatShift <> k.hatShift Then Return False
        Return True
    End Function
End Class