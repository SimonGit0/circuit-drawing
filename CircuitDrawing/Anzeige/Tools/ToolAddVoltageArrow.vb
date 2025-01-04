Public Class ToolAddVoltageArrow
    Inherits Tool

    Private myDrawFarbe As Color = Color.DarkBlue
    Private myLinestyleList As LineStyleList
    Private positionIndex As Integer

    Private mode As Modus

    Private snappoint As Point?
    Private snappointAtStart As Snappoint
    Private lastSnappoint As Snappoint

    Private start As Point
    Private ziel As Point
    Private pfeil As Integer

    Private hatStartSnap As Boolean
    Private hatZielSnap As Boolean

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        mode = Modus.IDLE

        myLinestyleList = New LineStyleList()
        Dim ls As LineStyle = sender.myLineStyles.getLineStyle(0).copy()
        myLinestyleList.add(ls)
        ls.farbe = New Farbe(myDrawFarbe.A, myDrawFarbe.R, myDrawFarbe.G, myDrawFarbe.B)

        positionIndex = 0
        pfeil = 0

        hatStartSnap = False
        hatZielSnap = False
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        mode = Modus.IDLE

        hatStartSnap = False
        hatZielSnap = False
    End Sub

    Private Function snapCursorPos(sender As Vektor_Picturebox, p As Point) As Point
        If mode = Modus.HatErstenSnapoint AndAlso sender.myMoveRichtung = Vektor_Picturebox.MoveRichtung.NurRechtwinklig Then
            Dim dx As Integer = p.X - start.X
            Dim dy As Integer = p.Y - start.Y
            If Math.Abs(dx) >= Math.Abs(dy) Then
                dy = 0
            Else
                dx = 0
            End If
            Return New Point(start.X + dx, start.Y + dy)
        End If
        Return p
    End Function

    Private Sub setSnappoint(sender As Vektor_Picturebox, pos As Point)
        lastSnappoint = Nothing

        Dim posOnWire As WireSnappoint = sender.getNextPosOnWire(pos)
        Dim snap As Snappoint = sender.sucheNextSnapPoint(pos, snappointAtStart)
        If snap IsNot Nothing AndAlso posOnWire IsNot Nothing Then
            Dim dist1 As Long = Mathe.abstandQuadrat(pos, posOnWire.getMitteInt())
            Dim dist2 As Long = Mathe.abstandQuadrat(pos, snap.p)
            If dist1 < dist2 Then
                snappoint = posOnWire.getMitteInt()
            Else
                snappoint = snap.p
                lastSnappoint = snap
            End If
        ElseIf snap IsNot Nothing Then
            snappoint = snap.p
            lastSnappoint = snap
        ElseIf posOnWire IsNot Nothing Then
            snappoint = posOnWire.getMitteInt()
        Else
            snappoint = Nothing
        End If
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If mode = Modus.IDLE Then
            snappointAtStart = Nothing
            start = Me.snapCursorPos(sender, e.CursorPos)
            setSnappoint(sender, start)
            sender.Invalidate()
        ElseIf mode = Modus.HatErstenSnapoint Then
            ziel = Me.snapCursorPos(sender, e.CursorPos)
            setSnappoint(sender, ziel)
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.IDLE Then
                start = Me.snapCursorPos(sender, sender.GetCursorPos())
                hatStartSnap = False
                mode = Modus.HatErstenSnapoint
                MouseMove(sender, e)
            ElseIf mode = Modus.HatErstenSnapoint Then
                mode = Modus.IDLE
                hatZielSnap = False
                addElement(sender, getVoltageArrow(sender.getNewID()))
                hatStartSnap = False
                hatZielSnap = False
                MouseMove(sender, e)
            End If
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keyToolDrawSnap.isDown(e.KeyCode) Then
            e.Handled = True
            If mode = Modus.IDLE Then
                If snappoint IsNot Nothing Then
                    mode = Modus.HatErstenSnapoint
                    hatStartSnap = True
                    start = snappoint.Value
                    If lastSnappoint IsNot Nothing Then
                        snappointAtStart = lastSnappoint
                    Else
                        snappointAtStart = Nothing
                    End If
                    MouseMove(sender, New ToolMouseEventArgs(sender.GetCursorPos()))
                End If
            ElseIf mode = Modus.HatErstenSnapoint Then
                If snappoint IsNot Nothing Then
                    mode = Modus.IDLE
                    hatZielSnap = True
                    ziel = snappoint.Value
                    addElement(sender, getVoltageArrow(sender.getNewID()))
                    hatStartSnap = False
                    hatZielSnap = False
                    MouseMove(sender, New ToolMouseEventArgs(sender.GetCursorPos()))
                End If
            End If
        ElseIf Key_Settings.getSettings().keySchalteBeschriftungsPosDurch.isDown(e.KeyCode) Then
            e.Handled = True
            If mode = Modus.HatErstenSnapoint Then
                positionIndex += 1
                If positionIndex > 1 Then
                    positionIndex = 0
                End If
                sender.Invalidate()
            End If
        ElseIf Key_Settings.getSettings().keyToolChangeArrow.isDown(e.KeyCode) Then
            e.Handled = True
            If mode = Modus.HatErstenSnapoint Then
                pfeil += 1
                If pfeil >= Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile Then
                    pfeil = 0
                End If
                sender.Invalidate()
            End If
        End If
    End Sub

    Private Sub addElement(sender As Vektor_Picturebox, v As Basic_Spannungspfeil)
        Dim rück As New RückgängigGrafik()
        rück.setText("Spannungspfeil hinzugefügt")
        rück.speicherVorherZustand(sender.getRückArgs())

        sender.addElement(v)

        rück.speicherNachherZustand(sender.getRückArgs())
        sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
    End Sub

    Private Function getVoltageArrow(ID As ULong) As Basic_Spannungspfeil
        Dim distStart As Integer
        Dim distZiel As Integer
        If hatStartSnap Then
            distStart = 100
        Else
            distStart = 0
        End If
        If hatZielSnap Then
            distZiel = 100
        Else
            distZiel = 0
        End If

        Dim vector As Point = New Point(ziel.X - start.X, ziel.Y - start.Y)
        Dim laenge As Single = CSng(Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y))
        Dim v As PointF
        If laenge = 0 Then
            v = New PointF(0, 0)
        Else
            v = New PointF(vector.X / laenge, vector.Y / laenge)
        End If

        Dim s As Point = New Point(CInt(start.X + v.X * distStart), CInt(start.Y + v.Y * distStart))
        Dim z As Point = New Point(CInt(ziel.X - v.X * distZiel), CInt(ziel.Y - v.Y * distZiel))

        Return New Basic_Spannungspfeil(ID, Basic_Spannungspfeil.DEFAULT_LINESTYLE, s, z, New ParamArrow(-1, 100), New ParamArrow(CShort(pfeil), 100), New Beschriftung("$U_1$", positionIndex, DO_Text.TextRotation.Normal, Basic_Spannungspfeil.DEFAULT_ABSTAND_BESCHRIFTUNG, Basic_Spannungspfeil.DEFAULT_ABSTAND_QUER), Basic_Spannungspfeil.DEFAULT_KRÜMMUNG, Basic_Spannungspfeil.DEFAULT_ECKIGKEIT, 0)
    End Function

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If mode = Modus.IDLE Then
            If snappoint IsNot Nothing Then
                sender.drawCursorAtPosition(e, snappoint.Value, Vektor_Picturebox.CursorStyle.Circle, False)
            End If
        ElseIf mode = Modus.HatErstenSnapoint Then
            If snappoint IsNot Nothing Then
                sender.drawCursorAtPosition(e, snappoint.Value, Vektor_Picturebox.CursorStyle.Circle, False)
            End If

            Dim arrow As Basic_Spannungspfeil = getVoltageArrow(0) 'zum malen temporär eine Pfeil erzeugen. --> Die ID ist dafür egal und ist 0. Man will ja auch nicht immer eine neue ID reservieren.

            Dim args As New GrafikDrawArgs(myLinestyleList, sender.myFillStyles, sender.myFonts, sender.calcPixelPerMM(), sender.TextVorschauMode)
            sender.setViewArgs(args)
            Dim g As DO_Grafik = arrow.getGrafik(New getGrafikArgs(False, Nothing, 0))
            If TypeOf g Is DO_MultiGrafik Then
                DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(0)
            Else
                g.lineStyle = New ScaleableLinestyle(0)
            End If
            g.drawGraphics(e.Graphics, args)
        End If
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tools_SpannungspfeilHinzufügen & ": " & My.Resources.Strings.Tools_ClickToAddStartAndEnd & " " &
               k.keyToolDrawSnap.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumSnappenAmNächstenVerbinder & " " &
               k.keySchalteBeschriftungsPosDurch.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerBeschriftung & " " &
               k.keyToolChangeArrow.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerPfeilspitze
    End Function

    Private Enum Modus
        IDLE
        HatErstenSnapoint
    End Enum
End Class
