Public Module WindowRendererConstants
    Public TabInsertColor1 As Color = Color.FromArgb(128, 255, 255, 75)
    Public TabInsertColor2 As Color = Color.FromArgb(128, 150, 150, 0)

    Public OneTabInsertColor1 As Color = Color.FromArgb(128, 255, 255, 75)
    Public OneTabInsertColor2 As Color = Color.FromArgb(128, 150, 150, 0)

    Public DockStackInsertColor1 As Color = Color.FromArgb(128, 51, 128, 255)
    Public DockStackInsertColor2 As Color = Color.FromArgb(128, 0, 50, 150)

    Public NewDockStackInsertColor1 As Color = Color.FromArgb(128, 255, 100, 100)
    Public NewDockStackInsertColor2 As Color = Color.FromArgb(128, 150, 50, 50)

    Public NewDockLeisteInsertColor1 As Color = Color.FromArgb(128, 100, 255, 100)
    Public NewDockLeisteInsertColor2 As Color = Color.FromArgb(128, 50, 105, 50)

    Public FormVerschiebenOpacity As Double = 0.5

    'Wird auch als Referenz für andere PopUpWindows genutzt! Z.B. in den Contents des Windowrenderers
    'als Sub-Windows (Farbenauswahl II)
    Public DockLeisteHCloseDelay As Double = 0.1
End Module

Public Class callbackeventargs
    Inherits EventArgs
    Public Property callbackDel As Action(Of Boolean)
    Public Sub New(ByVal del As Action(Of Boolean))
        Me.callbackDel = del
    End Sub
End Class

Public Class VisibleChangedEventArgs
    Inherits EventArgs
    Public Property DontSelectContent As Boolean
    Public Sub New(ByVal dontselect As Boolean)
        DontSelectContent = dontselect
    End Sub
End Class

Public Class movingeventargs
    Inherits EventArgs
    Public Property Höhe As Integer
    Public Sub New(ByVal höhe As Integer)
        Me.Höhe = höhe
    End Sub
End Class