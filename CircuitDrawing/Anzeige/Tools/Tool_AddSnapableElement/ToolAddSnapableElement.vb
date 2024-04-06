Public MustInherit Class ToolAddSnapableElement
    Inherits Tool

    Protected myDrawFarbe As Color = Color.DarkBlue
    Protected myLinestyleList As LineStyleList
    Protected lastSnappoint As WireSnappoint
    Protected invertRichtung As Boolean = False
    Protected positionIndex As Integer

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)

        myLinestyleList = New LineStyleList()
        Dim ls As LineStyle = sender.myLineStyles.getLineStyle(0).copy()
        myLinestyleList.add(ls)
        ls.farbe = New Farbe(myDrawFarbe.A, myDrawFarbe.R, myDrawFarbe.G, myDrawFarbe.B)

        positionIndex = 0
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Middle Then
            invertRichtung = Not invertRichtung
            sender.Invalidate()
        ElseIf e.Button = MouseButtons.Left Then
            If lastSnappoint IsNot Nothing Then
                Dim rück As New RückgängigGrafik()
                rück.setText(getRückText())
                rück.speicherVorherZustand(sender.getRückArgs())

                sender.addElement(getElement(sender.getNewID()))

                rück.speicherNachherZustand(sender.getRückArgs())
                sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

                sender.Invalidate()
            End If
        End If
    End Sub

    Protected MustOverride Function getRückText() As String

    Protected MustOverride Function getElement(ID As ULong) As SnapableElement


    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        Dim pos As WireSnappoint = sender.getNextPosOnWire(e.CursorPos)
        lastSnappoint = pos
        sender.Invalidate()
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If lastSnappoint IsNot Nothing Then
            Dim arrow As SnapableElement = getElement(0) 'zum malen temporär eine Pfeil erzeugen. --> Die ID ist dafür egal und ist 0. Man will ja auch nicht immer eine neue ID reservieren.

            Dim args As New GrafikDrawArgs(myLinestyleList, sender.myFillStyles, sender.myFonts, sender.calcPixelPerMM(), sender.TextVorschauMode)
            sender.setViewArgs(args)
            Dim g As DO_Grafik = arrow.getGrafik()
            If TypeOf g Is DO_MultiGrafik Then
                DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(0)
            Else
                g.lineStyle = New ScaleableLinestyle(0)
            End If
            g.drawGraphics(e.Graphics, args)
        End If
    End Sub

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        If Key_Settings.getSettings().keySchalteBeschriftungsPosDurch.isDown(e.KeyCode) Then
            e.Handled = True
            If lastSnappoint IsNot Nothing Then
                positionIndex += 1
                If positionIndex > 1 Then
                    positionIndex = 0
                End If
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return "'" & My.Resources.Strings.Mittlere_Maustaste & "' " & My.Resources.Strings.Tools_ZumDrehen & " " &
               k.keySchalteBeschriftungsPosDurch.getStatusStripString() & " " & My.Resources.Strings.Tools_ZumDurchschaltenDerBeschriftung
    End Function

End Class
