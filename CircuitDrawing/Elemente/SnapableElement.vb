Imports System.IO
Public MustInherit Class SnapableElement
    Inherits ElementMaster

    Public Sub New(ID As ULong)
        MyBase.New(ID)
    End Sub

    Public MustOverride Function getNrOfSnappoints() As Integer

    Public MustOverride Function getSnappoint(index As Integer) As WireSnappoint

    Public MustOverride Sub setSnappoint(index As Integer, s As WireSnappoint)

    Public Overrides Sub AfterDrawingGDI(bBox As Rectangle)
        Me.getSnappoint(0).lastSelectionRect = Mathe.moveInverted(bBox, Me.getSnappoint(0).getMitteInt())
    End Sub

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox, mode As ElementEinstellung.combineModus) As List(Of ElementEinstellung)
        Return New List(Of ElementEinstellung)
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Return False
    End Function

    Public Overrides Function hasSelection() As Boolean
        For i As Integer = 0 To getNrOfSnappoints() - 1
            If getSnappoint(i).isSelected Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Overrides Sub deselect()
        For i As Integer = 0 To getNrOfSnappoints() - 1
            getSnappoint(i).isSelected = False
        Next
    End Sub

    Public Sub selectAll()
        For i As Integer = 0 To getNrOfSnappoints() - 1
            getSnappoint(i).isSelected = True
        Next
    End Sub
End Class

