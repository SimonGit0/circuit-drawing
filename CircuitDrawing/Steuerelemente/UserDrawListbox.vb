Option Strict On
Imports System.Drawing.Drawing2D
Public Class UserDrawListbox
    Inherits Control
    Implements IEnumerable(Of ListboxItem)

    Private myItems As List(Of ListboxItem)
    Private topPosition As Integer
    Private VerticalScroll As VScrollBar
    Public mausMoveIndex As Integer
    Private MausDown As Boolean
    Private MittlereItemHeight As Double
    Private gesamtHöhe As Integer
    Private _selectedIndex As Integer
    Protected renderingStopped As Boolean
    Private _selectedInices As List(Of Integer)

    Public Property CanDrag As Boolean = False

    Public Sub New()
        myItems = New List(Of ListboxItem)
        topPosition = 0
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint Or ControlStyles.Opaque, True)
        Me.SetStyle(ControlStyles.Selectable, True)
        Me.VerticalScroll = New VScrollBar()
        Me.VerticalScroll.Dock = DockStyle.Right
        Me.VerticalScroll.Visible = False
        Me.VerticalScroll.Parent = Me

        mausMoveIndex = -1
        MausDown = False
        _selectedInices = New List(Of Integer)
        _selectionMode = SelectionModeEnum.SingleSelection
        _selectedIndex = -1
        renderingStopped = False
        AddHandler Me.VerticalScroll.Scroll, AddressOf OnScroll
    End Sub

    Public Shadows Sub SuspendLayout()
        MyBase.SuspendLayout()
        renderingStopped = True
    End Sub

    Public Shadows Sub ResumeLayout()
        MyBase.ResumeLayout()
        renderingStopped = False
        Me.Invalidate()
    End Sub

    Public Property TopScrollPosition As Integer
        Get
            Return topPosition
        End Get
        Set(value As Integer)
            If topPosition <> value Then
                topPosition = value

                If topPosition < 0 Then topPosition = 0
                Dim höhe As Integer = berechneHöhe()
                If höhe > Me.Height Then
                    If topPosition + Me.Height > höhe Then
                        'unten füllt es nicht mehr ganz aus!
                        topPosition = höhe - Me.Height
                    End If
                Else
                    topPosition = 0
                End If

                Me.Invalidate()
            End If
        End Set
    End Property


    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        If renderingStopped Then
            Return
        End If

        'Hintergrund malen
        e.Graphics.Clear(BackColor)

        OnDrawItems(e)

        MyBase.OnPaint(e)
    End Sub

    Protected Overridable Sub OnDrawItems(e As System.Windows.Forms.PaintEventArgs)
        If renderingStopped Then
            Return
        End If

        Dim yPos As Integer = 0
        Dim anfangRendern As Integer = topPosition
        Dim endeRendern As Integer = topPosition + Me.Height
        Dim breite As Integer = Me.Width
        If Me.VerticalScroll.Visible Then
            breite -= Me.VerticalScroll.Width
        End If

        For i As Integer = 0 To myItems.Count - 1
            If yPos < endeRendern And yPos + myItems(i).Height > anfangRendern Then
                'item muss gerendert werden
                Dim drawinargs As New ListboxPaintEventArgs(e.Graphics, New Rectangle(0, yPos - topPosition, breite, myItems(i).Height), i, BackColor, ForeColor, Font, Me.Enabled)
                drawinargs.State = ListboxItemState.None
                If i = mausMoveIndex Then
                    drawinargs.State = drawinargs.State Or ListboxItemState.MouseOver
                    If MausDown Then drawinargs.State = drawinargs.State Or ListboxItemState.MouseDown
                End If
                If SelectionMode = SelectionModeEnum.MultiSelect_Additiv Then
                    If _selectedInices.Contains(i) Then
                        drawinargs.State = drawinargs.State Or ListboxItemState.Selected
                    End If
                ElseIf SelectionMode <> SelectionModeEnum.None Then
                    If SelectedIndex = i Then
                        drawinargs.State = drawinargs.State Or ListboxItemState.Selected
                    End If
                End If
                myItems(i).DrawItem(drawinargs)
            End If
            yPos += myItems(i).Height
        Next

        gesamtHöhe = yPos
        If myItems.Count > 0 Then MittlereItemHeight = gesamtHöhe / myItems.Count
    End Sub

    Protected Overrides Sub OnSizeChanged(ByVal e As System.EventArgs)
        berechneScrollbars()
        MyBase.OnSizeChanged(e)
        Me.Invalidate()
    End Sub

    Public Function GetIndexAt(ByVal y As Integer) As Integer
        y += topPosition
        If y < 0 Then Return -1
        Dim höhe As Integer = 0
        For i As Integer = 0 To myItems.Count - 1
            höhe += myItems(i).Height
            If y < höhe Then
                Return i
            End If
        Next
        Return -1
    End Function

    Public Property SelectedIndex As Integer
        Get
            Return _selectedIndex
        End Get
        Set(ByVal value As Integer)
            If SelectionMode <> SelectionModeEnum.None Then
                If value <> _selectedIndex Then
                    If value = -1 And SelectionMode = SelectionModeEnum.ImmerEinItemMussSelectedSein Then
                        Return
                    End If
                    _selectedIndex = value
                    Me.Invalidate()
                    RaiseEvent SelectedIndexChanged(Me, EventArgs.Empty)
                End If
            Else
                If _selectedIndex <> -1 Then
                    _selectedIndex = -1
                    Me.Invalidate()
                End If
            End If
        End Set
    End Property

    Public ReadOnly Property SelectedInices As List(Of Integer)
        Get
            Return _selectedInices
        End Get
    End Property

    Private _selectionMode As SelectionModeEnum
    Public Property SelectionMode As SelectionModeEnum
        Get
            Return _selectionMode
        End Get
        Set(ByVal value As SelectionModeEnum)
            _selectionMode = value
            If value = SelectionModeEnum.None Then
                _selectedIndex = -1
                _selectedInices.Clear()
            ElseIf value = SelectionModeEnum.SingleSelection Then
                _selectedInices.Clear()
            ElseIf value = SelectionModeEnum.ImmerEinItemMussSelectedSein Then
                If _selectedIndex = -1 And Count > 0 Then
                    _selectedIndex = 0
                End If
            End If
            Me.Invalidate()
        End Set
    End Property

