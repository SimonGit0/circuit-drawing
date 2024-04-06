Imports System.IO
Public Class Ausdruck_findBezierX
    Inherits Ausdruck_Int

    Private x1 As Ausdruck_Int
    Private y1 As Ausdruck_Int
    Private x2 As Ausdruck_Int
    Private y2 As Ausdruck_Int
    Private x3 As Ausdruck_Int
    Private y3 As Ausdruck_Int
    Private x4 As Ausdruck_Int
    Private y4 As Ausdruck_Int

    Private y_toFind As Ausdruck_Int

    Public Sub New(y_toFind As Ausdruck_Int, x1 As Ausdruck_Int, y1 As Ausdruck_Int, x2 As Ausdruck_Int, y2 As Ausdruck_Int, x3 As Ausdruck_Int, y3 As Ausdruck_Int, x4 As Ausdruck_Int, y4 As Ausdruck_Int)
        Me.y_toFind = y_toFind
        Me.x1 = x1
        Me.x2 = x2
        Me.x3 = x3
        Me.x4 = x4
        Me.y1 = y1
        Me.y2 = y2
        Me.y3 = y3
        Me.y4 = y4
    End Sub

    Public Overrides Function AusrechnenSoweitMöglich() As Ausdruck_Int
        x1 = x1.AusrechnenSoweitMöglich()
        x2 = x2.AusrechnenSoweitMöglich()
        x3 = x3.AusrechnenSoweitMöglich()
        x4 = x4.AusrechnenSoweitMöglich()

        y1 = y1.AusrechnenSoweitMöglich()
        y2 = y2.AusrechnenSoweitMöglich()
        y3 = y3.AusrechnenSoweitMöglich()
        y4 = y4.AusrechnenSoweitMöglich()

        y_toFind = y_toFind.AusrechnenSoweitMöglich()
        If TypeOf x1 Is Ausdruck_Konstante AndAlso
           TypeOf x2 Is Ausdruck_Konstante AndAlso
           TypeOf x3 Is Ausdruck_Konstante AndAlso
           TypeOf x4 Is Ausdruck_Konstante AndAlso
           TypeOf y1 Is Ausdruck_Konstante AndAlso
           TypeOf y2 Is Ausdruck_Konstante AndAlso
           TypeOf y3 Is Ausdruck_Konstante AndAlso
           TypeOf y4 Is Ausdruck_Konstante AndAlso
           TypeOf y_toFind Is Ausdruck_Konstante Then
            Return New Ausdruck_Konstante(CInt(Me.Ausrechnen(Nothing)))
        End If
        Return Me
    End Function

    Public Overrides Function Ausrechnen(args As AusrechnenArgs) As Long
        Dim p(3) As PointD
        p(0) = New PointD(x1.Ausrechnen(args), y1.Ausrechnen(args))
        p(1) = New PointD(x2.Ausrechnen(args), y2.Ausrechnen(args))
        p(2) = New PointD(x3.Ausrechnen(args), y3.Ausrechnen(args))
        p(3) = New PointD(x4.Ausrechnen(args), y4.Ausrechnen(args))

        Dim y As Integer = CInt(y_toFind.Ausrechnen(args))

        Dim erg As List(Of Double) = Mathe.getSchnittpunkt_alpha(New Point(0, y), New Point(1, y), p(0), p(1), p(2), p(3))
        If erg Is Nothing OrElse erg.Count = 0 Then
            Dim abstandMin As Double = Double.MaxValue
            Dim abstand As Double
            Dim res As Double
            For i As Integer = 0 To 3
                abstand = Math.Abs(p(i).Y - y)
                If abstand < abstandMin Then
                    abstandMin = abstand
                    res = p(i).X
                End If
            Next
            Return CLng(Math.Round(res))
        End If
        erg.Sort()
        Dim alpha As Double = erg(0)
        Return CLng(Math.Round(alpha))
    End Function

    Public Sub save(writer As BinaryWriter)
        speichern(x1, writer)
        speichern(x2, writer)
        speichern(x3, writer)
        speichern(x4, writer)
        speichern(y1, writer)
        speichern(y2, writer)
        speichern(y3, writer)
        speichern(y4, writer)

        speichern(y_toFind, writer)
    End Sub

    Public Shared Function load(reader As BinaryReader, version As Integer) As Ausdruck_findBezierX
        Dim x1 As Ausdruck_Int = laden(reader, version)
        Dim x2 As Ausdruck_Int = laden(reader, version)
        Dim x3 As Ausdruck_Int = laden(reader, version)
        Dim x4 As Ausdruck_Int = laden(reader, version)
        Dim y1 As Ausdruck_Int = laden(reader, version)
        Dim y2 As Ausdruck_Int = laden(reader, version)
        Dim y3 As Ausdruck_Int = laden(reader, version)
        Dim y4 As Ausdruck_Int = laden(reader, version)
        Dim y_toFind As Ausdruck_Int = laden(reader, version)

        Return New Ausdruck_findBezierX(y_toFind, x1, y1, x2, y2, x3, y3, x4, y4)
    End Function

    Public Function exportiere(writer As Export_StreamWriter) As Export_AusdruckInt
        Dim erg As String
        erg = "findBezierX("
        erg &= export(y_toFind, writer).str & ", "
        erg &= export(x1, writer).str & ", " & export(y1, writer).str & ", "
        erg &= export(x2, writer).str & ", " & export(y2, writer).str & ", "
        erg &= export(x3, writer).str & ", " & export(y3, writer).str & ", "
        erg &= export(x4, writer).str & ", " & export(y4, writer).str & ")"
        Return New Export_AusdruckInt(erg, Export_AusdruckInt.Ausdruck_Art.Atom)
    End Function
End Class
