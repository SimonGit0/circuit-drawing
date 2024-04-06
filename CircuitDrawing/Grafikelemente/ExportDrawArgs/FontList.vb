Imports System.IO
Imports PdfSharp.Drawing
Public Class FontList
    Private liste As List(Of FontStyle)

    Public Sub New()
        Me.liste = New List(Of FontStyle)
    End Sub

    Public Sub add(l As FontStyle)
        Me.liste.Add(l)
    End Sub

    Public Function getFontStyle(index As Integer) As FontStyle
        Return liste(index)
    End Function

    Public Function getNumberOfNewFontStyle(style As FontStyle) As Integer
        'Prüfen ob Style schon vorhanden, dann diese Nummer zurückgeben!
        For i As Integer = 0 To liste.Count - 1
            If liste(i) IsNot Nothing AndAlso liste(i).isSameAs(style) Then
                Return i
            End If
        Next
        'Wenn Style noch nicht vorhanden, dann eine freie Position wählen
        For i As Integer = 0 To liste.Count - 1
            If liste(i) Is Nothing Then
                liste(i) = style.copy()
                Return i
            End If
        Next
        'Wenn keine Position frei ist am Ende hinzufügen
        liste.Add(style.copy())
        Return liste.Count - 1
    End Function

    Public Sub replaceStyle(index As Integer, style As FontStyle)
        liste(index) = style
    End Sub

    Public Function Count() As Integer
        Return liste.Count
    End Function

    Public Sub removeAt(i As Integer)
        liste(i) = Nothing
        If i = liste.Count - 1 AndAlso liste(liste.Count - 1) Is Nothing Then
            liste.RemoveAt(liste.Count - 1)
        End If
    End Sub

    Public Sub clear()
        Me.liste.Clear()
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(liste.Count)
        For i As Integer = 0 To liste.Count - 1
            If liste(i) Is Nothing Then
                writer.Write(-1)
            Else
                writer.Write(1)
                liste(i).speichern(writer)
            End If
        Next
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As FontList
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then
            Throw New Exception("Falsches Dateiformat (Fehler S3000)")
        End If
        Dim lneu As New FontList()
        For i As Integer = 0 To anzahl - 1
            Dim v As Integer = reader.ReadInt32()
            If v = -1 Then
                lneu.add(Nothing)
            ElseIf v = 1 Then
                lneu.add(FontStyle.Einlesen(reader, version))
            Else
                Throw New Exception("Falsches Dateiformat (Fehler S3001)")
            End If
        Next
        Return lneu
    End Function

    Public Function clone() As FontList
        Dim erg As New FontList()
        For i As Integer = 0 To liste.Count - 1
            If liste(i) Is Nothing Then
                erg.liste.Add(Nothing)
            Else
                erg.liste.Add(Me.liste(i).copy())
            End If
        Next
        Return erg
    End Function
End Class

