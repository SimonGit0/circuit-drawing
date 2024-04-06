Imports System.IO
Public Class Ausdruck_ODER
    Inherits Ausdruck_Boolean

    Private liste As List(Of Ausdruck_Boolean)

    Public Sub New(liste As List(Of Ausdruck_Boolean))
        Me.liste = liste
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean

        For i As Integer = liste.Count - 1 To 0 Step -1
            liste(i) = liste(i).AusrechnenSoweitMöglich()
            If TypeOf liste(i) Is Ausdruck_Boolean_Konstante Then
                If liste(i).Ausrechnen(Nothing) Then
                    Return New Ausdruck_Boolean_Konstante(True) 'Ein Oder True ist immer TRUE
                Else
                    liste.RemoveAt(i) 'ein ODER False ist immer egal und kann weggelassen werden!
                End If
            End If
        Next

        If liste.Count = 0 Then
            Return New Ausdruck_Boolean_Konstante(False) 'es wurden alle gelöscht, also waren alle false
        End If
        If liste.Count = 1 Then
            Return liste(0)
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        For i As Integer = 0 To liste.Count - 1
            If liste(i).Ausrechnen(args) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(liste.Count)
        For i As Integer = 0 To liste.Count - 1
            speichern(liste(i), writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_ODER
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Die Anzahl der Operanden bei einer ODER-Verknüpfung darf nicht negatív sein.")
        Dim liste As New List(Of Ausdruck_Boolean)(anzahl)
        For i As Integer = 0 To anzahl - 1
            liste.Add(laden(reader, version))
        Next
        Return New Ausdruck_ODER(liste)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim str As String = ""
        For i As Integer = 0 To liste.Count - 2
            Dim e As Export_AusdruckBoolean = export(liste(i), writer)
            str &= e.str & " | "
        Next
        str &= export(liste(liste.Count - 1), writer).str
        Return New Export_AusdruckBoolean(str, Export_AusdruckBoolean.Ausdruck_Art.Oder)
    End Function
End Class
