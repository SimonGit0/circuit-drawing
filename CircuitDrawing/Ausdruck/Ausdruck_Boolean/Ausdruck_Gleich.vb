Imports System.IO

Public Class Ausdruck_Gleich
    Inherits Ausdruck_Boolean

    Private links As Ausdruck_Int
    Private rechts As Ausdruck_Int
    Private ungleich As Boolean

    Public Sub New(links As Ausdruck_Int, rechts As Ausdruck_Int, ungleich As Boolean)
        Me.ungleich = ungleich
        Me.links = links
        Me.rechts = rechts
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Boolean
        links = links.AusrechnenSoweitMöglich()
        rechts = rechts.AusrechnenSoweitMöglich()
        If TypeOf links Is Ausdruck_Konstante AndAlso TypeOf rechts Is Ausdruck_Konstante Then
            If links.Ausrechnen(Nothing) = rechts.Ausrechnen(Nothing) Then
                Return New Ausdruck_Boolean_Konstante(Not ungleich)
            Else
                Return New Ausdruck_Boolean_Konstante(ungleich)
            End If
        ElseIf TypeOf links Is Ausdruck_Variable AndAlso TypeOf rechts Is Ausdruck_Variable Then
            If DirectCast(links, Ausdruck_Variable).istGleich(DirectCast(rechts, Ausdruck_Variable)) Then
                Return New Ausdruck_Boolean_Konstante(Not ungleich)
            Else
                Return Me 'wenn nicht exakt gleich (z.b. a=b), dann kann man nichts vereinfachen, sondern muss warten welchen wert die variablen zur laufzeit annehmen
            End If
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Boolean
        Dim left As Long = links.Ausrechnen(args)
        Dim right As Long = rechts.Ausrechnen(args)
        If left = right Then
            Return Not ungleich
        Else
            Return ungleich
        End If
    End Function

    Public Sub save(writer As BinaryWriter)
        writer.Write(ungleich)
        Ausdruck_Int.speichern(links, writer)
        Ausdruck_Int.speichern(rechts, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_Gleich
        Dim ungleich As Boolean = reader.ReadBoolean()
        Dim links As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim rechts As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Return New Ausdruck_Gleich(links, rechts, ungleich)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckBoolean
        Dim l As Export_AusdruckInt = Ausdruck_Int.export(links, writer)
        Dim r As Export_AusdruckInt = Ausdruck_Int.export(rechts, writer)

        If TypeOf links Is Ausdruck_Variable AndAlso TypeOf rechts Is Ausdruck_Konstante Then
            If DirectCast(links, Ausdruck_Variable).isParameter_param(writer) Then
                Dim param_index As Integer = DirectCast(links, Ausdruck_Variable).getVarIndex()
                Dim rechts_ausgerechnet As Long = rechts.Ausrechnen(Nothing)
                If rechts_ausgerechnet >= 0 AndAlso rechts_ausgerechnet < DirectCast(writer.parameter(param_index), TemplateParameter_Param).options.Length Then
                    r.str = """" & DirectCast(writer.parameter(param_index), TemplateParameter_Param).options(CInt(rechts_ausgerechnet)).get_ID() & """"
                    r.art = Export_AusdruckInt.Ausdruck_Art.Atom
                Else
                    Throw New Exception("Parameteroption liegt außerhalb des gültigen Bereichs")
                End If
            End If
        ElseIf TypeOf rechts Is Ausdruck_Variable AndAlso TypeOf links Is Ausdruck_Konstante Then
            If DirectCast(rechts, Ausdruck_Variable).isParameter_param(writer) Then
                Dim param_index As Integer = DirectCast(rechts, Ausdruck_Variable).getVarIndex()
                Dim links_ausgerechnet As Long = links.Ausrechnen(Nothing)
                If links_ausgerechnet >= 0 AndAlso links_ausgerechnet < DirectCast(writer.parameter(param_index), TemplateParameter_Param).options.Length Then
                    l.str = """" & DirectCast(writer.parameter(param_index), TemplateParameter_Param).options(CInt(links_ausgerechnet)).get_ID() & """"
                    l.art = Export_AusdruckInt.Ausdruck_Art.Atom
                Else
                    Throw New Exception("Parameteroption liegt außerhalb des gültigen Bereichs")
                End If
            End If
        End If

        Dim z As String
        If ungleich Then
            z = " != "
        Else
            z = " = "
        End If

        Return New Export_AusdruckBoolean(l.str & z & r.str, Export_AusdruckBoolean.Ausdruck_Art.GleichUngleichKleinerGrößer)
    End Function
End Class
