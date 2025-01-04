Imports System.IO

Public Class Element_Textfeld
    Inherits Element_BasicRectBased
    Implements IElementWithFont

    Private myText As String
    Private myFont As Integer
    Private ah As DO_Textfeld.AlignH
    Private av As DO_Text.AlignV

    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, linestyle As Integer, fillstyle As Integer, font As Integer, text As String, ah As DO_Textfeld.AlignH, av As DO_Text.AlignV)
        MyBase.New(ID, pos, breite, höhe, linestyle, fillstyle)
        Me.myFont = font
        Me.myText = text
        Me.ah = ah
        Me.av = av
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim t As New DO_Textfeld(New Rectangle(position, s), myText, myFont, av, ah, False)
        t.lineStyle.linestyle = linestyle
        t.lineStyle.scaling = 1.0F
        t.fillstyle = fillstyle
        Return t
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim r As New Element_Textfeld(newID, Me.position, Me.s.Width, Me.s.Height, Me.linestyle, Me.fillstyle, Me.myFont, Me.myText, Me.ah, Me.av)
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
        writer.Write(myFont)
        writer.Write(myText)
        writer.Write(CInt(ah))
        writer.Write(CInt(av))
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element_Textfeld
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim w As Integer = reader.ReadInt32()
        Dim h As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        Dim fs As Integer = reader.ReadInt32()
        Dim font As Integer = reader.ReadInt32()
        Dim text As String = reader.ReadString()
        Dim ah As DO_Textfeld.AlignH = CType(reader.ReadInt32(), DO_Textfeld.AlignH)
        Dim av As DO_Text.AlignV = CType(reader.ReadInt32(), DO_Text.AlignV)
        If w < 0 OrElse h < 0 Then
            Throw New Exception("Fehler beim Einlesen von EBR (Fehler R1000)")
        End If
        Dim r As New Element_Textfeld(sender.getNewID(), New Point(posX, posY), w, h, ls, fs, font, text, ah, av)
        r.isSelected = isSelected
        Return r
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_Textfeld Then Return False
        With DirectCast(e2, Element_Textfeld)
            If Me.myFont <> .myFont Then Return False
            If Me.myText <> .myText Then Return False
            If Me.ah <> .ah Then Return False
            If Me.av <> .av Then Return False
        End With
        Return MyBase.isEqualExceptSelection(e2)
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim liste As New List(Of ElementEinstellung)
        liste.Add(New EinstellungTextMultiLine("Text", Me.myText))
        liste.Add(New Einstellung_Textausrichtung(My.Resources.Strings.Einstellung_Textausrichtung_Textfeld, ah, av))
        liste.Add(New Einstellung_Fontstyle(Element.EINSTELLUNG_FONTSTYLE, Me.myFont, sender.myFonts))
        liste.AddRange(MyBase.getEinstellungen(sender))
        Return liste
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is EinstellungTextMultiLine AndAlso e.Name.get_ID() = "Text" Then
                With DirectCast(e, EinstellungTextMultiLine)
                    If .textChanged Then
                        Me.myText = .neuerText
                        changed = True
                    End If
                End With
            ElseIf TypeOf e Is Einstellung_Textausrichtung AndAlso e.Name.get_ID() = My.Resources.Strings.Einstellung_Textausrichtung_Textfeld Then
                With DirectCast(e, Einstellung_Textausrichtung)
                    If .av_changed Then
                        Me.av = .av
                        changed = True
                    End If
                    If .ah_changed Then
                        Me.ah = .ah
                        changed = True
                    End If
                End With
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = Element.EINSTELLUNG_FONTSTYLE Then
                Me.myFont = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.myFont, sender.myFonts, changed, False)
            End If
        Next
        Return changed
    End Function

    Public Function get_fontstyle() As Integer Implements IElementWithFont.get_fontstyle
        Return myFont
    End Function

    Public Sub set_fontstyle(fs As Integer) Implements IElementWithFont.set_fontstyle
        myFont = fs
    End Sub
End Class