Public Class FontStyle
    Private Color As Farbe
    Private fontName As String
    Private schriftgrad_In_DTP As Single
    Private fett As Boolean
    Private kursiv As Boolean

    Public Property farbe As Farbe
        Get
            Return Color
        End Get
        Set(value As Farbe)
            Color = value
            updateBrush()
        End Set
    End Property

    Private brush As Brush
    Private myFontFamily As FontFamily

    Public Sub New(c As Farbe, fontName As String, schriftgrad_In_DTP As Single, kursiv As Boolean, fett As Boolean)
        Me.Color = c
        Me.fontName = fontName
        Me.kursiv = kursiv
        Me.fett = fett
        Me.schriftgrad_In_DTP = schriftgrad_In_DTP
        If schriftgrad_In_DTP < 0 Then
            Throw New Exception("Schriftgröße ist negativ!")
        End If

        updateBrush()
        updateFont()
    End Sub

    Public Sub setFontName(name As String)
        Me.fontName = name
        updateFont()
    End Sub

    Public Sub setSizeInPoints(size As Single)
        Me.schriftgrad_In_DTP = size
    End Sub

    Private Sub updateBrush()
        brush = New SolidBrush(Color.toColor())
    End Sub

    Private Sub updateFont()
        Me.myFontFamily = New FontFamily(fontName)
    End Sub

    Public Function getBrush(PixelPerMM As Single) As Brush
        Return brush
    End Function

    Public Function getXBrush() As XBrush
        Return New XSolidBrush(Color.toXColor())
    End Function

    Public Function getFont(PixelPerMM As Single) As Font
        Dim style As Drawing.FontStyle = Drawing.FontStyle.Regular
        If kursiv Then
            style = style Or Drawing.FontStyle.Italic
        End If
        If fett Then
            style = style Or Drawing.FontStyle.Bold
        End If
        Return New Font(myFontFamily, Me.schriftgrad_In_DTP * 0.352778F * PixelPerMM, style, GraphicsUnit.Pixel)
    End Function

    Public Function getTiefHochFont(PixelPerMM As Single) As Font
        Dim style As Drawing.FontStyle = Drawing.FontStyle.Regular
        If kursiv Then
            style = style Or Drawing.FontStyle.Italic
        End If
        If fett Then
            style = style Or Drawing.FontStyle.Bold
        End If
        Return New Font(myFontFamily, 0.7F * Me.schriftgrad_In_DTP * 0.352778F * PixelPerMM, style, GraphicsUnit.Pixel)
    End Function

    Public Function getXFont(args As GrafikPDFSharp_DrawArgs) As XFont
        Dim style As XFontStyle = XFontStyle.Regular
        If kursiv Then
            style = style Or XFontStyle.Italic
        End If
        If fett Then
            style = style Or XFontStyle.Bold
        End If

        Dim xfont As XFont
        Try
            xfont = New XFont(myFontFamily, Me.schriftgrad_In_DTP, style, XPdfFontOptions.UnicodeDefault)
        Catch ex As Exception
            xfont = New XFont("Times New Roman", Me.schriftgrad_In_DTP, style, XPdfFontOptions.UnicodeDefault)
            args.warnings.add("Die Schriftart '" & myFontFamily.Name & "' kann nicht exportiert werden. Es wird 'Times New Roman' verwendet.")
        End Try
        Return xfont
    End Function

    Public Function getTiefHochXFont(args As GrafikPDFSharp_DrawArgs) As XFont
        Dim style As XFontStyle = XFontStyle.Regular
        If kursiv Then
            style = style Or XFontStyle.Italic
        End If
        If fett Then
            style = style Or XFontStyle.Bold
        End If

        Dim xfont As XFont
        Try
            xfont = New XFont(myFontFamily, 0.7 * Me.schriftgrad_In_DTP, style, XPdfFontOptions.UnicodeDefault)
        Catch ex As Exception
            xfont = New XFont("Times New Roman", 0.7 * Me.schriftgrad_In_DTP, style, XPdfFontOptions.UnicodeDefault)
            args.warnings.Add("Die Schriftart '" & myFontFamily.Name & "' kann nicht exportiert werden. Es wird 'Times New Roman' verwendet.")
        End Try
        Return xfont
    End Function

    Public Function copy() As FontStyle
        Return New FontStyle(Me.Color, fontName, schriftgrad_In_DTP, kursiv, fett)
    End Function

    Public Function isSameAs(s As FontStyle) As Boolean
        Return Me.Color.isEqual(s.Color) AndAlso Me.fontName = s.fontName AndAlso Me.fett = s.fett AndAlso Me.kursiv = s.kursiv AndAlso Me.schriftgrad_In_DTP = s.schriftgrad_In_DTP
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(Color.Color_A)
        writer.Write(Color.Color_R)
        writer.Write(Color.Color_G)
        writer.Write(Color.Color_B)
        writer.Write(fontName)
        writer.Write(schriftgrad_In_DTP)
        writer.Write(fett)
        writer.Write(kursiv)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As FontStyle
        Dim A As Byte = reader.ReadByte()
        Dim R As Byte = reader.ReadByte()
        Dim G As Byte = reader.ReadByte()
        Dim B As Byte = reader.ReadByte()
        Dim name As String = reader.ReadString()
        If version < 11 AndAlso name = "Times" Then
            name = FontCombobox.getDefaultFont_OLD()
        End If
        Dim größe As Single = reader.ReadSingle()
        Dim fett As Boolean = reader.ReadBoolean()
        Dim kursiv As Boolean = reader.ReadBoolean()
        Return New FontStyle(New Farbe(A, R, G, B), name, größe, kursiv, fett)
    End Function

    Public Function getSizeInPoints() As Single
        Return Me.schriftgrad_In_DTP
    End Function

    Public Function getFontName() As String
        Return Me.fontName
    End Function
End Class