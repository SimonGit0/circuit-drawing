Imports System.IO
Public Class Precompiled_IF
    Inherits Precompiled_MultiGrafik

    Private ausdruckIF As Ausdruck_Boolean

    Public Sub New(VarCountAtStart As Integer, ausdruckIF As Ausdruck_Boolean)
        MyBase.New(VarCountAtStart)
        Me.ausdruckIF = ausdruckIF
    End Sub

    Public Overrides Function simplifyBlock() As Precompiled_Grafik
        If TypeOf ausdruckIF Is Ausdruck_Boolean_Konstante Then
            If ausdruckIF.Ausrechnen(Nothing) Then
                'if true
                Dim erg As Precompiled_Grafik = MyBase.simplifyBlock()
                If erg IsNot Nothing AndAlso TypeOf erg Is Precompiled_IF Then
                    With DirectCast(erg, Precompiled_IF)
                        For i As Integer = 0 To .childs.Count - 1
                            If TypeOf .childs(i) Is Precompiled_ELSE OrElse TypeOf .childs(i) Is Precompiled_ELSE_IF Then
                                .childs.RemoveAt(i)
                            End If
                        Next
                        If .childs.Count = 0 Then
                            Return Nothing
                        End If
                    End With
                End If
                Return erg
            Else
                'if false
                Dim erg As Precompiled_Grafik = MyBase.simplifyBlock()
                If erg IsNot Nothing AndAlso TypeOf erg Is Precompiled_IF Then
                    With DirectCast(erg, Precompiled_IF)
                        'Zuerst einmal alles bis zum ersten else oder elseif löschen!
                        While .childs.Count > 0
                            If TypeOf .childs(0) Is Precompiled_ELSE OrElse TypeOf .childs(0) Is Precompiled_ELSE_IF Then
                                Exit While
                            End If
                            .childs.RemoveAt(0)
                        End While
                        If .childs.Count = 0 Then
                            Return Nothing
                        End If
                        'Das erste elseif was immer true ist wird auch das ende sein!
                        For i As Integer = 0 To .childs.Count - 2 'Minus 2, da es kein Sinn hat das letze Element zu prüfen (danach kommt nichts mehr!)
                            If TypeOf .childs(i) Is Precompiled_ELSE_IF AndAlso DirectCast(.childs(i), Precompiled_ELSE_IF).isAlwaysTrue() Then
                                .childs.RemoveRange(i + 1, .childs.Count - (i + 1))
                                Exit For
                            End If
                        Next
                    End With
                End If
                Return erg
            End If
        Else
            Return MyBase.simplifyBlock()
        End If
    End Function

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        If args.ausrechnen(ausdruckIF) Then
            For i As Integer = 0 To Me.childs.Count - 1
                If TypeOf Me.childs(i) Is Precompiled_ELSE OrElse TypeOf Me.childs(i) Is Precompiled_ELSE_IF Then
                    Exit Sub 'else wird nicht gemacht!
                Else
                    Me.childs(i).compile(args, erg)
                End If
            Next
        Else
            For i As Integer = 0 To Me.childs.Count - 1
                If TypeOf Me.childs(i) Is Precompiled_ELSE Then
                    Me.childs(i).compile(args, erg)
                    'Else
                    'Normales if wird nicht gemacht, da die Bedingung false ist!
                ElseIf TypeOf Me.childs(i) Is Precompiled_ELSE_IF Then
                    If DirectCast(Me.childs(i), Precompiled_ELSE_IF).compile_elseif(args, erg) Then
                        Exit Sub 'wenn elseif Bedingung erfüllt ist, dann muss der rest nicht mehr gemacht werden!
                    End If
                End If
            Next
        End If
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Boolean.speichern(ausdruckIF, writer)

        writer.Write(ist_Abgeschlossen)
        writer.Write(VarCountAtStart)
        writer.Write(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            speicherGrafik(childs(i), writer)
        Next
    End Sub

    Public Shared Function laden(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_IF
        Dim ausdruckIF As Ausdruck_Boolean = Ausdruck_Boolean.laden(reader, version)

        Dim ist_Abgeschlossen As Boolean = reader.ReadBoolean()
        Dim VarCountAtStart As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_IF(VarCountAtStart, ausdruckIF)
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
        Dim line As String = "if "
        line &= Ausdruck_Boolean.export(ausdruckIF, writer).str & ":"
        writer.WriteLine(line)

        writer.increase_Indend()
        exportMULTI(writer)
        writer.decrease_Indend()

        writer.WriteLine("end")
    End Sub
End Class
