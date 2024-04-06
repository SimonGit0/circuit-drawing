Public Class MatheAusrechnen
    Public Shared Function ausrechnen(ByVal term As String, x As Double) As Double
        term = term.Trim()
        '+, -
        Dim str() As String = Mathe.splitString(term, "+"c, "-"c)
        If str.Length >= 2 Then
            Dim erg As Double = 0
            For i As Integer = 0 To str.Length - 1
                If str(i).Length > 1 AndAlso (str(i)(0) = "+"c OrElse str(i)(0) = "-"c) Then
                    Dim op As Double = ausrechnen(str(i).Substring(1), x)
                    If str(i)(0) = "-"c Then
                        erg -= op
                    Else
                        erg += op
                    End If
                ElseIf i = 0 Then
                    erg += ausrechnen(str(i), x)
                End If
            Next
            Return erg
        ElseIf str.Count = 1 Then
            If str(0)(0) = "+" Then
                Return ausrechnen(str(0).Substring(1), x)
            ElseIf str(0)(0) = "-" Then
                Return -ausrechnen(str(0).Substring(1), x)
            End If
        End If
        '*, /
        str = Mathe.splitString(term, "*"c, "/"c)
        If str.Length >= 2 Then
            Dim erg As Double = 1.0
            For i As Integer = 0 To str.Length - 1
                If str(i).Length > 1 AndAlso (str(i)(0) = "*"c OrElse str(i)(0) = "/"c) Then
                    Dim op As Double = ausrechnen(str(i).Substring(1), x)
                    If str(i)(0) = "/"c Then
                        erg /= op
                    Else
                        erg *= op
                    End If
                ElseIf i = 0 Then
                    erg *= ausrechnen(str(i), x)
                End If
            Next
            Return erg
        End If
        '^
        str = Mathe.splitString(term, "^"c)
        If str.Length >= 2 Then
            Dim erg As Double = ausrechnen(str(0), x)
            For i As Integer = 1 To str.Length - 1
                If str(i).Length >= 1 Then
                    Dim op As Double = ausrechnen(str(i), x)
                    erg = Math.Pow(erg, op)
                End If
            Next
            Return erg
        End If
        '()
        If term.StartsWith("(") AndAlso term.EndsWith(")") Then
            Return ausrechnen(term.Substring(1, term.Length - 2), x)
        End If

        'Funktionen
        If term.StartsWith("sin(") AndAlso term.EndsWith(")") Then
            Return Math.Sin(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("cos(") AndAlso term.EndsWith(")") Then
            Return Math.Cos(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("tan(") AndAlso term.EndsWith(")") Then
            Return Math.Tan(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("cot(") AndAlso term.EndsWith(")") Then
            Return 1 / Math.Tan(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("round(") AndAlso term.EndsWith(")") Then
            Return Math.Round(ausrechnen(term.Substring(6, term.Length - 7), x))
        End If
        If term.StartsWith("ceil(") AndAlso term.EndsWith(")") Then
            Return Math.Ceiling(ausrechnen(term.Substring(5, term.Length - 6), x))
        End If
        If term.StartsWith("floor(") AndAlso term.EndsWith(")") Then
            Return Math.Floor(ausrechnen(term.Substring(6, term.Length - 7), x))
        End If
        If term.StartsWith("exp(") AndAlso term.EndsWith(")") Then
            Return Math.Exp(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("ln(") AndAlso term.EndsWith(")") Then
            Return Math.Log(ausrechnen(term.Substring(3, term.Length - 4), x))
        End If
        If term.StartsWith("log(") AndAlso term.EndsWith(")") Then
            Return Math.Log10(ausrechnen(term.Substring(4, term.Length - 5), x))
        End If
        If term.StartsWith("sgn(") AndAlso term.EndsWith(")") Then
            Dim op As Double = ausrechnen(term.Substring(4, term.Length - 5), x)
            If op > 0 Then
                Return 1.0
            ElseIf op < 0 Then
                Return -1.0
            Else
                Return 0
            End If
        End If


        If term = "e" Then Return Math.E
        If term = "pi" Then Return Math.PI
        If term = "x" Then Return x
        Return CDbl(term)
    End Function


End Class
