Option Strict Off
Imports System.Drawing.Drawing2D
Imports System.IO

''' <summary>
''' Sortiert mehrere, bzw. ein Content-Element horizontal
''' </summary>
''' <remarks></remarks>
Public Class TabElement
    Private _breite As Integer
    Public Property Aktualisierebreite As Boolean = True
    Public Property breite As Integer
        Get
            Return _breite
        End Get
        Set(ByVal value As Integer)
            If _breite <> value Then
                _breite = value
                If Aktualisierebreite AndAlso headerPanel IsNot Nothing AndAlso headerPanel.Controls.Count > 0 Then
                    headerRendern()
                End If
            End If
        End Set
    End Property

    Public Sub setBreiteOhneEvents(breiteNeu As Integer)
        Me._breite = breiteNeu
    End Sub

    Public NichtContentRendern As Boolean = False
    Public ContentList As List(Of Content)
    Public Contentpanel As Panel
    Public headerPanel As FlowLayoutPanel
    Public headers() As HeaderButton

    Public Property selectedIndex As Integer = 0

    Private HerausziehAbstand As Integer = 30
    Public CurentSize As Integer
    Private _visible As Boolean = True
    Private nichtVisibleDurchführen As Boolean = False
    Private _iconOnly As Boolean = False

    Private style As DesignStyle

    Public Property NurIconsAnzeigen As Boolean
        Get
            Return _iconOnly
        End Get
        Set(ByVal value As Boolean)
            If value <> _iconOnly Then
                _iconOnly = value
                If headers IsNot Nothing Then
                    headerLöschen()
                    headerRendern()
                End If
            End If
        End Set
    End Property
    Public Property Visible As Boolean
        Get
            Return _visible
        End Get
        Set(ByVal value As Boolean)
            _visible = value
            nichtVisibleDurchführen = True
            For i As Integer = 0 To ContentList.Count - 1
                ContentList(i).Visible = value
            Next
            nichtVisibleDurchführen = False
            RaiseEvent VisibleChanged(Me, EventArgs.Empty)
        End Set
    End Property

    Public Sub setVisibleOhneEvents(visible As Boolean)
        Me._visible = visible
    End Sub

    Public Sub New(style As DesignStyle)
        Me.style = style
        ContentList = New List(Of Content)
    End Sub
    Public Sub New(style As DesignStyle, ByVal P As Panel, ByVal hatmaxsize As Boolean)
        Me.New(style)
        Dim c As New Content(P, hatmaxsize, 300, 300)
        ContentList.Add(c)
        addhandlers(c)
    End Sub

    Public Sub add(ByVal P As Panel, ByVal hatmaxsize As Boolean)
        Dim c As New Content(P, hatmaxsize, 300, 300)
        ContentList.Add(c)
        addhandlers(c)
    End Sub
    Public Sub add(ByVal c As Content)
        ContentList.Add(c)
        If Me.breite <> 0 Then c.FavoriteWidth = Me.breite
        addhandlers(c)
    End Sub
    Public Sub addhandlers(ByVal c As Content)
        AddHandler c.VisibleChanged, AddressOf ContentvisibleChanged
    End Sub

    Private Sub ContentvisibleChanged(ByVal sender As Object, ByVal e As VisibleChangedEventArgs)
        If nichtVisibleDurchführen = False Then
            Dim nr As Integer = ContentList.IndexOf(sender)
            Dim visible As Boolean = False
            Dim anzahl As Integer = 0
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).Visible = True Then
                    visible = True
                    anzahl += 1
                End If
            Next

            If anzahl = 1 Then
                RaiseEvent LostElement(Me, EventArgs.Empty)
            End If
            If anzahl = 2 Then
                RaiseEvent LostElement(Me, EventArgs.Empty)
            End If

            If visible <> _visible Then
                If _visible = False Then
                    _visible = visible
                    selectedIndex = nr
                    OnSelectedIndexChanged()
                    If headerPanel IsNot Nothing Then
                        headerLöschen()
                        headerRendern()
                    End If
                    RaiseEvent VisibleChanged(Me, EventArgs.Empty)
                    Exit Sub
                Else
                    _visible = visible
                    RaiseEvent VisibleChanged(Me, EventArgs.Empty)
                    Exit Sub
                End If
            End If

            If CType(sender, Content).Visible = True Then
                If Not e.DontSelectContent Then
                    selectedIndex = nr
                End If
                OnSelectedIndexChanged()
            Else
                If nr = selectedIndex Then
                    Dim succes As Boolean = False
                    For i As Integer = nr - 1 To 0 Step -1
                        If ContentList(i).Visible Then
                            selectedIndex = i
                            succes = True
                            Exit For
                        End If
                    Next
                    If Not succes Then
                        For i As Integer = nr + 1 To ContentList.Count - 1
                            If ContentList(i).Visible Then
                                selectedIndex = i
                                Exit For
                            End If
                        Next
                    End If
                    OnSelectedIndexChanged()
                End If
            End If
            If headerPanel Is Nothing Then Return
            headerPanel.Controls.Clear()
            headerRendern()
        End If
    End Sub

    Public Sub clear()
        RemoveContentHandlers()
    End Sub

    Public Function GetMaxHeight() As Integer
        Dim max As Integer = -1
        If ContentList Is Nothing OrElse ContentList.Count = 0 Then Return -1
        For i As Integer = 0 To ContentList.Count - 1
            If ContentList(i).MaximumHeight > max Then max = ContentList(i).MaximumHeight
        Next
        If anzeigeheight <> -1 Then Return anzeigeheight
        Return max
    End Function
    Public Function getPanel(ByVal size As Integer) As Panel
        Dim höhe As Integer = size - 20
        Dim Elementbreite As Single = Me.breite / ContentList.Count

        Dim ParentPanel As New Panel
        ParentPanel.Location = New Point(0, 0)
        ParentPanel.Size = New Size(breite, size)

        Contentpanel = New Panel

        Contentpanel.Parent = ParentPanel
        Contentpanel.Location = New Point(0, 20)
        Contentpanel.Size = New Size(breite, höhe)
        Contentpanel.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right Or AnchorStyles.Bottom

        AddHandler Contentpanel.MouseMove, AddressOf ContentPanelMousMove
        AddHandler Contentpanel.MouseDown, AddressOf ContentPanelMousDown
        AddHandler Contentpanel.MouseUp, AddressOf ContentPanelMousUp
        AddHandler Contentpanel.MouseLeave, AddressOf ContentPanelMousleave

        headerPanel = New FlowLayoutPanel
        headerPanel.Location = New Point(0, 0)
        headerPanel.Parent = ParentPanel
        headerPanel.Size = New Size(ParentPanel.Width, 20)
        headerPanel.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
        headerRendern()
        Contentrendern()

        Return ParentPanel
    End Function
    Public Sub headerRendern()
        If NichtContentRendern = False Then
            If ContentList Is Nothing OrElse ContentList.Count = 0 Then Return
            Dim anzahl As Integer = ContentList.Count
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).Visible = False Then
                    anzahl -= 1
                End If
            Next
            If anzahl = 0 Then Exit Sub
            Dim breiteProTabPage As Double = breite / anzahl
            Dim btns(ContentList.Count - 1) As HeaderButton
            Dim p1 As New FlowLayoutPanel
            p1.Size = headerPanel.Size
            p1.Location = headerPanel.Location
            p1.Anchor = headerPanel.Anchor
            For i As Integer = 0 To btns.Length - 1
                btns(i) = New HeaderButton(style)
                btns(i).Name = i
                'AddHandler btns(i).GotSelection, AddressOf HeaderSelectedIndexChanged
                AddHandler btns(i).Schließen, AddressOf HeaderClosing
                AddHandler btns(i).DragDropStart, AddressOf HeaderStartDragDrop
                If _iconOnly Then
                    btns(i).HeaderName = ""
                Else
                    btns(i).HeaderName = ContentList(i).Name
                End If
                btns(i).contentName = ContentList(i).Name
                btns(i).Width = Int(breiteProTabPage)
                btns(i).headerIcon = ContentList(i).Icon
                btns(i).Margin = New Padding(0)
                If ContentList(i).Visible Then
                    btns(i).Parent = p1
                End If
            Next
            'btns(btns.Length - 1).Width = breite - btns(btns.Length - 1).Location.X
            'wichtiger bugfix: Der letzte Headerbutton kann ja auch visible = false sein!
            p1.Controls(p1.Controls.Count - 1).Width = breite - p1.Controls(p1.Controls.Count - 1).Location.X
            btns(selectedIndex).Selected = True
            Dim parent As Panel = headerPanel.Parent
            parent.Controls.Remove(headerPanel)
            headerPanel = p1
            p1.Parent = parent
            headers = btns
        End If
    End Sub
    Public Sub headerLöschen()
        If NichtContentRendern = False AndAlso headerPanel IsNot Nothing AndAlso headerPanel.Controls.Count > 0 Then
            headerPanel.Controls.Clear()
        End If
    End Sub
    Public Sub SetSelectedIndex(ByVal index As Integer)
        Me.selectedIndex = index
        If headers IsNot Nothing Then
            For i As Integer = 0 To headers.Count - 1
                If i = selectedIndex Then
                    headers(i).Selected = True
                Else
                    headers(i).Selected = False
                End If
            Next
        End If
    End Sub
    Public Sub Contentrendern()
        If Not NichtContentRendern AndAlso Me.Contentpanel IsNot Nothing Then
            Me.Contentpanel.Visible = False
            Me.Contentpanel.Controls.Clear()
            Me.Contentpanel.BackColor = style.SelectedColor
            Me.Contentpanel.Controls.Add(ContentList(selectedIndex).InhaltsPanel)
            ContentList(selectedIndex).InhaltsPanel.UseWaitCursor = ContentList(selectedIndex).InhaltsPanel.Parent.UseWaitCursor
            Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).Location = New Point(2, 2)
            Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).Size = New Size(Me.Contentpanel.Width - 4, Me.Contentpanel.Height - 4)
            Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
            Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).BackColor = style.Control

            Me.Contentpanel.Visible = True
        End If
        ' Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).Dock = DockStyle.Fill
    End Sub
    Private Function getNumberofheader(ByVal header As Object) As Integer
        Return CInt(CType(header, HeaderButton).Name)
    End Function
    Protected Sub HeaderSelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim nr As Integer = getNumberofheader(sender)
        headers(selectedIndex).Selected = False
        selectedIndex = nr
        OnSelectedIndexChanged()
    End Sub
    Protected Sub HeaderClosing(ByVal sender As Object, ByVal e As EventArgs)
        Dim nr As Integer = getNumberofheader(sender)
        Me.ContentList(nr).Visible = False
    End Sub
    Protected Sub OnLostAllContentElements()
        RaiseEvent LostAllContentElements(Me, EventArgs.Empty)
    End Sub
    Public Event LostAllContentElements(ByVal sender As Object, ByVal e As EventArgs)
    Protected Sub OnSelectedIndexChanged()
        Contentrendern()
        RaiseEvent SelectedIndexChanged(Me, EventArgs.Empty)
        ContentList(selectedIndex).OnImTabElementSelected()
    End Sub
    Public Event SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
    Public Event LostElement(ByVal sender As Object, ByVal e As EventArgs)
