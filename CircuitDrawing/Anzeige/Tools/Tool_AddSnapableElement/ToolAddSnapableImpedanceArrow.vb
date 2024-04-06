Public Class ToolAddSnapableImpedanceArrow
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

    Protected Overrides Function getRückText() As String
        Return "Impedanz-Beschriftung hinzugefügt"
    End Function

    Protected Overrides Function getElement(ID As ULong) As SnapableElement
        If invertRichtung Then
            Return New SnapableImpedanceArrow(ID, New ParamArrow(CShort(mypfeil), 100), lastSnappoint.flip(), New Beschriftung("$Z_1$", positionIndex, DO_Text.TextRotation.Normal, SnapableImpedanceArrow.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableImpedanceArrow.DEFAULT_ABSTAND_QUER), 0, 0, SnapableImpedanceArrow.DEFAULT_ABSTAND_PFEIL, SnapableImpedanceArrow.DEFAULT_LAENGE_X, SnapableImpedanceArrow.DEFAULT_LAENGE_Y)
        Else
            Return New SnapableImpedanceArrow(ID, New ParamArrow(CShort(mypfeil), 100), lastSnappoint, New Beschriftung("$Z_1$", positionIndex, DO_Text.TextRotation.Normal, SnapableImpedanceArrow.DEFAULT_ABSTAND_BESCHRIFTUNG, SnapableImpedanceArrow.DEFAULT_ABSTAND_QUER), 0, 0, SnapableImpedanceArrow.DEFAULT_ABSTAND_PFEIL, SnapableImpedanceArrow.DEFAULT_LAENGE_X, SnapableImpedanceArrow.DEFAULT_LAENGE_Y)
        End If
    End Function

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.ToolAddImpedanz & " " & MyBase.getDefaultHilfeText()
    End Function
End Class
