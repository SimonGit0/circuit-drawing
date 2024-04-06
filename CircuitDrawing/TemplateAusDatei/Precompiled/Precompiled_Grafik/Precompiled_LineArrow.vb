Imports System.IO
Public Class Precompiled_LineArrow
    Inherits Precompiled_Grafik

    Private x1, y1, x2, y2 As Ausdruck_Int
    Private start, ende As Ausdruck

    Private step_Over_OtherLines As Ausdruck_Int

    Public Sub New(x1 As Ausdruck_Int, y1 As Ausdruck_Int, x2 As Ausdruck_Int, y2 As Ausdruck_Int, start As Ausdruck, ende As Ausdruck)
        Me.New(x1, y1, x2, y2, start, ende, New Ausdruck_Konstante(0))
    End Sub

    Public Sub New(x1 As Ausdruck_Int, y1 As Ausdruck_Int, x2 As Ausdruck_Int, y2 As Ausdruck_Int, start As Ausdruck, ende As Ausdruck, step_over_lines As Ausdruck_Int)
        Me.x1 = x1
        Me.y1 = y1
        Me.x2 = x2
        Me.y2 = y2
        If TypeOf start Is Ausdruck_Pfeil OrElse TypeOf start Is Ausdruck_Int Then
            Me.start = start
        Else
            Throw New Exception("Ungültiger Parameter! Es wird eine Pfeilspitze (oder ein Int-Wert) erwartet.")
        End If
        If TypeOf ende Is Ausdruck_Pfeil OrElse TypeOf ende Is Ausdruck_Int Then
            Me.ende = ende
        Else
            Throw New Exception("Ungültiger Parameter! Es wird eine Pfeilspitze (oder ein Int-Wert) erwartet.")
        End If
        If step_over_lines IsNot Nothing Then
            Me.step_Over_OtherLines = step_over_lines
        End If
    End Sub

    Private _tempP1, _tempP2 As Point
    Private _tempMode As Integer
    Private _tempPs, _tempPe As ParamArrow
    Private _tempGrafikErsetzen As DO_Linie

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim ps As ParamArrow
        If TypeOf start Is Ausdruck_Pfeil Then
            ps = args.ausrechnen(DirectCast(start, Ausdruck_Pfeil))
        ElseIf TypeOf start Is Ausdruck_Int Then
            ps = New ParamArrow(CShort(args.ausrechnen(DirectCast(start, Ausdruck_Int))), 100)
        Else
            Throw New Exception("Falscher Ausdruck. Pfeil erwartet!")
        End If
        Dim pe As ParamArrow
        If TypeOf ende Is Ausdruck_Pfeil Then
            pe = args.ausrechnen(DirectCast(ende, Ausdruck_Pfeil))
        ElseIf TypeOf ende Is Ausdruck_Int Then
            pe = New ParamArrow(CShort(args.ausrechnen(DirectCast(ende, Ausdruck_Int))), 100)
        Else
            Throw New Exception("Falscher Ausdruck. Pfeil erwartet!")
        End If

        Dim p1 As New Point(args.ausrechnen(x1), args.ausrechnen(y1))
        Dim p2 As New Point(args.ausrechnen(x2), args.ausrechnen(y2))
        Dim step_Over_mode As Integer = args.ausrechnen(step_Over_OtherLines)
        If step_Over_mode = 1 OrElse step_Over_mode = 2 Then
            _tempP1 = p1
            _tempP2 = p2
            _tempMode = step_Over_mode
            _tempPs = ps
            _tempPe = pe
            erg.callbackAfterOrigin.Add(New Action(Of CompileArgs, Point)(AddressOf callback_AfterOrigin))
            _tempGrafikErsetzen = New DO_Linie(p1, p2, False)
            erg.addGrafik(_tempGrafikErsetzen)
        Else
            erg.addGrafik(Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, ps, pe))
        End If
    End Sub

    Private Sub callback_AfterOrigin(erg As CompileArgs, origin As Point)
        Dim p1 As Point = New Point(_tempP1.X - origin.X, _tempP1.Y - origin.Y)
        Dim p2 As Point = New Point(_tempP2.X - origin.Y, _tempP2.Y - origin.Y)
        Dim ps As ParamArrow = _tempPs
        Dim pe As ParamArrow = _tempPe

        Dim indexGrafik As Integer = erg.getIndexOfGrafik(_tempGrafikErsetzen)
        erg.removeGrafik(indexGrafik)

        Dim otherLines As List(Of DO_Linie) = erg.parentArgs.list_of_parent_lines
        Dim alphas As New List(Of Double)
        For i As Integer = 0 To otherLines.Count - 1
            Dim alpha As Double = Mathe.getSchnittpunkt_alpha(p1, p2, otherLines(i))
            If alpha >= 0 AndAlso alpha <= 1 Then
                alphas.Add(alpha)
            End If
        Next
        Dim otherEll As List(Of DO_Ellipse) = erg.parentArgs.list_of_parent_ellipse
        For i As Integer = 0 To otherEll.Count - 1
            Dim alpha As List(Of Double) = Mathe.getSchnittpunkt_alpha(p1, p2, otherEll(i))
            If alpha IsNot Nothing Then
                For j As Integer = 0 To alpha.Count - 1
                    If alpha(j) >= 0 AndAlso alpha(j) <= 1 Then
                        alphas.Add(alpha(j))
                    End If
                Next
            End If
        Next
        Dim otherArc As List(Of DO_Arc) = erg.parentArgs.list_of_parent_Arcs
        For i As Integer = 0 To otherArc.Count - 1
            Dim alpha As List(Of Double) = Mathe.getSchnittpunkt_alpha(p1, p2, otherArc(i))
            If alpha IsNot Nothing Then
                For j As Integer = 0 To alpha.Count - 1
                    If alpha(j) >= 0 AndAlso alpha(j) <= 1 Then
                        alphas.Add(alpha(j))
                    End If
                Next
            End If
        Next
        Dim otherBezier As List(Of DO_Bezier) = erg.parentArgs.list_of_parent_bezier
        For i As Integer = 0 To otherBezier.Count - 1
            Dim alpha As List(Of Double) = Mathe.getSchnittpunkt_alpha(p1, p2, otherBezier(i))
            If alpha IsNot Nothing Then
                For j As Integer = 0 To alpha.Count - 1
                    If alpha(j) >= 0 AndAlso alpha(j) <= 1 Then
                        alphas.Add(alpha(j))
                    End If
                Next
            End If
        Next

        p1 = New Point(p1.X + origin.X, p1.Y + origin.Y)
        p2 = New Point(p2.X + origin.X, p2.Y + origin.Y)

        If alphas.Count > 0 Then
            alphas.Sort()

            For i As Integer = alphas.Count - 2 To 0 Step -1
                If alphas(i) = alphas(i + 1) Then
                    alphas.RemoveAt(i + 1)
                End If
            Next

            If alphas.Count >= 2 Then
                If _tempMode = 1 Then
                    Dim pi As New Point(CInt(p1.X + alphas(0) * (p2.X - p1.X)), CInt(p1.Y + alphas(0) * (p2.Y - p1.Y)))
                    erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, pi, ps, New ParamArrow(-1, 100)))
                    indexGrafik += 1
                    pi = New Point(CInt(p1.X + alphas(alphas.Count - 1) * (p2.X - p1.X)), CInt(p1.Y + alphas(alphas.Count - 1) * (p2.Y - p1.Y)))
                    erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(pi, p2, New ParamArrow(-1, 100), pe))
                    indexGrafik += 1
                ElseIf _tempMode = 2 Then
                    'Alter Algorithmus, der immer Abwechselnd die Linien zeichnet.

                    Dim pi As New Point(CInt(p1.X + alphas(0) * (p2.X - p1.X)), CInt(p1.Y + alphas(0) * (p2.Y - p1.Y)))
                    Dim zeichnen As Boolean = False
                    erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, pi, ps, New ParamArrow(-1, 100)))
                    indexGrafik += 1
                    For i As Integer = 1 To alphas.Count - 1
                        Dim piNeu As New Point(CInt(p1.X + alphas(i) * (p2.X - p1.X)), CInt(p1.Y + alphas(i) * (p2.Y - p1.Y)))
                        If zeichnen Then
                            erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(pi, piNeu, New ParamArrow(-1, 100), New ParamArrow(-1, 100)))
                            indexGrafik += 1
                        End If
                        zeichnen = Not zeichnen
                        pi = piNeu
                    Next
                    If zeichnen Then
                        erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(pi, p2, New ParamArrow(-1, 100), pe))
                        indexGrafik += 1
                    End If
                End If
            Else
                erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, ps, pe))
                indexGrafik += 1
            End If
        Else
            erg.insertGrafik(indexGrafik, Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, ps, pe))
            indexGrafik += 1
        End If
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(x1, writer)
        Ausdruck_Int.speichern(y1, writer)
        Ausdruck_Int.speichern(x2, writer)
        Ausdruck_Int.speichern(y2, writer)
        Ausdruck.speichernAllgemein(start, writer)
        Ausdruck.speichernAllgemein(ende, writer)

        Ausdruck_Int.speichern(step_Over_OtherLines, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_LineArrow
        Dim x1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y1 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim x2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y2 As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim start As Ausdruck = Ausdruck.ladenAllgemein(reader, version)
        Dim ende As Ausdruck = Ausdruck.ladenAllgemein(reader, version)

        Dim step_over_lines As Ausdruck_Int
        If version >= 18 Then
            step_over_lines = Ausdruck_Int.laden(reader, version)
        Else
            step_over_lines = New Ausdruck_Konstante(0)
        End If

        Return New Precompiled_LineArrow(x1, y1, x2, y2, start, ende, step_over_lines)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "linearrow("
        line &= Ausdruck_Int.export(x1, writer).str & ", "
        line &= Ausdruck_Int.export(y1, writer).str & ", "
        line &= Ausdruck_Int.export(x2, writer).str & ", "
        line &= Ausdruck_Int.export(y2, writer).str & ", "
        line &= Ausdruck.exportAllgemein(start, writer) & ", "
        line &= Ausdruck.exportAllgemein(ende, writer)

        If TypeOf step_Over_OtherLines Is Ausdruck_Konstante AndAlso step_Over_OtherLines.Ausrechnen(Nothing) = 0 Then
            line &= ")"
        Else
            line &= ", " & Ausdruck_Int.export(step_Over_OtherLines, writer).str & ")"
        End If

        writer.WriteLine(line)
    End Sub
End Class
