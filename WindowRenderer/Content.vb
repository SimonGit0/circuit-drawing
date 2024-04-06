''' <summary>
''' Der Content, also ein panel, welches irgendwo angezeigt wird
''' </summary>
''' <remarks></remarks>
Public Class Content
    Public StandardHöhe As Integer
    Public StandardBreite As Integer

    Public MaximumHeight As Integer
    Public InhaltsPanel As Control
    Public Name As String
    Public Icon As Bitmap = SystemIcons.Information.ToBitmap
    Private _visible As Boolean = True
    Public notSelect As Boolean = False

    Private _AvailableModusAnsicht As Boolean = True
    Private _AvailableEbene As Boolean = True
    Private _AvailableTool As Boolean = True

    Public Property Visible As Boolean
        Get
            Return _visible AndAlso _AvailableEbene AndAlso _AvailableModusAnsicht AndAlso _AvailableTool
        End Get
        Set(ByVal value As Boolean)
            If _visible <> value Then
                _visible = value
                RaiseEvent VisibleChanged(Me, New VisibleChangedEventArgs(notSelect))
            End If
        End Set
    End Property

    Public ReadOnly Property VisibleInternal As Boolean
        Get
            Return _visible
        End Get
    End Property

    ''' <summary>
    ''' Nur zum Initialisieren! Normalerweise nicht verwenden!
    ''' </summary>
    ''' <param name="visible"></param>
    ''' <remarks></remarks>
    Public Sub setVisibleInit(visible As Boolean)
        Me._visible = visible
    End Sub

    Public Property AvailableModusAnsicht As Boolean
        Get
            Return _AvailableModusAnsicht
        End Get
        Set(value As Boolean)
            _AvailableModusAnsicht = value
        End Set
    End Property
    Public Property AvailableEbene As Boolean
        Get
            Return _AvailableEbene
        End Get
        Set(value As Boolean)
            _AvailableEbene = value
        End Set
    End Property
    Public Property AvailableTool As Boolean
        Get
            Return _AvailableTool
        End Get
        Set(value As Boolean)
            _AvailableTool = value
        End Set
    End Property

    Public ReadOnly Property Available As Boolean
        Get
            Return AvailableEbene AndAlso AvailableModusAnsicht AndAlso AvailableTool
        End Get
    End Property

    ''' <summary>
    ''' Für das einladen von Standartvorlagen
    ''' Rasit das event visiblechanged
    ''' GANZ VORSICHTIG SEIN, NICHT BENUTZEN NACH MÖGLICHKEIT!
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub RaiseVisibleChanged()
        RaiseEvent VisibleChanged(Me, New VisibleChangedEventArgs(notSelect))
    End Sub

    Private _mywidth As Integer
    ''' <summary>
    ''' Die vom Content bevorzugte Breite
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FavoriteWidth As Integer
        Get
            Return _mywidth
        End Get
        Set(ByVal value As Integer)
            If value < 10 Then value = 10 'minimum size abfrage
            _mywidth = value
        End Set
    End Property

    ''' <summary>
    ''' Die vom Content bevorzugte Höhe
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FavoriteHeight As Integer

    Public Property LinksPosition As Integer = 0

    Public Property Maximiert As Boolean

    ''' <summary>
    ''' Diese Sub kann aufgerufen werden, um Tooltips, Dropdowns oder Contextmenus zu schließen, wenn sich das Fenster minimiert.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub OnClosing()
        If InhaltsPanel IsNot Nothing Then
            RaiseEvent PanelShouldCloseContextMenus(Me, EventArgs.Empty)
        End If
    End Sub

    Public Sub New(ByVal P As Control, ByVal hatMaximum As Boolean, standardbreite As Integer, standardhöhe As Integer)
        Me.InhaltsPanel = P
        Me.Name = P.Text
        Me.StandardBreite = standardbreite
        If standardhöhe = -1 Then
            Me.StandardHöhe = P.Height
        Else
            Me.StandardHöhe = standardhöhe
        End If

        If hatMaximum Then
            Me.MaximumHeight = P.Height
        Else
            Me.MaximumHeight = Integer.MaxValue
        End If
        Me.FavoriteWidth = P.Width
        Me.FavoriteHeight = Math.Min(Me.MaximumHeight, CInt(My.Computer.Screen.WorkingArea.Height * 0.75))
    End Sub
    Public Sub New(p As Control, hatMaximum As Boolean, standardbreite As Integer)
        Me.New(p, hatMaximum, standardbreite, -1)
    End Sub
    Public Event VisibleChanged(ByVal sender As Object, ByVal e As VisibleChangedEventArgs)
    Public Event ImTabElementSelected(sender As Object, e As EventArgs)
    Public Event PanelShouldCloseContextMenus(sender As Object, e As EventArgs)

    Public Sub OnImTabElementSelected()
        RaiseEvent ImTabElementSelected(Me, EventArgs.Empty)
    End Sub
End Class
