Public Class ToolDrawWire
    Inherits Tool

    Private routingColor As Color = Color.Orange
    Private penRouting As Pen
    Private myLinestyles As LineStyleList

    Private snap_point As Snappoint = Nothing
    Private mode As Modus = Modus.IDLE
    Private wireMode As DrawWireMode = DrawWireMode.Rechtwinklig
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
                route(sender, start, ende)
                If Me.snap_point IsNot Nothing AndAlso Me.snap_point.p = e.CursorPos Then
                    'Wire zuende!
                    mode = Modus.IDLE
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                Else
                    'Weiteres wire zeichnen (direkt aktuellen punkt als neuen Startpunkt definieren!)
                    suche_snapPoint(sender, e.CursorPos, Nothing)
                    start = snap_point
                    mode = Modus.Has_Start
                    suche_snapPoint(sender, sender.GetCursorPos, Nothing)
                End If
            End If
        ElseIf e.Button = MouseButtons.Middle Then
            If wireMode = DrawWireMode.Rechtwinklig Then
                wireMode = DrawWireMode.Luftlinie
            ElseIf wireMode = DrawWireMode.Luftlinie Then
                wireMode = DrawWireMode.Rechtwinklig
            End If
            sender.Invalidate()
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
                    route(sender, start, ende)
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

    Private Sub route(sender As Vektor_Picturebox, start As Snappoint, ende As Snappoint)
        If start.p.X <> ende.p.X OrElse start.p.Y <> ende.p.Y Then
            If wireMode = DrawWireMode.Rechtwinklig Then
                sender.DoRouting(start, ende, pfeilEnde.CopyPfeil(), False)
            Else
                sender.DoRoutingLuftlinie(start, ende, pfeilEnde.CopyPfeil())
            End If
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
                If wireMode = DrawWireMode.Rechtwinklig Then
                    Dim routing As List(Of Point) = sender.findRouting(start, ziel, False)
                    Dim punkte(routing.Count - 1) As PointF
                    For i As Integer = 0 To routing.Count - 1
                        punkte(i) = sender.toPictureboxPoint(routing(i))
                    Next
                    If pfeilEnde.pfeilArt > -1 Then
                        Dim myPfeilEnde As Pfeilspitze = Pfeil_Verwaltung.getVerwaltung().getPfeil(pfeilEnde)
                        Dim vector As New Point(routing(routing.Count - 1).X - routing(routing.Count - 2).X, routing(routing.Count - 1).Y - routing(routing.Count - 2).Y)
                        Dim pEnde As DO_Polygon = myPfeilEnde.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, routing(routing.Count - 1), vector)
                        Dim args As New GrafikDrawArgs(myLinestyles, sender.myFillStyles, sender.myFonts, sender.calcPixelPerMM(), sender.TextVorschauMode)
                        sender.setViewArgs(args)
                        pEnde.drawGraphics(e.Graphics, args)

                        Dim verkürzung As Single = args.toPictureboxScale(myPfeilEnde.getLineVerkürzung())
                        Dim lEnde As Single = Mathe.abstand(punkte(punkte.Length - 1), punkte(punkte.Length - 2))
                        verkürzung = Math.Min(lEnde, verkürzung)
                        punkte(punkte.Length - 1).X += verkürzung / lEnde * (punkte(punkte.Length - 2).X - punkte(punkte.Length - 1).X)
                        punkte(punkte.Length - 1).Y += verkürzung / lEnde * (punkte(punkte.Length - 2).Y - punkte(punkte.Length - 1).Y)
                    End If
                    e.Graphics.DrawLines(penRouting, punkte)
                Else
                    Dim p1 As PointF = sender.toPictureboxPoint(start.p)
                    Dim p2 As PointF = sender.toPictureboxPoint(ziel.p)
                    If pfeilEnde.pfeilArt > -1 Then
                        Dim myPfeilEnde As Pfeilspitze = Pfeil_Verwaltung.getVerwaltung().getPfeil(pfeilEnde)
                        Dim vector As New Point(ziel.p.X - start.p.X, ziel.p.Y - start.p.Y)
                        Dim pEnde As DO_Polygon = myPfeilEnde.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, ziel.p, vector)
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
        Return My.Resources.Strings.Tools_DrawWire & " " &
               k.keyToolDrawSnap.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumSnappenAmNächstenVerbinder & " " &
               k.keyToolChangeArrow.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerPfeilspitze
    End Function

    Private Enum Modus
        IDLE
        Has_Start
    End Enum

    Public Enum DrawWireMode
        Rechtwinklig
        Luftlinie
    End Enum
End Class
