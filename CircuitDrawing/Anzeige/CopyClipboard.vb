Imports System.IO
Public Class CopyClipboard
    Public Const CLIPBOARD_MITTEMPLATES As ULong = 4561003253987654321
    Public ElementeAll As List(Of ElementMaster)

    Public LinestyleList As LineStyleList
    Public FillstyleList As FillStyleList
    Public FontstyleList As FontList

    Public Sub New(e As List(Of ElementMaster), ls As LineStyleList, fs As FillStyleList, fnts As FontList)
        Me.ElementeAll = e
        Me.LinestyleList = ls
        Me.FillstyleList = fs
        Me.FontstyleList = fnts
    End Sub

    Public Sub transform(dx As Integer, dy As Integer)
        For i As Integer = 0 To ElementeAll.Count - 1
            If TypeOf ElementeAll(i) Is Element Then
                DirectCast(ElementeAll(i), Element).position = New Point(DirectCast(ElementeAll(i), Element).position.X + dx, DirectCast(ElementeAll(i), Element).position.Y + dy)
            ElseIf TypeOf ElementeAll(i) Is SnapableElement Then
                For k As Integer = 0 To DirectCast(ElementeAll(i), SnapableElement).getNrOfSnappoints() - 1
                    DirectCast(ElementeAll(i), SnapableElement).getSnappoint(k).move(dx, dy)
                Next
            End If
        Next
    End Sub

    Public Function copy() As CopyClipboard
        Dim neueE As New List(Of ElementMaster)(ElementeAll.Count)
        For i As Integer = 0 To Me.ElementeAll.Count - 1
            neueE.Add(Me.ElementeAll(i).Clone())
        Next
        Return New CopyClipboard(neueE, Me.LinestyleList.clone(), FillstyleList.clone(), FontstyleList.clone())
    End Function

    Public Function getByteArray(bib As Bibliothek) As Byte()
        Dim ms As New MemoryStream()
        Dim writer As New BinaryWriter(ms)

        '-------------------------------------------------------
        'Variante ohne LokaleTemplates!!!
        'writer.Write(Form_Vektorgrafik.VEKTORGRAFIK_DATEIKONSTANTE)
        'Variante mit LokalenTemplates!!!
        writer.Write(CLIPBOARD_MITTEMPLATES)
        '-------------------------------------------------------
        writer.Write(Form_Vektorgrafik.Speicher_Version)

        '''''''''''''''''''''''''''''''''''''''''
        'Speichern der verwendeten Templates (alle Templates speichern, besser ist besser!):
        bib.speicherVerwendeteTemplates(writer, ElementeAll, True, True)
        '''''''''''''''''''''''''''''''''''''''''
        Dim anzahlSelected As Integer = 0
        For i As Integer = 0 To ElementeAll.Count - 1
            If ElementeAll(i).hasSelection() Then
                anzahlSelected += 1
            End If
        Next
        writer.Write(anzahlSelected)
        For i As Integer = 0 To ElementeAll.Count - 1
            If ElementeAll(i).hasSelection() Then
                If TypeOf ElementeAll(i) Is Element Then
                    Vektor_Picturebox.SpeicherCircuitElement(DirectCast(ElementeAll(i), Element), writer)
                ElseIf TypeOf ElementeAll(i) Is SnapableElement Then
                    Vektor_Picturebox.SpeicherSnapElement(DirectCast(ElementeAll(i), SnapableElement), writer)
                End If
            End If
        Next
        '''''''''''''''''''''''''''''''''''''''''
        Me.LinestyleList.speichern(writer)
        '''''''''''''''''''''''''''''''''''''''''
        Me.FillstyleList.speichern(writer)
        '''''''''''''''''''''''''''''''''''''''''
        Me.FontstyleList.speichern(writer)
        '''''''''''''''''''''''''''''''''''''''''

        writer.Flush()
        Dim byt() As Byte = ms.GetBuffer()
        Return byt
    End Function

    Public Shared Function Einlesen(sender As Vektor_Picturebox, bib As Bibliothek, byt() As Byte) As CopyClipboard
        Dim ms As New MemoryStream(byt)
        Dim reader As New BinaryReader(ms)

        Dim dateiArt As ULong = reader.ReadUInt64()

        If dateiArt = Form_Vektorgrafik.VEKTORGRAFIK_DATEIKONSTANTE OrElse dateiArt = CLIPBOARD_MITTEMPLATES Then
            Dim version As Integer = reader.ReadInt32()
            If version < 0 Then
                Throw New Exception("Falsche Version! Fehler V9000")
            End If
            Dim fillStile_VersionKleiner27_transparent As Integer
            If version < 27 Then
                fillStile_VersionKleiner27_transparent = sender.myFillStyles.getNumberOfNewFillStyle(New FillStyle(New Farbe(0, 255, 255, 255)))
            End If
            '--------------------------------------------
            'Laden der Bauteilbibliothek
            Dim myLokalBib As LokaleBibliothek
            If dateiArt = CLIPBOARD_MITTEMPLATES Then
                myLokalBib = Bibliothek.ladeGespeicherteSymboleAlsNeueBib(reader, version)
            Else
                myLokalBib = New LokaleBibliothek()
            End If
            ''''''''''''''''''''''''''''''''''''''''
            Dim passes As Integer = 1
            If version <= 30 Then
                passes = 2 'Bei Version <= 30 wurden snapableElemente separat von circuitElementen gespeichert! Daher muss zweimal eingelesen werden!
            End If
            Dim neueE As New List(Of ElementMaster)
            For iters As Integer = 1 To passes
                Dim anzahlSelected As Integer = reader.ReadInt32()
                For i As Integer = 0 To anzahlSelected - 1
                    Dim typ As Integer = reader.ReadInt32()
                    If typ = Vektor_Picturebox._SPEICHERN_StartElement Then
                        neueE.Add(Vektor_Picturebox.LadeCircuitElement(sender, reader, version, bib, myLokalBib, False, fillStile_VersionKleiner27_transparent, typ))
                    ElseIf typ = Vektor_Picturebox._SPEICHERN_StartElementSnapping Then
                        neueE.Add(Vektor_Picturebox.LadeSnapElement(sender, reader, version, typ))
                    End If
                Next
            Next
            ''''''''''''''''''''''''''''''''''''''''
            Dim ls As LineStyleList = LineStyleList.Einlesen(reader, version)
            Dim fs As FillStyleList = FillStyleList.Einlesen(reader, version)
            Dim fts As FontList = sender.myFonts
            If version >= 32 Then
                fts = FontList.Einlesen(reader, version)
            End If
            Return New CopyClipboard(neueE, ls, fs, fts)
        Else
            Return Nothing
        End If
    End Function

End Class
