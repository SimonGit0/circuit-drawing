Public Class ToolPlace
    Inherits Tool

    Private myPen As Pen = Pens.DarkBlue
    Private myLineStyles As LineStyleList
    Private myFontStyles As FontList
    Private drehung As Drehmatrix
    Private myTextPos As Integer
    Private currentName As String

    Private lastTemplate As String = ""
    Private lastTemplateCompiled As Template_Compiled = Nothing

    Private lastGravityPointsX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsY As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateX As List(Of GravityPoint) = Nothing
    Private lastGravityPointsTemplateY As List(Of GravityPoint) = Nothing

    Public Sub New()
    End Sub

    Public Overrides Sub meldeAn(sender As Vektor_Picturebox)
        MyBase.meldeAn(sender)
        drehung = Drehmatrix.getIdentity()
        onDrehungChanged()
        myTextPos = 0
        initLineStyle(sender)
        currentName = sender.getCurrentTemplateBauteilName()
        If getTemplate(sender) IsNot Nothing Then
            sender.showSnappoints = True
        Else
            sender.showSnappoints = False
        End If
    End Sub

    Public Overrides Sub weiter(sender As Vektor_Picturebox)
        MyBase.weiter(sender)
        drehung = Drehmatrix.getIdentity()
        onDrehungChanged()
        myTextPos = 0
        currentName = sender.getCurrentTemplateBauteilName()
        If getTemplate(sender) IsNot Nothing Then
            sender.showSnappoints = True
        Else
            sender.showSnappoints = False
        End If
    End Sub

    Public Overrides Sub meldeAb(sender As Vektor_Picturebox)
        MyBase.meldeAb(sender)
        sender.showSnappoints = False
        lastTemplateCompiled = Nothing
        lastGravityPointsX = Nothing
        lastGravityPointsY = Nothing
        lastGravityPointsTemplateX = Nothing
        lastGravityPointsTemplateY = Nothing
    End Sub

    Public Overrides Sub pause(sender As Vektor_Picturebox)
        MyBase.pause(sender)
        sender.showSnappoints = False
        lastTemplateCompiled = Nothing
        lastGravityPointsX = Nothing
        lastGravityPointsY = Nothing
        lastGravityPointsTemplateX = Nothing
        lastGravityPointsTemplateY = Nothing
    End Sub

    Private Sub initLineStyle(sender As Vektor_Picturebox)
        myLineStyles = New LineStyleList()
        Dim ls As LineStyle = sender.myLineStyles.getLineStyle(0).copy()
        ls.farbe = New Farbe(255, 0, 0, 128)
        myLineStyles.add(ls)

        Dim ls2 As LineStyle = ls.copy()
        ls2.farbe = New Farbe(128, 255, 0, 0)
        ls2.Dicke = ls.Dicke * 3
        myLineStyles.add(ls2)

        myFontStyles = New FontList()
        Dim fs As FontStyle = sender.myFonts.getFontStyle(0).copy()
        fs.farbe = New Farbe(255, 0, 0, 128)
        myFontStyles.add(fs)
    End Sub

    Public Overrides Sub MouseMove(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If getTemplate(sender) IsNot Nothing Then
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If getTemplate(sender) IsNot Nothing Then

                Dim template As TemplateAusDatei = getTemplate(sender)

                Dim rück As New RückgängigGrafik()
                rück.setText(template.getName() & " hinzugefügt")
                rück.speicherVorherZustand(sender.getRückArgs())
                Dim changed As Boolean = True
                If template.getIsDeko() Then
                    Dim b As BauteilAusDatei = getBauteilUnderCursor(sender, e.CursorPos)
                    If b IsNot Nothing Then
                        b.addDeko(New Deko_Bauteil(template, b))
                    Else
                        'kein Rück
                        changed = False
                    End If
                Else
                    Dim pos As Point = e.CursorPos
                    If sender.enable_gravity Then
                        If lastGravityPointsX Is Nothing Then lastGravityPointsX = sender.getGravityPointsX(True, True)
                        If lastGravityPointsY Is Nothing Then lastGravityPointsY = sender.getGravityPointsY(True, True)
                        If lastGravityPointsTemplateX Is Nothing Then lastGravityPointsTemplateX = getGravityTemplateX(lastTemplateCompiled)
                        If lastGravityPointsTemplateY Is Nothing Then lastGravityPointsTemplateY = getGravityTemplateY(lastTemplateCompiled)

                        pos = sender.performGravity(False, False, pos, pos, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY).posResult
                    Else
                        lastGravityPointsX = Nothing
                        lastGravityPointsY = Nothing
                        lastGravityPointsTemplateX = Nothing
                        lastGravityPointsTemplateY = Nothing
                    End If
                    sender.addElement(New BauteilAusDatei(sender.getNewID(), pos, template, drehung, New Beschriftung(currentName, myTextPos, DO_Text.TextRotation.Normal, BauteilAusDatei.DEFAULT_ABSTAND_TEXT, BauteilAusDatei.DEFAULT_ABSTAND_QUER), 0, BauteilAusDatei.DefaultFillstyle))
                End If
                If changed Then
                    rück.speicherNachherZustand(sender.getRückArgs())
                    sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
                    If sender.enable_gravity Then
                        lastGravityPointsX = sender.getGravityPointsX(True, True)
                        lastGravityPointsY = sender.getGravityPointsY(True, True)
                    Else
                        lastGravityPointsX = Nothing
                        lastGravityPointsY = Nothing
                    End If
                End If
                currentName = sender.getCurrentTemplateBauteilName()

                sender.Invalidate()

            End If
        ElseIf e.Button = MouseButtons.Middle Then
            drehung.um90GradDrehen()
            onDrehungChanged()
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        Dim template As TemplateAusDatei = getTemplate(sender)
        If template IsNot Nothing Then
            Dim template_compiled As Template_Compiled = Nothing
            Dim pos As Point = sender.GetCursorPos()


            If template.getIsDeko() Then
                Dim b As BauteilAusDatei = getBauteilUnderCursor(sender, pos)
                If b IsNot Nothing Then
                    template.recompile(Nothing, myTextPos, template_compiled, New CompileParentArgs(b))
                    Dim g As DO_Grafik = template_compiled.getGrafik()

                    Dim t As New TransformMulti()
                    t.add(New Transform_rotate(b.getDrehmatrix()))
                    t.add(New Transform_translate(b.position))
                    g.transform(t)

                    Dim args As New GrafikDrawArgs(myLineStyles, sender.myFillStyles, myFontStyles, sender.calcPixelPerMM(), sender.TextVorschauMode)
                    sender.setViewArgs(args)
                    g.drawGraphics(e.Graphics, args)


                    Dim gs As DO_Grafik = b.getSelection().getGrafik()
                    gs.lineStyle = New ScaleableLinestyle(1)
                    gs.drawGraphics(e.Graphics, args)
                End If
            Else
                If lastTemplateCompiled IsNot Nothing Then
                    template_compiled = lastTemplateCompiled
                Else
                    template.recompile(Nothing, myTextPos, template_compiled, Nothing)
                    lastTemplateCompiled = template_compiled
                End If
                If sender.enable_gravity Then
                    If lastGravityPointsX Is Nothing Then lastGravityPointsX = sender.getGravityPointsX(True, True)
                    If lastGravityPointsY Is Nothing Then lastGravityPointsY = sender.getGravityPointsY(True, True)
                    If lastGravityPointsTemplateX Is Nothing Then lastGravityPointsTemplateX = getGravityTemplateX(lastTemplateCompiled)
                    If lastGravityPointsTemplateY Is Nothing Then lastGravityPointsTemplateY = getGravityTemplateY(lastTemplateCompiled)

                    Dim gravityResult As GravityDrawState = sender.performGravity(False, False, pos, pos, lastGravityPointsTemplateX, lastGravityPointsTemplateY, lastGravityPointsX, lastGravityPointsY)
                    pos = gravityResult.posResult
                    gravityResult.draw(e.Graphics, sender)
                Else
                    lastGravityPointsX = Nothing
                    lastGravityPointsY = Nothing
                    lastGravityPointsTemplateX = Nothing
                    lastGravityPointsTemplateY = Nothing
                End If

                Dim g As DO_Grafik = template_compiled.getGrafik()
                Dim t As New TransformMulti()
                t.add(New Transform_rotate(drehung))
                t.add(New Transform_translate(pos))
                g.transform(t)

                BauteilAusDatei.AddBeschriftungToGrafik(DirectCast(g, DO_MultiGrafik), New Beschriftung(currentName, myTextPos, DO_Text.TextRotation.Normal, BauteilAusDatei.DEFAULT_ABSTAND_TEXT, BauteilAusDatei.DEFAULT_ABSTAND_QUER), template, template_compiled, pos, drehung)

                Dim args As New GrafikDrawArgs(myLineStyles, sender.myFillStyles, myFontStyles, sender.calcPixelPerMM(), sender.TextVorschauMode)
                sender.setViewArgs(args)
                g.drawGraphics(e.Graphics, args)
            End If
        End If
    End Sub

    Public Overrides Sub OnGravityChanged(sender As Vektor_Picturebox, e As EventArgs)
        If getTemplate(sender) IsNot Nothing Then
            sender.Invalidate()
        End If
    End Sub

    Private Function getGravityTemplateX(t As Template_Compiled) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)(t.getNrOfSnappoints())
        For i As Integer = 0 To t.getNrOfSnappoints() - 1
            Dim p As Snappoint = t.getSnappoint(i)
            drehung.transform(p)
            erg.Add(New GravityPoint(p.p.X, p.p.Y, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
        Next
        Return erg
    End Function
    Private Function getGravityTemplateY(t As Template_Compiled) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)(t.getNrOfSnappoints())
        For i As Integer = 0 To t.getNrOfSnappoints() - 1
            Dim p As Snappoint = t.getSnappoint(i)
            drehung.transform(p)
            erg.Add(New GravityPoint(p.p.Y, p.p.X, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
        Next
        Return erg
    End Function
    Private Sub onDrehungChanged()
        lastGravityPointsTemplateX = Nothing
        lastGravityPointsTemplateY = Nothing
    End Sub

    Private Function getBauteilUnderCursor(sender As Vektor_Picturebox, pos As Point) As BauteilAusDatei
        Dim elemente As List(Of ElementMaster) = sender.ElementListe
        Dim minDist As Long = Long.MaxValue
        Dim minB As BauteilAusDatei = Nothing

        For i As Integer = 0 To elemente.Count - 1
            If TypeOf elemente(i) Is BauteilAusDatei Then
                Dim select_b As Selection = DirectCast(elemente(i), BauteilAusDatei).getSelection()
                If TypeOf select_b Is SelectionRect Then
                    Dim rb As Rectangle = DirectCast(select_b, SelectionRect).r

                    Dim mitte As New Point(rb.X + rb.Width \ 2, rb.Y + rb.Height \ 2)
                    Dim distQ As Long = (CLng(mitte.X) - pos.X) * (CLng(mitte.X) - pos.X) + (CLng(mitte.Y) - pos.Y) * (CLng(mitte.Y) - pos.Y)
                    If distQ < minDist Then
                        minDist = distQ
                        minB = DirectCast(elemente(i), BauteilAusDatei)
                    End If
                End If
            End If
        Next

        Return minB
    End Function

    Public Overrides Sub KeyDown(sender As Vektor_Picturebox, e As ToolKeyEventArgs)
        MyBase.KeyDown(sender, e)
        If Key_Settings.getSettings().keySchalteBeschriftungsPosDurch.isDown(e.KeyCode) Then
            Dim t As TemplateAusDatei = getTemplate(sender)
            If t IsNot Nothing AndAlso t.getNrOfTextPoints() > 1 Then
                myTextPos += 1
                While myTextPos >= t.getNrOfTextPoints()
                    myTextPos -= t.getNrOfTextPoints()
                End While
                If myTextPos < 0 Then
                    myTextPos = 0
                End If
                sender.Invalidate()
                e.Handled = True
            End If
        ElseIf Key_Settings.getSettings().keyToolAddInstanceMirrorX.isDown(e.KeyCode) Then
            Dim t As TemplateAusDatei = getTemplate(sender)
            If t IsNot Nothing Then
                drehung.SpielgenX()
                onDrehungChanged()
                sender.Invalidate()
                e.Handled = True
            End If
        ElseIf Key_Settings.getSettings().keyToolAddInstanceMirrorY.isDown(e.KeyCode) Then
            Dim t As TemplateAusDatei = getTemplate(sender)
            If t IsNot Nothing Then
                drehung.SpielgenY()
                onDrehungChanged()
                sender.Invalidate()
                e.Handled = True
            End If
        End If
    End Sub

    Private Function getTemplate(sender As Vektor_Picturebox) As TemplateAusDatei
        Dim t As TemplateAusDatei = sender.getCurrentTemplate()
        If t IsNot Nothing Then
            Dim tmp As String = t.getNameSpace() & "." & t.getName()
            If tmp <> Me.lastTemplate Then
                'Bauteil geändert!
                currentName = sender.getCurrentTemplateBauteilName()
                myTextPos = 0
                Me.lastTemplate = tmp
                Me.lastTemplateCompiled = Nothing
                lastGravityPointsTemplateX = Nothing
                lastGravityPointsTemplateY = Nothing
            End If
        End If
        Return t
    End Function

    Public Overrides Sub OnSelectedBauteilTemplateChanged(sender As Vektor_Picturebox, e As EventArgs)
        sender.Invalidate()
    End Sub

    Public Overrides Function getDefaultHilfeText() As String
        Dim k As Key_Settings = Key_Settings.getSettings()
        Return My.Resources.Strings.Tool_Place & " " &
               k.keyToolAddInstanceMirrorX.getStatusStripString() & " " & My.Resources.Strings.Tools_zumHSpiegeln & " " &
               k.keyToolAddInstanceMirrorY.getStatusStripString() & " " & My.Resources.Strings.Tools_zumVSpiegeln & " " &
               "'" & My.Resources.Strings.Mittlere_Maustaste & "' " & My.Resources.Strings.Tools_ZumDrehen & " " &
               k.keySchalteBeschriftungsPosDurch.getStatusStripString() & My.Resources.Strings.Tools_ZumDurchschaltenDerBeschriftung
    End Function
End Class
