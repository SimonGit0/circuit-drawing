Imports System.IO
Public Class WindowRendererContentConfig

    Private myContents As List(Of Content)
    Private windowRenderer As WindowRenderer
    Private OnInitWindowRenderer As Boolean = False
    Private WindowRendererPerspektiven As MultiPerspektivenManager(Of Integer)
    Private fensterItems() As ToolStripMenuItem
    Private nichtFensterCheckedChangedAuslösen As Boolean = False
    Private parentForm As Form

    Public Sub New()
        myContents = New List(Of Content)
    End Sub

    Public Sub addContent(c As Content)
        myContents.Add(c)
    End Sub

    Public Function ANZAHL_WINDOWRENDERER_FENSTER() As Integer
        Return myContents.Count
    End Function

    Public Function getNumberOfName(name As String) As Integer
        For i As Integer = 0 To myContents.Count - 1
            If myContents(i).Name = name Then
                Return i
            End If
        Next
        Return -1
    End Function

    Public Function getNameOfNumber(nr As Integer) As String
        Return myContents(nr).Name
    End Function

    Public Function getContent(nr As Integer) As Content
        Return myContents(nr)
    End Function

    Public Function getContent(ByVal n As String) As Content
        Return getContent(getNumberOfName(n))
    End Function

#Region "WindowRenderer"
    Public Sub initWindowRenderer(parentPanel As Panel, middlePanel As Panel, parentForm As Form, dateiPfad_lastConfig As String, FensterToolStrip As ToolStripMenuItem, nurIconsAnzeigen As Boolean, standardvorlage As VirtualWindowRenderer)
        Me.parentForm = parentForm

        OnInitWindowRenderer = True

        windowRenderer = New WindowRenderer(DesignStyle.DefaultStyle(), parentPanel, middlePanel)
        Me.windowRenderer.LeisteRechts.breite = 200
        Me.windowRenderer.LeisteLinks.breite = 250
        Me.windowRenderer.WindowsInFormForm = parentForm
        Me.windowRenderer.NurIconsAnzeigen = nurIconsAnzeigen

        Me.WindowRendererPerspektiven = New MultiPerspektivenManager(Of Integer)({0})

        ReDim fensterItems(myContents.Count - 1)

        For i As Integer = 0 To myContents.Count - 1
            Dim item As New ToolStripMenuItem()
            item.Text = myContents(i).Name
            item.Image = myContents(i).Icon
            item.Checked = myContents(i).VisibleInternal
            item.CheckOnClick = True
            item.Tag = myContents(i)
            AddHandler item.CheckedChanged, AddressOf MenuStrip_FensterCheckedChanged
            fensterItems(i) = item
            AddHandler myContents(i).VisibleChanged, AddressOf ContentVisibleChange
        Next

        If File.Exists(dateiPfad_lastConfig) Then
            Try
                ladeausFileEin(parentForm, dateiPfad_lastConfig)
            Catch
                File.Delete(dateiPfad_lastConfig)
                Me.windowRenderer.CloseAll()
                Me.windowRenderer = New WindowRenderer(DesignStyle.DefaultStyle(), parentPanel, middlePanel)
                Me.windowRenderer.LeisteRechts.breite = 200
                Me.windowRenderer.LeisteLinks.breite = 250
                Me.windowRenderer.WindowsInFormForm = parentForm
                Me.windowRenderer.NurIconsAnzeigen = nurIconsAnzeigen
                LadeStandartEin(parentForm, standardvorlage)
            End Try
        Else
            LadeStandartEin(parentForm, standardvorlage)
        End If

        FensterToolStrip.DropDownItems.AddRange(fensterItems)

        OnInitWindowRenderer = False

        Dim vorlage As VirtualWindowRenderer = WindowRendererPerspektiven.getAktivePerspektive()
        LadeWindowrendererVorlageEin(vorlage)

        AddHandler parentForm.FormClosing, AddressOf ParentFormClosing
    End Sub

    Private Sub ParentFormClosing(sender As Object, e As FormClosingEventArgs)
        'speicherfenster()
        windowRenderer.CloseAll()
        e.Cancel = False
    End Sub

    Private Sub ladeausFileEin(parentForm As Form, pfad As String)
        Try
            Me.windowRenderer.WindowsInFormForm = parentForm

            WindowRendererPerspektiven.initAusDatei(pfad, Me, False)

        Catch e As Exception
            Throw e
        End Try
    End Sub

    Private Sub LadeStandartEin(parentForm As Form, standardvorlage As VirtualWindowRenderer)
        Me.windowRenderer.WindowsInFormForm = parentForm

        WindowRendererPerspektiven.initPerspektive(0, standardvorlage)
    End Sub

    Private Sub MenuStrip_FensterCheckedChanged(ByVal sender As Object, ByVal e As EventArgs)
        If Not nichtFensterCheckedChangedAuslösen Then
            Dim item As ToolStripMenuItem = CType(sender, ToolStripMenuItem)
            TryCast(item.Tag, Content).Visible = item.Checked
        End If
    End Sub

    Private Sub ContentVisibleChange(ByVal sender As Object, ByVal e As EventArgs)
        If parentForm IsNot Nothing AndAlso parentForm.IsHandleCreated AndAlso Not OnInitWindowRenderer Then
            Dim c As Content = CType(sender, Content)
            If c.VisibleInternal = False Then
                Dim nr As Integer = getNumberOfName(c.Name)
                fensterItems(nr).Checked = False
            End If
        End If
    End Sub

    Private Sub LadeWindowrendererVorlageEin(vorlage As VirtualWindowRenderer)
        vorlage.LadeEin(windowRenderer)

        Dim vorher As Boolean = nichtFensterCheckedChangedAuslösen
        nichtFensterCheckedChangedAuslösen = True
        For Each Content As Content In myContents
            fensterItems(getNumberOfName(Content.Name)).Checked = Content.VisibleInternal
        Next
        nichtFensterCheckedChangedAuslösen = vorher
    End Sub
#End Region

End Class
