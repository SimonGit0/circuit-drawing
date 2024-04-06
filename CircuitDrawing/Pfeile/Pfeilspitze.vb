Public MustInherit Class Pfeilspitze
    'Pfeilspitze wird immer in richtung 'rechts' gemalt. Also Vektor (0,1)

    Public MustOverride Function getLineVerkürzung() As Integer

    Public Function getGrafik(a As AlignPfeil, pos As Point, vektor As Point) As DO_Polygon
        Dim g As DO_Polygon = getGrafik_Basic(a)
        g.SetMatrix(vektor)
        g.transform(New Transform_translate(pos))
        Return g
    End Function

    Public MustOverride Function getGrafik_Basic(a As AlignPfeil) As DO_Polygon

    Public Enum AlignPfeil
        Align_An_Spitze
        Align_An_Mitte
    End Enum
End Class
