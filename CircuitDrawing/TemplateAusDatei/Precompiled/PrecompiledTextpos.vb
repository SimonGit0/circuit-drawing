Imports System.IO
Public Class PrecompiledTextpos
    Inherits Precompiled_Befehl

    Public px, py As Ausdruck_Int
    Public vx, vy As Ausdruck_Int
    Public distx, disty As Ausdruck_Int
    Public nr As Integer
    Public hasAbstandVektor As Boolean

    Public Sub New()
        Me.px = New Ausdruck_Konstante(0)
        Me.py = New Ausdruck_Konstante(0)
        Me.vx = New Ausdruck_Konstante(0)
        Me.vy = New Ausdruck_Konstante(0)
        Me.distx = New Ausdruck_Konstante(0)
        Me.disty = New Ausdruck_Konstante(0)
        Me.nr = -1
        hasAbstandVektor = False
    End Sub

    Public Sub setPos(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.px = x
        Me.py = y
    End Sub

    Public Sub setVector(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.vx = x
        Me.vy = y
    End Sub

    Public Sub setDist(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.distx = x
        Me.disty = y
    End Sub

    Public Function compile(args As AusrechnenArgs) As TextPoint
        Dim vx, vy As Integer
        vx = args.ausrechnen(Me.vx)
        vy = args.ausrechnen(Me.vy)
        If vx > 0 Then vx = 1
        If vx < 0 Then vx = -1
        If vy > 0 Then vy = 1
        If vy < 0 Then vy = -1

        Dim distx, disty As Integer
        If Not hasAbstandVektor Then
            distx = vx
            disty = vy
        Else
            distx = args.ausrechnen(Me.distx)
            disty = args.ausrechnen(Me.disty)
            If distx > 0 Then distx = 1
            If distx < 0 Then distx = -1
            If disty > 0 Then disty = 1
            If disty < 0 Then disty = -1
        End If

        Return New TextPoint(New Point(args.ausrechnen(px), args.ausrechnen(py)), vx, vy, New Point(distx, disty))
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(nr)
        writer.Write(hasAbstandVektor)
        Ausdruck_Int.speichern(px, writer)
        Ausdruck_Int.speichern(py, writer)
        Ausdruck_Int.speichern(vx, writer)
        Ausdruck_Int.speichern(vy, writer)
        Ausdruck_Int.speichern(distx, writer)
        Ausdruck_Int.speichern(disty, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As PrecompiledTextpos
        Dim erg As New PrecompiledTextpos()
        erg.nr = reader.ReadInt32()
        erg.hasAbstandVektor = reader.ReadBoolean()
        erg.px = Ausdruck_Int.laden(reader, version)
        erg.py = Ausdruck_Int.laden(reader, version)
        erg.vx = Ausdruck_Int.laden(reader, version)
        erg.vy = Ausdruck_Int.laden(reader, version)
        erg.distx = Ausdruck_Int.laden(reader, version)
        erg.disty = Ausdruck_Int.laden(reader, version)
        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter)
        writer.WriteLine("Textpos:")
        writer.increase_Indend()
        writer.WriteLine("pos(" & Ausdruck_Int.export(px, writer).str & ", " & Ausdruck_Int.export(py, writer).str & ")")
        writer.WriteLine("vector(" & Ausdruck_Int.export(vx, writer).str & ", " & Ausdruck_Int.export(vy, writer).str & ")")
        If Not (TypeOf distx Is Ausdruck_Konstante AndAlso distx.Ausrechnen(Nothing) = 0 AndAlso
                TypeOf disty Is Ausdruck_Konstante AndAlso disty.Ausrechnen(Nothing) = 0) Then
            writer.WriteLine("dist(" & Ausdruck_Int.export(distx, writer).str & ", " & Ausdruck_Int.export(disty, writer).str & ")")
        End If
        If nr <> -1 Then
            writer.WriteLine("type(" & nr & ")")
        End If
        writer.decrease_Indend()
        writer.WriteLine("Textpos End")
    End Sub
End Class
