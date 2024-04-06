Public Class Maus

    Private Const MOUSEEVENTF_LEFTDOWN = &H2
    Private Const MOUSEEVENTF_LEFTUP = &H4
    Private Const MOUSEEVENTF_MIDDLEDOWN = &H20
    Private Const MOUSEEVENTF_MIDDLEUP = &H40
    Private Const MOUSEEVENTF_RIGHTDOWN = &H8
    Private Const MOUSEEVENTF_RIGHTUP = &H10

    Public Shared Sub MouseDown(Optional ByVal Button As MouseButtons = MouseButtons.Left, Optional ByVal XPos As Long = -1, Optional ByVal YPos As Long = -1)
        ' Mauszeiger positionieren
        If XPos <> -1 Or YPos <> -1 Then
            Cursor.Position = New Point(CInt(XPos), CInt(YPos))
        End If
        ' Mausklick simulieren
        Select Case Button
            ' linke Maustaste
            Case MouseButtons.Left
                NativeMethods.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)

                ' mittlere Maustaste
            Case MouseButtons.Middle
                NativeMethods.mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0)

                ' rechte Maustaste
            Case MouseButtons.Right
                NativeMethods.mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0)
        End Select
    End Sub
    Public Shared Sub MouseUp(Optional ByVal Button As MouseButtons = MouseButtons.Left, Optional ByVal XPos As Long = -1, Optional ByVal YPos As Long = -1)
        ' Mauszeiger positionieren
        If XPos <> -1 Or YPos <> -1 Then
            Cursor.Position = New Point(CInt(XPos), CInt(YPos))
        End If
        ' Mausklick simulieren
        Select Case Button
            ' linke Maustaste
            Case MouseButtons.Left
                NativeMethods.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)

                ' mittlere Maustaste
            Case MouseButtons.Middle
                NativeMethods.mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0)

                ' rechte Maustaste
            Case MouseButtons.Right
                NativeMethods.mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0)
        End Select
    End Sub
    Private Const LMT = &H1 'Konstante für Linke Maustaste
    Private Const RMT = &H2 'Konstante für Rechte Maustaste
    Private Const MMT = &H4 'Konstante für Mittlere Maustaste
    Private Const KeyPressed = -32767
    Private Const KeyDown = -32768
    Public Shared Function MausGeklickt(ByVal btn As MouseButtons) As Boolean
        Select Case btn
            Case MouseButtons.Left
                Dim nr As Integer = NativeMethods.GetAsyncKeyState(LMT)
                Return nr = KeyPressed Or nr = KeyDown
            Case MouseButtons.Right
                Dim nr As Integer = NativeMethods.GetAsyncKeyState(RMT)
                Return nr = KeyPressed Or nr = KeyDown
            Case MouseButtons.Middle
                Dim nr As Integer = NativeMethods.GetAsyncKeyState(MMT)
                Return nr = KeyPressed Or nr = KeyDown
            Case MouseButtons.None
                Return Not MausGeklickt(MouseButtons.Left) And Not MausGeklickt(MouseButtons.Right) And Not MausGeklickt(MouseButtons.Middle)
        End Select
        Return False
    End Function
End Class
