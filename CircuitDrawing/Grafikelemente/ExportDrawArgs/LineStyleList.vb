Imports System.Drawing.Drawing2D
Imports System.IO
Imports PdfSharp.Drawing
Public Class LineStyleList
    Private liste As List(Of LineStyle)

    Public Sub New()
        Me.liste = New List(Of LineStyle)
    End Sub

    Public Sub add(l As LineStyle)
        Me.liste.Add(l)
    End Sub

    Public Function getLineStyle(index As Integer) As LineStyle
        Return liste(index)
    End Function

    Public Function getNumberOfNewLinestyle(style As LineStyle) As Integer
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

    Public Sub replaceStyle(index As Integer, style As LineStyle)
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

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As LineStyleList
        Dim anzahl As Integer = reader.ReadInt32()
        If anzahl < 0 Then
            Throw New Exception("Falsches Dateiformat (Fehler S1000)")
        End If
        Dim lneu As New LineStyleList()
        For i As Integer = 0 To anzahl - 1
            Dim v As Integer = reader.ReadInt32()
            If v = -1 Then
                lneu.add(Nothing)
            ElseIf v = 1 Then
                lneu.add(LineStyle.Einlesen(reader, version))
            Else
                Throw New Exception("Falsches Dateiformat (Fehler S1001)")
            End If
        Next
        Return lneu
    End Function

    Public Function clone() As LineStyleList
        Dim erg As New LineStyleList()
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

