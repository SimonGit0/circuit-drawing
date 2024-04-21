Imports System.IO
Public Class Multi_Lang_String
    Private ID As String
    Private names As Dictionary(Of String, String)
    Public Sub New(ID As String, names As Dictionary(Of String, String))
        Me.ID = ID
        Me.names = names
    End Sub
    Public Sub New(einleseStr As String)
        If einleseStr.StartsWith("[") AndAlso einleseStr.EndsWith("]") Then
            einleseStr = einleseStr.Substring(1, einleseStr.Length - 2).Trim()
            Dim hat_str As Boolean = False
            Me.ID = ""
            Me.names = New Dictionary(Of String, String)
            Dim option_start As Integer = 0
            For i As Integer = 0 To einleseStr.Count
                If i < einleseStr.Length AndAlso einleseStr(i) = """"c Then
                    hat_str = Not hat_str
                ElseIf (i = einleseStr.Length OrElse einleseStr(i) = ",") AndAlso Not hat_str Then
                    Dim _option As String = einleseStr.Substring(option_start, i - option_start).Trim()
                    option_start = i + 1
                    Dim neue_option = lese_option(_option)
                    If neue_option.Key = "" Then
                        If Me.ID = "" Then
                            Me.ID = neue_option.Value
                        Else
                            Throw New Exception("ID mehrfach definiert (bei '" & einleseStr & "')")
                        End If
                    Else
                        Me.names.Add(neue_option.Key, neue_option.Value)
                    End If
                End If
            Next
            If Me.ID = "" Then
                Throw New Exception("ID nicht definiert (bei '" & einleseStr & "')!")
            End If
            If names.Count = 0 Then
                names = Nothing
            End If
        Else
            ID = einleseStr
            names = Nothing
        End If
    End Sub

    Private Function lese_option(opt As String) As KeyValuePair(Of String, String)
        Dim hat_str As Boolean = False
        Dim part1 As String = ""
        Dim part2 As String = ""
        For i As Integer = 0 To opt.Count - 1
            If opt(i) = """"c Then
                hat_str = Not hat_str
            ElseIf opt(i) = "="c And Not hat_str Then
                part1 = opt.Substring(0, i).Trim()
                part2 = opt.Substring(i + 1).Trim()
            End If
        Next
        If part1 = "" AndAlso part2 = "" Then
            If opt(0) = """"c AndAlso opt(opt.Length - 1) = """"c Then
                Return New KeyValuePair(Of String, String)("", opt.Substring(1, opt.Length - 2))
            Else
                Throw New Exception("'""' erwartet!")
            End If
        ElseIf part1 <> "" AndAlso part2 <> "" Then
            If part2.StartsWith(""""c) AndAlso part2.EndsWith(""""c) Then
                Return New KeyValuePair(Of String, String)(part1.ToLower(), part2.Substring(1, part2.Length - 2))
            Else
                Throw New Exception("'""' erwartet!")
            End If
        Else
            Throw New Exception("'lang = ""Bezeichner""' erwartet")
        End If
    End Function

    Public Function get_str() As String
        Return Me.get_str_intern(Settings.lang_str)
    End Function
    Private Function get_str_intern(lang As String) As String
        If names Is Nothing Then
            Return ID
        Else
            If names.ContainsKey(lang) Then
                Return names(lang)
            Else
                Return ID
            End If
        End If
    End Function

    Public Function get_ID() As String
        Return Me.ID
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(ID)
        If names Is Nothing Then
            writer.Write(-1)
        Else
            writer.Write(names.Count)
            For Each kvp In names
                writer.Write(kvp.Key)
                writer.Write(kvp.Value)
            Next
        End If
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As Multi_Lang_String
        If version < 33 Then
            Dim id As String = reader.ReadString()
            Return New Multi_Lang_String(id, Nothing)
        Else
            Dim id As String = reader.ReadString()
            Dim anzahl As Integer = reader.ReadInt32()
            If anzahl <= 0 Then
                Return New Multi_Lang_String(id, Nothing)
            Else
                Dim names As New Dictionary(Of String, String)(anzahl)
                For i As Integer = 0 To anzahl - 1
                    Dim k As String = reader.ReadString()
                    Dim v As String = reader.ReadString()
                    names.Add(k, v)
                Next
                Return New Multi_Lang_String(id, names)
            End If
        End If
    End Function

    Public Function exportieren() As String
        If names Is Nothing OrElse names.Count = 0 Then
            Return """" & ID & """"
        Else
            Dim erg As String = "[""" & ID & """, "
            For Each kvp In names
                erg &= kvp.Key & " = """ & kvp.Value & """, "
            Next
            Return erg.Substring(0, erg.Length - 2) & "]"
        End If
    End Function

    Public Function is_equal(str2 As Multi_Lang_String) As Boolean
        If Me.ID <> str2.ID Then Return False
        If Me.names Is Nothing And str2.names Is Nothing Then Return True
        If Me.names IsNot Nothing AndAlso str2.names IsNot Nothing Then
            If Me.names.Count <> str2.names.Count Then Return False
            For Each kvp In Me.names
                If Not str2.names.ContainsKey(kvp.Key) Then Return False
                If kvp.Value <> str2.names(kvp.Key) Then Return False
            Next
            Return True
        End If
        Return False
    End Function
End Class