#Region "Scrollbar"
    Private Sub berechneScrollbars()
        Dim höhe As Integer = berechneHöhe()
        If höhe > Me.Height Then
            'scrollbar anzeigen
            If topPosition + Me.Height > höhe Then
                'unten füllt es nicht mehr ganz aus!
                topPosition = höhe - Me.Height
            End If
            Me.VerticalScroll.Visible = True
            Me.VerticalScroll.Minimum = 0
            Me.VerticalScroll.Maximum = höhe
            Me.VerticalScroll.LargeChange = Me.Height
            Me.VerticalScroll.Value = topPosition
            Me.VerticalScroll.SmallChange = CInt(höhe / myItems.Count) 'duchschnittliche Item höhe
        Else
            'scrollbar nicht anzeigen
            Me.topPosition = 0 'wieder ganz oben anfangen
            Me.VerticalScroll.Visible = False
        End If
    End Sub

    Private Sub OnScroll(ByVal sender As Object, ByVal e As ScrollEventArgs)
        If e.ScrollOrientation = ScrollOrientation.VerticalScroll Then
            Me.topPosition = e.NewValue
            Me.Invalidate()
        End If
        Me.Focus()
    End Sub

    Public Sub DoScroll(e As MouseEventArgs)
        Me.OnMouseWheel(e)
    End Sub

    Public Sub ScrollSelectedIntoView()
        If Me.SelectedIndex <> -1 Then
            Dim min As Integer = GetIndexAt(0)
            Dim max As Integer = GetIndexAt(Me.Height - 1)
            If Me.SelectedIndex < min Then
                Dim dY As Integer = 0
                For i As Integer = min - 1 To SelectedIndex Step -1
                    dY += myItems(i).Height
                Next
                VerticalScroll.Value = Math.Max(VerticalScroll.Value - dY, 0)
                Me.Invalidate()
            ElseIf Me.SelectedIndex > max Then
                Dim dY As Integer = 0
                For i As Integer = max + 1 To SelectedIndex
                    dY += myItems(i).Height
                Next
                VerticalScroll.Value = Math.Min(VerticalScroll.Value + dY, VerticalScroll.Maximum)
                Me.Invalidate()
            End If
            Me.topPosition = VerticalScroll.Value
        End If
    End Sub

    Public Sub ScrollIndexIntoView(index As Integer)
        Dim min As Integer = GetIndexAt(0) + 1
        Dim max As Integer = GetIndexAt(Me.Height - 1) - 1
        If index < min Then
            Dim dY As Integer = 0
            For i As Integer = min - 1 To index Step -1
                dY += myItems(i).Height
            Next
            VerticalScroll.Value = Math.Max(VerticalScroll.Value - dY, 0)
            Me.Invalidate()
        ElseIf index > max Then
            Dim dY As Integer = 0
            For i As Integer = max + 1 To index
                dY += myItems(i).Height
            Next
            VerticalScroll.Value = Math.Min(VerticalScroll.Value + dY, VerticalScroll.Maximum)
            Me.Invalidate()
        End If
        Me.topPosition = VerticalScroll.Value
    End Sub


    ''' <summary>
    ''' Berechnet die Höhe der Listbox, indem die Höhen aller items zusammenaddiert werden
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function berechneHöhe() As Integer
        Dim höhe As Integer = 0
        For i As Integer = 0 To myItems.Count - 1
            höhe += myItems(i).Height
        Next
        Return höhe
    End Function
