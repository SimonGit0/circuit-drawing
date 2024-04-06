Imports System.Threading

Public Class PopUpWindow
    Public myDropdown As ToolStripDropDown
    Private closeThread As Thread
    Private openTimer As System.Windows.Forms.Timer
    Private closeThreadAbbrechen As Boolean = False

    Private myOpacity As Double
    Private myControls As List(Of Control)
    Private _isClosing As Boolean = False
    Public ReadOnly Property isClosing As Boolean
        Get
            Return _isClosing
        End Get
    End Property
    Public Sub New()
        Me.New(New ToolStripDropDown())
    End Sub

    Public Sub New(ByVal dp As ToolStripDropDown)
        myDropdown = dp
        Me.Richtung = ToolStripLayoutStyle.VerticalStackWithOverflow
        myOpacity = 1
        myDropdown.AutoClose = False
        Me.CloseDelay = 0.05
        Me.OpenDelay = 0.1
        Me.Schatten = True
        Me.CloseWithEffekt = True
        Me.CanChangeSizeDirections = AnchorStyles.None
        AddHandler myDropdown.KeyUp, AddressOf _KeyUp
        myControls = New List(Of Control)
    End Sub

#Region "Öffnen, schließen"
    Public Sub Open(ctr As Control, ByVal posRel As Point, ByVal withEffekt As Boolean)
        Open(ctr, Nothing, posRel, withEffekt)
    End Sub

    Public Sub Open(ByVal screenLocation As Point, ByVal withEffekt As Boolean)
        Open(Nothing, Nothing, screenLocation, withEffekt)
    End Sub

    Public Sub Open(ctr_rel As Control, ctr As Control, ByVal screenLocation As Point, ByVal withEffekt As Boolean)
        RaiseEvent Opening(Me, EventArgs.Empty)
        If withEffekt Then
            beendeÖffnen()
            beendeSchließen()
            openTimer = New System.Windows.Forms.Timer()
            openTimer.Interval = 1
            myDropdown.Opacity = 0
            AddHandler openTimer.Tick, Sub()
                                           If myDropdown.Opacity >= myOpacity Then
                                               myDropdown.Opacity = myOpacity
                                               RaiseEvent ClosingOrOpening(Me, New OpacityPercentEventArgs(myDropdown.Opacity))
                                               openTimer.Stop()
                                           Else
                                               myDropdown.Opacity += OpenDelay
                                               RaiseEvent ClosingOrOpening(Me, New OpacityPercentEventArgs(myDropdown.Opacity))
                                           End If
                                       End Sub
            openTimer.Start()
        End If
        If ctr IsNot Nothing Then
            Dim s As Screen = Screen.FromHandle(ctr.Handle)
            Dim max As New Point(screenLocation.X + myDropdown.Width - 1, screenLocation.Y + myDropdown.Height - 1)
            If max.X > s.WorkingArea.Right Then
                screenLocation.X -= max.X - s.WorkingArea.Right
            End If
            If max.Y > s.WorkingArea.Bottom Then
                screenLocation.Y -= max.Y - s.WorkingArea.Height
            End If

        End If
        If ctr_rel IsNot Nothing Then
            myDropdown.Show(ctr_rel, screenLocation)
        Else
            myDropdown.Show(screenLocation)
        End If
    End Sub

    Private Sub beendeÖffnen()
        If openTimer IsNot Nothing AndAlso openTimer.Enabled Then
            openTimer.Stop()
            openTimer.Dispose()
        End If
    End Sub

    Private Sub beendeSchließen()
        If closeThread IsNot Nothing AndAlso closeThread.IsAlive Then
            closeThreadAbbrechen = True
            While closeThread IsNot Nothing AndAlso closeThread.IsAlive
                Application.DoEvents()
                Thread.Sleep(10)
            End While
            closeThreadAbbrechen = False
        End If
    End Sub

    ''' <summary>
    ''' schließt das fenster
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Close()
        Me.Close(False)
    End Sub

    ''' <summary>
    ''' schließt das fenster
    ''' </summary>
    ''' <param name="VerzögerungUnterdrücken">Wenn der eingestellte Close-Effekt in jedem Fall unterdrückt werden soll auf true setzen</param>
    ''' <remarks></remarks>
    Public Sub Close(VerzögerungUnterdrücken As Boolean)
        If Not Me.isOpen Then
            Return
        End If
        Dim e As New FormClosingEventArgs(CloseReason.None, False)
        RaiseEvent Closing(Me, e)
        If Not e.Cancel Then
            If CloseWithEffekt And Not VerzögerungUnterdrücken Then
                beendeÖffnen()
                If Not isClosing Then 'neu hinzugefügt. falls das probleme gibt wieder löschen?
                    beendeSchließen()
                    closeThread = New Thread(AddressOf CloseTimerTick)
                    closeThread.IsBackground = True
                    closeThread.Start()
                End If
            Else
                beendeÖffnen()
                beendeSchließen()
                myDropdown.Close()
                RaiseEvent Closed(Me, EventArgs.Empty)
            End If
        End If
    End Sub

    Private Sub CloseTimerTick()
        Dim fertig As Boolean = False
        _isClosing = True
        Do
            If closeThreadAbbrechen Then Exit Do
            Thread.Sleep(20)
            If closeThreadAbbrechen Then Exit Do
            myDropdown.Invoke(Sub()
                                  If myDropdown.Opacity <= 0 Then
                                      myDropdown.Close()
                                      RaiseEvent Closed(Me, EventArgs.Empty)
                                      myDropdown.Opacity = myOpacity
                                      RaiseEvent ClosingOrOpening(Me, New OpacityPercentEventArgs(0))
                                      fertig = True
                                  Else
                                      Dim p As Point = myDropdown.PointToClient(Cursor.Position)
                                      myDropdown.Opacity -= CloseDelay
                                      RaiseEvent ClosingOrOpening(Me, New OpacityPercentEventArgs(myDropdown.Opacity))
                                  End If
                              End Sub)
        Loop Until fertig
        _isClosing = False
    End Sub

    Public Sub BeendeÖffnenSchließenEffekt(sichtbar As Boolean)
        beendeÖffnen()
        beendeSchließen()
        If sichtbar Then
            Me.Opacity = myOpacity
        End If
    End Sub
#End Region

#Region "Add"
    ''' <summary>
    ''' fügt ein control zu der liste hinzu
    ''' </summary>
    ''' <param name="c"></param>
    ''' <remarks></remarks>
    Public Sub addControl(ByVal c As Control)
        addControl(c, New Padding(0))
    End Sub

    ''' <summary>
    ''' fügt ein control zu der liste hinzu. Man kann hier noch ein padding für den ControlHost angeben, um die Position des Elements zu verschönern!
    ''' </summary>
    ''' <param name="c"></param>
    ''' <remarks></remarks>
    Public Sub addControl(ByVal c As Control, hostmargin As Padding)
        Dim host As New ToolStripControlHost(c)
        host.Margin = hostmargin
        host.Padding = hostmargin

        c.Margin = New Padding(0)
        c.Padding = New Padding(0)

        myDropdown.Items.Add(host)
        myControls.Add(c)

        addHandlers(c)
    End Sub

    Public Sub addControlOhneAutoSize(ByVal c As Control, hostmargin As Padding)
        Dim host As New ToolStripControlHost(c)
        host.Margin = hostmargin
        host.Padding = hostmargin
        host.AutoSize = False
        host.Size = c.Size

        c.Margin = New Padding(0)
        c.Padding = New Padding(0)

        myDropdown.Items.Add(host)
        myControls.Add(c)

        addHandlers(c)
    End Sub


    Private Sub addHandlers(ByVal c As Control)
        AddHandler c.GotFocus, AddressOf OnGotFocus

        For i As Integer = 0 To c.Controls.Count - 1
            addHandlers(c.Controls(i))
        Next
    End Sub

    Public Sub removeHandlers(ByVal c As Control)
        RemoveHandler c.GotFocus, AddressOf OnGotFocus

        For i As Integer = 0 To c.Controls.Count - 1
            removeHandlers(c.Controls(i))
        Next
    End Sub

    ''' <summary>
    ''' fügt einen Seperator zu der liste hinzu
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub addSeperator()
        Dim s As New ToolStripSeparator
        myDropdown.Items.Add(s)
    End Sub
#End Region

#Region "Propertys"
    ''' <summary>
    ''' Gibt die Richtung an, in der sich das fenster öffnet
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Richtung As ToolStripLayoutStyle
        Get
            Return myDropdown.LayoutStyle
        End Get
        Set(ByVal value As ToolStripLayoutStyle)
            myDropdown.LayoutStyle = value
        End Set
    End Property

    ''' <summary>
    ''' Gibt an, ob das Fenster mit oder Ohne Schatten gemalt werden soll
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Schatten As Boolean
        Get
            Return myDropdown.DropShadowEnabled
        End Get
        Set(ByVal value As Boolean)
            myDropdown.DropShadowEnabled = value
        End Set
    End Property

    ''' <summary>
    ''' Gibt an ob beim Schließen des Fensters ein Effekt angewendet werden soll
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CloseWithEffekt As Boolean

    ''' <summary>
    ''' Gibt einen Wert an, der die Zeit des Öffnen-Effektes charakterisiert. Je näher er an Null ist desto langsamer geht es.
    ''' Typische werte sind im Bereich von 0.001 bis 0.3
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CloseDelay As Double

    ''' <summary>
    ''' Gibt einen Wert an, der die Zeit des Schließen-Effektes charakterisiert. Je näher er an Null ist desto langsamer geht es.
    ''' Typische werte sind im Bereich von 0.001 bis 0.3
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OpenDelay As Double

    ''' <summary>
    ''' Gibt die Deckkraft des Fensters an. 1 enstpricht 100% sichtbar 0 enstspricht total unsichtbar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Opacity() As Double
        Get
            Return myOpacity
        End Get
        Set(ByVal value As Double)
            myOpacity = value
            myDropdown.Opacity = value
        End Set
    End Property

    Public Property ForeColor As Color

    Public Property Tag As Object = Nothing

    Public Property CanChangeSizeDirections() As AnchorStyles

    Public Property AutoSize As Boolean
        Get
            Return myDropdown.AutoSize
        End Get
        Set(ByVal value As Boolean)
            myDropdown.AutoSize = value
        End Set
    End Property

    Public ReadOnly Property Cursor As Cursor
        Get
            Return myDropdown.Cursor
        End Get
    End Property

    ''' <summary>
    ''' Die Minimumsize wird nur in der Höhe unterstützt
    ''' Für die Width setzten Sie bitte die rechtsMin und linksMax Eigenschaften!
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ZiehenMinimumSize As New Size(-1, -1)

    Public Property Padding As Padding
        Get
            Return myDropdown.Padding
        End Get
        Set(ByVal value As Padding)
            myDropdown.Padding = value
        End Set
    End Property

    Public Property BackColor As Color
        Get
            Return myDropdown.BackColor
        End Get
        Set(ByVal value As Color)
            myDropdown.BackColor = value
        End Set
    End Property
