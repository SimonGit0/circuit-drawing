Imports System.IO
Public Class Ausdruck_Produkt
    Inherits Ausdruck_Int

    Private faktoren As List(Of Tuple(Of Boolean, Ausdruck_Int))

    Public Sub New(f As List(Of Tuple(Of Boolean, Ausdruck)))
        Me.faktoren = New List(Of Tuple(Of Boolean, Ausdruck_Int))(f.Count)
        For i As Integer = 0 To f.Count - 1
            If TypeOf f(i).Item2 Is Ausdruck_Int Then
                faktoren.Add(New Tuple(Of Boolean, Ausdruck_Int)(f(i).Item1, DirectCast(f(i).Item2, Ausdruck_Int)))
            Else
                Throw New Exception("Ein Produkt ist nur für Integer definiert.")
            End If
        Next
    End Sub

    Public Sub New(faktoren As List(Of Tuple(Of Boolean, Ausdruck_Int)))
        Me.faktoren = faktoren
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        For i As Integer = 0 To faktoren.Count - 1
            faktoren(i) = New Tuple(Of Boolean, Ausdruck_Int)(faktoren(i).Item1, faktoren(i).Item2.AusrechnenSoweitMöglich())
        Next

        Dim produkt As Long = 1
        Dim kann_komplett_ausrechnen As Boolean = True
        For i As Integer = 0 To faktoren.Count - 1
            If TypeOf faktoren(i).Item2 Is Ausdruck_Konstante Then
                If faktoren(i).Item1 Then
                    produkt \= faktoren(i).Item2.Ausrechnen(Nothing)
                Else
                    produkt *= faktoren(i).Item2.Ausrechnen(Nothing)
                End If
            Else
                kann_komplett_ausrechnen = False
                If i > 1 Then
                    faktoren.RemoveRange(0, i)
                    faktoren.Insert(0, New Tuple(Of Boolean, Ausdruck_Int)(False, New Ausdruck_Konstante(produkt)))
                End If
                Exit For
            End If
        Next
        If kann_komplett_ausrechnen Then
            Return New Ausdruck_Konstante(produkt)
        Else
            Return Me
        End If
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim produkt As Long = 1
        For i As Integer = 0 To faktoren.Count - 1
            If faktoren(i).Item1 Then
                produkt \= faktoren(i).Item2.Ausrechnen(args)
            Else
                produkt *= faktoren(i).Item2.Ausrechnen(args)
            End If
        Next
        Return produkt
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(faktoren.Count)
        For i As Integer = 0 To faktoren.Count - 1
            writer.Write(faktoren(i).Item1)
            speichern(faktoren(i).Item2, writer)
        Next
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Produkt
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then Throw New Exception("Anzahl der Faktoren ist kleiner als 0")
        Dim fak As New List(Of Tuple(Of Boolean, Ausdruck_Int))(anzahl)
        For i As Integer = 0 To anzahl - 1
            Dim vz As Boolean = reader.ReadBoolean()
            Dim ausd As Ausdruck_Int = laden(reader, version)
            fak.Add(New Tuple(Of Boolean, Ausdruck_Int)(vz, ausd))
        Next
        Return New Ausdruck_Produkt(fak)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Dim erg As String = ""
        Dim div As Boolean
        If faktoren(0).Item1 = True Then
            erg &= "1 / "
            div = True
        Else
            div = False
        End If

        Dim fak0 As Export_AusdruckInt = export(faktoren(0).Item2, writer)
        If fak0.art = Export_AusdruckInt.Ausdruck_Art.Summand OrElse (fak0.art = Export_AusdruckInt.Ausdruck_Art.Produkt AndAlso div) Then
            erg &= "(" & fak0.str & ")"
        Else
            erg &= fak0.str
        End If

        For i As Integer = 1 To faktoren.Count - 1
            Dim fak As Export_AusdruckInt = export(faktoren(i).Item2, writer)
            div = faktoren(i).Item1
            If div Then
                erg &= " / "
            Else
                erg &= " * "
            End If
            If fak.art = Export_AusdruckInt.Ausdruck_Art.Summand OrElse (fak.art = Export_AusdruckInt.Ausdruck_Art.Produkt AndAlso div) Then
                erg &= "(" & fak.str & ")"
            Else
                erg &= fak.str
            End If
        Next
        Return New Export_AusdruckInt(erg, Export_AusdruckInt.Ausdruck_Art.Produkt)
    End Function
End Class
