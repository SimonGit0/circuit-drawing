Public Class Pfeil_Verwaltung

    Private Function getPfeil(nr As Integer, size As Integer) As Pfeilspitze
        Select Case nr
            Case 0
                Return New Pfeil_Basic(size / 100.0F)
            Case 1
                Return New Pfeil3(size / 100.0F)
            Case 2
                Return New Pfeil_Line(False, size / 100.0F)
            Case 3
                Return New Pfeil_Line(True, size / 100.0F)
        End Select
        Throw New Exception("Pfeil nicht definiert!")
    End Function

    Public Function AnzahlPfeile() As Integer
        Return 4
    End Function

    Public Function getPfeil(index As ParamArrow) As Pfeilspitze
        Return getPfeil(index.pfeilArt, index.pfeilSize)
    End Function

    Private Shared myPfeil_Verwaltung As Pfeil_Verwaltung
    Public Shared Function getVerwaltung() As Pfeil_Verwaltung
        If myPfeil_Verwaltung Is Nothing Then
            myPfeil_Verwaltung = New Pfeil_Verwaltung()
        End If
        Return myPfeil_Verwaltung
    End Function

    Public Function getLineWithPfeil(start As Point, ende As Point, pfeilStart As ParamArrow, pfeilEnde As ParamArrow) As DO_Grafik
        Dim hatEnde As Boolean = pfeilEnde.pfeilArt > -1 AndAlso pfeilEnde.pfeilArt < AnzahlPfeile()
        Dim hatStart As Boolean = pfeilStart.pfeilArt > -1 AndAlso pfeilStart.pfeilArt < AnzahlPfeile()
        If Not hatStart AndAlso Not hatEnde Then
            'Normale Linie
            Return New DO_Linie(start, ende, False)
        Else
            Dim erg As New DO_MultiGrafik()
            Dim offsetEnde As New Point(0, 0)
            Dim offsetStart As New Point(0, 0)
            Dim verkürzung As Integer = 0
            If hatStart Then
                Dim pStart As Pfeilspitze = getPfeil(pfeilStart)
                erg.childs.Add(pStart.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, start, New Point(start.X - ende.X, start.Y - ende.Y)))
                offsetStart = getVektorWithLaenge(New Point(ende.X - start.X, ende.Y - start.Y), pStart.getLineVerkürzung())
                verkürzung += pStart.getLineVerkürzung()
            End If
            If hatEnde Then
                Dim pEnde As Pfeilspitze = getPfeil(pfeilEnde)
                erg.childs.Add(pEnde.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, ende, New Point(ende.X - start.X, ende.Y - start.Y)))
                offsetEnde = getVektorWithLaenge(New Point(start.X - ende.X, start.Y - ende.Y), pEnde.getLineVerkürzung())
                verkürzung += pEnde.getLineVerkürzung()
            End If
            If getLaenge(start, ende) >= verkürzung Then
                start.X += offsetStart.X
                start.Y += offsetStart.Y
                ende.X += offsetEnde.X
                ende.Y += offsetEnde.Y
                erg.childs.Add(New DO_Linie(start, ende, False))
            End If

            Return erg
        End If
    End Function

    Public Function getBezierWithPfeil(p1 As Point, p2 As Point, p3 As Point, p4 As Point, pfeilStart As ParamArrow, pfeilEnde As ParamArrow) As DO_Grafik
        Dim hatEnde As Boolean = pfeilEnde.pfeilArt > -1 AndAlso pfeilEnde.pfeilArt < AnzahlPfeile()
        Dim hatStart As Boolean = pfeilStart.pfeilArt > -1 AndAlso pfeilStart.pfeilArt < AnzahlPfeile()
        If Not hatStart AndAlso Not hatEnde Then
            Return New DO_Bezier({p1, p2, p3, p4}, False, Drawing_FillMode.OnlyStroke)
        Else
            Dim erg As New DO_MultiGrafik()
            If hatStart Then
                Dim pStart As Pfeilspitze = getPfeil(pfeilStart)

                Dim richtungPfeil As Point = New Point(p1.X - p2.X, p1.Y - p2.Y)
                If richtungPfeil.X = 0 AndAlso richtungPfeil.Y = 0 Then
                    richtungPfeil = New Point(2 * p2.X - p3.X - p1.X, 2 * p2.Y - p3.Y - p1.Y)
                End If

                erg.childs.Add(pStart.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, p1, richtungPfeil))

                Dim verkürzung As Integer = pStart.getLineVerkürzung()
                If verkürzung > 0 Then
                    Mathe.VerkürzeBezierStart(p1, p2, p3, p4, verkürzung)
                End If

            End If
            If hatEnde Then
                Dim pEnde As Pfeilspitze = getPfeil(pfeilEnde)

                Dim richtungPfeil As Point = New Point(p4.X - p3.X, p4.Y - p3.Y)
                If richtungPfeil.X = 0 AndAlso richtungPfeil.Y = 0 Then
                    richtungPfeil = New Point(2 * p3.X - p2.X - p4.X, 2 * p3.Y - p2.Y - p4.Y)
                End If

                erg.childs.Add(pEnde.getGrafik(Pfeilspitze.AlignPfeil.Align_An_Spitze, p4, richtungPfeil))

                Dim verkürzung As Integer = pEnde.getLineVerkürzung()
                If verkürzung > 0 Then
                    Mathe.VerkürzeBezierEnde(p1, p2, p3, p4, verkürzung)
                End If

            End If
            erg.childs.Add(New DO_Bezier({p1, p2, p3, p4}, False, Drawing_FillMode.OnlyStroke))

            Return erg
        End If
    End Function

    Public Function getPfeilOnLine(pos As WireSnappoint, pfeil As ParamArrow) As DO_Grafik
        Dim vector As Point = pos.getVector()
        Dim posMitte As New Point(CInt(pos.wireStart.X + pos.alpha * vector.X), CInt(pos.wireStart.Y + pos.alpha * vector.Y))
        Return getPfeil(pfeil).getGrafik(Pfeilspitze.AlignPfeil.Align_An_Mitte, posMitte, vector)
    End Function

    Private Function getVektorWithLaenge(richtung As Point, Laenge As Integer) As Point
        If richtung.X = 0 AndAlso richtung.Y = 0 Then
            Return New Point(0, 0)
        End If

        If Laenge = 0 Then Return New Point(0, 0)
        Dim l As Double = Math.Sqrt(CLng(richtung.X) * CLng(richtung.X) + CLng(richtung.Y) * CLng(richtung.Y))
        Return New Point(CInt(richtung.X / l * Laenge), CInt(richtung.Y / l * Laenge))
    End Function

    Private Function getLaenge(start As Point, ende As Point) As Integer
        Dim v As New Point(start.X - ende.X, start.Y - ende.Y)
        Return CInt(Math.Sqrt(CLng(v.X) * CLng(v.X) + CLng(v.Y) * CLng(v.Y)))
    End Function

End Class
