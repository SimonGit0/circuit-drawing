Imports System.IO
Public Class Ausdruck_GleichBool
    Inherits Ausdruck_Boolean

    Private links As Ausdruck_Boolean
    Private rechts As Ausdruck_Boolean
    Private ungleich As Boolean

    Public Sub New(links As Ausdruck_Boolean, rechts As Ausdruck_Boolean, ungleich As Boolean)
        Me.ungleich = ungleich
        Me.links = links
        Me.rechts = rechts
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        links = links.AusrechnenSoweitMöglich()
        rechts = rechts.AusrechnenSoweitMöglich()
        If TypeOf links Is Ausdruck_Boolean_Konstante AndAlso TypeOf rechts Is Ausdruck_Boolean_Konstante Then
            If links.Ausrechnen(Nothing) = rechts.Ausrechnen(Nothing) Then
                Return New Ausdruck_Boolean_Konstante(Not ungleich)
            Else
                Return New Ausdruck_Boolean_Konstante(ungleich)
            End If
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        Dim left As Boolean = links.Ausrechnen(args)
        Dim right As Boolean = rechts.Ausrechnen(args)
        If left = right Then
            Return Not ungleich
        Else
            Return ungleich
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(ungleich)
        speichern(links, writer)
        speichern(rechts, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_GleichBool
        Dim ungleich As Boolean = reader.ReadBoolean()
        Dim links As Ausdruck_Boolean = laden(reader, version)
        Dim rechts As Ausdruck_Boolean = laden(reader, version)
        Return New Ausdruck_GleichBool(links, rechts, ungleich)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim l As Export_AusdruckBoolean = export(links, writer)
        Dim r As Export_AusdruckBoolean = export(rechts, writer)

        Dim z As String
        If ungleich Then
            z = " != "
        Else
            z = " = "
        End If

        Dim str As String
        If l.art = Export_AusdruckBoolean.Ausdruck_Art.Oder OrElse l.art = Export_AusdruckBoolean.Ausdruck_Art.Und Then
            str = "(" & l.str & ")"
        Else
            str = l.str
        End If
        str &= z
        If r.art = Export_AusdruckBoolean.Ausdruck_Art.Oder OrElse r.art = Export_AusdruckBoolean.Ausdruck_Art.Und Then
            str &= "(" & r.str & ")"
        Else
            str &= r.str
        End If
        Return New Export_AusdruckBoolean(str, Export_AusdruckBoolean.Ausdruck_Art.GleichUngleichKleinerGrößer)
    End Function
End Class