#Region "Drag Drop"
    Private dragDropStartPosition As Point
    Private invisibleHeaderElement As HeaderButton
    Public nichtTabVerschieben As Boolean = False
    Public Sub HeaderStartDragDrop(ByVal sender As HeaderButton, ByVal e As MouseEventArgs)
        If My.Computer.Keyboard.ShiftKeyDown = False Then
            dragDropStartPosition = New Point(e.X, e.Y)
            Dim p As New PictureBox
            p.Size = sender.Size
            p.Image = New Bitmap(sender.Width, sender.Height)
            sender.DrawToBitmap(p.Image, New Rectangle(0, 0, sender.Width, sender.Height))
            p.Location = sender.Location
            p.Parent = headerPanel.Parent
            p.BringToFront()
            dragstrecke = p.Location.X
            invisibleHeaderElement = sender
            sender.Sichtbar = False
            'Maus.MouseUp(MouseButtons.Left)
            'Maus.MouseDown(MouseButtons.Left)
            p.Capture = True

            AddHandler p.MouseMove, AddressOf PicHeaderMouseMove
            AddHandler p.MouseUp, AddressOf PicHeaderMouseUp
            AddHandler p.LostFocus, AddressOf PicHeaderMouseUp
            AddHandler p.MouseCaptureChanged, AddressOf PicHeaderMouseCaptureChanged
        Else
            dragDropStartPosition = New Point(e.X, e.Y)
            AddHandler sender.MouseMove, AddressOf HeaderMoveDragDrop
        End If
    End Sub
    Private dragstrecke As Integer = 0
    Private Sub PicHeaderMouseMove(ByVal sender As PictureBox, ByVal e As MouseEventArgs)
        If e.Button <> MouseButtons.Left Then
            PicHeaderMouseUp(sender, EventArgs.Empty) 'zur sicherheit falls mausUp nicht / zu früh geraist wurde
            Return
        End If
        Dim dx As Integer = e.X - dragDropStartPosition.X + dragstrecke
        Dim dy As Integer = e.Y - dragDropStartPosition.Y
        If dragDropStartPosition.X + dx < 0 Then
            dx = -dragDropStartPosition.X
        End If
        If dragDropStartPosition.X + dx + sender.Width > headerPanel.Width Then
            dx = headerPanel.Width - dragDropStartPosition.X - sender.Width
        End If
        dragstrecke = dx
        Dim anzahl As Integer = 0
        For i As Integer = 0 To ContentList.Count - 1
            If ContentList(i).Visible Then
                anzahl += 1
            End If
        Next
        Dim alteIndex As Integer = CInt(sender.Location.X / headerPanel.Width * anzahl)
        sender.Location = New Point(dragDropStartPosition.X + dx, sender.Location.Y)
        Dim neuerIndex As Integer = CInt(sender.Location.X / headerPanel.Width * anzahl)
        If neuerIndex <> alteIndex Then
            Dim alterIndexC As Integer = CInt(invisibleHeaderElement.Name)
            Dim neuerIndexC As Integer
            If neuerIndex = 0 Then
                neuerIndexC = 0
            Else
                Dim itemVorDiesem As String = DirectCast(headerPanel.Controls(neuerIndex - 1), HeaderButton).contentName
                For i As Integer = 0 To ContentList.Count - 1
                    If ContentList(i).Name = itemVorDiesem Then
                        neuerIndexC = i + 1
                        Exit For
                    End If
                Next
            End If
            If neuerIndexC <> alterIndexC Then
                Dim c As Content = ContentList(alterIndexC)
                ContentList.RemoveAt(alterIndexC)
                ContentList.Insert(neuerIndexC, c)
                Dim headersNeu(headers.Count - 1) As HeaderButton
                For i As Integer = 0 To ContentList.Count - 1
                    For k As Integer = 0 To headers.Length - 1
                        If headers(k).contentName = ContentList(i).Name Then
                            headersNeu(i) = headers(k)
                            Exit For
                        End If
                    Next
                    headersNeu(i).Name = CStr(i)
                Next
                headers = headersNeu
            End If
            headerPanel.Controls.SetChildIndex(invisibleHeaderElement, neuerIndex)
        End If
        If e.X < -HerausziehAbstand Or e.X > sender.Width + HerausziehAbstand Or e.Y < -HerausziehAbstand Or e.Y > sender.Height + HerausziehAbstand Then
            'In Fenster ziehen
            Dim nr As Integer = getNumberofheader(invisibleHeaderElement)
            Dim content As Content = ContentList(nr)
            Dim visible As Boolean = False
            For i As Integer = 0 To ContentList.Count - 1
                If i <> nr Then
                    If ContentList(i).Visible Then
                        visible = True
                        Exit For
                    End If
                End If
            Next
            If Not visible Then
                _visible = False
                RaiseEvent VisibleChanged(Me, EventArgs.Empty)
            End If
            Dim f As New Window(style, content)
            Dim mausPosition As Point = Cursor.Position
            f.Location = New Point(mausPosition.X - dragDropStartPosition.X, mausPosition.Y - dragDropStartPosition.Y)
            If mausPosition.X - f.Location.X < 10 Then
                f.Location = New Point(mausPosition.X - 10, f.Location.Y)
            End If
            If mausPosition.Y - f.Location.Y < 5 Then
                f.Location = New Point(f.Location.X, mausPosition.Y - 5)
            End If

            If nr = selectedIndex AndAlso nr > 0 Then
                'Änderung, Jonas, 12.2.13
                'Nachträgliche Änderung. deshalb war er bei der Startform immer abgestürtz (Beim nächsten Start), 
                'weil er es irgendwie geschafft hat, dass der selectedIndex größer war als die maximale Anzahl.
                'Das war der fall, wenn man den letzten sichtbaren herausgezogen hat, der einen größeren index hat, als ein unsichtbarer vorgänger.
                selectedIndex = nr - 1
            End If

            f.StartPosition = FormStartPosition.Manual
            f.ClientSize = New Size(headerPanel.Width, f.Height)
            RaiseEvent FensterHinzufügen(Me, f)
            'f.Show()
            Dim neueMausPosition As Point = Cursor.Position
            f.Focus()
            Maus.MouseUp(MouseButtons.Left, mausPosition.X, mausPosition.Y)
            Maus.MouseDown(MouseButtons.Left, mausPosition.X, mausPosition.Y)
            Cursor.Position = neueMausPosition
            sender.Dispose()
            headerLöschen(invisibleHeaderElement, EventArgs.Empty)
            f.Focus()
            RemoveHandler Contentpanel.MouseMove, AddressOf ContentPanelMousMove
            RemoveHandler Contentpanel.MouseDown, AddressOf ContentPanelMousDown
            RemoveHandler Contentpanel.MouseUp, AddressOf ContentPanelMousUp
            RemoveHandler Contentpanel.MouseLeave, AddressOf ContentPanelMousleave
        End If
    End Sub
    Private Sub headerLöschen(ByVal sender As Object, ByVal e As EventArgs)
        Dim nr As Integer = getNumberofheader(sender)
        RemoveHandler ContentList(nr).VisibleChanged, AddressOf ContentvisibleChanged
        Me.ContentList.RemoveAt(nr)
        If ContentList.Count > 0 Then
            If nr = ContentList.Count Then nr -= 1
            Dim succes As Boolean = False
            For i As Integer = nr To 0 Step -1
                If ContentList(i).Visible Then
                    selectedIndex = i
                    OnSelectedIndexChanged()
                    succes = True
                    Exit For
                End If
            Next
            If Not succes Then
                For i As Integer = nr To ContentList.Count - 1
                    If ContentList(i).Visible Then
                        selectedIndex = i
                        OnSelectedIndexChanged()
                        succes = True
                        Exit For
                    End If
                Next
            End If
            If succes Then
                headerLöschen()
                headerRendern()
            End If
        Else
            OnLostAllContentElements()
        End If
        RaiseEvent LostElement(Me, EventArgs.Empty)
    End Sub

    Private Sub PicHeaderMouseUp(ByVal sender As PictureBox, ByVal e As EventArgs)
        invisibleHeaderElement.Sichtbar = True
        sender.Visible = False
        headerPanel.Parent.Controls.Remove(sender)
        sender.Dispose()
        If Not invisibleHeaderElement.Selected Then
            HeaderSelectedIndexChanged(invisibleHeaderElement, EventArgs.Empty)
            invisibleHeaderElement.Selected = True
            For i As Integer = 0 To headers.Count - 1
                If Not headers(i).Equals(invisibleHeaderElement) Then
                    headers(i).Selected = False
                Else
                    selectedIndex = i
                End If
            Next
        Else
            For i As Integer = 0 To headers.Count - 1
                If headers(i).Selected Then
                    selectedIndex = i
                End If
            Next
        End If
    End Sub
    Private Sub PicHeaderMouseCaptureChanged(sender As Object, e As EventArgs)
        If sender IsNot Nothing AndAlso TypeOf sender Is PictureBox Then
            Dim pbox As PictureBox = DirectCast(sender, PictureBox)
            If pbox.Capture = False Then
                'Control hat den Maus-Capture verloren
                invisibleHeaderElement.Sichtbar = True
                pbox.Visible = False
                headerPanel.Parent.Controls.Remove(pbox)
                pbox.Dispose()
            End If
        End If
    End Sub
    Private Sub HeaderMoveDragDrop(ByVal sender As Object, ByVal e As MouseEventArgs)
        If Not nichtTabVerschieben Then
            If e.Button = MouseButtons.Left Then
                'If Math.Sqrt((e.X - dragDropStartPosition.X) ^ 2 + (e.Y - dragDropStartPosition.Y) ^ 2) > 20 Then
                Dim header As HeaderButton = TryCast(sender, HeaderButton)
                RemoveHandler header.MouseMove, AddressOf HeaderMoveDragDrop
                Dim f As New Window(style, Me)
                Dim mausposition = Cursor.Position
                f.Location = New Point(mausposition.X - dragDropStartPosition.X - sender.location.x, mausposition.Y - dragDropStartPosition.Y)
                f.StartPosition = FormStartPosition.Manual
                f.ClientSize = New Size(headerPanel.Width, f.Height)
                RaiseEvent FensterHinzufügen(Me, f)
                'f.Show()
                Dim neuePosition As Point = Cursor.Position
                f.BringToFront()
                Maus.MouseUp(MouseButtons.Left, mausposition.X, mausposition.Y)
                Maus.MouseDown(MouseButtons.Left, mausposition.X, mausposition.Y)
                Cursor.Position = neuePosition
                RaiseEvent LostAllContentElements(Me, EventArgs.Empty)
                RemoveHandler Contentpanel.MouseMove, AddressOf ContentPanelMousMove
                RemoveHandler Contentpanel.MouseDown, AddressOf ContentPanelMousDown
                RemoveHandler Contentpanel.MouseUp, AddressOf ContentPanelMousUp
                RemoveHandler Contentpanel.MouseLeave, AddressOf ContentPanelMousleave
                'End If
            End If
        End If
    End Sub
    Public Event FensterHinzufügen(ByVal sender As Object, ByVal fenster As Window)
