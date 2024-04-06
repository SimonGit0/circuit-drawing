Public Class ToolScale
    Inherits Tool

    Private amZiehen As Boolean
    Private ziehKante As ScaleKante
    Private refZiehen As Point
    Private rück As RückgängigGrafik
    Private myRückCurrentMove As RückgängigGrafik

    Private penAmZiehen, penIDLE As Pen

    Private mouseDownPoint As Point
    Private offsetAtMouseDown As Point

#Region "Anmelden/Abmelden/Pause"
    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        amZiehen = False
        ziehKante = Nothing

        penAmZiehen = New Pen(Color.FromArgb(128, Color.Orange), 6)
        penIDLE = New Pen(Color.FromArgb(128, Color.Orange), 3)
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        amZiehen = False
        ziehKante = Nothing
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        abbort(sender)
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        abbort(sender)
    End Sub
#End Region

    Private Sub updateScaleKante(sender As Vektor_Picturebox)
        Dim kante As ScaleKante = sender.getNextScaleKante(sender.GetCursorPosOffgrid())
        If kante IsNot Nothing Then
            ziehKante = kante
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If e.KeyCode = Keys.ControlKey Or e.KeyCode = Keys.Control Or e.KeyCode = Keys.LControlKey OrElse e.KeyCode = Keys.RControlKey Then
            If Not amZiehen Then
                updateScaleKante(sender)
            End If
        End If
    End Sub

    Public Overrides Sub KeyUp(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If e.KeyCode = Keys.ControlKey Or e.KeyCode = Keys.Control Or e.KeyCode = Keys.LControlKey OrElse e.KeyCode = Keys.RControlKey Then
            If Not amZiehen Then
                updateScaleKante(sender)
            End If
        End If
    End Sub

    Public Overrides Sub MouseMoveOffgrid(sender As Vektor_Picturebox, e As ToolMouseMoveOffgridEventArgs)
        If amZiehen Then
            If TypeOf Me.ziehKante Is ScaleKanteBasicGrafikElement Then
                Dim alleKanten As List(Of Kante) = DirectCast(Me.ziehKante, ScaleKanteBasicGrafikElement).mykanten
                Dim myZiehKante As Kante = alleKanten(0)
                If myZiehKante.offgrid OrElse e.isOnGridMove Then
                    Dim cursorPos As Point = e.CursorPos
                    If myZiehKante.offgrid Then
                        cursorPos = sender.GetCursorPosOffgrid()
                    End If

                    Dim dx As Integer = cursorPos.X - refZiehen.X
                    Dim dy As Integer = cursorPos.Y - refZiehen.Y

                    If myZiehKante.isOnlyStartpunkt Then
                        'prüfen des Modes!
                        If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                            Dim aktiviere_rechtwinkligZuEndeZiehen As Boolean = True
                            If myZiehKante.rechtwinkligZuEndeZiehen AndAlso alleKanten.Count > 1 Then
                                'Bei mehreren Kanten macht rechtwinklig zu Ende ziehen unter umständen nicht so viel sinn, da man eh in x und y richtung ziehen kann (Ecke zwischen zwei linien)
                                Dim kannX As Boolean = False
                                Dim kannY As Boolean = False
                                For Each k As Kante In alleKanten
                                    If k.start.Y <> k.ende.Y Then
                                        kannY = True
                                    End If
                                    If k.start.X <> k.ende.X Then
                                        kannX = True
                                    End If
                                Next
                                If kannX AndAlso kannY Then
                                    aktiviere_rechtwinkligZuEndeZiehen = False
                                End If
                            End If
                            If myZiehKante.rechtwinkligZuEndeZiehen AndAlso aktiviere_rechtwinkligZuEndeZiehen Then
                                Dim deltaGesamt As Point = New Point(cursorPos.X + offsetAtMouseDown.X - myZiehKante.ende.X, cursorPos.Y + offsetAtMouseDown.Y - myZiehKante.ende.Y)
                                Dim neuerPunkt As Point
                                If Math.Abs(deltaGesamt.X) > Math.Abs(deltaGesamt.Y) Then
                                    'nur in X-Richtung ziehen!
                                    neuerPunkt = New Point(cursorPos.X + offsetAtMouseDown.X, myZiehKante.ende.Y)
                                Else
                                    neuerPunkt = New Point(myZiehKante.ende.X, cursorPos.Y + offsetAtMouseDown.Y)
                                End If
                                dx = neuerPunkt.X - offsetAtMouseDown.X - refZiehen.X
                                dy = neuerPunkt.Y - offsetAtMouseDown.Y - refZiehen.Y
                                refZiehen = New Point(neuerPunkt.X - offsetAtMouseDown.X, neuerPunkt.Y - offsetAtMouseDown.Y)
                            Else
                                Dim deltaGesamt As Point = New Point(cursorPos.X - mouseDownPoint.X, cursorPos.Y - mouseDownPoint.Y)
                                Dim neuerPunkt As Point
                                If Math.Abs(deltaGesamt.X) > Math.Abs(deltaGesamt.Y) Then
                                    'nur in X-Richtung ziehen!
                                    neuerPunkt = New Point(cursorPos.X, mouseDownPoint.Y)
                                Else
                                    neuerPunkt = New Point(mouseDownPoint.X, cursorPos.Y)
                                End If
                                dx = neuerPunkt.X - refZiehen.X
                                dy = neuerPunkt.Y - refZiehen.Y
                                refZiehen = neuerPunkt
                            End If
                        Else
                            refZiehen = cursorPos
                        End If
                    Else
                        refZiehen = cursorPos
                    End If

                    If dx <> 0 OrElse dy <> 0 Then
                        Dim invalidate As Boolean = False
                        If myZiehKante.sender.ScaleKante(myZiehKante, dx, dy, invalidate) Then
                            Dim kantenRest As List(Of Kante) = DirectCast(Me.ziehKante, ScaleKanteBasicGrafikElement).mykanten
                            Me.ziehKante = New ScaleKanteBasicGrafikElement(myZiehKante.sender.getScaleKante(myZiehKante.KantenIndex, myZiehKante))

                            For i As Integer = 1 To kantenRest.Count - 1
                                If Not kantenRest(i).sender.ScaleKante(kantenRest(i), dx, dy, invalidate) Then
                                    MessageBox.Show("Allgemeiner Fehler beim Skalieren. Das Skalieren wird abgebrochen.", "Fehler beim Skalieren", MessageBoxButtons.OK)
                                    abbort(sender)
                                    Exit Sub
                                End If
                                DirectCast(Me.ziehKante, ScaleKanteBasicGrafikElement).mykanten.Add(kantenRest(i).sender.getScaleKante(kantenRest(i).KantenIndex, kantenRest(i)))
                            Next

                            If invalidate Then
                                sender.Invalidate()
                            End If
                        Else
                            'Fehler beim skalieren!
                            MessageBox.Show("Allgemeiner Fehler beim Skalieren. Das Skalieren wird abgebrochen.", "Fehler beim Skalieren", MessageBoxButtons.OK)
                            abbort(sender)
                            Exit Sub
                        End If
                    End If
                End If
            ElseIf TypeOf Me.ziehKante Is ScaleKante_Wire Then
                If e.isOnGridMove Then 'Wires kann man nur Ongrid ziehen!
                    Dim pos As Point = DirectCast(Me.ziehKante, ScaleKante_Wire).pos

                    'mache rückgängig
                    myRückCurrentMove.machRückgängigInklusiveKorrekterSelection(sender.getRückArgs())

                    Dim dx As Integer = e.CursorPos.X - refZiehen.X
                    Dim dy As Integer = e.CursorPos.Y - refZiehen.Y
                    If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                        If Math.Abs(dx) > Math.Abs(dy) Then
                            dy = 0
                        Else
                            dx = 0
                        End If
                    End If


                    sender.moveWirePos(pos, dx, dy)
                End If
            End If
        Else
            updateScaleKante(sender)
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If Not amZiehen Then
            If e.Button = MouseButtons.Left AndAlso ziehKante IsNot Nothing Then
                amZiehen = True
                mouseDownPoint = e.CursorPos
                Dim offgrid As Boolean = False
                If TypeOf ziehKante Is ScaleKanteBasicGrafikElement Then
                    Dim kante As Kante = DirectCast(ziehKante, ScaleKanteBasicGrafikElement).mykanten(0)
                    offsetAtMouseDown = New Point(kante.start.X - e.CursorPos.X, kante.start.Y - e.CursorPos.Y)
                    offgrid = kante.offgrid
                ElseIf TypeOf ziehKante Is ScaleKante_Wire Then
                    Dim pos As Point = DirectCast(ziehKante, ScaleKante_Wire).pos
                    offsetAtMouseDown = pos
                    myRückCurrentMove = New RückgängigGrafik(True)
                    myRückCurrentMove.speicherVorherZustand(sender.getRückArgs())
                    offgrid = False 'wires werden immer on-grid skaliert!
                End If
                If offgrid Then
                    refZiehen = sender.GetCursorPosOffgrid()
                Else
                    refZiehen = e.CursorPos
                End If
                rück = New RückgängigGrafik()
                rück.setText("Skalierung")
                rück.speicherVorherZustand(sender.getRückArgs())
                sender.Invalidate()
            End If
        ElseIf amZiehen Then
            If e.Button = MouseButtons.Left Then
                rück.speicherNachherZustand(sender.getRückArgs())
                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
                rück = Nothing
                myRückCurrentMove = Nothing
                amZiehen = False
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Function abortAction(sender As Vektor_Picturebox) As Boolean
        If amZiehen Then
            Me.abbort(sender)
            Return True
        End If
        Return False
    End Function

    Private Sub abbort(sender As Vektor_Picturebox)
        If amZiehen Then

            rück.macheRückgängig(sender.getRückArgs())
            rück = Nothing

            amZiehen = False
            ziehKante = Nothing
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If ziehKante IsNot Nothing Then
            If TypeOf Me.ziehKante Is ScaleKanteBasicGrafikElement Then
                Dim alleKanten As List(Of Kante) = DirectCast(Me.ziehKante, ScaleKanteBasicGrafikElement).mykanten
                Dim myZiehKante As Kante = alleKanten(0)
                Dim p1 As PointF = sender.toPictureboxPoint(myZiehKante.start)
                Dim p2 As PointF = sender.toPictureboxPoint(myZiehKante.ende)

                Dim pen As Pen
                If amZiehen Then
                    pen = penAmZiehen
                Else
                    pen = penIDLE
                End If
                If myZiehKante.isOnlyStartpunkt Then
                    e.Graphics.DrawEllipse(pen, p1.X - 5.0F, p1.Y - 5.0F, 11.0F, 11.0F)
                    If myZiehKante.ZeichneImmerHilfslinie Then
                        e.Graphics.DrawLine(pen, p1, p2)
                        e.Graphics.DrawLine(pen, p2.X - 5.0F, p2.Y - 5.0F, p2.X + 5.0F, p2.Y + 5.0F)
                        e.Graphics.DrawLine(pen, p2.X + 5.0F, p2.Y - 5.0F, p2.X - 5.0F, p2.Y + 5.0F)
                        For i As Integer = 1 To alleKanten.Count - 1
                            If alleKanten(i).ZeichneImmerHilfslinie Then
                                Dim pk1 As PointF = sender.toPictureboxPoint(alleKanten(i).start)
                                Dim pk2 As PointF = sender.toPictureboxPoint(alleKanten(i).ende)
                                e.Graphics.DrawLine(pen, pk1, pk2)
                                e.Graphics.DrawEllipse(pen, pk1.X - 5.0F, pk1.Y - 5.0F, 11.0F, 11.0F)
                            End If
                        Next
                    End If
                Else
                    e.Graphics.DrawLine(pen, p1, p2)
                End If
            ElseIf TypeOf Me.ziehKante Is ScaleKante_Wire Then
                Dim pos_Int As Point = DirectCast(Me.ziehKante, ScaleKante_Wire).pos
                If amZiehen Then
                    Dim pos_cursor As Point = sender.GetCursorPos()
                    Dim dx As Integer = pos_cursor.X - refZiehen.X
                    Dim dy As Integer = pos_cursor.Y - refZiehen.Y
                    If sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
                        If Math.Abs(dx) > Math.Abs(dy) Then
                            dy = 0
                        Else
                            dx = 0
                        End If
                    End If
                    pos_Int.X += dx
                    pos_Int.Y += dy
                End If
                Dim pos As PointF = sender.toPictureboxPoint(pos_Int)
                Dim pen As Pen
                If amZiehen Then
                    pen = penAmZiehen
                Else
                    pen = penIDLE
                End If
                e.Graphics.DrawEllipse(pen, pos.X - 5.0F, pos.Y - 5.0F, 11.0F, 11.0F)
            End If
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        Dim aL As Single = 0.2

        e.Graphics.DrawLine(e.CursorPen, New PointF(p.X, p.Y + 0.5F * s.Height), New PointF(p.X + s.Width, p.Y + 0.5F * s.Height))
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + aL * s.Width, p.Y + s.Height * (0.5F - aL)),
                                           New PointF(p.X, p.Y + s.Height * 0.5F),
                                           New PointF(p.X + aL * s.Width, p.Y + s.Height * (0.5F + aL))})
        e.Graphics.DrawLines(e.CursorPen, {New PointF(p.X + (1.0F - aL) * s.Width, p.Y + s.Height * (0.5F - aL)),
                                           New PointF(p.X + s.Width, p.Y + s.Height * 0.5F),
                                           New PointF(p.X + (1.0F - aL) * s.Width, p.Y + s.Height * (0.5F + aL))})
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tool_Scale
    End Function

End Class

Public MustInherit Class ScaleKante

End Class

Public Class ScaleKanteBasicGrafikElement
    Inherits ScaleKante

    Public mykanten As List(Of Kante)
    Public Sub New(k As Kante)
        mykanten = New List(Of Kante)(1)
        mykanten.Add(k)
    End Sub
End Class

Public Class ScaleKante_Wire
    Inherits ScaleKante

    Public pos As Point

    Public Sub New(pos As Point)
        Me.pos = pos
    End Sub
End Class
