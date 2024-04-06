Public Class ToolAddBusTap
    Inherits ToolAddSnapableElement

    Protected Overrides Function getRückText() As String
        Return "Bus-Beschriftung hinzugefügt"
    End Function

    Protected Overrides Function getElement(ID As ULong) As SnapableElement
        If invertRichtung Then
            Return New SnapableBusTap(ID, lastSnappoint.flip(), New Beschriftung("8", positionIndex, DO_Text.TextRotation.Normal, SnapableBusTap.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableBusTap.DEFAULT_ABSTAND_QUER), 0, 0)
        Else
            Return New SnapableBusTap(ID, lastSnappoint, New Beschriftung("8", positionIndex, DO_Text.TextRotation.Normal, SnapableBusTap.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableBusTap.DEFAULT_ABSTAND_QUER), 0, 0)
        End If
    End Function

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tool_AddBus & " " & MyBase.getDefaultHilfeText()
    End Function
End Class
