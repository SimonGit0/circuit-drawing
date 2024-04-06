Imports System.IO
Public Class Ausdruck_UND
    Inherits Ausdruck_Boolean

    Private liste As List(Of Ausdruck_Boolean)

    Public Sub New(liste As List(Of Ausdruck_Boolean))
        Me.liste = liste
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean

        For i As Integer = liste.Count - 1 To 0 Step -1
            liste(i) = liste(i).AusrechnenSoweitMöglich()
            If TypeOf liste(i) Is Ausdruck_Boolean_Konstante Then
                If Not liste(i).Ausrechnen(Nothing) Then
                    Return New Ausdruck_Boolean_Konstante(False) 'Ein UND False ist immer FALSE
                Else
                    liste.RemoveAt(i) 'ein UND True ist immer egal und kann weggelassen werden!
                End If
            End If
        Next

        If liste.Count = 0 Then
            Return New Ausdruck_Boolean_Konstante(True) 'es wurden alle gelöscht, also waren alle true
        End If
        If liste.Count = 1 Then
            Return liste(0)
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        For i As Integer = 0 To liste.Count - 1
            If Not liste(i).Ausrechnen(args) Then
                Return False
            End If
        Next
        Return True
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(liste.Count)
        For i As Integer = 0 To liste.Count - 1
            speichern(liste(i), writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_UND
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Operanden bei einer UND-Verknüpfung darf nicht negatív sein.")
        Dim liste As New List(Of Ausdruck_Boolean)(anzahl)
        For i As Integer = 0 To anzahl - 1
            liste.Add(laden(reader, version))
        Next
        Return New Ausdruck_UND(liste)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim str As String = ""
        For i As Integer = 0 To liste.Count - 2
            Dim e As Export_AusdruckBoolean = export(liste(i), writer)
            If e.art = Export_AusdruckBoolean.Ausdruck_Art.Oder Then
                str &= "(" & e.str & ") & "
            Else
                str &= e.str & " & "
            End If
        Next
        Dim en As Export_AusdruckBoolean = export(liste(liste.Count - 1), writer)
        If en.art = Export_AusdruckBoolean.Ausdruck_Art.Oder Then
            str &= "(" & en.str & ")"
        Else
            str &= en.str
        End If
        Return New Export_AusdruckBoolean(str, Export_AusdruckBoolean.Ausdruck_Art.Und)
    End Function

End Class
