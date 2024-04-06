Imports System.IO
Public Class Ausdruck_MaxMin
    Inherits Ausdruck_Int

    Private subTerm As List(Of Ausdruck_Int)
    Private max As Boolean

    Public Sub New(subTerm As List(Of Ausdruck_Int), max As Boolean)
        Me.subTerm = subTerm
        Me.max = max
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        'Alle Terme einzeln vereinfachen!
        For i As Integer = 0 To subTerm.Count - 1
            subTerm(i) = subTerm(i).AusrechnenSoweitMöglich()
        Next
        'Alle Konstenten vergleichen
        Dim ergKonst As Long
        If max Then
            ergKonst = Long.MinValue
        Else
            ergKonst = Long.MaxValue
        End If
        For i As Integer = 0 To subTerm.Count - 1
            If TypeOf subTerm(i) Is Ausdruck_Konstante Then
                If max Then
                    ergKonst = Math.Max(ergKonst, subTerm(i).Ausrechnen(Nothing))
                Else
                    ergKonst = Math.Min(ergKonst, subTerm(i).Ausrechnen(Nothing))
                End If
            End If
        Next
        'Alle nicht min./max. Konstanten löschen
        Dim hatMaxMin As Boolean = False
        For i As Integer = subTerm.Count - 1 To 0 Step -1
            If TypeOf subTerm(i) Is Ausdruck_Konstante Then
                If max Then
                    If ergKonst > subTerm(i).Ausrechnen(Nothing) OrElse (hatMaxMin AndAlso ergKonst >= subTerm(i).Ausrechnen(Nothing)) Then
                        subTerm.RemoveAt(i)
                    ElseIf ergKonst = subTerm(i).Ausrechnen(Nothing) Then
                        hatMaxMin = True
                    End If
                Else
                    If ergKonst < subTerm(i).Ausrechnen(Nothing) OrElse (hatMaxMin AndAlso ergKonst <= subTerm(i).Ausrechnen(Nothing)) Then
                        subTerm.RemoveAt(i)
                    ElseIf ergKonst = subTerm(i).Ausrechnen(Nothing) Then
                        hatMaxMin = True
                    End If
                End If
            End If
        Next
        'Wenn nur ein Wert, dann kann min/max aufgelöst werden!
        If subTerm.Count = 1 Then
            Return subTerm(0)
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim erg As Long
        If max Then
            erg = Long.MinValue
        Else
            erg = Long.MaxValue
        End If
        For i As Integer = 0 To subTerm.Count - 1
            If max Then
                erg = Math.Max(erg, subTerm(i).Ausrechnen(args))
            Else
                erg = Math.Min(erg, subTerm(i).Ausrechnen(args))
            End If
        Next
        Return erg
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(max)
        writer.Write(subTerm.Count)
        For i As Integer = 0 To subTerm.Count - 1
            speichern(subTerm(i), writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_MaxMin
        Dim max As Boolean = reader.ReadBoolean()
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Fehler P0001: Anzahl der Subterme darf nicht kleiner als 0 sein.")
        Dim subterme As New List(Of Ausdruck_Int)(anzahl)
        For i As Integer = 0 To anzahl - 1
            subterme.Add(Ausdruck_Int.laden(reader, version))
        Next
        Return New Ausdruck_MaxMin(subterme, max)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Dim erg As String
        If max Then
            erg = "max("
        Else
            erg = "min("
        End If
        For i As Integer = 0 To subTerm.Count - 2
            erg &= export(subTerm(i), writer).str & ", "
        Next
        erg &= export(subTerm(subTerm.Count - 1), writer).str & ")"
        Return New Export_AusdruckInt(erg, Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function
End Class
