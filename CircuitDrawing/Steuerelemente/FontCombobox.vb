Imports System.Drawing.Text
Public Class FontCombobox
    Inherits JoSiCombobox

    Private Shared fonts As New InstalledFontCollection()

    Public Sub New()
        For Each f As FontFamily In fonts.Families
            Me.Items.Add(f.Name)
        Next
    End Sub

    Public Sub selectFont(name As String)
        For i As Integer = 0 To Me.Items.Count - 1
            If CStr(Me.Items(i)) = name Then
                Me.SelectedIndex = i
                Exit Sub
            End If
        Next
        Throw New Exception("Diese Schriftart wurde nicht gefunden (" & name & ").")
    End Sub

    Public Function getSelectedFontName() As String
        Return CStr(Me.Items(Me.SelectedIndex))
    End Function

    Private Shared Function hasFont(name As String) As Boolean
        For Each f As FontFamily In fonts.Families
            If f.Name = name Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Shared Function getDefaultFont() As String
        If hasFont("Times New Roman") Then Return "Times New Roman"
        If hasFont("Calibri") Then Return "Calibri"
        If hasFont("Arial") Then Return "Arial"
        If hasFont("Verdana") Then Return "Verdana"
        Dim name1 As String = SystemFonts.DefaultFont.OriginalFontName
        If hasFont(name1) Then Return name1
        Return fonts.Families(0).Name
    End Function

    Public Shared Function getDefaultFont_OLD() As String
        'Gibt die Default-Font für alte Dateien (Version < 11; ohne Einstlellungsmöglichkeit der Font) an
        'Auch wenn oben, die getDefaultFont Reihenfolge irgendwann mal geändert wird, sollte hier bei 
        'getDefaultFont_OLD immer noch Times New Roman genommen werden (falls möglich), damit es weitestgehend Kompatibel ist zur
        'alte Version, wo IMMER Times New Roman genommen wurde!!
        If hasFont("Times New Roman") Then Return "Times New Roman"
        Return getDefaultFont()
    End Function

End Class
