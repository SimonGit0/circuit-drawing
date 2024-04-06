Imports System.IO

Public Class Element_Rect
    Inherits Element_BasicRectBased

    Public Const DefaultMultiRectVec_X As Integer = 50
    Public Const DefaultMultiRectVec_Y As Integer = -50
    Public Const DefaultMultiRectAnzahlStart As Integer = 2

    Private myMode As Modus
    Private multiRectVec As Point
    Private anzahlMultiRectStart As Integer

    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, linestyle As Integer, fillstyle As Integer, myMode As Modus, multiRectVec As Point, anzahlMultiRectStart As Integer)
        MyBase.New(ID, pos, breite, höhe, linestyle, fillstyle)
        Me.myMode = myMode
        Me.multiRectVec = multiRectVec
        Me.anzahlMultiRectStart = anzahlMultiRectStart
    End Sub

    Public Overrides Function getGrafik() As DO_Grafik
        If myMode = Modus.EinRect Then
            Dim r As New DO_Rechteck(New Rectangle(position, s), False, Drawing_FillMode.FillAndStroke)
            r.lineStyle.linestyle = linestyle
            r.lineStyle.scaling = 1.0F
            r.fillstyle = fillstyle
            Return r
        Else
            Dim g As New DO_MultiGrafik()
            Dim r As New DO_Rechteck(New Rectangle(position, s), False, Drawing_FillMode.FillAndStroke)
            g.childs.Add(r)
            Dim px As Integer
            Dim py As Integer
            Dim sh As Integer
            Dim sw As Integer
            If multiRectVec.X >= 0 AndAlso multiRectVec.Y <= 0 Then 'oben rechts
                px = position.X
                py = position.Y
                sh = s.Height
                sw = s.Width
            ElseIf multiRectVec.X >= 0 Then 'unten rechts
                px = position.X
                py = position.Y + s.Height
                sh = -s.Height
                sw = s.Width
            ElseIf multiRectVec.Y <= 0 Then 'oben links
                px = position.X + s.Width
                py = position.Y
                sh = s.Height
                sw = -s.Width
            Else 'unten links
                px = position.X + s.Width
                py = position.Y + s.Height
                sh = -s.Height
                sw = -s.Width
            End If
            For i As Integer = 1 To anzahlMultiRectStart
                Dim ps(4) As Point
                ps(0) = New Point(px + i * multiRectVec.X, py + (i - 1) * multiRectVec.Y)
                ps(1) = New Point(px + i * multiRectVec.X, py + i * multiRectVec.Y)
                ps(2) = New Point(px + i * multiRectVec.X + sw, py + i * multiRectVec.Y)
                ps(3) = New Point(px + i * multiRectVec.X + sw, py + i * multiRectVec.Y + sh)
                ps(4) = New Point(px + (i - 1) * multiRectVec.X + sw, py + i * multiRectVec.Y + sh)
                Dim l As New DO_MultiLinie(ps, False)
                g.childs.Add(l)
            Next

            g.setLineStyleRekursiv(linestyle)
            g.setLineScalingRekursiv(1.0F)
            g.setFillStyleRekursiv(fillstyle)
            Return g
        End If
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Public Function Clone_intern(newID As ULong) As ElementMaster
        Dim r As New Element_Rect(newID, Me.position, Me.s.Width, Me.s.Height, Me.linestyle, Me.fillstyle, Me.myMode, Me.multiRectVec, Me.anzahlMultiRectStart)
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
        writer.Write(CInt(Me.myMode))
        writer.Write(multiRectVec.X)
        writer.Write(multiRectVec.Y)
        writer.Write(anzahlMultiRectStart)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element_Rect
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim w As Integer = reader.ReadInt32()
        Dim h As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        Dim fs As Integer = reader.ReadInt32()
        Dim mode As Modus = Modus.EinRect
        Dim multiRectVec As New Point(DefaultMultiRectVec_X, DefaultMultiRectVec_Y)
        Dim anzahlStart As Integer = DefaultMultiRectAnzahlStart
        If version >= 24 Then
            mode = CType(reader.ReadInt32(), Modus)
            multiRectVec.X = reader.ReadInt32()
            multiRectVec.Y = reader.ReadInt32()
            anzahlStart = reader.ReadInt32()
        End If
        If w < 0 OrElse h < 0 Then
            Throw New Exception("Fehler beim Einlesen von EBR (Fehler R1000)")
        End If
        Dim r As New Element_Rect(sender.getNewID(), New Point(posX, posY), w, h, ls, fs, mode, multiRectVec, anzahlStart)
        r.isSelected = isSelected
        Return r
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_Rect Then Return False
        If Me.myMode <> DirectCast(e2, Element_Rect).myMode Then Return False
        If Me.multiRectVec.X <> DirectCast(e2, Element_Rect).multiRectVec.X Then Return False
        If Me.multiRectVec.Y <> DirectCast(e2, Element_Rect).multiRectVec.Y Then Return False
        If Me.anzahlMultiRectStart <> DirectCast(e2, Element_Rect).anzahlMultiRectStart Then Return False

        Return MyBase.isEqualExceptSelection(e2)
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)

        Dim e1 As New Einstellung_Multi("Parameter", False)

        Dim param As New TemplateParameter_Param("Typ", {"Einfach", "Mehrere Rechtecke"}, -1)
        e1.add(New Einstellung_TemplateParameter(param, CInt(myMode)))
        If myMode = Modus.MehrereRect Then
            Dim paramX As New TemplateParameter_Int("Offset X", New Intervall(-1000, 1000, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), DefaultMultiRectVec_X, "")
            Dim paramY As New TemplateParameter_Int("Offset Y", New Intervall(-1000, 1000, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), DefaultMultiRectVec_Y, "")
            e1.add(New Einstellung_TemplateParameter_Int(paramX, Me.multiRectVec.X))
            e1.add(New Einstellung_TemplateParameter_Int(paramY, Me.multiRectVec.Y))
            Dim paramAnzahl As New TemplateParameter_Int("Anzahl", New Intervall(0, 10, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), DefaultMultiRectAnzahlStart, "")
            e1.add(New Einstellung_TemplateParameter_Int(paramAnzahl, Me.anzahlMultiRectStart))
        End If
        l.Add(e1)

        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)

        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Multi Then
                If e.Name = "Parameter" Then
                    For Each eSub As Einstellung_TemplateParam In DirectCast(e, Einstellung_Multi).getListe()
                        If TypeOf eSub Is Einstellung_TemplateParameter Then
                            With DirectCast(eSub, Einstellung_TemplateParameter)
                                If .nrChanged Then
                                    If .Name = "Typ" Then
                                        If .myNr <> CInt(Me.myMode) Then
                                            Me.myMode = CType(.myNr, Modus)
                                            changed = True
                                        End If
                                    End If
                                End If
                            End With
                        ElseIf TypeOf eSub Is Einstellung_TemplateParameter_Int Then
                            With DirectCast(eSub, Einstellung_TemplateParameter_Int)
                                If .nrChanged Then
                                    If .Name = "Offset X" Then
                                        If .myNr <> Me.multiRectVec.X Then
                                            Me.multiRectVec.X = .myNr
                                            changed = True
                                        End If
                                    ElseIf .Name = "Offset Y" Then
                                        If .myNr <> Me.multiRectVec.Y Then
                                            Me.multiRectVec.Y = .myNr
                                            changed = True
                                        End If
                                    ElseIf .Name = "Anzahl" Then
                                        If .myNr <> Me.anzahlMultiRectStart Then
                                            Me.anzahlMultiRectStart = .myNr
                                            changed = True
                                        End If
                                    End If
                                End If
                            End With
                        End If
                    Next
                End If
            End If
        Next

        Return changed
    End Function

    Public Enum Modus
        EinRect = 0
        MehrereRect = 1
    End Enum
End Class
