Public Interface IWire
    Function getStart() As Point

    Function getStart(delta As Point) As Point
    Function getEnde() As Point
    Function getEnde(delta As Point) As Point
End Interface
