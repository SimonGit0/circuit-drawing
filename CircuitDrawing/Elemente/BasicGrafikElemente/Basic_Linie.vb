Imports System.IO

Public Class Basic_Linie
    Inherits ElementWithStrokeFill
    Implements IElementWithAddableSnappoint

    Private vector As Point
    Private pfeilStart As ParamArrow
    Private pfeilEnde As ParamArrow

    Private mySnappoints As List(Of SnappointOnLine)

    Public Sub New(ID As ULong, linestyle As Integer, start As Point, ende As Point, pfeilStart As ParamArrow, pfeilEnde As ParamArrow)
        MyBase.New(ID, linestyle, 0)
        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
        Me.pfeilStart = pfeilStart
        Me.pfeilEnde = pfeilEnde
        Me.mySnappoints = New List(Of SnappointOnLine)
    End Sub

    Public Function getStart() As Point
        Return Me.position
    End Function

    Public Function getEnde() As Point
        Return New Point(Me.position.X + vector.X, Me.position.Y + vector.Y)
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        Dim start As Point = Me.position
        Dim ende As Point = getEnde()
        start.X -= drehpunkt.X
        start.Y -= drehpunkt.Y
        ende.X -= drehpunkt.X
        ende.Y -= drehpunkt.Y

        start = drehung.transformPoint(start)
        ende = drehung.transformPoint(ende)

        start.X += drehpunkt.X
        start.Y += drehpunkt.Y
        ende.X += drehpunkt.X
        ende.Y += drehpunkt.Y

        Me.position = start
        Me.vector = New Point(ende.X - start.X, ende.Y - start.Y)
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim g As DO_Grafik
        Dim start As Point = Me.getStart()
        Dim ende As Point = Me.getEnde()
        Dim längeSQ As Long = Mathe.abstandQuadrat(start, ende)

        g = Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(start, ende, pfeilStart, pfeilEnde)
        g.lineStyle.linestyle = Me.linestyle
        If TypeOf g Is DO_MultiGrafik Then
            DirectCast(g, DO_MultiGrafik).setLineStyleRekursiv(Me.linestyle)
        End If
        Return g
    End Function

    Public Overrides Function getSelection() As Selection
        Dim p1 As Point = getStart()
        Dim p4 As Point = getEnde()
        Return New SelectionLine(p1, p4)
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim v As New Basic_Linie(newID, linestyle, Me.getStart(), Me.getEnde(), pfeilStart.CopyPfeil(), pfeilEnde.CopyPfeil())
        v.isSelected = Me.isSelected
        v.mySnappoints = New List(Of SnappointOnLine)(Me.mySnappoints.Count)
        For i As Integer = 0 To Me.mySnappoints.Count - 1
            v.mySnappoints.Add(mySnappoints(i).clone())
        Next
        Return v
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Basic_Linie Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Basic_Linie)
            If .linestyle <> Me.linestyle Then Return False
            If .position <> Me.position Then Return False
            If .vector <> Me.vector Then Return False
            If Not .pfeilEnde.isEqual(Me.pfeilEnde) Then Return False
            If Not .pfeilStart.isEqual(Me.pfeilStart) Then Return False
            If .mySnappoints.Count <> Me.mySnappoints.Count Then Return False
            For i As Integer = 0 To Me.mySnappoints.Count - 1
                If Not .mySnappoints(i).isEqual(Me.mySnappoints(i)) Then Return False
            Next
        End With
        Return True
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(getStart().X)
        writer.Write(getStart().Y)
        writer.Write(getEnde().X)
        writer.Write(getEnde().Y)

        writer.Write(CInt(pfeilEnde.pfeilArt))
        writer.Write(CInt(pfeilEnde.pfeilSize))
        writer.Write(CInt(pfeilStart.pfeilArt))
        writer.Write(CInt(pfeilStart.pfeilSize))

        writer.Write(linestyle)

        writer.Write(mySnappoints.Count)
        For i As Integer = 0 To mySnappoints.Count - 1
            mySnappoints(i).speichern(writer)
        Next
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Basic_Linie
        Dim sx As Integer = reader.ReadInt32()
        Dim sy As Integer = reader.ReadInt32()
        Dim ex As Integer = reader.ReadInt32()
        Dim ey As Integer = reader.ReadInt32()

        Dim pfeilEnde As New ParamArrow(-1, 100)
        Dim pfeilStart As New ParamArrow(-1, 100)
        pfeilEnde.pfeilArt = CShort(reader.ReadInt32())
        pfeilEnde.pfeilSize = CUShort(reader.ReadInt32())
        pfeilStart.pfeilArt = CShort(reader.ReadInt32())
        pfeilStart.pfeilSize = CUShort(reader.ReadInt32())

        Dim ls As Integer = reader.ReadInt32()

        Dim erg As New Basic_Linie(sender.getNewID(), ls, New Point(sx, sy), New Point(ex, ey), pfeilStart, pfeilEnde)

        Dim anzahlSnappoints As Integer = 0
        If version >= 17 Then
            anzahlSnappoints = reader.ReadInt32()
        End If
        For i As Integer = 0 To anzahlSnappoints - 1
            erg.mySnappoints.Add(SnappointOnLine.Einlesen(reader, version))
        Next

        Return erg
    End Function

