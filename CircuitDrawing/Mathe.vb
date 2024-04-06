Imports System.Text
Public Class Mathe
    Public Shared Function Union(rect1 As Rectangle, rect2 As Rectangle) As Rectangle
        If rect1.Width = 0 AndAlso rect1.Height = 0 Then
            Return rect2
        End If
        If rect2.Width = 0 AndAlso rect2.Height = 0 Then
            Return rect1
        End If
        Dim minX As Integer = Math.Min(rect1.X, rect2.X)
        Dim minY As Integer = Math.Min(rect1.Y, rect2.Y)

        Return New Rectangle(minX, minY, Math.Max(rect1.X + rect1.Width, rect2.X + rect2.Width) - minX, Math.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - minY)
    End Function

    Public Shared Function Union(rect1 As RectangleF, rect2 As RectangleF) As RectangleF
        If rect1.Width = 0 AndAlso rect1.Height = 0 Then
            Return rect2
        End If
        If rect2.Width = 0 AndAlso rect2.Height = 0 Then
            Return rect1
        End If
        Dim minX As Single = Math.Min(rect1.X, rect2.X)
        Dim minY As Single = Math.Min(rect1.Y, rect2.Y)

        Return New RectangleF(minX, minY, Math.Max(rect1.X + rect1.Width, rect2.X + rect2.Width) - minX, Math.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - minY)
    End Function

    Public Shared Function move(rect As Rectangle, delta As Point) As Rectangle
        Return New Rectangle(rect.X + delta.X, rect.Y + delta.Y, rect.Width, rect.Height)
    End Function

    Public Shared Function moveInverted(rect As Rectangle, delta As Point) As Rectangle
        Return New Rectangle(rect.X - delta.X, rect.Y - delta.Y, rect.Width, rect.Height)
    End Function

    Public Shared Function rectIneinander(r1 As Rectangle, r2 As Rectangle) As Boolean
        If isInRect(New Point(r1.X, r1.Y), r2) Then Return True
        If isInRect(New Point(r1.X + r1.Width, r1.Y), r2) Then Return True
        If isInRect(New Point(r1.X + r1.Width, r1.Y + r1.Height), r2) Then Return True
        If isInRect(New Point(r1.X, r1.Y + r1.Height), r2) Then Return True

        If isInRect(New Point(r2.X, r2.Y), r1) Then Return True
        If isInRect(New Point(r2.X + r2.Width, r2.Y), r1) Then Return True
        If isInRect(New Point(r2.X + r2.Width, r2.Y + r2.Height), r1) Then Return True
        If isInRect(New Point(r2.X, r2.Y + r2.Height), r1) Then Return True

        Return False
    End Function

    Public Shared Function rectFIneinander(r1 As RectangleF, r2 As RectangleF) As Boolean
        If isInRectF(New PointF(r1.X, r1.Y), r2) Then Return True
        If isInRectF(New PointF(r1.X + r1.Width, r1.Y), r2) Then Return True
        If isInRectF(New PointF(r1.X + r1.Width, r1.Y + r1.Height), r2) Then Return True
        If isInRectF(New PointF(r1.X, r1.Y + r1.Height), r2) Then Return True

        If isInRectF(New PointF(r2.X, r2.Y), r1) Then Return True
        If isInRectF(New PointF(r2.X + r2.Width, r2.Y), r1) Then Return True
        If isInRectF(New PointF(r2.X + r2.Width, r2.Y + r2.Height), r1) Then Return True
        If isInRectF(New PointF(r2.X, r2.Y + r2.Height), r1) Then Return True

        Return False
    End Function

    Public Shared Function getCenter(r As RectangleF) As PointF
        Return New PointF(r.X + r.Width * 0.5F, r.Y + r.Height * 0.5F)
    End Function

    Public Shared Function MirrorY_Rect(r As RectangleF) As RectangleF
        Dim yMin As Single = r.Y
        Dim yMax As Single = r.Y + r.Height
        yMax = -yMax
        yMin = -yMin
        Return New RectangleF(r.X, Math.Min(yMin, yMax), r.Width, Math.Max(yMin, yMax) - Math.Min(yMin, yMax))
    End Function

    Public Shared Function getRect(p1 As Point, p2 As Point) As Rectangle
        Dim minX As Integer = Math.Min(p1.X, p2.X)
        Dim minY As Integer = Math.Min(p1.Y, p2.Y)
        Dim maxX As Integer = Math.Max(p1.X, p2.X)
        Dim maxY As Integer = Math.Max(p1.Y, p2.Y)

        Return New Rectangle(minX, minY, maxX - minX, maxY - minY)

    End Function

    Public Shared Function abstandQuadrat(p1 As Point, p2 As Point) As Long
        Return CLng(p1.X - p2.X) * CLng(p1.X - p2.X) + CLng(p1.Y - p2.Y) * CLng(p1.Y - p2.Y)
    End Function

    Public Shared Function removeEmptyPoints(p() As Point) As Point()
        Dim löschen As Integer = 0
        For i As Integer = p.Length - 1 To 0 Step -1
            If p(i).X = 0 AndAlso p(i).Y = 0 Then
                For j = i To p.Length - 2
                    p(j) = p(j + 1)
                Next
                löschen += 1
            End If
        Next
        ReDim Preserve p(p.Length - 1 - löschen)
        Return p
    End Function

    Public Shared Function isInRect(p As Point, r As Rectangle) As Boolean
        Return p.X >= r.X AndAlso p.Y >= r.Y AndAlso p.X <= r.X + r.Width AndAlso p.Y <= r.Y + r.Height
    End Function

    Public Shared Function isInRectF(p As PointF, r As RectangleF) As Boolean
        Return p.X >= r.X AndAlso p.Y >= r.Y AndAlso p.X <= r.X + r.Width AndAlso p.Y <= r.Y + r.Height
    End Function

    Public Shared Function calcDist(k As Kante, p As Point) As Double
        Return distLiniePoint(k.start, k.ende, p)
    End Function

    Public Shared Function distLiniePoint(linieStart As Point, linieEnde As Point, p As Point) As Double
        Dim v As New Point(linieEnde.X - linieStart.X, linieEnde.Y - linieStart.Y)
        Dim normal As New Point(-v.Y, v.X)
        Dim d As New Point(linieStart.X - p.X, linieStart.Y - p.Y)

        Dim nenner As Long = -CLng(normal.X) * v.Y + CLng(normal.Y) * v.X
        Dim zähler As Long = CLng(normal.X) * d.Y - CLng(normal.Y) * d.X


        Dim alpha As Double = zähler / nenner
        If alpha >= 0 AndAlso alpha <= 1 Then
            Dim schnittpunkt As New PointF(CSng(linieStart.X + alpha * v.X), CSng(linieStart.Y + alpha * v.Y))

            Return Math.Sqrt((schnittpunkt.X - p.X) * (schnittpunkt.X - p.X) + (schnittpunkt.Y - p.Y) * (schnittpunkt.Y - p.Y))
        Else
            Dim d1 As Double = Math.Sqrt(CLng(linieStart.X - p.X) * (linieStart.X - p.X) + CLng(linieStart.Y - p.Y) * (linieStart.Y - p.Y))
            Dim d2 As Double = Math.Sqrt(CLng(linieEnde.X - p.X) * (linieEnde.X - p.X) + CLng(linieEnde.Y - p.Y) * (linieEnde.Y - p.Y))
            Return Math.Min(d1, d2)
        End If
    End Function

    Public Shared Function distLiniePoint(linieStart As Point, linieEnde As Point, p As PointF) As Double
        Dim v As New Point(linieEnde.X - linieStart.X, linieEnde.Y - linieStart.Y)
        Dim normal As New Point(-v.Y, v.X)
        Dim d As New PointF(linieStart.X - p.X, linieStart.Y - p.Y)

        Dim zähler As Single = normal.X * d.Y - normal.Y * d.X
        Dim nenner As Integer = -normal.X * v.Y + normal.Y * v.X


        Dim alpha As Double = zähler / nenner
        If alpha >= 0 AndAlso alpha <= 1 Then
            Dim schnittpunkt As New PointF(CSng(linieStart.X + alpha * v.X), CSng(linieStart.Y + alpha * v.Y))

            Return Math.Sqrt((schnittpunkt.X - p.X) * (schnittpunkt.X - p.X) + (schnittpunkt.Y - p.Y) * (schnittpunkt.Y - p.Y))
        Else
            Dim d1 As Double = Math.Sqrt((linieStart.X - p.X) * (linieStart.X - p.X) + (linieStart.Y - p.Y) * (linieStart.Y - p.Y))
            Dim d2 As Double = Math.Sqrt((linieEnde.X - p.X) * (linieEnde.X - p.X) + (linieEnde.Y - p.Y) * (linieEnde.Y - p.Y))
            Return Math.Min(d1, d2)
        End If
    End Function

    Public Shared Function pointFDrehen(p As PointF, winkel As Single) As PointF
        Dim sin As Double = Math.Sin(winkel * Math.PI / 180)
        Dim cos As Double = Math.Cos(winkel * Math.PI / 180)

        Return New PointF(CSng(cos * p.X - sin * p.Y),
                          CSng(sin * p.X + cos * p.Y))
    End Function

    Public Shared Function pointFDrehen(p As PointF, mitte As PointF, winkel As Single) As PointF
        p.X -= mitte.X
        p.Y -= mitte.Y
        p = pointFDrehen(p, winkel)
        p.X += mitte.X
        p.Y += mitte.Y
        Return p
    End Function

