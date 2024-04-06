Public Class ToolMove
    Inherits Tool

    Private isMoving As Boolean = False
    Private deselectAfterMoving As Boolean
    Private lastPos As Point
    Private startPos As Point

    Private myRück As RückgängigGrafik
    Private myRückCurrentMove As RückgängigGrafik

    Private lastGravityPointsX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsY As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateY As List(Of GravityPoint) = Nothing
    Private lastGravityDrawState As GravityDrawState = Nothing

    Private previewMarkierung As ToolHelper_PreviewMarkierung

    Public Sub New()
        Me.previewMarkierung = New ToolHelper_PreviewMarkierung()
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        If sender.has_selection() Then
            previewMarkierung.HighlightLöschen(sender)
        Else
            previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
        End If
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        abbort(sender)
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        abbort(sender)
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        If sender.has_selection() Then
            previewMarkierung.HighlightLöschen(sender)
        Else
            previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If Not isMoving Then
                'Start Moving on first click
                myRück = New RückgängigGrafik()
                myRück.setText("Verschieben")

                If Not sender.has_selection Then
                    sender.select_Element_At(e.CursorPos, Vektor_Picturebox.SelectionMode.SelectOnlyNewElements)
                    deselectAfterMoving = True
                Else
                    deselectAfterMoving = False
                End If

                lastGravityPointsX = sender.getGravityPointsX(True, False)
                lastGravityPointsY = sender.getGravityPointsY(True, False)
                lastGravityPointsTemplateX = sender.getGravityPointsX(False, True)
                lastGravityPointsTemplateY = sender.getGravityPointsY(False, True)

                'An allen Punkte vom Template (also die verschoben werden) sollte nicht gesnappt werden.
                For i As Integer = lastGravityPointsX.Count - 1 To 0 Step -1
                    For j As Integer = 0 To lastGravityPointsTemplateX.Count - 1
                        If lastGravityPointsTemplateX(j).posSnap = lastGravityPointsX(i).posSnap AndAlso
                           lastGravityPointsTemplateX(j).posAndereKoordinate = lastGravityPointsX(i).posAndereKoordinate Then
                            lastGravityPointsX.RemoveAt(i)
                            Exit For
                        End If
                    Next
                Next
                For i As Integer = lastGravityPointsY.Count - 1 To 0 Step -1
                    For j As Integer = 0 To lastGravityPointsTemplateY.Count - 1
                        If lastGravityPointsTemplateY(j).posSnap = lastGravityPointsY(i).posSnap AndAlso
                           lastGravityPointsTemplateY(j).posAndereKoordinate = lastGravityPointsY(i).posAndereKoordinate Then
                            lastGravityPointsY.RemoveAt(i)
                            Exit For
                        End If
                    Next
                Next

                myRück.speicherVorherZustand(sender.getRückArgs())

                Me.previewMarkierung.HighlightLöschen(sender)
                If sender.has_selection Then
                    myRückCurrentMove = New RückgängigGrafik(True)
                    myRückCurrentMove.speicherVorherZustand(sender.getRückArgs())

                    isMoving = True
                    sender.LockSelection()
                    lastPos = e.CursorPos
                    startPos = e.CursorPos
                Else
                    myRück = Nothing
                End If
            Else
                'End Moving on second click
                isMoving = False
                sender.UnlockSelection()
                If deselectAfterMoving Then
                    sender.deselect_All()
                Else
                    sender.cancelCurrentTool()
                End If

                myRück.speicherNachherZustand(sender.getRückArgs())
                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(myRück))
                myRück = Nothing

                lastGravityPointsTemplateX = Nothing
                lastGravityPointsTemplateY = Nothing
                lastGravityPointsX = Nothing
                lastGravityPointsY = Nothing

                Me.previewMarkierung.PreviewSelect(sender, e.CursorPos)
            End If
        ElseIf e.Button = MouseButtons.Middle Then
            If isMoving Then
                drehe(sender, e.CursorPos, Drehmatrix.Drehen90Grad)
            End If
        End If
    End Sub

    Private Sub drehe(sender As Vektor_Picturebox, cursorPos As Point, drehmatrix As Drehmatrix)
        'Drehen um "Alten" Mittelpunkt (Punkt an dem Maus-Down gemacht wurde)
        'Deshalb erstmal zurückschieben (über Rückgängig)
        myRückCurrentMove.machRückgängigInklusiveKorrekterSelection(sender.getRückArgs())
        'Dann drehen
        sender.dreheSelected(Me.startPos, drehmatrix)
        'Jetzt als neue Ausgangsposition speichern
        myRückCurrentMove.speicherVorherZustand(sender.getRückArgs())

        'In diesem Zustand jetzt die gravity snapPoints neu speichern (wurden gedreht!)
        lastGravityPointsTemplateX = sender.getGravityPointsX(False, True)
        lastGravityPointsTemplateY = sender.getGravityPointsY(False, True)

        'Element wieder zurückschieben an neue Position
        Dim dx As Integer = cursorPos.X - lastPos.X
        Dim dy As Integer = cursorPos.Y - lastPos.Y
        lastPos = cursorPos
        Dim dxGesamt As Integer = cursorPos.X - startPos.X
        Dim dyGesamt As Integer = cursorPos.Y - startPos.Y
        Dim keinX As Boolean = False
        Dim keinY As Boolean = False
        If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
            If Math.Abs(dxGesamt) > Math.Abs(dyGesamt) Then
                dyGesamt = 0
                keinY = True
            Else
                dxGesamt = 0
                keinX = True
            End If
        End If

        If sender.enable_gravity Then
            Dim pos As New Point(dxGesamt, dyGesamt)

            lastGravityDrawState = sender.performGravity(keinX, keinY, pos, pos, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY)
            dxGesamt = lastGravityDrawState.posResult.X
            dyGesamt = lastGravityDrawState.posResult.Y
        Else
            lastGravityDrawState = Nothing
        End If

        If My.Computer.Keyboard.CtrlKeyDown Then
            sender.moveSelectedElements_Simple(dxGesamt, dyGesamt)
        Else
            sender.moveSelectedElements(dxGesamt, dyGesamt)
        End If

        sender.Invalidate()
    End Sub

    Public Overrides Sub OnGravityChanged(sender As Vektor_Picturebox, e As EventArgs)
        If isMoving Then
            _forcedMoveDueGravityChange = True
            sender.performNewCursorMove()
            _forcedMoveDueGravityChange = False
        End If
    End Sub

    Private _forcedMoveDueGravityChange As Boolean = False
    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If isMoving Then
            Dim dx As Integer = e.CursorPos.X - lastPos.X
            Dim dy As Integer = e.CursorPos.Y - lastPos.Y
            lastPos = e.CursorPos

            If dx <> 0 OrElse dy <> 0 OrElse _forcedMoveDueGravityChange Then
                Dim keinY As Boolean = False
                Dim keinX As Boolean = False

                Dim dxGesamt As Integer = e.CursorPos.X - startPos.X
                Dim dyGesamt As Integer = e.CursorPos.Y - startPos.Y

                If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                    If Math.Abs(dxGesamt) > Math.Abs(dyGesamt) Then
                        dyGesamt = 0
                        keinY = True
                    Else
                        dxGesamt = 0
                        keinX = True
                    End If
                End If
                myRückCurrentMove.machRückgängigInklusiveKorrekterSelection(sender.getRückArgs())

                If sender.enable_gravity Then
                    Dim pos As New Point(dxGesamt, dyGesamt)

                    lastGravityDrawState = sender.performGravity(keinX, keinY, pos, pos, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY)
                    dxGesamt = lastGravityDrawState.posResult.X
                    dyGesamt = lastGravityDrawState.posResult.Y
                Else
                    lastGravityDrawState = Nothing
                End If

                If My.Computer.Keyboard.CtrlKeyDown Then
                    sender.moveSelectedElements_Simple(dxGesamt, dyGesamt)
                Else
                    sender.moveSelectedElements(dxGesamt, dyGesamt)
                End If
            End If
        Else
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, e.CursorPos)
            End If
        End If
    End Sub

    Public Overrides Sub OnMultiSelectChanged(sender As Vektor_Picturebox, e As EventArgs)
        If Not isMoving Then
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
            End If
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If isMoving Then
            If Key_Settings.getSettings().keyToolAddInstanceMirrorX.isDown(e.KeyCode) Then
                drehe(sender, sender.GetCursorPos(), Drehmatrix.MirrorX)
                e.Handled = True
            ElseIf Key_Settings.getSettings().keyToolAddInstanceMirrorY.isDown(e.KeyCode) Then
                drehe(sender, sender.GetCursorPos(), Drehmatrix.MirrorY)
                e.Handled = True
            End If
        End If
        If Key_Settings.getSettings().keySchalteBeschriftungsPosDurch.isDown(e.KeyCode) Then
            e.Handled = True
        End If
    End Sub

    Public Overrides Function abortAction(sender As Vektor_Picturebox) As Boolean
        If isMoving Then
            abbort(sender)
            sender.Invalidate()
            Return True
        End If
        Return False
    End Function

    Private Sub abbort(sender As Vektor_Picturebox)
        If isMoving Then

            sender.UnlockSelection()
            myRück.macheRückgängig(sender.getRückArgs())
            myRück = Nothing

            If deselectAfterMoving Then
                sender.deselect_All()
            End If
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
            End If

            isMoving = False
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If lastGravityDrawState IsNot Nothing AndAlso isMoving Then
            lastGravityDrawState.draw(e.Graphics, sender)
        End If
        If Not isMoving Then
            previewMarkierung.OnDraw(sender, e)
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        Dim aL As Single = 0.1

        e.Graphics.DrawLine(e.CursorPen, New PointF(p.X, p.Y + 0.5F * s.Height), New PointF(p.X + s.Width, p.Y + 0.5F * s.Height))
        e.Graphics.DrawLine(e.CursorPen, New PointF(p.X + 0.5F * s.Width, p.Y), New PointF(p.X + 0.5F * s.Width, p.Y + s.Height))
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + aL * s.Width, p.Y + s.Height * (0.5F - aL)),
                                           New PointF(p.X, p.Y + s.Height * 0.5F),
                                           New PointF(p.X + aL * s.Width, p.Y + s.Height * (0.5F + aL))})
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + (1.0F - aL) * s.Width, p.Y + s.Height * (0.5F - aL)),
                                           New PointF(p.X + s.Width, p.Y + s.Height * 0.5F),
                                           New PointF(p.X + (1.0F - aL) * s.Width, p.Y + s.Height * (0.5F + aL))})
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + s.Width * (0.5F - aL), p.Y + aL * s.Height),
                                           New PointF(p.X + s.Width * 0.5F, p.Y),
                                           New PointF(p.X + s.Width * (0.5F + aL), p.Y + aL * s.Height)})
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + s.Width * (0.5F - aL), p.Y + (1.0F - aL) * s.Height),
                                           New PointF(p.X + s.Width * 0.5F, p.Y + s.Height),
                                           New PointF(p.X + s.Width * (0.5F + aL), p.Y + (1.0F - aL) * s.Height)})


    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tool_Move & " " &
               k.keyToolAddInstanceMirrorX.getStatusStripString() & " " & My.Resources.Strings.Tools_zumHSpiegeln & " " &
               k.keyToolAddInstanceMirrorY.getStatusStripString() & " " & My.Resources.Strings.Tools_zumVSpiegeln & " " &
               "'" & My.Resources.Strings.Mittlere_Maustaste & "' " & My.Resources.Strings.Tools_ZumDrehen
    End Function
End Class
