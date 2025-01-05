Imports System.IO
Public MustInherit Class ElementMaster

    Public ReadOnly ID As ULong

    Public Sub New(ID As ULong)
        Me.ID = ID
    End Sub

    Public MustOverride Function getGrafik(args As getGrafikArgs) As DO_Grafik

    Public MustOverride Function getEinstellungen(sender As Vektor_Picturebox, mode As ElementEinstellung.combineModus) As List(Of ElementEinstellung)

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

Public Class getGrafikArgs
    Public ReadOnly mitZeilensprüngen As Boolean
    Public ReadOnly wires As List(Of Tuple(Of Point, Point))
    Public ReadOnly radiusZeilensprünge As Integer
    Public deltaX As Integer
    Public deltaY As Integer

    Public Sub New(mitZeilensprüngen As Boolean, wires As List(Of Tuple(Of Point, Point)), radiusZeilensprünge As Integer)
        Me.mitZeilensprüngen = mitZeilensprüngen
        Me.wires = wires
        Me.radiusZeilensprünge = radiusZeilensprünge
        Me.deltaX = 0
        Me.deltaY = 0
    End Sub
End Class