#End Region

    ''' <summary>
    ''' Gibt an ob das Fenster zurzeit offen ist
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function isOpen() As Boolean
        Return myDropdown.Visible
    End Function

    ''' <summary>
    ''' Gibt an, ob die maus gerade über dem Tooltip schwebt.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function isMouseOver() As Boolean
        Dim p As Point = Me.PointToClient(Cursor.Position)
        Return Not (p.X < 0 Or p.Y < 0 Or p.X >= myDropdown.Width Or p.Y >= myDropdown.Height)
    End Function

    Public Property Location() As Point
        Get
            Return New Point(myDropdown.Left, myDropdown.Top)
        End Get
        Set(ByVal value As Point)
            myDropdown.SetBounds(value.X, value.Y, myDropdown.Width, myDropdown.Height)
        End Set
    End Property

    Public ReadOnly Property Width As Integer
        Get
            Return myDropdown.Width
        End Get
    End Property

    Public ReadOnly Property Height As Integer
        Get
            Return myDropdown.Height
        End Get
    End Property

    Public Function PointToClient(ByVal value As Point) As Point
        Return myDropdown.PointToClient(value)
    End Function

    Public Function PointToScreen(ByVal value As Point) As Point
        Return myDropdown.PointToScreen(value)
    End Function

    Protected Overridable Sub OnGotFocus(ByVal sender As Object, ByVal e As EventArgs)
        beendeSchließen()
    End Sub

    Public ReadOnly Property Items As ToolStripItemCollection
        Get
            Return myDropdown.Items
        End Get
    End Property

    Private Sub _KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs)
        RaiseEvent KeyUp(Me, e)
    End Sub
#Region "Events"
    Public Event MouseLeave(ByVal sender As Object, ByVal e As EventArgs)
    Public Event Opening(ByVal sender As Object, ByVal e As EventArgs)
    Public Event Closing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
    Public Event Closed(ByVal sender As Object, ByVal e As EventArgs)
    Public Event KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs)
    Public Event ClosingOrOpening(sender As Object, e As OpacityPercentEventArgs)
#End Region
End Class

Public Class OpacityPercentEventArgs
    Inherits EventArgs
    Public Property prozent As Double
    Public Sub New(prozent As Double)
        Me.prozent = prozent
    End Sub
End Class