#End Region

#Region "Maus - Events"

    Private dragdropPoint As Point
    Private hasStartedDraging As Boolean = False

    Protected Overrides Sub OnMouseWheel(ByVal e As System.Windows.Forms.MouseEventArgs)
        If Me.VerticalScroll.Visible Then
            Dim lines As Double = e.Delta / SystemInformation.MouseWheelScrollDelta * SystemInformation.MouseWheelScrollLines
            topPosition -= CInt(lines * MittlereItemHeight)
            If topPosition < 0 Then topPosition = 0
            If topPosition + Me.Height > gesamtHöhe Then
                topPosition = gesamtHöhe - Me.Height + 1
            End If
            Me.VerticalScroll.Value = topPosition
            mausMoveIndex = GetIndexAt(e.Y)
            Me.Invalidate()
        End If
        MyBase.OnMouseWheel(e)
    End Sub

    Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim index As Integer = GetIndexAt(e.Y)
        If Not e.Button = MouseButtons.Left Then
            MausDown = False
        ElseIf Me.CanDrag AndAlso (Math.Abs(e.X - dragdropPoint.X) >= 2 OrElse Math.Abs(e.Y - dragdropPoint.Y) >= 2) Then
            OnStartDrag(index)
            hasStartedDraging = True
        End If
        If index <> mausMoveIndex Then
            mausMoveIndex = index
            Me.Invalidate()
        End If
        MyBase.OnMouseMove(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(ByVal e As System.EventArgs)
        If mausMoveIndex <> -1 Then
            mausMoveIndex = -1
            Me.Invalidate()
        End If
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
        Me.Select()
        If Not MausDown And e.Button = MouseButtons.Left Then
            MausDown = True
            hasStartedDraging = False
            If CanDrag Then
                dragdropPoint = New Point(e.X, e.Y)
            End If
            Me.Invalidate()
        End If
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
        MausDown = False
        If (e.Button = MouseButtons.Left OrElse e.Button = MouseButtons.Right) AndAlso Not hasStartedDraging Then
            Select Case SelectionMode
                Case SelectionModeEnum.ImmerEinItemMussSelectedSein, SelectionModeEnum.SingleSelection
                    If SelectedIndex = mausMoveIndex AndAlso SelectedIndex <> -1 Then
                        RaiseEvent SelectedItemClicked(Me, EventArgs.Empty)
                    Else
                        SelectedIndex = mausMoveIndex
                    End If
                Case SelectionModeEnum.MultiSelect_Additiv
                    SelectedIndex = mausMoveIndex
                    If mausMoveIndex <> -1 Then
                        If _selectedInices.Contains(mausMoveIndex) Then
                            _selectedInices.Remove(mausMoveIndex)
                        Else
                            _selectedInices.Add(mausMoveIndex)
                        End If
                    End If
            End Select
        End If
        Me.Invalidate()
        MyBase.OnMouseUp(e)
    End Sub
#End Region

    Private Sub OnStartDrag(index As Integer)
        If Me.CanDrag Then
            RaiseEvent DragContent(Me, New UserDragDropEventArgs(index, Me))
        End If
    End Sub

#Region "Items hinzufügen, entfernen, ..."
    Public ReadOnly Property Items(ByVal index As Integer) As ListboxItem
        Get
            Return myItems(index)
        End Get
    End Property

    Public Function IndexOf(ByVal item As ListboxItem) As Integer
        Return myItems.IndexOf(item)
    End Function

#Region "Add"
    Public Sub addItem(ByVal item As ListboxItem)
        myItems.Add(item)
        Addhandlers(item)
        OnItemsChanged()
    End Sub
    Public Sub addItemRange(ByVal items As IEnumerable(Of ListboxItem))
        myItems.AddRange(items)
        For Each i As ListboxItem In items
            Addhandlers(i)
        Next
        OnItemsChanged()
    End Sub
#End Region

#Region "Insert"
    Public Sub insertItem(ByVal index As Integer, ByVal item As ListboxItem)
        myItems.Insert(index, item)
        Addhandlers(item)
        OnItemsChanged()
    End Sub
    Public Sub insertItemRange(ByVal index As Integer, ByVal items As IEnumerable(Of ListboxItem))
        myItems.InsertRange(index, items)
        For Each i As ListboxItem In items
            Addhandlers(i)
        Next
        OnItemsChanged()
    End Sub
#End Region

#Region "Remove"
    Public Function RemoveItem(ByVal item As ListboxItem) As Boolean
        If myItems.Remove(item) Then
            Removehandlers(item)
            OnItemsChanged()
            Return True
        End If
        Return False
    End Function

    Public Sub RemoveItemAtIndex(ByVal index As Integer)
        Removehandlers(myItems(index))
        myItems.RemoveAt(index)
        OnItemsChanged()
    End Sub

    Public Sub RemoveItemRange(ByVal index As Integer, ByVal count As Integer)
        For i As Integer = index To index + count
            Removehandlers(myItems(i))
        Next
        myItems.RemoveRange(index, count)
        OnItemsChanged()
    End Sub

    Public Sub Clear()
        For Each i As ListboxItem In myItems
            Removehandlers(i)
        Next
        myItems.Clear()
        OnItemsChanged()
    End Sub
#End Region

    Protected Overridable Sub OnItemsChanged()
        berechneScrollbars()
        Me.Invalidate()
    End Sub


    Private Sub Addhandlers(ByVal item As ListboxItem)
    End Sub

    Private Sub Removehandlers(ByVal item As ListboxItem)
    End Sub
#End Region

    Public ReadOnly Property Count As Integer
        Get
            Return myItems.Count
        End Get
    End Property

    Public Event SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
    Public Event SelectedItemClicked(sender As Object, e As EventArgs)
    Public Event DragContent(sender As Object, e As UserDragDropEventArgs)


    Public Enum SelectionModeEnum
        None
        SingleSelection
        MultiSelect_Additiv
        ImmerEinItemMussSelectedSein
    End Enum

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of ListboxItem) Implements System.Collections.Generic.IEnumerable(Of ListboxItem).GetEnumerator
        Return myItems.GetEnumerator
    End Function
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class