#End Region
    Private Overpic As PictureBox
    Private InsertNr As Integer
    Public Sub OnFensterOver()
        If Overpic Is Nothing Then
            Dim p1 As Point = headerPanel.PointToClient(Cursor.Position)

            Dim item As HeaderButton = headers(headers.Count - 1)
            Dim anzahl As Integer = 0
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).Visible Then
                    anzahl += 1
                End If
            Next
            Dim nr As Integer = p1.X / breite * anzahl
            InsertNr = nr

            Dim breiteElement As Integer = headerPanel.Width / (anzahl + 1)

            Overpic = New PictureBox
            Overpic.Size = headerPanel.Parent.Size
            Overpic.Location = New Point(0, 0)
            Overpic.Image = New Bitmap(Overpic.Width, Overpic.Height)
            headerPanel.Parent.DrawToBitmap(Overpic.Image, New Rectangle(0, 0, Overpic.Width, Overpic.Height))
            Using g As Graphics = Graphics.FromImage(Overpic.Image)
                Dim l As New LinearGradientBrush(New Rectangle(0, 0, Overpic.Width, Overpic.Height), TabInsertColor1, TabInsertColor2, LinearGradientMode.Vertical)
                g.FillRectangle(l, 0, 20, Overpic.Width, Overpic.Height)
                g.FillRectangle(l, breiteElement * nr, 0, breiteElement, 20)
            End Using
            Overpic.Parent = headerPanel.Parent
            Overpic.BringToFront()
        Else
            Dim p1 As Point = headerPanel.PointToClient(Cursor.Position)
            Dim anzahl As Integer = 0
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).Visible Then
                    anzahl += 1
                End If
            Next
            Dim nr As Integer = p1.X / breite * anzahl
            If nr <> InsertNr Then
                OnFensterLeave()
                OnFensterOver()
            End If
        End If
    End Sub
    Public Sub OnFensterLeave()
        If Overpic IsNot Nothing Then
            headerPanel.Parent.Controls.Remove(Overpic)
            Overpic.Dispose()
            Overpic = Nothing
        End If
    End Sub
    Public Sub Insert(ByVal t As TabElement)
        t.nichtTabVerschieben = False
        If Overpic IsNot Nothing Then
            OnFensterLeave()
            If InsertNr <> 0 Then
                Dim anzahl As Integer = 0
                For i As Integer = 0 To ContentList.Count - 1
                    If ContentList(i).Visible Then
                        anzahl += 1
                        If anzahl = InsertNr Then
                            InsertNr = i + 1
                            Exit For
                        End If
                    End If
                Next
            End If
            Dim s As Integer = t.selectedIndex + InsertNr
            For i As Integer = 0 To t.ContentList.Count - 1
                ContentList.Insert(InsertNr + i, t.ContentList(i))
                RemoveHandler t.ContentList(i).VisibleChanged, AddressOf ContentvisibleChanged
                addhandlers(t.ContentList(i))
            Next
            SetSelectedIndex(s)
            headerRendern()
        End If
    End Sub
    Public ReadOnly Property HatMaxSize As Boolean
        Get
            Dim max As Integer = -1
            If ContentList Is Nothing OrElse ContentList.Count = 0 Then Return -1
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).MaximumHeight > max Then max = ContentList(i).MaximumHeight
            Next
            Return Not max = Integer.MaxValue
        End Get
    End Property
    Public Event VisibleChanged(ByVal sender As Object, ByVal e As EventArgs)
