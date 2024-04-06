Public Class ToolDrawRect
    Inherits Tool

    Public ReadOnly form As FormArt
    Private isDrawing As Boolean
    Private start As Point
    Private ziel As Point

    Public Sub New(form As FormArt)
        Me.form = form
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        isDrawing = False
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        isDrawing = False
    End Sub


    Public Overrides Sub MouseDown(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            start = e.CursorPos
            isDrawing = True
        End If
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If isDrawing Then
            ziel = e.CursorPos
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If isDrawing AndAlso e.Button = MouseButtons.Left Then
            ziel = e.CursorPos
            If start.X <> ziel.X OrElse start.Y <> ziel.Y Then
                Dim minX As Integer = Math.Min(start.X, ziel.X)
                Dim minY As Integer = Math.Min(start.Y, ziel.Y)
                Dim maxX As Integer = Math.Max(start.X, ziel.X)
                Dim maxY As Integer = Math.Max(start.Y, ziel.Y)

                Dim rück As New RückgängigGrafik()
                rück.speicherVorherZustand(sender.getRückArgs())

                Select Case form
                    Case FormArt.Rect
                        rück.setText("Rechteck hinzugefügt")
                        sender.addElement(New Element_Rect(sender.getNewID(), New Point(minX, minY), maxX - minX, maxY - minY, 2, 0, Element_Rect.Modus.EinRect, New Point(Element_Rect.DefaultMultiRectVec_X, Element_Rect.DefaultMultiRectVec_Y), Element_Rect.DefaultMultiRectAnzahlStart))
                    Case FormArt.Ellipse
                        rück.setText("Ellipse hinzugefügt")
                        sender.addElement(New Element_Ellipse(sender.getNewID(), New Point(minX, minY), maxX - minX, maxY - minY, 2, 0))
                    Case FormArt.Textfeld
                        rück.setText("Textfeld hinzugefügt")
                        sender.addElement(New Element_Textfeld(sender.getNewID(), New Point(minX, minY), maxX - minX, maxY - minY, 2, 0, 0, "Neues Textfeld", DO_Textfeld.AlignH.Mitte, DO_Text.AlignV.Mitte))
                    Case FormArt.RoundRect
                        rück.setText("Rundes Rechteck hinzugefügt")
                        Dim radius As Integer = Math.Min(maxX - minX, maxY - minY) \ 10
                        sender.addElement(New Element_RoundRect(sender.getNewID(), New Point(minX, minY), maxX - minX, maxY - minY, radius, 2, 0))
                    Case FormArt.Graph
                        rück.setText("Graph hinzugefügt")
                        sender.addElement(New Element_Graph(sender.getNewID(), New Point(minX, minY), maxX - minX, maxY - minY, 0, 0))
                End Select

                rück.speicherNachherZustand(sender.getRückArgs())
                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

                sender.Invalidate()
            End If
            isDrawing = False
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If isDrawing Then
            Dim p1 As PointF = sender.toPictureboxPoint(start)
            Dim p2 As PointF = sender.toPictureboxPoint(ziel)
            Dim minX As Single = Math.Min(p1.X, p2.X)
            Dim minY As Single = Math.Min(p1.Y, p2.Y)
            Dim maxX As Single = Math.Max(p1.X, p2.X)
            Dim maxY As Single = Math.Max(p1.Y, p2.Y)

            Select Case form
                Case FormArt.Rect
                    e.Graphics.DrawRectangle(Pens.Black, minX, minY, maxX - minX, maxY - minY)
                Case FormArt.Ellipse
                    e.Graphics.DrawEllipse(Pens.Black, minX, minY, maxX - minX, maxY - minY)
                Case FormArt.Textfeld
                    e.Graphics.DrawRectangle(Pens.Black, minX, minY, maxX - minX, maxY - minY)
                Case FormArt.RoundRect
                    Dim radius As Single = 0.1F * Math.Min(maxX - minX, maxY - minY)
                    Dim p As Drawing2D.GraphicsPath = Mathe.getRoundRect(minX, minY, maxX - minX, maxY - minY, radius)
                    e.Graphics.DrawPath(Pens.Black, p)
                Case FormArt.Graph
                    e.Graphics.DrawRectangle(Pens.Black, minX, minY, maxX - minX, maxY - minY)
            End Select

        End If
    End Sub

    Public Overrides Sub OnDrawCursorExtension(sender As Vektor_Picturebox, e As PaintCursorEventArgs)
        Dim p As PointF = e._Default_CursorExtensionTopLeft
        Dim s As SizeF = e._Default_CursorExtensionSize
        Select Case form
            Case FormArt.Rect, FormArt.RoundRect, FormArt.Textfeld, FormArt.Graph
                e.Graphics.DrawRectangle(e.CursorPen, p.X, p.Y + 0.2F * s.Height, s.Width * 0.4F, 0.6F * s.Height)
            Case FormArt.Ellipse
                e.Graphics.DrawEllipse(e.CursorPen, p.X, p.Y + 0.2F * s.Height, s.Width * 0.4F, 0.6F * s.Height)
        End Select
        e.Graphics.DrawLine(e.CursorPen, p.X + 0.75F * s.Width, p.Y, p.X + 0.75F * s.Width, p.Y + s.Height * 0.5F)
        e.Graphics.DrawLine(e.CursorPen, p.X + 0.5F * s.Width, p.Y + 0.25F * s.Height, p.X + s.Width, p.Y + 0.25F * s.Height)
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Select Case form
            Case FormArt.Rect
                Return My.Resources.Strings.Tool_AddRect
            Case FormArt.Ellipse
                Return My.Resources.Strings.Tool_AddEllipse
            Case FormArt.Textfeld
                Return My.Resources.Strings.Tool_AddTextfeld
            Case FormArt.RoundRect
                Return My.Resources.Strings.Tool_AddRountRect
        End Select
        Return My.Resources.Strings.Tool_AddForm
    End Function

    Public Enum FormArt
        Rect
        Ellipse
        Textfeld
        RoundRect
        Graph
    End Enum

End Class
