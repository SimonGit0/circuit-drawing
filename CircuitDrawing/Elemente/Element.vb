Public MustInherit Class Element
    Inherits ElementMaster

    Public Const EINSTELLUNG_POS As String = "Position"
    Public Shared ReadOnly EINSTELLUNG_LINESTYLE As String = My.Resources.Strings.Einstellung_Linienstil
    Public Shared ReadOnly EINSTELLUNG_FONTSTYLE As String = My.Resources.Strings.Einstellung_Fontstyle
    Public Shared ReadOnly EINSTELLUNG_FILLSTYLE As String = My.Resources.Strings.Einstellung_Füllstil
    Public Shared ReadOnly EINSTELLUNG_BESCHRIFTUNG As String = My.Resources.Strings.Einstellung_Beschriftung
    Public Shared ReadOnly EINSTELLUNG_SINGLEPFEILSPITZE As String = My.Resources.Strings.Einstellung_Pfeilspitze
    Public Shared ReadOnly EINSTELLUNG_ABSTAND_BESCHRIFTUNG As String = My.Resources.Strings.Einstellung_AbstandBeschriftung
    Public Shared ReadOnly EINSTELLUNGNAME_ABSTAND_BESCHRIFTUNG As String = My.Resources.Strings.Einstellung_Abstand
    Public Shared ReadOnly EINSTELLUNGNAME_ABSTAND_QUER As String = My.Resources.Strings.Einstellung_AbstandQuer
    Public Shared ReadOnly EINSTELLUNG_PFEILSPITZEN As String = My.Resources.Strings.Einstellung_Pfeilspitzen

    Public Property position As Point
    Public Property isSelected As Boolean

    Public Sub New(ID As ULong)
        MyBase.New(ID)
    End Sub

    Public MustOverride Function getSelection() As Selection

    Public MustOverride Function NrOfSnappoints() As Integer

    Public MustOverride Function getSnappoint(i As Integer) As Snappoint

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim erg As New List(Of ElementEinstellung)
        erg.Add(New Einstellung_Pos(EINSTELLUNG_POS, position, "X:", "Y:"))
        Return erg
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = False
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Pos AndAlso e.Name = EINSTELLUNG_POS Then
                With DirectCast(e, Einstellung_Pos)
                    If .changedX Then
                        Me.position = New Point(.pos.X, position.Y)
                        changed = True
                    End If
                    If .changedY Then
                        Me.position = New Point(position.X, .pos.Y)
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function

    Public Overridable Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        Throw New NotImplementedException()
    End Function

    Public Overridable Function getScaleKantenCount() As Integer
        Return 0
    End Function

    Public Overridable Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        Return False
    End Function

    Public Function snapsTo(e2 As Element) As Boolean
        For i As Integer = 0 To Me.NrOfSnappoints() - 1
            Dim p1 As Point = getSnappoint(i).p
            For j As Integer = 0 To e2.NrOfSnappoints() - 1
                Dim p2 As Point = e2.getSnappoint(j).p
                If p1.X = p2.X AndAlso p1.Y = p2.Y Then
                    Return True
                End If
            Next
        Next
        Return False
    End Function

    Public Overridable Sub markAllUsedFillstyles(usedFillstyles() As Boolean)
    End Sub

    Public Overrides Function hasSelection() As Boolean
        Return Me.isSelected
    End Function

    Public Overrides Sub deselect()
        Me.isSelected = False
    End Sub
End Class

Public MustInherit Class Selection
    Public MustOverride Function isInRect(rTest As Rectangle) As Boolean

    Public MustOverride Function getGrafik() As DO_Grafik

    Public MustOverride Function distanceToBounds(p As Point, faktorAnzeige As Single, gridX As Integer, gridY As Integer) As Double

    Public MustOverride Function distanceToBoundsWithoutClip(p As Point, faktorAnzeige As Single) As Double
End Class

Public Class SelectionRect
    Inherits Selection

    Public r As Rectangle
    Public Sub New(r As Rectangle)
        Me.r = r
    End Sub

    Public Overrides Function isInRect(rTest As Rectangle) As Boolean
        Return rTest.X <= r.X AndAlso rTest.Y <= r.Y AndAlso rTest.X + rTest.Width >= r.X + r.Width AndAlso rTest.Y + rTest.Height >= r.Y + r.Height
    End Function

    Public Overrides Function getGrafik() As DO_Grafik
        Return New DO_Rechteck(r, False, Drawing_FillMode.OnlyStroke)
    End Function

    Public Overrides Function distanceToBounds(p As Point, faktorAnzeige As Single, gridX As Integer, gridY As Integer) As Double
        If Mathe.isInRect(p, r) Then
            Dim d1 As Double = Mathe.distLiniePoint(New Point(r.X, r.Y), New Point(r.X, r.Y + r.Height), p)
            d1 = Math.Min(d1, Mathe.distLiniePoint(New Point(r.X, r.Y + r.Height), New Point(r.X + r.Width, r.Y + r.Height), p))
            d1 = Math.Min(d1, Mathe.distLiniePoint(New Point(r.X + r.Width, r.Y + r.Height), New Point(r.X + r.Width, r.Y), p))
            Return Math.Min(d1, Mathe.distLiniePoint(New Point(r.X + r.Width, r.Y), New Point(r.X, r.Y), p))
        End If
        Return Double.MaxValue
    End Function

    Public Overrides Function distanceToBoundsWithoutClip(p As Point, faktorAnzeige As Single) As Double
        Dim d1 As Double = Mathe.distLiniePoint(New Point(r.X, r.Y), New Point(r.X, r.Y + r.Height), p)
        d1 = Math.Min(d1, Mathe.distLiniePoint(New Point(r.X, r.Y + r.Height), New Point(r.X + r.Width, r.Y + r.Height), p))
        d1 = Math.Min(d1, Mathe.distLiniePoint(New Point(r.X + r.Width, r.Y + r.Height), New Point(r.X + r.Width, r.Y), p))
        Return Math.Min(d1, Mathe.distLiniePoint(New Point(r.X + r.Width, r.Y), New Point(r.X, r.Y), p))
    End Function