#Region "Verkleinern"
    Public anzeigeheight As Integer = -1
    Private Sub ContentPanelMousMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If startPosition <> -1 Then
                RaiseEvent MoveTo(Me, New movingeventargs(e.Y + sender.Height - startPosition))
            Else
                ContentPanelMousDown(sender, e)
            End If
        Else
            If e.Y > sender.Height - 3 Then
                If Not istletzteselement() Then
                    sender.Cursor = Cursors.HSplit
                End If

            Else
                sender.Cursor = Cursors.Default
            End If
        End If
    End Sub
    Private Function istletzteselement() As Boolean
        Dim erg As Boolean = True
        RaiseEvent IfIsLastIsQuestioned(Me, New callbackeventargs(Sub(ja As Boolean)
                                                                      erg = ja
                                                                  End Sub))
        Return erg
    End Function
    Public Event IfIsLastIsQuestioned(ByVal sender As Object, ByVal e As callbackeventargs)
    Private startPosition As Integer = -1
    Private Sub ContentPanelMousDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If e.Y > sender.Height - 3 Then
                startPosition = e.Y
                RaiseEvent MovingStarts(Me, EventArgs.Empty)
            End If
        End If
    End Sub
    Private Sub ContentPanelMousUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left And startPosition <> -1 Then
            RaiseEvent MovingComplete(Me, New movingeventargs(e.Y + sender.Height - startPosition))
            startPosition = -1
        End If
        sender.Cursor = Cursors.Default
    End Sub
    Private Sub ContentPanelMousleave(ByVal sender As Object, ByVal e As EventArgs)
        sender.Cursor = Cursors.Default
    End Sub
    Public Event MoveTo(ByVal sender As Object, ByVal e As movingeventargs)
    Public Event MovingComplete(ByVal sender As Object, ByVal e As movingeventargs)
    Public Event MovingStarts(ByVal sender As Object, ByVal e As EventArgs)

