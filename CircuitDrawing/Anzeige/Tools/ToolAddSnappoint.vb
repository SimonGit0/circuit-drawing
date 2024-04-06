Public Class ToolAddSnappoint
    Inherits Tool

    Private nextSnappoint_Element As IElementWithAddableSnappoint
    Private nextSnappoint As Point
    Private hat_Snappoint As Boolean

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        MouseMove(sender, New ToolMouseEventArgs(sender.GetCursorPos()))
        sender.showSnappoints = True
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        MouseMove(sender, New ToolMouseEventArgs(sender.GetCursorPos()))
        sender.showSnappoints = True
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        sender.showSnappoints = False
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        sender.showSnappoints = False
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        Me.hat_Snappoint = False

        Dim minDist As Long = Long.MaxValue
        For Each el As ElementMaster In sender.ElementListe
            If TypeOf el Is IElementWithAddableSnappoint Then
                Dim p As Nullable(Of Point) = DirectCast(el, IElementWithAddableSnappoint).getNextAddableSnappoint(e.CursorPos, minDist)
                If p.HasValue Then
                    Dim dist As Long = Mathe.abstandQuadrat(e.CursorPos, p.Value)
                    If dist < minDist Then
                        minDist = dist
                        Me.hat_Snappoint = True
                        Me.nextSnappoint = p.Value
                        Me.nextSnappoint_Element = DirectCast(el, IElementWithAddableSnappoint)
                    End If
                End If
            End If
        Next

        sender.Invalidate()
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If (e.Button And MouseButtons.Left) = MouseButtons.Left Then
            If hat_Snappoint Then
                Dim rück As New RückgängigGrafik()
                rück.setText("Snappoint hinzugefügt")
                rück.speicherVorherZustand(sender.getRückArgs())
                If nextSnappoint_Element.addSnappoint(e.CursorPos) Then
                    rück.speicherNachherZustand(sender.getRückArgs())
                    sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
                End If
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If hat_Snappoint Then
            sender.drawCursorAtPosition(e, nextSnappoint, Vektor_Picturebox.CursorStyle.Circle, False)
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        Dim pen As Pen = Pens.Blue
        Dim fc As Single = 0.7
        e.Graphics.DrawLine(pen, p, New PointF(p.X + s.Width * fc, p.Y + s.Height * fc))
        e.Graphics.DrawLine(pen, New PointF(p.X, p.Y + s.Height * fc), New PointF(p.X + s.Width * fc, p.Y))
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tools_SnappointHinzufügen
    End Function
End Class

Public Interface IElementWithAddableSnappoint

    'Gebe nächsten schon vorhandenen Snappoint aus (z.b. zum löschen)
    '-1 für kein Snappoint ist da (in der Nähe)!
    Function getNextSnappoint(cursorPos As Point, max_allowed_dist_from_cursor As Long) As Integer

    'Suche nächsten Punkt an dem ein Snappoint hinzugefügt werden kann
    Function getNextAddableSnappoint(cursorPos As Point, max_allowed_dist_from_cursor As Long) As Nullable(Of Point)

    Function addSnappoint(cursorPos As Point) As Boolean

    Function getSnappointSimple(index As Integer) As Point

    Sub deleteSnappoint(index As Integer)
End Interface