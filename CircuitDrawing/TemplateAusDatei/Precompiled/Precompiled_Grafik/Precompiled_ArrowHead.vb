Imports System.IO
Public Class Precompiled_ArrowHead
    Inherits Precompiled_Grafik

    Private x, y, align, vx, vy As Ausdruck_Int
    Private pfeil As Ausdruck

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, pfeil As Ausdruck_Pfeil, align As Ausdruck_Int, vx As Ausdruck_Int, vy As Ausdruck_Int)
        Me.x = x
        Me.y = y
        Me.pfeil = pfeil
        Me.align = align
        Me.vx = vx
        Me.vy = vy
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, pfeil As Ausdruck_Int, align As Ausdruck_Int, vx As Ausdruck_Int, vy As Ausdruck_Int)
        Me.x = x
        Me.y = y
        Me.pfeil = pfeil
        Me.align = align
        Me.vx = vx
        Me.vy = vy
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim x As Integer = args.ausrechnen(Me.x)
        Dim y As Integer = args.ausrechnen(Me.y)
        Dim pfeilIndex As ParamArrow
        If TypeOf pfeil Is Ausdruck_Pfeil Then
            pfeilIndex = args.ausrechnen(DirectCast(Me.pfeil, Ausdruck_Pfeil))
        ElseIf TypeOf pfeil Is Ausdruck_Int Then
            pfeilIndex = New ParamArrow(CShort(args.ausrechnen(DirectCast(Me.pfeil, Ausdruck_Int))), 100)
        Else
            Throw New Exception("Falsche Parameterart bei ArrowHead!")
        End If
        If pfeilIndex.pfeilArt < 0 OrElse pfeilIndex.pfeilArt >= Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile() Then
            Exit Sub
        End If
        Dim align As Pfeilspitze.AlignPfeil
        Dim align_int As Integer = args.ausrechnen(Me.align)
        If align_int = 0 Then
            align = Pfeilspitze.AlignPfeil.Align_An_Spitze
        Else
            align = Pfeilspitze.AlignPfeil.Align_An_Mitte
        End If
        Dim vx As Integer = args.ausrechnen(Me.vx)
        Dim vy As Integer = args.ausrechnen(Me.vy)
        Dim g As DO_Polygon = Pfeil_Verwaltung.getVerwaltung().getPfeil(pfeilIndex).getGrafik_Basic(align)
        g.SetMatrix(New Point(vx, vy))
        g.transform(New Transform_translate(New Point(x, y)))
        erg.addGrafik(g)
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Int.speichern(x, writer)
        Ausdruck_Int.speichern(y, writer)
        Ausdruck_Int.speichern(align, writer)
        Ausdruck_Int.speichern(vx, writer)
        Ausdruck_Int.speichern(vy, writer)
        If TypeOf pfeil Is Ausdruck_Int Then
            writer.Write(1)
            Ausdruck_Int.speichern(DirectCast(pfeil, Ausdruck_Int), writer)
        ElseIf TypeOf pfeil Is Ausdruck_Pfeil Then
            writer.Write(2)
            Ausdruck_Pfeil.speichern(DirectCast(pfeil, Ausdruck_Pfeil), writer)
        Else
            Throw New NotImplementedException("Ungültiger Ausdruck für den Pfeil")
        End If
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_ArrowHead
        Dim x As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim y As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim align As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vx As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim vy As Ausdruck_Int = Ausdruck_Int.laden(reader, version)

        Dim artPfeil As Integer = reader.ReadInt32()
        If artPfeil = 1 Then
            Dim pfeil As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
            Return New Precompiled_ArrowHead(x, y, pfeil, align, vx, vy)
        ElseIf artPfeil = 2 Then
            Dim pfeil As Ausdruck_Pfeil = Ausdruck_Pfeil.laden(reader, version)
            Return New Precompiled_ArrowHead(x, y, pfeil, align, vx, vy)
        Else
            Throw New NotImplementedException("Ungültiger Ausdruck für den Pfeil")
        End If
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "arrow("
        line &= Ausdruck_Int.export(x, writer).str & ", "
        line &= Ausdruck_Int.export(y, writer).str & ", "
        If TypeOf pfeil Is Ausdruck_Int Then
            line &= Ausdruck_Int.export(DirectCast(pfeil, Ausdruck_Int), writer).str & ", "
        ElseIf TypeOf pfeil Is Ausdruck_Pfeil Then
            line &= Ausdruck_Pfeil.export(DirectCast(pfeil, Ausdruck_Pfeil), writer).str & ", "
        Else
            Throw New NotImplementedException("Ungültiger Ausdruck für den Pfeil")
        End If
        line &= Ausdruck_Int.export(align, writer).str & ", "
        line &= Ausdruck_Int.export(vx, writer).str & ", "
        line &= Ausdruck_Int.export(vy, writer).str & ", "
        writer.WriteLine(line)
    End Sub
End Class
