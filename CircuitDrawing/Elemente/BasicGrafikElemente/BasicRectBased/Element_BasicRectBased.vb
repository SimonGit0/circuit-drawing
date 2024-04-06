Public MustInherit Class Element_BasicRectBased
    Inherits ElementWithStrokeFill

    Protected s As Size

    Protected Overridable Sub OnSizeChanged(r As OnSizeChangedReason)
    End Sub

    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, linestyle As Integer, fillstyle As Integer)
        MyBase.New(ID, linestyle, fillstyle)
        Me.position = pos
        Me.s = New Size(breite, höhe)
        Me.OnSizeChanged(OnSizeChangedReason.Konstruktor)
    End Sub

    Public Overrides Function getSelection() As Selection
        Return New SelectionRect(New Rectangle(position, s))
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        l.Add(New Einstellung_Pos(My.Resources.Strings.Einstellung_Größe, New Point(s.Width, s.Height), My.Resources.Strings.Breite & ":", My.Resources.Strings.Höhe & ":"))
        l.AddRange(MyBase.getEinstellungen(sender))
        addEinstellungenStrokeFill(sender, l)
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Pos AndAlso e.Name = My.Resources.Strings.Einstellung_Größe Then
                With DirectCast(e, Einstellung_Pos)
                    If .changedX Then
                        Me.s.Width = .pos.X
                        changed = True
                    End If
                    If .changedY Then
                        Me.s.Height = .pos.Y
                        changed = True
                    End If
                    If .changedX OrElse .changedY Then
                        OnSizeChanged(OnSizeChangedReason.SetEinstellungen)
                    End If
                End With
            End If
        Next
        Return changed
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_BasicRectBased Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, Element_BasicRectBased)
            If Me.position <> .position Then Return False
            If Me.s <> .s Then Return False
            If Me.linestyle <> .linestyle Then Return False
            If Me.fillstyle <> .fillstyle Then Return False
        End With
        Return True
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        Dim p1 As Point = Me.position
        Dim p2 As Point = New Point(Me.position.X + s.Width, Me.position.Y + s.Height)

        p1.X -= drehpunkt.X
        p1.Y -= drehpunkt.Y
        p2.X -= drehpunkt.X
        p2.Y -= drehpunkt.Y

        p1 = drehung.transformPoint(p1)
        p2 = drehung.transformPoint(p2)

        p1.X += drehpunkt.X
        p1.Y += drehpunkt.Y
        p2.X += drehpunkt.X
        p2.Y += drehpunkt.Y

        Dim minX As Integer = Math.Min(p1.X, p2.X)
        Dim minY As Integer = Math.Min(p1.Y, p2.Y)
        Dim maxX As Integer = Math.Max(p1.X, p2.X)
        Dim maxY As Integer = Math.Max(p1.Y, p2.Y)

        Me.position = New Point(minX, minY)
        Me.s = New Size(maxX - minX, maxY - minY)
        Me.OnSizeChanged(OnSizeChangedReason.Drehen)
    End Sub

    Public Overrides Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        Select Case kante.KantenIndex
            Case 0 'left
                Me.position = New Point(Me.position.X + dx, Me.position.Y)
                Me.s.Width -= dx

                If Me.s.Width < 0 Then
                    Me.position = New Point(Me.position.X + Me.s.Width, Me.position.Y)
                    Me.s.Width = Math.Abs(Me.s.Width)
                    'neue kante ist rechts!
                    kante.KantenIndex = 2
                End If

            Case 1 'bottom
                Me.s.Height += dy

                If Me.s.Height < 0 Then
                    Me.position = New Point(Me.position.X, Me.position.Y + Me.s.Height)
                    Me.s.Height = Math.Abs(Me.s.Height)

                    'neue kante ist oben
                    kante.KantenIndex = 3
                End If
            Case 2 'right
                Me.s.Width += dx

                If Me.s.Width < 0 Then
                    Me.position = New Point(Me.position.X + Me.s.Width, Me.position.Y)
                    Me.s.Width = Math.Abs(Me.s.Width)
                    'neue kante ist links!
                    kante.KantenIndex = 0
                End If

            Case 3 'top
                Me.position = New Point(Me.position.X, Me.position.Y + dy)
                Me.s.Height -= dy

                If Me.s.Height < 0 Then
                    Me.position = New Point(Me.position.X, Me.position.Y + Me.s.Height)
                    Me.s.Height = Math.Abs(Me.s.Height)

                    'neue kante ist unten
                    kante.KantenIndex = 1
                End If
            Case Else
                Return False
        End Select
        out_invalidate_screen = True
        Me.OnSizeChanged(OnSizeChangedReason.Skalieren)
        Return True
    End Function

    Public Overrides Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        Select Case index
            Case 0 'left
                Return New Kante(Me.position, New Point(position.X, position.Y + s.Height), 0, False, Me, False)
            Case 1 'bottom
                Return New Kante(New Point(position.X, position.Y + s.Height), New Point(position.X + s.Width, position.Y + s.Height), 1, False, Me, False)
            Case 2 'right
                Return New Kante(New Point(position.X + s.Width, position.Y + s.Height), New Point(position.X + s.Width, position.Y), 2, False, Me, False)
            Case 3 'top
                Return New Kante(New Point(position.X + s.Width, position.Y), Me.position, 3, False, Me, False)
            Case Else
                Throw New IndexOutOfRangeException("Das Rechteck hat nur 4 Kanten.")
        End Select
    End Function

    Public Overrides Function getScaleKantenCount() As Integer
        Return 4
    End Function

    Public Overrides Function Hat_Fillstyle() As Boolean
        Return True
    End Function

    Protected Enum OnSizeChangedReason
        Konstruktor
        SetEinstellungen
        Skalieren
        Drehen
    End Enum
End Class
