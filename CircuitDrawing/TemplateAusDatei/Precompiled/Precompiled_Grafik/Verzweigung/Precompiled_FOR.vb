Imports System.IO

Public Class Precompiled_FOR
    Inherits Precompiled_MultiGrafik

    Private start As Ausdruck_Int
    Private ende As Ausdruck_Int
    Private start_geschlossen As Boolean
    Private ende_geschlossen As Boolean
    Private var_nr As Integer

    Public Sub New(VarCountAtStart As Integer, var_nr As Integer, start As Ausdruck_Int, ende As Ausdruck_Int, start_geschlossen As Boolean, ende_geschlossen As Boolean)
        MyBase.New(VarCountAtStart)
        Me.start = start
        Me.ende = ende
        Me.start_geschlossen = start_geschlossen
        Me.ende_geschlossen = ende_geschlossen
        Me.var_nr = var_nr
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim start As Integer = args.ausrechnen(Me.start)
        Dim ende As Integer = args.ausrechnen(Me.ende)
        If Not start_geschlossen Then
            start += 1
        End If
        If Not ende_geschlossen Then
            ende -= 1
        End If
        If ende < start Then
            Exit Sub
        ElseIf ende = start Then
            args.vars_intern(var_nr) = New VariablenWertInt(start)
            MyBase.compile(args, erg)
        Else
            For i As Integer = start To ende
                args.vars_intern(var_nr) = New VariablenWertInt(i)
                MyBase.compile(args, erg)
            Next
        End If
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(start_geschlossen)
        writer.Write(ende_geschlossen)
        writer.Write(var_nr)
        Ausdruck_Int.speichern(start, writer)
        Ausdruck_Int.speichern(ende, writer)

        writer.Write(ist_Abgeschlossen)
        writer.Write(VarCountAtStart)
        writer.Write(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            speicherGrafik(childs(i), writer)
        Next
    End Sub

    Public Shared Function laden(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_FOR
        Dim start_geschlossen As Boolean = reader.ReadBoolean()
        Dim ende_geschlossen As Boolean = reader.ReadBoolean()
        Dim var_nr As Integer = reader.ReadInt32()
        Dim start As Ausdruck_Int = Ausdruck_Int.laden(reader, version)
        Dim ende As Ausdruck_Int = Ausdruck_Int.laden(reader, version)

        Dim ist_Abgeschlossen As Boolean = reader.ReadBoolean()
        Dim VarCountAtStart As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_FOR(VarCountAtStart, var_nr, start, ende, start_geschlossen, ende_geschlossen)
        erg.ist_Abgeschlossen = ist_Abgeschlossen

        Dim anzahlChilds As Integer = reader.ReadInt32()
        If anzahlChilds < 0 Then Throw New Exception("Anzahl der Befehle darf nicht negativ sein")
        For i As Integer = 0 To anzahlChilds - 1
            erg.childs.Add(ladeGrafik(erg, reader, version))
        Next
        erg.parent = parent
        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String = "for "
        line &= "v" & Me.var_nr & " = "
        If Me.start_geschlossen Then
            line &= "["
        Else
            line &= "("
        End If
        line &= Ausdruck_Int.export(start, writer).str & ", "
        line &= Ausdruck_Int.export(ende, writer).str
        If Me.ende_geschlossen Then
            line &= "]"
        Else
            line &= ")"
        End If
        line &= ":"
        writer.WriteLine(line)

        writer.increase_Indend()
        exportMULTI(writer)
        writer.decrease_Indend()

        writer.WriteLine("end")
    End Sub
End Class
