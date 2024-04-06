Imports System.Drawing.Drawing2D
''' <summary>
''' Sortiert die Eigenschaften eines Andockbereichs( Links, rechts). Besitzt mehrere TabElemente
''' </summary>
''' <remarks></remarks>
Public Class DockStack
    Public MinimumContentheight As Integer = 30
    Public Margin As Integer = 2
    Private _visible As Boolean = True
    Public IconsOnly As Boolean

    Private style As DesignStyle

    Public ReadOnly Property Visible As Boolean
        Get
            Return _visible
        End Get
    End Property
    Private _breite As Integer

    Public Property breite As Integer
        Get
            Return _breite
        End Get
        Set(ByVal value As Integer)
            _breite = value
            If Visiblecollection IsNot Nothing AndAlso Visiblecollection.Count > 0 Then
                For i As Integer = 0 To Visiblecollection.Count - 1
                    Visiblecollection(i).breite = value
                Next
            End If
        End Set
    End Property
    Public Visiblecollection As List(Of TabElement)
    Public Invisiblecollection As List(Of TabElement)
    Private ParentPanel As Panel
    Private Höhe As Integer

    Private Höhenoffset As Integer = 24
    Public Sub New(style As DesignStyle)
        Me.style = style

        Visiblecollection = New List(Of TabElement)
        Invisiblecollection = New List(Of TabElement)
    End Sub
    Public Function getPanel(ByVal höhe As Integer) As Panel
        Dim P As New Panel()
        P.Size = New Size(Me.breite, höhe)
        ParentPanel = P
        Me.Höhe = höhe
        designLayout()
        Return P
    End Function
    Private Function summe(ByVal arr() As Integer) As Integer
        Dim sum As Integer = 0
        For i As Integer = 0 To arr.Length - 1
            sum += arr(i)
        Next
        Return sum
    End Function
    Protected Sub TabElementLosesAllElements(ByVal sender As Object, ByVal e As EventArgs)
        Dim nr As Integer = Visiblecollection.IndexOf(CType(sender, TabElement))
        If nr <> -1 Then
            RemoveHandler Visiblecollection(nr).VisibleChanged, AddressOf TabVisibleChanged
            RemoveHandler Visiblecollection(nr).IfIsLastIsQuestioned, AddressOf TabElement_IfIsLastIsQuestioned
            RemoveHandler Visiblecollection(nr).MoveTo, AddressOf TabMoves
            RemoveHandler Visiblecollection(nr).MovingComplete, AddressOf TabMovingComplete
            RemoveHandler Visiblecollection(nr).MovingStarts, AddressOf TabMoveStarts
            Visiblecollection.RemoveAt(nr)
            nr = Invisiblecollection.IndexOf(CType(sender, TabElement))
            Invisiblecollection.RemoveAt(nr)
            If Visiblecollection.Count > 0 Then
                Me.ParentPanel.Controls.Clear()
                designLayout()
            Else
                OnLostAllElements()
            End If
        Else
            nr = Invisiblecollection.IndexOf(CType(sender, TabElement))
            If nr <> -1 Then
                RemoveHandler Invisiblecollection(nr).VisibleChanged, AddressOf TabVisibleChanged
                RemoveHandler Invisiblecollection(nr).IfIsLastIsQuestioned, AddressOf TabElement_IfIsLastIsQuestioned
                RemoveHandler Invisiblecollection(nr).MoveTo, AddressOf TabMoves
                RemoveHandler Invisiblecollection(nr).MovingComplete, AddressOf TabMovingComplete
                RemoveHandler Invisiblecollection(nr).MovingStarts, AddressOf TabMoveStarts
                Invisiblecollection.RemoveAt(nr)
            End If
        End If
        RemoveHandler CType(sender, TabElement).FensterHinzufügen, AddressOf OnFensterHinzufügen
    End Sub
    Protected Sub OnLostAllElements()
        RaiseEvent LostAllElements(Me, EventArgs.Empty)
    End Sub
    Protected Sub OnFensterHinzufügen(ByVal sender As Object, ByVal fenster As Window)
        RaiseEvent FensterHinzufügen(Me, fenster)
    End Sub

    Public Event FensterHinzufügen(ByVal sender As Object, ByVal fenster As Window)
    Public Event LostAllElements(ByVal sender As Object, ByVal e As EventArgs)

    Private OverPic As PictureBox
    Private InsertNr As Integer
    Public Function IsLastElement(ByVal sender As TabElement) As Boolean
        Return Me.Visiblecollection.IndexOf(sender) = Me.Visiblecollection.Count - 1
    End Function
    Public Sub OnFensterOver(ByVal Renderpanel As Panel)
        If OverPic Is Nothing Then
            Dim p1 As Point = Renderpanel.PointToClient(Cursor.Position)
            Dim nr As Integer = Visiblecollection.Count
            For i As Integer = 0 To Visiblecollection.Count - 1
                If p1.Y < Visiblecollection(i).headerPanel.Parent.Location.Y + Visiblecollection(i).headerPanel.Parent.Height / 2 Then
                    nr = i
                    Exit For
                End If
            Next
            InsertNr = nr

            Dim höheElement As Integer = CInt(Renderpanel.Height / (Visiblecollection.Count + 1))

            OverPic = New PictureBox
            OverPic.Size = Renderpanel.Size
            OverPic.Location = New Point(0, 0)
            OverPic.Image = New Bitmap(OverPic.Width, OverPic.Height)
            Renderpanel.DrawToBitmap(CType(OverPic.Image, Bitmap), New Rectangle(0, 0, OverPic.Width, OverPic.Height))
            Using g As Graphics = Graphics.FromImage(OverPic.Image)
                If nr = 0 Then
                    Dim l As New LinearGradientBrush(New Rectangle(0, 0, Renderpanel.Width, höheElement), DockStackInsertColor1, DockStackInsertColor2, LinearGradientMode.Vertical)
                    g.FillRectangle(l, 0, 0, Renderpanel.Width, höheElement)
                ElseIf nr = Visiblecollection.Count Then
                    Dim l As New LinearGradientBrush(New Rectangle(0, Renderpanel.Height - höheElement, Renderpanel.Width, höheElement), DockStackInsertColor1, DockStackInsertColor2, LinearGradientMode.Vertical)
                    g.FillRectangle(l, 0, Renderpanel.Height - höheElement, Renderpanel.Width, höheElement)
                Else
                    Dim l As New LinearGradientBrush(New Rectangle(0, CInt(Visiblecollection(nr - 1).headerPanel.Parent.Bottom + Margin / 2 - höheElement / 2), Renderpanel.Width, höheElement), DockStackInsertColor1, DockStackInsertColor2, LinearGradientMode.Vertical)
                    g.FillRectangle(l, 0, CInt(Visiblecollection(nr - 1).headerPanel.Parent.Bottom + Margin / 2 - höheElement / 2), Renderpanel.Width, höheElement)
                End If
            End Using
            OverPic.Parent = Renderpanel
            OverPic.BringToFront()
        Else
            Dim p1 As Point = Renderpanel.PointToClient(Cursor.Position)
            Dim nr As Integer = Visiblecollection.Count
            For i As Integer = 0 To Visiblecollection.Count - 1
                If p1.Y < Visiblecollection(i).headerPanel.Parent.Location.Y + Visiblecollection(i).headerPanel.Parent.Height / 2 Then
                    nr = i
                    Exit For
                End If
            Next
            If nr <> InsertNr Then
                OnFensterLeave(Renderpanel)
                OnFensterOver(Renderpanel)
            End If
        End If
    End Sub
    Public Sub OnFensterLeave(ByVal renderpanel As Panel)
        If OverPic IsNot Nothing Then
            renderpanel.Parent.Controls.Remove(OverPic)
            OverPic.Dispose()
            OverPic = Nothing
        End If
    End Sub
    Public Sub Insert(ByVal t As TabElement, ByVal renderPanel As Panel)
        t.nichtTabVerschieben = False
        If OverPic IsNot Nothing Then
            OnFensterLeave(renderPanel)
            If InsertNr <= Visiblecollection.Count Then
                Visiblecollection.Insert(InsertNr, t)
                If InsertNr = 0 Then
                    Invisiblecollection.Insert(0, t)
                Else
                    Invisiblecollection.Insert(Invisiblecollection.IndexOf(Visiblecollection(InsertNr - 1)) + 1, t)
                End If
                t.breite = Me.breite
                t.NurIconsAnzeigen = IconsOnly
                t.NichtContentRendern = False
                neuertab_schreibeHandlers(t)
                For Each c As Content In t.ContentList
                    c.FavoriteWidth = t.CurentSize
                Next
            End If
        End If
    End Sub

