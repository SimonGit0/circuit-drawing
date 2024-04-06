Imports System.IO

Public Class Precompiled_ELSE
    Inherits Precompiled_MultiGrafik

    Public Sub New(VarCountAtStart As Integer)
        MyBase.New(VarCountAtStart)
    End Sub

    Public Overrides Function simplifyBlock() As Precompiled_Grafik
        Return MyBase.simplifyBlock()
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(ist_Abgeschlossen)
        writer.Write(VarCountAtStart)
        writer.Write(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            speicherGrafik(childs(i), writer)
        Next
    End Sub

    Public Shared Function laden(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_ELSE
        Dim ist_Abgeschlossen As Boolean = reader.ReadBoolean()
        Dim VarCountAtStart As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_ELSE(VarCountAtStart)
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
        writer.decrease_Indend() 'das else wird wieder zurück eingerückt!
        writer.WriteLine("else")
        writer.increase_Indend()

        exportMULTI(writer)
    End Sub
End Class