End Class

Public Class SelectionLine
    Inherits Selection

    Public start As Point
    Public ende As Point

    Public Sub New(s As Point, e As Point)
        Me.start = s
        Me.ende = e
    End Sub

    Public Overrides Function isInRect(rTest As Rectangle) As Boolean
        Return Mathe.isInRect(start, rTest) AndAlso Mathe.isInRect(ende, rTest)
    End Function

    Public Overrides Function getGrafik() As DO_Grafik
        Return New DO_Linie(start, ende, False)
    End Function

    Public Overrides Function distanceToBounds(p As Point, faktorAnzeige As Single, gridX As Integer, gridY As Integer) As Double
        Dim d As Double = Mathe.distLiniePoint(start, ende, p)
        If start.X = ende.X Then
            If d < gridX Then
                Return 0
            Else
                Return Double.MaxValue
            End If
        ElseIf start.Y = ende.Y Then
            If d < gridY Then
                Return 0
            Else
                Return Double.MaxValue
            End If
        Else
            If d * faktorAnzeige < 5.0 Then
                Return d
            Else
                Return Double.MaxValue
            End If
        End If
    End Function

    Public Overrides Function distanceToBoundsWithoutClip(p As Point, faktorAnzeige As Single) As Double
        Return Mathe.distLiniePoint(start, ende, p)
    End Function
End Class

Public Class SelectionBezier
    Inherits Selection

    Public p1 As Point
    Public p2 As Point
    Public p3 As Point
    Public p4 As Point

    Public Sub New(p1 As Point, p2 As Point, p3 As Point, p4 As Point)
        Me.p1 = p1
        Me.p2 = p2
        Me.p3 = p3
        Me.p4 = p4
    End Sub

    Public Overrides Function isInRect(rTest As Rectangle) As Boolean
        Dim r As Rectangle = Mathe.getBezierBoundingBox(p1, p2, p3, p4)
        Return rTest.X <= r.X AndAlso rTest.Y <= r.Y AndAlso rTest.X + rTest.Width >= r.X + r.Width AndAlso rTest.Y + rTest.Height >= r.Y + r.Height
    End Function

    Public Overrides Function getGrafik() As DO_Grafik
        Return New DO_Bezier({p1, p2, p3, p4}, False, Drawing_FillMode.OnlyStroke)
    End Function

    Public Overrides Function distanceToBounds(p As Point, faktorAnzeige As Single, gridX As Integer, gridY As Integer) As Double
        'Zum schnellen Test, erstmal mit dem Boundingbox checken, ob es überhaupt in Frage kommt!
        Dim r As Rectangle = Mathe.getBezierBoundingBox(p1, p2, p3, p4)
        'Boundingbox entsprechend vergrößern, damit auch die Randpixel gefunden werden können
        r.X -= CInt(5.0 / faktorAnzeige)
        r.Y -= CInt(5.0 / faktorAnzeige)
        r.Width += 2 * CInt(5.0 / faktorAnzeige)
        r.Height += 2 * CInt(5.0 / faktorAnzeige)
        If p.X >= r.X AndAlso p.Y >= r.Y AndAlso p.X <= r.X + r.Width AndAlso p.Y <= r.Y + r.Height Then
            'Nur wenn man innerhalb des Boundingbox liegt den genauen (und langsamen - ca. 0,1ms) vergleich ausführen
            Dim nextPoint As PointD = Mathe.approxNextPointOnBezier(p1, p2, p3, p4, p, 8)
            Dim dist As Double = Math.Sqrt((nextPoint.X - p.X) * (nextPoint.X - p.X) + (nextPoint.Y - p.Y) * (nextPoint.Y - p.Y))
            If dist * faktorAnzeige < 5.0 Then
                Return dist
            Else
                Return Double.MaxValue
            End If
        End If
        Return Double.MaxValue
    End Function

    Public Overrides Function distanceToBoundsWithoutClip(p As Point, faktorAnzeige As Single) As Double
        Throw New NotImplementedException()
    End Function
End Class
