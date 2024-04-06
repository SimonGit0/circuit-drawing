Imports System.IO
Public Class Precompiled_Text
    Inherits Precompiled_Grafik

    Private txt As AusdruckString
    Private font_index As Integer
    Private px, py, vx, vy As Ausdruck_Int
    'Private textRot As DO_Text.TextRotation
    Private myTextRot_int As Ausdruck_Int

    Public Sub New(txt As AusdruckString, font_index As Integer, px As Ausdruck_Int, py As Ausdruck_Int, vx As Ausdruck_Int, vy As Ausdruck_Int, TextRot_int As Ausdruck_Int)
        Me.txt = txt
        Me.font_index = font_index
        Me.px = px
        Me.py = py
        Me.vx = vx
        Me.vy = vy
        Me.myTextRot_int = TextRot_int
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim txt As String = args.ausrechnen(Me.txt)

        Dim textRot As DO_Text.TextRotation
        Dim richtung As Integer = args.ausrechnen(myTextRot_int)
        richtung = richtung Mod 360
        If richtung < 0 Then richtung += 360
        If richtung <= 45 Then
            textRot = DO_Text.TextRotation.Normal
        ElseIf richtung <= 135 Then
            textRot = DO_Text.TextRotation.Rot90
        ElseIf richtung <= 225 Then
            textRot = DO_Text.TextRotation.Rot180
        ElseIf richtung <= 315 Then
            textRot = DO_Text.TextRotation.Rot270
        Else
            textRot = DO_Text.TextRotation.Normal
        End If

        erg.addGrafik(New DO_Text(txt, font_index, New Point(args.ausrechnen(px), args.ausrechnen(py)), New Point(args.ausrechnen(vx), args.ausrechnen(vy)), textRot, False))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(font_index)
        Ausdruck_Int.speichern(px, writer)
        Ausdruck_Int.speichern(py, writer)
        Ausdruck_Int.speichern(vx, writer)
        Ausdruck_Int.speichern(vy, writer)
        Ausdruck_Int.speichern(myTextRot_int, writer)
        AusdruckString.speichern(txt, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Text
        Dim font_index As Integer = reader.ReadInt32()
        Dim px As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim py As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim myTextRot As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim txt As AusdruckString = AusdruckString.laden(reader, version)

        Return New Precompiled_Text(txt, font_index, px, py, vx, vy, myTextRot)
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "text("
        line &= Ausdruck_Int.export(px, writer).str & ", "
        line &= Ausdruck_Int.export(py, writer).str & ", "
        line &= Ausdruck_Int.export(vx, writer).str & ", "
        line &= Ausdruck_Int.export(vy, writer).str & ", "
        line &= AusdruckString.export(txt, writer).str
        If TypeOf myTextRot_int Is Ausdruck_Konstante AndAlso myTextRot_int.Ausrechnen(Nothing) = 0 Then
            line &= ")" '0 ist die Default Rotation und muss nicht angegeben werden!
        Else
            line &= ", " & Ausdruck_Int.export(myTextRot_int, writer).str & ")"
        End If
        writer.WriteLine(line)
    End Sub
End Class
