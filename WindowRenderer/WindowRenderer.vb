Option Strict Off
Imports System.Drawing.Drawing2D
Imports System.IO

''' <summary>
''' Die Instanz der Klasse, welche den Bildschirm in die 3 Teile (Links, Rechts, Mitte) unterteilt und alle Information als höchste Instanz vereint
''' </summary>
''' <remarks></remarks>
Public Class WindowRenderer
    Public Shared OnWindowRendererFormClosing As Boolean = False

    'die drei Aufenthaltsräume der Panel (des Contents)
    Public LeisteLinks As DockStack
    Public LeisteRechts As DockStack

    ''' <summary>
    ''' Toolstrip2 aus dem Ebenenprojekt. Damit man das DrawToBitmap nochmal einzeln für den
    ''' Toolstrip machen kann. Weil er macht das nicht für den Toolstrip, wenn er in den anderen Controls verschachtelt ist
    ''' Das ist anscheinend noch ein Bug in Windows.Forms den wir so versuchen zu umgehen...
    ''' </summary>
    ''' <remarks></remarks>
    Public toolstrip2 As ToolStrip

    Public Fenster As List(Of Window)
    Private Ownerpanel As Panel
    Private MiddlePanel As Panel
    Private ganzMiddlePanel As Panel

    Private ZiehPanelRechts As Panel
    Private ZiehPanelLinks As Panel
    Public ZiehBereichbreite As Integer = 3
    Private ZiehBereichfarbe As Color
    Private altehöhe As Integer = -1
    Public Const minGröße As Integer = 100
    Public defaultbreite As Integer = 150
    Public MiddlepanelMinimumWidth As Integer = 50
    Public PanelR, panelL As Panel
    Public WindowsInFormForm As Form
    Private _nurIcons As Boolean = False
    Private nichtLocationChangedAufrufen As Boolean = False

    Private style As DesignStyle
    Public ReadOnly Property DesignStyle As DesignStyle
        Get
            Return style
        End Get
    End Property

    Public Property NurIconsAnzeigen As Boolean
        Get
            Return _nurIcons
        End Get
        Set(ByVal value As Boolean)
            If value <> _nurIcons Then
                _nurIcons = value
                LeisteLinks.IconsOnly = value
                LeisteRechts.IconsOnly = value

                For i As Integer = 0 To Fenster.Count - 1
                    Fenster(i).Tabs.NurIconsAnzeigen = value
                Next
                For i As Integer = 0 To LeisteLinks.Invisiblecollection.Count - 1
                    LeisteLinks.Invisiblecollection(i).NurIconsAnzeigen = value
                Next
                For i As Integer = 0 To LeisteRechts.Invisiblecollection.Count - 1
                    LeisteRechts.Invisiblecollection(i).NurIconsAnzeigen = value
                Next
            End If
        End Set
    End Property
    Public Sub New(style As DesignStyle, ByVal Parentpanel As Panel, ByVal MiddlePanel As Panel)
        Me.style = style
        Me.ZiehBereichfarbe = style.ToolStripHeaderBackcolor
        AddHandler Me.style.StyleChanged, AddressOf StyleChanged
        Fenster = New List(Of Window)
        LeisteLinks = New DockStack(style)
        LeisteLinks.breite = defaultbreite
        LeisteRechts = New DockStack(style)
        LeisteRechts.breite = defaultbreite

        ganzMiddlePanel = MiddlePanel
        MiddlePanel = New Panel()
        MiddlePanel.Location = New Point(0, 0)
        MiddlePanel.Size = New Size(ganzMiddlePanel.Width, ganzMiddlePanel.Height)
        ganzMiddlePanel.Location = New Point(0, 0)
        ganzMiddlePanel.Size = New Size(MiddlePanel.Width, MiddlePanel.Height)
        ganzMiddlePanel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ganzMiddlePanel.Parent = MiddlePanel

        AddHandler LeisteLinks.LostAllElements, AddressOf LeisteLinks_LostAllElements
        AddHandler LeisteRechts.LostAllElements, AddressOf LeisteRechts_LostAllElements

        AddHandler LeisteLinks.FensterHinzufügen, AddressOf FensterHinzufügen
        AddHandler LeisteRechts.FensterHinzufügen, AddressOf FensterHinzufügen

        AddHandler LeisteLinks.VisibleChanged, AddressOf linksVisibleChanged
        AddHandler LeisteRechts.VisibleChanged, AddressOf rechtsVisibleChanged

        Me.Ownerpanel = Parentpanel
        Me.MiddlePanel = MiddlePanel
        AddHandler Ownerpanel.SizeChanged, Sub()
                                               If altehöhe <> Ownerpanel.Height - LeisteLinks.Margin Then
                                                   altehöhe = Ownerpanel.Height - LeisteLinks.Margin
                                                   aktualiesieren()
                                               End If
                                           End Sub
        Window.DefaultBreite = Me.defaultbreite
        anzeigen()
    End Sub
    Private Sub rechtsVisibleChanged()
        If LeisteRechts.Visible Then
            If LeisteRechts.Visiblecollection.Count > 0 Then
                MiddlePanel.Dock = DockStyle.None
                If LeisteLinks.Visiblecollection.Count > 0 Then
                    MiddlePanel.Location = New Point(LeisteLinks.breite + ZiehBereichbreite, 0)
                    MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - LeisteRechts.breite - ZiehBereichbreite * 2, Ownerpanel.Height)
                Else
                    MiddlePanel.Location = New Point(0, 0)
                    MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteRechts.breite - ZiehBereichbreite, Ownerpanel.Height)
                End If
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
                If ZiehPanelRechts IsNot Nothing AndAlso Ownerpanel.Controls.Contains(ZiehPanelRechts) Then
                    Ownerpanel.Controls.Remove(ZiehPanelRechts)
                End If
                If PanelR IsNot Nothing AndAlso Ownerpanel.Controls.Contains(PanelR) Then
                    Ownerpanel.Controls.Remove(PanelR)
                End If
                locatePanelRechts()
            End If
        Else
            Ownerpanel.Controls.Remove(PanelR)
            If LeisteLinks.Visiblecollection.Count > 0 Then
                MiddlePanel.Dock = DockStyle.None
                MiddlePanel.Location = New Point(LeisteLinks.breite + ZiehBereichbreite, 0)
                MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - ZiehBereichbreite, Ownerpanel.Height)
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
            Else
                MiddlePanel.Dock = DockStyle.Fill
            End If
        End If
    End Sub
    Private Sub linksVisibleChanged()
        If LeisteLinks.Visible Then
            If LeisteLinks.Visiblecollection.Count > 0 Then
                MiddlePanel.Dock = DockStyle.None
                MiddlePanel.Location = New Point(LeisteLinks.breite + ZiehBereichbreite, 0)
                If LeisteRechts.Visiblecollection.Count > 0 Then
                    MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - LeisteRechts.breite - ZiehBereichbreite * 2, Ownerpanel.Height)
                Else
                    MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - ZiehBereichbreite, Ownerpanel.Height)
                End If
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
                If ZiehPanelLinks IsNot Nothing AndAlso Ownerpanel.Controls.Contains(ZiehPanelLinks) Then
                    Ownerpanel.Controls.Remove(ZiehPanelLinks)
                End If
                If panelL IsNot Nothing AndAlso Ownerpanel.Controls.Contains(panelL) Then
                    Ownerpanel.Controls.Remove(panelL)
                End If
                locatePanelLinks()
            End If
        Else
            Ownerpanel.Controls.Remove(panelL)
            If LeisteRechts.Visiblecollection.Count > 0 Then
                MiddlePanel.Dock = DockStyle.None
                MiddlePanel.Location = New Point(0, 0)
                MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteRechts.breite - ZiehBereichbreite, Ownerpanel.Height)
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
            Else
                MiddlePanel.Dock = DockStyle.Fill
            End If
        End If
    End Sub

    Private Sub aktualiesieren()
        If LeisteLinks.Visiblecollection IsNot Nothing AndAlso LeisteLinks.Visiblecollection.Count > 0 Then
            LeisteLinks.RerangeLayout(Ownerpanel.Height - LeisteLinks.Margin, True)
        End If
        If LeisteRechts.Visiblecollection IsNot Nothing AndAlso LeisteRechts.Visiblecollection.Count > 0 Then
            LeisteRechts.RerangeLayout(Ownerpanel.Height - LeisteRechts.Margin, True)
        End If
    End Sub
    Public Sub addWindow(ByVal t As TabElement, ByVal loc As Point, ByVal s As Size)
        Dim f As New Window(style, t)
        f.StartPosition = FormStartPosition.Manual
        FensterHinzufügen(Me, f)
        nichtLocationChangedAufrufen = True
        f.Location = loc
        f.Size = s
        nichtLocationChangedAufrufen = False
    End Sub

    Public Sub addWindowOhneEvents(t As TabElement, loc As Point, s As Size)
        Dim f As New Window(style, t)
        f.FesteStartHöheShow = s.Height
        f.StartPosition = FormStartPosition.Manual
        Fenster.Add(f)

        nichtLocationChangedAufrufen = True
        f.Location = loc
        f.ClientSize = s
        nichtLocationChangedAufrufen = False

        addHandlersWindow(f)
    End Sub


    Public Sub AddContent(ByVal c As Content, ByVal Aufenthalt As Aufenthaltsort)
        Select Case Aufenthalt
            Case Aufenthaltsort.Fenster
                Dim f As New Window(style, c)
                FensterHinzufügen(Me, f)
                ' f.Show()
            Case Aufenthaltsort.Links
                LeisteLinks.add(c)
            Case Aufenthaltsort.Rechts
                LeisteRechts.add(c)
        End Select
        anzeigen()
    End Sub

    Public Sub AddContent(ByVal contentpanel As Panel, ByVal hatMaxSize As Boolean, ByVal Aufenthalt As Aufenthaltsort)
        Select Case Aufenthalt
            Case Aufenthaltsort.Fenster
                Dim f As New Window(style, contentpanel, hatMaxSize)
                FensterHinzufügen(Me, f)
                'f.Show()
                LeisteLinks.add(contentpanel, hatMaxSize)
            Case Aufenthaltsort.Rechts
                LeisteRechts.add(contentpanel, hatMaxSize)
            Case Aufenthaltsort.Links
                LeisteLinks.add(contentpanel, hatMaxSize)
            Case Else
                Throw New NotImplementedException("Das geht hier nicht! Nutze die Funktion die einen content mitgiebt dafür!!")
        End Select
        anzeigen()
    End Sub
    Public Sub AddTabElement(ByVal t As TabElement, ByVal Aufenthalt As Aufenthaltsort)
        Select Case Aufenthalt
            Case Aufenthaltsort.Fenster
                Dim f As New Window(style, t)
                FensterHinzufügen(Me, f)
                'f.Show()
            Case Aufenthaltsort.Links
                LeisteLinks.add(t)
            Case Aufenthaltsort.Rechts
                LeisteRechts.add(t)
        End Select
        anzeigen()
    End Sub
    'Public Sub anzeigenAlt()
    '    Ownerpanel.SuspendLayout()
    '    Ownerpanel.Controls.Clear()
    '    Ownerpanel.Controls.Add(MiddlePanel)
    '    MiddlePanel.Dock = DockStyle.Fill
    '    If LeisteLinks.Visiblecollection.Count > 0 Then
    '        MiddlePanel.Dock = DockStyle.None
    '        MiddlePanel.Anchor = AnchorStyles.None
    '        MiddlePanel.Location = New Point(LeisteLinks.breite + ZiehBereichbreite, 0)
    '        If LeisteRechts.Visiblecollection.Count > 0 Then
    '            MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - LeisteRechts.breite - ZiehBereichbreite * 2, Ownerpanel.Height)
    '            locatePanelRechts()
    '        Else
    '            MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteLinks.breite - ZiehBereichbreite, Ownerpanel.Height)
    '        End If
    '        MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
    '        locatePanelLinks()
    '    ElseIf LeisteRechts.Visiblecollection.Count > 0 Then
    '        MiddlePanel.Dock = DockStyle.None
    '        MiddlePanel.Anchor = AnchorStyles.None
    '        MiddlePanel.Location = New Point(0, 0)
    '        MiddlePanel.Size = New Size(Ownerpanel.Width - LeisteRechts.breite - ZiehBereichbreite, Ownerpanel.Height)
    '        MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
    '        locatePanelRechts()
    '    End If
    '    Ownerpanel.ResumeLayout()
    'End Sub

    Public Sub anzeigen()
        Ownerpanel.SuspendLayout()
        If LeisteLinks.Visiblecollection.Count > 0 Then
            Dim loc As Point
            Dim size As Size

            loc = New Point(LeisteLinks.breite + ZiehBereichbreite, 0)
            If LeisteRechts.Visiblecollection.Count > 0 Then
                size = New Size(Ownerpanel.Width - LeisteLinks.breite - LeisteRechts.breite - ZiehBereichbreite * 2, Ownerpanel.Height)
            Else
                size = New Size(Ownerpanel.Width - LeisteLinks.breite - ZiehBereichbreite, Ownerpanel.Height)
            End If
            MiddlePanel.Dock = DockStyle.None
            MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
            MiddlePanel.SetBounds(loc.X, loc.Y, size.Width, size.Height)
        ElseIf LeisteRechts.Visiblecollection.Count > 0 Then
            Dim loc As Point = New Point(0, 0)
            Dim size As Size = New Size(Ownerpanel.Width - LeisteRechts.breite - ZiehBereichbreite, Ownerpanel.Height)

            MiddlePanel.Dock = DockStyle.None
            MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom Or AnchorStyles.Right Or AnchorStyles.Top
            MiddlePanel.SetBounds(loc.X, loc.Y, size.Width, size.Height)
        Else
            MiddlePanel.Dock = DockStyle.Fill
        End If
        For i As Integer = Ownerpanel.Controls.Count - 1 To 0 Step -1
            If Not Ownerpanel.Controls(i).Equals(MiddlePanel) Then
                Ownerpanel.Controls.Remove(Ownerpanel.Controls(i))
            End If
        Next
        If Not Ownerpanel.Controls.Contains(MiddlePanel) Then
            Ownerpanel.Controls.Add(MiddlePanel)
        End If
        If LeisteLinks.Visiblecollection.Count > 0 Then
            locatePanelLinks()
        End If
        If LeisteRechts.Visiblecollection.Count > 0 Then
            locatePanelRechts()
        End If
        Ownerpanel.ResumeLayout()
    End Sub

    Public Sub AddTabElementOhneEvents(ByVal t As TabElement, ByVal Aufenthalt As Aufenthaltsort)
        Select Case Aufenthalt
            Case Aufenthaltsort.Fenster
                Throw New NotImplementedException("")
            Case Aufenthaltsort.Links
                LeisteLinks.addOhneEvents(t)
            Case Aufenthaltsort.Rechts
                LeisteRechts.addOhneEvents(t)
        End Select
    End Sub


    Private Sub locatePanelRechts()
        Dim p As Panel = LeisteRechts.getPanel(Ownerpanel.Height - LeisteRechts.Margin)
        PanelR = p
        p.Size = New Size(LeisteRechts.breite, Ownerpanel.Height - LeisteLinks.Margin)
        p.Location = New Point(MiddlePanel.Right + ZiehBereichbreite, LeisteRechts.Margin)
        ZiehPanelRechts = New Panel
        ZiehPanelRechts.Size = New Size(ZiehBereichbreite, Ownerpanel.Height)
        ZiehPanelRechts.Location = New Point(MiddlePanel.Right, 0)
        ZiehPanelRechts.BackColor = ZiehBereichfarbe
        ZiehPanelRechts.Cursor = Cursors.VSplit
        ZiehPanelRechts.Anchor = AnchorStyles.Top Or AnchorStyles.Right Or AnchorStyles.Bottom
        AddHandler ZiehPanelRechts.MouseDown, AddressOf ZiehPanelRechts_MouseDown
        AddHandler ZiehPanelRechts.MouseMove, AddressOf ZiehPanelRechts_MouseMove
        AddHandler ZiehPanelRechts.MouseUp, AddressOf ZiehPanelRechts_MouseUp
        p.Anchor = AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom

        p.Parent = Ownerpanel
        ZiehPanelRechts.Parent = Ownerpanel
    End Sub
    Private Sub locatePanelLinks()
        Dim p As Panel = LeisteLinks.getPanel(Ownerpanel.Height - LeisteLinks.Margin)
        panelL = p
        p.Size = New Size(LeisteLinks.breite, Ownerpanel.Height - LeisteLinks.Margin)
        p.Location = New Point(0, LeisteLinks.Margin)
        ZiehPanelLinks = New Panel
        ZiehPanelLinks.Size = New Size(ZiehBereichbreite, Ownerpanel.Height)
        ZiehPanelLinks.Location = New Point(MiddlePanel.Left - ZiehBereichbreite, 0)
        ZiehPanelLinks.BackColor = ZiehBereichfarbe
        ZiehPanelLinks.Cursor = Cursors.VSplit
        ZiehPanelLinks.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Bottom

        AddHandler ZiehPanelLinks.MouseDown, AddressOf ZiehPanelLinks_MouseDown
        AddHandler ZiehPanelLinks.MouseMove, AddressOf ZiehPanelLinks_MouseMove
        AddHandler ZiehPanelLinks.MouseUp, AddressOf ZiehPanelLinks_MouseUp
        p.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Bottom

        p.Parent = Ownerpanel
        ZiehPanelLinks.Parent = Ownerpanel
    End Sub
    Private Sub LeisteLinks_LostAllElements()
        anzeigen()
    End Sub
    Private Sub LeisteRechts_LostAllElements()
        anzeigen()
    End Sub

    'Rechts Verschieben
    Private MausPositionAnfang As Integer = -1
    Private Sub ZiehPanelRechts_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            MausPositionAnfang = e.X
            For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
                LeisteRechts.Visiblecollection(i).Aktualisierebreite = False
            Next
        End If
    End Sub
    Private Sub ZiehPanelRechts_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If MausPositionAnfang <> -1 Then
                Dim größe As Integer = LeisteRechts.breite - (e.X - MausPositionAnfang)
                If größe < minGröße Then größe = minGröße

                Dim größeLinks As Integer = 0
                If LeisteLinks.Visiblecollection.Count > 0 Then größeLinks = LeisteLinks.breite + ZiehBereichbreite
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Top
                Dim mittebreite As Integer = Ownerpanel.Width - größe - größeLinks - ZiehBereichbreite
                If mittebreite < MiddlepanelMinimumWidth Then
                    größe -= (MiddlepanelMinimumWidth - mittebreite)
                    mittebreite = MiddlepanelMinimumWidth
                End If
                LeisteRechts.breite = größe
                MiddlePanel.Width = mittebreite
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom Or AnchorStyles.Top
                ZiehPanelRechts.Anchor = AnchorStyles.Left Or AnchorStyles.Top
                ZiehPanelRechts.Location = New Point(MiddlePanel.Right, 0)
                ZiehPanelRechts.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom Or AnchorStyles.Top
                PanelR.Anchor = AnchorStyles.Top Or AnchorStyles.Left
                PanelR.Location = New Point(ZiehPanelRechts.Right, PanelR.Location.Y)
                PanelR.Width = größe
                PanelR.Anchor = AnchorStyles.Right Or AnchorStyles.Top Or AnchorStyles.Bottom
            Else
                MausPositionAnfang = e.X
            End If
        End If
    End Sub
    Private Sub ZiehPanelRechts_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        MausPositionAnfang = -1
        For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
            LeisteRechts.Visiblecollection(i).Aktualisierebreite = True
            LeisteRechts.Visiblecollection(i).headerRendern()
            LeisteRechts.Visiblecollection(i).Contentrendern()
        Next
    End Sub

    'Links Verschieben
    Private Sub ZiehPanelLinks_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            MausPositionAnfang = e.X
            For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
                LeisteLinks.Visiblecollection(i).Aktualisierebreite = False
            Next
        End If
    End Sub
    Private Sub ZiehPanelLinks_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            If MausPositionAnfang <> -1 Then
                Dim größe As Integer = LeisteLinks.breite + (e.X - MausPositionAnfang)
                If größe < minGröße Then größe = minGröße

                Dim größerechts As Integer = 0
                If LeisteRechts.Visiblecollection.Count > 0 Then größerechts = LeisteRechts.breite + ZiehBereichbreite
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Top
                Dim mittebreite As Integer = Ownerpanel.Width - größe - größerechts - ZiehBereichbreite
                If mittebreite < MiddlepanelMinimumWidth Then
                    größe -= (MiddlepanelMinimumWidth - mittebreite)
                    mittebreite = MiddlepanelMinimumWidth
                End If
                LeisteLinks.breite = größe
                MiddlePanel.Width = mittebreite
                MiddlePanel.Location = New Point(größe + ZiehBereichbreite, 0)
                MiddlePanel.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom Or AnchorStyles.Top
                ZiehPanelLinks.Anchor = AnchorStyles.Left Or AnchorStyles.Top
                ZiehPanelLinks.Location = New Point(größe, 0)
                ZiehPanelLinks.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Bottom Or AnchorStyles.Top
                panelL.Anchor = AnchorStyles.Top Or AnchorStyles.Left
                panelL.Width = größe
                panelL.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Bottom
            Else
                MausPositionAnfang = e.X
            End If
        End If
    End Sub
    Private Sub ZiehPanelLinks_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs)
        MausPositionAnfang = -1
        For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
            LeisteLinks.Visiblecollection(i).Aktualisierebreite = True
            LeisteLinks.Visiblecollection(i).headerRendern()
            LeisteLinks.Visiblecollection(i).Contentrendern()
        Next
    End Sub

    'Fenster Hinzufügen
    Private Sub FensterHinzufügen(ByVal Sender As Object, ByVal fenster As Window)
        Me.Fenster.Add(fenster)
        If Sender.GetType.ToString = New DockStack(style).GetType.ToString Then
            fenster.NichtNeuesFensterRaisen = True
        End If
        addHandlersWindow(Me.Fenster(Me.Fenster.Count - 1))
        'Hier ist ein kleiner "Trick" notwendig:
        'Das Fenster soll angezeigt werden muss danach auf visible=false geschalten werden.
        'Durch die opacity wird erreicht, dass das fenster shown ist, danach auf visible=false
        'und der benutzer sieht trotzdem kein aufblitzen!
        If Not fenster.Tabs.Visible Then
            fenster.Opacity = 0
        End If
        fenster.Show(WindowsInFormForm)
        If Not fenster.Tabs.Visible Then
            fenster.Visible = False
            fenster.Opacity = 1
        End If
        fenster.Tabs.NurIconsAnzeigen = _nurIcons
    End Sub

    Public Sub FensterAktivieren()
        For Each Fenster As Window In Me.Fenster
            'Hier ist ein kleiner "Trick" notwendig:
            'Das Fenster soll angezeigt werden muss danach auf visible=false geschalten werden.
            'Durch die opacity wird erreicht, dass das fenster shown ist, danach auf visible=false
            'und der benutzer sieht trotzdem kein aufblitzen!
            If Not Fenster.Tabs.Visible Then
                Fenster.Opacity = 0
            End If
            Fenster.Show(WindowsInFormForm)
            If Not Fenster.Tabs.Visible Then
                Fenster.Visible = False
                Fenster.Opacity = 1
            End If
            Fenster.Tabs.NurIconsAnzeigen = _nurIcons
        Next
    End Sub

    Private Sub addHandlersWindow(f As Window)
        AddHandler f.LocationChanged, AddressOf LocationFensterChanged
        AddHandler f.FensterHinzufügenEv, AddressOf FensterHinzufügen
        AddHandler f.FormClosing, AddressOf fensterClosing
    End Sub

    Private Sub fensterClosing(ByVal sender As Window, ByVal e As FormClosingEventArgs)
        If Not e.Cancel Then
            Fenster.Remove(sender)
            RemoveHandler sender.FensterHinzufügenEv, AddressOf FensterHinzufügen
        End If
    End Sub

    Private timerVerschieben As Timer
    Private fensterRollOverPosition As Integer = -1
    Private RechtsRollOverPosition As Integer = -1
    Private LinksRollOverPosition As Integer = -1
    Private OverpicR, OverpicL As PictureBox
    Private letzteSTRGPosition As Boolean
    Private Sub LocationFensterChanged(ByVal sender As Window, ByVal e As EventArgs)
        If Not nichtLocationChangedAufrufen Then
            If sender.start Then Return
            If sender.OnWindowClosing Then Return

            If Cursor.Current = Cursors.SizeWE Or
               Cursor.Current = Cursors.SizeNWSE Or
               Cursor.Current = Cursors.SizeNS Or
               Cursor.Current = Cursors.SizeNESW Then
                Exit Sub
            End If

            If My.Computer.Keyboard.CtrlKeyDown Then
                RechtsFensterLeave()
                LinksFensterLeave()
                If LeisteLinks IsNot Nothing AndAlso LeisteLinks.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
                        LeisteLinks.Visiblecollection(i).OnFensterLeave()
                    Next
                    LeisteLinks.OnFensterLeave(panelL)
                End If
                If LeisteRechts IsNot Nothing AndAlso LeisteRechts.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
                        LeisteRechts.Visiblecollection(i).OnFensterLeave()
                    Next
                    LeisteRechts.OnFensterLeave(PanelR)
                End If
                For i As Integer = 0 To Fenster.Count - 1
                    Fenster(i).OnFensterLeave()
                Next
                fensterRollOverPosition = -1
                RechtsRollOverPosition = -1
                LinksRollOverPosition = -1
                letzteSTRGPosition = True
                Exit Sub
            End If
            letzteSTRGPosition = False

            If sender.Opacity = 1 Then sender.Opacity = FormVerschiebenOpacity

            'Fenster testen
            Dim istOver As Integer = -1
            For i As Integer = 0 To Fenster.Count - 1
                If Not Fenster(i).Equals(sender) Then
                    Dim p As Point = Fenster(i).PointToClient(Cursor.Position)
                    If Fenster(i).NurEinElement Then
                        If p.X > 0 And p.Y > -Fenster(i).Height + Fenster(i).ClientSize.Height And p.Y < 0 And p.X < Fenster(i).ClientSize.Width Then
                            Fenster(i).OnFensterOver()
                            istOver = i
                            Exit For
                        End If
                    Else
                        If p.X > 0 And p.Y > 0 And p.Y < 20 And p.X < Fenster(i).ClientSize.Width Then
                            Fenster(i).OnFensterOver()
                            istOver = i
                            Exit For
                        End If
                    End If
                End If
            Next

            For i As Integer = 0 To Fenster.Count - 1
                If i <> istOver Then
                    Fenster(i).OnFensterLeave()
                End If
            Next
            fensterRollOverPosition = istOver
            If fensterRollOverPosition = -1 Then
                Dim hatFenster As Boolean = False 'gibt an ob das fenster schon links oder rechts eingefügt wurde
                'Rechts testen
                istOver = -1
                If LeisteRechts IsNot Nothing AndAlso LeisteRechts.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
                        Dim panel As Panel = LeisteRechts.Visiblecollection(i).headerPanel
                        Dim p As Point = panel.PointToClient(Cursor.Position)
                        If p.X > 0 And p.Y > 0 And p.Y < panel.Height And p.X < panel.Width Then
                            LeisteRechts.Visiblecollection(i).OnFensterOver()
                            hatFenster = True
                            istOver = i
                            Exit For
                        End If
                    Next
                    If istOver = -1 Then
                        'kein header getroffen
                        Dim p As Point = PanelR.PointToClient(Cursor.Position)
                        If p.X > 0 And p.X < PanelR.Width And p.Y > 0 And p.Y < PanelR.Height Then
                            LeisteRechts.OnFensterOver(PanelR)
                            hatFenster = True
                            istOver = -3
                        End If
                    End If
                    For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
                        If i <> istOver Then
                            LeisteRechts.Visiblecollection(i).OnFensterLeave()
                        End If
                    Next
                    RechtsFensterLeave()
                    If istOver <> -3 Then
                        LeisteRechts.OnFensterLeave(PanelR)
                    End If
                    RechtsRollOverPosition = istOver
                Else
                    Dim p As Point = MiddlePanel.PointToClient(Cursor.Position)
                    If p.Y < MiddlePanel.Height And p.X > MiddlePanel.Width - sender.ClientSize.Width And p.X < MiddlePanel.Width Then
                        If OverpicR Is Nothing Then
                            OverpicR = New PictureBox
                            OverpicR.Size = New Size(sender.ClientSize.Width, MiddlePanel.Height)
                            OverpicR.Location = New Point(MiddlePanel.Width - sender.ClientSize.Width, 0)
                            OverpicR.Image = New Bitmap(OverpicR.Width, OverpicR.Height)
                            Dim bhelp As New Bitmap(MiddlePanel.Width, MiddlePanel.Height)
                            MiddlePanel.DrawToBitmap(bhelp, New Rectangle(0, 0, MiddlePanel.Width, MiddlePanel.Height))
                            If toolstrip2 IsNot Nothing AndAlso toolstrip2.Visible = True Then
                                'Bugfix: Sonst hat er den Toolstrip aus irgendeinem Grund immer nicht mitgemalt. So malt er den manuel. Keine Ahnung warum das nicht von sich aus schon ging...
                                toolstrip2.DrawToBitmap(bhelp, New Rectangle(MiddlePanel.PointToClient(toolstrip2.PointToScreen(New Point(0, 0))), toolstrip2.Size))
                            End If
                            Using g As Graphics = Graphics.FromImage(OverpicR.Image)
                                g.DrawImage(bhelp, New Rectangle(0, 0, OverpicR.Width, OverpicR.Height), New Rectangle(MiddlePanel.Width - OverpicR.Width, 0, OverpicR.Width, MiddlePanel.Height), GraphicsUnit.Pixel)
                                Dim l As New LinearGradientBrush(New Rectangle(0, 0, OverpicR.Width, OverpicR.Height), NewDockStackInsertColor1, NewDockStackInsertColor2, LinearGradientMode.Vertical)
                                g.FillRectangle(l, 0, 0, OverpicR.Width, OverpicR.Height)
                            End Using
                            OverpicR.Parent = MiddlePanel
                            OverpicR.BringToFront()
                        End If
                        RechtsRollOverPosition = -2
                        hatFenster = True
                    Else
                        RechtsRollOverPosition = -1
                        RechtsFensterLeave()
                    End If
                    If istOver <> -3 Then
                        LeisteRechts.OnFensterLeave(PanelR)
                    End If
                End If

                'Links testen
                istOver = -1
                If LeisteLinks IsNot Nothing AndAlso LeisteLinks.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
                        Dim panel As Panel = LeisteLinks.Visiblecollection(i).headerPanel
                        Dim p As Point = panel.PointToClient(Cursor.Position)
                        If p.X > 0 And p.Y > 0 And p.Y < panel.Height And p.X < panel.Width Then
                            LeisteLinks.Visiblecollection(i).OnFensterOver()
                            hatFenster = True
                            istOver = i
                            Exit For
                        End If
                    Next
                    If istOver = -1 Then
                        'kein header getroffen
                        Dim p As Point = panelL.PointToClient(Cursor.Position)
                        If p.X > 0 And p.X < panelL.Width And p.Y > 0 And p.Y < panelL.Height Then
                            LeisteLinks.OnFensterOver(panelL)
                            hatFenster = True
                            istOver = -3
                        End If
                    End If
                    For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
                        If i <> istOver Then
                            LeisteLinks.Visiblecollection(i).OnFensterLeave()
                        End If
                    Next
                    If istOver <> -3 Then
                        LeisteLinks.OnFensterLeave(panelL)
                    End If
                    LinksFensterLeave()
                    LinksRollOverPosition = istOver
                Else
                    Dim p As Point = MiddlePanel.PointToClient(Cursor.Position)
                    If p.Y < MiddlePanel.Height And p.X > 0 And p.X < sender.ClientSize.Width Then
                        If OverpicL Is Nothing Then
                            OverpicL = New PictureBox
                            OverpicL.Size = New Size(sender.ClientSize.Width, MiddlePanel.Height)
                            OverpicL.Location = New Point(0, 0)
                            OverpicL.Image = New Bitmap(OverpicL.Width, OverpicL.Height)
                            MiddlePanel.DrawToBitmap(OverpicL.Image, New Rectangle(0, 0, OverpicL.Width, MiddlePanel.Height))
                            If toolstrip2 IsNot Nothing AndAlso toolstrip2.Visible = True Then
                                'Bugfix: Sonst hat er den Toolstrip aus irgendeinem Grund immer nicht mitgemalt. So malt er den manuel. Keine Ahnung warum das nicht von sich aus schon ging...
                                toolstrip2.DrawToBitmap(OverpicL.Image, New Rectangle(MiddlePanel.PointToClient(toolstrip2.PointToScreen(New Point(0, 0))), toolstrip2.Size))
                            End If
                            Using g As Graphics = Graphics.FromImage(OverpicL.Image)
                                Dim l As New LinearGradientBrush(New Rectangle(0, 0, OverpicL.Width, OverpicL.Height), NewDockStackInsertColor1, NewDockStackInsertColor2, LinearGradientMode.Vertical)
                                g.FillRectangle(l, 0, 0, OverpicL.Width, OverpicL.Height)
                            End Using
                            OverpicL.Parent = MiddlePanel
                            OverpicL.BringToFront()
                        End If
                        LinksRollOverPosition = -2
                        hatFenster = True
                    Else
                        LinksRollOverPosition = -1
                        LinksFensterLeave()
                    End If
                    If istOver <> -3 Then
                        LeisteLinks.OnFensterLeave(panelL)
                    End If
                End If
            Else
                LinksFensterLeave()
                RechtsFensterLeave()
                If LeisteLinks IsNot Nothing AndAlso LeisteLinks.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteLinks.Visiblecollection.Count - 1
                        LeisteLinks.Visiblecollection(i).OnFensterLeave()
                    Next
                    LeisteLinks.OnFensterLeave(panelL)
                End If
                If LeisteRechts IsNot Nothing AndAlso LeisteRechts.Visiblecollection.Count > 0 Then
                    For i As Integer = 0 To LeisteRechts.Visiblecollection.Count - 1
                        LeisteRechts.Visiblecollection(i).OnFensterLeave()
                    Next
                    LeisteRechts.OnFensterLeave(PanelR)
                End If
                LinksRollOverPosition = -1
                RechtsRollOverPosition = -1
            End If
            'MouseUp abfangen
            If timerVerschieben Is Nothing Then
                timerVerschieben = New Timer
                AddHandler timerVerschieben.Tick, Sub()
                                                      If Not Maus.MausGeklickt(MouseButtons.Left) Then
                                                          FensterVerschiebenEnde(sender)
                                                          If timerVerschieben IsNot Nothing Then
                                                              timerVerschieben.Enabled = False
                                                              timerVerschieben.Dispose()
                                                              timerVerschieben = Nothing
                                                          End If
                                                      Else
                                                          If My.Computer.Keyboard.CtrlKeyDown <> letzteSTRGPosition Then
                                                              LocationFensterChanged(sender, EventArgs.Empty)
                                                          End If
                                                      End If
                                                  End Sub
                timerVerschieben.Interval = 1
                timerVerschieben.Enabled = True
            End If
        End If
    End Sub

    Private Sub FensterVerschiebenEnde(ByVal sender As Window)
        If fensterRollOverPosition <> -1 Then
            Dim f As Window = Fenster(fensterRollOverPosition)
            If f.NurEinElement Then
                f.OnFensterLeave()
                f.add(sender.Tabs)
                sender.OnWindowClosing = True
                sender.Close()
                f.Tabs.Contentrendern()
            Else
                f.Insert(sender.Tabs)
                sender.OnWindowClosing = True
                sender.Close()
                f.Tabs.Contentrendern()
            End If
        ElseIf RechtsRollOverPosition <> -1 Then
            Select Case RechtsRollOverPosition
                Case -2 'Rechts neu einfügen
                    RechtsFensterLeave()
                    LeisteRechts.breite = sender.Tabs.breite
                    LeisteRechts.add(sender.Tabs)
                    sender.OnWindowClosing = True
                    sender.Close()
                    anzeigen()
                Case -3 'Als neues Tabelement einfügen
                    LeisteRechts.Insert(sender.Tabs, PanelR)
                    sender.OnWindowClosing = True
                    sender.Close()
                    Ownerpanel.Controls.Remove(PanelR)
                    Ownerpanel.Controls.Remove(ZiehPanelRechts)
                    locatePanelRechts()
                Case Else 'In Tabelement einfügen
                    Dim t As TabElement = LeisteRechts.Visiblecollection(RechtsRollOverPosition)
                    t.Insert(sender.Tabs)
                    sender.OnWindowClosing = True
                    sender.Close()
                    t.Contentrendern()
            End Select
        ElseIf LinksRollOverPosition <> -1 Then
            Select Case LinksRollOverPosition
                Case -2 'Rechts neu einfügen
                    LinksFensterLeave()
                    LeisteLinks.breite = sender.Tabs.breite
                    LeisteLinks.add(sender.Tabs)
                    sender.OnWindowClosing = True
                    sender.Close()
                    anzeigen()
                Case -3 'Als neues Tabelement einfügen
                    LeisteLinks.Insert(sender.Tabs, panelL)
                    sender.OnWindowClosing = True
                    sender.Close()
                    Ownerpanel.Controls.Remove(panelL)
                    Ownerpanel.Controls.Remove(ZiehPanelLinks)
                    locatePanelLinks()
                Case Else 'In Tabelement einfügen
                    Dim t As TabElement = LeisteLinks.Visiblecollection(LinksRollOverPosition)
                    t.Insert(sender.Tabs)
                    sender.OnWindowClosing = True
                    sender.Close()
                    t.Contentrendern()
            End Select
        Else
            sender.Opacity = 1
        End If
    End Sub

    Private Sub LinksFensterLeave()
        If OverpicL IsNot Nothing Then
            MiddlePanel.Controls.Remove(OverpicL)
            OverpicL.Dispose()
            OverpicL = Nothing
        End If
    End Sub
    Private Sub RechtsFensterLeave()
        If OverpicR IsNot Nothing Then
            MiddlePanel.Controls.Remove(OverpicR)
            OverpicR.Dispose()
            OverpicR = Nothing
        End If
    End Sub

    Private Sub StyleChanged(sender As Object, e As EventArgs)
        LeisteLinks.StyleChanged()
        LeisteRechts.StyleChanged()
        If Fenster IsNot Nothing Then
            For Each w As Window In Fenster
                w.DesignStyleChanged()
            Next
        End If
        Me.ZiehBereichfarbe = style.ToolStripHeaderBackcolor
        If ZiehPanelLinks IsNot Nothing Then
            ZiehPanelLinks.BackColor = Me.ZiehBereichfarbe
        End If
        If ZiehPanelRechts IsNot Nothing Then
            ZiehPanelRechts.BackColor = Me.ZiehBereichfarbe
        End If
    End Sub

    Public Sub Clear(ohneEvents As Boolean)
        For Each w As Window In Fenster
            w.clear()
        Next
        If ohneEvents Then Fenster.Clear()

        If Not ohneEvents Then CloseAll()
        LeisteLinks.clear(ohneEvents)
        LeisteRechts.clear(ohneEvents)
        If Not ohneEvents Then
            Ownerpanel.Controls.Remove(PanelR)
            Ownerpanel.Controls.Remove(PanelR)
            ganzMiddlePanel.Location = New Point(0, 0)
            If ZiehPanelRechts IsNot Nothing AndAlso Ownerpanel.Controls.Contains(ZiehPanelRechts) Then
                Ownerpanel.Controls.Remove(ZiehPanelRechts)
            End If
            Ownerpanel.Controls.Remove(panelL)
            If ZiehPanelLinks IsNot Nothing AndAlso Ownerpanel.Controls.Contains(ZiehPanelLinks) Then
                Ownerpanel.Controls.Remove(ZiehPanelLinks)
            End If
        End If
    End Sub

    Public Sub CloseAll()
        Do Until Fenster.Count = 0
            Fenster(0).Close()
        Loop
    End Sub
    Public Sub Minimize()
        For i As Integer = 0 To Fenster.Count - 1
            Fenster(i).TopMost = False
            Fenster(i).WindowState = FormWindowState.Minimized
        Next
    End Sub
    Public Sub Maximize()
        For i As Integer = 0 To Fenster.Count - 1
            If WindowsInFormForm Is Nothing Then Fenster(i).TopMost = True
            Fenster(i).WindowState = FormWindowState.Normal
            Fenster(i).ContentRendern()
            Fenster(i).Opacity = 1
        Next
    End Sub

    Public Sub SpeichereAktuellenZustand(writer As BinaryWriter)
        Dim tabsList As New List(Of TabElement)
        Dim loc As New List(Of Aufenthaltsort)
        Dim fensterzähler As Integer = 0

        writer.Write(LeisteLinks.breite)
        writer.Write(LeisteRechts.breite)

        If LeisteLinks.Invisiblecollection.Count > 0 Then
            For i As Integer = 0 To LeisteLinks.Invisiblecollection.Count - 1
                tabsList.Add(LeisteLinks.Invisiblecollection(i))
                loc.Add(Aufenthaltsort.Links)
            Next
        End If
        If LeisteRechts.Invisiblecollection.Count > 0 Then
            For i As Integer = 0 To LeisteRechts.Invisiblecollection.Count - 1
                tabsList.Add(LeisteRechts.Invisiblecollection(i))
                loc.Add(Aufenthaltsort.Rechts)
            Next
        End If
        If Fenster.Count > 0 Then
            For i As Integer = 0 To Fenster.Count - 1
                tabsList.Add(Fenster(i).Tabs)
                loc.Add(Aufenthaltsort.Fenster)
            Next
        End If

        writer.Write(tabsList.Count)

        For i As Integer = 0 To tabsList.Count - 1
            writer.Write(CInt(loc(i)))
            tabsList(i).export(writer)
            If loc(i) = Aufenthaltsort.Fenster Then
                writer.Write(Fenster(fensterzähler).Location.X)
                writer.Write(Fenster(fensterzähler).Location.Y)
                writer.Write(Fenster(fensterzähler).Width)
                writer.Write(Fenster(fensterzähler).Height)
                fensterzähler += 1
            End If
        Next
    End Sub

    Public Sub RefreshWindows()
        For Each w As Window In Fenster
            w.Width += 1 'ganz böser Workaround. Zum verhindern von renderfehlern.
            w.Width -= 1
        Next
    End Sub

    Public Sub UpdateContentAvailableChanges()
        LeisteLinks.UpdateContentAvailableChanges()
        LeisteRechts.UpdateContentAvailableChanges()

        For Each w As Window In Fenster
            w.UpdateContentAvailableChanges()
        Next
        'If initWindows IsNot Nothing Then
        '    For Each w As WindowStartInformation In initWindows
        '        w.UpdateContentAvailableChanges()
        '    Next
        'End If
    End Sub
End Class

Public Enum Aufenthaltsort
    Links
    Rechts
    Fenster
End Enum

Public Class WindowStartInformation
    Public Tab As TabElement
    Public Pos As Point
    Public Größe As Size

    Public Sub New(t As TabElement, p As Point, s As Size)
        Me.Tab = t
        Me.Pos = p
        Me.Größe = s
    End Sub

    Public Sub UpdateContentAvailableChanges()
        Dim changes As TabElementContentAvailableChangeUntersuchung = Tab.UntersucheContentAvailableChanges
        If changes.AnzahlVisibleNeu = 0 Then
            changes.LostAllElements = True
        End If
        Tab.WendeChangesAn(changes)
    End Sub

End Class
