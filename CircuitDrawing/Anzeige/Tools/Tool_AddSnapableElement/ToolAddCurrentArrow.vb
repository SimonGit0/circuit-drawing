Public Class ToolAddCurrentArrow
    Inherits ToolAddSnapableElement

    Private mypfeil As Integer

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        mypfeil = 0
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keyToolChangeArrow.isDown(e.KeyCode) Then
            e.Handled = True
            If lastSnappoint IsNot Nothing Then
                mypfeil += 1
                If mypfeil >= Pfeil_Verwaltung.getVerwaltung.AnzahlPfeile() Then
                    mypfeil = 0
                End If
                sender.Invalidate()
            End If
        Else
            MyBase.KeyDown(sender, e)
        End If
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tool_AddCurrentArrow & " " & MyBase.getDefaultHilfeText() & " " &
               k.keyToolChangeArrow.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerPfeilspitze
    End Function

    Protected Overrides Function getRückText() As String
        Return "Strompfeil hinzugefügt"
    End Function

    Protected Overrides Function getElement(ID As ULong) As SnapableElement
        If invertRichtung Then
            Return New SnapableCurrentArrow(ID, New ParamArrow(CShort(mypfeil), 100), lastSnappoint.flip(), New Beschriftung("$I_1$", positionIndex, DO_Text.TextRotation.Normal, SnapableCurrentArrow.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableCurrentArrow.DEFAULT_ABSTAND_QUER), 3, SnapableCurrentArrow.DEFAULT_ART, 0)
        Else
            Return New SnapableCurrentArrow(ID, New ParamArrow(CShort(mypfeil), 100), lastSnappoint, New Beschriftung("$I_1$", positionIndex, DO_Text.TextRotation.Normal, SnapableCurrentArrow.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableCurrentArrow.DEFAULT_ABSTAND_QUER), 3, SnapableCurrentArrow.DEFAULT_ART, 0)
        End If
    End Function
End Class


