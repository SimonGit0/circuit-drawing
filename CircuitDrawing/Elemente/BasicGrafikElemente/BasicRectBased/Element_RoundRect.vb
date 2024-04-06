Imports System.IO

Public Class Element_RoundRect
    Inherits Element_BasicRectBased

    Public Shared ReadOnly EINSTELLUNG_RUNDES_RECHTECK As String = My.Resources.Strings.Einstellung_Rundes_Rechteck

    Private radius As Integer

    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, radius As Integer, linestyle As Integer, fillstyle As Integer)
        MyBase.New(ID, pos, breite, höhe, linestyle, fillstyle)
        Me.radius = radius
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        Dim r As New DO_RoundRect(New Rectangle(position, s), radius, False)
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

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim r As New Element_RoundRect(newID, Me.position, Me.s.Width, Me.s.Height, Me.radius, Me.linestyle, Me.fillstyle)
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
        writer.Write(radius)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element_RoundRect
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim w As Integer = reader.ReadInt32()
        Dim h As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        Dim fs As Integer = reader.ReadInt32()
        Dim radius As Integer = reader.ReadInt32()
        If w < 0 OrElse h < 0 Then
            Throw New Exception("Fehler beim Einlesen von EBR (Fehler R1000)")
        End If
        Dim r As New Element_RoundRect(sender.getNewID(), New Point(posX, posY), w, h, radius, ls, fs)
        r.isSelected = isSelected
        Return r
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_RoundRect Then Return False
        With DirectCast(e2, Element_RoundRect)
            If .radius <> Me.radius Then Return False
        End With
        Return MyBase.isEqualExceptSelection(e2)
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)()
        l.Add(New Einstellung_Int(EINSTELLUNG_RUNDES_RECHTECK, My.Resources.Strings.Einstellung_Rundes_Rechteck_Radius, radius))
        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Int AndAlso e.Name = EINSTELLUNG_RUNDES_RECHTECK Then
                With DirectCast(e, Einstellung_Int)
                    If .changed Then
                        If .value < 0 Then
                            .value = 0
                            If .myTxt IsNot Nothing Then .myTxt.Text = "0"
                        End If
                        Me.radius = .value
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function
End Class