#End Region

    Public Sub Stylechanged()
        If Me.Contentpanel IsNot Nothing Then
            Me.Contentpanel.BackColor = style.SelectedColor
            If Me.Contentpanel.Controls.Count > 0 Then
                Me.Contentpanel.Controls(Me.Contentpanel.Controls.Count - 1).BackColor = style.Control
            End If
        End If
        If headers IsNot Nothing AndAlso headers.Length > 0 Then
            For Each h As HeaderButton In headers
                If h IsNot Nothing Then
                    h.DesignStyleChanged()
                End If
            Next
        End If
    End Sub

    Public Sub export(writer As binaryWriter)
        writer.Write(Me.ContentList.Count)
        writer.Write(Me.anzeigeheight)
        writer.Write(Me.selectedIndex)
        writer.Write(Me._visible)
        For i As Integer = 0 To Me.ContentList.Count - 1
            writer.Write(ContentList(i).Name)
            writer.Write(ContentList(i).VisibleInternal)
            writer.Write(ContentList(i).FavoriteWidth)
            writer.Write(ContentList(i).FavoriteHeight)
            writer.Write(ContentList(i).LinksPosition)
            writer.Write(ContentList(i).Maximiert)
        Next
    End Sub

    Public Overrides Function ToString() As String
        Dim str As String = ""
        str &= Me.ContentList.Count & vbCrLf
        str &= Me.anzeigeheight & vbCrLf
        str &= Me.selectedIndex & vbCrLf
        str &= Me._visible & vbCrLf
        For i As Integer = 0 To Me.ContentList.Count - 1
            str &= ContentList(i).Name & vbCrLf
            str &= ContentList(i).Visible
            If i < Me.ContentList.Count - 1 Then
                str &= vbCrLf
            End If
        Next
        Return str
    End Function

    Public Sub RemoveContentHandlers()
        For i As Integer = 0 To ContentList.Count - 1
            RemoveHandler ContentList(i).VisibleChanged, AddressOf ContentvisibleChanged
        Next
    End Sub

    Public Function UntersucheContentAvailableChanges() As TabElementContentAvailableChangeUntersuchung
        If headers Is Nothing OrElse headerPanel Is Nothing OrElse headerPanel.Controls.Count = 0 OrElse _visible = False Then
            Dim selectedItemVorher As String = ""
            If headers IsNot Nothing AndAlso headers.Count > 0 Then
                For i As Integer = 0 To headers.Count - 1
                    If headers(i).Selected Then
                        selectedItemVorher = headers(i).contentName
                        Exit For
                    End If
                Next
            End If
            Return UntersucheContentAvailableChanges(New String() {}, selectedItemVorher)
        Else
            Dim alteNamen As New List(Of String)
            Dim sel As String = ""
            For i As Integer = 0 To headerPanel.Controls.Count - 1
                If TypeOf headerPanel.Controls(i) Is HeaderButton Then
                    alteNamen.Add(DirectCast(headerPanel.Controls(i), HeaderButton).contentName)
                    If DirectCast(headerPanel.Controls(i), HeaderButton).Selected Then
                        sel = alteNamen(alteNamen.Count - 1)
                    End If
                End If
            Next
            Return UntersucheContentAvailableChanges(alteNamen.ToArray, sel)
        End If
    End Function

    Public Function UntersucheContentAvailableChanges(Altenamen() As String, selectedItemVorher As String) As TabElementContentAvailableChangeUntersuchung
        Dim erg As New TabElementContentAvailableChangeUntersuchung

        Dim neueListe As New List(Of String)
        For i As Integer = 0 To ContentList.Count - 1
            If ContentList(i).Visible Then
                neueListe.Add(ContentList(i).Name)
            End If
        Next
        erg.AnzahlVisibleNeu = neueListe.Count

        If neueListe.Count = 0 Then
            If Altenamen.Length > 0 Then
                erg.LostAllElements = True
            End If
        Else
            If Altenamen.Length = 0 Then
                erg.WurdeVisibleTrue = True
                erg.MussHeadersRendern = True
                erg.SelectedIndexChanged = True
                For i As Integer = 0 To neueListe.Count - 1
                    If neueListe(i) = selectedItemVorher Then
                        erg.NeuerSelectedItem = selectedItemVorher
                        Exit For
                    End If
                Next
            Else
                If Altenamen.Length = neueListe.Count Then
                    'es besteht die Möglichkeit, dass es genau gleich geblieben ist!
                    For i As Integer = 0 To Altenamen.Length - 1
                        If Altenamen(i) <> neueListe(i) Then
                            erg.MussHeadersRendern = True
                            Exit For
                        End If
                    Next
                Else
                    erg.MussHeadersRendern = True
                End If

                erg.SelectedIndexChanged = True
                For i As Integer = 0 To neueListe.Count - 1
                    If neueListe(i) = selectedItemVorher Then
                        erg.SelectedIndexChanged = False
                        Exit For
                    End If
                Next

                If erg.SelectedIndexChanged Then
                    Dim selectedheader As Integer
                    For i As Integer = 0 To Altenamen.Length - 1
                        If Altenamen(i) = selectedItemVorher Then
                            selectedheader = i
                            Exit For
                        End If
                    Next

                    Dim succes As Boolean = False

                    For i As Integer = selectedheader - 1 To 0 Step -1
                        If neueListe.Contains(Altenamen(i)) Then
                            erg.NeuerSelectedItem = Altenamen(i)
                            succes = True
                            Exit For
                        End If
                    Next
                    If Not succes Then
                        For i As Integer = selectedheader + 1 To Altenamen.Count - 1
                            If neueListe.Contains(Altenamen(i)) Then
                                erg.NeuerSelectedItem = Altenamen(i)
                                succes = True
                                Exit For
                            End If
                        Next
                    End If
                    If Not succes Then
                        erg.NeuerSelectedItem = neueListe(0)
                    End If
                End If
            End If
        End If
        Return erg
    End Function

    Public Sub WendeChangesAn(changes As TabElementContentAvailableChangeUntersuchung)
        If changes.LostAllElements Then
            _visible = False
        ElseIf changes.WurdeVisibleTrue Then
            _visible = True
        End If

        If changes.MussHeadersRendern AndAlso headerPanel IsNot Nothing Then
            headerPanel.Controls.Clear()
            headerRendern()
        End If

        If changes.SelectedIndexChanged Then
            Dim index1 As Integer = 0
            For i As Integer = 0 To ContentList.Count - 1
                If ContentList(i).Name = changes.NeuerSelectedItem Then
                    index1 = i
                    Exit For
                End If
            Next
            SetSelectedIndex(index1)
            If Contentpanel IsNot Nothing Then Contentrendern()
            ContentList(selectedIndex).OnImTabElementSelected()
        ElseIf changes.MussAufJedenFallContentRendern Then
            Contentrendern()
        End If
    End Sub
End Class

Public Class TabElementContentAvailableChangeUntersuchung
    Public MussHeadersRendern As Boolean
    Public SelectedIndexChanged As Boolean
    Public NeuerSelectedItem As String
    Public MussAufJedenFallContentRendern As Boolean

    Public LostAllElements As Boolean
    Public WurdeVisibleTrue As Boolean
    Public AnzahlVisibleNeu As Integer

    Public Sub New()
        MussHeadersRendern = False
        SelectedIndexChanged = False
        NeuerSelectedItem = ""
        AnzahlVisibleNeu = 0
        MussAufJedenFallContentRendern = False

        LostAllElements = False
        WurdeVisibleTrue = False
    End Sub
End Class
