Imports System.IO
Public Class Ausdruck_KleinerGrößer
    Inherits Ausdruck_Boolean

    Private links As Ausdruck_Int
    Private rechts As Ausdruck_Int
    Private kleiner As Boolean

    Public Sub New(links As Ausdruck_Int, rechts As Ausdruck_Int, kleiner As Boolean)
        Me.kleiner = kleiner
        Me.links = links
        Me.rechts = rechts
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        links = links.AusrechnenSoweitMöglich()
        rechts = rechts.AusrechnenSoweitMöglich()
        If TypeOf links Is Ausdruck_Konstante AndAlso TypeOf rechts Is Ausdruck_Konstante Then
            If kleiner Then
                If links.Ausrechnen(Nothing) < rechts.Ausrechnen(Nothing) Then
                    Return New Ausdruck_Boolean_Konstante(True)
                Else
                    Return New Ausdruck_Boolean_Konstante(False)
                End If
            Else
                If links.Ausrechnen(Nothing) > rechts.Ausrechnen(Nothing) Then
                    Return New Ausdruck_Boolean_Konstante(True)
                Else
                    Return New Ausdruck_Boolean_Konstante(False)
                End If
            End If
        ElseIf TypeOf links Is Ausdruck_Variable AndAlso TypeOf rechts Is Ausdruck_Variable Then
            If DirectCast(links, Ausdruck_Variable).istGleich(DirectCast(rechts, Ausdruck_Variable)) Then
                Return New Ausdruck_Boolean_Konstante(False) 'Wenn beide Seiten gleich sind (a < a, a > a), dann ist es immer false
            End If
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        Dim left As Long = links.Ausrechnen(args)
        Dim right As Long = rechts.Ausrechnen(args)
        If kleiner Then
            Return left < right
        Else
            Return left > right
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(kleiner)
        Ausdruck_Int.speichern(links, writer)
        Ausdruck_Int.speichern(rechts, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_KleinerGrößer
        Dim kleiner As Boolean = reader.ReadBoolean()
        Dim links As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim rechts As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Ausdruck_KleinerGrößer(links, rechts, kleiner)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim l As Export_AusdruckInt = Ausdruck_Int.export(links, writer)
        Dim r As Export_AusdruckInt = Ausdruck_Int.export(rechts, writer)

        Dim z As String
        If kleiner Then
            z = " < "
        Else
            z = " > "
        End If

        Return New Export_AusdruckBoolean(l.str & z & r.str, Export_AusdruckBoolean.Ausdruck_Art.GleichUngleichKleinerGrößer)
    End Function
End Class
