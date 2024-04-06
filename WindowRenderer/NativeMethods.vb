Imports System.Runtime.InteropServices

Public Class NativeMethods
    <DllImport("User32.dll")>
    Public Shared Sub mouse_event(ByVal dwFlags As Integer, ByVal dx As Integer, ByVal dy As Integer, ByVal cButtons As Integer, ByVal dwExtraInfo As Integer)
    End Sub

    <DllImport("User32.dll")>
    Public Shared Function GetAsyncKeyState(ByVal vKey As Integer) As Short
    End Function
End Class