#Region "Moving"
    Private zieherPBox As PictureBox
    Private zieherStartPos As Integer
    Private Sub TabMovingComplete(ByVal sender As Object, ByVal e As movingeventargs)
        If Visiblecollection.IndexOf(DirectCast(sender, TabElement)) < Visiblecollection.Count - 1 Then
            If zieherStartPos = -1 OrElse zieherStartPos = zieherPBox.Location.Y Then
                Me.ParentPanel.Controls.Remove(zieherPBox)
                Return
            End If
            Cursor.Current = Cursors.Default
            Dim pos As Integer = ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, e.Höhe))).Y
            If e.Höhe < MinimumContentheight Then
                e.Höhe -= pos - ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, MinimumContentheight))).Y
                pos = ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, MinimumContentheight))).Y
            End If
            Dim nr As Integer = Visiblecollection.IndexOf(DirectCast(sender, TabElement))
            Dim höheunten As Integer = Visiblecollection(nr + 1).Contentpanel.PointToClient(ParentPanel.PointToScreen(New Point(0, pos))).Y
            If nr = 0 Then
                If Visiblecollection(nr + 1).Contentpanel.Height - höheunten - Höhenoffset < MinimumContentheight Then
                    e.Höhe -= pos - ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset))).Y
                    pos = ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset))).Y
                End If
            Else
                If Visiblecollection(nr + 1).Contentpanel.Height - höheunten - Höhenoffset - Margin < MinimumContentheight Then
                    e.Höhe -= pos - ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset - Margin))).Y
                    pos = ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset - Margin))).Y
                End If
            End If

            Me.ParentPanel.Controls.Remove(zieherPBox)

            Dim höhe1 As Integer = e.Höhe - 4 'Minus 4, weil das Contentpanel aufgrund des blauen randes 4 größer ist als der Inhalt!!!
            Dim höhe2 As Integer = Visiblecollection(nr).CurentSize + Visiblecollection(nr + 1).CurentSize - Höhenoffset * 2 - Margin - höhe1 + 4

            Visiblecollection(nr).anzeigeheight = höhe1
            If nr > 0 Then
                Visiblecollection(nr).CurentSize = höhe1 + Höhenoffset + Margin
            Else
                Visiblecollection(nr).CurentSize = höhe1 + Höhenoffset
            End If
            Visiblecollection(nr + 1).anzeigeheight = höhe2
            Visiblecollection(nr + 1).CurentSize = höhe2 + Höhenoffset + Margin
            RerangeLayout(Me.Höhe, False)
        End If
    End Sub
    Private Sub TabMoves(ByVal sender As Object, ByVal e As movingeventargs)
        If Visiblecollection.IndexOf(DirectCast(sender, TabElement)) < Visiblecollection.Count - 1 Then
            Dim pos As Integer = ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, e.Höhe))).Y
            If e.Höhe < MinimumContentheight Then
                pos = ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, MinimumContentheight))).Y
            End If
            Dim nr As Integer = Visiblecollection.IndexOf(DirectCast(sender, TabElement))
            Dim höheunten As Integer = Visiblecollection(nr + 1).Contentpanel.PointToClient(ParentPanel.PointToScreen(New Point(0, pos))).Y
            If nr = 0 Then
                If Visiblecollection(nr + 1).Contentpanel.Height - höheunten - Höhenoffset < MinimumContentheight Then
                    pos = ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset))).Y
                End If
            Else
                If Visiblecollection(nr + 1).Contentpanel.Height - höheunten - Höhenoffset - Margin < MinimumContentheight Then
                    pos = ParentPanel.PointToClient(Visiblecollection(nr + 1).Contentpanel.PointToScreen(New Point(0, Visiblecollection(nr + 1).Contentpanel.Height - MinimumContentheight - Höhenoffset - Margin))).Y
                End If
            End If
            zieherPBox.Location = New Point(0, pos)
            If zieherStartPos = -1 Then zieherStartPos = zieherPBox.Location.Y
        End If
    End Sub
    Private Sub TabMoveStarts(ByVal sender As Object, ByVal e As EventArgs)
        If Visiblecollection.IndexOf(DirectCast(sender, TabElement)) < Visiblecollection.Count - 1 Then
            zieherPBox = New PictureBox()
            zieherPBox.Location = New Point(0, ParentPanel.PointToClient(DirectCast(sender, TabElement).Contentpanel.PointToScreen(New Point(0, DirectCast(sender, TabElement).Contentpanel.Bottom))).Y - 1)
            Cursor.Current = Cursors.HSplit

            zieherPBox.Size = New Size(DirectCast(sender, TabElement).breite, 2)
            zieherPBox.BackColor = style.TabControlResizeColor
            zieherPBox.Parent = ParentPanel
            zieherPBox.BringToFront()
            zieherStartPos = -1
        End If
    End Sub