#Region "Scale"
    Public Overrides Function getScaleKantenCount() As Integer
        Return 2
    End Function

    Public Overrides Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        If index = 0 Then
            Return New Kante(Me.getStart(), Me.getEnde(), 0, True, True, Me, False)
        ElseIf index = 1 Then
            Return New Kante(Me.getEnde(), Me.getStart(), 1, True, True, Me, False)
        End If
        Throw New IndexOutOfRangeException()
    End Function

    Public Overrides Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        If kante.KantenIndex = 0 Then
            'start
            Me.position = New Point(Me.position.X + dx, Me.position.Y + dy)
            Me.vector = New Point(Me.vector.X - dx, Me.vector.Y - dy)
        ElseIf kante.KantenIndex = 1 Then
            'ende
            Me.vector = New Point(Me.vector.X + dx, Me.vector.Y + dy)
        Else
            Return False
        End If
        out_invalidate_screen = True
        Return True
    End Function
#End Region

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        l.Add(New Einstellung_Pfeilspitze(Element.EINSTELLUNG_PFEILSPITZEN, pfeilStart, pfeilEnde))
        MyBase.addEinstellungenStrokeFill(sender, l)
        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Pfeilspitze AndAlso e.Name.get_ID() = Element.EINSTELLUNG_PFEILSPITZEN Then
                With DirectCast(e, Einstellung_Pfeilspitze)
                    If .startChanged Then
                        Me.pfeilStart.pfeilArt = .pfeilStart.pfeilArt
                        changed = True
                    End If
                    If .endeChanged Then
                        Me.pfeilEnde.pfeilArt = .pfeilEnde.pfeilArt
                        changed = True
                    End If
                    If .startSizeChanged Then
                        Me.pfeilStart.pfeilSize = .pfeilStart.pfeilSize
                        changed = True
                    End If
                    If .endeSizeChanged Then
                        Me.pfeilEnde.pfeilSize = .pfeilEnde.pfeilSize
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function

    Public Overrides Function Hat_Fillstyle() As Boolean
        Return False
    End Function

