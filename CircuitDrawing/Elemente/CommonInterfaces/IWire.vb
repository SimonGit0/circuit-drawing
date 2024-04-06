Public Interface IWire
    Function getStart() As Point
    Function getEnde() As Point

    Function getGrafikMitZeilensprüngen(radius As Integer, allWires As List(Of IWire)) As DO_Grafik
End Interface