#Region "Extraklassen"
Public Class ListboxPaintEventArgs
    Public Property Bounds As Rectangle
    Public Property Graphics As Graphics
    Public Property index As Integer
    Public Property BackColor As Color
    Public Property ForeColor As Color
    Public Property Font As Font
    Public Property State As ListboxItemState
    Public Property Enabled As Boolean

    Private obenBrush As Brush
    Private untenBrush As Brush
    Private rpen As Pen
    Private obenBrushS As Brush
    Private untenBrushS As Brush
    Private rpenS As Pen

    Private Shared MouseOverPen As New Pen(Color.FromArgb(255, 152, 152, 152))
    Private Shared MouseDownPen As New Pen(Color.FromArgb(255, 80, 80, 80))

    Public Sub New(ByVal g As Graphics, ByVal bounds As Rectangle, ByVal index As Integer, ByVal backcolor As Color, ByVal forecolor As Color, ByVal font As Font, enabled As Boolean)
        Me.Bounds = bounds
        Me.Graphics = g
        Me.index = index
        Me.BackColor = backcolor
        Me.ForeColor = forecolor
        Me.Font = font
        Me.Enabled = enabled

        MouseOverPen.DashPattern = {1, 1}
        MouseDownPen.DashPattern = {1, 1}

        obenBrush = New SolidBrush(Color.FromArgb(255, 220, 235, 252))
        untenBrush = New SolidBrush(Color.FromArgb(255, 193, 219, 252))
        rpen = New Pen(Color.FromArgb(255, 143, 185, 236))
        obenBrushS = New SolidBrush(Color.FromArgb(255, 240, 247, 255))
        untenBrushS = New SolidBrush(Color.FromArgb(255, 228, 240, 254))
        rpenS = New Pen(Color.FromArgb(255, 205, 225, 248))
    End Sub

    ''' <summary>
    ''' malt den Hintergrund mit den Standart Focus-Bereich und SelectedFarben
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub DrawStandartBackground()
        If (State And ListboxItemState.Selected) = ListboxItemState.Selected Then
            drawSelectedRectangle()
            If (State And ListboxItemState.MouseOver) = ListboxItemState.MouseOver Then
                If (State And ListboxItemState.MouseDown) = ListboxItemState.MouseDown Then
                    drawFocusRectangleDown()
                Else
                    drawFocusRectangleOver()
                End If
            End If
        ElseIf (State And ListboxItemState.MouseOver) = ListboxItemState.MouseOver Then
            drawSelectedRectangleSchwach()
            If (State And ListboxItemState.MouseDown) = ListboxItemState.MouseDown Then
                drawFocusRectangleDown()
            Else
                'drawFocusRectangleOver()
            End If
        End If
    End Sub

    ''' <summary>
    ''' malt das Rechteck, welches bei markierten Items angezeigt wird bei dem aktuellen Item. Unabhängig von seinem Status
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub drawSelectedRectangle()
        Graphics.FillRectangle(obenBrush, Bounds)
        Dim y As Integer = CInt((Bounds.Y + Bounds.Bottom) / 2)
        Graphics.FillRectangle(untenBrush, New Rectangle(Bounds.X, y, Bounds.Width, Bounds.Bottom - y))
        Graphics.DrawRectangle(rpen, New Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1))
    End Sub

    ''' <summary>
    ''' malt das Rechteck, welches bei Items mit MouseOver angezeigt wird bei dem aktuellen Item. Unabhängig von seinem Status
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub drawSelectedRectangleSchwach()
        Graphics.FillRectangle(obenBrushS, Bounds)
        Dim y As Integer = CInt((Bounds.Y + Bounds.Bottom) / 2)
        Graphics.FillRectangle(untenBrushS, New Rectangle(Bounds.X, y, Bounds.Width, Bounds.Bottom - y))
        Graphics.DrawRectangle(rpenS, New Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1))
    End Sub

    ''' <summary>
    ''' mal das Focus-Rechteck, welches normalerweise beim MouseOver gemalt wird beim aktuellen Item, unabhängig von dessen Status
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub drawFocusRectangleOver()
        Graphics.DrawRectangle(MouseOverPen, New Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1))
    End Sub

    ''' <summary>
    ''' malt das Focus-Rechteck, welches normalerweise bei MouseDown gemalt wird beim aktuellen Item, unabhängig vom aktuellen Status
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub drawFocusRectangleDown()
        Graphics.DrawRectangle(MouseDownPen, New Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1))
    End Sub

    ''' <summary>
    ''' Malt den angegebenen Text in das Item als Item-text
    ''' </summary>
    ''' <param name="itemText"></param>
    ''' <param name="Alignment"></param>
    ''' <remarks></remarks>
    Public Sub DrawText(ByVal itemText As String, offsetX As Integer, ByVal Alignment As StringAlignment)
        Dim old As SmoothingMode = Graphics.SmoothingMode
        Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Graphics.SmoothingMode = SmoothingMode.HighQuality
        Dim format As New StringFormat()
        format.LineAlignment = StringAlignment.Center
        format.Alignment = Alignment
        Graphics.DrawString(itemText, Font, New SolidBrush(ForeColor), New Rectangle(Bounds.X + offsetX, Bounds.Y, Bounds.Width - offsetX, Bounds.Height), format)
        Graphics.SmoothingMode = old
    End Sub

    Public Sub DrawText(itemText As String, posX As Integer, posY As Integer, Font As Font)
        Dim old As SmoothingMode = Graphics.SmoothingMode
        Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
        Graphics.SmoothingMode = SmoothingMode.HighQuality
        Graphics.DrawString(itemText, Font, New SolidBrush(ForeColor), New Point(posX, posY))
        Graphics.SmoothingMode = old
    End Sub
