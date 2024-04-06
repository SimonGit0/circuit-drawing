Public Class ToolDrawBezier
    Inherits Tool

    Private routingColor As Color = Color.Orange
    Private penRouting As Pen
    Private penRoutingKontrolle As Pen

    Private p1, p2, p3, p4 As Point
    Private mode As Modus = Modus.IDLE
    Private pfeilEnde As ParamArrow = New ParamArrow(-1, 100)

    Public Sub New()
        penRouting = New Pen(routingColor)
        penRoutingKontrolle = New Pen(routingColor)
        penRoutingKontrolle.DashStyle = Drawing2D.DashStyle.Dash
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
        Me.mode = Modus.IDLE
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        Me.mode = Modus.IDLE
    End Sub

    Public Overrides Function abortAction(sender As Vektor_Picturebox) As Boolean
        If mode <> Modus.IDLE Then
            mode = Modus.IDLE
            sender.Invalidate()
            Return True
        End If
        Return False
    End Function

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keyToolChangeArrow.isDown(e.KeyCode) Then
            If mode = Modus.Has_P2 OrElse mode = Modus.Has_P4 Then
                pfeilEnde.pfeilArt += CShort(1)
                If pfeilEnde.pfeilArt >= Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile Then
                    pfeilEnde.pfeilArt = -1
                End If
                sender.Invalidate()
                e.Handled = True
            End If
        End If
    End Sub

    Public Overrides Sub MouseDown(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.IDLE Then
                p1 = e.CursorPos
                mode = Modus.Has_P1
                sender.Invalidate()
            ElseIf mode = Modus.Has_P2 Then
                p4 = e.CursorPos
                mode = Modus.Has_P4
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.Has_P1 Then
                p2 = e.CursorPos
                mode = Modus.Has_P2
                sender.Invalidate()
            ElseIf mode = Modus.Has_P4 Then
                Dim p3_gespiegelt As Point = e.CursorPos
                p3 = New Point(2 * p4.X - p3_gespiegelt.X, 2 * p4.Y - p3_gespiegelt.Y)
                route(sender, p1, p2, p3, p4)
                p1 = p4
                p2 = p3_gespiegelt
                mode = Modus.Has_P2
                sender.Invalidate()
            End If
        End If
    End Sub

    Private Sub route(sender As Vektor_Picturebox, p1 As Point, p2 As Point, p3 As Point, p4 As Point)
        If p1.X <> p4.X OrElse p1.Y <> p4.Y Then
            Dim rück As New RückgängigGrafik()
            rück.setText("Bezier-Kurve hinzugefügt")
            rück.speicherVorherZustand(sender.getRückArgs())

            Dim w As New Basic_Bezier(sender.getNewID(), 2, p1, p2, p3, p4, New ParamArrow(-1, 100), pfeilEnde.CopyPfeil())
            sender.addElement(w)

            rück.speicherNachherZustand(sender.getRückArgs())
            sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If mode = Modus.Has_P1 Then
            Dim p2 As Point = sender.GetCursorPos
            If p2.X <> p1.X OrElse p2.Y <> p1.Y Then
                Dim p1F As PointF = sender.toPictureboxPoint(p1)
                Dim p2F As PointF = sender.toPictureboxPoint(p2)
                e.Graphics.DrawLine(penRoutingKontrolle, p1F, p2F)
            End If
        ElseIf mode = Modus.Has_P2 Then
            Dim p34 As Point = sender.GetCursorPos()
            If p2.X <> p1.X OrElse p2.Y <> p1.Y Then
                Dim p1F As PointF = sender.toPictureboxPoint(p1)
                Dim p2F As PointF = sender.toPictureboxPoint(p2)
                e.Graphics.DrawLine(penRoutingKontrolle, p1F, p2F)
            End If
            If p34.X <> p1.X OrElse p34.Y <> p1.Y Then
                Dim p1F As PointF = sender.toPictureboxPoint(p1)
                Dim p2F As PointF = sender.toPictureboxPoint(p2)
                Dim p34F As PointF = sender.toPictureboxPoint(p34)

                Dim gr As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getBezierWithPfeil(p1, p2, p34, p34, New ParamArrow(-1, 100), pfeilEnde)
                gr.lineStyle.linestyle = 2
                If TypeOf gr Is DO_MultiGrafik Then
                    DirectCast(gr, DO_MultiGrafik).setLineStyleRekursiv(2)
                End If
                gr.drawGraphics(e.Graphics, e.args_Elemente)
                'e.Graphics.DrawBezier(penRouting, p1F, p2F, p34F, p34F)
            End If
        ElseIf mode = Modus.Has_P4 Then
            Dim p3_gespiegelt As Point = sender.GetCursorPos()
            p3 = New Point(2 * p4.X - p3_gespiegelt.X, 2 * p4.Y - p3_gespiegelt.Y)
            Dim p1F As PointF = sender.toPictureboxPoint(p1)
            Dim p2F As PointF = sender.toPictureboxPoint(p2)
            Dim p3F As PointF = sender.toPictureboxPoint(p3)
            Dim p4F As PointF = sender.toPictureboxPoint(p4)
            If p2.X <> p1.X OrElse p2.Y <> p1.Y Then
                e.Graphics.DrawLine(penRoutingKontrolle, p1F, p2F)
            End If
            If p4.X <> p3.X OrElse p4.Y <> p3.Y Then
                e.Graphics.DrawLine(penRoutingKontrolle, p3F, p4F)
            End If
            If p4.X <> p1.X OrElse p4.Y <> p1.Y Then

                Dim gr As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getBezierWithPfeil(p1, p2, p3, p4, New ParamArrow(-1, 100), pfeilEnde)
                gr.lineStyle.linestyle = 2
                If TypeOf gr Is DO_MultiGrafik Then
                    DirectCast(gr, DO_MultiGrafik).setLineStyleRekursiv(2)
                End If
                gr.drawGraphics(e.Graphics, e.args_Elemente)

                'e.Graphics.DrawBezier(penRouting, p1F, p2F, p3F, p4F)
            End If
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize
        'e.Graphics.DrawBezier(New Pen(e.CursorPen.Color, 1),
        '                                  New PointF(p.X + 0.3F * s.Width, p.Y),
        '                                  New PointF(p.X + 0.0F * s.Width, p.Y + 1.0F * s.Height),
        '                                  New PointF(p.X + 1.0F * s.Width, p.Y + 1.5F * s.Height),
        '                                  New PointF(p.X + 0.7F * s.Width, p.Y + 0.5F * s.Height))

        e.Graphics.DrawBezier(New Pen(e.CursorPen.Color, 1),
                                          New PointF(p.X + 0.0F * s.Width, p.Y + 0.4F * s.Height),
                                          New PointF(p.X + 0.0F * s.Width, p.Y + 1.4F * s.Height),
                                          New PointF(p.X + 1.0F * s.Width, p.Y + -0.4F * s.Height),
                                          New PointF(p.X + 1.0F * s.Width, p.Y + 0.6F * s.Height))
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tool_AddBezier & " " &
               k.keyToolChangeArrow.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerPfeilspitze
    End Function

    Private Enum Modus
        IDLE
        Has_P1
        Has_P2
        Has_P4
    End Enum
End Class