Public Class LineStyle
    Private Color As Farbe
    Private lineStartCap As LineCap
    Private lineEndCap As LineCap
    Private lineDashCap As DashCap
    Private lineJoin As LineJoin
    Private lineWidthInMM As Single
    Private dashStyle As DashStyle

    Public alwaysUsePenWidthOfOne As Boolean = False

    Public Property Dicke As Single
        Get
            Return lineWidthInMM
        End Get
        Set(value As Single)
            lineWidthInMM = value
            updatePen()
        End Set
    End Property

    Public Property _DashStyle As DashStyle
        Get
            Return dashStyle
        End Get
        Set(value As DashStyle)
            dashStyle = value
        End Set
    End Property

    Public Property farbe As Farbe
        Get
            Return Color
        End Get
        Set(value As Farbe)
            Color = value
            updatePen()
        End Set
    End Property

    Private pen As Pen
    Private xPen As XPen

    Public Sub New(c As Farbe, startCap As LineCap, endCap As LineCap, DashCap As DashCap, linejoin As LineJoin, lineWidthInMM As Single, dashStyle As DashStyle)
        Me.Color = c
        Me.lineStartCap = startCap
        Me.lineEndCap = endCap
        Me.lineDashCap = DashCap
        Me.lineJoin = linejoin
        Me.lineWidthInMM = lineWidthInMM
        Me.dashStyle = dashStyle

        updatePen()
    End Sub

    Private Sub updatePen()
        pen = New Pen(Drawing.Color.FromArgb(Color.Color_A, Color.Color_R, Color.Color_G, Color.Color_B), lineWidthInMM)
        pen.SetLineCap(lineStartCap, lineEndCap, lineDashCap)
        pen.LineJoin = lineJoin
    End Sub

    Public Function getPen(PixelPerMM As Single, scaling As Single, noDash As Boolean) As Pen
        Dim w As Single = lineWidthInMM * PixelPerMM * scaling
        w = Math.Max(1.0F, w)
        If w <> pen.Width Then
            pen.Width = w
        End If

        If alwaysUsePenWidthOfOne Then
            'Option zum Zeichnen von Symbolen in der Bauelementeliste oder so.
            'Es sollte eine "harmonische" Liniendicke haben, also immer 1
            'Nur das scaling berücksichtigen, da trotzdem die Dicken Linien dicker sein sollen!
            pen.Width = Math.Max(1.0F, scaling)
        End If

        If noDash Then
            pen.DashStyle = Drawing2D.DashStyle.Solid
        Else
            Dim ds() As Single = dashStyle.getDashStyle(PixelPerMM / pen.Width, 0.0F) 'DashPattern bei vb.net: Richtiger Pixelabstand gleich pen.width*dashPattern(i), daher hier länge / pen.width teilen!
            If ds Is Nothing Then
                pen.DashStyle = Drawing2D.DashStyle.Solid
            Else
                pen.DashPattern = ds
            End If
        End If
        Return pen
    End Function

    Public Function getXPen(scaling As Single, noDash As Boolean) As XPen
        Dim w As Double = GrafikPDFSharp_DrawArgs.mmToPoint(lineWidthInMM * scaling)
        If xPen Is Nothing Then
            xPen = New XPen(XColor.FromArgb(Color.Color_A, Color.Color_R, Color.Color_G, Color.Color_B), w)
            If lineStartCap = LineCap.Flat Then
                xPen.LineCap = XLineCap.Flat
            ElseIf lineStartCap = LineCap.Square Then
                xPen.LineCap = XLineCap.Square
            Else
                xPen.LineCap = XLineCap.Round
            End If
            If lineJoin = LineJoin.Bevel Then
                xPen.LineJoin = XLineJoin.Bevel
            ElseIf lineJoin = LineJoin.Miter OrElse lineJoin = LineJoin.MiterClipped Then
                xPen.LineJoin = XLineJoin.Miter
            Else
                xPen.LineJoin = XLineJoin.Round
            End If
        End If
        If w <> xPen.Width Then
            xPen.Width = w
        End If

        If alwaysUsePenWidthOfOne Then
            'Option zum Zeichnen von Symbolen in der Bauelementeliste oder so.
            'Es sollte eine "harmonische" Liniendicke haben, also immer 1
            'Nur das scaling berücksichtigen, da trotzdem die Dicken Linien dicker sein sollen!
            xPen.Width = Math.Max(1.0F, scaling)
        End If

        If noDash Then
            xPen.DashStyle = XDashStyle.Solid
        Else
            Dim ds() As Single = dashStyle.getDashStyle(CSng(GrafikPDFSharp_DrawArgs.mmToPoint(1.0) / xPen.Width), lineWidthInMM * scaling) 'DashPattern bei vb.net: Richtiger Pixelabstand gleich pen.width*dashPattern(i), daher hier länge / pen.width teilen!
            If ds Is Nothing Then
                xPen.DashStyle = XDashStyle.Solid
            Else
                Dim dd(ds.Length - 1) As Double
                For i As Integer = 0 To dd.Length - 1
                    dd(i) = ds(i)
                Next
                xPen.DashPattern = dd
            End If
        End If
        Return xPen
    End Function

    Public Sub writeEPS(writer As StreamWriter, scaling As Single, args As GrafikEPS_DrawArgs)
        args.switchToLinewidth(writer, Dicke * scaling)
        args.switchToColor(writer, Me.Color)
        If Me.lineStartCap = LineCap.Flat Then
            args.switchToLinecap(writer, 0) 'flat
        ElseIf Me.lineStartCap = LineCap.Square Then
            args.switchToLinecap(writer, 2) 'square
        Else
            args.switchToLinecap(writer, 1) 'round
        End If
        If Me.lineJoin = LineJoin.Miter OrElse Me.lineJoin = LineJoin.MiterClipped Then
            args.switchToLinejoin(writer, 0) 'miter
        ElseIf Me.lineJoin = LineJoin.Bevel Then
            args.switchToLinejoin(writer, 2) 'bevel
        Else
            args.switchToLinejoin(writer, 1) 'round
        End If

        Dim ds() As Single = dashStyle.getDashStyle(GrafikEPS_DrawArgs.mmToPoint(1.0F), Dicke * scaling)
        args.switchToDashstyle(writer, ds)
    End Sub

    Public Function getSolidBrush() As SolidBrush
        Return New SolidBrush(Me.farbe.toColor())
    End Function

    Public Function getSolidBrushX() As XSolidBrush
        Return New XSolidBrush(Me.farbe.toXColor())
    End Function

    Public Function copy() As LineStyle
        Return New LineStyle(Me.Color, Me.lineStartCap, Me.lineEndCap, Me.lineDashCap, Me.lineJoin, Me.lineWidthInMM, Me.dashStyle.copy())
    End Function

    Public Function isSameAs(s As LineStyle) As Boolean
        Return Me.Color.isEqual(s.Color) AndAlso
               Me.lineStartCap = s.lineStartCap AndAlso Me.lineEndCap = s.lineEndCap AndAlso Me.lineDashCap = s.lineDashCap AndAlso Me.lineJoin = s.lineJoin AndAlso
               Me.lineWidthInMM = s.lineWidthInMM AndAlso
               Me.dashStyle.isSameAs(s.dashStyle)
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(lineWidthInMM)
        writer.Write(CInt(lineJoin))
        writer.Write(CInt(lineDashCap))
        writer.Write(CInt(lineEndCap))
        writer.Write(CInt(lineStartCap))
        writer.Write(Color.Color_A)
        writer.Write(Color.Color_R)
        writer.Write(Color.Color_G)
        writer.Write(Color.Color_B)
        dashStyle.speichern(writer)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As LineStyle
        Dim lw As Single = reader.ReadSingle()
        Dim ljoin As LineJoin = CType(reader.ReadInt32(), LineJoin)
        Dim dcap As DashCap = CType(reader.ReadInt32(), DashCap)
        Dim ecap As LineCap = CType(reader.ReadInt32(), LineCap)
        Dim scap As LineCap = CType(reader.ReadInt32(), LineCap)
        Dim A As Byte = reader.ReadByte()
        Dim R As Byte = reader.ReadByte()
        Dim G As Byte = reader.ReadByte()
        Dim B As Byte = reader.ReadByte()
        Dim ds As DashStyle = DashStyle.laden(reader, version)
        Return New LineStyle(New Farbe(A, R, G, B), scap, ecap, dcap, ljoin, lw, ds)
    End Function

    'Public Shared Function getDashStyle(index As Integer, faktor As Single, lineWidth_PDF As Single) As Single()
    '    Dim erg() As Single
    '    Select Case index
    '        Case 0
    '            Return Nothing
    '        Case 1
    '            erg = {3.0F * faktor, 1.5F * faktor}
    '        Case 2
    '            erg = {3.0F * faktor, 3.0F * faktor}
    '        Case 3
    '            erg = {3.0F * faktor, 0.75F * faktor}
    '        Case 4
    '            erg = {1.5F * faktor, 1.5F * faktor}
    '        Case 5
    '            erg = {1.5F * faktor, 3.0F * faktor}
    '        Case 6
    '            erg = {1.5F * faktor, 0.75F * faktor}
    '        Case 7
    '            erg = {0.75F * faktor, 0.75F * faktor}
    '        Case 8
    '            erg = {0.75F * faktor, 1.5F * faktor}
    '        Case 9
    '            erg = {3.0F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor}
    '        Case 10
    '            erg = {3.0F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor}
    '        Case 11
    '            erg = {3.0F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor, 0.75F * faktor}
    '        Case 12
    '            erg = {3.0F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor}
    '        Case 13
    '            erg = {3.0F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor}
    '        Case 14
    '            erg = {3.0F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor, 1.5F * faktor, 0.75F * faktor}
    '        Case 15
    '            erg = {3.0F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor}
    '        Case 16
    '            erg = {3.0F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor}
    '        Case 17
    '            erg = {3.0F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor, 1.5F * faktor}
    '        Case Else
    '            Throw New IndexOutOfRangeException()
    '    End Select

    '    For i As Integer = 0 To erg.Length - 1
    '        If i Mod 2 = 0 Then
    '            erg(i) = Math.Max(0.0F, erg(i) - lineWidth_PDF * faktor)
    '        Else
    '            erg(i) += lineWidth_PDF * faktor
    '        End If
    '    Next

    '    Return erg
    'End Function
