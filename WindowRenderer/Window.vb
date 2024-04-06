Imports System.Drawing.Drawing2D

''' <summary>
''' Organisiert die Eigenschaften eines fensters, in dem der Content ist
''' </summary>
''' <remarks></remarks>
Public Class Window
    Inherits Form
    Public Tabs As TabElement
    Public Shared DefaultBreite As Integer
    'Public defaultHöhe As Integer = 400
    Private NurEinElementModus As Boolean
    Public OnWindowClosing As Boolean
    Public start As Boolean = True
    Private dragdropClosing As Boolean = False
    Public NichtNeuesFensterRaisen As Boolean = False

    Private style As DesignStyle

    ''' <summary>
    ''' Höhe in der das Fenster initial angezeigt wird.
    ''' Wenn diese Größe (-1,-1) ist wird die Standardgröße (abhängig vom Content) genommen. Dies ist der normale Fall
    ''' </summary>
    ''' <remarks></remarks>
    Public FesteStartHöheShow As Integer = -1

    Public ReadOnly Property NurEinElement As Boolean
        Get
            Return NurEinElementModus
        End Get
    End Property

    ''' <summary>
    ''' Nimmt allgemeine initialisierungen vor. Muss aus den anderen Konstruktoren aufgerufen werden
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub New(style As DesignStyle)
        Me.style = style
        Me.BackColor = style.Control
    End Sub

    Public Sub New(style As DesignStyle, ByVal c As Content)
        Me.New(style)
        Me.Tabs = New TabElement(style)
        Me.Tabs.add(c)
        Me.Tabs.breite = DefaultBreite
        setPropertysDefault()
        AddHandler Me.SizeChanged, AddressOf SizeChanging
        AddHandler Tabs.FensterHinzufügen, AddressOf FensterHinzufügen
    End Sub
    Public Sub New(style As DesignStyle, ByVal t As TabElement)
        Me.New(style)
        Me.Tabs = t
        setPropertysDefault()
        AddHandler Me.SizeChanged, AddressOf SizeChanging
        AddHandler Tabs.FensterHinzufügen, AddressOf FensterHinzufügen
    End Sub
    Public Sub New(style As DesignStyle, ByVal P As Panel, ByVal hatmax As Boolean)
        Me.New(style)
        Me.Tabs = New TabElement(style)
        Me.Tabs.breite = DefaultBreite
        Me.Tabs.add(P, hatmax)
        setPropertysDefault()
        AddHandler Me.SizeChanged, AddressOf SizeChanging
        AddHandler Tabs.FensterHinzufügen, AddressOf FensterHinzufügen
    End Sub
    Public Sub add(ByVal t As TabElement)
        Tabs.NichtContentRendern = False
        NichtNeuesFensterRaisen = False
        Dim s As Integer = t.selectedIndex + Tabs.ContentList.Count
        For i As Integer = 0 To t.ContentList.Count - 1
            Tabs.add(t.ContentList(i))
        Next
        If NurEinElementModus Then
            NurEinElementModus = False
            Me.Text = ""
            Me.Controls.RemoveAt(0)
            Me.Controls.Add(Tabs.getPanel(Me.Height))
            Me.Controls(0).Dock = DockStyle.Fill
        Else
            Tabs.headerLöschen()
            Tabs.headerRendern()
        End If
        Tabs.SetSelectedIndex(s)
    End Sub
    Public Sub Insert(ByVal t As TabElement)
        NichtNeuesFensterRaisen = False
        Tabs.Insert(t)
    End Sub
    Private Sub setPropertysDefault()
        Me.ShowInTaskbar = False
        Me.FormBorderStyle = FormBorderStyle.SizableToolWindow
        Me.ShowIcon = False
        Me.Text = ""
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.KeyPreview = True
        AddHandler Me.Tabs.VisibleChanged, AddressOf TabVisibleChanged
    End Sub
    Private Sub TabVisibleChanged(ByVal sender As Object, ByVal e As EventArgs)
        Me.Visible = Tabs.Visible
    End Sub
    Public Shadows Sub Show(ByVal f As Form)
        Me.Tabs.nichtTabVerschieben = True
        If Tabs.ContentList.Count = 1 Then
            'If Me.Tabs.GetMaxHeight = Integer.MaxValue Then
            'Me.ClientSize = New Size(Tabs.breite, Tabs.ContentList(0).FavoriteHeight)
            'Else
            'Me.ClientSize = New Size(Tabs.breite, Tabs.ContentList(0).MaximumHeight)
            'End If
            Dim höhe As Integer = FesteStartHöheShow
            If höhe = -1 Then höhe = Tabs.ContentList(0).FavoriteHeight
            Me.ClientSize = New Size(Tabs.breite, höhe)
            FesteStartHöheShow = -1

            Dim nr As Integer = 0
            For i As Integer = 0 To Tabs.ContentList.Count - 1
                If Tabs.ContentList(i).Visible Then
                    nr = i
                    Exit For
                End If
            Next
            Me.Controls.Add(Tabs.ContentList(nr).InhaltsPanel)
            Me.Text = Tabs.ContentList(nr).Name
            Me.Controls(0).Dock = DockStyle.Fill
            NurEinElementModus = True
            Tabs.NichtContentRendern = True
        Else
            Dim höhe As Integer = Me.FesteStartHöheShow
            If höhe = -1 Then
                If Me.Tabs.GetMaxHeight = Integer.MaxValue Then
                    höhe = Tabs.ContentList(0).FavoriteHeight
                Else
                    höhe = Tabs.GetMaxHeight + 24
                End If
            End If
            Me.ClientSize = New Size(Tabs.breite, höhe)
            FesteStartHöheShow = -1

            Me.Controls.Add(Tabs.getPanel(Me.Height))
            NurEinElementModus = False
            Tabs.NichtContentRendern = False
        End If
        Me.Controls(0).Dock = DockStyle.Fill
        Me.Controls(0).Margin = New Padding(0)
        AddHandler Tabs.LostElement, AddressOf TabsLostElement
        If f Is Nothing Then
            MyBase.Show()
            Me.TopMost = True
        Else
            MyBase.Show(f)
        End If
        start = False
    End Sub
    Private Sub TabsLostElement(ByVal sender As Object, ByVal e As EventArgs)
        If Tabs.ContentList.Count = 0 Then
            Me.Close()
        Else
            Dim anzahl As Integer = 0
            For i As Integer = 0 To Tabs.ContentList.Count - 1
                If Tabs.ContentList(i).Visible Then
                    anzahl += 1
                End If
            Next
            If anzahl = 1 Then
                If Me.Controls IsNot Nothing AndAlso Me.Controls.Count > 0 Then
                    Me.Controls.RemoveAt(0)
                End If
                Dim nr As Integer = 0
                For i As Integer = 0 To Tabs.ContentList.Count - 1
                    If Tabs.ContentList(i).Visible Then
                        nr = i
                        Exit For
                    End If
                Next
                Me.Controls.Add(Tabs.ContentList(nr).InhaltsPanel)
                Me.Controls(0).Dock = DockStyle.Fill
                Me.Text = Tabs.ContentList(nr).Name
                NurEinElementModus = True
                Tabs.NichtContentRendern = True
            End If
            If anzahl = 2 Then
                If NurEinElement Then
                    NurEinElementModus = False
                    Me.Text = ""
                    Me.Controls.RemoveAt(0)
                    Me.Controls.Add(Tabs.getPanel(Me.Height))
                    Me.Controls(0).Dock = DockStyle.Fill
                    Tabs.NichtContentRendern = False
                End If
            End If
        End If
    End Sub
    Private Sub SizeChanging(ByVal sender As Object, ByVal e As EventArgs)
        If Not OnWindowClosing Then
            Tabs.breite = Me.ClientSize.Width
            For Each c As Content In Tabs.ContentList
                c.FavoriteWidth = Me.ClientSize.Width
                If Me.IsHandleCreated Then
                    If Tabs.ContentList.Count = 1 Then
                        c.FavoriteHeight = Me.ClientSize.Height
                    Else
                        c.FavoriteHeight = Me.ClientSize.Height - 24
                    End If
                End If
            Next
        End If
    End Sub
    Private Sub FensterHinzufügen(ByVal sender As Object, ByVal fenster As Window)
        If Not NichtNeuesFensterRaisen Then
            RaiseEvent FensterHinzufügenEv(Me, fenster)
        End If
        NichtNeuesFensterRaisen = False
    End Sub
    Public Event FensterHinzufügenEv(ByVal sender As Object, ByVal fenster As Window)

    Private Overpic As PictureBox
    Public Sub OnFensterOver()
        If NurEinElement Then
            If Overpic Is Nothing Then
                Overpic = New PictureBox
                Overpic.Size = Me.ClientSize
                Overpic.Location = New Point(0, 0)
                Overpic.Image = New Bitmap(Overpic.Width, Overpic.Height)
                Tabs.ContentList(0).InhaltsPanel.DrawToBitmap(DirectCast(Overpic.Image, Bitmap), New Rectangle(0, 0, Overpic.Width, Overpic.Height))
                Using g As Graphics = Graphics.FromImage(Overpic.Image)
                    Dim l As New LinearGradientBrush(New Rectangle(0, 0, Overpic.Width, Overpic.Height), OneTabInsertColor1, OneTabInsertColor2, LinearGradientMode.Vertical)
                    g.FillRectangle(l, 0, 20, Overpic.Width, Overpic.Height)
                    g.FillRectangle(l, CInt(Int(Me.ClientSize.Width / 2)), 0, CInt(Int(Me.ClientSize.Width / 2) + 1), 20)
                End Using
                Overpic.Parent = Me
                Overpic.BringToFront()
            End If
        Else
            Tabs.OnFensterOver()
        End If
    End Sub
    Public Sub OnFensterLeave()
        If NurEinElement Then
            If Overpic IsNot Nothing Then
                Me.Controls.Remove(Overpic)
                Overpic.Dispose()
                Overpic = Nothing
            End If
        Else
            Tabs.OnFensterLeave()
        End If
    End Sub

    Public Sub clear()
        Tabs.clear()
    End Sub

    Private Sub Window_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not keineCloseEvents Then
            If e.CloseReason = CloseReason.WindowsShutDown OrElse e.CloseReason = CloseReason.TaskManagerClosing OrElse e.CloseReason = CloseReason.ApplicationExitCall Then
                'Wenn ende ist, dann ist auch Ende. Sonst beendet sich die Anwendung nicht!
                e.Cancel = False
                Return
            End If
            If e.CloseReason = CloseReason.FormOwnerClosing Then
                e.Cancel = True
                Exit Sub
            End If
            If Not dragdropClosing Then
                Tabs.Visible = False
                e.Cancel = True
                If Me.Owner IsNot Nothing Then
                    Me.Owner.BringToFront()
                End If
                Exit Sub
            End If
            'Ganz wichtig: Die MenuForm informieren, dass wir hier im Windowrenderer sind.
            WindowRenderer.OnWindowRendererFormClosing = True
        End If
        If Me.Controls.Count > 0 Then
            Me.Controls.RemoveAt(0)
        End If

        RemoveHandler Tabs.LostElement, AddressOf TabsLostElement
        RemoveHandler Tabs.FensterHinzufügen, AddressOf FensterHinzufügen
        RemoveHandler Tabs.VisibleChanged, AddressOf TabVisibleChanged
        dragdropClosing = False
    End Sub

    Public Shadows Sub Close()
        dragdropClosing = True
        MyBase.Close()
    End Sub

    Private keineCloseEvents As Boolean = False
    Public Sub CloseOhneEvents()
        keineCloseEvents = True
        MyBase.Close()
        keineCloseEvents = False
    End Sub

    Public Sub DesignStyleChanged()
        If Tabs IsNot Nothing Then
            Tabs.Stylechanged()
        End If
        Me.BackColor = style.Control
    End Sub

    Public Sub ContentRendern()
        If NurEinElement Then
            Me.Controls(0).Refresh()
        Else
            Tabs.Contentrendern()
        End If
    End Sub

    Public Sub UpdateContentAvailableChanges()
        Dim changes As TabElementContentAvailableChangeUntersuchung
        If NurEinElementModus Then
            Dim namen(0) As String
            Dim aktuellesPanel As Panel = DirectCast(Me.Controls(0), Panel)
            For i As Integer = 0 To Tabs.ContentList.Count - 1
                If Tabs.ContentList(i).InhaltsPanel.Equals(aktuellesPanel) Then
                    namen(0) = Tabs.ContentList(i).Name
                    Exit For
                End If
            Next
            changes = Tabs.UntersucheContentAvailableChanges(namen, namen(0))
        Else
            changes = Tabs.UntersucheContentAvailableChanges()
        End If

        If changes.LostAllElements Then
            Me.Visible = False
        ElseIf changes.WurdeVisibleTrue Or changes.AnzahlVisibleNeu > 0 Then
            Me.Visible = True
        End If

        If changes.AnzahlVisibleNeu = 1 Then
            If Not NurEinElementModus Or changes.SelectedIndexChanged Then
                'der angezeigte Content hat sich geändert und muss neu angezeigt werden
                '(im Ein-Element Modus)
                If Me.Controls IsNot Nothing AndAlso Me.Controls.Count > 0 Then
                    Me.Controls.RemoveAt(0)
                End If
                Dim nr As Integer = 0
                For i As Integer = 0 To Tabs.ContentList.Count - 1
                    If Tabs.ContentList(i).Visible Then
                        nr = i
                        Exit For
                    End If
                Next
                Me.Controls.Add(Tabs.ContentList(nr).InhaltsPanel)
                Me.Controls(0).Dock = DockStyle.Fill
                Me.Text = Tabs.ContentList(nr).Name
                NurEinElementModus = True
                Tabs.NichtContentRendern = True
            End If
        Else
            If NurEinElementModus Then
                NurEinElementModus = False
                Me.Text = ""
                If Me.Controls IsNot Nothing AndAlso Me.Controls.Count > 0 Then
                    Me.Controls.RemoveAt(0)
                End If
                Me.Controls.Add(Tabs.getPanel(Me.Height))
                Me.Controls(0).Dock = DockStyle.Fill
                Tabs.NichtContentRendern = False
                changes.MussAufJedenFallContentRendern = True
            End If
            Tabs.WendeChangesAn(changes)
        End If
    End Sub

End Class
