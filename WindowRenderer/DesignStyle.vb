Public Class DesignStyle

#Region "Colors"

    Private _SelectedColor As Color
    Public ReadOnly Property SelectedColor As Color
        Get
            Return _SelectedColor
        End Get
    End Property

    Private _DefaultColor As Color
    Public ReadOnly Property DefaultColor As Color
        Get
            Return _DefaultColor
        End Get
    End Property

    Private _scrollpictureboxColor As Color
    Public ReadOnly Property ScrollPictureboxColor As Color
        Get
            Return _scrollpictureboxColor
        End Get
    End Property

    Private _ButtonBackgroundcolorDefault As Color
    Public ReadOnly Property ButtonBackgroundcolorDefault As Color
        Get
            Return _ButtonBackgroundcolorDefault
        End Get
    End Property

    Private _ButtonBackgroundcolorSelected As Color
    Public ReadOnly Property ButtonBackgroundcolorSelected As Color
        Get
            Return _ButtonBackgroundcolorSelected
        End Get
    End Property

    Private _ButtonBackgroundcolorMouseOver As Color
    Public ReadOnly Property ButtonBackgroundcolorMouseOver As Color
        Get
            Return _ButtonBackgroundcolorMouseOver
        End Get
    End Property

    Private _ButtonBackgroundcolorMouseDown As Color
    Public ReadOnly Property ButtonBackgroundcolorMouseDown As Color
        Get
            Return _ButtonBackgroundcolorMouseDown
        End Get
    End Property

    Private _ButtonBorderColor As Color
    Public ReadOnly Property ButtonBorderColor As Color
        Get
            Return _ButtonBorderColor
        End Get
    End Property

    Private _ButtonBorderColorSelected As Color
    Public ReadOnly Property ButtonBorderColorSelected As Color
        Get
            Return _ButtonBorderColorSelected
        End Get
    End Property

    Private _FontColorSelected As Color
    Public ReadOnly Property FontColorSelected As Color
        Get
            Return _FontColorSelected
        End Get
    End Property

    Private _FontColorDefault As Color
    Public ReadOnly Property FontColorDefault As Color
        Get
            Return _FontColorDefault
        End Get
    End Property

    Private _Control As Color
    Public ReadOnly Property Control As Color
        Get
            Return _Control
        End Get
    End Property

    Private _ListboxBackColor As Color
    Public ReadOnly Property ListboxBackColor As Color
        Get
            Return _ListboxBackColor
        End Get
    End Property

    Private _ToolStripHeaderBackcolor As Color
    Public ReadOnly Property ToolStripHeaderBackcolor As Color
        Get
            Return _ToolStripHeaderBackcolor
        End Get
    End Property

    Private _ToolStrip1Backcolor As Color
    Public ReadOnly Property ToolStrip1Backcolor As Color
        Get
            Return _ToolStrip1Backcolor
        End Get
    End Property

    Private _MenuStripBackColor As Color
    Public ReadOnly Property MenuStripBackColor As Color
        Get
            Return _MenuStripBackColor
        End Get
    End Property

    Private _TabControlResizeColor As Color
    Public ReadOnly Property TabControlResizeColor As Color
        Get
            Return _TabControlResizeColor
        End Get
    End Property

    Private _LinealSchriftColor As Color
    Public ReadOnly Property LinealSchriftColor As Color
        Get
            Return _LinealSchriftColor
        End Get
    End Property

    Private _LinealMarkierung As Color
    Public ReadOnly Property LinealMarkierung As Color
        Get
            Return _LinealMarkierung
        End Get
    End Property

    Private _LinealLinienColor As Color
    Public ReadOnly Property LinealLinienColor As Color
        Get
            Return _LinealLinienColor
        End Get
    End Property

    Private _LinealColor As Color
    Public ReadOnly Property Linealcolor As Color
        Get
            Return _LinealColor
        End Get
    End Property

    Private _ScrollPictureboxBorderColor As Color
    Public ReadOnly Property ScrollPictureboxBorderColor As Color
        Get
            Return _ScrollPictureboxBorderColor
        End Get
    End Property

    Private _IconListboxSelectionColor As Color
    Public ReadOnly Property IconListboxSelectionColor As Color
        Get
            Return _IconListboxSelectionColor
        End Get
    End Property

    Private _IconListboxSelectionFilling As Color
    Public ReadOnly Property IconListboxSelectionFilling As Color
        Get
            Return _IconListboxSelectionFilling
        End Get
    End Property

    Private _IconListboxBorder As Color
    Public ReadOnly Property IconListboxBorder As Color
        Get
            Return _IconListboxBorder
        End Get
    End Property

    Private _IconListboxFontColor As Color
    Public ReadOnly Property IconListboxFontColor As Color
        Get
            Return _IconListboxFontColor
        End Get
    End Property

    Private _IconListboxGroupcolor1 As Color
    Public ReadOnly Property IconListboxGroupcolor1 As Color
        Get
            Return _IconListboxGroupcolor1
        End Get
    End Property

    Private _IconListboxGroupcolor2 As Color
    Public ReadOnly Property IconListboxGroupcolor2 As Color
        Get
            Return _IconListboxGroupcolor2
        End Get
    End Property

    Private _EbenenListeItemColor As Color
    Public ReadOnly Property EbenenListeItemColor As Color
        Get
            Return _EbenenListeItemColor
        End Get
    End Property

    Private _SuchProgressbarBottomForeColor As Color
    Public ReadOnly Property SuchProgressbarBottomForeColor As Color
        Get
            Return _SuchProgressbarBottomForeColor
        End Get
    End Property

    Private _SuchProgressbarTopForeColor As Color
    Public ReadOnly Property SuchProgressbarTopForeColor As Color
        Get
            Return _SuchProgressbarTopForeColor
        End Get
    End Property


    Private _TextForecolor As Color
    ''' <summary>
    ''' Die Schriftfarbe für Labels, Checkboxen, Groupboxen, etc. in Windowrenderer-Fenstern.
    ''' Auch die Schriftfarbe der Suchbox.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property TextForeColor As Color
        Get
            Return _TextForecolor
        End Get
    End Property

    Private _SuchboxBackColor As Color
    Public ReadOnly Property SuchboxBackColor As Color
        Get
            Return _SuchboxBackColor
        End Get
    End Property

    Private _SuchBoxSelected As Color
    Public ReadOnly Property SuchBoxSelected As Color
        Get
            Return _SuchBoxSelected
        End Get
    End Property

    Private _SuchBoxMouseOver As Color
    Public ReadOnly Property SuchBoxMouseOver As Color
        Get
            Return _SuchBoxMouseOver
        End Get
    End Property

    Private _ToolStripForecolor As Color
    Public ReadOnly Property ToolStripForecolor As Color
        Get
            Return _ToolStripForecolor
        End Get
    End Property

    Private _ToolStripSelectedForecolor As Color
    Public ReadOnly Property ToolStripSelectedForecolor As Color
        Get
            Return _ToolStripSelectedForecolor
        End Get
    End Property

    Private _ToolStripSelectedBorderColor As Color
    Public ReadOnly Property ToolStripSelectedBorderColor As Color
        Get
            Return _ToolStripSelectedBorderColor
        End Get
    End Property

    Private _ToolStripSelectedColor As Color
    Public ReadOnly Property ToolStripSelectedColor As Color
        Get
            Return _ToolStripSelectedColor
        End Get
    End Property

    Private _ComboboxSelectionFilling As Color
    Public ReadOnly Property ComboboxSelectionFilling As Color
        Get
            Return _ComboboxSelectionFilling
        End Get
    End Property

    Private _ComboboxForeColor As Color
    Public ReadOnly Property ComboboxForecolor As Color
        Get
            Return _ComboboxForeColor
        End Get
    End Property

    Private _comboboxDropDownBackColor As Color
    Public ReadOnly Property comboboxDropDownBackColor As Color
        Get
            Return _comboboxDropDownBackColor
        End Get
    End Property

    Private _ComboboxForeColorSelected As Color
    Public ReadOnly Property ComboboxForeColorSelected As Color
        Get
            Return _ComboboxForeColorSelected
        End Get
    End Property

    Private _ToolstripDropDownColor As Color
    Public ReadOnly Property ToolstripDropDownColor As Color
        Get
            Return _ToolstripDropDownColor
        End Get
    End Property

    Private _EbenenListeForecolor As Color
    Public ReadOnly Property EbenenListeForecolor As Color
        Get
            Return _EbenenListeForecolor
        End Get
    End Property

    Private _ToolStripSeperatorColor1 As Color
    Public ReadOnly Property ToolStripSeperatorColor1 As Color
        Get
            Return _ToolStripSeperatorColor1
        End Get
    End Property

    Private _ToolStripSeperatorColor2 As Color
    Public ReadOnly Property ToolStripSeperatorColor2 As Color
        Get
            Return _ToolStripSeperatorColor2
        End Get
    End Property

    Private _ToolstripBorderColor As Color
    Public ReadOnly Property ToolstripBorderColor As Color
        Get
            Return _ToolstripBorderColor
        End Get
    End Property

    Private _RückgängigSelectionColor As Color
    Public ReadOnly Property RückgängigSelectionColor As Color
        Get
            Return _RückgängigSelectionColor
        End Get
    End Property

    Private _RückgängigForeColor As Color
    Public ReadOnly Property RückgängigForeColor As Color
        Get
            Return _RückgängigForeColor
        End Get
    End Property

    Private _RückgängigBackColor As Color
    Public ReadOnly Property RückgängigBackColor As Color
        Get
            Return _RückgängigBackColor
        End Get
    End Property

    Private _ToolstriptextboxBackColor As Color
    Public ReadOnly Property ToolstriptextboxBackColor As Color
        Get
            Return _ToolstriptextboxBackColor
        End Get
    End Property

    Private _ToolstriptextboxForeColor As Color
    Public ReadOnly Property ToolstriptextboxForeColor As Color
        Get
            Return _ToolstriptextboxForeColor
        End Get
    End Property

    Private _TrackbarTextboxBackColor As Color
    Public ReadOnly Property TrackbarTextboxBackColor As Color
        Get
            Return _TrackbarTextboxBackColor
        End Get
    End Property

    Private _ComboboxBackgroundColor As Color
    Public ReadOnly Property ComboboxBackgroundColor As Color
        Get
            Return _ComboboxBackgroundColor
        End Get
    End Property

    Private _ComboboxBackgroundColorHot As Color
    Public ReadOnly Property ComboboxBackgroundColorHot As Color
        Get
            Return _ComboboxBackgroundColorHot
        End Get
    End Property

    Private _VerlaufsToolBorderColor As Color
    Public ReadOnly Property VerlaufsToolBorderColor As Color
        Get
            Return _VerlaufsToolBorderColor
        End Get
    End Property

    Private _GroupBoxColorInnen As Color
    Public ReadOnly Property GroupBoxColorInnen As Color
        Get
            Return _GroupBoxColorInnen
        End Get
    End Property

    Private _GroupBoxColorAußen As Color
    Public ReadOnly Property GroupBoxColorAußen As Color
        Get
            Return _GroupBoxColorAußen
        End Get
    End Property

    Private _ListboxTopColor As Color
    Public ReadOnly Property ListboxTopColor As Color
        Get
            Return _ListboxTopColor
        End Get
    End Property

    Private _ListboxBottomColor As Color
    Public ReadOnly Property ListboxBottomColor As Color
        Get
            Return _ListboxBottomColor
        End Get
    End Property

    Private _ListboxRahmenColor As Color
    Public ReadOnly Property ListboxRahmenColor As Color
        Get
            Return _ListboxRahmenColor
        End Get
    End Property

    Private _ListboxTopColorSelected As Color
    Public ReadOnly Property ListboxTopColorSelected As Color
        Get
            Return _ListboxTopColorSelected
        End Get
    End Property

    Private _ListboxBottomColorSelected As Color
    Public ReadOnly Property ListboxBottomColorSelected As Color
        Get
            Return _ListboxBottomColorSelected
        End Get
    End Property

    Private _ListboxRahmenColorSelected As Color
    Public ReadOnly Property ListboxRahmenColorSelected As Color
        Get
            Return _ListboxRahmenColorSelected
        End Get
    End Property

    Private _IconListBoxSelectedForeColor As Color
    Public ReadOnly Property IconListBoxSelectedForeColor As Color
        Get
            Return _IconListBoxSelectedForeColor
        End Get
    End Property

    Private _JoSiTreeViewSelectionColorNotFocused As Color
    Public ReadOnly Property JoSiTreeViewSelectionColorNotFocused As Color
        Get
            Return _JoSiTreeViewSelectionColorNotFocused
        End Get
    End Property

    Private _JoSiTreeViewSelectionColorFocused As Color
    Public ReadOnly Property JoSiTreeViewSelectionColorFocused As Color
        Get
            Return _JoSiTreeViewSelectionColorFocused
        End Get
    End Property

    Private _foreColorenabled As Color
    Public ReadOnly Property ForecolorEnabled As Color
        Get
            Return _foreColorenabled
        End Get
    End Property
#End Region

#Region "Integer und Bool'sche Werte"

    Private _ToolStripUsedefaultSeperator As Boolean
    Public ReadOnly Property ToolStripUsedefaultSeperator As Boolean
        Get
            Return _ToolStripUsedefaultSeperator
        End Get
    End Property

    Private _ButtonBorderWidth As Integer
    Public ReadOnly Property ButtonBorderWidth As Integer
        Get
            Return _ButtonBorderWidth
        End Get
    End Property

    Private _EbenenListeItemColorDeltaSelection As Integer
    Public ReadOnly Property EbenenListeItemColorDeltaSelection As Integer
        Get
            Return _EbenenListeItemColorDeltaSelection
        End Get
    End Property

    Private _ComboboxHighlightingHot As Integer
    Public ReadOnly Property ComboboxHighlightingHot As Integer
        Get
            Return _ComboboxHighlightingHot
        End Get
    End Property

    Private _ComboboxHighlighting As Integer
    Public ReadOnly Property ComboboxHighlighting As Integer
        Get
            Return _ComboboxHighlighting
        End Get
    End Property

    Private _ComboboxBasicModeIsRed As Boolean
    Public ReadOnly Property ComboboxBasicModeIsRed As Boolean
        Get
            Return _ComboboxBasicModeIsRed
        End Get
    End Property

#End Region

#Region "Sonstiges"

    Private _TextBoxBorderStyle As BorderStyle
    Public ReadOnly Property TextBoxBorderStyle As BorderStyle
        Get
            Return _TextBoxBorderStyle
        End Get
    End Property

#End Region

    Private Shared aktuellerDesignStyleEbenenProjekt As DesignStyle = Nothing

    ''' <summary>
    ''' Gibt den aktuell typischen Design-Style zurück.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function getCurentDefaultStyle() As DesignStyle
        Return aktuellerDesignStyleEbenenProjekt
    End Function

    Public Shared Sub setDefaultStyle(d As DesignStyle)
        aktuellerDesignStyleEbenenProjekt = d
    End Sub

    Public Shared Function DefaultStyle() As DesignStyle
        Dim s As New DesignStyle
        s._SelectedColor = Color.FromArgb(255, 51, 153, 255)
        s._DefaultColor = Color.FromArgb(255, 128, 128, 150)
        s._scrollpictureboxColor = SystemColors.Control
        s._ButtonBackgroundcolorDefault = Color.LightGray
        s._ButtonBackgroundcolorSelected = Color.SkyBlue
        s._ButtonBorderColor = Color.Black
        s._ButtonBorderColorSelected = Color.Black
        s._ButtonBorderWidth = 1
        s._ButtonBackgroundcolorMouseOver = Color.Orange
        s._ButtonBackgroundcolorMouseDown = Color.Red
        s._FontColorDefault = Color.Black
        s._FontColorSelected = Color.Black
        s._Control = SystemColors.Control
        s._ListboxBackColor = Color.White
        s._MenuStripBackColor = SystemColors.Control
        s._ToolStrip1Backcolor = SystemColors.Control
        s._ToolStripHeaderBackcolor = SystemColors.ControlDark
        s._TabControlResizeColor = Color.Black
        s._LinealLinienColor = Color.Gray
        s._LinealMarkierung = Color.DarkGray
        s._LinealSchriftColor = Color.Gray
        s._LinealColor = SystemColors.Control
        s._ScrollPictureboxBorderColor = Color.Gray
        s._IconListboxSelectionColor = Color.FromArgb(255, 51, 152, 255)
        s._IconListboxSelectionFilling = Color.FromArgb(255, 100, 200, 255)
        s._IconListboxBorder = Color.FromArgb(255, 210, 210, 210)
        s._IconListboxGroupcolor1 = Color.FromArgb(255, 240, 240, 240)
        s._IconListboxGroupcolor2 = Color.FromArgb(255, 210, 210, 210)
        s._IconListboxFontColor = Color.Black
        s._IconListBoxSelectedForeColor = Color.Black
        s._EbenenListeItemColor = Color.LightGray
        s._EbenenListeItemColorDeltaSelection = 50
        s._SuchboxBackColor = Color.White
        s._TextForecolor = Color.Black
        s._SuchBoxSelected = Color.FromArgb(150, 150, 150)
        s._SuchBoxMouseOver = Color.FromArgb(200, 200, 200)
        s._ToolStripUsedefaultSeperator = True
        s._ToolStripSelectedColor = Color.FromArgb(179, 210, 255)
        s._ToolStripSelectedBorderColor = Color.FromArgb(51, 153, 255)
        s._ToolStripForecolor = Color.Black
        s._ToolStripSelectedForecolor = Color.Black
        s._ToolStripSeperatorColor2 = Color.Black
        s._ToolStripSeperatorColor1 = Color.Black
        s._ToolstripBorderColor = Color.Gray
        s._RückgängigSelectionColor = Color.FromArgb(180, 180, 180)
        s._RückgängigForeColor = Color.Black
        s._RückgängigBackColor = Color.White
        s._ToolstriptextboxBackColor = Color.White
        s._ToolstriptextboxForeColor = Color.Black
        s._TrackbarTextboxBackColor = SystemColors.Control
        s._ComboboxHighlighting = 0
        s._ComboboxHighlightingHot = 0
        s._ComboboxBackgroundColorHot = Color.Black
        s._ComboboxBackgroundColor = Color.Black
        s._ComboboxSelectionFilling = Color.FromArgb(51, 153, 255)
        s._ComboboxForeColor = Color.Black
        s._ComboboxBasicModeIsRed = False
        s._TextBoxBorderStyle = BorderStyle.Fixed3D
        s._ToolstripDropDownColor = Color.White
        s._ComboboxForeColorSelected = Color.Black
        s._comboboxDropDownBackColor = Color.White
        s._EbenenListeForecolor = Color.Black
        s._VerlaufsToolBorderColor = Color.FromArgb(100, 100, 100)
        s._GroupBoxColorAußen = Color.FromArgb(213, 223, 229)

        s._ListboxTopColor = Color.FromArgb(255, 220, 235, 252)
        s._ListboxBottomColor = Color.FromArgb(255, 193, 219, 252)
        s._ListboxRahmenColor = Color.FromArgb(255, 143, 185, 236)
        s._ListboxTopColorSelected = Color.FromArgb(255, 240, 247, 255)
        s._ListboxBottomColorSelected = Color.FromArgb(255, 228, 240, 254)
        s._ListboxRahmenColorSelected = Color.FromArgb(255, 205, 225, 248)
        s._JoSiTreeViewSelectionColorNotFocused = Color.FromArgb(255, 200, 200, 200)
        s._JoSiTreeViewSelectionColorFocused = s.SelectedColor

        s._foreColorenabled = Color.FromArgb(255, 128, 128, 128)

        s._SuchProgressbarBottomForeColor = Color.FromArgb(0, 180, 0)
        s._SuchProgressbarTopForeColor = Color.FromArgb(0, 200, 0)
        Return s
    End Function

    Public Shared Function LightStyle() As DesignStyle
        Dim s As DesignStyle = DesignStyle.DefaultStyle
        s._SelectedColor = Color.FromArgb(179, 210, 255)
        s._DefaultColor = Color.FromArgb(255, 200, 200, 200)
        s._scrollpictureboxColor = Color.White
        s._ButtonBackgroundcolorDefault = s._DefaultColor
        s._ButtonBackgroundcolorSelected = s._SelectedColor
        s._ButtonBorderColor = Color.Black
        s._ButtonBorderWidth = 0
        s._ButtonBackgroundcolorMouseOver = Color.FromArgb(51, 153, 255)
        s._ButtonBackgroundcolorMouseDown = Color.FromArgb(0, 100, 255)
        s._FontColorDefault = Color.Black
        s._FontColorSelected = Color.Black
        s._Control = Color.White
        s._ListboxBackColor = Color.White
        s._ToolStripHeaderBackcolor = Color.FromArgb(235, 235, 235)
        s._ToolStrip1Backcolor = Color.FromArgb(235, 235, 235)
        s._MenuStripBackColor = Color.FromArgb(235, 235, 235)
        s._TabControlResizeColor = Color.FromArgb(51, 153, 255)
        s._LinealLinienColor = Color.Gray
        s._LinealMarkierung = Color.FromArgb(51, 153, 255)
        s._LinealSchriftColor = Color.Gray
        s._LinealColor = Color.FromArgb(235, 235, 235)
        s._ScrollPictureboxBorderColor = Color.FromArgb(235, 235, 235)
        s._IconListboxSelectionColor = Color.FromArgb(255, 51, 152, 255)
        s._IconListboxSelectionFilling = s._SelectedColor
        s._IconListboxBorder = Color.FromArgb(255, 152, 152, 152)
        s._IconListboxGroupcolor1 = s._ToolStripHeaderBackcolor
        s._IconListboxGroupcolor2 = s._ToolStripHeaderBackcolor
        s._IconListboxFontColor = Color.Black
        s._EbenenListeItemColor = Color.FromArgb(200, 200, 200)
        s._EbenenListeItemColorDeltaSelection = 50
        s._SuchboxBackColor = Color.White
        s._TextForecolor = Color.Black
        s._SuchBoxSelected = Color.FromArgb(150, 150, 150)
        s._SuchBoxMouseOver = Color.FromArgb(200, 200, 200)
        s._ComboboxSelectionFilling = s._SelectedColor
        s._ToolStripSelectedColor = s.SelectedColor
        s._ToolStripSelectedBorderColor = Color.FromArgb(53, 153, 255)
        s._JoSiTreeViewSelectionColorFocused = s._SelectedColor
        Return s
    End Function

    Public Shared Function OrangeStyle() As DesignStyle
        Dim s As DesignStyle = DesignStyle.DefaultStyle
        s._SelectedColor = Color.FromArgb(255, 153, 51)
        s._ButtonBackgroundcolorSelected = Color.FromArgb(235, 206, 135)
        s._ButtonBackgroundcolorMouseOver = Color.OrangeRed
        s._ButtonBackgroundcolorMouseDown = Color.Red
        s._IconListboxSelectionColor = Color.FromArgb(255, 255, 152, 51)
        s._IconListboxSelectionFilling = Color.FromArgb(255, 255, 200, 100)
        s._ToolStripSelectedColor = Color.FromArgb(255, 210, 179)
        s._ToolStripSelectedBorderColor = Color.FromArgb(255, 153, 51)
        s._ComboboxSelectionFilling = Color.FromArgb(255, 153, 51)
        s._ComboboxBackgroundColorHot = Color.FromArgb(2, 1, 0)
        s._ComboboxBasicModeIsRed = True
        s._ListboxTopColor = Color.FromArgb(255, 252, 235, 220)
        s._ListboxBottomColor = Color.FromArgb(255, 252, 219, 193)
        s._ListboxRahmenColor = Color.FromArgb(255, 236, 185, 143)
        s._ListboxTopColorSelected = Color.FromArgb(255, 255, 247, 240)
        s._ListboxBottomColorSelected = Color.FromArgb(255, 254, 240, 228)
        s._ListboxRahmenColorSelected = Color.FromArgb(255, 248, 225, 205)
        s._JoSiTreeViewSelectionColorFocused = s._SelectedColor
        Return s
    End Function

    Public Shared Function DarkStyle() As DesignStyle
        Dim s As New DesignStyle()
        s._SelectedColor = Color.FromArgb(51, 153, 255)
        s._DefaultColor = Color.FromArgb(255, 100, 100, 100)
        s._scrollpictureboxColor = Color.FromArgb(50, 50, 50)
        s._ButtonBackgroundcolorDefault = s._DefaultColor
        s._ButtonBackgroundcolorSelected = s._SelectedColor
        s._ButtonBorderColor = Color.FromArgb(150, 150, 150)
        s._ButtonBorderColorSelected = Color.FromArgb(132, 192, 255)
        s._ButtonBorderWidth = 1
        s._ButtonBackgroundcolorMouseOver = Color.FromArgb(179, 210, 255)
        s._ButtonBackgroundcolorMouseDown = Color.FromArgb(0, 100, 255)
        s._FontColorDefault = Color.White
        s._FontColorSelected = Color.FromArgb(50, 50, 50)
        s._Control = Color.FromArgb(80, 80, 80)
        s._ListboxBackColor = Color.FromArgb(80, 80, 80)
        s._ToolStripHeaderBackcolor = Color.FromArgb(50, 50, 50)
        s._ToolStrip1Backcolor = Color.FromArgb(80, 80, 80)
        s._MenuStripBackColor = Color.FromArgb(80, 80, 80)
        s._TabControlResizeColor = Color.FromArgb(51, 153, 255)
        s._LinealLinienColor = Color.FromArgb(200, 200, 200)
        s._LinealMarkierung = Color.FromArgb(179, 210, 255)
        s._LinealSchriftColor = Color.FromArgb(200, 200, 200)
        s._LinealColor = Color.FromArgb(50, 50, 50)
        s._ScrollPictureboxBorderColor = Color.FromArgb(70, 70, 70)
        s._IconListboxSelectionColor = Color.FromArgb(255, 51, 152, 255)
        s._IconListboxSelectionFilling = Color.FromArgb(179, 210, 255)
        s._IconListboxBorder = Color.FromArgb(50, 50, 50)
        s._IconListboxGroupcolor1 = s._ToolStripHeaderBackcolor
        s._IconListboxGroupcolor2 = s._ToolStripHeaderBackcolor
        s._IconListboxFontColor = Color.FromArgb(200, 200, 200)
        s._IconListBoxSelectedForeColor = Color.Black
        s._EbenenListeItemColor = Color.FromArgb(100, 100, 100)
        s._EbenenListeItemColorDeltaSelection = 50
        s._SuchboxBackColor = Color.FromArgb(50, 50, 50)
        s._TextForecolor = Color.White
        s._SuchBoxSelected = Color.FromArgb(51, 153, 255)
        s._SuchBoxMouseOver = Color.FromArgb(127, 127, 127)
        s._ToolStripUsedefaultSeperator = False
        s._ToolStripSelectedColor = Color.FromArgb(179, 210, 255)
        s._ToolStripSelectedBorderColor = Color.FromArgb(51, 153, 255)
        s._ToolStripForecolor = Color.White
        s._ToolStripSelectedForecolor = Color.Black
        s._ToolStripSeperatorColor2 = Color.FromArgb(127, 127, 127)
        s._ToolStripSeperatorColor1 = Color.Black
        s._ToolstripBorderColor = Color.Black
        s._RückgängigSelectionColor = Color.FromArgb(51, 153, 255)
        s._RückgängigForeColor = Color.White
        s._RückgängigBackColor = s._ToolStrip1Backcolor
        s._ToolstriptextboxBackColor = Color.FromArgb(80, 80, 80)
        s._ToolstriptextboxForeColor = Color.White
        s._TrackbarTextboxBackColor = Color.FromArgb(50, 50, 50)
        s._ComboboxHighlighting = 60
        s._ComboboxHighlightingHot = 25
        s._ComboboxBackgroundColorHot = Color.FromArgb(0, 75, 150)
        s._ComboboxBackgroundColor = Color.Black
        s._ComboboxSelectionFilling = Color.FromArgb(51, 153, 255)
        s._ComboboxForeColor = Color.White
        s._ComboboxBasicModeIsRed = False
        s._TextBoxBorderStyle = BorderStyle.FixedSingle
        s._ToolstripDropDownColor = s._ToolStripHeaderBackcolor
        s._ComboboxForeColorSelected = Color.Black
        s._comboboxDropDownBackColor = s._Control
        s._EbenenListeForecolor = Color.White
        s._VerlaufsToolBorderColor = Color.FromArgb(100, 100, 100)
        s._GroupBoxColorAußen = Color.FromArgb(255, 40, 40, 40)
        s._GroupBoxColorInnen = Color.FromArgb(255, 90, 90, 90)
        s._ListboxTopColor = Color.FromArgb(255, 105, 113, 126)
        s._ListboxBottomColor = Color.FromArgb(255, 92, 105, 126)
        s._ListboxRahmenColor = Color.FromArgb(255, 67, 88, 118)
        s._ListboxTopColorSelected = Color.FromArgb(255, 115, 119, 128)
        s._ListboxBottomColorSelected = Color.FromArgb(255, 109, 115, 127)
        s._ListboxRahmenColorSelected = Color.FromArgb(255, 95, 107, 124)
        s._JoSiTreeViewSelectionColorNotFocused = Color.FromArgb(255, 100, 100, 100)
        s._JoSiTreeViewSelectionColorFocused = s._SelectedColor
        s._foreColorenabled = Color.FromArgb(255, 128, 128, 128)
        s._SuchProgressbarBottomForeColor = Color.FromArgb(0, 130, 0)
        s._SuchProgressbarTopForeColor = Color.FromArgb(0, 150, 0)
        Return s
    End Function

    Public Sub Ändern(neuerStyle As DesignStyle)
        With neuerStyle
            Me._SelectedColor = ._SelectedColor
            Me._DefaultColor = ._DefaultColor
            Me._scrollpictureboxColor = ._scrollpictureboxColor
            Me._ButtonBackgroundcolorDefault = ._ButtonBackgroundcolorDefault
            Me._ButtonBackgroundcolorSelected = ._ButtonBackgroundcolorSelected
            Me._ButtonBackgroundcolorMouseDown = ._ButtonBackgroundcolorMouseDown
            Me._ButtonBackgroundcolorMouseOver = ._ButtonBackgroundcolorMouseOver
            Me._ButtonBorderColor = ._ButtonBorderColor
            Me._ButtonBorderColorSelected = ._ButtonBorderColorSelected
            Me._ButtonBorderWidth = ._ButtonBorderWidth
            Me._FontColorSelected = ._FontColorSelected
            Me._FontColorDefault = ._FontColorDefault
            Me._Control = ._Control
            Me._ListboxBackColor = ._ListboxBackColor
            Me._ToolStrip1Backcolor = ._ToolStrip1Backcolor
            Me._ToolStripHeaderBackcolor = ._ToolStripHeaderBackcolor
            Me._MenuStripBackColor = ._MenuStripBackColor
            Me._TabControlResizeColor = ._TabControlResizeColor
            Me._LinealSchriftColor = ._LinealSchriftColor
            Me._LinealMarkierung = ._LinealMarkierung
            Me._LinealLinienColor = ._LinealLinienColor
            Me._LinealColor = ._LinealColor
            Me._ScrollPictureboxBorderColor = ._ScrollPictureboxBorderColor
            Me._IconListboxSelectionColor = ._IconListboxSelectionColor
            Me._IconListboxSelectionFilling = ._IconListboxSelectionFilling
            Me._IconListboxBorder = ._IconListboxBorder
            Me._IconListboxGroupcolor1 = ._IconListboxGroupcolor1
            Me._IconListboxGroupcolor2 = ._IconListboxGroupcolor2
            Me._EbenenListeItemColor = ._EbenenListeItemColor
            Me._EbenenListeItemColorDeltaSelection = ._EbenenListeItemColorDeltaSelection
            Me._IconListboxFontColor = ._IconListboxFontColor
            Me._SuchboxBackColor = ._SuchboxBackColor
            Me._TextForecolor = ._TextForecolor
            Me._SuchBoxSelected = ._SuchBoxSelected
            Me._SuchBoxMouseOver = ._SuchBoxMouseOver
            Me._ToolStripUsedefaultSeperator = ._ToolStripUsedefaultSeperator
            Me._ToolStripSelectedColor = ._ToolStripSelectedColor
            Me._ToolStripSelectedBorderColor = ._ToolStripSelectedBorderColor
            Me._ToolStripForecolor = ._ToolStripForecolor
            Me._ToolStripSeperatorColor2 = ._ToolStripSeperatorColor2
            Me._ToolStripSeperatorColor1 = ._ToolStripSeperatorColor1
            Me._ToolStripSelectedForecolor = ._ToolStripSelectedForecolor
            Me._ToolstripBorderColor = ._ToolstripBorderColor
            Me._RückgängigSelectionColor = ._RückgängigSelectionColor
            Me._RückgängigForeColor = ._RückgängigForeColor
            Me._RückgängigBackColor = ._RückgängigBackColor
            Me._ToolstriptextboxBackColor = ._ToolstriptextboxBackColor
            Me._ToolstriptextboxForeColor = ._ToolstriptextboxForeColor
            Me._TrackbarTextboxBackColor = ._TrackbarTextboxBackColor
            Me._ComboboxBackgroundColor = ._ComboboxBackgroundColor
            Me._ComboboxHighlighting = ._ComboboxHighlighting
            Me._ComboboxBackgroundColorHot = ._ComboboxBackgroundColorHot
            Me._ComboboxHighlightingHot = ._ComboboxHighlightingHot
            Me._ComboboxSelectionFilling = ._ComboboxSelectionFilling
            Me._ComboboxForeColor = ._ComboboxForeColor
            Me._ComboboxBasicModeIsRed = ._ComboboxBasicModeIsRed
            Me._TextBoxBorderStyle = ._TextBoxBorderStyle
            Me._ToolstripDropDownColor = ._ToolstripDropDownColor
            Me._comboboxDropDownBackColor = ._comboboxDropDownBackColor
            Me._ComboboxForeColorSelected = ._ComboboxForeColorSelected
            Me._EbenenListeForecolor = ._EbenenListeForecolor
            Me._VerlaufsToolBorderColor = ._VerlaufsToolBorderColor
            Me._GroupBoxColorAußen = ._GroupBoxColorAußen
            Me._GroupBoxColorInnen = ._GroupBoxColorInnen
            Me._ListboxBottomColor = ._ListboxBottomColor
            Me._ListboxBottomColorSelected = ._ListboxBottomColorSelected
            Me._ListboxRahmenColor = ._ListboxRahmenColor
            Me._ListboxRahmenColorSelected = ._ListboxRahmenColorSelected
            Me._ListboxTopColor = ._ListboxTopColor
            Me._ListboxTopColorSelected = ._ListboxTopColorSelected
            Me._IconListBoxSelectedForeColor = ._IconListBoxSelectedForeColor
            Me._JoSiTreeViewSelectionColorNotFocused = ._JoSiTreeViewSelectionColorNotFocused
            Me._JoSiTreeViewSelectionColorFocused = ._JoSiTreeViewSelectionColorFocused
            Me._foreColorenabled = ._foreColorenabled
            Me._SuchProgressbarBottomForeColor = ._SuchProgressbarBottomForeColor
            Me._SuchProgressbarTopForeColor = ._SuchProgressbarTopForeColor
        End With
        RaiseEvent StyleChanged(Me, EventArgs.Empty)
    End Sub

    Public Sub setcolorsForNumericUpDown(num As NumericUpDown)
        num.BackColor = Me.SuchboxBackColor
        num.ForeColor = Me.TextForeColor
    End Sub

    Public Sub WendeDesignStyleAufControlAn(c As Control)
        If TypeOf c Is NumericUpDown Then
            Me.setcolorsForNumericUpDown(DirectCast(c, NumericUpDown))
        ElseIf TypeOf c Is Label OrElse TypeOf c Is RadioButton OrElse TypeOf c Is CheckBox Then
            c.ForeColor = Me.TextForeColor
        ElseIf TypeOf c Is TextBox Then
            c.BackColor = Me.SuchboxBackColor
            c.ForeColor = Me.TextForeColor
            DirectCast(c, TextBox).BorderStyle = Me.TextBoxBorderStyle
        End If
        If c.Controls IsNot Nothing AndAlso c.Controls.Count > 0 Then
            For Each c_Sub As Control In c.Controls
                WendeDesignStyleAufControlAn(c_Sub)
            Next
        End If
    End Sub

    Public Event StyleChanged(sender As Object, e As EventArgs)
End Class
