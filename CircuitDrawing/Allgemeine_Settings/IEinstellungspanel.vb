Public Interface IEinstellungspanel

    Function getPanel() As Panel

    Sub Set_Default()

    Sub InitValues()

    Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean

    Function getName() As String

    Sub OnShown()
End Interface

Public Class EinstellungSpeichernArgs
    Public bib As Bibliothek
    Public FRM_BauelementeAuswahl As WR_BauelementeAuswahl
    Public Vektor_Picturebox As Vektor_Picturebox

    Public Sub New(bib As Bibliothek, FRM_BauelementeAuswahl As WR_BauelementeAuswahl, Vektor_Picturebox As Vektor_Picturebox)
        Me.bib = bib
        Me.FRM_BauelementeAuswahl = FRM_BauelementeAuswahl
        Me.Vektor_Picturebox = Vektor_Picturebox
    End Sub
End Class