End Class

Public Structure Farbe
    Public Color_A As Byte
    Public Color_R As Byte
    Public Color_G As Byte
    Public Color_B As Byte

    Public Sub New(A As Byte, R As Byte, G As Byte, B As Byte)
        Me.Color_A = A
        Me.Color_R = R
        Me.Color_G = G
        Me.Color_B = B
    End Sub

    Public Function isEqual(f2 As Farbe) As Boolean
        Return Me.Color_A = f2.Color_A AndAlso
               Me.Color_R = f2.Color_R AndAlso
               Me.Color_G = f2.Color_G AndAlso
               Me.Color_B = f2.Color_B
    End Function

    Public Function toColor() As Color
        Return Color.FromArgb(Color_A, Color_R, Color_G, Color_B)
    End Function

    Public Function toXColor() As XColor
        Return XColor.FromArgb(Color_A, Color_R, Color_G, Color_B)
    End Function

    Public Function toColorOhne_A() As Color
        Return Color.FromArgb(255, Color_R, Color_G, Color_B)
    End Function
End Structure

Public Class DashStyle
    Public art As Integer
    Public scale As Integer

    Public Sub New(art As Integer, scale As Integer)
        Me.art = art
        Me.scale = scale
    End Sub

    Public Sub New(oldIndex As Integer)
        Select Case oldIndex
            Case 0
                Me.art = 0
                Me.scale = 100
            Case 1
                Me.art = 2
                Me.scale = 400
            Case 2
                Me.art = 1
                Me.scale = 400
            Case 3
                Me.art = 3
                Me.scale = 400
            Case 4
                Me.art = 1
                Me.scale = 200
            Case 5
                Me.art = 4
                Me.scale = 200
            Case 6
                Me.art = 2
                Me.scale = 200
            Case 7
                Me.art = 1
                Me.scale = 100
            Case 8
                Me.art = 4
                Me.scale = 100
            Case 9
                Me.art = 5
                Me.scale = 400
            Case 10
                Me.art = 6
                Me.scale = 400
            Case 11
                Me.art = 7
                Me.scale = 400
            Case 12
                Me.art = 8
                Me.scale = 400
            Case 13
                Me.art = 9
                Me.scale = 400
            Case 14
                Me.art = 10
                Me.scale = 400
            Case 15
                Me.art = 11
                Me.scale = 200
            Case 16
                Me.art = 12
                Me.scale = 200
            Case 17
                Me.art = 13
                Me.scale = 200
        End Select
    End Sub

    Public Function getDashStyle(faktor As Single, lineWidth_PDF As Single) As Single()
        Dim erg() As Single
        Dim faktorOhneScale As Single = faktor
        faktor = faktor * Me.scale / 100.0F
        Select Case Me.art
            Case 0
                Return Nothing
            Case 1
                erg = {0.75F * faktor, 0.75F * faktor}
            Case 2
                erg = {0.75F * faktor, 0.375F * faktor}
            Case 3
                erg = {0.75F * faktor, 0.1875F * faktor}
            Case 4
                erg = {0.75F * faktor, 1.5F * faktor}
            Case 5
                erg = {0.75F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor}
            Case 6
                erg = {0.75F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor}
            Case 7
                erg = {0.75F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor, 0.1875F * faktor}
            Case 8
                erg = {0.75F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor}
            Case 9
                erg = {0.75F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor}
            Case 10
                erg = {0.75F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor, 0.375F * faktor, 0.1875F * faktor}
            Case 11
                erg = {0.75F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor}
            Case 12
                erg = {0.75F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor}
            Case 13
                erg = {0.75F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor, 0.375F * faktor}
            Case Else
                Throw New IndexOutOfRangeException()
        End Select

        Dim delta As Single = 0
        For i As Integer = 0 To erg.Length - 1
            If i Mod 2 = 0 Then
                delta = erg(i) - Math.Max(0, erg(i) - lineWidth_PDF * faktorOhneScale)
                erg(i) -= delta
                If erg(i) = 0 Then
                    erg(i) = Single.Epsilon
                End If
            Else
                erg(i) += delta
            End If
        Next

        Return erg
    End Function

    Public Function copy() As DashStyle
        Return New DashStyle(Me.art, Me.scale)
    End Function

    Public Function isSameAs(ds As DashStyle) As Boolean
        If Me.art <> ds.art Then Return False
        If Me.scale <> ds.scale Then Return False
        Return True
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(art)
        writer.Write(scale)
    End Sub

    Public Shared Function Laden(reader As BinaryReader, version As Integer) As DashStyle
        If version >= 25 Then
            Dim art As Integer = reader.ReadInt32()
            Dim scale As Integer = reader.ReadInt32()
            Return New DashStyle(art, scale)
        Else
            Dim OldNr As Integer = reader.ReadInt32()
            Return New DashStyle(OldNr)
        End If
    End Function

    Public Shared Function getDashStyleCount() As Integer
        Return 14
    End Function
End Class