Public Class ToolCopy
    Inherits Tool

    Private mode As Modus
    Private deselectAfterCopy As Boolean

    Private neueElemente As List(Of ElementMaster)

    Private moveStartpunkt As Point

    Private rück1 As RückgängigGrafik
    Private rück2 As RückgängigLineStyle
    Private rück3 As RückgängigFillStyles

    Private copyFromClipboard As Boolean = False

    Private lastGravityPointsX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsY As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateY As List(Of GravityPoint) = Nothing

    Private previewMarkierung As ToolHelper_PreviewMarkierung

    Public Sub New()
        Me.previewMarkierung = New ToolHelper_PreviewMarkierung()
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        mode = Modus.IDLE
        If sender.has_selection() Then
            previewMarkierung.HighlightLöschen(sender)
        Else
            previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
        End If
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        mode = Modus.IDLE
        If sender.has_selection() Then
            previewMarkierung.HighlightLöschen(sender)
        Else
            previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
        End If
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        If Me.copyFromClipboard Then
            sender.cancelCurrentTool(True)
        Else
            sender.removeNichtBenötigteStyles()
        End If
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        sender.removeNichtBenötigteStyles()
    End Sub

    Private Function getPoint(sender As Vektor_Picturebox, p As Point, ByRef keinX As Boolean, ByRef keinY As Boolean) As Point
        If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.AlleRichtungen OrElse Me.copyFromClipboard Then
            Return p
        Else
            Dim dx As Integer = p.X - moveStartpunkt.X
            Dim dy As Integer = p.Y - moveStartpunkt.Y
            If Math.Abs(dx) > Math.Abs(dy) Then
                dy = 0
                keinY = True
            Else
                dx = 0
                keinX = True
            End If
            Return New Point(moveStartpunkt.X + dx, moveStartpunkt.Y + dy)
        End If
    End Function

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.IDLE Then

                rück1 = New RückgängigGrafik()
                rück1.speicherVorherZustand(sender.getRückArgs())

                rück2 = New RückgängigLineStyle()
                rück2.speicherVorher(sender.getRückArgs())

                rück3 = New RückgängigFillStyles()
                rück3.speicherVorher(sender.getRückArgs())

                If Not sender.has_selection Then
                    sender.select_Element_At(e.CursorPos, Vektor_Picturebox.SelectionMode.SelectOnlyNewElements)
                    deselectAfterCopy = True
                Else
                    deselectAfterCopy = False
                End If

                If sender.has_selection Then
                    mode = Modus.HatElementeAnMaus
                    sender.copySelectedElements(neueElemente)
                    sender.deselect_All()
                    lastGravityPointsX = sender.getGravityPointsX(True, True)
                    lastGravityPointsY = sender.getGravityPointsY(True, True)
                    lastGravityPointsTemplateX = sender.getGravityPointsX_Template(neueElemente)
                    lastGravityPointsTemplateY = sender.getGravityPointsY_Template(neueElemente)

                    moveStartpunkt = e.CursorPos()
                    previewMarkierung.HighlightLöschen(sender)
                    sender.Invalidate()
                End If
            ElseIf mode = Modus.HatElementeAnMaus Then
                mode = Modus.IDLE
                Dim keinX As Boolean = False
                Dim keinY As Boolean = False
                Dim cp As Point = getPoint(sender, e.CursorPos, keinX, keinY)

                Dim delta As New Point(cp.X - moveStartpunkt.X, cp.Y - moveStartpunkt.Y)
                If sender.enable_gravity Then
                    delta = sender.performGravity(keinX, keinY, delta, delta, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY).posResult
                End If

                For Each element As ElementMaster In neueElemente
                    If TypeOf element Is Element Then
                        DirectCast(element, Element).position = New Point(DirectCast(element, Element).position.X + delta.X, DirectCast(element, Element).position.Y + delta.Y)
                        If deselectAfterCopy Then DirectCast(element, Element).isSelected = False
                        sender.addElement(DirectCast(element.Clone(AddressOf sender.getNewID), Element))
                    ElseIf TypeOf element Is SnapableElement Then
                        For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                            DirectCast(element, SnapableElement).getSnappoint(i).move(delta.X, delta.Y)
                        Next
                        If deselectAfterCopy Then element.deselect()
                        sender.addElement(DirectCast(element.Clone(AddressOf sender.getNewID), SnapableElement))
                    End If
                Next

                sender.simplifyWires()
                freeElements()

                rück1.speicherNachherZustand(sender.getRückArgs())
                rück2.speicherNachher(sender.getRückArgs())
                rück3.speicherNachher(sender.getRückArgs())

                Dim rückMulti As New RückgängigMulti()
                rückMulti.setText("Elemente Kopieren")
                If rück1.istNotwendig Then
                    rückMulti.Rück.Add(rück1)
                End If
                If rück2.RückBenötigt() Then
                    rückMulti.Rück.Add(rück2)
                End If
                If rück3.RückBenötigt() Then
                    rückMulti.Rück.Add(rück3)
                End If

                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rückMulti))

                lastGravityPointsTemplateX = Nothing
                lastGravityPointsTemplateY = Nothing
                lastGravityPointsX = Nothing
                lastGravityPointsY = Nothing

                If Not deselectAfterCopy Then
                    sender.cancelCurrentTool()
                Else
                    previewMarkierung.PreviewSelect(sender, e.CursorPos)
                End If
            End If
        ElseIf e.Button = MouseButtons.Middle Then
            dreheElemente(Drehmatrix.Drehen90Grad, sender)
            sender.Invalidate()
        End If
    End Sub

    Public Sub set_copy_elemente(sender As Vektor_Picturebox, myClipboard As CopyClipboard)
        If mode = Modus.IDLE AndAlso myClipboard IsNot Nothing Then

            copyFromClipboard = True

            myClipboard = myClipboard.copy()
            Dim CP As Point = sender.GetCursorPos()
            myClipboard.transform(CP.X, CP.Y)

            rück1 = New RückgängigGrafik()
            rück1.speicherVorherZustand(sender.getRückArgs())

            rück2 = New RückgängigLineStyle()
            rück2.speicherVorher(sender.getRückArgs())

            rück3 = New RückgängigFillStyles()
            rück3.speicherVorher(sender.getRückArgs())

            If neueElemente Is Nothing Then neueElemente = New List(Of ElementMaster)
            neueElemente.Clear()
            For i As Integer = 0 To myClipboard.ElementeAll.Count - 1
                neueElemente.Add(myClipboard.ElementeAll(i))
                If TypeOf myClipboard.ElementeAll(i) Is Element Then
                    DirectCast(neueElemente(neueElemente.Count - 1), Element).isSelected = True
                ElseIf TypeOf myClipboard.ElementeAll(i) Is SnapableElement Then
                    DirectCast(neueElemente(neueElemente.Count - 1), SnapableElement).selectAll()
                End If

                If TypeOf neueElemente(i) Is IElementWithFont Then
                    Dim fs As FontStyle = myClipboard.FontstyleList.getFontStyle(DirectCast(myClipboard.ElementeAll(i), IElementWithFont).get_fontstyle())
                    DirectCast(neueElemente(neueElemente.Count - 1), IElementWithFont).set_fontstyle(sender.myFonts.getNumberOfNewFontStyle(fs))
                End If
                If TypeOf neueElemente(i) Is IElementWithFill Then
                    Dim fs As FillStyle = myClipboard.FillstyleList.getFillStyle(DirectCast(myClipboard.ElementeAll(i), IElementWithFill).get_fillstyle())
                    DirectCast(neueElemente(neueElemente.Count - 1), IElementWithFill).set_fillstyle(sender.myFillStyles.getNumberOfNewFillStyle(fs))
                End If
                If TypeOf neueElemente(i) Is IElementWithLinestyle Then
                    Dim ls As LineStyle = myClipboard.LinestyleList.getLineStyle(DirectCast(myClipboard.ElementeAll(i), IElementWithLinestyle).get_mylinestyle())
                    DirectCast(neueElemente(neueElemente.Count - 1), IElementWithLinestyle).set_mylinestyle(sender.myLineStyles.getNumberOfNewLinestyle(ls))
                End If
            Next
            If neueElemente.Count > 0 Then
                deselectAfterCopy = False
                mode = Modus.HatElementeAnMaus
                sender.deselect_All()

                lastGravityPointsX = sender.getGravityPointsX(True, True)
                lastGravityPointsY = sender.getGravityPointsY(True, True)
                lastGravityPointsTemplateX = sender.getGravityPointsX_Template(neueElemente)
                lastGravityPointsTemplateY = sender.getGravityPointsY_Template(neueElemente)

                moveStartpunkt = sender.GetCursorPos()
                sender.Invalidate()
            Else
                Throw New Exception("Allgemeiner Fehler beim Einfügen")
            End If
        Else
            Throw New Exception("Falscher Modus!")
        End If
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If mode = Modus.HatElementeAnMaus Then
            sender.Invalidate()
        Else
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, e.CursorPos)
            End If
        End If
    End Sub

    Public Overrides Sub OnMultiSelectChanged(sender As Vektor_Picturebox, e As EventArgs)
        If mode <> Modus.HatElementeAnMaus Then
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
            End If
        End If
    End Sub

    Public Overrides Sub OnGravityChanged(sender As Vektor_Picturebox, e As EventArgs)
        If mode = Modus.HatElementeAnMaus Then
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If mode = Modus.HatElementeAnMaus Then
            Dim keinX As Boolean = False
            Dim keinY As Boolean = False
            Dim cp As Point = getPoint(sender, sender.GetCursorPos(), keinX, keinY)

            Dim delta As New Point(cp.X - moveStartpunkt.X, cp.Y - moveStartpunkt.Y)
            If sender.enable_gravity Then
                Dim gravityResult As GravityDrawState = sender.performGravity(keinX, keinY, delta, delta, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY)
                delta = gravityResult.posResult
                gravityResult.draw(e.Graphics, sender)
            End If

            Dim transform As New Transform_translate(delta.X, delta.Y)
            For Each element As ElementMaster In neueElemente
                Dim grafik As DO_Grafik = element.getGrafik()
                grafik.transform(transform)
                grafik.drawGraphics(e.Graphics, e.args_Elemente)

                Dim bBox As Rectangle = grafik.getBoundingBox()
                bBox.X -= delta.X
                bBox.Y -= delta.Y
                element.AfterDrawingGDI(bBox)

                If TypeOf element Is Element Then
                    grafik = DirectCast(element, Element).getSelection().getGrafik()
                    grafik.transform(transform)
                    grafik.drawGraphics(e.Graphics, e.args_Selection)
                ElseIf TypeOf element Is SnapableElement Then

                    For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                        grafik = DirectCast(element, SnapableElement).getSnappoint(i).getSelection().getGrafik()
                        grafik.transform(transform)
                        grafik.drawGraphics(e.Graphics, e.args_Selection)
                    Next
                End If
            Next
        Else
            previewMarkierung.OnDraw(sender, e)
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        p.X += 0.25F * s.Width
        s.Width *= 0.75F
        e.Graphics.DrawRectangle(e.CursorPen, p.X, p.Y + 0.25F * s.Height, 0.75F * s.Width, 0.75F * s.Height)
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + 0.25F * s.Width, p.Y + 0.25F * s.Height),
                                           New PointF(p.X + 0.25F * s.Width, p.Y),
                                           New PointF(p.X + s.Width, p.Y),
                                           New PointF(p.X + s.Width, p.Y + 0.75F * s.Height),
                                           New PointF(p.X + 0.75F * s.Width, p.Y + 0.75F * s.Height)})
    End Sub

    Public Overrides Function abortAction(sender As Vektor_Picturebox) As Boolean
        If mode = Modus.HatElementeAnMaus Then
            mode = Modus.IDLE
            freeElements()
            If sender.has_selection() Then
                previewMarkierung.HighlightLöschen(sender)
            Else
                previewMarkierung.PreviewSelect(sender, sender.GetCursorPos())
            End If
            sender.Invalidate()
            If copyFromClipboard Then
                Return False
            End If
            Return True
        End If
        Return False
    End Function

    Private Sub freeElements()
        If neueElemente IsNot Nothing Then neueElemente.Clear()
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keySchalteBeschriftungsPosDurch.isDown(e.KeyCode) AndAlso mode = Modus.HatElementeAnMaus Then
            Dim changed As Boolean = False
            For Each element In neueElemente
                If TypeOf element Is BauteilAusDatei Then
                    If DirectCast(element, BauteilAusDatei).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                ElseIf TypeOf element Is Basic_Spannungspfeil Then
                    If DirectCast(element, Basic_Spannungspfeil).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                ElseIf TypeOf element Is SnapableCurrentArrow Then
                    If DirectCast(element, SnapableCurrentArrow).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                ElseIf TypeOf element Is SnapableImpedanceArrow Then
                    If DirectCast(element, SnapableImpedanceArrow).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                ElseIf TypeOf element Is SnapableLabel Then
                    If DirectCast(element, SnapableLabel).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                ElseIf TypeOf element Is SnapableBusTap Then
                    If DirectCast(element, SnapableBusTap).schalteBeschriftungsPosDurch() Then
                        changed = True
                    End If
                End If
            Next
            If changed Then
                sender.Invalidate()
            End If
            e.Handled = True
        ElseIf Key_Settings.getSettings().keyToolAddInstanceMirrorX.isDown(e.KeyCode) AndAlso mode = Modus.HatElementeAnMaus Then
            dreheElemente(Drehmatrix.MirrorX, sender)
            sender.Invalidate()
            e.Handled = True
        ElseIf Key_Settings.getSettings().keyToolAddInstanceMirrorY.isDown(e.KeyCode) AndAlso mode = Modus.HatElementeAnMaus Then
            dreheElemente(Drehmatrix.MirrorY, sender)
            sender.Invalidate()
            e.Handled = True
        End If
    End Sub

    Private Sub dreheElemente(drehmatrix As Drehmatrix, sender As Vektor_Picturebox)
        If mode = Modus.HatElementeAnMaus Then
            Dim drehpunkt As Point = Me.moveStartpunkt
            For Each element In neueElemente
                element.drehe(drehpunkt, drehmatrix)
            Next
            lastGravityPointsTemplateX = sender.getGravityPointsX_Template(neueElemente)
            lastGravityPointsTemplateY = sender.getGravityPointsY_Template(neueElemente)
        End If
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tools_BauteileKopieren & " " &
               k.keyToolAddInstanceMirrorX.getStatusStripString() & " " & My.Resources.Strings.Tools_zumHSpiegeln & " " &
               k.keyToolAddInstanceMirrorY.getStatusStripString() & " " & My.Resources.Strings.Tools_zumVSpiegeln & " " &
               "'" & My.Resources.Strings.Mittlere_Maustaste & "' " & My.Resources.Strings.Tools_ZumDrehen & " " &
               k.keySchalteBeschriftungsPosDurch.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerBeschriftung
    End Function

    Private Enum Modus
        IDLE
        HatElementeAnMaus
    End Enum

End Class
