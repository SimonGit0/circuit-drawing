Imports System.IO
Public Class Precompiled_ELSE_IF
    Inherits Precompiled_MultiGrafik

    Private ausdruckELSEIF As Ausdruck_Boolean

    Public Sub New(VarCountAtStart As Integer, ausdruckELSEIF As Ausdruck_Boolean)
        MyBase.New(VarCountAtStart)
        Me.ausdruckELSEIF = ausdruckELSEIF
    End Sub

    Public Overrides Function simplifyBlock() As Precompiled_Grafik
        If TypeOf ausdruckELSEIF Is Ausdruck_Boolean_Konstante Then
            If ausdruckELSEIF.Ausrechnen(Nothing) Then
                'if true
                Return MyBase.simplifyBlock()
            Else
                'if false
                Return Nothing
            End If
        Else
            Return MyBase.simplifyBlock()
        End If
    End Function

    Public Function isAlwaysTrue() As Boolean
        If TypeOf ausdruckELSEIF Is Ausdruck_Boolean_Konstante Then
            If ausdruckELSEIF.Ausrechnen(Nothing) Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function compile_elseif(args As AusrechnenArgs, erg As CompileArgs) As Boolean
        If args.ausrechnen(ausdruckELSEIF) Then
            compile(args, erg)
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub speichern(writer As BinaryWriter)
        Ausdruck_Boolean.speichern(ausdruckELSEIF, writer)

        writer.Write(ist_Abgeschlossen)
        writer.Write(VarCountAtStart)
        writer.Write(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            speicherGrafik(childs(i), writer)
        Next
    End Sub

    Public Shared Function laden(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_ELSE_IF
        Dim ausdruckELSEIF As Ausdruck_Boolean = Ausdruck_Boolean.laden(reader, version)

        Dim ist_Abgeschlossen As Boolean = reader.ReadBoolean()
        Dim VarCountAtStart As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_ELSE_IF(VarCountAtStart, ausdruckELSEIF)
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
        writer.decrease_Indend() 'das elseif wird wieder zurück eingerückt!
        Dim line As String = "elseif "
        line &= Ausdruck_Boolean.export(ausdruckELSEIF, writer).str
        writer.WriteLine(line)
        writer.increase_Indend()

        exportMULTI(writer)
    End Sub

End Class