#Region "String einlesen"

    Public Shared Function strToLower(str As String) As String
        Dim erg As New StringBuilder()
        Dim inAnführungzeichen As Boolean = False
        For i As Integer = 0 To str.Length - 1
            If str(i) = """" Then
                inAnführungzeichen = Not inAnführungzeichen
            End If
            If inAnführungzeichen Then
                erg.Append(str(i))
            Else
                erg.Append(str.Substring(i, 1).ToLower())
            End If
        Next
        Return erg.ToString()
    End Function


    Public Shared Function readSeperatedStrings(str As String) As String()
        Dim erg() As String = splitString(str, ","c)
        For i As Integer = 0 To erg.Length - 1
            erg(i) = erg(i).Trim()
        Next
        Return erg
    End Function

    Public Shared Function readSeperatedInts(str As String, variablen As Dictionary(Of String, Integer), variablen2 As Dictionary(Of String, Integer)) As Integer()
        Dim strings() As String = splitString(str, ","c)
        Dim erg(strings.Length - 1) As Integer
        For i As Integer = 0 To erg.Length - 1
            erg(i) = rechneStringAus(strings(i).Trim(), variablen, variablen2)
        Next
        Return erg
    End Function

    Public Shared Function readSeperatedAusdrücke(str As String, konst_lokal As Dictionary(Of String, Integer), params As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), parameter As List(Of TemplateParameter)) As Ausdruck()
        Dim strings() As String = splitString(str, ","c)
        Dim erg(strings.Length - 1) As Ausdruck
        For i As Integer = 0 To erg.Length - 1
            erg(i) = Ausdruck.EinlesenAusdruck(strings(i).Trim, konst_lokal, params, vars_Intern_lokal, parameter)
        Next
        Return erg
    End Function

    Public Shared Function rechneStringAus(str As String, variablen As Dictionary(Of String, Integer), variablen2 As Dictionary(Of String, Integer)) As Integer
        str = string_vorm_auswertenAufbereiten(str)
        Return CInt(auswerten(str, variablen, variablen2))
    End Function

    Public Shared Function string_vorm_auswertenAufbereiten(str As String) As String
        Dim res As New StringBuilder()
        Dim plusAnzahl As Integer = 0
        Dim minusAnzahl As Integer = 0
        Dim malAnzahl As Integer = 0
        Dim durchAnzahl As Integer = 0
        Dim inAnführungszeichen As Boolean = False

        For i As Integer = 0 To str.Length - 1
            If str(i) = """" Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                If str(i) = "+" Then
                    plusAnzahl += 1
                ElseIf str(i) = "-" Then
                    minusAnzahl += 1
                ElseIf str(i) = "*" Then
                    malAnzahl += 1
                ElseIf str(i) = "/" Then
                    durchAnzahl += 1
                ElseIf str(i) <> " " Then
                    If malAnzahl + durchAnzahl > 0 Then
                        'Malgeteilt hinzufügen
                        If malAnzahl > 1 Then
                            Throw New Exception("Zu viele '*' am Strück!")
                        End If
                        If durchAnzahl > 1 Then
                            Throw New Exception("Zu viele '/' am Stück!")
                        ElseIf durchAnzahl = 1 Then
                            res.Append("/")
                        ElseIf malAnzahl = 1 Then
                            res.Append("*")
                        End If
                    End If
                    If plusAnzahl + minusAnzahl > 0 Then
                        'Plusminus hinzufügen
                        If minusAnzahl Mod 2 = 1 Then
                            'Ist ein Minus!!!
                            res.Append("-")
                        Else
                            res.Append("+")
                        End If
                    End If
                    plusAnzahl = 0
                    minusAnzahl = 0
                    malAnzahl = 0
                    durchAnzahl = 0
                    res.Append(str(i))
                End If
            Else
                plusAnzahl = 0
                minusAnzahl = 0
                malAnzahl = 0
                durchAnzahl = 0
                res.Append(str(i))
            End If
        Next
        Return res.ToString()
    End Function

    Private Shared Function auswerten(str As String, v As Dictionary(Of String, Integer), v2 As Dictionary(Of String, Integer)) As Long
        If v2 IsNot Nothing AndAlso v2.ContainsKey(str) Then
            Return v2(str)
        End If
        If v.ContainsKey(str) Then
            Return v(str)
        End If

        If str.StartsWith("+") Then
            Return auswerten(str.Substring(1), v, v2)
        End If

        Dim resultParse As Integer
        If Integer.TryParse(str, resultParse) Then
            Return resultParse
        End If

        Dim summanden() As String = splitString(str, "+"c, "-"c)
        If summanden.Length > 1 Then
            Dim erg As Long = 0
            For i As Integer = 0 To summanden.Count - 1
                If summanden(i).StartsWith("-") Then
                    erg -= auswerten(summanden(i).Substring(1), v, v2)
                ElseIf summanden(i).StartsWith("+") Then
                    erg += auswerten(summanden(i).Substring(1), v, v2)
                Else
                    erg += auswerten(summanden(i), v, v2)
                End If
            Next
            Return erg
        ElseIf summanden.Length = 1 Then
            If summanden(0).StartsWith("-") Then
                Return -1 * auswerten(summanden(0).Substring(1), v, v2)
            End If
        End If

        Dim faktoren() As String = splitString(str, "*"c, "/"c)
        If faktoren.Length > 1 Then
            Dim erg As Long = 1
            For i As Integer = 0 To faktoren.Count - 1
                If faktoren(i).StartsWith("/") Then
                    erg \= auswerten(faktoren(i).Substring(1), v, v2)
                ElseIf faktoren(i).StartsWith("*"c) Then
                    erg *= auswerten(faktoren(i).Substring(1), v, v2)
                Else
                    erg *= auswerten(faktoren(i), v, v2)
                End If
            Next
            Return erg
        End If

        If str.StartsWith("(") AndAlso str.EndsWith(")") Then
            str = str.Substring(1, str.Length - 2)
            Return auswerten(str, v, v2)
        End If
        Throw New NotImplementedException("Fehler beim ausrechnen des Wertes '" & str & "'.")
    End Function

    Public Shared Function splitString(str As String, Trennzeichen As Char) As String()
        Dim liste As New List(Of String)
        Dim klammerpos As Integer = 0
        Dim lastString As New StringBuilder()
        Dim isInAnführungszeichen As Boolean = False
        For i As Integer = 0 To str.Length - 1
            If str(i) = """" Then
                isInAnführungszeichen = Not isInAnführungszeichen
            End If
            If Not isInAnführungszeichen Then
                If str(i) = "(" Then
                    klammerpos += 1
                    lastString.Append("(")
                ElseIf str(i) = ")" Then
                    klammerpos -= 1
                    lastString.Append(")")
                ElseIf klammerpos = 0 Then
                    If str(i) = Trennzeichen Then
                        If lastString.Length > 0 Then
                            liste.Add(lastString.ToString())
                            lastString.Clear()
                        End If
                    Else
                        lastString.Append(str(i))
                    End If
                Else
                    lastString.Append(str(i))
                End If
            Else
                lastString.Append(str(i))
            End If
        Next
        If lastString.Length > 0 Then
            liste.Add(lastString.ToString())
        End If
        Return liste.ToArray()
    End Function

    Public Shared Function splitString(str As String, Trennzeichen1 As Char, Trennzeichen2 As Char) As String()
        Dim liste As New List(Of String)
        Dim klammerpos As Integer = 0
        Dim lastString As New StringBuilder()

        Dim istGültigerOperator As Boolean = False

        For i As Integer = 0 To str.Length - 1
            If str(i) = "(" Then
                klammerpos += 1
                lastString.Append("(")
            ElseIf str(i) = ")" Then
                klammerpos -= 1
                lastString.Append(")")
            ElseIf klammerpos = 0 Then
                If istGültigerOperator AndAlso (str(i) = Trennzeichen1 OrElse str(i) = Trennzeichen2) Then
                    If lastString.Length > 0 Then
                        liste.Add(lastString.ToString())
                        lastString.Clear()
                        lastString.Append(str(i))
                    Else
                        Throw New Exception("Fehler beim Einlesen (F0001")
                    End If
                Else
                    lastString.Append(str(i))
                End If
            Else
                lastString.Append(str(i))
            End If

            If "+-*/".IndexOf(str(i)) <> -1 Then
                istGültigerOperator = False
            Else
                istGültigerOperator = True
            End If

        Next
        If lastString.Length > 0 Then
            liste.Add(lastString.ToString())
        End If
        Return liste.ToArray()
    End Function
#End Region

    Public Shared Function getRoundRect(x As Single, y As Single, w As Single, h As Single, radius As Single) As Drawing2D.GraphicsPath
        Dim p As New Drawing2D.GraphicsPath()
        Dim xw As Single = x + w
        Dim yh As Single = y + h
        Dim xwr As Single = xw - radius
        Dim yhr As Single = yh - radius
        Dim xr As Single = x + radius
        Dim yr As Single = y + radius
        Dim r2 As Single = radius + radius
        Dim xwr2 As Single = xw - r2
        Dim yhr2 As Single = yh - r2


        If w = 0 AndAlso h = 0 Then
            'Nichts malen!
        ElseIf w = 0 Then
            p.StartFigure()
            p.AddLine(x, y, x, y + h) 'wenn w=0, dann vertikale Linie!
            p.CloseFigure()
        ElseIf h = 0 Then
            p.StartFigure()
            p.AddLine(x, y, x + w, y) 'wenn h=0, dann horizontale Linie!
            p.CloseFigure()
        ElseIf radius = 0 Then 'wenn radius = 0, dann rechteck malen!
            p.StartFigure()
            p.AddRectangle(New RectangleF(x, y, w, h))
            p.CloseFigure()
        Else
            p.StartFigure()
            'Top Left Corner
            p.AddArc(x, y, r2, r2, 180, 90)

            'Top Edge
            p.AddLine(xr, y, xwr, y)

            'Top Right Corner
            p.AddArc(xwr2, y, r2, r2, 270, 90)

            'Right Edge
            p.AddLine(xw, yr, xw, yhr)

            'Bottom Right Corner
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90)

            'Bottom Edge
            p.AddLine(xwr, yh, xr, yh)

            'Bottom Left Corner
            p.AddArc(x, yhr2, r2, r2, 90, 90)

            'Left Edge
            p.AddLine(x, yhr, x, yr)

            p.CloseFigure()

        End If
        Return p
    End Function

    Public Shared Function getBezierBoundingBox(p1 As Point, p2 As Point, p3 As Point, p4 As Point) As Rectangle
        Dim a As New Point(-p1.X + 3 * p2.X - 3 * p3.X + p4.X,
                           -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y)
        Dim b As New Point(3 * p1.X - 6 * p2.X + 3 * p3.X,
                           3 * p1.Y - 6 * p2.Y + 3 * p3.Y)
        Dim c As New Point(-3 * p1.X + 3 * p2.X,
                           -3 * p1.Y + 3 * p2.Y)

        Dim minX As Integer = Math.Min(p1.X, p4.X)
        Dim minY As Integer = Math.Min(p1.Y, p4.Y)
        Dim maxX As Integer = Math.Max(p1.X, p4.X)
        Dim maxY As Integer = Math.Max(p1.Y, p4.Y)

        If a.X <> 0 Then
            Dim p As Double = 2 * b.X / (6 * a.X)
            Dim d As Double = p * p - c.X / (3 * a.X)
            If d >= 0 Then
                Dim t1 As Double = -p + Math.Sqrt(d)
                If t1 >= 0.0 AndAlso t1 <= 1.0 Then
                    Dim x1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.X + 3 * t1 * (1 - t1) * (1 - t1) * p2.X + 3 * t1 * t1 * (1 - t1) * p3.X + t1 * t1 * t1 * p4.X)
                    minX = Math.Min(minX, x1)
                    maxX = Math.Max(maxX, x1)
                End If
                t1 = -p - Math.Sqrt(d)
                If t1 >= 0.0 AndAlso t1 <= 1.0 Then
                    Dim x1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.X + 3 * t1 * (1 - t1) * (1 - t1) * p2.X + 3 * t1 * t1 * (1 - t1) * p3.X + t1 * t1 * t1 * p4.X)
                    minX = Math.Min(minX, x1)
                    maxX = Math.Max(maxX, x1)
                End If
            End If
        ElseIf b.X <> 0 Then
            Dim t1 As Double = -c.X / (2 * b.X)
            If t1 >= 0 AndAlso t1 <= 1.0 Then
                Dim x1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.X + 3 * t1 * (1 - t1) * (1 - t1) * p2.X + 3 * t1 * t1 * (1 - t1) * p3.X + t1 * t1 * t1 * p4.X)
                minX = Math.Min(minX, x1)
                maxX = Math.Max(maxX, x1)
            End If
        End If

        If a.Y <> 0 Then
            Dim p As Double = 2 * b.Y / (6 * a.Y)
            Dim d As Double = p * p - c.Y / (3 * a.Y)
            If d >= 0 Then
                Dim t1 As Double = -p + Math.Sqrt(d)
                If t1 >= 0.0 AndAlso t1 <= 1.0 Then
                    Dim y1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.Y + 3 * t1 * (1 - t1) * (1 - t1) * p2.Y + 3 * t1 * t1 * (1 - t1) * p3.Y + t1 * t1 * t1 * p4.Y)
                    minY = Math.Min(minY, y1)
                    maxY = Math.Max(maxY, y1)
                End If
                t1 = -p - Math.Sqrt(d)
                If t1 >= 0.0 AndAlso t1 <= 1.0 Then
                    Dim y1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.Y + 3 * t1 * (1 - t1) * (1 - t1) * p2.Y + 3 * t1 * t1 * (1 - t1) * p3.Y + t1 * t1 * t1 * p4.Y)
                    minY = Math.Min(minY, y1)
                    maxY = Math.Max(maxY, y1)
                End If
            End If
        ElseIf b.Y <> 0 Then
            Dim t1 As Double = -c.Y / (2 * b.Y)
            If t1 >= 0 AndAlso t1 <= 1.0 Then
                Dim y1 As Integer = CInt((1 - t1) * (1 - t1) * (1 - t1) * p1.Y + 3 * t1 * (1 - t1) * (1 - t1) * p2.Y + 3 * t1 * t1 * (1 - t1) * p3.Y + t1 * t1 * t1 * p4.Y)
                minY = Math.Min(minY, y1)
                maxY = Math.Max(maxY, y1)
            End If
        End If

        Return New Rectangle(minX, minY, maxX - minX, maxY - minY)
    End Function

    Public Shared Function approxNextPointOnBezier(p1 As Point, p2 As Point, p3 As Point, p4 As Point, pRef As Point, iters As Integer) As PointD
        Dim p1D As New PointD(p1.X - pRef.X, p1.Y - pRef.Y)
        Dim p2D As New PointD(p2.X - pRef.X, p2.Y - pRef.Y)
        Dim p3D As New PointD(p3.X - pRef.X, p3.Y - pRef.Y)
        Dim p4D As New PointD(p4.X - pRef.X, p4.Y - pRef.Y)

        Dim minDist As Double = p1D.X * p1D.X + p1D.Y * p1D.Y
        Dim minPoint As PointD = p1D
        Dim dist As Double = p4D.X * p4D.X + p4D.Y * p4D.Y
        If dist < minDist Then
            minDist = dist
            minPoint = p4D
        End If

        approxNextPointToZeroBezierKurveRekursiv(p1D, p2D, p3D, p4D, 0, iters, minPoint, minDist)

        Return New PointD(minPoint.X + pRef.X, minPoint.Y + pRef.Y)
    End Function

    Private Shared Sub approxNextPointToZeroBezierKurveRekursiv(p1 As PointD, p2 As PointD, p3 As PointD, p4 As PointD, iters As Integer, maxIters As Integer, ByRef minPoint As PointD, ByRef minDistSquared As Double)
        If iters < maxIters Then
            Dim p12, p23, p34, p123, p234, p1234 As PointD
            p12 = (p1 + p2) / 2
            p23 = (p2 + p3) / 2
            p34 = (p3 + p4) / 2

            p123 = (p12 + p23) / 2
            p234 = (p23 + p34) / 2
            p1234 = (p123 + p234) / 2

            approxNextPointToZeroBezierKurveRekursiv(p1, p12, p123, p1234, iters + 1, maxIters, minPoint, minDistSquared)
            approxNextPointToZeroBezierKurveRekursiv(p1234, p234, p34, p4, iters + 1, maxIters, minPoint, minDistSquared)
        ElseIf p1 <> p4 Then
            'Linie von p1 nach p4 testen

            Dim v As New PointD(p4.X - p1.X, p4.Y - p1.Y)
            Dim normal As New PointD(-v.Y, v.X)

            Dim zähler As Double = normal.X * p1.Y - normal.Y * p1.X
            Dim nenner As Double = -normal.X * v.Y + normal.Y * v.X
            Dim alpha As Double = zähler / nenner
            If alpha >= 0 AndAlso alpha <= 1 Then
                Dim schnittpunkt As New PointD(p1.X + alpha * v.X, p1.Y + alpha * v.Y)
                alpha = schnittpunkt.X * schnittpunkt.X + schnittpunkt.Y * schnittpunkt.Y
                If alpha < minDistSquared Then
                    minDistSquared = alpha
                    minPoint = schnittpunkt
                End If
            Else
                alpha = p1.X * p1.X + p1.Y * p1.Y
                If alpha < minDistSquared Then
                    minDistSquared = alpha
                    minPoint = p1
                End If
                alpha = p4.X * p4.X + p4.Y * p4.Y
                If alpha < minDistSquared Then
                    minDistSquared = alpha
                    minPoint = p4
                End If
            End If
        Else
            'Einfach nur p1=p4 testen!
            Dim dist As Double = p1.X * p1.X + p1.Y * p1.Y
            If dist < minDistSquared Then
                minDistSquared = dist
                minPoint = p1
            End If
        End If
    End Sub

    Public Shared Sub VerkürzeBezierEnde(ByRef p1 As Point, ByRef p2 As Point, ByRef p3 As Point, ByRef p4 As Point, verkürzung As Integer)
        Dim länge As Double = Math.Sqrt(abstandQuadrat(p1, p2)) + Math.Sqrt(abstandQuadrat(p2, p3)) + Math.Sqrt(abstandQuadrat(p3, p4))
        If länge < 1.0 Then
            Exit Sub
        End If
        Dim alpha As Double = 1.0 - verkürzung / länge

        Dim a As New Point(-p1.X + 3 * p2.X - 3 * p3.X + p4.X,
                           -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y)
        Dim b As New Point(3 * p1.X - 6 * p2.X + 3 * p3.X,
                           3 * p1.Y - 6 * p2.Y + 3 * p3.Y)
        Dim c As New Point(-3 * p1.X + 3 * p2.X,
                           -3 * p1.Y + 3 * p2.Y)
        Dim d As New Point(p1.X, p1.Y)

        Dim dist_t_p4 As Double = Double.MaxValue
        Dim ptx, pty As Double
        'Dim ptx_, pty_ As Double
        Dim iters As Integer = 0
        Dim alpha3, alpha2 As Double

        Dim dx, dy As Double
        Dim ableitung_malDist As Double

        While True
            'Debug.Print("Iteration " & iters & " alpha = " & alpha)

            '-----------------------------------------------
            'Newton Iteration zur Annäherung:
            '
            'Bezier-Kurve: B(alpha) = a*alpha^3+b*alpha^2+c*alpha+d
            '
            'Ziel ist Abstand |B(alpha)-P1| == verkürzung zu erreichen.
            'Man muss also die Nullstellen von |B(alpha)-P4)| - Verkürzung = 0 finden. Der Initial-guess ist alpha wie oben berechnet. Dies soll mit Newton-Iteration verbessert werden!
            'Also ist definiert: f(alpha) = |B(alpha)-P4)| - Verkürzung
            'gemäß Newton Iteration: alpha = alpha - f(alpha) / f'(alpha)
            '
            'f(alpha) kann einfach berechnet werden (Abstand Punkt zu B(alpha))
            '
            'f'(alpha) ergibt sich zu: deriv[sqrt{ (B.x(alpha) - P4.x)^2 + (B.y(alpha) - P4.y)^2 } + const]
            '--> Die Wurzel kann mit Kettenregel abgeleitet werden: deriv[sqrt(g(x))] = deriv(g(x))/(2*sqrt(g(x))
            '--> g(x) (alles in der Wurzel) wird ebenfalls mit Kettenregel abgeleitet:
            'Ableitung Zähler = 2 * (B.x(alpha) - P4.x) * B.x'(alpha) + 2 * (B.y(alpha) - P4.y) * B.y'(alpha)
            'Der Nenner ist ja einfach nur 2*Abstand zu Punkt und ist schon für f(alpha) berechnet worden
            '
            'Damit fehlen nur noch die Ableitungen von B(alpha) in x und y richtung. Die ergeben sich einfach als Ableitung des Polynoms dritten gerades:
            'B'(alpha) = 3 * alpha^2 * a + 2 * alpha * b + cs
            '-----------------------------------------------

            'Zuerst alpha^2 und alpha^3 berechnen
            alpha2 = alpha * alpha
            alpha3 = alpha2 * alpha
            'Jetzt den Punkt ptx, pty auf der Bezierkurve berechnen (B(alpha))
            ptx = alpha3 * a.X + alpha2 * b.X + alpha * c.X + d.X
            pty = alpha3 * a.Y + alpha2 * b.Y + alpha * c.Y + d.Y
            'Abstand dieses Punktes zum Anfangspunkt
            dist_t_p4 = Math.Sqrt((ptx - p4.X) * (ptx - p4.X) + (pty - p4.Y) * (pty - p4.Y))

            'Wenn schon besser als halbe minimaldist ist, kann man eh aufhören
            If Math.Abs(dist_t_p4 - verkürzung) <= 0.5 Then
                Exit While
            End If
            'Jetzt noch die Ableitung B'(x) berechnen in x und y richtungs
            dx = 3 * a.X * alpha2 + 2 * b.X * alpha + c.X
            dy = 3 * a.Y * alpha2 + 2 * b.Y * alpha + c.Y
            'Jetzt die Ableitung von f(alpha) berechnen für Newton Näherung
            ableitung_malDist = ((ptx - p4.X) * dx + (pty - p4.Y) * dy)
            If ableitung_malDist = 0 Then
                Debug.Print("Abbruch weil die Ableitung Null ist...")
                Return
            End If
            'Das neue alpha ergibt sich jetzt aus der Newton-Formel
            alpha = alpha - dist_t_p4 * (dist_t_p4 - verkürzung) / ableitung_malDist

            If alpha > 1 Then alpha = 1
            If alpha < 0 Then alpha = 0
            iters += 1
            If iters > 15 Then
                Debug.Print("Abbruch wegen zu vielen Iterationen...")
                Return
            End If
        End While
        alpha2 = 1 - alpha
        Dim p12, p23, p34, p123, p234, p1234 As PointD
        p12 = New PointD(alpha2 * p1.X + alpha * p2.X, alpha2 * p1.Y + alpha * p2.Y)
        p23 = New PointD(alpha2 * p2.X + alpha * p3.X, alpha2 * p2.Y + alpha * p3.Y)
        p34 = New PointD(alpha2 * p3.X + alpha * p4.X, alpha2 * p3.Y + alpha * p4.Y)

        p123 = New PointD(alpha2 * p12.X + alpha * p23.X, alpha2 * p12.Y + alpha * p23.Y)
        p234 = New PointD(alpha2 * p23.X + alpha * p34.X, alpha2 * p23.Y + alpha * p34.Y)

        p1234 = New PointD(alpha2 * p123.X + alpha * p234.X, alpha2 * p123.Y + alpha * p234.Y)

        p2 = New Point(CInt(p12.X), CInt(p12.Y))
        p3 = New Point(CInt(p123.X), CInt(p123.Y))
        p4 = New Point(CInt(p1234.X), CInt(p1234.Y))
    End Sub

    Public Shared Sub VerkürzeBezierStart(ByRef p1 As Point, ByRef p2 As Point, ByRef p3 As Point, ByRef p4 As Point, verkürzung As Integer)
        Dim länge As Double = Math.Sqrt(abstandQuadrat(p1, p2)) + Math.Sqrt(abstandQuadrat(p2, p3)) + Math.Sqrt(abstandQuadrat(p3, p4))
        If länge < 1.0 Then
            Exit Sub
        End If
        Dim alpha As Double = verkürzung / länge

        Dim a As New Point(-p1.X + 3 * p2.X - 3 * p3.X + p4.X,
                           -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y)
        Dim b As New Point(3 * p1.X - 6 * p2.X + 3 * p3.X,
                           3 * p1.Y - 6 * p2.Y + 3 * p3.Y)
        Dim c As New Point(-3 * p1.X + 3 * p2.X,
                           -3 * p1.Y + 3 * p2.Y)
        Dim d As New Point(p1.X, p1.Y)

        Dim dist_t_p1 As Double = Double.MaxValue
        Dim ptx, pty As Double
        'Dim ptx_, pty_ As Double
        Dim iters As Integer = 0
        Dim alpha3, alpha2 As Double

        Dim dx, dy As Double
        Dim ableitung_malDist As Double

        While True
            'Debug.Print("Iteration " & iters & " alpha = " & alpha)

            '-----------------------------------------------
            'Newton Iteration zur Annäherung:
            '
            'Bezier-Kurve: B(alpha) = a*alpha^3+b*alpha^2+c*alpha+d
            '
            'Ziel ist Abstand |B(alpha)-P1| == verkürzung zu erreichen.
            'Man muss also die Nullstellen von |B(alpha)-P1)| - Verkürzung = 0 finden. Der Initial-guess ist alpha wie oben berechnet. Dies soll mit Newton-Iteration verbessert werden!
            'Also ist definiert: f(alpha) = |B(alpha)-P1)| - Verkürzung
            'gemäß Newton Iteration: alpha = alpha - f(alpha) / f'(alpha)
            '
            'f(alpha) kann einfach berechnet werden (Abstand Punkt zu B(alpha))
            '
            'f'(alpha) ergibt sich zu: deriv[sqrt{ (B.x(alpha) - P1.x)^2 + (B.y(alpha) - P1.y)^2 } + const]
            '--> Die Wurzel kann mit Kettenregel abgeleitet werden: deriv[sqrt(g(x))] = deriv(g(x))/(2*sqrt(g(x))
            '--> g(x) (alles in der Wurzel) wird ebenfalls mit Kettenregel abgeleitet:
            'Ableitung Zähler = 2 * (B.x(alpha) - P1.x) * B.x'(alpha) + 2 * (B.y(alpha) - P1.y) * B.y'(alpha)
            'Der Nenner ist ja einfach nur 2*Abstand zu Punkt und ist schon für f(alpha) berechnet worden
            '
            'Damit fehlen nur noch die Ableitungen von B(alpha) in x und y richtung. Die ergeben sich einfach als Ableitung des Polynoms dritten gerades:
            'B'(alpha) = 3 * alpha^2 * a + 2 * alpha * b + c
            '-----------------------------------------------

            'Zuerst alpha^2 und alpha^3 berechnen
            alpha2 = alpha * alpha
            alpha3 = alpha2 * alpha
            'Jetzt den Punkt ptx, pty auf der Bezierkurve berechnen (B(alpha))
            ptx = alpha3 * a.X + alpha2 * b.X + alpha * c.X + d.X
            pty = alpha3 * a.Y + alpha2 * b.Y + alpha * c.Y + d.Y
            'Abstand dieses Punktes zum Anfangspunkt
            dist_t_p1 = Math.Sqrt((ptx - p1.X) * (ptx - p1.X) + (pty - p1.Y) * (pty - p1.Y))
            'Wenn schon besser als halbe minimaldist ist, kann man eh aufhören
            If Math.Abs(dist_t_p1 - verkürzung) <= 0.5 Then
                Exit While
            End If
            'Jetzt noch die Ableitung B'(x) berechnen in x und y richtungs
            dx = 3 * a.X * alpha2 + 2 * b.X * alpha + c.X
            dy = 3 * a.Y * alpha2 + 2 * b.Y * alpha + c.Y
            'Jetzt die Ableitung von f(alpha) berechnen für Newton Näherung
            ableitung_malDist = ((ptx - p1.X) * dx + (pty - p1.Y) * dy)
            If ableitung_malDist = 0 Then
                Debug.Print("Abbruch weil die Ableitung Null ist...")
                Return
            End If
            'Das neue alpha ergibt sich jetzt aus der Newton-Formel
            alpha = alpha - dist_t_p1 * (dist_t_p1 - verkürzung) / ableitung_malDist

            If alpha > 1 Then alpha = 1
            If alpha < 0 Then alpha = 0
            iters += 1
            If iters > 15 Then
                Debug.Print("Abbruch wegen zu vielen Iterationen...")
                Return
            End If
        End While
        alpha2 = 1 - alpha
        Dim p12, p23, p34, p123, p234, p1234 As PointD
        p12 = New PointD(alpha2 * p1.X + alpha * p2.X, alpha2 * p1.Y + alpha * p2.Y)
        p23 = New PointD(alpha2 * p2.X + alpha * p3.X, alpha2 * p2.Y + alpha * p3.Y)
        p34 = New PointD(alpha2 * p3.X + alpha * p4.X, alpha2 * p3.Y + alpha * p4.Y)

        p123 = New PointD(alpha2 * p12.X + alpha * p23.X, alpha2 * p12.Y + alpha * p23.Y)
        p234 = New PointD(alpha2 * p23.X + alpha * p34.X, alpha2 * p23.Y + alpha * p34.Y)

        p1234 = New PointD(alpha2 * p123.X + alpha * p234.X, alpha2 * p123.Y + alpha * p234.Y)

        p1 = New Point(CInt(p1234.X), CInt(p1234.Y))
        p2 = New Point(CInt(p234.X), CInt(p234.Y))
        p3 = New Point(CInt(p34.X), CInt(p34.Y))
    End Sub

    Public Shared Sub VerkürzeBezierStartOLD(ByRef p1 As Point, ByRef p2 As Point, ByRef p3 As Point, ByRef p4 As Point, verkürzung As Integer)
        Dim länge As Double = Math.Sqrt(abstandQuadrat(p1, p2)) + Math.Sqrt(abstandQuadrat(p2, p3)) + Math.Sqrt(abstandQuadrat(p3, p4))
        If länge < 1.0 Then
            Exit Sub
        End If
        Dim alpha As Double = verkürzung / länge

        Dim a As New Point(-p1.X + 3 * p2.X - 3 * p3.X + p4.X,
                           -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y)
        Dim b As New Point(3 * p1.X - 6 * p2.X + 3 * p3.X,
                           3 * p1.Y - 6 * p2.Y + 3 * p3.Y)
        Dim c As New Point(-3 * p1.X + 3 * p2.X,
                           -3 * p1.Y + 3 * p2.Y)
        Dim d As New Point(p1.X, p1.Y)

        Dim dist_t_p1 As Double = Double.MaxValue
        Dim ptx, pty As Double
        'Dim ptx_, pty_ As Double
        Dim iters As Integer = 0
        Dim alpha3, alpha2 As Double
        While Math.Abs(dist_t_p1 - verkürzung) > 1.0
            alpha2 = alpha * alpha
            alpha3 = alpha2 * alpha
            ptx = alpha3 * a.X + alpha2 * b.X + alpha * c.X + d.X
            pty = alpha3 * a.Y + alpha2 * b.Y + alpha * c.Y + d.Y
            dist_t_p1 = Math.Sqrt((ptx - p1.X) * (ptx - p1.X) + (pty - p1.Y) * (pty - p1.Y))

            alpha = alpha * verkürzung / dist_t_p1
            If alpha > 1 Then alpha = 1
            If alpha < 0 Then alpha = 0
            iters += 1
            If iters > 10 Then
                Debug.Print("Abbruch wegen zu vielen Iterationen...")
                Exit While
            End If
        End While
        alpha2 = 1 - alpha
        Dim p12, p23, p34, p123, p234, p1234 As PointD
        p12 = New PointD(alpha2 * p1.X + alpha * p2.X, alpha2 * p1.Y + alpha * p2.Y)
        p23 = New PointD(alpha2 * p2.X + alpha * p3.X, alpha2 * p2.Y + alpha * p3.Y)
        p34 = New PointD(alpha2 * p3.X + alpha * p4.X, alpha2 * p3.Y + alpha * p4.Y)

        p123 = New PointD(alpha2 * p12.X + alpha * p23.X, alpha2 * p12.Y + alpha * p23.Y)
        p234 = New PointD(alpha2 * p23.X + alpha * p34.X, alpha2 * p23.Y + alpha * p34.Y)

        p1234 = New PointD(alpha2 * p123.X + alpha * p234.X, alpha2 * p123.Y + alpha * p234.Y)

        p1 = New Point(CInt(p1234.X), CInt(p1234.Y))
        p2 = New Point(CInt(p234.X), CInt(p234.Y))
        p3 = New Point(CInt(p34.X), CInt(p34.Y))
    End Sub

    Public Shared Sub VerkürzeBezierEndeOLD(ByRef p1 As Point, ByRef p2 As Point, ByRef p3 As Point, ByRef p4 As Point, verkürzung As Integer)
        Dim länge As Double = Math.Sqrt(abstandQuadrat(p1, p2)) + Math.Sqrt(abstandQuadrat(p2, p3)) + Math.Sqrt(abstandQuadrat(p3, p4))
        If länge < 1.0 Then
            Exit Sub
        End If
        Dim alpha As Double = 1.0 - verkürzung / länge

        Dim a As New Point(-p1.X + 3 * p2.X - 3 * p3.X + p4.X,
                           -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y)
        Dim b As New Point(3 * p1.X - 6 * p2.X + 3 * p3.X,
                           3 * p1.Y - 6 * p2.Y + 3 * p3.Y)
        Dim c As New Point(-3 * p1.X + 3 * p2.X,
                           -3 * p1.Y + 3 * p2.Y)
        Dim d As New Point(p1.X, p1.Y)

        Dim dist_t_p4 As Double = Double.MaxValue
        Dim ptx, pty As Double
        'Dim ptx_, pty_ As Double
        Dim iters As Integer = 0
        Dim alpha3, alpha2 As Double
        While Math.Abs(dist_t_p4 - verkürzung) > 1.0
            alpha2 = alpha * alpha
            alpha3 = alpha2 * alpha
            ptx = alpha3 * a.X + alpha2 * b.X + alpha * c.X + d.X
            pty = alpha3 * a.Y + alpha2 * b.Y + alpha * c.Y + d.Y
            dist_t_p4 = Math.Sqrt((ptx - p4.X) * (ptx - p4.X) + (pty - p4.Y) * (pty - p4.Y))

            alpha = 1.0 - (1 - alpha) * verkürzung / dist_t_p4
            If alpha > 1 Then alpha = 1
            If alpha < 0 Then alpha = 0
            iters += 1
            If iters > 10 Then
                Debug.Print("Abbruch wegen zu vielen Iterationen...")
                Exit While
            End If
        End While
        alpha2 = 1 - alpha
        Dim p12, p23, p34, p123, p234, p1234 As PointD
        p12 = New PointD(alpha2 * p1.X + alpha * p2.X, alpha2 * p1.Y + alpha * p2.Y)
        p23 = New PointD(alpha2 * p2.X + alpha * p3.X, alpha2 * p2.Y + alpha * p3.Y)
        p34 = New PointD(alpha2 * p3.X + alpha * p4.X, alpha2 * p3.Y + alpha * p4.Y)

        p123 = New PointD(alpha2 * p12.X + alpha * p23.X, alpha2 * p12.Y + alpha * p23.Y)
        p234 = New PointD(alpha2 * p23.X + alpha * p34.X, alpha2 * p23.Y + alpha * p34.Y)

        p1234 = New PointD(alpha2 * p123.X + alpha * p234.X, alpha2 * p123.Y + alpha * p234.Y)

        p2 = New Point(CInt(p12.X), CInt(p12.Y))
        p3 = New Point(CInt(p123.X), CInt(p123.Y))
        p4 = New Point(CInt(p1234.X), CInt(p1234.Y))
    End Sub

    Public Shared Function abstand(p1 As PointF, p2 As PointF) As Single
        Return CSng(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)))
    End Function

    Public Shared Function schneidetRechteck(start As Point, ende As Point, r As Rectangle, EXTRA_SPACE As Integer) As Boolean
        'liegt Start im Rechteck?
        If start.X >= r.X - EXTRA_SPACE AndAlso start.Y >= r.Y - EXTRA_SPACE AndAlso start.X <= r.X + r.Width + EXTRA_SPACE AndAlso start.Y <= r.Y + r.Height + EXTRA_SPACE Then
            Return True
        End If
        'liegt Ende im Rechteck?
        If ende.X >= r.X - EXTRA_SPACE AndAlso ende.Y >= r.Y - EXTRA_SPACE AndAlso ende.X <= r.X + r.Width + EXTRA_SPACE AndAlso ende.Y <= r.Y + r.Height + EXTRA_SPACE Then
            Return True
        End If
        'liegt Rechteck in der Mitte zwischen start und Ende?
        If start.Y = ende.Y Then
            If start.Y >= r.Y - EXTRA_SPACE AndAlso start.Y <= r.Y + r.Height + EXTRA_SPACE Then
                If start.X <= r.X AndAlso ende.X >= r.X + r.Width Then
                    Return True
                End If
                If ende.X <= r.X AndAlso start.X >= r.X + r.Width Then
                    Return True
                End If
            End If
        ElseIf start.X = ende.X Then
            If start.X >= r.X - EXTRA_SPACE AndAlso start.X <= r.X + r.Width + EXTRA_SPACE Then
                If start.Y <= r.Y AndAlso ende.Y >= r.Y + r.Height Then
                    Return True
                End If
                If ende.Y <= r.Y AndAlso start.Y >= r.Y + r.Height Then
                    Return True
                End If
            End If
        Else
            Throw New NotImplementedException()
        End If
        Return False
    End Function

    Public Shared Function liegtAufRand(start As Point, ende As Point, r As Rectangle, EXTRA_SPACE As Integer) As Boolean
        Dim startAmRand As Boolean = liegtAufRand(start, r, EXTRA_SPACE)
        Dim endeAmRand As Boolean = liegtAufRand(ende, r, EXTRA_SPACE)
        If startAmRand OrElse endeAmRand Then
            Dim startAußerhalb As Boolean = start.X < r.X - EXTRA_SPACE OrElse start.Y < r.Y - EXTRA_SPACE OrElse start.Y > r.Y + r.Height + EXTRA_SPACE OrElse start.X > r.X + r.Width + EXTRA_SPACE
            Dim endeAußerhalb As Boolean = ende.X < r.X - EXTRA_SPACE OrElse ende.Y < r.Y - EXTRA_SPACE OrElse ende.Y > r.Y + r.Height + EXTRA_SPACE OrElse ende.X > r.X + r.Width + EXTRA_SPACE
            Return startAußerhalb OrElse endeAußerhalb
        End If
        Return False
    End Function

    Private Shared Function liegtAufRand(start As Point, r As Rectangle, EXTRA_SPACE As Integer) As Boolean
        If start.Y >= r.Y - EXTRA_SPACE AndAlso start.Y <= r.Y + r.Height + EXTRA_SPACE Then
            If start.X = r.X - EXTRA_SPACE OrElse start.X = r.X + r.Width + EXTRA_SPACE Then
                Return True
            End If
        End If
        If start.X >= r.X - EXTRA_SPACE AndAlso start.X <= r.X + r.Width + EXTRA_SPACE Then
            If start.Y = r.Y - EXTRA_SPACE OrElse start.Y = r.Y + r.Height + EXTRA_SPACE Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Shared Function getSchnittpunkt_alpha(p1 As Point, p2 As Point, linie As DO_Linie) As Double
        Dim start As Point = linie.p1
        Dim ende As Point = linie.p2
        If start.X < p1.X AndAlso start.X < p2.X AndAlso ende.X < p1.X AndAlso ende.X < p2.X Then
            Return -1
        End If
        If start.X > p1.X AndAlso start.X > p2.X AndAlso ende.X > p1.X AndAlso ende.X > p2.X Then
            Return -1
        End If
        If start.Y < p1.Y AndAlso start.Y < p2.Y AndAlso ende.Y < p1.Y AndAlso ende.Y < p2.Y Then
            Return -1
        End If
        If start.Y > p1.Y AndAlso start.Y > p2.Y AndAlso ende.Y > p1.Y AndAlso ende.Y > p2.Y Then
            Return -1
        End If

        Dim dp1 As New Point(p2.X - p1.X, p2.Y - p1.Y)
        Dim dp2 As New Point(ende.X - start.X, ende.Y - start.Y)

        Dim det As Long = -CLng(dp2.X) * dp1.Y + CLng(dp2.Y) * dp1.X
        If det = 0 Then Return -1

        Dim zähler As Long = CLng(dp2.X) * (p1.Y - start.Y) - CLng(dp2.Y) * (p1.X - start.X)

        Dim zähler2 As Long = -CLng(dp1.Y) * (p1.X - start.X) + CLng(dp1.X) * (p1.Y - start.Y)
        If zähler2 / det >= 0 AndAlso zähler2 / det <= 1 Then
            Return zähler / det
        Else
            Return -1
        End If
    End Function

    Public Shared Function getSchnittpunkt_alpha(p1 As Point, p2 As Point, cir As DO_Ellipse) As List(Of Double)
        Dim mX As Double = cir.r.X + cir.r.Width / 2
        Dim mY As Double = cir.r.Y + cir.r.Height / 2
        Dim rX As Double = cir.r.Width / 2
        Dim rY As Double = cir.r.Height / 2
        Dim x1 As Double = (p1.X - mX) / rX
        Dim y1 As Double = (p1.Y - mY) / rY
        Dim dx As Double = (p2.X - mX) / rX - x1
        Dim dy As Double = (p2.Y - mY) / rY - y1

        Dim teiler As Double = dx * dx + dy * dy
        If teiler = 0 Then
            Return Nothing
        End If
        Dim pHalbe As Double = (dx * x1 + dy * y1) / teiler
        Dim q As Double = (x1 * x1 + y1 * y1 - 1) / teiler
        Dim D As Double = pHalbe * pHalbe - q
        If D < 0 Then
            Return Nothing
        End If
        Dim erg As List(Of Double)
        If D = 0 Then
            erg = New List(Of Double)(1)
            erg.Add(-pHalbe)
        Else
            erg = New List(Of Double)(2)
            D = Math.Sqrt(D)
            erg.Add(-pHalbe + D)
            erg.Add(-pHalbe - D)
        End If
        Return erg
    End Function

    Public Shared Function getSchnittpunkt_alpha(p1 As Point, p2 As Point, arc As DO_Arc) As List(Of Double)
        Dim mX As Double = arc.Mitte.X
        Dim mY As Double = arc.Mitte.Y
        Dim rX As Double = arc.radiusX
        Dim rY As Double = arc.radiusY
        Dim x1 As Double = (p1.X - mX) / rX
        Dim y1 As Double = (p1.Y - mY) / rY
        Dim dx As Double = (p2.X - mX) / rX - x1
        Dim dy As Double = (p2.Y - mY) / rY - y1

        Dim teiler As Double = dx * dx + dy * dy
        If teiler = 0 Then
            Return Nothing
        End If
        Dim pHalbe As Double = (dx * x1 + dy * y1) / teiler
        Dim q As Double = (x1 * x1 + y1 * y1 - 1) / teiler
        Dim D As Double = pHalbe * pHalbe - q
        If D < 0 Then
            Return Nothing
        End If
        Dim erg As List(Of Double)
        If D = 0 Then
            erg = New List(Of Double)(1)
            erg.Add(-pHalbe)
        Else
            erg = New List(Of Double)(2)
            D = Math.Sqrt(D)
            erg.Add(-pHalbe + D)
            erg.Add(-pHalbe - D)
        End If

        Dim startwinkel As Single = arc.startwinkel Mod 360
        If startwinkel < 0 Then startwinkel += 360
        Dim endewinkel As Single = (arc.startwinkel + arc.deltawinkel) Mod 360
        If endewinkel < 0 Then endewinkel += 360

        If arc.deltawinkel > -360 AndAlso arc.deltawinkel < 360 Then
            'Nur gegenprüfen, wenn es weniger als ein Kreis ist
            For i As Integer = erg.Count - 1 To 0 Step -1
                Dim x As Double = x1 + erg(i) * dx
                Dim y As Double = y1 + erg(i) * dy
                Dim winkel As Double = Math.Atan2(y, x)
                winkel = (winkel * 180 / Math.PI) Mod 360
                If winkel < 0 Then winkel += 360

                If arc.deltawinkel > 0 Then
                    If endewinkel > startwinkel Then
                        If winkel < startwinkel OrElse winkel > endewinkel Then
                            erg.RemoveAt(i)
                        End If
                    Else
                        If winkel > endewinkel AndAlso winkel < startwinkel Then
                            erg.RemoveAt(i)
                        End If
                    End If
                ElseIf arc.deltawinkel < 0 Then
                    If endewinkel < startwinkel Then
                        If winkel > startwinkel OrElse winkel < endewinkel Then
                            erg.RemoveAt(i)
                        End If
                    Else
                        If winkel > startwinkel AndAlso winkel < endewinkel Then
                            erg.RemoveAt(i)
                        End If
                    End If
                Else
                    erg.RemoveAt(i)
                End If
            Next
        End If
        Return erg
    End Function

    Public Shared Function getSchnittpunkt_alpha(p1 As Point, p2 As Point, bezier As DO_Bezier) As List(Of Double)
        Dim erg As New List(Of Double)
        For i As Integer = 0 To bezier.punkte.Count - 4 Step 3
            Dim erg1 As List(Of Double) = getSchnittpunkt_alpha(p1, p2, New PointD(bezier.punkte(i)), New PointD(bezier.punkte(i + 1)), New PointD(bezier.punkte(i + 2)), New PointD(bezier.punkte(i + 3)))
            If erg1 IsNot Nothing AndAlso erg1.Count > 0 Then
                erg.AddRange(erg1)
            End If
        Next
        Return erg
    End Function

    Public Shared Function getSchnittpunkt_alpha(p1 As Point, p2 As Point, B0 As PointD, B1 As PointD, B2 As PointD, B3 As PointD) As List(Of Double)
        'Algorithmus angelehnt an: https://www.particleincell.com/2013/cubic-line-intersection/
        Dim A As New PointD(-B0.X + 3 * B1.X - 3 * B2.X + B3.X, -B0.Y + 3 * B1.Y - 3 * B2.Y + B3.Y)
        Dim B As New PointD(3 * B0.X - 6 * B1.X + 3 * B2.X, 3 * B0.Y - 6 * B1.Y + 3 * B2.Y)
        Dim C As New PointD(-3 * B0.X + 3 * B1.X, -3 * B0.Y + 3 * B1.Y)
        Dim D As New PointD(B0.X, B0.Y)

        Dim alpha As Long = CLng(p2.Y) - p1.Y
        Dim beta As Long = CLng(p1.X) - p2.X
        Dim gamma As Long = -alpha * p1.X - beta * p1.Y

        Dim k1 As Double = alpha * A.X + beta * A.Y
        Dim k2 As Double = alpha * B.X + beta * B.Y
        Dim k3 As Double = alpha * C.X + beta * C.Y
        Dim k4 As Double = alpha * D.X + beta * D.Y + gamma

        'Die Gleichung k1*t^3 + k2*t^2 + k3*t + k4 = 0 muss gelöst werden
        Dim erg As New List(Of Double)
        If k1 = 0 Then 'nur quadratische Gleichung
            If k2 = 0 Then 'nur lineare Gleichung
                'k3 * t + k4 = 0
                If k3 = 0 Then
                    Return Nothing
                End If
                Dim t As Double = -k4 / k3
                erg.Add(t)
            Else
                'k2*t^2+k3*t+k4=0
                Dim p As Double = k3 / k2
                Dim q As Double = k4 / k2
                Dim delta As Double = p * p / 4 - q
                If delta < 0 Then
                    Return Nothing
                ElseIf delta = 0 Then
                    erg.Add(-p / 2)
                Else
                    delta = Math.Sqrt(delta)
                    erg.Add(-p / 2 + delta)
                    erg.Add(-p / 2 - delta)
                End If
            End If
        Else
            'normalisieren
            k2 = k2 / k1
            k3 = k3 / k1
            k4 = k4 / k1
            't^3+k2*t^2+k3*t+k4 = 0
            Dim Q As Double = (3 * k3 - k2 * k2) / 9
            Dim R As Double = (9 * k2 * k3 - 27 * k4 - 2 * k2 * k2 * k2) / 54
            Dim delta As Double = Q * Q * Q + R * R
            If delta >= 0 Then
                Dim S As Double = sgn(R + Math.Sqrt(delta)) * Math.Pow(Math.Abs(R + Math.Sqrt(delta)), (1 / 3))
                Dim T As Double = sgn(R - Math.Sqrt(delta)) * Math.Pow(Math.Abs(R - Math.Sqrt(delta)), (1 / 3))

                erg.Add(-k2 / 3 + (S + T)) 'real root
                erg.Add(-k2 / 3 - (S + T) / 2) 'real part Of complex root
                erg.Add(-k2 / 3 - (S + T) / 2) 'real part Of complex root
                Dim Im As Double = Math.Abs(Math.Sqrt(3) * (S - T) / 2) 'complex part Of root pair   

                'discard complex roots
                If (Im <> 0) Then
                    erg.RemoveAt(erg.Count - 1)
                    erg.RemoveAt(erg.Count - 1)
                End If
            Else
                Dim th As Double = Math.Acos(R / Math.Sqrt(-Q * Q * Q))
                erg.Add(2 * Math.Sqrt(-Q) * Math.Cos(th / 3) - k2 / 3)
                erg.Add(2 * Math.Sqrt(-Q) * Math.Cos((th + 2 * Math.PI) / 3) - k2 / 3)
                erg.Add(2 * Math.Sqrt(-Q) * Math.Cos((th + 4 * Math.PI) / 3) - k2 / 3)
            End If
        End If
        'check if it is in valid range of bezier
        For i As Integer = erg.Count - 1 To 0 Step -1
            erg(i) = Math.Round(erg(i), 8)
            If erg(i) < 0 OrElse erg(i) > 1 Then
                erg.RemoveAt(i)
            End If
        Next
        If erg.Count = 0 Then Return Nothing
        'calc x,y coordinates and the coresponding alpha values
        Dim px, py As Double
        Dim ergLine As New List(Of Double)(erg.Count)
        For i As Integer = 0 To erg.Count - 1
            px = D.X + erg(i) * (C.X + erg(i) * (B.X + erg(i) * A.X))
            py = D.Y + erg(i) * (C.Y + erg(i) * (B.Y + erg(i) * A.Y))
            Dim dx As Long = CLng(p2.X) - p1.X
            Dim dy As Long = CLng(p2.Y) - p1.Y
            Dim det As Long = dx * dx + dy * dy
            If det <> 0 Then
                Dim alpha_line As Double = ((px - p1.X) * dx + (py - p1.Y) * dy) / det
                ergLine.Add(alpha_line)
            End If
        Next
        Return ergLine
    End Function

    Private Shared Function sgn(x As Double) As Double
        If x < 0 Then Return -1
        If x > 0 Then Return 1
        Return 0
    End Function
End Class

''' <summary>
''' Stellt einen Punkt mit Doppelte-Koordinaten da.
''' Verfügt über grundlegende Operatoren. +, -, *, /
''' </summary>
''' <remarks></remarks>
Public Structure PointD
    Public Property X As Double
    Public Property Y As Double
    Public Sub New(ByVal x As Double, ByVal y As Double)
        Me.X = x
        Me.Y = y
    End Sub
    Public Sub New(ByVal p As Point)
        Me.X = p.X
        Me.Y = p.Y
    End Sub
    Public Sub New(ByVal p As PointF)
        Me.X = p.X
        Me.Y = p.Y
    End Sub

    Public Sub drehen(ByVal drehpunkt As PointD, ByVal winkel As Double)
        Dim sinWinkel As Double = Math.Sin(winkel * Math.PI / 180)
        Dim cosWinkel As Double = Math.Cos(winkel * Math.PI / 180)
        X -= drehpunkt.X
        Y -= drehpunkt.Y
        Dim xneu As Double = X * cosWinkel - Y * sinWinkel
        Dim yneu As Double = X * sinWinkel + Y * cosWinkel
        X = xneu + drehpunkt.X
        Y = yneu + drehpunkt.Y
    End Sub

    Public Shared Operator =(ByVal a As PointD, ByVal b As PointD) As Boolean
        Return (a.X = b.X) AndAlso (a.Y = b.Y)
    End Operator
    Public Shared Operator <>(ByVal a As PointD, ByVal b As PointD) As Boolean
        Return Not (a = b)
    End Operator
    Public Shared Operator -(ByVal a As PointD, ByVal b As PointD) As PointD
        Return New PointD(a.X - b.X, a.Y - b.Y)
    End Operator
    Public Shared Operator +(ByVal a As PointD, ByVal b As PointD) As PointD
        Return New PointD(a.X + b.X, a.Y + b.Y)
    End Operator
    Public Shared Operator *(ByVal a As PointD, ByVal f As Double) As PointD
        Return New PointD(a.X * f, a.Y * f)
    End Operator
    Public Shared Operator *(f As Double, ByVal a As PointD) As PointD
        Return a * f
    End Operator
    Public Shared Operator /(ByVal a As PointD, ByVal f As Double) As PointD
        Return New PointD(a.X / f, a.Y / f)
    End Operator

    ''' <summary>
    ''' Gibt den Betrag des als Vektor interpretierten Punktes zurück
    ''' </summary>
    ''' <returns></returns>
    Public Function Abs() As Double
        Return X * X + Y * Y
    End Function

    ''' <summary>
    ''' Gibt den normierten Vektor zurück.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function norm() As PointD
        Dim länge As Double = Math.Sqrt(X * X + Y * Y)
        If länge = 0 Then Return Me
        Return New PointD(X / länge, Y / länge)
    End Function

    Public Function toPoint() As Point
        Return New Point(CInt(X), CInt(Y))
    End Function
    Public Function toPointF() As PointF
        Return New PointF(CSng(X), CSng(Y))
    End Function
    Public Overrides Function ToString() As String
        Return "X: " & X.ToString() & " Y:" & Y.ToString()
    End Function
End Structure