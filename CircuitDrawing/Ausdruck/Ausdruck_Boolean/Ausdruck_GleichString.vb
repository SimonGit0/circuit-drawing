Imports System.IO
Public Class Ausdruck_GleichString
    Inherits Ausdruck_Boolean

    Private links As AusdruckString
    Private rechts As AusdruckString
    Private ungleich As Boolean

    Public Sub New(links As AusdruckString, rechts As AusdruckString, ungleich As Boolean)
        Me.ungleich = ungleich
        Me.links = links
        Me.rechts = rechts
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        links = links.AusrechnenSoweitMöglich()
        rechts = rechts.AusrechnenSoweitMöglich()
        If TypeOf links Is AusdruckString_Konstante AndAlso TypeOf rechts Is AusdruckString_Konstante Then
            If links.Ausrechnen(Nothing) = rechts.Ausrechnen(Nothing) Then
                Return New Ausdruck_Boolean_Konstante(Not ungleich)
            Else
                Return New Ausdruck_Boolean_Konstante(ungleich)
            End If
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        Dim left As String = links.Ausrechnen(args)
        Dim right As String = rechts.Ausrechnen(args)
        If left = right Then
            Return Not ungleich
        Else
            Return ungleich
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(ungleich)
        AusdruckString.speichern(links, writer)
        AusdruckString.speichern(rechts, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_GleichString
        Dim ungleich As Boolean = reader.ReadBoolean()
        Dim links As AusdruckString = AusdruckString.laden(reader, version)
        Dim rechts As AusdruckString = AusdruckString.laden(reader, version)
        Return New Ausdruck_GleichString(links, rechts, ungleich)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim l As Export_AusdruckString = AusdruckString.export(links, writer)
        Dim r As Export_AusdruckString = AusdruckString.export(rechts, writer)

        Dim z As String
        If ungleich Then
            z = " != "
        Else
            z = " = "
        End If

        Return New Export_AusdruckBoolean(l.str & z & r.str, Export_AusdruckBoolean.Ausdruck_Art.GleichUngleichKleinerGrößer)
    End Function
End Class
