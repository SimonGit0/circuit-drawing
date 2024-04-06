Public Class ToolDeleteSnappoint
    Inherits Tool

    Private nextSnappoint_Element As IElementWithAddableSnappoint
    Private nextSnappoint As Integer
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
                Dim index As Integer = DirectCast(el, IElementWithAddableSnappoint).getNextSnappoint(e.CursorPos, minDist)
                If index >= 0 Then
                    Dim p As Point = DirectCast(el, IElementWithAddableSnappoint).getSnappointSimple(index)
                    Dim dist As Long = Mathe.abstandQuadrat(e.CursorPos, p)
                    If dist < minDist Then
                        minDist = dist
                        Me.hat_Snappoint = True
                        Me.nextSnappoint = index
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
                rück.setText("Snappoint gelöscht")
                rück.speicherVorherZustand(sender.getRückArgs())
                nextSnappoint_Element.deleteSnappoint(Me.nextSnappoint)
                rück.speicherNachherZustand(sender.getRückArgs())
                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

                'Neuen Snappoint markieren
                MouseMove(sender, New ToolMouseEventArgs(sender.GetCursorPos()))
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If hat_Snappoint Then
            Dim p As Point = nextSnappoint_Element.getSnappointSimple(Me.nextSnappoint)
            sender.drawCursorAtPosition(e, p, Vektor_Picturebox.CursorStyle.FatCross, False)
        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize

        Dim pen As New Pen(e.CursorPen.Color, 2)

        e.Graphics.DrawLine(pen, p, New PointF(p.X + s.Width, p.Y + s.Height))
        e.Graphics.DrawLine(pen, New PointF(p.X, p.Y + s.Height), New PointF(p.X + s.Width, p.Y))
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Return My.Resources.Strings.Tools_SnappointLöschen
    End Function
End Class
