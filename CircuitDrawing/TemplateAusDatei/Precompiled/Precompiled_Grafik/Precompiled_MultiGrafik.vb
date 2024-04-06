Imports System.IO

Public Class Precompiled_MultiGrafik
    Inherits Precompiled_Grafik

    Protected childs As List(Of Precompiled_Grafik)
    Protected ist_Abgeschlossen As Boolean
    Protected VarCountAtStart As Integer
    Public parent As Precompiled_MultiGrafik

    Public Sub New(VarCountAtStart As Integer)
        Me.VarCountAtStart = VarCountAtStart

        childs = New List(Of Precompiled_Grafik)
        ist_Abgeschlossen = False
    End Sub

    Public Sub add(child As Precompiled_Grafik)
        If childs.Count > 0 AndAlso TypeOf childs(childs.Count - 1) Is Precompiled_MultiGrafik AndAlso Not DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).ist_Abgeschlossen Then
            DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).add(child)
        Else
            add_Direct(child)
        End If
    End Sub

    Public Sub add_Direct(child As Precompiled_Grafik)
        childs.Add(child)
        If TypeOf child Is Precompiled_MultiGrafik Then
            DirectCast(child, Precompiled_MultiGrafik).parent = Me
        End If
    End Sub

    Public Sub add_End()
        If childs.Count > 0 AndAlso TypeOf childs(childs.Count - 1) Is Precompiled_MultiGrafik AndAlso Not DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).ist_Abgeschlossen Then
            DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).add_End()
        Else
            If Me.ist_Abgeschlossen Then
                Throw New Exception("Zu viele 'end'")
            End If
            Me.ist_Abgeschlossen = True
        End If
    End Sub

    Public Function getCurrentSubElement() As Precompiled_MultiGrafik
        If childs.Count > 0 AndAlso TypeOf childs(childs.Count - 1) Is Precompiled_MultiGrafik AndAlso Not DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).ist_Abgeschlossen Then
            Return DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).getCurrentSubElement()
        Else
            Return Me
        End If
    End Function

    Public Function getVarCountAtStart() As Integer
        If Me.ist_Abgeschlossen Then
            Return VarCountAtStart
        ElseIf childs.Count > 0 AndAlso TypeOf childs(childs.Count - 1) Is Precompiled_MultiGrafik Then
            Return DirectCast(childs(childs.Count - 1), Precompiled_MultiGrafik).getVarCountAtStart()
        End If
        Throw New Exception("Allgemeiner Fehler Typ1 beim Beenden des Blockes.")
    End Function

    Public Function getVarCountAtStart_Direkt() As Integer
        Return Me.VarCountAtStart
    End Function

    Public Function fertig() As Boolean
        Return ist_Abgeschlossen
    End Function

    Public Overrides Function simplifyBlock() As Precompiled_Grafik
        For i As Integer = childs.Count - 1 To 0 Step -1
            childs(i) = childs(i).simplifyBlock()
            If childs(i) Is Nothing Then
                childs.RemoveAt(i)
            End If
        Next
        If childs.Count = 0 Then
            Return Nothing
        Else
            Return Me
        End If
    End Function

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        For i As Integer = 0 To childs.Count - 1
            childs(i).compile(args, erg)
        Next
    End Sub

    Public Sub speichernMULTI(writer As BinaryWriter)
        writer.Write(ist_Abgeschlossen)
        writer.Write(VarCountAtStart)
        writer.Write(childs.Count)
        For i As Integer = 0 To childs.Count - 1
            speicherGrafik(childs(i), writer)
        Next
    End Sub

    Public Shared Function ladenMULTI(parent As Precompiled_MultiGrafik, reader As BinaryReader, version As Integer) As Precompiled_MultiGrafik
        Dim ist_Abgeschlossen As Boolean = reader.ReadBoolean()
        Dim VarCountAtStart As Integer = reader.ReadInt32()
        Dim erg As New Precompiled_MultiGrafik(VarCountAtStart)
        erg.ist_Abgeschlossen = ist_Abgeschlossen

        Dim anzahlChilds As Integer = reader.ReadInt32()
        If anzahlChilds < 0 Then Throw New Exception("Anzahl der Befehle darf nicht negativ sein")
        For i As Integer = 0 To anzahlChilds - 1
            erg.childs.Add(ladeGrafik(erg, reader, version))
        Next
        erg.parent = parent
        Return erg
    End Function

    Public Sub exportMULTI(writer As Export_StreamWriter)
        For i As Integer = 0 To childs.Count - 1
            exportiereGrafik(childs(i), writer)
        Next
    End Sub
End Class