End Class

Public Class UserDragDropEventArgs
    Inherits EventArgs

    Public Property DragedIndex As Integer

    Public Listbox As UserDrawListbox

    Public Sub New(index As Integer, sender As UserDrawListbox)
        Me.DragedIndex = index
        Me.Listbox = sender
    End Sub

End Class

<Flags()>
Public Enum ListboxItemState
    None = 0
    Selected = 1
    MouseOver = 2
    MouseDown = 4
End Enum
#End Region

#Region "Spezielle Items"
Public Class ListboxItem
    Public Const DEFAULT_HEIGHT As Integer = 16
    Public Property Height As Integer

    Public Property Tag As Object

    Public Sub New()
        Me.Height = DEFAULT_HEIGHT
    End Sub

    Public Overridable Sub DrawItem(ByVal e As ListboxPaintEventArgs)
        e.DrawStandartBackground()
    End Sub
End Class

''' <summary>
''' Stellt einen Item da, der wie in einer klassischen Listbox einen Text darstellt
''' </summary>
''' <remarks></remarks>
Public Class ListboxItemText
    Inherits ListboxItem

    Public Property Text As String
    Public Property offsetX As Integer

    Public Sub New(ByVal text As String)
        Me.Text = text
        Me.offsetX = 3
    End Sub

    Public Overrides Sub DrawItem(ByVal e As ListboxPaintEventArgs)
        e.DrawStandartBackground()
        e.DrawText(Me.Text, offsetX, StringAlignment.Near)
    End Sub

