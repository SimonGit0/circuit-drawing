Imports System.IO
Public Class BauteileNamespace
    Public ReadOnly Name As String
    Private ReadOnly cells As Dictionary(Of String, BauteilCell)

    Public Sub New(name As String)
        Me.Name = name
        Me.cells = New Dictionary(Of String, BauteilCell)
    End Sub

    Public Sub add(t As TemplateAusDatei)
        Dim c As BauteilCell
        If cells.ContainsKey(t.getName()) Then
            c = cells(t.getName())
        Else
            c = New BauteilCell(t.getName(), Me)
            cells.Add(t.getName(), c)
        End If
        c.add(t)
    End Sub

    Public Function getCellCount() As Integer
        Return cells.Count()
    End Function

    Public Function getCellsNamen() As String()
        Return cells.Keys().ToArray()
    End Function

    Public Function getCell(name As String) As BauteilCell
        Return cells(name)
    End Function

    Public Function hasBauteil(_name As String, _view As String) As Boolean
        If cells.ContainsKey(_name) Then
            Return cells(_name).hasView(_view)
        Else
            Return False
        End If
    End Function

    Public Function sucheBauteil_Compatibility(_namespace As String, _name As String) As TemplateCompatibility
        For Each pair As KeyValuePair(Of String, BauteilCell) In cells
            Dim erg As TemplateCompatibility = pair.Value.sucheBauteil_Compatibility(_namespace, _name)
            If erg IsNot Nothing Then
                Return erg
            End If
        Next
        Return Nothing
    End Function

    Public Function getBauteil(_name As String, _view As String) As TemplateAusDatei
        Return cells(_name).getView(_view).template
    End Function

    Public Sub reload_default_params()
        For Each c As BauteilCell In cells.Values
            c.reload_default_params()
        Next
    End Sub

    Public Function try_translate_param_value(param As String, value As String, str_to_ID As Boolean) As KeyValuePair(Of String, String)?
        For Each c As BauteilCell In cells.Values
            Dim erg = c.try_translate_param_value(param, value, str_to_ID)
            If erg IsNot Nothing Then Return erg
        Next
        Return Nothing
    End Function

    Public Function getBauteile() As Dictionary(Of String, BauteilCell)
        Return Me.cells
    End Function

    Public Sub deleteLocalBauteil(tmpl As TemplateAusDatei)
        Dim name As String = tmpl.getName()
        If Me.cells.ContainsKey(name) Then
            Me.cells.Remove(name)
        End If
    End Sub

    Public Sub speicher(writer As BinaryWriter)
        Dim anzahl As Integer = cells.Count
        writer.Write(anzahl)
        For i As Integer = 0 To anzahl - 1
            cells(cells.Keys(i)).speichern(writer)
        Next
    End Sub

    Public Sub laden(reader As BinaryReader, version As Integer)
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then
            Throw New Exception("Anzahl an lokalen Bauteilen darf nicht negativ sein!")
        End If
        For i As Integer = 0 To anzahl - 1
            Dim c As BauteilCell = BauteilCell.laden(Me, reader, version)
            cells.Add(c.Name, c)
        Next
    End Sub
End Class
