Imports System.IO
Public Class Precompiled_Template

    Private liste As List(Of Precompiled_Befehl)

    Private scalings As List(Of Precompiled_Scaling)

    Public Sub New()
        liste = New List(Of Precompiled_Befehl)
        scalings = New List(Of Precompiled_Scaling)
    End Sub

    Public Sub add(befehl As Precompiled_Befehl)
        Me.liste.Add(befehl)
    End Sub

    Public Sub add_Scaling(scaling As Precompiled_Scaling)
        scalings.Add(scaling)
    End Sub

    Public Sub setOrigin(o As PrecompiledSetOrigin)
        liste.Add(o)
    End Sub

    Public Function findScaling(name As String) As Integer
        For i As Integer = 0 To scalings.Count - 1
            If scalings(i).name = name Then
                Return i
            End If
        Next
        Throw New Exception("Das Scaling '" & name & "' wurde nicht definiert.")
    End Function

    Public Function hatScaling(name As String) As Boolean
        For i As Integer = 0 To scalings.Count - 1
            If scalings(i).name = name Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function getScaling(nr As Integer) As Precompiled_Scaling
        Return scalings(nr)
    End Function

    Public Sub recompile(args As AusrechnenArgs, ByRef compiled_out As Template_Compiled, NrOfParams As Integer, parentArgs As CompileParentArgs)
        If compiled_out Is Nothing Then
            compiled_out = New Template_Compiled(NrOfParams)
        Else
            compiled_out.snaps_Clear()
            compiled_out.textpos_Clear()
        End If
        compiled_out.AllParamsVisible()

        Dim originX As Integer = 0
        Dim originY As Integer = 0

        Dim CompileArgs As New CompileArgs(compiled_out.getParamsVisible(), parentArgs)
        For i As Integer = 0 To liste.Count - 1
            If TypeOf liste(i) Is Precompiled_Grafik Then
                DirectCast(liste(i), Precompiled_Grafik).compile(args, CompileArgs)
            ElseIf TypeOf liste(i) Is PrecompiledTextpos Then
                Dim tp As TextPoint = DirectCast(liste(i), PrecompiledTextpos).compile(args)
                compiled_out.add_textpos(tp)
            ElseIf TypeOf liste(i) Is Precompiled_SetVar Then
                DirectCast(liste(i), Precompiled_SetVar).compile(args)
            ElseIf TypeOf liste(i) Is PrecompiledSetOrigin Then
                originX = args.ausrechnen(DirectCast(liste(i), PrecompiledSetOrigin).originX)
                originY = args.ausrechnen(DirectCast(liste(i), PrecompiledSetOrigin).originY)
            End If
        Next

        For i As Integer = 0 To CompileArgs.callbackAfterOrigin.Count - 1
            CompileArgs.callbackAfterOrigin(i)(CompileArgs, New Point(originX, originY))
        Next

        Dim dx As Integer = -originX
        Dim dy As Integer = -originY
        Dim t As New Transform_translate(dx, dy)

        CompileArgs.TransformScalings(t)

        compiled_out.moveTextpos(dx, dy)

        CompileArgs.get_grafik.transform(t)
        compiled_out.set_grafik(CompileArgs.get_grafik)
        compiled_out.set_Scaling(CompileArgs.get_Scalings)
        compiled_out.set_benötigt_Fillstil(CompileArgs.benötigt_fillStil)
        Dim selectRect As Rectangle = CompileArgs.selection_Rect
        selectRect.X += dx
        selectRect.Y += dy
        compiled_out.set_selectionRect(selectRect)
        For i As Integer = 0 To CompileArgs.snaps.Count - 1
            Dim snap As Snappoint = CompileArgs.snaps(i)
            snap.p.X += dx
            snap.p.Y += dy
            compiled_out.add_Snap(snap)
        Next
    End Sub

    Public Function recompileScaling(CallbackNr As Integer, scale As Integer, scale_normiert As Integer, args As AusrechnenArgs, bauteil As BauteilAusDatei, paramEinstellungen() As ParamValue, parentArgs As CompileParentArgs) As Boolean
        Dim s As Precompiled_Scaling = scalings(CallbackNr)
        args.vars_intern(s.varNr_Scale) = New VariablenWertInt(scale)
        If s.varNr_ScaleNormiert <> -1 Then
            args.vars_intern(s.varNr_ScaleNormiert) = New VariablenWertInt(scale_normiert)
        End If
        Return s.recompile(args, bauteil, paramEinstellungen, parentArgs)
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(liste.Count)
        For i As Integer = 0 To liste.Count - 1
            If TypeOf liste(i) Is PrecompiledSetOrigin Then
                writer.Write(1)
                DirectCast(liste(i), PrecompiledSetOrigin).speichern(writer)
            ElseIf TypeOf liste(i) Is PrecompiledTextpos Then
                writer.Write(2)
                DirectCast(liste(i), PrecompiledTextpos).speichern(writer)
            ElseIf TypeOf liste(i) Is Precompiled_SetVar Then
                writer.Write(3)
                DirectCast(liste(i), Precompiled_SetVar).speichern(writer)
            ElseIf TypeOf liste(i) Is Precompiled_Grafik Then
                writer.Write(4)
                Precompiled_Grafik.speicherGrafik(DirectCast(liste(i), Precompiled_Grafik), writer)
            Else
                Throw New NotImplementedException("Fehler P0000: Kann diesen Typ nicht speichern!")
            End If
        Next
        writer.Write(scalings.Count)
        For i As Integer = 0 To scalings.Count - 1
            scalings(i).speichern(writer)
        Next
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Template
        Dim erg As New Precompiled_Template()
        Dim anzahl As Integer = reader.ReadInt32()
        For i As Integer = 0 To anzahl - 1
            Dim art As Integer = reader.ReadInt32()
            Select Case art
                Case 1 'PrecompiledSetOrigin
                    erg.liste.Add(PrecompiledSetOrigin.laden(reader, version))
                Case 2 'PrecompiledTextpos
                    erg.liste.Add(PrecompiledTextpos.laden(reader, version))
                Case 3 'Precompiled_SetVar
                    erg.liste.Add(Precompiled_SetVar.laden(reader, version))
                Case 4 'Precompiled_Grafik
                    erg.liste.Add(Precompiled_Grafik.ladeGrafik(Nothing, reader, version))
                Case Else
                    Throw New Exception("Fehler L0000: Kann diesen Typ nicht laden!")
            End Select
        Next
        If version >= 22 Then
            'Scalings laden
            anzahl = reader.ReadInt32()
            For i As Integer = 0 To anzahl - 1
                erg.scalings.Add(Precompiled_Scaling.laden(reader, version))
            Next
        End If
        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter)
        For i As Integer = 0 To liste.Count - 1
            If TypeOf liste(i) Is PrecompiledSetOrigin Then
                writer.lastSetOrigin = DirectCast(liste(i), PrecompiledSetOrigin)
            ElseIf TypeOf liste(i) Is PrecompiledTextpos Then
                DirectCast(liste(i), PrecompiledTextpos).export(writer)
            ElseIf TypeOf liste(i) Is Precompiled_SetVar Then
                DirectCast(liste(i), Precompiled_SetVar).export(writer)
            ElseIf TypeOf liste(i) Is Precompiled_Grafik Then
                writer.WriteLine("Drawing:")
                writer.increase_Indend()
                Precompiled_Grafik.exportiereGrafik(DirectCast(liste(i), Precompiled_Grafik), writer)
                writer.decrease_Indend()
                writer.WriteLine("Drawing End")
            Else
                Throw New NotImplementedException("Fehler E0000: Kann diesen Typ nicht exportieren!")
            End If
        Next
    End Sub

    Public Sub exportScalings(writer As Export_StreamWriter)
        For i As Integer = 0 To scalings.Count - 1
            scalings(i).export(writer)
        Next
    End Sub
End Class

Public MustInherit Class Precompiled_Befehl

End Class