#Region "Add Snappoint"
    Private Function getNextAddableSnappoint(cursorPos As Point, max_allowed_dist_from_cursor As Long) As Point? Implements IElementWithAddableSnappoint.getNextAddableSnappoint
        Dim l_squared As Long = CLng(vector.X) * CLng(vector.X) + CLng(vector.Y) * CLng(vector.Y)
        If l_squared = 0 Then Return Nothing

        Dim q As New Point(position.X - cursorPos.X, position.Y - cursorPos.Y)

        Dim alpha As Double = -(q.X * vector.X + q.Y * vector.Y) / l_squared
        alpha = Math.Max(Math.Min(alpha, 1.0), 0.0)
        Return New Point(CInt(position.X + alpha * vector.X), CInt(position.Y + alpha * vector.Y))
    End Function

    Public Function addSnappoint(cursorPos As Point) As Boolean Implements IElementWithAddableSnappoint.addSnappoint
        Dim l_squared As Long = CLng(vector.X) * CLng(vector.X) + CLng(vector.Y) * CLng(vector.Y)
        If l_squared = 0 Then Return False 'kann keinen snappoint hinzufügen!

        Dim q As New Point(position.X - cursorPos.X, position.Y - cursorPos.Y)

        Dim alpha As Double = -(q.X * vector.X + q.Y * vector.Y) / l_squared
        alpha = Math.Max(Math.Min(alpha, 1.0), 0.0)

        Me.mySnappoints.Add(New SnappointOnLine(CSng(alpha)))
        Return True
    End Function

    Public Function getNextSnappoint(cursorPos As Point, max_allowed_dist_from_cursor As Long) As Integer Implements IElementWithAddableSnappoint.getNextSnappoint
        If mySnappoints.Count <= 0 Then Return -1
        Dim mindist As Long = max_allowed_dist_from_cursor
        Dim minIndex As Integer = -1
        For i As Integer = 0 To mySnappoints.Count - 1
            Dim dist As Long = Mathe.abstandQuadrat(cursorPos, getSnappointSimple(i))
            If dist < mindist Then
                mindist = dist
                minIndex = i
            End If
        Next
        Return minIndex
    End Function

    Public Function getSnappointSimple(index As Integer) As Point Implements IElementWithAddableSnappoint.getSnappointSimple
        Dim alpha As Single = Me.mySnappoints(index).alpha
        Return New Point(CInt(position.X + alpha * vector.X), CInt(position.Y + alpha * vector.Y))
    End Function

    Public Sub deleteSnappoint(index As Integer) Implements IElementWithAddableSnappoint.deleteSnappoint
        Me.mySnappoints.RemoveAt(index)
    End Sub

    Public Overrides Function NrOfSnappoints() As Integer
        Return mySnappoints.Count
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Dim alpha As Single = Me.mySnappoints(i).alpha
        Dim pos As New Point(CInt(position.X + alpha * vector.X), CInt(position.Y + alpha * vector.Y))
        If vector.X = 0 Then
            If alpha = 0 Then
                If vector.Y > 0 Then
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY2)
                Else
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY0)
                End If
            ElseIf alpha = 1 Then
                If vector.Y < 0 Then
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY2)
                Else
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY0)
                End If
            Else
                Return New Snappoint(pos, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY2)
            End If
        ElseIf vector.Y = 0 Then
            If alpha = 0 Then
                If vector.X > 0 Then
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1)
                Else
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1)
                End If
            ElseIf alpha = 1 Then
                If vector.X < 0 Then
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1)
                Else
                    Return New Snappoint(pos, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY1, TemplateAusDatei.PENALTY1)
                End If
            Else
                Return New Snappoint(pos, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY2, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY0)
            End If
        Else
            Return New Snappoint(pos, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY0, TemplateAusDatei.PENALTY0)
        End If
    End Function
#End Region

End Class

Public Class SnappointOnLine
    Public alpha As Single 'wo auf der Linie liegt der Snappoint
    Public Sub New(alpha As Single)
        Me.alpha = alpha
        If alpha < 0 OrElse alpha > 1 Then Throw New Exception("Der Snappoint muss auf der Linie liegen!")
    End Sub

    Public Function clone() As SnappointOnLine
        Return New SnappointOnLine(alpha)
    End Function

    Public Function isEqual(s As SnappointOnLine) As Boolean
        If Me.alpha <> s.alpha Then Return False
        Return True
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(alpha)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As SnappointOnLine
        Dim alpha As Single = reader.ReadSingle()
        Return New SnappointOnLine(alpha)
    End Function
End Class
