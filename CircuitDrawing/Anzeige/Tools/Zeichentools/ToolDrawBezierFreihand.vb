Public Class ToolDrawBezierFreihand
    Inherits Tool

    Private vorschauColor As Color = Color.Orange
    Private penVorschau As Pen
    Private mode As Modus

    Private punkteListe As List(Of Point)
    Private längeDrawing As Double
    Private darfSchließen As Boolean
    Private schließenRadius As Integer = 7
    Private geschlossen As Boolean

    Public Sub New()
        penVorschau = New Pen(vorschauColor)
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

    Public Overrides Sub MouseDown(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.IDLE Then
                If punkteListe Is Nothing Then
                    punkteListe = New List(Of Point)
                Else
                    punkteListe.Clear()
                End If
                längeDrawing = 0
                darfSchließen = False
                geschlossen = False
                punkteListe.Add(sender.GetCursorPosOffgrid())
                mode = Modus.Zeichnen
            End If
        End If
    End Sub

    Public Overrides Sub MouseMoveOffgrid(sender As Vektor_Picturebox, e As ToolMouseMoveOffgridEventArgs)
        If mode = Modus.Zeichnen Then
            punkteListe.Add(sender.GetCursorPosOffgrid())
            If punkteListe.Count >= 2 Then
                längeDrawing += Math.Sqrt(Mathe.abstandQuadrat(punkteListe(punkteListe.Count - 1), punkteListe(punkteListe.Count - 2)))
            End If
            If Not darfSchließen Then
                darfSchließen = sender.toPictureboxScale(längeDrawing) > 50
            End If
            geschlossen = False
            If darfSchließen Then
                Dim abstandEnde As Double = Math.Sqrt(Mathe.abstandQuadrat(punkteListe(0), punkteListe(punkteListe.Count - 1)))
                If sender.toPictureboxScale(abstandEnde) < schließenRadius Then
                    geschlossen = True
                End If
            End If
            sender.Invalidate()
        End If
    End Sub

    Public Overrides Sub MouseUp(sender As Vektor_Picturebox, e As ToolMouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If mode = Modus.Zeichnen Then
                If punkteListe.Count >= 2 Then
                    Dim punkteAll() As PointD = erweitern(punkteListe)
                    Dim bezier As Point()
                    Try
                        bezier = berechneBezierAlgo2(punkteAll, Settings.getSettings().BezierKurveAbweichung, geschlossen)
                        If bezier Is Nothing Then
                            sender.Invalidate()
                            punkteListe.Clear()
                            mode = Modus.IDLE
                            Exit Sub
                        End If
                    Catch
                        MessageBox.Show("Fehler beim Berechnen der Bezierkurve! Bitte erneut versuchen.", "Fehler")
                        sender.Invalidate()
                        punkteListe.Clear()
                        mode = Modus.IDLE
                        Exit Sub
                    End Try

                    Dim rück As New RückgängigGrafik()
                    rück.setText("Freihand Bezier-Kurve hinzugefügt")
                    rück.speicherVorherZustand(sender.getRückArgs())

                    For i As Integer = 0 To bezier.Length - 4 Step 3
                        Dim w As New Basic_Bezier(sender.getNewID(), 2, bezier(i), bezier(i + 1), bezier(i + 2), bezier(i + 3), New ParamArrow(-1, 100), New ParamArrow(-1, 100))
                        sender.addElement(w)
                    Next

                    rück.speicherNachherZustand(sender.getRückArgs())
                    sender.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
                End If
                punkteListe.Clear()
                mode = Modus.IDLE
                sender.Invalidate()
            End If
        End If
    End Sub

    Public Overrides Sub OnDraw(sender As Vektor_Picturebox, e As ToolPaintEventArgs)
        If mode = Modus.Zeichnen Then
            If punkteListe.Count >= 2 Then
                Dim pF(punkteListe.Count - 1) As PointF
                For i As Integer = 0 To punkteListe.Count - 1
                    pF(i) = sender.toPictureboxPoint(punkteListe(i))
                Next
                e.Graphics.DrawLines(penVorschau, pF)
                If darfSchließen Then
                    If geschlossen Then
                        e.Graphics.DrawEllipse(penVorschau, pF(0).X - schließenRadius, pF(0).Y - schließenRadius, 2 * schließenRadius + 1, 2 * schließenRadius + 1)
                    End If
                End If
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
        Return My.Resources.Strings.Tool_AddBezierFreihand
    End Function

    Private Function erweitern(ByVal Punkteliste As List(Of Point)) As PointD()
        If Punkteliste.Count < 2 OrElse Punkteliste(0) = Punkteliste(1) Then
            Return Nothing
        End If
        Dim liste As New List(Of PointD)
        Dim offset As Double = 0
        For i As Integer = 0 To Punkteliste.Count - 2
            Dim richtung As New PointD(Punkteliste(i + 1).X - Punkteliste(i).X, Punkteliste(i + 1).Y - Punkteliste(i).Y)
            Dim aktuellerPunkt As New PointD(Punkteliste(i).X, Punkteliste(i).Y)
            Dim gesamtLänge As Double = Math.Sqrt(CDbl(Punkteliste(i).X - Punkteliste(i + 1).X) * (Punkteliste(i).X - Punkteliste(i + 1).X) + CDbl(Punkteliste(i).Y - Punkteliste(i + 1).Y) * (Punkteliste(i).Y - Punkteliste(i + 1).Y))
            Dim faktor As Double
            Dim cumFaktor As Double = -offset / gesamtLänge

            Dim punktAlt As PointD = New PointD(Punkteliste(i))
            Dim punktNeu As PointD
            If gesamtLänge > 0 Then
                Do
                    faktor = 2.0 / gesamtLänge
                    cumFaktor += faktor
                    If cumFaktor > 1 Then Exit Do
                    aktuellerPunkt.X = richtung.X * cumFaktor + Punkteliste(i).X
                    aktuellerPunkt.Y = richtung.Y * cumFaktor + Punkteliste(i).Y
                    punktNeu = New PointD(aktuellerPunkt.X, aktuellerPunkt.Y)
                    liste.Add(punktNeu)
                    punktAlt = punktNeu
                Loop
                offset = (1 - cumFaktor + faktor) * gesamtLänge
            End If
        Next
        Return liste.ToArray
    End Function

#Region "Algorithmus 2 (Rund)"
    Private Function berechneBezierAlgo2(punkte() As PointD, genauigkeit As Double, geschlossen As Boolean) As Point()
        'Debug.Print(punkte.Length.ToString())
        Dim bezier() As Point = Nothing
        Dim teilpunkte() As List(Of PointD) = Nothing
        Dim t()() As Double = Nothing
        Dim rest As Double
        Dim bez(3) As Point
        Dim anzahl As Integer = 1
        Dim stpw As New Stopwatch()
        stpw.Start()
        Dim timeout As Integer = 10000
        Do
            bezier = berechneBezierBasisAlgo2(punkte, anzahl, teilpunkte, t, geschlossen)
            rest = 0
            For i As Integer = 0 To anzahl - 1
                bez(0) = bezier(i * 3)
                bez(1) = bezier(i * 3 + 1)
                bez(2) = bezier(i * 3 + 2)
                bez(3) = bezier(i * 3 + 3)
                rest += berechneResiduum(teilpunkte(i), t(i), bez)
            Next
            rest = Math.Sqrt(rest / anzahl)
            If rest < genauigkeit Then Exit Do
            anzahl += 1
            If stpw.ElapsedMilliseconds > timeout Then
                stpw.Stop()
                If MessageBox.Show(My.Resources.Strings.BezierLangeZeit, My.Resources.Strings.BerechnungAbbrechen, MessageBoxButtons.YesNo) = DialogResult.Yes Then
                    stpw.Stop()
                    Return Nothing
                Else
                    timeout += 10000
                    stpw.Start()
                End If
            End If
        Loop
        stpw.Stop()
        Return bezier
    End Function

    Private Function berechneBezierBasisAlgo2(punkte() As PointD, anzahl As Integer, ByRef _teilpunkte() As List(Of PointD), ByRef _t()() As Double, geschlossen As Boolean) As Point()
        Dim länge As Double = 0
        For i As Integer = 1 To punkte.Length - 1
            länge += Math.Sqrt((punkte(i).X - punkte(i - 1).X) * (punkte(i).X - punkte(i - 1).X) +
                               (punkte(i).Y - punkte(i - 1).Y) * (punkte(i).Y - punkte(i - 1).Y))
        Next
        Dim gesamtlänge As Double = länge
        Dim teillänge As Double = gesamtlänge / anzahl
        Dim Teilpunkte(anzahl - 1) As List(Of PointD)
        Teilpunkte(0) = New List(Of PointD)
        Dim zähler As Integer = 0
        länge = 0
        For i As Integer = 0 To punkte.Length - 1
            If i > 0 Then
                länge += Math.Sqrt((punkte(i).X - punkte(i - 1).X) * (punkte(i).X - punkte(i - 1).X) +
                                   (punkte(i).Y - punkte(i - 1).Y) * (punkte(i).Y - punkte(i - 1).Y))
            End If
            If länge <= teillänge OrElse zähler = Teilpunkte.Length - 1 Then
                Teilpunkte(zähler).Add(punkte(i))
            Else
                zähler += 1
                länge -= teillänge
                Teilpunkte(zähler) = New List(Of PointD)
                Teilpunkte(zähler).Add(punkte(i))
            End If
        Next

        Dim t(anzahl - 1)() As Double
        Dim matrix_X(anzahl - 1)(,) As Double
        Dim matrix_Y(anzahl - 1)(,) As Double

        For i As Integer = 0 To anzahl - 1
            t(i) = generateT(Teilpunkte(i).ToArray)
            matrix_X(i) = getMatrix1(t(i), Teilpunkte(i).ToArray, True)
            matrix_Y(i) = getMatrix1(t(i), Teilpunkte(i).ToArray, False)
            If geschlossen Then
                If i > 0 And i < anzahl - 1 Then
                    matrix_X(i) = MatrixTransformieren(matrix_X(i))
                    matrix_Y(i) = MatrixTransformieren(matrix_Y(i))
                ElseIf i = anzahl - 1 Then
                    matrix_X(i) = MatrixTransformieren2(matrix_X(i))
                    matrix_Y(i) = MatrixTransformieren2(matrix_Y(i))
                End If
            Else
                If i > 0 Then
                    matrix_X(i) = MatrixTransformieren(matrix_X(i))
                    matrix_Y(i) = MatrixTransformieren(matrix_Y(i))
                End If
            End If

        Next

        _teilpunkte = Teilpunkte
        _t = t

        Dim matrix_X_ges(,) As Double
        Dim matrix_Y_ges(,) As Double
        Dim modulo As Integer
        If geschlossen And anzahl > 1 Then
            'geschlossen
            ReDim matrix_X_ges(anzahl * 2, anzahl * 2 - 1)
            ReDim matrix_Y_ges(anzahl * 2, anzahl * 2 - 1)
            modulo = anzahl * 2
        Else
            ReDim matrix_X_ges(anzahl * 2 + 2, anzahl * 2 + 1)
            ReDim matrix_Y_ges(anzahl * 2 + 2, anzahl * 2 + 1)
            modulo = anzahl * 2 + 2
        End If

        For k As Integer = 0 To anzahl - 1
            For i As Integer = 0 To 3
                For j As Integer = 0 To 3
                    matrix_X_ges((i + 2 * k) Mod modulo, (j + 2 * k) Mod modulo) += matrix_X(k)(i, j)
                    matrix_Y_ges((i + 2 * k) Mod modulo, (j + 2 * k) Mod modulo) += matrix_Y(k)(i, j)
                Next
            Next
        Next

        For k As Integer = 0 To anzahl - 1
            For i As Integer = 0 To 3
                matrix_X_ges(modulo, (2 * k + i) Mod modulo) += matrix_X(k)(4, i)
                matrix_Y_ges(modulo, (2 * k + i) Mod modulo) += matrix_Y(k)(4, i)
            Next
        Next

        Dim m_x As New Matrix(matrix_X_ges)
        Dim m_y As New Matrix(matrix_Y_ges)

        Dim lösungenX() As Double = m_x.Auflösen()
        Dim lösungenY() As Double = m_y.Auflösen()

        Dim erg(3 * anzahl) As Point
        erg(0) = New Point(CInt(lösungenX(0)), CInt(lösungenY(0)))
        erg(1) = New Point(CInt(lösungenX(1)), CInt(lösungenY(1)))
        erg(2) = New Point(CInt(lösungenX(2)), CInt(lösungenY(2)))
        erg(3) = New Point(CInt(lösungenX(3)), CInt(lösungenY(3)))
        zähler = 4
        Dim nr As Integer = 3
        For i As Integer = 1 To anzahl - 1
            erg(zähler) = New Point(2 * erg(zähler - 1).X - erg(zähler - 2).X, 2 * erg(zähler - 1).Y - erg(zähler - 2).Y)
            If geschlossen And i = anzahl - 1 Then
                erg(zähler + 1) = New Point(2 * erg(0).X - erg(1).X, 2 * erg(0).Y - erg(1).Y)
                erg(zähler + 2) = New Point(erg(0).X, erg(0).Y)
            Else
                erg(zähler + 1) = New Point(CInt(lösungenX(nr + 1)), CInt(lösungenY(nr + 1)))
                erg(zähler + 2) = New Point(CInt(lösungenX(nr + 2)), CInt(lösungenY(nr + 2)))
            End If

            nr += 2
            zähler += 3
        Next
        Return erg
    End Function

    Private Function getMatrix1(t() As Double, punkte() As PointD, x As Boolean) As Double(,)
        Dim matrix(4, 3) As Double
        For i As Integer = 0 To t.Length - 1
            matrix(0, 0) += (1 - t(i)) ^ 6
            matrix(1, 0) += 3 * t(i) * (1 - t(i)) ^ 5
            matrix(2, 0) += 3 * t(i) ^ 2 * (1 - t(i)) ^ 4
            matrix(3, 0) += t(i) ^ 3 * (1 - t(i)) ^ 3

            matrix(0, 1) += t(i) * (1 - t(i)) ^ 5
            matrix(1, 1) += 3 * t(i) ^ 2 * (1 - t(i)) ^ 4
            matrix(2, 1) += 3 * t(i) ^ 3 * (1 - t(i)) ^ 3
            matrix(3, 1) += t(i) ^ 4 * (1 - t(i)) ^ 2

            matrix(0, 2) += t(i) ^ 2 * (1 - t(i)) ^ 4
            matrix(1, 2) += 3 * t(i) ^ 3 * (1 - t(i)) ^ 3
            matrix(2, 2) += 3 * t(i) ^ 4 * (1 - t(i)) ^ 2
            matrix(3, 2) += t(i) ^ 5 * (1 - t(i))

            matrix(0, 3) += t(i) ^ 3 * (1 - t(i)) ^ 3
            matrix(1, 3) += 3 * t(i) ^ 4 * (1 - t(i)) ^ 2
            matrix(2, 3) += 3 * t(i) ^ 5 * (1 - t(i))
            matrix(3, 3) += t(i) ^ 6

            If x Then
                matrix(4, 0) += punkte(i).X * (1 - t(i)) ^ 3
                matrix(4, 1) += punkte(i).X * t(i) * (1 - t(i)) ^ 2
                matrix(4, 2) += punkte(i).X * t(i) ^ 2 * (1 - t(i))
                matrix(4, 3) += punkte(i).X * t(i) ^ 3
            Else
                matrix(4, 0) += punkte(i).Y * (1 - t(i)) ^ 3
                matrix(4, 1) += punkte(i).Y * t(i) * (1 - t(i)) ^ 2
                matrix(4, 2) += punkte(i).Y * t(i) ^ 2 * (1 - t(i))
                matrix(4, 3) += punkte(i).Y * t(i) ^ 3
            End If
        Next
        Return matrix
    End Function

    Private Function MatrixTransformieren(m1(,) As Double) As Double(,)
        Dim erg(,) As Double = CType(m1.Clone, Double(,))

        erg(0, 0) = -m1(1, 0)
        erg(1, 0) = m1(0, 0) + 2 * m1(1, 0)

        erg(0, 1) = -m1(1, 1)
        erg(1, 1) = m1(0, 1) + 2 * m1(1, 1)

        erg(0, 2) = -m1(1, 2)
        erg(1, 2) = m1(0, 2) + 2 * m1(1, 2)

        erg(0, 3) = -m1(1, 3)
        erg(1, 3) = m1(0, 3) + 2 * m1(1, 3)

        Return erg
    End Function

    Private Function MatrixTransformieren2(m1(,) As Double) As Double(,)
        Dim erg(,) As Double = CType(m1.Clone, Double(,))
        'linke Seite (wie bei MatrixTransformieren())
        erg(0, 0) = -m1(1, 0)
        erg(1, 0) = m1(0, 0) + 2 * m1(1, 0)

        erg(0, 1) = -m1(1, 1)
        erg(1, 1) = m1(0, 1) + 2 * m1(1, 1)

        erg(0, 2) = -m1(1, 2)
        erg(1, 2) = m1(0, 2) + 2 * m1(1, 2)

        erg(0, 3) = -m1(1, 3)
        erg(1, 3) = m1(0, 3) + 2 * m1(1, 3)


        'rechte Seite
        erg(2, 0) = 2 * m1(2, 0) + m1(3, 0)
        erg(3, 0) = -m1(2, 0)

        erg(2, 1) = 2 * m1(2, 1) + m1(3, 1)
        erg(3, 1) = -m1(2, 1)

        erg(2, 2) = 2 * m1(2, 2) + m1(3, 2)
        erg(3, 2) = -m1(2, 2)

        erg(2, 3) = 2 * m1(2, 3) + m1(3, 3)
        erg(3, 3) = -m1(2, 3)
        Return erg
    End Function


    Private Function generateT(punkte() As PointD) As Double()
        Dim t(punkte.Length - 1) As Double
        Dim länge As Double
        For i As Integer = 1 To punkte.Length - 1
            t(i) = Math.Sqrt((punkte(i).X - punkte(i - 1).X) * (punkte(i).X - punkte(i - 1).X) +
                             (punkte(i).Y - punkte(i - 1).Y) * (punkte(i).Y - punkte(i - 1).Y))

            länge += t(i)
            If i > 0 Then
                t(i) += t(i - 1)
            End If
        Next
        For i As Integer = 0 To punkte.Length - 1
            t(i) /= länge
        Next
        Return t
    End Function

    Private Function berechneResiduum(punkte As List(Of PointD), t() As Double, bezier() As Point) As Double
        Dim a1 As Double = -bezier(0).X + 3 * bezier(1).X - 3 * bezier(2).X + bezier(3).X
        Dim a2 As Double = 3 * bezier(0).X - 6 * bezier(1).X + 3 * bezier(2).X
        Dim a3 As Double = -3 * bezier(0).X + 3 * bezier(1).X
        Dim a4 As Double = bezier(0).X

        Dim b1 As Double = -bezier(0).Y + 3 * bezier(1).Y - 3 * bezier(2).Y + bezier(3).Y
        Dim b2 As Double = 3 * bezier(0).Y - 6 * bezier(1).Y + 3 * bezier(2).Y
        Dim b3 As Double = -3 * bezier(0).Y + 3 * bezier(1).Y
        Dim b4 As Double = bezier(0).Y

        Dim summe As Double = 0
        For i As Integer = 0 To t.Length - 1
            summe += Math.Abs(punkte(i).X - (a1 * t(i) * t(i) * t(i) + a2 * t(i) * t(i) + a3 * t(i) + a4)) ^ 2
            summe += Math.Abs(punkte(i).Y - (b1 * t(i) * t(i) * t(i) + b2 * t(i) * t(i) + b3 * t(i) + b4)) ^ 2
        Next
        Return summe / t.Length
    End Function
#End Region

    Private Enum Modus
        IDLE
        Zeichnen
    End Enum
End Class
