Imports System.IO
Public Class Precompiled_Scaling

    Private befehle As List(Of Precompiled_ScalingBefehl)
    Public ReadOnly name As String
    Public ReadOnly varNr_Scale As Integer
    Public ReadOnly varNr_ScaleNormiert As Integer

    Public Sub New(name As String, varNr1 As Integer)
        Me.New(name, varNr1, -1)
    End Sub

    Public Sub New(name As String, varNr1 As Integer, varNr2 As Integer)
        befehle = New List(Of Precompiled_ScalingBefehl)
        Me.name = name
        Me.varNr_Scale = varNr1
        Me.varNr_ScaleNormiert = varNr2
    End Sub

    Public Sub add(b As Precompiled_ScalingBefehl)
        Me.befehle.Add(b)
    End Sub

    Public Function recompile(args As AusrechnenArgs, bauteil As BauteilAusDatei, paramEinstellungen() As ParamValue, parentArgs As CompileParentArgs) As Boolean
        Dim hatÄnderung As Boolean = False
        Dim compileArgs As New Precompiled_Scaling_CompileArgs(paramEinstellungen)
        For i As Integer = 0 To befehle.Count - 1
            If befehle(i).Compile(args, compileArgs) Then
                hatÄnderung = True
            End If
        Next

        If compileArgs.dx <> 0 OrElse compileArgs.dy <> 0 Then
            bauteil.getDrehmatrix()
            Dim vec As New Point(compileArgs.dx, compileArgs.dy)
            vec = bauteil.getDrehmatrix().transformPoint(vec)
            bauteil.position = New Point(bauteil.position.X + vec.X, bauteil.position.Y + vec.Y)
            hatÄnderung = True
        End If

        Return hatÄnderung
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(name)
        writer.Write(varNr_Scale)
        writer.Write(varNr_ScaleNormiert)
        writer.Write(befehle.Count)
        For i As Integer = 0 To befehle.Count - 1
            If TypeOf befehle(i) Is Precompiled_Scaling_SetParamInt Then
                writer.Write(1)
                befehle(i).speichern(writer)
            ElseIf TypeOf befehle(i) Is precompiled_Scaling_Move Then
                writer.Write(2)
                befehle(i).speichern(writer)
            Else
                Throw New NotImplementedException("Fehler P0001: Kann diesen Skalierungsbefehl nicht speichern")
            End If
        Next
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Scaling
        Dim name As String = reader.ReadString()
        Dim varNr1 As Integer = reader.ReadInt32()
        Dim varNr2 As Integer = -1

        If version >= 23 Then
            varNr2 = reader.ReadInt32()
        End If

        Dim anzahl As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_Scaling(name, varNr1, varNr2)
        For i As Integer = 0 To anzahl - 1
            Dim art As Integer = reader.ReadInt32()
            If art = 1 Then
                erg.befehle.Add(Precompiled_Scaling_SetParamInt.laden(reader, version))
            ElseIf art = 2 Then
                erg.befehle.Add(precompiled_Scaling_Move.laden(reader, version))
            Else
                Throw New NotImplementedException("Fehler P0001: Kann diesen Skalierungsbefehl nicht laden")
            End If
        Next
        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter)
        If varNr_ScaleNormiert = -1 Then
            writer.WriteLine("scaling " & name & "(v" & varNr_Scale & "):")
        Else
            writer.WriteLine("scaling " & name & "(v" & varNr_Scale & ", v" & varNr_ScaleNormiert & "):")
        End If
        For i As Integer = 0 To befehle.Count - 1
            befehle(i).export(writer)
        Next
        writer.WriteLine("scaling end")
    End Sub
End Class

Public Class Precompiled_Scaling_CompileArgs
    Public paramEinstellungen() As ParamValue

    Public dx As Integer
    Public dy As Integer

    Public Sub New(paramEinstellungen() As ParamValue)
        Me.paramEinstellungen = paramEinstellungen
        dx = 0
        dy = 0
    End Sub

    Public Sub move(dx As Integer, dy As Integer)
        Me.dx += dx
        Me.dy += dy
    End Sub

End Class