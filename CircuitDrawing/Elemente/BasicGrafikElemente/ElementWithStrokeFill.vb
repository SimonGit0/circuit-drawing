Public MustInherit Class ElementWithStrokeFill
    Inherits Bauteil
    Implements IElementWithFill

    Public fillstyle As Integer

    Public Sub New(ID As ULong, linestyle As Integer, fillstyle As Integer)
        MyBase.New(ID, linestyle)
        Me.fillstyle = fillstyle
    End Sub

    Public MustOverride Function Hat_Fillstyle() As Boolean

    Public Sub addEinstellungenStrokeFill(sender As Vektor_Picturebox, l As List(Of ElementEinstellung))
        If Hat_Fillstyle() Then l.Add(New Einstellung_Fillstil(Element.EINSTELLUNG_FILLSTYLE, fillstyle, sender.myFillStyles))
    End Sub

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Fillstil AndAlso e.Name = Element.EINSTELLUNG_FILLSTYLE Then
                Me.fillstyle = DirectCast(e, Einstellung_Fillstil).getNewFillstil(Me.fillstyle, sender.myFillStyles, changed)
            End If
        Next
        Return changed
    End Function

    Public Overrides Function NrOfSnappoints() As Integer
        Return 0
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Return Nothing
    End Function

    Public Function get_fillstyle() As Integer Implements IElementWithFill.get_fillstyle
        Return fillstyle
    End Function

    Public Sub set_fillstyle(fs As Integer) Implements IElementWithFill.set_fillstyle
        Me.fillstyle = fs
    End Sub
End Class

Public Class Kante
    Public start As Point
    Public ende As Point

    Public KantenIndex As Integer
    Public sender As Element
    Public isOnlyStartpunkt As Boolean
    Public rechtwinkligZuEndeZiehen As Boolean

    Public temp_remainingX As Integer
    Public temp_remainingY As Integer

    Public offgrid As Boolean
    Public ZeichneImmerHilfslinie As Boolean
    Public gespiegeltXY As Boolean = False

    Public Sub New(start As Point, ende As Point, index As Integer, isOnlyStartpunkt As Boolean, sender As Element, offgrid As Boolean)
        Me.New(start, ende, index, isOnlyStartpunkt, False, sender, offgrid, False)
    End Sub

    Public Sub New(start As Point, ende As Point, index As Integer, isOnlyStartpunkt As Boolean, rechtwinkligZuEndeZiehen As Boolean, sender As Element, offgrid As Boolean)
        Me.New(start, ende, index, isOnlyStartpunkt, rechtwinkligZuEndeZiehen, sender, offgrid, False)
    End Sub

    Public Sub New(start As Point, ende As Point, index As Integer, isOnlyStartpunkt As Boolean, rechtwinkligZuEndeZiehen As Boolean, sender As Element, offgrid As Boolean, ZeichneImmerHilfslinie As Boolean)
        Me.start = start
        Me.ende = ende
        Me.KantenIndex = index
        Me.sender = sender
        Me.isOnlyStartpunkt = isOnlyStartpunkt
        Me.rechtwinkligZuEndeZiehen = rechtwinkligZuEndeZiehen

        Me.temp_remainingX = 0
        Me.temp_remainingY = 0
        Me.offgrid = offgrid
        Me.ZeichneImmerHilfslinie = ZeichneImmerHilfslinie
    End Sub

End Class