Public Class WireSnappoint
    Public alpha As Single
    Public wireStart As Point
    Public wireEnd As Point
    Public isSelected As Boolean

    Public lastSelectionRect As Rectangle? = Nothing 'nur Zwischengröße, die angiebt ob eine größere Selection genommen werden soll.

    Private lastVectorGroeßerNull As Point 'nur Zwischengröße für Berechnung, wenn vorübergehend die Länge Null ist und man nicht mehr weiß in welche Richtung es ging...

    Public Sub setLastVectorGroeßerNull_DuringDraw()
        Dim v As Point = getVector()
        If v.X <> 0 OrElse v.Y <> 0 Then
            lastVectorGroeßerNull = v
        End If
    End Sub

    Public Sub New(alpha As Single, start As Point, ende As Point)
        Me.alpha = alpha
        Me.wireStart = start
        Me.wireEnd = ende
        Me.isSelected = False
        lastVectorGroeßerNull = getVector()
    End Sub

    Public Sub New(posCursor As Point, w As Wire)
        Me.wireStart = w.getStart()
        Me.wireEnd = w.getEnde()
        berechneAlpha(posCursor)
        Me.isSelected = False
        lastVectorGroeßerNull = getVector()
    End Sub

    Public Sub New(posCursor As Point, w As WireLuftlinie)
        Me.wireStart = w.getStart()
        Me.wireEnd = w.getEnde()
        berechneAlpha(posCursor)
        Me.isSelected = False
        lastVectorGroeßerNull = getVector()
    End Sub

    Public Function getSelection() As SelectionRect
        Dim pMitte As Point = getMitteInt()
        Dim r As New Rectangle(pMitte.X - 50, pMitte.Y - 50, 100, 100)
        If lastSelectionRect IsNot Nothing Then
            Return New SelectionRect(Mathe.Union(r, Mathe.move(lastSelectionRect.Value, pMitte)))
        Else
            Return New SelectionRect(r)
        End If
    End Function

    Public Function clone() As WireSnappoint
        Dim posNeu As New WireSnappoint(alpha, wireStart, wireEnd)
        posNeu.isSelected = Me.isSelected
        posNeu.lastVectorGroeßerNull = Me.lastVectorGroeßerNull
        Return posNeu
    End Function

    Public Function isEqualWithoutSelection(s As WireSnappoint) As Boolean
        If s.alpha <> Me.alpha Then Return False
        If s.wireStart <> Me.wireStart Then Return False
        If s.wireEnd <> Me.wireEnd Then Return False
        Return True
    End Function

    Private Sub berechneAlpha(posCuror As Point)
        Dim v As New Point(wireEnd.X - wireStart.X, wireEnd.Y - wireStart.Y)
        Dim normal As New Point(-v.Y, v.X)
        Dim d As New Point(wireStart.X - posCuror.X, wireStart.Y - posCuror.Y)

        Dim zähler As Integer = normal.X * d.Y - normal.Y * d.X
        Dim nenner As Integer = -normal.X * v.Y + normal.Y * v.X

        Dim alpha As Single = CSng(zähler / nenner)
        If alpha < 0 Then alpha = 0
        If alpha > 1 Then alpha = 1
        Me.alpha = alpha
    End Sub

    Private Sub berechneAlpha(posCuror As PointF)
        Dim v As New Point(wireEnd.X - wireStart.X, wireEnd.Y - wireStart.Y)
        Dim normal As New Point(-v.Y, v.X)
        Dim d As New PointF(wireStart.X - posCuror.X, wireStart.Y - posCuror.Y)

        Dim zähler As Single = normal.X * d.Y - normal.Y * d.X
        Dim nenner As Integer = -normal.X * v.Y + normal.Y * v.X
        If nenner = 0 Then
            Me.alpha = 0.5F
        Else
            Dim alpha As Single = CSng(zähler / nenner)
            If alpha < 0 Then alpha = 0
            If alpha > 1 Then alpha = 1
            Me.alpha = alpha
        End If
    End Sub

    Public Sub move(dx As Integer, dy As Integer)
        wireStart.X += dx
        wireEnd.X += dx
        wireStart.Y += dy
        wireEnd.Y += dy
    End Sub

    Public Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        wireStart.X -= drehpunkt.X
        wireStart.Y -= drehpunkt.Y
        wireStart = drehung.transformPoint(wireStart)
        wireStart.X += drehpunkt.X
        wireStart.Y += drehpunkt.Y

        wireEnd.X -= drehpunkt.X
        wireEnd.Y -= drehpunkt.Y
        wireEnd = drehung.transformPoint(wireEnd)
        wireEnd.X += drehpunkt.X
        wireEnd.Y += drehpunkt.Y
    End Sub

    Public Function flip() As WireSnappoint
        Return New WireSnappoint(1.0F - alpha, wireEnd, wireStart)
    End Function

    Public Sub flipIt()
        Me.alpha = 1.0F - alpha
        Dim z As Point = wireEnd
        Me.wireEnd = wireStart
        Me.wireStart = z
    End Sub

    Public Function getVector() As Point
        Return New Point(wireEnd.X - wireStart.X, wireEnd.Y - wireStart.Y)
    End Function

    Public Function getLastDirectionVector() As Point
        If wireEnd.X <> wireStart.X OrElse wireEnd.Y <> wireStart.Y Then
            Return getVector()
        Else
            'Wenn Vector = 0 ist, kann man nicht entscheiden wohin es geht...
            Return lastVectorGroeßerNull
        End If
    End Function

    Public Function getMitteInt() As Point
        Return New Point(CInt(wireStart.X + alpha * (wireEnd.X - wireStart.X)), CInt(wireStart.Y + alpha * (wireEnd.Y - wireStart.Y)))
    End Function

    Public Function getMitteF() As PointF
        Return New PointF(wireStart.X + alpha * (wireEnd.X - wireStart.X), wireStart.Y + alpha * (wireEnd.Y - wireStart.Y))
    End Function

    Public Function liegtAufWire(wStart As Point, wEnde As Point) As Boolean
        Return (wStart = Me.wireStart AndAlso wEnde = Me.wireEnd) OrElse
               (wStart = Me.wireEnd AndAlso wEnde = Me.wireStart)
    End Function

    Public Function Move_WennliegtAufWire(wireAlt As Element, wireNeu As ElementMaster, dx As Integer, dy As Integer) As WireSnappoint
        Dim wStart As Point
        Dim wEnde As Point
        If TypeOf wireAlt Is Wire Then
            wStart = DirectCast(wireAlt, Wire).getStart()
            wEnde = DirectCast(wireAlt, Wire).getEnde()
        ElseIf TypeOf wireAlt Is WireLuftlinie Then
            wStart = DirectCast(wireAlt, WireLuftlinie).getStart()
            wEnde = DirectCast(wireAlt, WireLuftlinie).getEnde()
        Else
            Throw New Exception("Kein gültiger Verbinder.")
        End If

        If wStart = Me.wireStart AndAlso wEnde = Me.wireEnd Then
            Dim neuStart As Point = wStart
            Dim neuEnde As Point = wEnde
            If wireNeu IsNot Nothing Then
                If TypeOf wireNeu Is Wire Then
                    neuStart = DirectCast(wireNeu, Wire).getStart()
                    neuEnde = DirectCast(wireNeu, Wire).getEnde()
                ElseIf TypeOf wireNeu Is WireLuftlinie Then
                    neuStart = DirectCast(wireNeu, WireLuftlinie).getStart()
                    neuEnde = DirectCast(wireNeu, WireLuftlinie).getEnde()
                Else
                    Throw New Exception("Kein gültiger Verbinder.")
                End If
            End If

            Return setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(neuStart, neuEnde, dx, dy)
        ElseIf wStart = Me.wireEnd AndAlso wEnde = Me.wireStart Then
            Dim neuStart As Point = wStart
            Dim neuEnde As Point = wEnde
            If wireNeu IsNot Nothing Then
                If TypeOf wireNeu Is Wire Then
                    neuStart = DirectCast(wireNeu, Wire).getStart()
                    neuEnde = DirectCast(wireNeu, Wire).getEnde()
                ElseIf TypeOf wireNeu Is WireLuftlinie Then
                    neuStart = DirectCast(wireNeu, WireLuftlinie).getStart()
                    neuEnde = DirectCast(wireNeu, WireLuftlinie).getEnde()
                Else
                    Throw New Exception("Kein gültiger Verbinder.")
                End If
            End If
            Return setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(neuEnde, neuStart, dx, dy)
        End If
        Return Nothing
    End Function

    Public Function setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(wStartNeu As Point, wEndeNeu As Point, dx As Integer, dy As Integer) As WireSnappoint
        'Dim w As New WireSnappoint(alpha, wStartNeu, wEndeNeu)
        Dim w As WireSnappoint = Me.clone()
        w.alpha = Me.alpha
        w.wireStart = wStartNeu
        w.wireEnd = wEndeNeu

        Dim mitte As PointF = getMitteF()
        mitte.X += dx
        mitte.Y += dy
        w.berechneAlpha(mitte)
        Return w
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(wireStart.X)
        writer.Write(wireStart.Y)
        writer.Write(wireEnd.X)
        writer.Write(wireEnd.Y)
        writer.Write(alpha)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As WireSnappoint
        Dim sx As Integer = reader.ReadInt32()
        Dim sy As Integer = reader.ReadInt32()
        Dim ex As Integer = reader.ReadInt32()
        Dim ey As Integer = reader.ReadInt32()
        Dim alpha As Single = reader.ReadSingle()
        Return New WireSnappoint(alpha, New Point(sx, sy), New Point(ex, ey))
    End Function

End Class
