Public Class ToolAddLabel
    Inherits ToolAddSnapableElement

    Protected Overrides Function getRückText() As String
        Return "Label Hinzugefügt"
    End Function

    Protected Overrides Function getElement(ID As ULong) As SnapableElement
        If invertRichtung Then
            Return New SnapableLabel(ID, lastSnappoint.flip(), New Beschriftung("label", positionIndex, DO_Text.TextRotation.Normal, SnapableLabel.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableLabel.DEFAULT_ABSTAND_QUER), 0)
        Else
            Return New SnapableLabel(ID, lastSnappoint, New Beschriftung("label", positionIndex, DO_Text.TextRotation.Normal, SnapableLabel.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableLabel.DEFAULT_ABSTAND_QUER), 0)
        End If
    End Function

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tool_AddLabel & " " & MyBase.getDefaultHilfeText()
    End Function
End Class