#End Region

    Public Sub clear(ohneEvents As Boolean)
        For Each t As TabElement In Visiblecollection
            t.clear()
            RemoveHandler t.VisibleChanged, AddressOf TabVisibleChanged
            RemoveHandler t.IfIsLastIsQuestioned, AddressOf TabElement_IfIsLastIsQuestioned
            RemoveHandler t.MoveTo, AddressOf TabMoves
            RemoveHandler t.MovingComplete, AddressOf TabMovingComplete
            RemoveHandler t.MovingStarts, AddressOf TabMoveStarts
        Next
        For Each t As TabElement In Invisiblecollection
            t.clear()
            RemoveHandler t.VisibleChanged, AddressOf TabVisibleChanged
            RemoveHandler t.IfIsLastIsQuestioned, AddressOf TabElement_IfIsLastIsQuestioned
            RemoveHandler t.MoveTo, AddressOf TabMoves
            RemoveHandler t.MovingComplete, AddressOf TabMovingComplete
            RemoveHandler t.MovingStarts, AddressOf TabMoveStarts
        Next
        Visiblecollection.Clear()
        Invisiblecollection.Clear()
        If Not ohneEvents Then OnLostAllElements()
    End Sub

    Private Sub designLayout()
        If Visiblecollection IsNot Nothing AndAlso Visiblecollection.Count > 0 Then
            If ParentPanel IsNot Nothing Then
                ParentPanel.Controls.Clear()
                For i As Integer = 0 To Visiblecollection.Count - 1
                    Visiblecollection(i).getPanel(300).Parent = Me.ParentPanel
                Next
                RerangeLayout(Me.Höhe, True)
            End If
        Else
            OnLostAllElements()
        End If
    End Sub
    Public Sub RerangeLayout(ByVal höhe As Integer, ByVal mitGrößeÄndern As Boolean)
        Me.Höhe = höhe
        If Visiblecollection IsNot Nothing AndAlso Visiblecollection.Count > 0 Then
            Dim höhen(Visiblecollection.Count - 1) As Integer
            Dim letztepos As Integer = 0
            Dim anzahlrestkonsumenten As Integer = 0
            For i As Integer = 0 To Visiblecollection.Count - 1
                höhen(i) = Visiblecollection(i).GetMaxHeight
                If höhen(i) = Integer.MaxValue Then
                    höhen(i) = MinimumContentheight
                    anzahlrestkonsumenten += 1
                End If
                If i = 0 Then
                    höhen(i) += Höhenoffset
                Else
                    höhen(i) += Höhenoffset + Margin
                End If
            Next
            If summe(höhen) <= höhe Then
                Dim rest As Integer = höhe - summe(höhen)
                If anzahlrestkonsumenten > 0 Then
                    Dim offsetklein As Double = rest / anzahlrestkonsumenten

                    For i As Integer = 0 To höhen.Length - 1
                        If Visiblecollection(i).GetMaxHeight = Integer.MaxValue Then
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) + offsetklein))
                            Else
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) + offsetklein - Margin))
                            End If
                        Else
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = höhen(i)
                            Else
                                ParentPanel.Controls(i).Height = höhen(i) - Margin
                            End If
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Top
                        If i > 0 Then
                            ParentPanel.Controls(i).Location = New Point(0, letztepos + Margin)
                        Else
                            ParentPanel.Controls(i).Location = New Point(0, letztepos)
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
                        Visiblecollection(i).CurentSize = ParentPanel.Controls(i).Height
                        letztepos = ParentPanel.Controls(i).Bottom
                    Next
                Else
                    anzahlrestkonsumenten = 0
                    For i As Integer = 0 To Visiblecollection.Count - 1
                        If Not Visiblecollection(i).HatMaxSize Then
                            anzahlrestkonsumenten += 1
                        End If
                    Next
                    Dim offsetklein As Double = rest / anzahlrestkonsumenten
                    For i As Integer = 0 To höhen.Length - 1
                        If Not Visiblecollection(i).HatMaxSize Then
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) + offsetklein))
                            Else
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) + offsetklein - Margin))
                            End If
                        Else
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = höhen(i)
                            Else
                                ParentPanel.Controls(i).Height = höhen(i) - Margin
                            End If
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Top
                        If i > 0 Then
                            ParentPanel.Controls(i).Location = New Point(0, letztepos + Margin)
                        Else
                            ParentPanel.Controls(i).Location = New Point(0, letztepos)
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
                        Visiblecollection(i).CurentSize = ParentPanel.Controls(i).Height
                        letztepos = ParentPanel.Controls(i).Bottom
                    Next
                End If
            Else
                Dim changeblVarlist As New List(Of TabElement)
                For i As Integer = 0 To Visiblecollection.Count - 1
                    If Not Visiblecollection(i).HatMaxSize And Visiblecollection(i).GetMaxHeight <> Integer.MaxValue Then
                        changeblVarlist.Add(Visiblecollection(i))
                    End If
                Next
                If changeblVarlist.Count > 0 Then
                    Dim rest As Integer = summe(höhen) - höhe
                    anzahlrestkonsumenten = 0
                    For i As Integer = 0 To Visiblecollection.Count - 1
                        If Not Visiblecollection(i).HatMaxSize Then
                            anzahlrestkonsumenten += 1
                        End If
                    Next
                    Dim offsetklein As Double = rest / anzahlrestkonsumenten
                    Dim abzugandere As Integer = 0
                    For i As Integer = 0 To höhen.Length - 1
                        If Not Visiblecollection(i).HatMaxSize Then
                            Dim h As Integer = CInt(höhen(i) - offsetklein)
                            If h < MinimumContentheight + Höhenoffset Then
                                abzugandere = MinimumContentheight + Höhenoffset - h
                                h = MinimumContentheight + Höhenoffset
                            End If
                            höhen(i) = h
                        End If
                    Next
                    offsetklein = abzugandere / (Visiblecollection.Count - anzahlrestkonsumenten)
                    For i As Integer = 0 To höhen.Length - 1
                        If Not Visiblecollection(i).HatMaxSize Then
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = höhen(i)
                            Else
                                ParentPanel.Controls(i).Height = höhen(i) - Margin
                            End If
                        Else
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = CInt(höhen(i) - offsetklein)
                            Else
                                ParentPanel.Controls(i).Height = CInt(höhen(i) - offsetklein - Margin)
                            End If
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Top
                        If i > 0 Then
                            ParentPanel.Controls(i).Location = New Point(0, letztepos + Margin)
                        Else
                            ParentPanel.Controls(i).Location = New Point(0, letztepos)
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
                        Visiblecollection(i).CurentSize = ParentPanel.Controls(i).Height
                        letztepos = ParentPanel.Controls(i).Bottom
                    Next
                Else
                    Dim varheight As Integer = 0
                    Dim mussheight As Integer = 0
                    For i As Integer = 0 To höhen.Length - 1
                        If höhen(i) > MinimumContentheight + Höhenoffset + Margin And (Visiblecollection(i).anzeigeheight = -1 Or mitGrößeÄndern) Then
                            varheight += höhen(i)
                        Else
                            mussheight += höhen(i)
                        End If
                    Next
                    Dim faktor As Double = (höhe - mussheight) / varheight
                    For i As Integer = 0 To höhen.Length - 1
                        If höhen(i) > MinimumContentheight + Höhenoffset + Margin And (Visiblecollection(i).anzeigeheight = -1 Or mitGrößeÄndern) Then
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) * faktor))
                            Else
                                ParentPanel.Controls(i).Height = CInt(Math.Truncate(höhen(i) * faktor) - Margin)
                            End If
                        Else
                            If i = 0 Then
                                ParentPanel.Controls(i).Height = höhen(i)
                            Else
                                ParentPanel.Controls(i).Height = höhen(i) - Margin
                            End If
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Top
                        If i > 0 Then
                            ParentPanel.Controls(i).Location = New Point(0, letztepos + Margin)
                        Else
                            ParentPanel.Controls(i).Location = New Point(0, letztepos)
                        End If
                        ParentPanel.Controls(i).Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
                        Visiblecollection(i).CurentSize = ParentPanel.Controls(i).Height
                        letztepos = ParentPanel.Controls(i).Bottom
                    Next
                End If
            End If
            ParentPanel.Controls(ParentPanel.Controls.Count - 1).Height = höhe - ParentPanel.Controls(ParentPanel.Controls.Count - 1).Location.Y
            Visiblecollection(Visiblecollection.Count - 1).CurentSize = ParentPanel.Controls(ParentPanel.Controls.Count - 1).Height
        Else
            OnLostAllElements()
        End If
    End Sub

    Public Sub add(ByVal Content As Panel, ByVal hatmaxsize As Boolean)
        Dim t As New TabElement(style, Content, hatmaxsize)
        t.NurIconsAnzeigen = IconsOnly
        t.nichtTabVerschieben = False
        Visiblecollection.Add(t)
        Invisiblecollection.Add(t)
        t.breite = Me.breite
        t.NichtContentRendern = False
        neuertab_schreibeHandlers(t)
    End Sub
    Public Sub add(ByVal c As Content)
        Dim t As New TabElement(style)
        t.add(c)
        c.FavoriteWidth = t.CurentSize
        t.NurIconsAnzeigen = IconsOnly
        t.NichtContentRendern = False
        t.nichtTabVerschieben = False
        Visiblecollection.Add(t)
        Invisiblecollection.Add(t)
        t.breite = Me.breite
        neuertab_schreibeHandlers(t)
    End Sub

    Public Sub StyleChanged()
        For Each t As TabElement In Invisiblecollection
            t.Stylechanged()
        Next
    End Sub

    Private Sub TabElement_IfIsLastIsQuestioned(ByVal sender As Object, ByVal e As callbackeventargs)
        e.callbackDel.Invoke(sender.Equals(Visiblecollection(Visiblecollection.Count - 1)))
    End Sub
    Private Sub neuertab_schreibeHandlers(ByVal t As TabElement)
        AddHandler t.LostAllContentElements, AddressOf TabElementLosesAllElements
        AddHandler t.FensterHinzufügen, AddressOf OnFensterHinzufügen
        AddHandler t.MovingStarts, AddressOf TabMoveStarts
        AddHandler t.MoveTo, AddressOf TabMoves
        AddHandler t.MovingComplete, AddressOf TabMovingComplete
        AddHandler t.IfIsLastIsQuestioned, AddressOf TabElement_IfIsLastIsQuestioned
        AddHandler t.VisibleChanged, AddressOf TabVisibleChanged
    End Sub
    Private Sub TabVisibleChanged(ByVal sender As Object, ByVal e As EventArgs)
        If TryCast(sender, TabElement).Visible = False Then
            Visiblecollection.Remove(DirectCast(sender, TabElement))
            If Visiblecollection.Count = 0 Then
                Me._visible = False
                RaiseEvent VisibleChanged(Me, EventArgs.Empty)
            Else
                designLayout()
            End If
        Else
            Visiblecollection.Clear()
            For i As Integer = 0 To Invisiblecollection.Count - 1
                If Invisiblecollection(i).Visible Then
                    Visiblecollection.Add(Invisiblecollection(i))
                End If
            Next
            DirectCast(sender, TabElement).NichtContentRendern = False
            DirectCast(sender, TabElement).breite = Me.breite
            designLayout()
            If Me._visible = False Then Me._visible = True : RaiseEvent VisibleChanged(Me, EventArgs.Empty)
        End If
    End Sub
    Public Sub add(ByVal t As TabElement)
        Visiblecollection.Add(t)
        Invisiblecollection.Add(t)
        t.NichtContentRendern = False
        t.nichtTabVerschieben = False
        t.NurIconsAnzeigen = Me.IconsOnly
        t.breite = Me.breite
        neuertab_schreibeHandlers(t)
        For Each c As Content In t.ContentList
            c.FavoriteWidth = t.CurentSize
        Next
    End Sub

    Public Sub addOhneEvents(ByVal t As TabElement)
        If t.Visible Then Visiblecollection.Add(t)
        Invisiblecollection.Add(t)
        t.NichtContentRendern = False
        t.nichtTabVerschieben = False
        t.NurIconsAnzeigen = Me.IconsOnly
        t.breite = Me.breite
        neuertab_schreibeHandlers(t)
        For Each c As Content In t.ContentList
            c.FavoriteWidth = t.CurentSize
        Next
        evaluiereVisible()
    End Sub

    Public Event VisibleChanged(ByVal sender As Object, ByVal e As EventArgs)

    Public Sub UpdateContentAvailableChanges()
        Dim changes(Invisiblecollection.Count - 1) As TabElementContentAvailableChangeUntersuchung
        Dim mussNeuRendern As Boolean = False
        For i As Integer = 0 To Invisiblecollection.Count - 1
            changes(i) = Invisiblecollection(i).UntersucheContentAvailableChanges()
            If changes(i).LostAllElements AndAlso Visiblecollection.Contains(Invisiblecollection(i)) Then
                Visiblecollection.Remove(Invisiblecollection(i))
                mussNeuRendern = True
            ElseIf changes(i).AnzahlVisibleNeu = 0 AndAlso Visiblecollection.Contains(Invisiblecollection(i)) Then
                'verbugter zustand: eigentlich müsste auch LostAllElements geraist werden!
                Visiblecollection.Remove(Invisiblecollection(i))
                mussNeuRendern = True
            ElseIf changes(i).WurdeVisibleTrue AndAlso Not Visiblecollection.Contains(Invisiblecollection(i)) Then
                'nur add geht hier nicht, da sonst die Reihenfolge nicht mehr stimmt!
                Dim index As Integer = -1
                For k As Integer = i + 1 To Invisiblecollection.Count - 1
                    index = Visiblecollection.IndexOf(Invisiblecollection(k))
                    If index <> -1 Then
                        Visiblecollection.Insert(index, Invisiblecollection(i))
                        Exit For
                    End If
                Next
                If index = -1 Then
                    Visiblecollection.Add(Invisiblecollection(i))
                End If
                mussNeuRendern = True
            End If
        Next

        For i As Integer = 0 To Invisiblecollection.Count - 1
            Invisiblecollection(i).WendeChangesAn(changes(i))
        Next

        If Visiblecollection.Count = 0 AndAlso _visible Then
            'gesamtvisibility changed to false
            Me._visible = False
            RaiseEvent VisibleChanged(Me, EventArgs.Empty)
        ElseIf Visiblecollection.Count > 0 AndAlso Not _visible Then
            'gesamtvisibility changed to true
            Me._visible = True
            RaiseEvent VisibleChanged(Me, EventArgs.Empty)
        ElseIf mussNeuRendern Then
            designLayout()
        End If
    End Sub

    Public Sub evaluiereVisible()
        Me._visible = (Me.Visiblecollection.Count > 0)
    End Sub

End Class

