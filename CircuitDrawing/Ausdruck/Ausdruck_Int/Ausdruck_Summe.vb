Imports System.IO
Public Class Ausdruck_Summe
    Inherits Ausdruck_Int

    Private summanden As List(Of Tuple(Of Boolean, Ausdruck_Int))

    Public Sub New(s As List(Of Tuple(Of Boolean, Ausdruck)))
        Me.summanden = New List(Of Tuple(Of Boolean, Ausdruck_Int))(s.Count)
        For i As Integer = 0 To s.Count - 1
            If TypeOf s(i).Item2 Is Ausdruck_Int Then
                summanden.Add(New Tuple(Of Boolean, Ausdruck_Int)(s(i).Item1, DirectCast(s(i).Item2, Ausdruck_Int)))
            Else
                Throw New Exception("Eine Summe ist nur für Integer definiert.")
            End If
        Next
    End Sub

    Public Sub New(summanden As List(Of Tuple(Of Boolean, Ausdruck_Int)))
        Me.summanden = summanden
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        'erstmal alle subterme vereinfachen
        For i As Integer = 0 To summanden.Count - 1
            summanden(i) = New Tuple(Of Boolean, Ausdruck_Int)(summanden(i).Item1, summanden(i).Item2.AusrechnenSoweitMöglich())
        Next
        Dim summe As Long = 0
        Dim kann_komplett_ausrechnen As Boolean = True
        For i As Integer = 0 To summanden.Count - 1
            If TypeOf summanden(i).Item2 Is Ausdruck_Konstante Then
                If summanden(i).Item1 Then
                    summe -= summanden(i).Item2.Ausrechnen(Nothing)
                Else
                    summe += summanden(i).Item2.Ausrechnen(Nothing)
                End If
            Else
                kann_komplett_ausrechnen = False
                If i > 1 Then
                    summanden.RemoveRange(0, i)
                    summanden.Insert(0, New Tuple(Of Boolean, Ausdruck_Int)(False, New Ausdruck_Konstante(summe)))
                End If
                Exit For
            End If
        Next
        If kann_komplett_ausrechnen Then
            Return New Ausdruck_Konstante(summe)
        Else
            'kann +0, -0 löschen!
            For i As Integer = summanden.Count - 1 To 0 Step -1
                If TypeOf summanden(i).Item2 Is Ausdruck_Konstante Then
                    If summanden(i).Item2.Ausrechnen(Nothing) = 0 Then
                        summanden.RemoveAt(i)
                    End If
                End If
            Next
            If summanden.Count >= 2 Then
                Return Me
            ElseIf summanden.Count = 1 Then
                If summanden(0).Item1 Then
                    'Minus muss als summand bleiben -x
                    Return Me
                Else
                    Return summanden(0).Item2
                End If
            ElseIf summanden.Count = 0 Then
                Return New Ausdruck_Konstante(0)
            Else
                Throw New NotImplementedException("Dies sollte nicht vorkommen dürfen")
            End If
        End If
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim summe As Long = 0
        For i As Integer = 0 To summanden.Count - 1
            If summanden(i).Item1 Then
                summe -= summanden(i).Item2.Ausrechnen(args)
            Else
                summe += summanden(i).Item2.Ausrechnen(args)
            End If
        Next
        Return summe
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(summanden.Count)
        For i As Integer = 0 To summanden.Count - 1
            writer.Write(summanden(i).Item1)
            speichern(summanden(i).Item2, writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Summe
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Negative Anzahl von Summanden nicht möglich")
        Dim summanden As New List(Of Tuple(Of Boolean, Ausdruck_Int))(anzahl)
        For i As Integer = 0 To anzahl - 1
            Dim vz As Boolean = reader.ReadBoolean()
            Dim aus As Ausdruck_Int = laden(reader, version)
            summanden.Add(New Tuple(Of Boolean, Ausdruck_Int)(vz, aus))
        Next
        Return New Ausdruck_Summe(summanden)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Dim erg As String = ""
        Dim minus As Boolean
        If summanden(0).Item1 Then
            minus = True
            erg &= "-"
        Else
            minus = False
        End If
        Dim sum As Export_AusdruckInt = export(summanden(0).Item2, writer)
        If minus AndAlso sum.art = Export_AusdruckInt.Ausdruck_Art.Summand Then
            erg &= "(" & sum.str & ")"
        Else
            erg &= sum.str
        End If
        For i As Integer = 1 To summanden.Count - 1
            minus = summanden(i).Item1
            If minus Then
                erg &= " - "
            Else
                erg &= " + "
            End If
            sum = export(summanden(i).Item2, writer)
            If minus AndAlso sum.art = Export_AusdruckInt.Ausdruck_Art.Summand Then
                erg &= "(" & sum.str & ")"
            Else
                erg &= sum.str
            End If
        Next
        Return New Export_AusdruckInt(erg, Export_AusdruckInt.Ausdruck_Art.Summand)
    End Function

End Class
