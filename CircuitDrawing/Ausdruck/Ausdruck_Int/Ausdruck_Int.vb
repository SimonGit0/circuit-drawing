Imports System.IO
Public MustInherit Class Ausdruck_Int
    Inherits Ausdruck

    Public MustOverride Function AusrechnenSoweitMöglich() As Ausdruck_Int

    Public MustOverride Function Ausrechnen(args As AusrechnenArgs) As Long

    Public Overrides Function vereinfacheSoweitMöglich() As Ausdruck
        Return AusrechnenSoweitMöglich()
    End Function
#Region "Speichern, Laden, Exportieren"
    Public Shared Sub speichern(ausdruck As Ausdruck_Int, writer As binaryWriter)
        If TypeOf ausdruck Is Ausdruck_Konstante Then
            writer.Write(1)
            DirectCast(ausdruck, Ausdruck_Konstante).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_MaxMin Then
            writer.Write(2)
            DirectCast(ausdruck, Ausdruck_MaxMin).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Produkt Then
            writer.Write(3)
            DirectCast(ausdruck, Ausdruck_Produkt).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_SqrtAbs Then
            writer.Write(4)
            DirectCast(ausdruck, Ausdruck_SqrtAbs).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_StringLength Then
            writer.Write(5)
            DirectCast(ausdruck, Ausdruck_StringLength).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Summe Then
            writer.Write(6)
            DirectCast(ausdruck, Ausdruck_Summe).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Trigonometry Then
            writer.Write(7)
            DirectCast(ausdruck, Ausdruck_Trigonometry).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Variable Then
            writer.Write(8)
            DirectCast(ausdruck, Ausdruck_Variable).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_findBezierX Then
            writer.Write(9)
            DirectCast(ausdruck, Ausdruck_findBezierX).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ToInt Then
            writer.Write(10)
            DirectCast(ausdruck, Ausdruck_ToInt).save(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ToIntHex Then
            writer.Write(11)
            DirectCast(ausdruck, Ausdruck_ToIntHex).save(writer)
        Else
            Throw New NotImplementedException("Fehler P0001: Kann diesen Ausdruck nicht speichern")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Ausdruck_Int
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1 'Ausdruck_Konstante
                Return Ausdruck_Konstante.load(reader, version)
            Case 2 'Ausdruck_MaxMin
                Return Ausdruck_MaxMin.load(reader, version)
            Case 3 'Ausdruck_Produkt
                Return Ausdruck_Produkt.load(reader, version)
            Case 4 'Ausdruck_SqrtAbs
                Return Ausdruck_SqrtAbs.load(reader, version)
            Case 5 'Ausdruck_StringLength
                Return Ausdruck_StringLength.load(reader, version)
            Case 6 'Ausdruck_Summe
                Return Ausdruck_Summe.load(reader, version)
            Case 7 'Ausdruck_Trigonometry
                Return Ausdruck_Trigonometry.load(reader, version)
            Case 8 'Ausdruck_Variable
                Return Ausdruck_Variable.load(reader, version)
            Case 9 'Ausdruck_BezierFindX
                Return Ausdruck_findBezierX.load(reader, version)
            Case 10 'Ausdruck_toInt
                Return Ausdruck_ToInt.load(reader, version)
            Case 11 'Ausdruck_ToIntHex
                Return Ausdruck_ToIntHex.load(reader, version)
            Case Else
                Throw New NotImplementedException("Fehler L0001: Kann diesen Ausdruck nicht laden")
        End Select
    End Function

    Public Shared Sub speichern_kannNothingSein(ausdruck As Ausdruck_Int, writer As BinaryWriter)
        If ausdruck Is Nothing Then
            writer.Write(0)
        Else
            writer.Write(1)
            speichern(ausdruck, writer)
        End If
    End Sub

    Public Shared Function laden_kannNothingSein(reader As BinaryReader, version As Integer) As Ausdruck_Int
        Dim art As Integer = reader.ReadInt32()
        If art = 0 Then Return Nothing
        If art = 1 Then Return laden(reader, version)
        Throw New Exception("L0011: Kann diesen Ausdruck nicht laden")
    End Function

    Public Shared Function export(ausdruck As Ausdruck_Int, writer As Export_StreamWriter) As Export_AusdruckInt
        If TypeOf ausdruck Is Ausdruck_Konstante Then
            Return DirectCast(ausdruck, Ausdruck_Konstante).exportiere()
        ElseIf TypeOf ausdruck Is Ausdruck_MaxMin Then
            Return DirectCast(ausdruck, Ausdruck_MaxMin).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Produkt Then
            Return DirectCast(ausdruck, Ausdruck_Produkt).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_SqrtAbs Then
            Return DirectCast(ausdruck, Ausdruck_SqrtAbs).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_StringLength Then
            Return DirectCast(ausdruck, Ausdruck_StringLength).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Summe Then
            Return DirectCast(ausdruck, Ausdruck_Summe).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Trigonometry Then
            Return DirectCast(ausdruck, Ausdruck_Trigonometry).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Variable Then
            Return DirectCast(ausdruck, Ausdruck_Variable).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_findBezierX Then
            Return DirectCast(ausdruck, Ausdruck_findBezierX).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ToInt Then
            Return DirectCast(ausdruck, Ausdruck_ToInt).exportiere(writer)
        ElseIf TypeOf ausdruck Is Ausdruck_ToIntHex Then
            Return DirectCast(ausdruck, Ausdruck_ToIntHex).exportiere(writer)
        Else
            Throw New NotImplementedException("Export für diesen Ausdruck nicht implementiert")
        End If
    End Function
#End Region
End Class

Public Class AusrechnenArgs
    Public params As List(Of ParamValue)
    Public vars_intern As List(Of VariablenWert)

    Public Sub New(vars As List(Of ParamValue), vars_intern As List(Of VariablenWert))
        Me.params = vars
        Me.vars_intern = vars_intern
    End Sub

    Public Function ausrechnen(a As Ausdruck_Int) As Integer
        Dim erg As Long = a.Ausrechnen(Me)
        If erg > Integer.MaxValue Then Return Integer.MaxValue
        If erg < Integer.MinValue Then Return Integer.MinValue
        Return CInt(erg)
    End Function

    Public Function ausrechnen(a As Ausdruck_Boolean) As Boolean
        Return a.Ausrechnen(Me)
    End Function

    Public Function ausrechnen(a As AusdruckString) As String
        Return a.Ausrechnen(Me)
    End Function

    Public Function ausrechnen(a As Ausdruck_Pfeil) As ParamArrow
        Return a.Ausrechnen(Me)
    End Function
End Class

Public Class Export_AusdruckInt
    Public str As String
    Public art As Ausdruck_Art

    Public Sub New(str As String, art As Ausdruck_Art)
        Me.str = str
        Me.art = art
    End Sub

    Public Enum Ausdruck_Art
        Summand 'Z.b. a+b
        Produkt 'Z.b. a*b
        Exponent 'Z.b. e^(x)
        Atom 'Z.b. "4"; "x"; "(a+b)"; "abs(x)" 
    End Enum
End Class
