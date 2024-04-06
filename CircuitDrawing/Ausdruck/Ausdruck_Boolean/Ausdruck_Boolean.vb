Imports System.IO
Public MustInherit Class Ausdruck_Boolean
    Inherits Ausdruck

    Public MustOverride Function AusrechnenSoweitMöglich() As Ausdruck_Boolean

    Public MustOverride Function Ausrechnen(args As AusrechnenArgs) As Boolean

    Public Overrides Function vereinfacheSoweitMöglich() As Ausdruck
        Return AusrechnenSoweitMöglich()
    End Function
#Region "Speichern, Laden, Exportieren"
    Public Shared Sub speichern(ausdruck As Ausdruck_Boolean, writer As BinaryWriter)
        If TypeOf ausdruck Is Ausdruck_Boolean_Konstante Then
            writer.Write(1)
            DirectCast(ausdruck, Ausdruck_Boolean_Konstante).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Gleich Then
            writer.Write(2)
            DirectCast(ausdruck, Ausdruck_Gleich).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_GleichBool Then
            writer.Write(3)
            DirectCast(ausdruck, Ausdruck_GleichBool).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_GleichString Then
            writer.Write(4)
            DirectCast(ausdruck, Ausdruck_GleichString).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_KleinerGrößer Then
            writer.Write(5)
            DirectCast(ausdruck, Ausdruck_KleinerGrößer).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_KleinerGrößerGleich Then
            writer.Write(6)
            DirectCast(ausdruck, Ausdruck_KleinerGrößerGleich).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ODER Then
            writer.Write(7)
            DirectCast(ausdruck, Ausdruck_ODER).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_UND Then
            writer.Write(8)
            DirectCast(ausdruck, Ausdruck_UND).save(writer)
        ElseIf TypeOf ausdruck Is AusdruckBoolean_Variable Then
            writer.Write(9)
            DirectCast(ausdruck, AusdruckBoolean_Variable).save(writer)
        Else
            Throw New NotImplementedException("Fehler P0002: Kann diesen Ausdruck nicht speichern")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Ausdruck_Boolean
        Dim mode As Integer = reader.ReadInt32()
        Select Case mode
            Case 1 'Ausdruck_Boolean_Konstante
                Return Ausdruck_Boolean_Konstante.load(reader, version)
            Case 2 'Ausdruck_Gleich
                Return Ausdruck_Gleich.load(reader, version)
            Case 3 'Ausdruck_GleichBool
                Return Ausdruck_GleichBool.load(reader, version)
            Case 4 'Ausdruck_GleichString
                Return Ausdruck_GleichString.load(reader, version)
            Case 5 'Ausdruck_KleinerGrößer
                Return Ausdruck_KleinerGrößer.load(reader, version)
            Case 6 'Ausdruck_KleinerGrößerGleich
                Return Ausdruck_KleinerGrößerGleich.load(reader, version)
            Case 7 'Ausdruck_ODER
                Return Ausdruck_ODER.load(reader, version)
            Case 8 'Ausdruck_UND
                Return Ausdruck_UND.load(reader, version)
            Case 9 'AusdruckBoolean_Variable
                Return AusdruckBoolean_Variable.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L0002: Kann diesen Ausdruck nicht laden")
        End Select
    End Function

    Public Shared Sub speichern_kannNothingSein(ausdruck As Ausdruck_Boolean, writer As BinaryWriter)
        If ausdruck Is Nothing Then
            writer.Write(0)
        Else
            writer.Write(1)
            speichern(ausdruck, writer)
        End If
    End Sub

    Public Shared Function laden_kannNothingSein(reader As BinaryReader, version As Integer) As Ausdruck_Boolean
        Dim art As Integer = reader.ReadInt32()
        If art = 0 Then Return Nothing
        If art = 1 Then Return laden(reader, version)
        Throw New Exception("L0012: Kann diesen Ausdruck nicht laden")
    End Function

    Public Shared Function export(ausdruck As Ausdruck_Boolean, writer As Export_StreamWriter) As Export_AusdruckBoolean
        If TypeOf ausdruck Is Ausdruck_Boolean_Konstante Then
            Return DirectCast(ausdruck, Ausdruck_Boolean_Konstante).exportiere()
        ElseIf TypeOf ausdruck Is Ausdruck_Gleich Then
            Return DirectCast(ausdruck, Ausdruck_Gleich).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_GleichBool Then
            Return DirectCast(ausdruck, Ausdruck_GleichBool).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_GleichString Then
            Return DirectCast(ausdruck, Ausdruck_GleichString).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_KleinerGrößer Then
            Return DirectCast(ausdruck, Ausdruck_KleinerGrößer).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_KleinerGrößerGleich Then
            Return DirectCast(ausdruck, Ausdruck_KleinerGrößerGleich).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ODER Then
            Return DirectCast(ausdruck, Ausdruck_ODER).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_UND Then
            Return DirectCast(ausdruck, Ausdruck_UND).exportiere(writer)
        ElseIf TypeOf ausdruck Is AusdruckBoolean_Variable Then
            Return DirectCast(ausdruck, AusdruckBoolean_Variable).exportiere(writer)
        Else
            Throw New NotImplementedException("Export für diesen Ausdruck nicht implementiert")
        End If
    End Function
#End Region
End Class

Public Class Export_AusdruckBoolean
    Public str As String
    Public art As Ausdruck_Art

    Public Sub New(str As String, art As Ausdruck_Art)
        Me.str = str
        Me.art = art
    End Sub

    Public Enum Ausdruck_Art
        Oder 'Z.b. a | b
        Und 'Z.b. a & b
        GleichUngleichKleinerGrößer 'Z.b. a = b, a != b
        Atom 'Z.b. "true", "false", "a", "(a=b)" 
    End Enum
End Class