End Class

''' <summary>
''' Stellt einen Item dar, der ein Bild anzeigt
''' </summary>
''' <remarks></remarks>
Public Class ListboxItemImage
    Inherits ListboxItem
    Public Property Image As Bitmap
    Public Sub New(ByVal image As Bitmap)
        Me.Height = image.Height
        Me.Image = image
    End Sub

    Public Overrides Sub DrawItem(ByVal e As ListboxPaintEventArgs)
        e.DrawStandartBackground()
        e.Graphics.DrawImage(Image, New Rectangle(e.Bounds.Location, Image.Size), New Rectangle(0, 0, Image.Width, Image.Height), GraphicsUnit.Pixel)
    End Sub
End Class

''' <summary>
''' Stellt einen Item dar, der sowohl ein Bild als auch einen Text unterstützt
''' </summary>
''' <remarks></remarks>
Public Class ListboxItemImageAndText
    Inherits ListboxItem

    Public Property Image As Bitmap
    Public Property Text As String
    Private format As StringFormat

    Public Property imageBorder As Integer

    Public Sub New()
        Me.New(20)
    End Sub

    Public Sub New(ByVal itemHöhe As Integer)
        Me.Height = itemHöhe
        Me.imageBorder = 2
        format = New StringFormat
        format.LineAlignment = StringAlignment.Center
        format.Alignment = StringAlignment.Near
    End Sub

    Public Overrides Sub DrawItem(ByVal e As ListboxPaintEventArgs)
        e.DrawStandartBackground()
        Dim höhe As Integer = e.Bounds.Height - 2 * imageBorder
        Dim breite As Integer = CInt(4 / 3 * höhe)
        Dim dx As Integer
        Dim dy As Integer

        If Image.Width / Image.Height > breite / höhe Then
            'an der Breite orientieren
            höhe = CInt(breite * Image.Height / Image.Width)
        Else
            'an der Höhe orientieren
            breite = CInt(höhe * Image.Width / Image.Height)
        End If

        dy = CInt((e.Bounds.Height - höhe) / 2)
        dx = CInt((4 / 3 * e.Bounds.Height - breite) / 2)

        e.Graphics.DrawImage(Image, New Rectangle(e.Bounds.X + dx, e.Bounds.Y + dy, breite, höhe), New Rectangle(0, 0, Image.Width, Image.Height), GraphicsUnit.Pixel)

        If e.Bounds.Width - e.Bounds.Height * 4 / 3 > 5 Then
            e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality

            'Anpassen von String, damit der String in die Box hineinpasst, ggf. werden "..." angezeigt.
            Dim str As String = Me.Text
            Dim länge As Integer = str.Length
            While länge > 0
                str = str.Substring(0, länge)
                Dim rect As SizeF = e.Graphics.MeasureString(str, e.Font, e.Bounds.Width - CInt(4 / 3 * e.Bounds.Height))
                If rect.Height > e.Bounds.Height Then
                    länge -= 1
                Else
                    If str.Length <> Me.Text.Length Then
                        länge = Math.Max(1, länge - 3)
                        str = str.Substring(0, länge)
                        str &= "..."
                    End If
                    Exit While
                End If
            End While
            e.Graphics.DrawString(str, e.Font, New SolidBrush(e.ForeColor), New Rectangle(e.Bounds.X + CInt(4 / 3 * e.Bounds.Height), e.Bounds.Y, e.Bounds.Width - CInt(4 / 3 * e.Bounds.Height), e.Bounds.Height), format)
        End If
        e.Graphics.SmoothingMode = SmoothingMode.None
    End Sub
End Class
#End Region
