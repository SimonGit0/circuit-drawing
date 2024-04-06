Imports System.IO
Public MustInherit Class ElementMaster

    Public ReadOnly ID As ULong

    Public Sub New(ID As ULong)
        Me.ID = ID
    End Sub

    Public MustOverride Function getGrafik() As DO_Grafik

    Public MustOverride Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)

    Public MustOverride Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean

    Public MustOverride Sub drehe(drehpunkt As Point, drehung As Drehmatrix)

    Public MustOverride Sub speichern(writer As BinaryWriter)

    Public MustOverride Function hasSelection() As Boolean

    Public MustOverride Sub deselect()

    Public Overridable Sub AfterDrawingGDI(bBox As Rectangle)
    End Sub

    Public MustOverride Function Clone() As ElementMaster

    Public MustOverride Function Clone(get_newID As Func(Of ULong)) As ElementMaster

    Public MustOverride Function isEqualExceptSelection(e2 As ElementMaster) As Boolean

End Class
