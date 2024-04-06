Public Class ToolDrawLine
    Inherits Tool

    Private routingColor As Color = Color.Orange
    Private penRouting As Pen
    Private myLinestyles As LineStyleList

    Private snap_point As Snappoint = Nothing
    Private mode As Modus = Modus.IDLE
    Private pfeilEnde As ParamArrow = New ParamArrow(-1, 100)
    Private start As Snappoint

    Public Sub New()
        penRouting = New Pen(routingColor)
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        sender.Invalidate()
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        sender.Invalidate()
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)

        myLinestyles = New LineStyleList()
        Dim ls As LineStyle = sender.myLineStyles.getLineStyle(0).copy()
        ls.farbe = New Farbe(routingColor.A, routingColor.R, routingColor.G, routingColor.B)
        myLinestyles.add(ls)

        Me.mode = Modus.IDLE
        suche_snapPoint(sender, sender.GetCursorPos, Nothing)
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        Me.mode = Modus.IDLE
        suche_snapPoint(sender, sender.GetCursorPos, Nothing)
    End Sub

    Public Overrides Function abortAction(sender As Vektor_Picturebox) As Boolean
        If mode = Modus.Has_Start Then
            mode = Modus.IDLE
            sender.Invalidate()
            Return True
        End If
        Return False
    End Function

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If mode = Modus.IDLE Then
            suche_snapPoint(sender, e.CursorPos, Nothing)
        ElseIf mode = Modus.Has_Start Then
            suche_snapPoint(sender, e.CursorPos, start)
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.IDLE Then
                start = New Snappoint(e.CursorPos, 0, 0, 0, 0)
                mode = Modus.Has_Start
                suche_snapPoint(sender, e.CursorPos, start)
            ElseIf mode = Modus.Has_Start Then
                Dim ende As New Snappoint(e.CursorPos, 0, 0, 0, 0)
                Dim zl As Point = ende.p
                Dim st As Point = start.p
                If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                    If Math.Abs(zl.X - st.X) > Math.Abs(zl.Y - st.Y) Then
                        zl.Y = st.Y
                    Else
                        zl.X = st.X
                    End If
                End If
                route(sender, st, zl)
                If Me.snap_point IsNot Nothing AndAlso Me.snap_point.p = e.CursorPos Then
                    'Wire zuende!
                    mode = Modus.IDLE
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                Else
                    'Weiteres wire zeichnen (direkt aktuellen punkt als neuen Startpunkt definieren!)
                    start = New Snappoint(zl, 0, 0, 0, 0)
                    mode = Modus.Has_Start
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                End If
            End If
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keyToolDrawSnap.isDown(e.KeyCode) Then
            e.Handled = True
            If mode = Modus.IDLE Then
                If snap_point Is Nothing Then
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                End If
                If snap_point IsNot Nothing Then
                    start = snap_point
                    mode = Modus.Has_Start
                    suche_snapPoint(sender, sender.GetCursorPos, start)
                End If
            ElseIf mode = Modus.Has_Start Then
                If snap_point Is Nothing Then
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                End If
                If snap_point IsNot Nothing Then
                    Dim ende As Snappoint = snap_point
                    Dim zl As Point = ende.p
                    Dim st As Point = start.p
                    If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                        If Math.Abs(zl.X - st.X) > Math.Abs(zl.Y - st.Y) Then
                            zl.Y = st.Y
                        Else
                            zl.X = st.X
                        End If
                    End If
                    route(sender, st, zl)
                    mode = Modus.IDLE
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                End If
            End If
        ElseIf Key_Settings.getSettings().keyToolChangeArrow.isDown(e.KeyCode) Then
            If mode = Modus.Has_Start Then
                pfeilEnde.pfeilArt += CShort(1)
                If pfeilEnde.pfeilArt >= Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile Then
                    pfeilEnde.pfeilArt = -1
                End If
                sender.Invalidate()
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub suche_snapPoint(sender As Vektor_Picturebox, p As Point, ignore As Snappoint)
        Dim changed As Boolean = False
        Dim pos As Snappoint = sender.sucheNextSnapPoint(p, ignore)
        Dim wireSnap As WireSnappoint = sender.getNextPosOnWire(p)
        If wireSnap IsNot Nothing Then
            Dim wirePos As Point = wireSnap.getMitteInt()
            Dim wireVector As Point = wireSnap.getVector()
            If pos Is Nothing OrElse pos.p <> p Then
                'Nur wenn der "normale" Snappoint p nicht schon perfekt auf dem wire liegt...
                If (wireVector.X = 0 OrElse wireVector.Y = 0) AndAlso Math.Abs(wirePos.X - p.X) < sender.getGridX() AndAlso Math.Abs(wirePos.Y - p.Y) < sender.getGridY() Then
                    pos = New Snappoint(wirePos, 0, 0, 0, 0)
                End If
                'If wirePos = p AndAlso (wireVector.X = 0 OrElse wireVector.Y = 0) Then
                'pos = New Snappoint(wirePos, Nothing, 0, 0, 0, 0)
                'End If
            End If
        End If
        If pos IsNot Nothing Then
            If snap_point Is Nothing Then
                snap_point = pos
                changed = True
            Else
                If Not snap_point.isEqual(pos) Then
                    snap_point = pos
                    changed = True
                End If
            End If
        Else
            If snap_point IsNot Nothing Then
                snap_point = Nothing
                changed = True
            End If
        End If
        If changed Then
            sender.Invalidate()
        End If
    End Sub

    Private Sub route(sender As Vektor_Picturebox, st As Point, zl As Point)
        If st.X <> zl.X OrElse st.Y <> zl.Y Then
            Dim rück As New RückgängigGrafik()
            rück.setText("Linie hinzugefügt")
            rück.speicherVorherZustand(sender.getRückArgs())

            Dim w As New Basic_Linie(sender.getNewID(), 2, st, zl, New ParamArrow(-1, 100), pfeilEnde.CopyPfeil())
            sender.addElement(w)

            rück.speicherNachherZustand(sender.getRückArgs())
            sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If mode = Modus.IDLE AndAlso snap_point IsNot Nothing Then
            sender.drawCursorAtPosition(e, snap_point.p, Vektor_Picturebox.CursorStyle.Circle, False)
        ElseIf mode = Modus.Has_Start Then
            If snap_point IsNot Nothing Then
                sender.drawCursorAtPosition(e, snap_point.p, Vektor_Picturebox.CursorStyle.Circle, False)
            End If

            Dim pos As Point = sender.GetCursorPos
            Dim ziel As Snappoint
            If snap_point IsNot Nothing AndAlso pos.X = snap_point.p.X AndAlso pos.Y = snap_point.p.Y Then
                ziel = snap_point
            Else
                ziel = New Snappoint(pos, 0, 0, 0, 0)
            End If
            If start.p.X <> ziel.p.X OrElse start.p.Y <> ziel.p.Y Then
                Dim st As Point = start.p
                Dim zl As Point = ziel.p
                If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                    If Math.Abs(zl.X - st.X) > Math.Abs(zl.Y - st.Y) Then
                        zl.Y = st.Y
                    Else
                        zl.X = st.X
                    End If
                End If
                Dim p1 As PointF = sender.toPictureboxPoint(st)
                Dim p2 As PointF = sender.toPictureboxPoint(zl)
                If pfeilEnde.pfeilArt > -1 Then
                    Dim myPfeilEnde As Pfeilspitze = Pfeil_Verwaltung.getVerwaltung().getPfeil(pfeilEnde)
                    Dim vector As New Point(zl.X - st.X, zl.Y - st.Y)
                    Dim pEnde As DO_Polygon = myPfeilEnde.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, zl, vector)
                    Dim args As New GrafikDrawArgs(myLinestyles, sender.myFillStyles, sender.myFonts, sender.calcPixelPerMM(), sender.TextVorschauMode)
                    sender.setViewArgs(args)
                    pEnde.drawGraphics(e.Graphics, args)

                    Dim verkürzung As Single = args.toPictureboxScale(myPfeilEnde.getLineVerkürzung())
                    Dim lEnde As Single = Mathe.abstand(p1, p2)
                    verkürzung = Math.Min(lEnde, verkürzung)
                    p2.X += verkürzung / lEnde * (p1.X - p2.X)
                    p2.Y += verkürzung / lEnde * (p1.Y - p2.Y)
                End If
                e.Graphics.DrawLine(penRouting, p1, p2)
            End If
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize
        e.Graphics.DrawLines(New Pen(e.CursorPen.Color, 1),
                                          {New PointF(p.X + 0.2F * s.Width, p.Y),
                                           New PointF(p.X + 0.2F * s.Width, p.Y + 0.5F * s.Height),
                                           New PointF(p.X + 0.8F * s.Width, p.Y + 0.5F * s.Height),
                                           New PointF(p.X + 0.8F * s.Width, p.Y + s.Height)})
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tool_AddLine & " " &
               k.keyToolDrawSnap.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumSnappenAmNächstenVerbinder & " " &
               k.keyToolChangeArrow.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerPfeilspitze
    End Function

    Private Enum Modus
        IDLE
        Has_Start
    End Enum
End Class
