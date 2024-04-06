Imports System.IO
Imports PdfSharp.Drawing
Public Class FillStyleList
    Private liste As List(Of FillStyle)

    Public Sub New()
        Me.liste = New List(Of FillStyle)
    End Sub

    Public Sub add(l As FillStyle)
        Me.liste.Add(l)
    End Sub

    Public Function getFillStyle(index As Integer) As FillStyle
        Return liste(index)
    End Function

    Public Function getNumberOfNewFillStyle(style As FillStyle) As Integer
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

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As FillStyleList
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then
            Throw New Exception("Falsches Dateiformat (Fehler S2000)")
        End If
        Dim lneu As New FillStyleList()
        For i As Integer = 0 To anzahl - 1
            Dim v As Integer = reader.ReadInt32()
            If v = -1 Then
                lneu.add(Nothing)
            ElseIf v = 1 Then
                lneu.add(FillStyle.Einlesen(reader, version))
            Else
                Throw New Exception("Falsches Dateiformat (Fehler S2001)")
            End If
        Next
        Return lneu
    End Function

    Public Function clone() As FillStyleList
        Dim erg As New FillStyleList()
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

Public Class FillStyle
    Private Color As Farbe

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

    Public Sub New(c As Farbe)
        Me.Color = c

        updateBrush()
    End Sub

    Private Sub updateBrush()
        brush = New SolidBrush(Color.toColor())
    End Sub

    Public Function getBrush(PixelPerMM As Single) As Brush
        Return brush
    End Function

    Public Function getXBrush() As XBrush
        Return New XSolidBrush(Color.toXColor())
    End Function

    Public Function copy() As FillStyle
        Return New FillStyle(Me.Color)
    End Function

    Public Function isSameAs(s As FillStyle) As Boolean
        Return Me.Color.isEqual(s.Color)
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(Color.Color_A)
        writer.Write(Color.Color_R)
        writer.Write(Color.Color_G)
        writer.Write(Color.Color_B)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As FillStyle
        Dim A As Byte = reader.ReadByte()
        Dim R As Byte = reader.ReadByte()
        Dim G As Byte = reader.ReadByte()
        Dim B As Byte = reader.ReadByte()
        Return New FillStyle(New Farbe(A, R, G, B))
    End Function

    Public Sub writeEPS(writer As StreamWriter, args As GrafikEPS_DrawArgs)
        args.switchToColor(writer, Color)
    End Sub
End Class