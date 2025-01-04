Imports System.IO

Public Class Element_Ellipse
    Inherits Element_BasicRectBased

    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, linestyle As Integer, fillstyle As Integer)
        MyBase.New(ID, pos, breite, höhe, linestyle, fillstyle)
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim r As New DO_Ellipse(New Rectangle(position, s), False, Drawing_FillMode.FillAndStroke)
        r.lineStyle.linestyle = linestyle
        r.lineStyle.scaling = 1.0F
        r.fillstyle = fillstyle
        Return r
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Public Function Clone_intern(newID As ULong) As ElementMaster
        Dim r As New Element_Ellipse(newID, Me.position, Me.s.Width, Me.s.Height, Me.linestyle, Me.fillstyle)
        r.isSelected = Me.isSelected
        Return r
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(isSelected)
        writer.Write(position.X)
        writer.Write(position.Y)
        writer.Write(s.Width)
        writer.Write(s.Height)
        writer.Write(linestyle)
        writer.Write(fillstyle)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element_Ellipse
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim w As Integer = reader.ReadInt32()
        Dim h As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        Dim fs As Integer = reader.ReadInt32()
        If w < 0 OrElse h < 0 Then
            Throw New Exception("Fehler beim Einlesen von EBR (Fehler R1001)")
        End If
        Dim r As New Element_Ellipse(sender.getNewID(), New Point(posX, posY), w, h, ls, fs)
        r.isSelected = isSelected
        Return r
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_Ellipse Then Return False
        Return MyBase.isEqualExceptSelection(e2)
    End Function
End Class
