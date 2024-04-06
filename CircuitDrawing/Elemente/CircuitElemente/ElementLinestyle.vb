Public MustInherit Class ElementLinestyle
    Inherits Element
    Implements IElementWithLinestyle

    Public Property linestyle As Integer
    Public Sub New(ID As ULong, linestyle As Integer)
        MyBase.New(ID)
        Me.linestyle = linestyle
    End Sub

    Public Function get_mylinestyle() As Integer Implements IElementWithLinestyle.get_mylinestyle
        Return linestyle
    End Function

    Public Sub set_mylinestyle(ls As Integer) Implements IElementWithLinestyle.set_mylinestyle
        Me.linestyle = ls
    End Sub

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim erg As List(Of ElementEinstellung) = MyBase.getEinstellungen(sender)
        erg.Add(New Einstellung_Linienstil(Element.EINSTELLUNG_LINESTYLE, Me.linestyle, sender.myLineStyles))
        Return erg
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Linienstil AndAlso e.Name = Element.EINSTELLUNG_LINESTYLE Then
                Me.linestyle = DirectCast(e, Einstellung_Linienstil).getNewLinienstil(Me.linestyle, sender.myLineStyles, changed, False)
            End If
        Next
        Return changed
    End Function

End Class
