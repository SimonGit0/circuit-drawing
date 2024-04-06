Imports System.IO

''' <summary>
''' Nur noch zur Kompatibilität da, falls eine alte Datei noch diese Blackbox-Klasse nutzt. Man kann es nicht mehr so hinzufügen!
''' </summary>
Public Class Blackbox
    Inherits Bauteil

    Private breite As Integer
    Private höhe As Integer
    Private snappoints As List(Of Snappoint)

    Private beschriftung As Beschriftung
    Private vectorBeschriftung As Point
    Private vectorBeschriftung_Senkrecht As Point

    Public Sub New(ID As ULong, linestyle As Integer, pos As Point, breite As Integer, höhe As Integer, beschriftung As Beschriftung)
        MyBase.New(ID, linestyle)
        Me.position = pos
        Me.breite = breite
        Me.höhe = höhe
        Me.snappoints = New List(Of Snappoint)
        Me.beschriftung = beschriftung
        vectorBeschriftung = New Point(1, 0)
        vectorBeschriftung_Senkrecht = New Point(0, 1)
    End Sub

    Public Sub addSnappoint(s As Snappoint)
        Me.snappoints.Add(s)
    End Sub

    Public Overrides Sub drehe(drehpunkt As Point, d As Drehmatrix)
        vectorBeschriftung = d.transformPoint(vectorBeschriftung)
        vectorBeschriftung_Senkrecht = d.transformPoint(vectorBeschriftung_Senkrecht)

        Dim myRect As Rectangle = New Rectangle(Me.position, New Size(breite, höhe))
        myRect.X -= drehpunkt.X
        myRect.Y -= drehpunkt.Y
        myRect = d.transformRect(myRect)
        myRect.X += drehpunkt.X
        myRect.Y += drehpunkt.Y

        Me.breite = myRect.Width
        Me.höhe = myRect.Height
        Dim posAlt As Point = Me.position
        Me.position = myRect.Location

        For i As Integer = 0 To snappoints.Count - 1
            Dim p As Point = snappoints(i).p
            p.X += posAlt.X - drehpunkt.X
            p.Y += posAlt.Y - drehpunkt.Y
            p = d.transformPoint(p)
            p.X += drehpunkt.X - position.X
            p.Y += drehpunkt.Y - position.Y
            Dim xPlus As New Point(1, 0)
            Dim xMinus As New Point(-1, 0)
            Dim yPlus As New Point(0, 1)
            Dim yMinus As New Point(0, -1)
            xPlus = d.transformPoint(xPlus)
            xMinus = d.transformPoint(xMinus)
            yPlus = d.transformPoint(yPlus)
            yMinus = d.transformPoint(yMinus)
            Dim neuerSnappoint As New Snappoint(p, 0, 0, 0, 0)
            setSnapppointRichtung(xPlus, snappoints(i).Xplus, neuerSnappoint)
            setSnapppointRichtung(xMinus, snappoints(i).Xminus, neuerSnappoint)
            setSnapppointRichtung(yMinus, snappoints(i).Yminus, neuerSnappoint)
            setSnapppointRichtung(yPlus, snappoints(i).Yplus, neuerSnappoint)

            snappoints(i) = neuerSnappoint
        Next
    End Sub

    Private Sub setSnapppointRichtung(vector As Point, wert As Integer, s As Snappoint)
        If vector.X = 0 Then
            If vector.Y > 0 Then
                s.Yplus = wert
                Exit Sub
            ElseIf vector.Y < 0 Then
                s.Yminus = wert
                Exit Sub
            End If
        ElseIf vector.Y = 0 Then
            If vector.X > 0 Then
                s.Xplus = wert
                Exit Sub
            ElseIf vector.X < 0 Then
                s.Xminus = wert
                Exit Sub
            End If
        End If
        Throw New NotImplementedException("Falsche Richtung")
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        Dim g As New DO_MultiGrafik()
        g.childs.Add(New DO_Rechteck(New Rectangle(Me.position, New Size(breite, höhe)), False, Drawing_FillMode.FillAndStroke))

        AddBeschriftungToGrafik(g, beschriftung)

        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
        Return g
    End Function

    Private Sub AddBeschriftungToGrafik(g As DO_MultiGrafik, b As Beschriftung)

        Dim tp As TextPoint = New TextPoint(New Point(Me.position.X + Me.breite \ 2, Me.position.Y + Me.höhe \ 2), 0, 0, vectorBeschriftung)
        Dim pos_text As New Point(tp.pos.X, tp.pos.Y)

        pos_text.X += b.abstand * tp.vektorAbstand.X
        pos_text.Y += b.abstand * tp.vektorAbstand.Y

        Dim normal As Point = vectorBeschriftung_Senkrecht
        pos_text.X += b.abstandQuer * normal.X
        pos_text.Y += b.abstandQuer * normal.Y

        Dim ah As DO_Text.AlignH
        If tp.xDir = 0 Then
            ah = DO_Text.AlignH.Mitte
        ElseIf tp.xDir < 0 Then
            ah = DO_Text.AlignH.Rechts
        Else
            ah = DO_Text.AlignH.Links
        End If
        Dim av As DO_Text.AlignV
        If tp.yDir = 0 Then
            av = DO_Text.AlignV.Mitte
        ElseIf tp.yDir < 0 Then
            av = DO_Text.AlignV.Unten
        Else
            av = DO_Text.AlignV.Oben
        End If

        Dim gText As New DO_Text(b.text, 0, pos_text, ah, av, b.textRot, False)

        g.childs.Add(gText)
    End Sub

    Public Overrides Function getSelection() As Selection
        Return New SelectionRect(New Rectangle(position.X, position.Y, breite, höhe))
    End Function

    Public Overrides Function NrOfSnappoints() As Integer
        Return snappoints.Count
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Dim s As Snappoint = snappoints(i).Clone()
        s.p.X += position.X
        s.p.Y += position.Y
        Return s
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Private Function Clone_intern(newID As ULong) As ElementMaster
        Dim erg As New Blackbox(newID, Me.linestyle, Me.position, Me.breite, Me.höhe, Me.beschriftung)
        erg.isSelected = Me.isSelected
        erg.vectorBeschriftung = Me.vectorBeschriftung
        erg.vectorBeschriftung_Senkrecht = Me.vectorBeschriftung_Senkrecht
        For i As Integer = 0 To snappoints.Count - 1
            erg.snappoints.Add(snappoints(i).Clone())
        Next
        Return erg
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Blackbox Then Return False
        If e2.ID <> Me.ID Then Return False
        If DirectCast(e2, Blackbox).position <> Me.position Then Return False
        If DirectCast(e2, Blackbox).linestyle <> Me.linestyle Then Return False
        With DirectCast(e2, Blackbox)
            If .breite <> breite Then Return False
            If .höhe <> höhe Then Return False
            If Not .beschriftung.isEqual(Me.beschriftung) Then Return False
            If .vectorBeschriftung <> vectorBeschriftung Then Return False
            If .vectorBeschriftung_Senkrecht <> vectorBeschriftung_Senkrecht Then Return False
            If .snappoints.Count <> snappoints.Count Then Return False
            For i As Integer = 0 To snappoints.Count - 1
                If Not .snappoints(i).isEqual(snappoints(i)) Then Return False
            Next
        End With
        Return True
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)()
        beschriftung.addEinstellungen(l)
        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If beschriftung.setEinstellung(e) Then
                changed = True
            End If
        Next
        Return changed
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(position.X)
        writer.Write(position.Y)
        writer.Write(breite)
        writer.Write(höhe)
        writer.Write(linestyle)
        beschriftung.speichern(writer)
        writer.Write(vectorBeschriftung.X)
        writer.Write(vectorBeschriftung.Y)
        writer.Write(vectorBeschriftung_Senkrecht.X)
        writer.Write(vectorBeschriftung_Senkrecht.Y)
        writer.Write(snappoints.Count)
        For i As Integer = 0 To snappoints.Count - 1
            snappoints(i).speichern(writer)
        Next
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Blackbox
        Dim x As Integer = reader.ReadInt32()
        Dim y As Integer = reader.ReadInt32()
        Dim breite As Integer = reader.ReadInt32()
        Dim höhe As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        If ls < 0 Then
            Throw New Exception("LS < 0. Fehler L1000")
        End If
        Dim b As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim vx As Integer = reader.ReadInt32()
        Dim vy As Integer = reader.ReadInt32()
        Dim vsx As Integer = reader.ReadInt32()
        Dim vsy As Integer = reader.ReadInt32()
        Dim anzahlSnappoints As Integer = reader.ReadInt32()
        If anzahlSnappoints < 0 Then
            Throw New Exception("Negative Anzahl an Snappoints. Fehler S0100")
        End If
        Dim erg As New Blackbox(sender.getNewID(), ls, New Point(x, y), breite, höhe, b)
        erg.vectorBeschriftung = New Point(vx, vy)
        erg.vectorBeschriftung_Senkrecht = New Point(vsx, vsy)
        For i As Integer = 0 To anzahlSnappoints - 1
            erg.snappoints.Add(Snappoint.Einlesen(reader, version))
        Next
        Return erg
    End Function
End Class
