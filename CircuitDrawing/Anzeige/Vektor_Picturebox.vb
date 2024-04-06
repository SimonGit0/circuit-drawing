Imports System.IO
Public Class Vektor_Picturebox
    Inherits Control

    Private Const MIN_FAKTOR As Single = 0.001
    Private Const MAX_FAKTOR As Single = 10

    Public Const _SPEICHERN_StartElement As Integer = 1477253037
    Public Const _SPEICHERN_StartElementDrawing As Integer = 340612946
    Public Const _SPEICHERN_StartElementSnapping As Integer = 347638238

    Public Const DEFAULT_MM_PER_INT As Single = 7.5F / 600.0F 'Anzahl mm / 600 ints (Breite eines Res)
    Private Const DEFAULT_LINE_WIDTH As Single = 0.2 'Linienbreite in mm

    Private Const KEY_SCROLL_OFFSET As Single = 10
    Private Const CURSORBREITE As Single = 8.0F
    Private Const CURSORHÖHE As Single = 8.0F
    Private Const CROSSCURSOR_EXTENTION As Single = 2.0F
    Public Const RADIUS_DOT As Integer = 40

    Private Const ROUTING_AUSWEICHABSTAND As Integer = 500

    Private _faktor As Single = 0.1
    Private offsetX As Single = 0
    Private offsetY As Single = 0

    Public GridX As Integer = 50
    Public GridY As Integer = 50

    Private myElemente As List(Of ElementMaster)

    Private MAX_ID As ULong

    Private myTools As Stack(Of Tool)
    Private _lockSelection As Boolean = False

    Private _currentPlaceBauteil As BauteilCell
    Private currentPlaceTemplate As TemplateAusDatei

    Private myLineStylesSelectionDrawing As LineStyleList

    Public myLineStyles As LineStyleList
    Public myFillStyles As FillStyleList
    Public myFonts As FontList

    Private myNumberFormatInfoTEX As Globalization.NumberFormatInfo

    Private myLastDrawingBoundingBox As Rectangle

    Private _GRID_IMAGE_LARGE As Bitmap
    Private _GRID_IMAGE_SMALL As Bitmap
    Private _GRID_BYTES_LARGE() As Byte
    Private _GRID_BYTES_SMALL() As Byte

    Public Select_Bauteile As Boolean = True
    Public Select_Wires As Boolean = True
    Public Select_Drawings As Boolean = True
    Public Select_Beschriftung As Boolean = True

    Public noSelectionChangedEvents_ForSelectionBeforeDelete As Boolean = False

    Public showSnappoints As Boolean = False

    Public myMoveRichtung As MoveRichtung

    Public drawDotsOnIntersectingWires As Boolean = True
    Public Const DEFAULT_RADIUS_ZEILENSPRÜNGE As Integer = 60
    Public radius_Zeilensprünge As Integer = DEFAULT_RADIUS_ZEILENSPRÜNGE
    Public drawZeilensprünge As Boolean = False

    Private _multiwireSelect As Boolean = False
    Public Property MultiWireSelect As Boolean
        Get
            Return _multiwireSelect
        End Get
        Set(value As Boolean)
            If _multiwireSelect <> value Then
                _multiwireSelect = value
                OnMultiSelectChanged()
            End If
        End Set
    End Property

    Private _enable_gravity As Boolean
    Public Property enable_gravity As Boolean
        Get
            Return _enable_gravity
        End Get
        Set(value As Boolean)
            If value <> _enable_gravity Then
                _enable_gravity = value
                OnGravityChanged()
            End If
        End Set
    End Property

    Private _textVorschauMode As Boolean = False
    Public Property TextVorschauMode As Boolean
        Get
            Return _textVorschauMode
        End Get
        Set(value As Boolean)
            If value <> _textVorschauMode Then
                _textVorschauMode = value
                Me.Invalidate()
            End If
        End Set
    End Property

    Public gravityStärke As Integer = 20

    Public crossCursor As Boolean = False
    Public snappoinsImmerAnzeigen As Boolean = False
    Public showBoarder As Boolean = False

    Public Sub New()
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.UserPaint, True)
        Me.SetStyle(ControlStyles.ResizeRedraw, True)
        Me.SetStyle(ControlStyles.Selectable, False)

        Me.SetStyle(ControlStyles.ContainerControl, True)

        myTools = New Stack(Of Tool)
        myElemente = New List(Of ElementMaster)

        _cursorInside = False

        myLineStyles = New LineStyleList()
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, DEFAULT_LINE_WIDTH, New DashStyle(0))) 'für Elemente
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, DEFAULT_LINE_WIDTH, New DashStyle(0))) 'für Wires
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, DEFAULT_LINE_WIDTH * 0.5F, New DashStyle(7))) 'für Grafik-Elemente
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, DEFAULT_LINE_WIDTH, New DashStyle(0))) 'für Snaping-Elemente

        myLineStylesSelectionDrawing = New LineStyleList()
        myLineStylesSelectionDrawing.add(New LineStyle(New Farbe(128, 135, 206, 235), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 3 * DEFAULT_LINE_WIDTH, New DashStyle(0)))
        'Style für HighlightSelection!
        myLineStylesSelectionDrawing.add(New LineStyle(New Farbe(255, 255, 128, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 3 * DEFAULT_LINE_WIDTH, New DashStyle(1, 100)))

        myFillStyles = New FillStyleList()
        myFillStyles.add(New FillStyle(New Farbe(0, 255, 255, 255))) 'Default Linestyle (wird nicht geändert! Wird immer für selection + Bauelemente + Neue Drawings verwendet!

        myFonts = New FontList()

        myFonts.add(New FontStyle(New Farbe(255, 0, 0, 0), FontCombobox.getDefaultFont(), 10.0, False, False))

        MAX_ID = 0

        myNumberFormatInfoTEX = New Globalization.NumberFormatInfo()
        myNumberFormatInfoTEX.NumberDecimalSeparator = "."
        Me.myMoveRichtung = MoveRichtung.NurRechtwinklig
        Me.MarginExport = New Padding(50)
    End Sub

    Private Property faktor As Single
        Get
            Return _faktor
        End Get
        Set(value As Single)
            If value < MIN_FAKTOR Then
                value = MIN_FAKTOR
            ElseIf value > MAX_FAKTOR Then
                value = MAX_FAKTOR
            End If
            _faktor = value
        End Set
    End Property

    Public Property MarginExport As Padding

    Public MM_PER_INT As Single = DEFAULT_MM_PER_INT

#Region "Tool Starten & Enden"
    Public Sub startTool(t As Tool)
        If myTools.Count > 0 Then
            myTools.Peek().pause(Me)
        End If
        myTools.Push(t)
        t.meldeAn(Me)
        OnToolChanged()
    End Sub

    Public Function getCurrentTool() As Tool
        If myTools.Count = 0 Then Return Nothing
        Return myTools.Peek()
    End Function

    Public Sub startToolMove()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolMove) Then
            Dim t As New ToolMove()
            startTool(t)
        End If
    End Sub

    Public Sub startToolCopy()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolCopy) Then
            Dim t As New ToolCopy()
            startTool(t)
        End If
    End Sub

    Public Sub startToolWire()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolDrawWire) Then
            Dim t As New ToolDrawWire()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddCurrentArrow()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddCurrentArrow) Then
            Dim t As New ToolAddCurrentArrow()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddVoltageArrow()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddVoltageArrow) Then
            Dim t As New ToolAddVoltageArrow()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddLabel()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddLabel) Then
            Dim t As New ToolAddLabel()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddBusTap()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddBusTap) Then
            Dim t As New ToolAddBusTap()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddImpedanceArrow()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddSnapableImpedanceArrow) Then
            Dim t As New ToolAddSnapableImpedanceArrow()
            startTool(t)
        End If
    End Sub

    Public Sub startToolPlace()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolPlace) Then
            Dim t As New ToolPlace()
            startTool(t)
        End If
    End Sub

    Public Sub startToolScale()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolScale) Then
            Dim t As New ToolScale()
            startTool(t)
        End If
    End Sub

    Public Sub deleteOrStartToolDelete()
        If Me.has_selection() Then
            Me.delete_selected(False)
        Else
            Me.startDeleteTool()
        End If
    End Sub

    Public Sub startDeleteTool()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolEntfernen) Then
            Dim t As New ToolEntfernen()
            startTool(t)
        End If
    End Sub

    Public Sub startToolDrawLine()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolDrawLine) Then
            Dim t As New ToolDrawLine()
            startTool(t)
        End If
    End Sub

    Public Sub startToolDrawBezier()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolDrawBezier) Then
            Dim t As New ToolDrawBezier()
            startTool(t)
        End If
    End Sub

    Public Sub startToolDrawBezierFreihand()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolDrawBezierFreihand) Then
            Dim t As New ToolDrawBezierFreihand()
            startTool(t)
        End If
    End Sub

    Public Sub startToolAddSnappoint()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolAddSnappoint) Then
            Dim t As New ToolAddSnappoint()
            startTool(t)
        End If
    End Sub

    Public Sub startToolDeleteSnappoint()
        If (myTools.Count = 0) OrElse (TypeOf myTools.Peek() IsNot ToolDeleteSnappoint) Then
            Dim t As New ToolDeleteSnappoint()
            startTool(t)
        End If
    End Sub

    Public Sub cancelCurrentTool()
        Me.cancelCurrentTool(False)
    End Sub

    ''' <summary>
    ''' Achtung! Spezielle Überladung, bei der die Funktion 'weiter' des aktuell neuen Tools unterdrückt werden kann! Nur für spezielle Fälle.
    ''' </summary>
    ''' <param name="ohneEventsUndAllem"></param>
    Public Sub cancelCurrentTool(ohneEventsUndAllem As Boolean)
        If myTools.Count > 0 AndAlso myTools.Peek().KannAbbrechen Then
            myTools.Pop().meldeAb(Me)
            If Not ohneEventsUndAllem Then
                If myTools.Count > 0 Then
                    myTools.Peek().weiter(Me)
                Else
                    RaiseEvent ToolInfoTextChanged(Me, New ToolInfoTextEventArgs("Bereit"))
                End If
                OnToolChanged()
            End If
        End If
    End Sub

    Private Sub OnToolChanged()
        Me.Invalidate()
    End Sub
#End Region

#Region "Elemente"
    Public Sub addElement(e As Element)
        myElemente.Add(e)
        simplifyWires()
    End Sub

    Public Sub addElement_OHNE_SIMPLIFY_WIRES(e As Element)
        myElemente.Add(e)
    End Sub

    Public Sub addElement(e As SnapableElement)
        myElemente.Add(e)
    End Sub

    Public Function ElementListe() As List(Of ElementMaster)
        Return myElemente
    End Function

    Public Function getEinstellungenOfSelectedElements() As List(Of ElementEinstellung)
        Dim liste As List(Of ElementEinstellung) = Nothing
        Dim listeNeu As List(Of ElementEinstellung) = Nothing
        Dim hatEinstellungInBeiden As Boolean

        For Each e As ElementMaster In myElemente
            If e.hasSelection() Then
                If liste Is Nothing Then
                    liste = e.getEinstellungen(Me)
                Else
                    listeNeu = e.getEinstellungen(Me)
                    For i As Integer = liste.Count - 1 To 0 Step -1
                        hatEinstellungInBeiden = False
                        For j As Integer = 0 To listeNeu.Count - 1
                            If liste(i).Name = listeNeu(j).Name AndAlso liste(i).GetType().ToString() = listeNeu(j).GetType().ToString() Then
                                hatEinstellungInBeiden = True
                                liste(i).CombineValues(listeNeu(j))
                                Exit For
                            End If
                        Next
                        If Not hatEinstellungInBeiden Then
                            liste.RemoveAt(i)
                        ElseIf TypeOf liste(i) Is Einstellung_Multi AndAlso DirectCast(liste(i), Einstellung_Multi).getListe().Count = 0 Then
                            liste.RemoveAt(i) 'Leere Liste löschen!
                        End If
                    Next
                End If
            End If
        Next
        Return liste
    End Function

    Public Sub setEinstellungenSelected(einstellungen As List(Of ElementEinstellung))
        Dim rück As New RückgängigGrafik()
        rück.speicherVorherZustand(Me.getRückArgs())
        Dim rück2 As New RückgängigLineStyle()
        rück2.speicherVorher(Me.getRückArgs())
        Dim rück3 As New RückgängigFillStyles()
        rück3.speicherVorher(Me.getRückArgs())
        Dim rück4 As New RückgängigFontStyle()
        rück4.speicherVorher(Me.getRückArgs())

        Dim changed As Boolean = False

        For Each e As ElementMaster In myElemente
            If e.hasSelection Then
                If e.setEinstellungen(Me, einstellungen) Then
                    changed = True
                End If
            End If
        Next

        If changed Then

            removeNichtBenötigteStyles()

            rück.speicherNachherZustand(Me.getRückArgs())
            rück2.speicherNachher(Me.getRückArgs())
            rück3.speicherNachher(Me.getRückArgs())
            rück4.speicherNachher(Me.getRückArgs())

            Dim rückGesamt As New RückgängigMulti()
            rückGesamt.setText("Einstellungen ändern")
            If rück.istNotwendig() Then rückGesamt.Rück.Add(rück)
            If rück2.RückBenötigt() Then rückGesamt.Rück.Add(rück2)
            If rück3.RückBenötigt() Then rückGesamt.Rück.Add(rück3)
            If rück4.RückBenötigt() Then rückGesamt.Rück.Add(rück4)
            addNeuesRückgängig(New NeuesRückgängigEventArgs(rückGesamt))
            Me.Invalidate()
        Else
            rück = Nothing
        End If
    End Sub

    Public Sub removeNichtBenötigteStyles()
        removeNichtBenötigteLineStyles()
        removeNichtBenötigteFillStyles()
        removeNichtBenötigteFontstyles()
    End Sub

    Private Sub removeNichtBenötigteLineStyles()
        Dim usedLinestyles(myLineStyles.Count() - 1) As Boolean
        usedLinestyles(0) = True
        usedLinestyles(1) = True
        usedLinestyles(2) = True
        usedLinestyles(3) = True

        For Each e As ElementMaster In myElemente
            e.getGrafik().markAllUsedLinestyles(usedLinestyles)
        Next
        'Nicht verwendete Linestyles wieder löschen
        For i As Integer = usedLinestyles.Count - 1 To 0 Step -1
            If usedLinestyles(i) = False Then
                myLineStyles.removeAt(i)
            End If
        Next
    End Sub

    Private Sub removeNichtBenötigteFillStyles()
        Dim usedFillStyles(myFillStyles.Count() - 1) As Boolean
        usedFillStyles(0) = True
        For Each e As ElementMaster In myElemente
            e.getGrafik().markAllUsedFillstyles(usedFillStyles)
            If TypeOf e Is Element Then
                DirectCast(e, Element).markAllUsedFillstyles(usedFillStyles)
            End If
        Next
        'Nicht verwendete Fillstyles wieder löschen
        For i As Integer = usedFillStyles.Count - 1 To 0 Step -1
            If usedFillStyles(i) = False Then
                myFillStyles.removeAt(i)
            End If
        Next
    End Sub

    Private Sub removeNichtBenötigteFontstyles()
        Dim usedFonStyles(myFonts.Count() - 1) As Boolean
        usedFonStyles(0) = True
        For Each e As ElementMaster In myElemente
            e.getGrafik().markAllUsedFontStyles(usedFonStyles)
        Next
        'Nicht verwendete Fillstyles wieder löschen
        For i As Integer = usedFonStyles.Count - 1 To 0 Step -1
            If usedFonStyles(i) = False Then
                myFonts.removeAt(i)
            End If
        Next
    End Sub

    Public Function getNewID() As ULong
        Dim id As ULong = MAX_ID
        MAX_ID += 1UL
        Return id
    End Function
#End Region

#Region "Allgemeine Funktionen"
#Region "Selection"
    Public Sub deselect_All()
        If Not _lockSelection Then
            Dim invalidate As Boolean = False
            For Each element In myElemente
                If element.hasSelection Then
                    element.deselect()
                    invalidate = True
                End If
            Next
            If invalidate Then
                OnSelectionChanged()
                Me.Invalidate()
            End If
        End If
    End Sub

    Public Sub selectAll()
        If Not _lockSelection Then
            Dim invalidate As Boolean = False
            For Each element In myElemente
                If TypeOf element Is Element Then
                    If Not DirectCast(element, Element).isSelected AndAlso darfSelektieren(element) Then
                        DirectCast(element, Element).isSelected = True
                        invalidate = True
                    End If
                ElseIf TypeOf element Is SnapableElement Then
                    If darfSelektieren(element) Then
                        For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                            If Not DirectCast(element, SnapableElement).getSnappoint(i).isSelected Then
                                DirectCast(element, SnapableElement).getSnappoint(i).isSelected = True
                                invalidate = True
                            End If
                        Next
                    End If
                End If
            Next
            If invalidate Then
                OnSelectionChanged()
                Me.Invalidate()
            End If
        End If
    End Sub

    Public Sub select_All_in_Rect(r As Rectangle, mode As SelectionMode)
        If Not _lockSelection Then
            Dim changed As Boolean = False
            Dim new_selected As Boolean

            For Each element In myElemente
                If TypeOf element Is Element Then
                    If isElementInRect(DirectCast(element, Element), r) Then
                        If mode = SelectionMode.SubtractFromSelection Then
                            new_selected = False
                        Else
                            new_selected = True
                        End If
                    Else
                        If mode = SelectionMode.SelectOnlyNewElements Then
                            new_selected = False
                        Else
                            new_selected = element.hasSelection()
                        End If
                    End If

                    If new_selected <> element.hasSelection() Then
                        changed = True
                        DirectCast(element, Element).isSelected = new_selected
                    End If
                ElseIf TypeOf element Is SnapableElement Then
                    For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                        If Me.isElementInRect(DirectCast(element, SnapableElement), i, r) Then
                            If mode = SelectionMode.SubtractFromSelection Then
                                new_selected = False
                            Else
                                new_selected = True
                            End If
                        Else
                            If mode = SelectionMode.SelectOnlyNewElements Then
                                new_selected = False
                            Else
                                new_selected = DirectCast(element, SnapableElement).getSnappoint(i).isSelected
                            End If
                        End If
                        If new_selected <> DirectCast(element, SnapableElement).getSnappoint(i).isSelected Then
                            changed = True
                            DirectCast(element, SnapableElement).getSnappoint(i).isSelected = new_selected
                        End If
                    Next
                End If
            Next

            If changed Then
                OnSelectionChanged()
                Me.Invalidate()
            End If
        End If

    End Sub

    Public Sub select_Element_At(p As Point, mode As SelectionMode)
        If _lockSelection Then
            Return 'Selection nicht ändern!
        End If

        Dim sel As List(Of Object) = getElementAt_forSelect(p)

        Dim changed As Boolean = False
        If mode = SelectionMode.SelectOnlyNewElements Then
            If sel Is Nothing Then
                Me.deselect_All()
                Exit Sub
            End If
            For Each element As ElementMaster In myElemente
                If TypeOf element Is Element Then
                    If sel.Contains(element) Then
                        If element.hasSelection() = False Then
                            changed = True
                        End If
                        DirectCast(element, Element).isSelected = True
                    Else
                        If element.hasSelection() = True Then
                            changed = True
                        End If
                        DirectCast(element, Element).isSelected = False
                    End If
                ElseIf TypeOf element Is SnapableElement Then
                    For k As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                        If sel.Contains(DirectCast(element, SnapableElement).getSnappoint(k)) Then
                            If DirectCast(element, SnapableElement).getSnappoint(k).isSelected = False Then
                                changed = True
                            End If
                            DirectCast(element, SnapableElement).getSnappoint(k).isSelected = True
                        Else
                            If DirectCast(element, SnapableElement).getSnappoint(k).isSelected = True Then
                                changed = True
                            End If
                            DirectCast(element, SnapableElement).getSnappoint(k).isSelected = False
                        End If
                    Next
                End If
            Next
        ElseIf mode = SelectionMode.AddSelection Then
            If sel IsNot Nothing Then
                For Each o As Object In sel
                    If TypeOf o Is Element Then
                        With DirectCast(o, Element)
                            If .isSelected = False Then changed = True
                            .isSelected = True
                        End With
                    ElseIf TypeOf o Is WireSnappoint Then
                        With DirectCast(o, WireSnappoint)
                            If .isSelected = False Then changed = True
                            .isSelected = True
                        End With
                    End If
                Next
            End If
        ElseIf mode = SelectionMode.SubtractFromSelection Then
            If sel IsNot Nothing Then
                For Each o As Object In sel
                    If TypeOf o Is Element Then
                        With DirectCast(o, Element)
                            If .isSelected Then changed = True
                            .isSelected = False
                        End With
                    ElseIf TypeOf o Is WireSnappoint Then
                        With DirectCast(o, WireSnappoint)
                            If .isSelected Then changed = True
                            .isSelected = False
                        End With
                    End If
                Next
            End If
        End If

        If changed Then
            OnSelectionChanged()
            Me.Invalidate()
        End If
    End Sub

    Public Function getElementAt_forSelect(p As Point) As List(Of Object)
        If _lockSelection Then
            Return Nothing
        End If
        Dim abstand As Double
        Dim minAbstand As Double = Double.MaxValue
        Dim minElement As Element = Nothing
        Dim minSnapingPos As WireSnappoint = Nothing

        For Each element As ElementMaster In myElemente
            If darfSelektieren(element) Then
                If TypeOf element Is Element Then
                    abstand = DirectCast(element, Element).getSelection().distanceToBounds(p, faktor, GridX, GridY)
                    If abstand < minAbstand Then
                        minAbstand = abstand
                        minElement = DirectCast(element, Element)
                    End If
                ElseIf TypeOf element Is SnapableElement Then
                    For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                        abstand = DirectCast(element, SnapableElement).getSnappoint(i).getSelection().distanceToBounds(p, faktor, GridX, GridY)
                        If abstand < minAbstand Then
                            minAbstand = abstand
                            minSnapingPos = DirectCast(element, SnapableElement).getSnappoint(i)
                        End If
                    Next
                End If
            End If
        Next

        If minSnapingPos IsNot Nothing Then
            Dim erg As New List(Of Object)
            erg.Add(minSnapingPos)
            Return erg
        Else
            Dim erg As New List(Of Object)
            erg.Add(minElement)

            If Me.MultiWireSelect Then
                If TypeOf minElement Is Wire OrElse TypeOf minElement Is WireLuftlinie Then
                    Dim wiresList(myElemente.Count - 1) As List(Of Integer)
                    For i As Integer = 0 To wiresList.Length - 1
                        If myElemente(i).Equals(minElement) Then
                            wiresList(i) = New List(Of Integer)(1)
                            wiresList(i).Add(0)
                        Else
                            wiresList(i) = Nothing
                        End If
                    Next
                    If TypeOf minElement Is Wire Then
                        detectAllWiresConnectedTo(DirectCast(minElement, Wire).getStart(), wiresList, 0)
                        detectAllWiresConnectedTo(DirectCast(minElement, Wire).getEnde(), wiresList, 0)
                    Else
                        detectAllWiresConnectedTo(DirectCast(minElement, WireLuftlinie).getStart(), wiresList, 0)
                        detectAllWiresConnectedTo(DirectCast(minElement, WireLuftlinie).getEnde(), wiresList, 0)
                    End If
                    erg.Clear()
                    For i As Integer = 0 To myElemente.Count - 1
                        If wiresList(i) IsNot Nothing Then
                            erg.Add(myElemente(i))
                        End If
                    Next
                ElseIf TypeOf minElement Is Basic_Bezier Then
                    findAllBezierConnectedTo(DirectCast(minElement, Basic_Bezier), erg)
                ElseIf TypeOf minElement Is Basic_Linie Then
                    findAllLinieConnectedTo(DirectCast(minElement, Basic_Linie), erg)
                End If
            End If

            Return erg
        End If
    End Function

    Private Sub findAllBezierConnectedTo(b As Basic_Bezier, erg As List(Of Object))
        Dim hatSchon(myElemente.Count - 1) As Boolean 'false = unbekannt, true = ist dabei
        For i As Integer = 0 To hatSchon.Length - 1
            hatSchon(i) = False
            If myElemente(i).Equals(b) Then
                hatSchon(i) = True
            End If
        Next

        findRecAllBezierConnectedTo(b.getStart(), hatSchon)
        findRecAllBezierConnectedTo(b.getEnde(), hatSchon)

        For i As Integer = 0 To myElemente.Count - 1
            If hatSchon(i) AndAlso Not myElemente(i).Equals(b) Then
                erg.Add(myElemente(i))
            End If
        Next
    End Sub
    Private Sub findRecAllBezierConnectedTo(p As Point, hatSchon() As Boolean)
        For i As Integer = 0 To myElemente.Count - 1
            If Not hatSchon(i) AndAlso TypeOf myElemente(i) Is Basic_Bezier Then
                With DirectCast(myElemente(i), Basic_Bezier)
                    If .getStart() = p Then
                        hatSchon(i) = True
                        findRecAllBezierConnectedTo(.getEnde(), hatSchon)
                    ElseIf .getEnde() = p Then
                        hatSchon(i) = True
                        findRecAllBezierConnectedTo(.getStart(), hatSchon)
                    End If
                End With
            End If
        Next
    End Sub

    Private Sub findAllLinieConnectedTo(b As Basic_Linie, erg As List(Of Object))
        Dim hatSchon(myElemente.Count - 1) As Boolean 'false = unbekannt, true = ist dabei
        For i As Integer = 0 To hatSchon.Length - 1
            hatSchon(i) = False
            If myElemente(i).Equals(b) Then
                hatSchon(i) = True
            End If
        Next

        findRecAllLinieConnectedTo(b.getStart(), hatSchon)
        findRecAllLinieConnectedTo(b.getEnde(), hatSchon)

        For i As Integer = 0 To myElemente.Count - 1
            If hatSchon(i) AndAlso Not myElemente(i).Equals(b) Then
                erg.Add(myElemente(i))
            End If
        Next
    End Sub
    Private Sub findRecAllLinieConnectedTo(p As Point, hatSchon() As Boolean)
        For i As Integer = 0 To myElemente.Count - 1
            If Not hatSchon(i) AndAlso TypeOf myElemente(i) Is Basic_Linie Then
                With DirectCast(myElemente(i), Basic_Linie)
                    If .getStart() = p Then
                        hatSchon(i) = True
                        findRecAllLinieConnectedTo(.getEnde(), hatSchon)
                    ElseIf .getEnde() = p Then
                        hatSchon(i) = True
                        findRecAllLinieConnectedTo(.getStart(), hatSchon)
                    End If
                End With
            End If
        Next
    End Sub

    Public Function getCurrentSelectionMode() As SelectionMode
        Dim ctrl As Boolean = My.Computer.Keyboard.CtrlKeyDown
        Dim shift As Boolean = My.Computer.Keyboard.ShiftKeyDown
        If ctrl And Not shift Then
            Return SelectionMode.SubtractFromSelection
        ElseIf Not ctrl And shift Then
            Return SelectionMode.AddSelection
        Else
            Return SelectionMode.SelectOnlyNewElements
        End If
    End Function

    Public Function has_selection() As Boolean
        For Each element As ElementMaster In myElemente
            If element.hasSelection() Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Sub LockSelection()
        _lockSelection = True
    End Sub

    Public Sub UnlockSelection()
        _lockSelection = False
    End Sub

    Private Sub OnSelectionChanged()
        Me.simplifyWires()
        If Not noSelectionChangedEvents_ForSelectionBeforeDelete Then
            RaiseEvent SelectionChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Public Function darfSelektieren(e As ElementMaster) As Boolean
        If Not Select_Bauteile AndAlso (TypeOf e Is Bauteil AndAlso Not TypeOf e Is ElementWithStrokeFill) Then Return False
        If Not Select_Wires AndAlso (TypeOf e Is Wire OrElse TypeOf e Is WireLuftlinie) Then Return False
        If Not Select_Beschriftung AndAlso TypeOf e Is Basic_Spannungspfeil Then Return False
        If Not Select_Drawings AndAlso TypeOf e Is ElementWithStrokeFill Then Return False
        If Not Select_Beschriftung AndAlso TypeOf e Is SnapableCurrentArrow Then Return False
        If Not Select_Beschriftung AndAlso TypeOf e Is SnapableLabel Then Return False
        If Not Select_Beschriftung AndAlso TypeOf e Is SnapableBusTap Then Return False
        If Not Select_Beschriftung AndAlso TypeOf e Is SnapableImpedanceArrow Then Return False
        Return True
    End Function
#End Region

#Region "Zoom"
    Public Sub fit_to_screen()
        If (myElemente IsNot Nothing AndAlso myElemente.Count > 0) Then

            Dim rect As Rectangle = Me.myLastDrawingBoundingBox

            If rect.Width > 0 AndAlso rect.Height > 0 Then
                rect.X -= rect.Width \ 20
                rect.Y -= rect.Height \ 20
                rect.Width += rect.Width \ 10
                rect.Height += rect.Height \ 10

                Dim faktorX As Single = CSng(Me.Width / rect.Width)
                Dim faktorY As Single = CSng(Me.Height / rect.Height)

                faktor = Math.Min(faktorX, faktorY)
                'f*rect.mitte+offset=mitte
                'offset = mitte - f*rect.mitte
                offsetX = CSng(Me.Width / 2 - faktor * (rect.X + rect.Width / 2))
                offsetY = CSng(Me.Height / 2 - faktor * (rect.Y + rect.Height / 2))

                updateCursorIfRequiered()
                Me.Invalidate()
            End If
        End If
    End Sub
#End Region

#Region "Move, Rotate"

    Public Sub moveSelectedElements_Simple(dx As Integer, dy As Integer)
        For Each element As ElementMaster In myElemente
            If TypeOf element Is Element Then
                If DirectCast(element, Element).isSelected Then
                    DirectCast(element, Element).position = New Point(DirectCast(element, Element).position.X + dx, DirectCast(element, Element).position.Y + dy)
                End If
            ElseIf TypeOf element Is SnapableElement Then
                For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                    If DirectCast(element, SnapableElement).getSnappoint(i).isSelected Then
                        DirectCast(element, SnapableElement).getSnappoint(i).move(dx, dy)
                    End If
                Next
            End If
        Next
        simplifyWires()
    End Sub

    Public Sub moveSelectedElements(dx As Integer, dy As Integer)
        'Wenn man erst x und dann y verschiebt kann es zu folgendem Bug kommen:
        'Beim verschieben von x snappen zwei elemente zusammen. Diese werden dann beim verschieben von y mitverschoben und bleiben gesnappt (auch wenn sie es vorher nicht waren!)

        'If dx <> 0 Then
        'moveSelectedElements_Intern(dx, 0, Nothing)
        'End If
        'If dy <> 0 Then
        'moveSelectedElements_Intern(0, dy, Nothing)
        'End If
        moveSelectedElements_Intern(dx, dy, Nothing)
        simplifyWires()
    End Sub

    Private Sub moveSelectedElements_Intern(dx As Integer, dy As Integer, onlyMoveElement As Element)
        Dim verschobeneElemente(myElemente.Count - 1) As ElementMaster
        Dim neueWires As New List(Of Wire)
        Dim neueWiresFix As New List(Of Wire)

        'Selected Elemente verschieben
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Element Then
                If (onlyMoveElement IsNot Nothing AndAlso myElemente(i).Equals(onlyMoveElement)) OrElse
                   (onlyMoveElement Is Nothing AndAlso DirectCast(myElemente(i), Element).isSelected) Then
                    verschobeneElemente(i) = myElemente(i).Clone()

                    If TypeOf myElemente(i) Is BauteilAusDatei Then
                        DirectCast(myElemente(i), BauteilAusDatei).FlatCopyCompiledTemplate(DirectCast(verschobeneElemente(i), BauteilAusDatei))
                    ElseIf TypeOf myElemente(i) Is ElementGruppe Then
                        DirectCast(myElemente(i), ElementGruppe).FlatCopyCompiledTemplates(DirectCast(verschobeneElemente(i), ElementGruppe))
                    End If

                    DirectCast(verschobeneElemente(i), Element).position = New Point(DirectCast(myElemente(i), Element).position.X + dx, DirectCast(myElemente(i), Element).position.Y + dy)

                    If TypeOf myElemente(i) Is Wire Then
                        'mitverschieben von wires
                        mitVerschiebenWire(False, i, verschobeneElemente, True, True, neueWires, neueWiresFix)
                    ElseIf TypeOf myElemente(i) Is Bauteil OrElse TypeOf myElemente(i) Is WireLuftlinie OrElse TypeOf myElemente(i) Is ElementGruppe Then
                        'mitverschieben von wires
                        For k As Integer = 0 To DirectCast(myElemente(i), Element).NrOfSnappoints - 1
                            mitVerschiebenBauteilSnappoint(False, DirectCast(myElemente(i), Element).getSnappoint(k).p, DirectCast(verschobeneElemente(i), Element).getSnappoint(k).p, verschobeneElemente, neueWires, neueWiresFix)
                        Next
                    End If
                End If
            End If
        Next


        'Für alle Verschobenen wires die Wire-bezogenen Elemente mitverschieben
        Dim dx1 As Integer = dx
        Dim dy1 As Integer = dy
        If onlyMoveElement IsNot Nothing Then
            dx1 = 0 'Auch wenn selektiert -> nicht verschieben!
            dy1 = 0
        End If
        For i As Integer = 0 To myElemente.Count - 1
            'If verschobeneElemente(i) IsNot Nothing Then
            ''dieses element wurde verschoben!
            If TypeOf myElemente(i) Is IWire Then
                'Prüfen ob ein Snappoint mitverschoben werden muss!
                For j As Integer = 0 To myElemente.Count - 1
                    If TypeOf myElemente(j) Is SnapableElement Then
                        mitVerschiebenWennNötig(DirectCast(myElemente(i), Element), verschobeneElemente(i), DirectCast(myElemente(j), SnapableElement), verschobeneElemente(j), dx1, dy1)
                    End If
                Next
            End If
            'End If
        Next

        'zurückkopieren
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is SnapableElement Then
                If verschobeneElemente(i) IsNot Nothing Then
                    For k As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                        If DirectCast(verschobeneElemente(i), SnapableElement).getSnappoint(k) IsNot Nothing Then
                            'zurückkopieren, wenn geändert worde!
                            DirectCast(myElemente(i), SnapableElement).setSnappoint(k, DirectCast(verschobeneElemente(i), SnapableElement).getSnappoint(k))
                        ElseIf DirectCast(myElemente(i), SnapableElement).getSnappoint(k).isSelected Then
                            'wurde noch nicht verschoben! Wenn es selected ist muss es noch verschoben werden!
                            DirectCast(myElemente(i), SnapableElement).getSnappoint(k).move(dx1, dy1)
                        End If
                    Next
                Else
                    If onlyMoveElement Is Nothing Then
                        'Muss noch komplett verschoben werden!
                        For k As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                            If DirectCast(myElemente(i), SnapableElement).getSnappoint(k).isSelected Then
                                DirectCast(myElemente(i), SnapableElement).getSnappoint(k).move(dx1, dy1)
                            End If
                        Next
                    End If
                End If
            End If
        Next
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Element AndAlso verschobeneElemente(i) IsNot Nothing Then
                myElemente(i) = verschobeneElemente(i)
            End If
        Next

        'neue Wires hinzufügen
        Dim hatStart As Boolean
        Dim hatEnde As Boolean
        Dim w As Wire
        For i As Integer = 0 To neueWires.Count - 1
            'nur neue Wires hinzufügen, die an beiden Seiten irgendwo Kontakt haben!
            hatEnde = False
            hatStart = False
            For j As Integer = 0 To myElemente.Count - 1
                If TypeOf myElemente(j) Is Wire Then
                    w = DirectCast(myElemente(j), Wire)
                    If Not hatStart AndAlso w.liegtAufWire(neueWires(i).getStart()) Then
                        hatStart = True
                    End If
                    If Not hatEnde AndAlso w.liegtAufWire(neueWires(i).getEnde()) Then
                        hatEnde = True
                    End If
                    If hatStart AndAlso hatEnde Then
                        Exit For
                    End If
                End If
            Next
            If hatStart AndAlso hatEnde Then
                myElemente.Add(neueWires(i))
            End If
        Next

        'Neue WiresFix hinzufügen
        myElemente.AddRange(neueWiresFix)

        Me.Invalidate()
    End Sub

    Private Sub mitVerschiebenBauteilSnappoint(IGNORE_SELECTION As Boolean, pVorher As Point, pNachher As Point, verschobeneElemente() As ElementMaster, neueWires As List(Of Wire), neueWiresFix As List(Of Wire))
        Dim w As Wire
        Dim wl As WireLuftlinie
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Wire AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) AndAlso verschobeneElemente(i) Is Nothing Then
                w = DirectCast(myElemente(i), Wire)
                If w.getStart() = pVorher Then
                    verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), Wire).moveStart(pNachher, w.vector)
                    mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                ElseIf w.getEnde() = pVorher Then
                    verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), Wire).moveEnde(pNachher, w.vector)
                    mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                End If
            ElseIf TypeOf myElemente(i) Is Wire AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) AndAlso verschobeneElemente(i) IsNot Nothing Then
                'Für ein wire das schon verschoben wurde!
                w = DirectCast(myElemente(i), Wire)
                If w.getStart() = pVorher Then
                    DirectCast(verschobeneElemente(i), Wire).moveStart(pNachher, w.vector)
                ElseIf w.getEnde() = pVorher Then
                    DirectCast(verschobeneElemente(i), Wire).moveEnde(pNachher, w.vector)
                End If
            ElseIf TypeOf myElemente(i) Is WireLuftlinie AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) Then
                wl = DirectCast(myElemente(i), WireLuftlinie)
                If wl.getStart() = pVorher Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveStart(pNachher)
                ElseIf wl.getEnde() = pVorher Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveEnde(pNachher)
                End If
            ElseIf TypeOf myElemente(i) Is Bauteil AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) AndAlso verschobeneElemente(i) Is Nothing Then
                'mit verschieben von Bauteil!
                Dim istVerbunden As Boolean = False
                For k As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                    If DirectCast(myElemente(i), Bauteil).getSnappoint(k).p = pVorher Then
                        istVerbunden = True
                        Exit For
                    End If
                Next
                If istVerbunden Then
                    verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), Bauteil).position = New Point(DirectCast(verschobeneElemente(i), Bauteil).position.X + pNachher.X - pVorher.X, DirectCast(verschobeneElemente(i), Bauteil).position.Y + pNachher.Y - pVorher.Y)
                    'mitverschieben von wires
                    For k As Integer = 0 To DirectCast(verschobeneElemente(i), Bauteil).NrOfSnappoints - 1
                        mitVerschiebenBauteilSnappoint(IGNORE_SELECTION, DirectCast(verschobeneElemente(i), Bauteil).getSnappoint(k).p, DirectCast(verschobeneElemente(i), Bauteil).getSnappoint(k).p, verschobeneElemente, neueWires, neueWiresFix)
                    Next
                End If
            End If
        Next
    End Sub

    Private Sub mitVerschiebenWire(IGNORE_SELECTION As Boolean, index As Integer, verschobeneElemente() As ElementMaster, doStart As Boolean, doEnde As Boolean, neueWires As List(Of Wire), neueWiresFix As List(Of Wire))
        Dim wRef As Wire = DirectCast(myElemente(index), Wire)
        Dim wRefPostMove As Wire = DirectCast(verschobeneElemente(index), Wire)

        Dim wRefStartSchieben As Boolean = doStart And wRef.getStart <> wRefPostMove.getStart
        Dim wRefEndeSchieben As Boolean = doEnde And wRef.getEnde <> wRefPostMove.getEnde

        If wRefStartSchieben = False AndAlso wRefEndeSchieben = False Then
            Return
        End If

        Dim w As Wire
        Dim wPostMove As Wire
        Dim wl As WireLuftlinie

        Dim liegenInSenkrechterLinie, liegenInHorizontalerLinie As Boolean
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Wire AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection) AndAlso verschobeneElemente(i) Is Nothing Then
                w = DirectCast(myElemente(i), Wire)
                'Fall 1: Wires stehen senkrecht aufeinander
                If (w.vector.X = 0 AndAlso wRef.vector.Y = 0) OrElse (w.vector.Y = 0 AndAlso wRef.vector.X = 0) Then
                    'prüfen ob Start oder Endpunkte zusammenfallen!
                    If w.getStart() = wRef.getStart() AndAlso wRefStartSchieben Then
                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), Wire).moveStart(wRefPostMove.getStart(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                    ElseIf w.getStart() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), Wire).moveStart(wRefPostMove.getEnde(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                    ElseIf w.getEnde() = wRef.getStart() AndAlso wRefStartSchieben Then
                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), Wire).moveEnde(wRefPostMove.getStart(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                    ElseIf w.getEnde() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), Wire).moveEnde(wRefPostMove.getEnde(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                    End If
                Else
                    liegenInSenkrechterLinie = w.vector.X = 0 AndAlso wRef.vector.X = 0 AndAlso w.position.X = wRef.position.X
                    liegenInHorizontalerLinie = w.vector.Y = 0 AndAlso wRef.vector.Y = 0 AndAlso w.position.Y = wRef.position.Y
                    If liegenInHorizontalerLinie OrElse liegenInSenkrechterLinie Then

                        If w.getStart() = wRef.getStart() AndAlso wRefStartSchieben Then
                            Dim neuerStart As Point
                            If liegenInSenkrechterLinie Then
                                neuerStart = New Point(w.getStart().X, wRefPostMove.getStart().Y)
                            Else
                                neuerStart = New Point(wRefPostMove.getStart().X, w.getStart().Y)
                            End If
                            If neuerStart <> w.getStart() Then
                                verschobeneElemente(i) = myElemente(i).Clone()
                                DirectCast(verschobeneElemente(i), Wire).moveStart(neuerStart, w.vector)
                                mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                            End If
                            neueWires.Add(New Wire(Me.getNewID(), neuerStart, wRefPostMove.getStart()))
                        ElseIf w.getStart() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                            Dim neuerStart As Point
                            If liegenInSenkrechterLinie Then
                                neuerStart = New Point(w.getStart().X, wRefPostMove.getEnde().Y)
                            Else
                                neuerStart = New Point(wRefPostMove.getEnde().X, w.getStart().Y)
                            End If
                            If neuerStart <> w.getStart() Then
                                verschobeneElemente(i) = myElemente(i).Clone()
                                DirectCast(verschobeneElemente(i), Wire).moveStart(neuerStart, w.vector)
                                mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                            End If
                            neueWires.Add(New Wire(Me.getNewID(), neuerStart, wRefPostMove.getEnde()))
                        ElseIf w.getEnde() = wRef.getStart() AndAlso wRefStartSchieben Then
                            Dim neuesEnde As Point
                            If liegenInSenkrechterLinie Then
                                neuesEnde = New Point(w.getEnde().X, wRefPostMove.getStart().Y)
                            Else
                                neuesEnde = New Point(wRefPostMove.getStart().X, w.getEnde().Y)
                            End If
                            If neuesEnde <> w.getEnde() Then
                                verschobeneElemente(i) = myElemente(i).Clone()
                                DirectCast(verschobeneElemente(i), Wire).moveEnde(neuesEnde, w.vector)
                                mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                            End If
                            neueWires.Add(New Wire(Me.getNewID(), neuesEnde, wRefPostMove.getStart()))
                        ElseIf w.getEnde() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                            Dim neuesEnde As Point
                            If liegenInSenkrechterLinie Then
                                neuesEnde = New Point(w.getEnde().X, wRefPostMove.getEnde().Y)
                            Else
                                neuesEnde = New Point(wRefPostMove.getEnde().X, w.getEnde().Y)
                            End If
                            If neuesEnde <> w.getEnde() Then
                                verschobeneElemente(i) = myElemente(i).Clone()
                                DirectCast(verschobeneElemente(i), Wire).moveEnde(neuesEnde, w.vector)
                                mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                            End If
                            neueWires.Add(New Wire(Me.getNewID(), neuesEnde, wRefPostMove.getEnde()))
                        End If
                    End If
                End If
            ElseIf TypeOf myElemente(i) Is Wire AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) AndAlso verschobeneElemente(i) IsNot Nothing Then
                'Für ein Wire das schon verschoben wurde!
                'Fall 1: Stehen senkrecht aufeinander
                w = DirectCast(myElemente(i), Wire)
                wPostMove = DirectCast(verschobeneElemente(i), Wire)

                Dim wStartSchieben As Boolean = w.getStart() = wPostMove.getStart()
                Dim wEndeSchieben As Boolean = w.getEnde() = wPostMove.getEnde()

                If (w.vector.X = 0 AndAlso wRef.vector.Y = 0) OrElse (w.vector.Y = 0 AndAlso wRef.vector.X = 0) Then
                    'prüfen ob Start oder Endpunkte zusammenfallen!
                    If wStartSchieben AndAlso w.getStart() = wRef.getStart() AndAlso wRefStartSchieben Then
                        DirectCast(verschobeneElemente(i), Wire).moveStart(wRefPostMove.getStart(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                    ElseIf wStartSchieben AndAlso w.getStart() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                        DirectCast(verschobeneElemente(i), Wire).moveStart(wRefPostMove.getEnde(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, False, True, neueWires, neueWiresFix)
                    ElseIf wEndeSchieben AndAlso w.getEnde() = wRef.getStart() AndAlso wRefStartSchieben Then
                        DirectCast(verschobeneElemente(i), Wire).moveEnde(wRefPostMove.getStart(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                    ElseIf wEndeSchieben AndAlso w.getEnde() = wRef.getEnde() AndAlso wRefEndeSchieben Then
                        DirectCast(verschobeneElemente(i), Wire).moveEnde(wRefPostMove.getEnde(), w.vector)
                        mitVerschiebenWire(IGNORE_SELECTION, i, verschobeneElemente, True, False, neueWires, neueWiresFix)
                    End If
                End If
            ElseIf TypeOf myElemente(i) Is Bauteil AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection) Then
                'prüfen ob Anschluss an das Bauteil weitergeführt werden muss!

                If wRefStartSchieben Then
                    KeepConnectivityToStart(DirectCast(myElemente(i), Bauteil), wRef, wRefPostMove, neueWiresFix)
                End If
                If wRefEndeSchieben Then
                    KeepConnectivityToEnde(DirectCast(myElemente(i), Bauteil), wRef, wRefPostMove, neueWiresFix)
                End If
            ElseIf TypeOf myElemente(i) Is ElementGruppe AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection) Then
                'prüfen ob Anschluss an die Gruppe weitergeführt werden muss!
                If wRefStartSchieben Then
                    KeepConnectivityToStart(DirectCast(myElemente(i), ElementGruppe), wRef, wRefPostMove, neueWiresFix)
                End If
                If wRefEndeSchieben Then
                    KeepConnectivityToEnde(DirectCast(myElemente(i), ElementGruppe), wRef, wRefPostMove, neueWiresFix)
                End If
            ElseIf TypeOf myElemente(i) Is WireLuftlinie AndAlso (IGNORE_SELECTION OrElse Not myElemente(i).hasSelection()) Then
                wl = DirectCast(myElemente(i), WireLuftlinie)
                If wRefStartSchieben AndAlso wRef.getStart() = wl.getStart() Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveStart(wRefPostMove.getStart())
                ElseIf wRefStartSchieben AndAlso wRef.getStart() = wl.getEnde() Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveEnde(wRefPostMove.getStart())
                ElseIf wRefEndeSchieben AndAlso wRef.getEnde() = wl.getStart() Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveStart(wRefPostMove.getEnde())
                ElseIf wRefEndeSchieben AndAlso wRef.getEnde() = wl.getEnde() Then
                    If verschobeneElemente(i) Is Nothing Then verschobeneElemente(i) = myElemente(i).Clone()
                    DirectCast(verschobeneElemente(i), WireLuftlinie).moveEnde(wRefPostMove.getEnde())
                End If
            End If
        Next
    End Sub

    Private Sub KeepConnectivityToStart(b As Element, wPreMove As Wire, wPostMove As Wire, neueWires As List(Of Wire))
        Dim s As Snappoint
        For i As Integer = 0 To b.NrOfSnappoints - 1
            s = b.getSnappoint(i)
            If s.p = wPreMove.getStart() Then
                'Verbindung weiter aufrechterhalten!
                Dim ecke As Point

                If wPreMove.vector.Y = 0 Then
                    ecke = New Point(wPreMove.getStart().X, wPostMove.getStart().Y)
                ElseIf wPreMove.vector.X = 0 Then
                    ecke = New Point(wPostMove.getStart().X, wPreMove.getStart().Y)
                Else
                    Throw New Exception("Wire darf nicht schräg sein!")
                End If
                If ecke <> wPostMove.getStart() Then neueWires.Add(New Wire(Me.getNewID(), ecke, wPostMove.getStart()))
                If ecke <> wPreMove.getStart() Then neueWires.Add(New Wire(Me.getNewID(), wPreMove.getStart(), ecke))
            End If
        Next
    End Sub

    Private Sub KeepConnectivityToEnde(b As Element, wPreMove As Wire, wPostMove As Wire, neueWires As List(Of Wire))
        Dim s As Snappoint
        For i As Integer = 0 To b.NrOfSnappoints - 1
            s = b.getSnappoint(i)
            If s.p = wPreMove.getEnde() Then
                'Verbindung weiter aufrechterhalten!
                Dim ecke As Point

                If wPreMove.vector.Y = 0 Then
                    ecke = New Point(wPreMove.getEnde().X, wPostMove.getEnde().Y)
                ElseIf wPreMove.vector.X = 0 Then
                    ecke = New Point(wPostMove.getEnde().X, wPreMove.getEnde().Y)
                Else
                    Throw New Exception("Wire darf nicht schräg sein!")
                End If
                If ecke <> wPostMove.getEnde() Then neueWires.Add(New Wire(Me.getNewID(), ecke, wPostMove.getEnde()))
                If ecke <> wPreMove.getEnde() Then neueWires.Add(New Wire(Me.getNewID(), wPreMove.getEnde(), ecke))
            End If
        Next
    End Sub

    Private Sub mitVerschiebenWennNötig(wireVorher As Element, wireNachhaer As ElementMaster, snapVorher As SnapableElement, ByRef _out_snapNachher As ElementMaster, dx As Integer, dy As Integer)
        Dim pos, posNeu As WireSnappoint
        For i As Integer = 0 To snapVorher.getNrOfSnappoints() - 1
            pos = snapVorher.getSnappoint(i)
            If Not pos.isSelected Then
                'Nicht selektiert -> nicht von selbst verschieben!
                posNeu = pos.Move_WennliegtAufWire(wireVorher, wireNachhaer, 0, 0)
            Else
                'ist selektiert! -> von selbst schon um dx, dy verschieben!
                posNeu = pos.Move_WennliegtAufWire(wireVorher, wireNachhaer, dx, dy)
            End If

            If posNeu IsNot Nothing Then
                If _out_snapNachher Is Nothing Then
                    _out_snapNachher = DirectCast(snapVorher.Clone(), SnapableElement)
                    For k As Integer = 0 To DirectCast(_out_snapNachher, SnapableElement).getNrOfSnappoints() - 1
                        DirectCast(_out_snapNachher, SnapableElement).setSnappoint(k, Nothing)
                    Next
                End If
                DirectCast(_out_snapNachher, SnapableElement).setSnappoint(i, posNeu)
            End If
        Next
    End Sub

#Region "Rotate"
    Public Sub dreheSelectedUm90Grad(umCursor As Boolean, ohneUndo As Boolean)
        If Me.has_selection Then
            Dim rück As RückgängigGrafik = Nothing
            If Not ohneUndo Then
                rück = New RückgängigGrafik()
                rück.setText("Im Uhrzeigersinn drehen")
                rück.speicherVorherZustand(Me.getRückArgs())
            End If

            Dim pos As Point = getDrehpunkt(umCursor)
            dreheSelected(pos, Drehmatrix.Drehen90Grad)

            If Not ohneUndo Then
                rück.speicherNachherZustand(Me.getRückArgs())
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
        End If
    End Sub

    Public Sub dreheSelectedUmMinus90Grad(umCursor As Boolean, ohneUndo As Boolean)
        If Me.has_selection() Then
            Dim rück As RückgängigGrafik = Nothing
            If Not ohneUndo Then
                rück = New RückgängigGrafik()
                rück.setText("Gegen den Uhrzeigersinn drehen")
                rück.speicherVorherZustand(Me.getRückArgs())
            End If

            Dim pos As Point = getDrehpunkt(umCursor)
            dreheSelected(pos, Drehmatrix.DrehenMinus90Grad)

            If Not ohneUndo Then
                rück.speicherNachherZustand(Me.getRückArgs())
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
        End If
    End Sub

    Public Sub dreheSelectedSpiegelnHorizontal(umCursor As Boolean, ohneUndo As Boolean)
        If Me.has_selection() Then
            Dim rück As RückgängigGrafik = Nothing
            If Not ohneUndo Then
                rück = New RückgängigGrafik()
                rück.setText("Horizontal spiegeln")
                rück.speicherVorherZustand(Me.getRückArgs())
            End If

            Dim pos As Point = getDrehpunkt(umCursor)
            dreheSelected(pos, Drehmatrix.MirrorX)

            If Not ohneUndo Then
                rück.speicherNachherZustand(Me.getRückArgs())
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
        End If
    End Sub

    Public Sub dreheSelectedSpiegelnVertikal(umCursor As Boolean, ohneUndo As Boolean)
        If Me.has_selection() Then
            Dim rück As RückgängigGrafik = Nothing
            If Not ohneUndo Then
                rück = New RückgängigGrafik()
                rück.setText("Vertikal spiegeln")
                rück.speicherVorherZustand(Me.getRückArgs())
            End If

            Dim pos As Point = getDrehpunkt(umCursor)
            dreheSelected(pos, Drehmatrix.MirrorY)

            If Not ohneUndo Then
                rück.speicherNachherZustand(Me.getRückArgs())
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
        End If
    End Sub

    Private Function getDrehpunkt(umCursor As Boolean) As Point
        If umCursor Then
            Return Me.GetCursorPos()
        Else
            Dim minX As Integer = Integer.MaxValue
            Dim minY As Integer = Integer.MaxValue
            Dim maxX As Integer = Integer.MinValue
            Dim maxY As Integer = Integer.MinValue
            For i As Integer = 0 To myElemente.Count - 1
                If TypeOf myElemente(i) Is Element AndAlso myElemente(i).hasSelection() Then
                    Dim s As Selection = DirectCast(myElemente(i), Element).getSelection()
                    If TypeOf s Is SelectionRect Then
                        Dim sr As SelectionRect = DirectCast(s, SelectionRect)
                        minX = Math.Min(minX, sr.r.X)
                        minY = Math.Min(minY, sr.r.Y)
                        maxX = Math.Max(maxX, sr.r.X + sr.r.Width)
                        maxY = Math.Max(maxY, sr.r.Y + sr.r.Height)
                    ElseIf TypeOf s Is SelectionLine Then
                        Dim sl As SelectionLine = DirectCast(s, SelectionLine)
                        minX = Math.Min(minX, sl.start.X)
                        minY = Math.Min(minY, sl.start.Y)
                        maxX = Math.Max(maxX, sl.start.X)
                        maxY = Math.Max(maxY, sl.start.Y)

                        minX = Math.Min(minX, sl.ende.X)
                        minY = Math.Min(minY, sl.ende.Y)
                        maxX = Math.Max(maxX, sl.ende.X)
                        maxY = Math.Max(maxY, sl.ende.Y)
                    ElseIf TypeOf s Is SelectionBezier Then
                        Dim sb As SelectionBezier = DirectCast(s, SelectionBezier)
                        minX = Math.Min(minX, sb.p1.X)
                        minY = Math.Min(minY, sb.p1.Y)
                        maxX = Math.Max(maxX, sb.p1.X)
                        maxY = Math.Max(maxY, sb.p1.Y)

                        minX = Math.Min(minX, sb.p4.X)
                        minY = Math.Min(minY, sb.p4.Y)
                        maxX = Math.Max(maxX, sb.p4.X)
                        maxY = Math.Max(maxY, sb.p4.Y)
                    End If
                ElseIf TypeOf myElemente(i) Is SnapableElement Then
                    For j As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                        If DirectCast(myElemente(i), SnapableElement).getSnappoint(j).isSelected Then
                            Dim s As Selection = DirectCast(myElemente(i), SnapableElement).getSnappoint(j).getSelection()
                            If TypeOf s Is SelectionRect Then
                                Dim sr As SelectionRect = DirectCast(s, SelectionRect)
                                minX = Math.Min(minX, sr.r.X)
                                minY = Math.Min(minY, sr.r.Y)
                                maxX = Math.Max(maxX, sr.r.X + sr.r.Width)
                                maxY = Math.Max(maxY, sr.r.Y + sr.r.Height)
                            ElseIf TypeOf s Is SelectionLine Then
                                Dim sl As SelectionLine = DirectCast(s, SelectionLine)
                                minX = Math.Min(minX, sl.start.X)
                                minY = Math.Min(minY, sl.start.Y)
                                maxX = Math.Max(maxX, sl.start.X)
                                maxY = Math.Max(maxY, sl.start.Y)

                                minX = Math.Min(minX, sl.ende.X)
                                minY = Math.Min(minY, sl.ende.Y)
                                maxX = Math.Max(maxX, sl.ende.X)
                                maxY = Math.Max(maxY, sl.ende.Y)
                            End If
                        End If
                    Next
                End If
            Next
            If maxX < minX OrElse maxY < minY Then
                Throw New Exception("Kann jetzt nicht drehen")
            End If

            Return New Point(fitToGridX((minX + maxX) \ 2) * GridX, fitToGridY((minY + maxY) \ 2) * GridY)
        End If
    End Function

    Public Sub dreheSelected(pos As Point, drehung As Drehmatrix)
        For i As Integer = 0 To myElemente.Count - 1
            If myElemente(i).hasSelection() Then
                myElemente(i).drehe(pos, drehung)
            End If
        Next
        simplifyWires()
        Me.Invalidate()
    End Sub

    Public Sub dreheSelectedBauteileUmJeweiligenMittelpunkt(drehung As Drehmatrix)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Bauteil AndAlso myElemente(i).hasSelection() Then
                Dim rect As Rectangle = myElemente(i).getGrafik().getBoundingBox()
                Dim pos As New Point(rect.X + rect.Width \ 2, rect.Y + rect.Height \ 2)
                myElemente(i).drehe(pos, drehung)
            End If
        Next
        Me.Invalidate()
    End Sub
#End Region
#End Region

#Region "Scale Wire"
    Public Sub moveWirePos(pos As Point, dx As Integer, dy As Integer)
        Dim neueWires As New List(Of Wire)
        Dim neueWiresFix As New List(Of Wire)

        Dim posNeu As Point = New Point(pos.X + dx, pos.Y + dy)
        Dim verschobeneElemente(myElemente.Count - 1) As ElementMaster
        Dim moveUrsprung(myElemente.Count - 1) As Boolean

        'Selected Elemente verschieben
        For i As Integer = 0 To myElemente.Count - 1
            moveUrsprung(i) = False
            If TypeOf myElemente(i) Is Wire Then
                With DirectCast(myElemente(i), Wire)
                    If .getStart() = pos Then
                        If .vector.X = 0 AndAlso .vector.Y <> 0 Then
                            verschobeneElemente(i) = myElemente(i).Clone()
                            DirectCast(verschobeneElemente(i), Wire).position = posNeu
                            DirectCast(verschobeneElemente(i), Wire).vector.Y -= dy
                            moveUrsprung(i) = True
                        ElseIf .vector.Y = 0 AndAlso .vector.X <> 0 Then
                            verschobeneElemente(i) = myElemente(i).Clone()
                            DirectCast(verschobeneElemente(i), Wire).position = posNeu
                            DirectCast(verschobeneElemente(i), Wire).vector.X -= dx
                            moveUrsprung(i) = True
                        End If
                    ElseIf .getEnde() = pos Then
                        If .vector.X = 0 AndAlso .vector.Y <> 0 Then
                            verschobeneElemente(i) = myElemente(i).Clone()
                            DirectCast(verschobeneElemente(i), Wire).position = New Point(.position.X + dx, .position.Y)
                            DirectCast(verschobeneElemente(i), Wire).vector.Y += dy
                            moveUrsprung(i) = True
                        ElseIf .vector.Y = 0 AndAlso .vector.X <> 0 Then
                            verschobeneElemente(i) = myElemente(i).Clone()
                            DirectCast(verschobeneElemente(i), Wire).position = New Point(.position.X, .position.Y + dy)
                            DirectCast(verschobeneElemente(i), Wire).vector.X += dx
                            moveUrsprung(i) = True
                        End If
                    End If
                End With
            ElseIf TypeOf myElemente(i) Is WireLuftlinie Then
                With DirectCast(myElemente(i), WireLuftlinie)
                    If .getStart() = pos Then

                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), WireLuftlinie).position = posNeu
                        DirectCast(verschobeneElemente(i), WireLuftlinie).vector.Y -= dy
                        DirectCast(verschobeneElemente(i), WireLuftlinie).vector.X -= dx
                        moveUrsprung(i) = True

                    ElseIf .getEnde() = pos Then

                        verschobeneElemente(i) = myElemente(i).Clone()
                        DirectCast(verschobeneElemente(i), WireLuftlinie).vector.Y += dy
                        DirectCast(verschobeneElemente(i), WireLuftlinie).vector.X += dx
                        moveUrsprung(i) = True

                    End If
                End With
            End If
        Next

        For i As Integer = 0 To myElemente.Count - 1
            If moveUrsprung(i) AndAlso TypeOf myElemente(i) Is Wire Then
                'mit verschieben von anderen wires
                Dim doStart As Boolean = DirectCast(verschobeneElemente(i), Wire).getStart() <> DirectCast(myElemente(i), Wire).getStart()
                Dim doEnde As Boolean = DirectCast(verschobeneElemente(i), Wire).getEnde() <> DirectCast(myElemente(i), Wire).getEnde()

                mitVerschiebenWire(True, i, verschobeneElemente, doStart, doEnde, neueWires, neueWiresFix)
            End If
        Next

        'Für alle Verschobenen wires die Wire-bezogenen Elemente mitverschieben
        Dim dx1 As Integer = dx
        Dim dy1 As Integer = dy
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is IWire Then
                'Prüfen ob ein Snappoint mitverschoben werden muss!
                For j As Integer = 0 To myElemente.Count - 1
                    If TypeOf myElemente(j) Is SnapableElement Then
                        mitVerschiebenWennNötig(DirectCast(myElemente(i), Element), verschobeneElemente(i), DirectCast(myElemente(j), SnapableElement), verschobeneElemente(j), dx1, dy1)
                    End If
                Next
            End If
        Next

        'zurückkopieren
        For i As Integer = 0 To myElemente.Count - 1
            If verschobeneElemente(i) IsNot Nothing AndAlso TypeOf myElemente(i) Is SnapableElement Then
                For k As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                    If DirectCast(verschobeneElemente(i), SnapableElement).getSnappoint(k) IsNot Nothing Then
                        'zurückkopieren, wenn geändert worde!
                        DirectCast(myElemente(i), SnapableElement).setSnappoint(k, DirectCast(verschobeneElemente(i), SnapableElement).getSnappoint(k))
                    End If
                Next
            ElseIf verschobeneElemente(i) IsNot Nothing AndAlso TypeOf myElemente(i) Is Element Then
                myElemente(i) = verschobeneElemente(i)
            End If
        Next

        'neue Wires hinzufügen
        Dim hatStart As Boolean
        Dim hatEnde As Boolean
        Dim w As Wire
        For i As Integer = 0 To neueWires.Count - 1
            'nur neue Wires hinzufügen, die an beiden Seiten irgendwo Kontakt haben!
            hatEnde = False
            hatStart = False
            For j As Integer = 0 To myElemente.Count - 1
                If TypeOf myElemente(j) Is Wire Then
                    w = DirectCast(myElemente(j), Wire)
                    If Not hatStart AndAlso w.liegtAufWire(neueWires(i).getStart()) Then
                        hatStart = True
                    End If
                    If Not hatEnde AndAlso w.liegtAufWire(neueWires(i).getEnde()) Then
                        hatEnde = True
                    End If
                    If hatStart AndAlso hatEnde Then
                        Exit For
                    End If
                End If
            Next
            If hatStart AndAlso hatEnde Then
                myElemente.Add(neueWires(i))
            End If
        Next

        'Neue WiresFix hinzufügen
        myElemente.AddRange(neueWiresFix)

        Me.Invalidate()

    End Sub

#End Region

#Region "Routing"
    Public Sub DoRoutingLuftlinie(start As Snappoint, ende As Snappoint, pfeilEnde As ParamArrow)
        Dim rück As New RückgängigGrafik()
        rück.setText("Verbindung hinzugefügt")
        rück.speicherVorherZustand(getRückArgs())

        Dim w As New WireLuftlinie(Me.getNewID(), start.p, ende.p)
        myElemente.Add(w)
        'Add ArrowHead!
        w.pfeilEnde = pfeilEnde

        simplifyWires()

        rück.speicherNachherZustand(getRückArgs())
        addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

        Me.Invalidate()
    End Sub

    Public Sub DoRouting(start As Snappoint, ende As Snappoint, pfeilEnde As ParamArrow, RoutingOhneUmwege As Boolean)
        Dim rück As New RückgängigGrafik()
        rück.setText("Verbindung hinzugefügt")
        rück.speicherVorherZustand(getRückArgs())

        Dim routing As List(Of Point) = findRouting(start, ende, RoutingOhneUmwege)
        Dim w As Wire = Nothing
        For i As Integer = 0 To routing.Count - 2
            w = New Wire(Me.getNewID(), routing(i), routing(i + 1))
            myElemente.Add(w)

        Next
        'Add ArrowHead!
        If pfeilEnde.pfeilArt > -1 Then
            w.pfeilEnde = pfeilEnde
        End If

        simplifyWires()

        rück.speicherNachherZustand(getRückArgs())
        addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

        Me.Invalidate()
    End Sub

    Public Function sucheNextSnapPoint(pos As Point, ignore As Snappoint) As Snappoint
        Dim p1 As Snappoint
        Dim minAbstand As Long = Long.MaxValue
        Dim abstand As Long
        Dim minPoint As Snappoint = Nothing

        For Each element As ElementMaster In myElemente
            If TypeOf element Is Element Then
                For i As Integer = 0 To DirectCast(element, Element).NrOfSnappoints - 1
                    p1 = DirectCast(element, Element).getSnappoint(i)
                    If ignore Is Nothing OrElse p1.p.X <> ignore.p.X OrElse p1.p.Y <> ignore.p.Y Then
                        abstand = Mathe.abstandQuadrat(p1.p, pos)
                        If abstand < minAbstand Then
                            minAbstand = abstand
                            minPoint = p1
                        End If
                    End If
                Next
            End If
        Next
        Return minPoint
    End Function

    Public Function findRouting(_start As Snappoint, _ende As Snappoint, RoutingOhneUmwege As Boolean) As List(Of Point)
        Dim start As New Snappoint(_start.p, 0, 0, 0, 0)
        Dim ende As New Snappoint(_ende.p, 0, 0, 0, 0)

        Dim p1 As Snappoint
        For Each element As ElementMaster In myElemente
            If TypeOf element Is Element Then
                For i As Integer = 0 To DirectCast(element, Element).NrOfSnappoints - 1
                    p1 = DirectCast(element, Element).getSnappoint(i)
                    If p1.p.X = start.p.X AndAlso p1.p.Y = start.p.Y Then
                        start.Xminus += p1.Xminus
                        start.Xplus += p1.Xplus
                        start.Yminus += p1.Yminus
                        start.Yplus += p1.Yplus
                    End If
                    If p1.p.X = ende.p.X AndAlso p1.p.Y = ende.p.Y Then
                        ende.Xminus += p1.Xminus
                        ende.Xplus += p1.Xplus
                        ende.Yminus += p1.Yminus
                        ende.Yplus += p1.Yplus
                    End If
                Next
                If TypeOf element Is Wire Then
                    Dim w As Wire = DirectCast(element, Wire)
                    If w.liegtInMitteVonWire(ende.p) Then
                        If w.vector.X = 0 Then
                            ende.Yminus += TemplateAusDatei.PENALTY2
                            ende.Yplus += TemplateAusDatei.PENALTY2
                        ElseIf w.vector.Y = 0 Then
                            ende.Xplus += TemplateAusDatei.PENALTY2
                            ende.Xminus += TemplateAusDatei.PENALTY2
                        End If
                    End If
                    If w.liegtInMitteVonWire(start.p) Then
                        If w.vector.X = 0 Then
                            start.Yminus += TemplateAusDatei.PENALTY2
                            start.Yplus += TemplateAusDatei.PENALTY2
                        ElseIf w.vector.Y = 0 Then
                            start.Xplus += TemplateAusDatei.PENALTY2
                            start.Xminus += TemplateAusDatei.PENALTY2
                        End If
                    End If
                End If
            End If
        Next

        Dim erg As New List(Of Point)
        erg.Add(start.p)

        Dim rs As Richtung
        Dim re As Richtung
        Dim routing1() As Point
        Dim bewertung As Tuple(Of Integer, Double)
        Dim minBewertung As New Tuple(Of Integer, Double)(Integer.MaxValue, Double.MaxValue)
        Dim routingMin() As Point = Nothing
        For i As Integer = 0 To 3
            rs = CType(i, Richtung)
            For j As Integer = 0 To 3
                re = CType(j, Richtung)
                routing1 = findRoutingSpeziell(start.p, ende.p, rs, re, RoutingOhneUmwege)
                bewertung = bewerteRouting(routing1, start, ende)
                If bewertung.Item1 < minBewertung.Item1 OrElse (bewertung.Item1 = minBewertung.Item1 AndAlso bewertung.Item2 < minBewertung.Item2) Then
                    minBewertung = bewertung
                    routingMin = routing1
                End If
            Next
        Next

        Dim x As Integer = start.p.X
        Dim y As Integer = start.p.Y
        For i As Integer = 0 To routingMin.Length - 2
            x += routingMin(i).X
            y += routingMin(i).Y
            erg.Add(New Point(x, y))
        Next

        x += routingMin(routingMin.Length - 1).X
        y += routingMin(routingMin.Length - 1).Y
        If x <> ende.p.X OrElse y <> ende.p.Y Then
            Throw New Exception("Fehler beim Routing!")
        End If

        erg.Add(ende.p)
        Return erg
    End Function

    Private Function findRoutingSpeziell(start As Point, ende As Point, richtungStart As Richtung, richtungEnde As Richtung, RoutingOhneUmwege As Boolean) As Point()
        Dim erg() As Point = findRoutingSpeziell_onGrid(start, ende, richtungStart, richtungEnde, RoutingOhneUmwege)

        Dim punkte(erg.Length) As Point
        punkte(0) = start
        For i As Integer = 1 To erg.Length
            punkte(i) = New Point(punkte(i - 1).X + erg(i - 1).X, punkte(i - 1).Y + erg(i - 1).Y)
        Next

        Dim s As Point
        Dim e As Point
        'Von zweiten bis zum vorletzen Wire alle verschieben, damit die Punkte auf dem Grid liegen.
        'Das erste und letzte Wire kann nicht geändert werden und es liegt off-grid, falls der snapppoint selbst off-grid ist.
        'Die Wires werden immer senkrecht zu Ihrer Richtung verschoben, um dx (bzw. dy)
        For i As Integer = 1 To erg.Length - 2
            s = punkte(i)
            e = punkte(i + 1)
            If s.X = e.X Then
                'verschieben in X-Richtung möglich!
                Dim dx As Integer = fitToGridX(s.X) * GridX - s.X
                punkte(i).X += dx
                punkte(i + 1).X += dx
                erg(i - 1).X += dx
                erg(i + 1).X -= dx
            ElseIf s.Y = e.Y Then
                'verschieben in Y-Richtung möglich!
                Dim dy As Integer = fitToGridY(s.Y) * GridY - s.Y
                punkte(i).Y += dy
                punkte(i + 1).Y += dy
                erg(i - 1).Y += dy
                erg(i + 1).Y -= dy
            End If
        Next

        erg = Mathe.removeEmptyPoints(erg)

        Return erg
    End Function

    Private Function findRoutingSpeziell_onGrid(start As Point, ende As Point, richtungStart As Richtung, richtungEnde As Richtung, RoutingOhneUmwege As Boolean) As Point()
        If richtungStart = Richtung.Xplus Then
            Return findRoutingSpeziell_Xplus(start, ende, richtungEnde, RoutingOhneUmwege)
        ElseIf richtungStart = Richtung.Xminus Then
            start.X *= -1
            ende.X *= -1
            richtungStart = Richtung.Xplus
            If richtungEnde = Richtung.Xminus Then
                richtungEnde = Richtung.Xplus
            ElseIf richtungEnde = Richtung.Xplus Then
                richtungEnde = Richtung.Xminus
            End If
            Dim route() As Point = findRoutingSpeziell_Xplus(start, ende, richtungEnde, RoutingOhneUmwege)
            For i As Integer = 0 To route.Length - 1
                route(i).X *= -1
            Next
            Return route
        ElseIf richtungStart = Richtung.Yplus Then
            Dim z As Integer = start.X
            start.X = start.Y
            start.Y = z
            z = ende.X
            ende.X = ende.Y
            ende.Y = z
            richtungStart = Richtung.Xplus
            If richtungEnde = Richtung.Xminus Then
                richtungEnde = Richtung.Yminus
            ElseIf richtungEnde = Richtung.Xplus Then
                richtungEnde = Richtung.Yplus
            ElseIf richtungEnde = Richtung.Yminus Then
                richtungEnde = Richtung.Xminus
            Else
                richtungEnde = Richtung.Xplus
            End If
            Dim route() As Point = findRoutingSpeziell_Xplus(start, ende, richtungEnde, RoutingOhneUmwege)
            For i As Integer = 0 To route.Length - 1
                z = route(i).X
                route(i).X = route(i).Y
                route(i).Y = z
            Next
            Return route
        Else
            start.Y *= -1
            ende.Y *= -1
            richtungStart = Richtung.Yplus
            If richtungEnde = Richtung.Yminus Then
                richtungEnde = Richtung.Yplus
            ElseIf richtungEnde = Richtung.Yplus Then
                richtungEnde = Richtung.Yminus
            End If
            Dim route() As Point = findRoutingSpeziell_onGrid(start, ende, richtungStart, richtungEnde, RoutingOhneUmwege)
            For i As Integer = 0 To route.Length - 1
                route(i).Y *= -1
            Next
            Return route
        End If
    End Function

    Private Function findRoutingSpeziell_Xplus(start As Point, ende As Point, richtungEnde As Richtung, RoutingOhneUmwege As Boolean) As Point()
        Dim dx As Integer = ende.X - start.X
        Dim dy As Integer = ende.Y - start.Y
        Dim AUSWEICHOFFSET As Integer = ROUTING_AUSWEICHABSTAND '\ Math.Max(GridX, GridY)
        AUSWEICHOFFSET = Math.Max(AUSWEICHOFFSET, 1)

        If RoutingOhneUmwege Then
            'Einfaches Routing. Keine unnötigen Um-die-ecke Routings (wird für Cadence-Detekt verwendet!)

            AUSWEICHOFFSET = 50 'nur sehr kleinen Ausweichoffset hier!

            If dx > 0 Then
                If richtungEnde = Richtung.Xminus Then
                    If dy = 0 Then
                        Return {New Point(dx, 0)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy), New Point(dx - dx \ 2, 0)}
                    End If
                ElseIf richtungEnde = Richtung.Yminus Then
                    If dy > 0 Then
                        Return {New Point(dx, 0), New Point(0, dy)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - dx \ 2, 0), New Point(0, AUSWEICHOFFSET)}
                    End If
                ElseIf richtungEnde = Richtung.Yplus Then
                    If dy < 0 Then
                        Return {New Point(dx, 0), New Point(0, dy)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - dx \ 2, 0), New Point(0, -AUSWEICHOFFSET)}
                    End If
                Else
                    If dy <> 0 Then
                        Return {New Point(dx + AUSWEICHOFFSET, 0), New Point(0, dy), New Point(-AUSWEICHOFFSET, 0)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx + AUSWEICHOFFSET - dx \ 2, 0), New Point(0, AUSWEICHOFFSET), New Point(-AUSWEICHOFFSET, 0)}
                    End If
                End If
            Else
                If richtungEnde = Richtung.Xminus Then
                    If dy <> 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET * 2, 0), New Point(0, dy - dy \ 2), New Point(AUSWEICHOFFSET, 0)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET * 2, 0), New Point(0, -AUSWEICHOFFSET), New Point(AUSWEICHOFFSET, 0)}
                    End If
                ElseIf richtungEnde = Richtung.Yminus Then
                    If dy > 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, dy - dy \ 2)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, AUSWEICHOFFSET)}
                    End If
                ElseIf richtungEnde = Richtung.Yplus Then
                    If dy < 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, dy - dy \ 2)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, -AUSWEICHOFFSET)}
                    End If
                Else
                    If dy <> 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy), New Point(dx - AUSWEICHOFFSET, 0)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - dx \ 2 - AUSWEICHOFFSET, 0), New Point(0, -AUSWEICHOFFSET), New Point(dx \ 2, 0)}
                    End If
                End If
            End If

        Else
            'Normales Routing.

            If dx > 0 Then
                If richtungEnde = Richtung.Xminus Then
                    If dy = 0 Then
                        Return {New Point(dx, 0)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy), New Point(dx - dx \ 2, 0)}
                    End If
                ElseIf richtungEnde = Richtung.Yminus Then
                    If dy > 0 Then
                        Return {New Point(dx, 0), New Point(0, dy)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - dx \ 2, 0), New Point(0, AUSWEICHOFFSET)}
                    End If
                ElseIf richtungEnde = Richtung.Yplus Then
                    If dy < 0 Then
                        Return {New Point(dx, 0), New Point(0, dy)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - dx \ 2, 0), New Point(0, -AUSWEICHOFFSET)}
                    End If
                Else
                    If dy >= AUSWEICHOFFSET OrElse dy <= -AUSWEICHOFFSET OrElse (dx < AUSWEICHOFFSET AndAlso dy <> 0) Then
                        Return {New Point(dx + AUSWEICHOFFSET, 0), New Point(0, dy), New Point(-AUSWEICHOFFSET, 0)}
                    ElseIf dy >= 0 Then
                        Return {New Point(dx \ 2, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx + AUSWEICHOFFSET - dx \ 2, 0), New Point(0, AUSWEICHOFFSET), New Point(-AUSWEICHOFFSET, 0)}
                    Else
                        Return {New Point(dx \ 2, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx + AUSWEICHOFFSET - dx \ 2, 0), New Point(0, -AUSWEICHOFFSET), New Point(-AUSWEICHOFFSET, 0)}
                    End If
                End If
            Else
                If richtungEnde = Richtung.Xminus Then
                    If dy >= AUSWEICHOFFSET OrElse dy <= -AUSWEICHOFFSET Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET * 2, 0), New Point(0, dy - dy \ 2), New Point(AUSWEICHOFFSET, 0)}
                    ElseIf dy >= 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET * 2, 0), New Point(0, -AUSWEICHOFFSET), New Point(AUSWEICHOFFSET, 0)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET * 2, 0), New Point(0, AUSWEICHOFFSET), New Point(AUSWEICHOFFSET, 0)}
                    End If
                ElseIf richtungEnde = Richtung.Yminus Then
                    If dy >= AUSWEICHOFFSET Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, dy - dy \ 2)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, AUSWEICHOFFSET)}
                    End If
                ElseIf richtungEnde = Richtung.Yplus Then
                    If dy <= -AUSWEICHOFFSET Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy \ 2), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, dy - dy \ 2)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - AUSWEICHOFFSET, 0), New Point(0, -AUSWEICHOFFSET)}
                    End If
                Else
                    If dy >= AUSWEICHOFFSET OrElse dy <= -AUSWEICHOFFSET OrElse (-dx < AUSWEICHOFFSET AndAlso dy <> 0) Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy), New Point(dx - AUSWEICHOFFSET, 0)}
                    ElseIf dy >= 0 Then
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy + AUSWEICHOFFSET), New Point(dx - dx \ 2 - AUSWEICHOFFSET, 0), New Point(0, -AUSWEICHOFFSET), New Point(dx \ 2, 0)}
                    Else
                        Return {New Point(AUSWEICHOFFSET, 0), New Point(0, dy - AUSWEICHOFFSET), New Point(dx - dx \ 2 - AUSWEICHOFFSET, 0), New Point(0, AUSWEICHOFFSET), New Point(dx \ 2, 0)}
                    End If
                End If
            End If


        End If
    End Function

    Private Function bewerteRouting(routing() As Point, start As Snappoint, ende As Snappoint) As Tuple(Of Integer, Double)

        Dim erg As Integer = routing.Length
        If routing(0).X > 0 Then
            erg += start.Xplus
        ElseIf routing(0).X < 0 Then
            erg += start.Xminus
        ElseIf routing(0).Y > 0 Then
            erg += start.Yplus
        ElseIf routing(0).Y < 0 Then
            erg += start.Yminus
        End If

        If routing(routing.Length - 1).X < 0 Then
            erg += ende.Xplus
        ElseIf routing(routing.Length - 1).X > 0 Then
            erg += ende.Xminus
        ElseIf routing(routing.Length - 1).Y < 0 Then
            erg += ende.Yplus
        ElseIf routing(routing.Length - 1).Y > 0 Then
            erg += ende.Yminus
        End If

        For i As Integer = 0 To routing.Length - 1
            If Math.Abs(routing(i).X) > 0 AndAlso Math.Abs(routing(i).X) < GridX Then
                erg += 1
            ElseIf Math.Abs(routing(i).Y) > 0 AndAlso Math.Abs(routing(i).Y) < GridY Then
                erg += 1
            End If
            If routing(i).X <> 0 AndAlso routing(i).Y <> 0 Then
                erg += 100 'schräge wires sind sehr sehr schlecht!
            End If
        Next

        Dim erg_fein As Double
        For i As Integer = 0 To routing.Length - 2
            erg_fein += Math.Sqrt(Mathe.abstandQuadrat(routing(i), routing(i + 1)))
        Next

        Return New Tuple(Of Integer, Double)(erg, erg_fein)
    End Function
#End Region

#Region "Routing Verbessern"

    Public Sub RoutingVerbessern(live_update As Boolean, mitUndo As Boolean, maxmaxSchieben As Integer)
        Dim rück As RückgängigGrafik = Nothing
        If mitUndo Then
            rück = New RückgängigGrafik()
            rück.setText("Verdrahtung optimieren")
            rück.speicherVorherZustand(getRückArgs())
        End If

        Me.Cursor = Cursors.WaitCursor

        Dim connections() As List(Of Integer) = detectConnectedNets()

        Dim posBauteil As New List(Of Point)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Bauteil Then
                For k As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints - 1
                    posBauteil.Add(DirectCast(myElemente(i), Bauteil).getSnappoint(k).p)
                Next
            End If
        Next

        'Modus = 0 -> only x pos
        'Modus = 1 -> only y pos
        'Modus = 2 -> only x neg
        'Modus = 3 -> only y neg
        'Modus = 4 -> move On grid (einmal!)
        'Modus = 5 -> weiche Bauteilen aus!
        Dim modus As Integer = 5 'mit Modus 5 anfangen!
        Dim maxSchieben As Integer = 50
        Dim MoveOnGridAlternativ As Boolean = False
        Dim anzahl_nicht_geschoben As Integer = 0 'Zum Abbrechen!

        Dim hatSchonmalGeschoben As New Dictionary(Of WireVerschiebung, Integer)

        For iters As Integer = 0 To 1000
            Dim hat_geschoben As Boolean = False

            Dim schieben As Integer
            Dim schiebenX As Boolean
            Dim bestVal As Integer = Integer.MaxValue
            Dim bestIndex As Integer = -1

            Dim mode5_best_schieben As Integer
            Dim mode5_best_schiebenX As Boolean

            For i As Integer = 0 To myElemente.Count - 1
                If TypeOf myElemente(i) Is Wire Then
                    Dim start As Point = DirectCast(myElemente(i), Wire).getStart()
                    Dim ende As Point = DirectCast(myElemente(i), Wire).getEnde()

                    If modus = 5 Then
                        If connections(i).Count <> 1 Then
                            Throw New Exception("Falsche Anzahl an Netzen verbunden mit dem Wire!")
                        End If
                        If RoutingVerbessern_BauteilUmgehen(DirectCast(myElemente(i), Wire), connections(i)(0), connections, schieben, schiebenX) Then
                            Dim val As Integer = schieben + Math.Abs(DirectCast(myElemente(i), Wire).vector.X) + Math.Abs(DirectCast(myElemente(i), Wire).vector.Y)
                            'Prüfen, ob diese verschiebung schon mehrfach probiert wurde.
                            For k As Integer = 0 To hatSchonmalGeschoben.Count - 1
                                Dim w As WireVerschiebung = hatSchonmalGeschoben.Keys(k)
                                If (w.start = start AndAlso w.ende = ende AndAlso w.schieben = schieben) OrElse
                                   (w.start = ende AndAlso w.ende = start AndAlso w.schieben = schieben) Then
                                    val = Integer.MaxValue
                                    Exit For
                                End If
                            Next
                            If val < bestVal Then
                                bestVal = val
                                bestIndex = i
                                mode5_best_schieben = schieben
                                mode5_best_schiebenX = schiebenX
                            End If
                        End If
                    Else
                        If connections(i).Count <> 1 Then
                            Throw New Exception("Falsche Anzahl an Netzen verbunden mit dem Wire!")
                        End If
                        If RoutingVerbessern_Wire(DirectCast(myElemente(i), Wire), connections(i)(0), connections, posBauteil, schieben, schiebenX, True, modus = 4, MoveOnGridAlternativ) Then
                            Dim modeHat As Integer = -1
                            If Math.Abs(schieben) < maxSchieben Then
                                If schiebenX Then
                                    If schieben > 0 Then
                                        modeHat = 0
                                    ElseIf schieben < 0 Then
                                        modeHat = 2
                                    End If
                                Else
                                    If schieben > 0 Then
                                        modeHat = 1
                                    ElseIf schieben < 0 Then
                                        modeHat = 3
                                    End If
                                End If
                                If modeHat = modus OrElse modus = 4 Then
                                    Dim val As Integer = schieben + Math.Abs(DirectCast(myElemente(i), Wire).vector.X) + Math.Abs(DirectCast(myElemente(i), Wire).vector.Y)

                                    'Wenn vor dem verschieben on-grid und nach dem verschieben off-grid, dann ist es deutlich negativ...
                                    Dim vorher_offgrid As Boolean
                                    Dim nachher_offgrid As Boolean
                                    If schiebenX Then
                                        vorher_offgrid = isOffgridX(DirectCast(myElemente(i), Wire).getStart().X)
                                        nachher_offgrid = isOffgridX(DirectCast(myElemente(i), Wire).getStart().X + schieben)
                                    Else
                                        vorher_offgrid = isOffgridY(DirectCast(myElemente(i), Wire).getStart().Y)
                                        nachher_offgrid = isOffgridY(DirectCast(myElemente(i), Wire).getStart().Y + schieben)
                                    End If
                                    If nachher_offgrid AndAlso Not vorher_offgrid Then
                                        val = Integer.MaxValue
                                        'Nicht schieben, wenn vorher on-grid und jetzt off-grid!!!
                                        'Auch zum vermeiden von hin- und herschieben (deadlock)
                                    End If

                                    'Prüfen, ob diese verschiebung schon mehrfach probiert wurde.
                                    For k As Integer = 0 To hatSchonmalGeschoben.Count - 1
                                        Dim w As WireVerschiebung = hatSchonmalGeschoben.Keys(k)
                                        If (w.start = start AndAlso w.ende = ende AndAlso w.schieben = schieben) OrElse
                                           (w.start = ende AndAlso w.ende = start AndAlso w.schieben = schieben) Then
                                            val = Integer.MaxValue
                                            Exit For
                                        End If
                                    Next

                                    If val < bestVal Then
                                        bestVal = val
                                        bestIndex = i
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            If bestIndex <> -1 Then
                Dim start As Point = DirectCast(myElemente(bestIndex), Wire).getStart()
                Dim ende As Point = DirectCast(myElemente(bestIndex), Wire).getEnde()

                If modus = 5 Then
                    deselect_All()
                    DirectCast(myElemente(bestIndex), Wire).isSelected = True
                    If mode5_best_schiebenX Then
                        moveSelectedElements(mode5_best_schieben, 0)
                    Else
                        moveSelectedElements(0, mode5_best_schieben)
                    End If
                    deselect_All()
                    schieben = mode5_best_schieben
                    schiebenX = mode5_best_schiebenX
                Else
                    RoutingVerbessern_Wire(DirectCast(myElemente(bestIndex), Wire), connections(bestIndex)(0), connections, posBauteil, schieben, False, False, modus = 4, MoveOnGridAlternativ)
                End If

                Dim hatGefunden As Boolean = False
                For k As Integer = 0 To hatSchonmalGeschoben.Count - 1
                    Dim w As WireVerschiebung = hatSchonmalGeschoben.Keys(k)
                    If w.start = start AndAlso w.ende = ende AndAlso w.schieben = schieben Then
                        hatSchonmalGeschoben(w) += 1
                        hatGefunden = True
                        Exit For
                    ElseIf w.start = ende AndAlso w.ende = start AndAlso w.schieben = schieben Then
                        hatSchonmalGeschoben(w) += 1
                        hatGefunden = True
                        Exit For
                    End If
                Next
                If Not hatGefunden Then
                    hatSchonmalGeschoben.Add(New WireVerschiebung(start, ende, schieben), 1)
                End If

                hat_geschoben = True
                Debug.Print("Hat geschoben um: " & schieben)
            Else
                hat_geschoben = False
                Debug.Print("Hat nicht geschoben...")
            End If

            If live_update Then
                Me.Invalidate()
                Me.Update()
            End If

            If hat_geschoben Then
                removeWireStubs()
                connections = detectConnectedNets()
                anzahl_nicht_geschoben = 0
            Else
                If removeWireStubs() Then
                    connections = detectConnectedNets()
                End If
                anzahl_nicht_geschoben += 1
            End If

            If modus = 5 Then
                If Not hat_geschoben Then
                    MoveOnGridAlternativ = Not MoveOnGridAlternativ
                    modus = 0
                    If maxSchieben < 10000 AndAlso anzahl_nicht_geschoben > 4 Then
                        maxSchieben *= 2
                        maxSchieben = Math.Min(maxSchieben, maxmaxSchieben)
                    End If
                    Debug.Print("Einmal durch.")
                End If
            ElseIf modus = 4 Then
                modus = 5
            Else
                If Not hat_geschoben Then
                    modus = modus + 1
                End If
            End If

            If anzahl_nicht_geschoben > 10 AndAlso (maxSchieben > 10000 OrElse maxSchieben >= maxmaxSchieben) Then
                Debug.Print("Abbrechen, da anscheinend keine Optimierung mehr möglich! (Nach " & iters & "Iterationen)")
                Exit For
            End If
        Next

        If mitUndo Then
            rück.speicherNachherZustand(getRückArgs())
            If rück.istNotwendig() Then
                RaiseEvent NeuesRückgängigElement(Me, New NeuesRückgängigEventArgs(rück))
            End If
        End If

        Me.Cursor = Cursors.Default
    End Sub

    Private Function isOffgridX(x As Integer) As Boolean
        Dim onGrid As Integer = Me.fitToGridX(x) * GridX
        Return onGrid <> x
    End Function

    Private Function isOffgridY(y As Integer) As Boolean
        Dim onGrid As Integer = Me.fitToGridY(y) * GridY
        Return onGrid <> y
    End Function

    Private Function removeWireStubs() As Boolean
        Dim changed As Boolean = False
        For i As Integer = myElemente.Count - 1 To 0 Step -1
            If TypeOf myElemente(i) Is Wire Then
                With DirectCast(myElemente(i), Wire)
                    If Not hatMehrAls1SnappointAnPosition(.getStart()) OrElse
                       Not hatMehrAls1SnappointAnPosition(.getEnde()) Then
                        myElemente.RemoveAt(i)
                        changed = True
                    End If
                End With
            End If
        Next
        Return changed
    End Function

    Private Function hatMehrAls1SnappointAnPosition(p As Point) As Boolean
        Dim anzahl As Integer = 0
        For Each e In myElemente
            If TypeOf e Is Element Then
                For k As Integer = 0 To DirectCast(e, Element).NrOfSnappoints() - 1
                    If DirectCast(e, Element).getSnappoint(k).p = p Then
                        anzahl += 1
                        If anzahl > 1 Then
                            Return True
                        End If
                    End If
                Next
            End If
        Next
        Return False
    End Function

    Private Function RoutingVerbessern_BauteilUmgehen(w As Wire, wireConnect As Integer, connections() As List(Of Integer), ByRef anzahlSchieben As Integer, ByRef schiebenX As Boolean) As Boolean
        Dim start As Point = w.getStart()
        Dim ende As Point = w.getEnde()
        'prüfe ob überhaupt ein Bauteil geschnitten wird!
        Dim liste As New List(Of Rectangle)
        Dim startContactSnappoint As Boolean = False
        Dim endeContactSnappoint As Boolean = False
        If schneidet_Wire_ein_Bauteil(start, ende, liste, startContactSnappoint, endeContactSnappoint) Then
            Dim startInBauteil As Boolean = False
            Dim endeInBauteil As Boolean = False
            For i As Integer = 0 To liste.Count - 1
                If Mathe.isInRect(start, liste(i)) Then
                    startInBauteil = True
                End If
                If Mathe.isInRect(ende, liste(i)) Then
                    endeInBauteil = True
                End If
            Next
            Dim startIstWichtig As Boolean = Not (startInBauteil OrElse startContactSnappoint)
            Dim endeIstWichtig As Boolean = Not (endeInBauteil OrElse endeContactSnappoint)

            Dim rectUnion As Rectangle = liste(0)
            For i As Integer = 1 To liste.Count - 1
                rectUnion = Mathe.Union(rectUnion, liste(i))
            Next
            'rectUnion muss umgangen werden
            If start.X = ende.X Then
                'schieben in x-Richtung
                Dim schieben(1) As Integer
                schieben(0) = Integer.MaxValue
                schieben(1) = Integer.MaxValue
                For richtung As Integer = 0 To 1
                    Dim xneu As Integer
                    Dim xStep As Integer
                    If richtung = 0 Then
                        'versuche rechts das Bauteil zu umgehen!
                        xneu = Me.fitToGridX(rectUnion.X + rectUnion.Width) * GridX + 0
                        xStep = 100
                    Else
                        'versuche links das Bauteil zu umgehen!
                        xneu = Me.fitToGridX(rectUnion.X) * GridX - 0
                        xStep = -100
                    End If

                    For iters As Integer = 1 To 20
                        xneu = xneu + xStep
                        If Not schneidet_Wire_ein_Bauteil(New Point(xneu, start.Y), New Point(xneu, ende.Y), Nothing, False, False) AndAlso
                           (Not startIstWichtig OrElse Not schneidet_Wire_ein_Bauteil(start, New Point(xneu, start.Y), Nothing, False, False)) AndAlso
                           (Not endeIstWichtig OrElse Not schneidet_Wire_ein_Bauteil(ende, New Point(xneu, ende.Y), Nothing, False, False)) Then

                            If darfHierRouten(start, ende, New Point(xneu, start.Y), New Point(xneu, ende.Y), wireConnect, connections) Then
                                schieben(richtung) = xneu - start.X
                                Exit For
                            End If
                        End If
                    Next
                Next
                If Math.Abs(schieben(0)) <= Math.Abs(schieben(1)) AndAlso schieben(0) <> Integer.MaxValue Then
                    anzahlSchieben = schieben(0)
                    schiebenX = True
                    Return True
                ElseIf Math.Abs(schieben(1)) <= Math.Abs(schieben(0)) AndAlso schieben(1) <> Integer.MaxValue Then
                    anzahlSchieben = schieben(1)
                    schiebenX = True
                    Return True
                End If
            ElseIf start.Y = ende.Y Then
                'schieben in y-Richtung
                Dim schieben(1) As Integer
                schieben(0) = Integer.MaxValue
                schieben(1) = Integer.MaxValue
                For richtung As Integer = 0 To 1
                    Dim yneu As Integer
                    Dim yStep As Integer
                    If richtung = 0 Then
                        'versuche unten das Bauteil zu umgehen!
                        yneu = Me.fitToGridY(rectUnion.Y + rectUnion.Height) * GridY + 100
                        yStep = 100
                    Else
                        'versuche oben das Bauteil zu umgehen!
                        yneu = Me.fitToGridY(rectUnion.Y) * GridY - 100
                        yStep = -100
                    End If

                    For iters As Integer = 1 To 20
                        yneu = yneu + yStep
                        If Not schneidet_Wire_ein_Bauteil(New Point(start.X, yneu), New Point(ende.X, yneu), Nothing, False, False) AndAlso
                           (Not startIstWichtig OrElse Not schneidet_Wire_ein_Bauteil(start, New Point(start.X, yneu), Nothing, False, False)) AndAlso
                           (Not endeIstWichtig OrElse Not schneidet_Wire_ein_Bauteil(ende, New Point(ende.X, yneu), Nothing, False, False)) Then

                            If darfHierRouten(start, ende, New Point(start.X, yneu), New Point(ende.X, yneu), wireConnect, connections) Then
                                schieben(richtung) = yneu - start.Y
                                Exit For
                            End If
                        End If
                    Next
                Next
                If Math.Abs(schieben(0)) <= Math.Abs(schieben(1)) AndAlso schieben(0) <> Integer.MaxValue Then
                    anzahlSchieben = schieben(0)
                    schiebenX = False
                    Return True
                ElseIf Math.Abs(schieben(1)) <= Math.Abs(schieben(0)) AndAlso schieben(1) <> Integer.MaxValue Then
                    anzahlSchieben = schieben(1)
                    schiebenX = False
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Public Function schneidet_Wire_ein_Bauteil(start As Point, ende As Point, ByRef liste As List(Of Rectangle), ByRef startContactSnappoint As Boolean, ByRef endeContactSnappoint As Boolean) As Boolean
        If liste IsNot Nothing Then liste.Clear()
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Bauteil Then
                'Prüfen ob ein Bauteil im weg liegt!
                Dim selection As Selection = DirectCast(myElemente(i), Bauteil).getSelection()
                If TypeOf selection Is SelectionRect Then
                    With DirectCast(selection, SelectionRect)
                        If Mathe.schneidetRechteck(start, ende, .r, 0) Then
                            'prüfen ob nur am Rand ein Pin angesteuert wird!
                            Dim nurEinWireAnSnappoint As Boolean = False
                            If Mathe.liegtAufRand(start, ende, .r, 0) Then
                                'Wenn nur ein Pin gerade angesteuert wird dann ist es ok!
                                For k As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                                    If start = DirectCast(myElemente(i), Bauteil).getSnappoint(k).p Then
                                        If start.X = ende.X Then
                                            If ende.Y < start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yminus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                startContactSnappoint = True
                                                Exit For
                                            ElseIf ende.Y > start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yplus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                startContactSnappoint = True
                                                Exit For
                                            End If
                                        ElseIf start.Y = ende.Y Then
                                            If ende.X < start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xminus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                startContactSnappoint = True
                                                Exit For
                                            ElseIf ende.X > start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xplus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                startContactSnappoint = True
                                                Exit For
                                            End If
                                        End If
                                    End If
                                    If ende = DirectCast(myElemente(i), Bauteil).getSnappoint(k).p Then
                                        If start.X = ende.X Then
                                            If ende.Y < start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yplus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                endeContactSnappoint = True
                                                Exit For
                                            ElseIf ende.Y > start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yminus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                endeContactSnappoint = True
                                                Exit For
                                            End If
                                        ElseIf start.Y = ende.Y Then
                                            If ende.X < start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xplus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                endeContactSnappoint = True
                                                Exit For
                                            ElseIf ende.X > start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xminus <= TemplateAusDatei.PENALTY1 Then
                                                nurEinWireAnSnappoint = True
                                                endeContactSnappoint = True
                                                Exit For
                                            End If
                                        End If
                                    End If
                                Next
                            End If
                            If Not nurEinWireAnSnappoint Then
                                If liste Is Nothing Then Return True
                                liste.Add(.r)
                            End If
                        End If
                    End With
                End If
            End If
        Next
        If liste Is Nothing Then Return False
        If liste.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function RoutingVerbessern_Wire(w As Wire, wireConnect As Integer, connections() As List(Of Integer), posBauteil As List(Of Point), ByRef anzahlSchieben As Integer, ByRef schiebenX As Boolean, dontMove As Boolean, MoveOnGrid As Boolean, MoveOnGridAlternativ As Boolean) As Boolean
        'Alle Wires, die direkt an ein Bauteil angeschlossen sind, können nicht verschoben werden!
        'If posBauteil.Contains(w.getStart()) OrElse posBauteil.Contains(w.getEnde()) Then
        'Return False
        'End If

        Dim start As Point = w.getStart()
        Dim ende As Point = w.getEnde()

        If w.vector.X = 0 Then
            schiebenX = True

            'teste ob man in x-Richtung schieben kann!
            Dim schiebenStart As New List(Of Integer)
            Dim schiebenEnde As New List(Of Integer)
            Dim noSchiebenStart As Boolean = False
            Dim noSchiebenEnde As Boolean = False

            Dim Start_open As Boolean = True
            Dim Ende_open As Boolean = True

            For Each e In myElemente
                If Not e.Equals(w) AndAlso TypeOf e Is Wire Then

                    With DirectCast(e, Wire)
                        If .vector.Y = 0 Then
                            If start = .getStart() Then
                                schiebenStart.Add(.vector.X)
                                Start_open = False
                            ElseIf start = .getEnde() Then
                                schiebenStart.Add(- .vector.X)
                                Start_open = False
                            ElseIf ende = .getStart() Then
                                schiebenEnde.Add(.vector.X)
                                Ende_open = False
                            ElseIf ende = .getEnde() Then
                                schiebenEnde.Add(- .vector.X)
                                Ende_open = False
                            End If
                        ElseIf .vector.X = 0 Then
                            If start = .getStart() OrElse start = .getEnde() Then
                                noSchiebenStart = True
                                Start_open = False
                            ElseIf ende = .getStart() OrElse ende = .getEnde() Then
                                noSchiebenEnde = True
                                Ende_open = False
                            End If
                        End If
                    End With

                End If
            Next

            If Start_open OrElse Ende_open Then
                Return False
            End If

            'Wires, die inmitten einer geraden Linie liegen nicht verschieben. (-*---*-)
            If noSchiebenStart AndAlso noSchiebenEnde Then
                Return False
            End If

            Dim schiebenMöglich As New List(Of Integer)
            schiebenMöglich.AddRange(schiebenStart)
            schiebenMöglich.AddRange(schiebenEnde)
            schiebenMöglich.Sort(New Comparison(Of Integer)(AddressOf comparerAbsValue))

            For Each schieben As Integer In schiebenMöglich
                If schieben = Integer.MaxValue Then
                    Continue For
                End If
                If Not MoveOnGrid AndAlso darfHierRouten(start, ende, New Point(start.X + schieben, start.Y), New Point(ende.X + schieben, ende.Y), wireConnect, connections) Then
                    If schieben < Integer.MaxValue AndAlso Math.Abs(schieben) > 0 Then
                        anzahlSchieben = schieben
                        If Not dontMove Then
                            deselect_All()
                            w.isSelected = True
                            moveSelectedElements(schieben, 0)
                            deselect_All()
                        End If
                        Return True
                    End If
                ElseIf MoveOnGrid Then
                    'Sonst nur so viel schieben, dass es on grid liegt!
                    Dim onGrid = fitToGridX(w.getStart().X) * GridX
                    If onGrid <> w.getStart().X Then
                        'Muss auf grid schieben
                        schieben = onGrid - w.getStart().X
                        If MoveOnGridAlternativ Then
                            If Math.Abs(onGrid + GridX - w.getStart().X) < Math.Abs(onGrid - GridX - w.getStart().X) Then
                                schieben += GridX
                            Else
                                schieben -= GridX
                            End If
                        End If
                        If Math.Abs(schieben) > 0 AndAlso darfHierRouten(start, ende, New Point(start.X + schieben, start.Y), New Point(ende.X + schieben, ende.Y), wireConnect, connections) Then
                            anzahlSchieben = schieben
                            If Not dontMove Then
                                deselect_All()
                                w.isSelected = True
                                moveSelectedElements(schieben, 0)
                                deselect_All()
                            End If
                            Return True
                        End If
                    End If
                End If
            Next
            Return False
        Else
            schiebenX = False

            'teste ob man in y-Richtung schieben kann!
            Dim schiebenStart As New List(Of Integer)
            Dim schiebenEnde As New List(Of Integer)
            Dim noSchiebenStart As Boolean = False
            Dim noSchiebenEnde As Boolean = False

            Dim Start_open As Boolean = True
            Dim Ende_open As Boolean = True

            For Each e In myElemente
                If Not e.Equals(w) AndAlso TypeOf e Is Wire Then

                    With DirectCast(e, Wire)
                        If .vector.X = 0 Then
                            If start = .getStart() Then
                                schiebenStart.Add(.vector.Y)
                                Start_open = False
                            ElseIf start = .getEnde() Then
                                schiebenStart.Add(- .vector.Y)
                                Start_open = False
                            ElseIf ende = .getStart() Then
                                schiebenEnde.Add(.vector.Y)
                                Ende_open = False
                            ElseIf ende = .getEnde() Then
                                schiebenEnde.Add(- .vector.Y)
                                Ende_open = False
                            End If
                        ElseIf .vector.Y = 0 Then
                            If start = .getStart() OrElse start = .getEnde() Then
                                noSchiebenStart = True
                                Start_open = False
                            ElseIf ende = .getStart() OrElse ende = .getEnde() Then
                                noSchiebenEnde = True
                                Ende_open = False
                            End If
                        End If
                    End With

                End If
            Next

            If Start_open OrElse Ende_open Then
                Return False
            End If

            'Wires, die inmitten einer geraden Linie liegen nicht verschieben. (-*---*-)
            If noSchiebenStart AndAlso noSchiebenEnde Then
                Return False
            End If

            Dim schiebenMöglich As New List(Of Integer)
            schiebenMöglich.AddRange(schiebenStart)
            schiebenMöglich.AddRange(schiebenEnde)
            schiebenMöglich.Sort(New Comparison(Of Integer)(AddressOf comparerAbsValue))

            For Each schieben As Integer In schiebenMöglich
                If schieben = Integer.MaxValue Then
                    Continue For
                End If
                If Not MoveOnGrid AndAlso darfHierRouten(start, ende, New Point(start.X, start.Y + schieben), New Point(ende.X, ende.Y + schieben), wireConnect, connections) Then
                    If schieben < Integer.MaxValue AndAlso Math.Abs(schieben) > 0 Then
                        anzahlSchieben = schieben
                        If Not dontMove Then
                            deselect_All()
                            w.isSelected = True
                            moveSelectedElements(0, schieben)
                            deselect_All()
                        End If
                        Return True
                    End If
                ElseIf MoveOnGrid Then
                    'Sonst nur so viel schieben, dass es on grid liegt!
                    Dim onGrid = fitToGridY(w.getStart().Y) * GridY
                    If onGrid <> w.getStart().Y Then
                        'Muss auf grid schieben
                        schieben = onGrid - w.getStart().Y
                        If MoveOnGridAlternativ Then
                            If Math.Abs(onGrid + GridY - w.getStart().Y) < Math.Abs(onGrid - GridY - w.getStart().Y) Then
                                schieben += GridY
                            Else
                                schieben -= GridY
                            End If
                        End If
                        If Math.Abs(schieben) > 0 AndAlso darfHierRouten(start, ende, New Point(start.X, start.Y + schieben), New Point(ende.X, ende.Y + schieben), wireConnect, connections) Then
                            anzahlSchieben = schieben
                            If Not dontMove Then
                                deselect_All()
                                w.isSelected = True
                                moveSelectedElements(0, schieben)
                                deselect_All()
                            End If
                            Return True
                        End If
                    End If
                End If
            Next
            Return False
        End If
        Return False
    End Function

    Private Function comparerAbsValue(i1 As Integer, i2 As Integer) As Integer
        Return Math.Abs(i1).CompareTo(Math.Abs(i2))
    End Function

    Private Function darfHierRouten(startVorher As Point, endeVorher As Point, start As Point, ende As Point, wireNr As Integer, connections() As List(Of Integer)) As Boolean
        Dim EXTRA_SPACE As Integer = 0

        If Not darfHierRouten_wirdWireOderPinBerührt(startVorher, endeVorher, start, ende, wireNr, connections) Then
            Return False
        End If

        Dim hatWire_StartVorherStart As Boolean = hatWireHier(startVorher, start)
        Dim hatWire_EndeVorherEnde As Boolean = hatWireHier(endeVorher, ende)

        If Not hatWire_StartVorherStart AndAlso schneidet_Wire_ein_Bauteil(startVorher, start, Nothing, False, False) Then
            Return False
        End If
        If Not hatWire_EndeVorherEnde AndAlso schneidet_Wire_ein_Bauteil(endeVorher, ende, Nothing, False, False) Then
            Return False
        End If

        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Bauteil Then
                'Prüfen ob ein Bauteil im weg liegt!
                Dim selection As Selection = DirectCast(myElemente(i), Bauteil).getSelection()
                If TypeOf selection Is SelectionRect Then
                    With DirectCast(selection, SelectionRect)
                        If Mathe.schneidetRechteck(start, ende, .r, EXTRA_SPACE) Then
                            If Not Mathe.schneidetRechteck(startVorher, endeVorher, .r, EXTRA_SPACE) Then
                                Dim nurEinWireAnSnappoint As Boolean = False
                                If Mathe.liegtAufRand(start, ende, .r, EXTRA_SPACE) Then
                                    'Wenn nur ein Pin gerade angesteuert wird dann ist es ok!
                                    For k As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                                        If start = DirectCast(myElemente(i), Bauteil).getSnappoint(k).p Then
                                            If start.X = ende.X Then
                                                If ende.Y < start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yminus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                ElseIf ende.Y > start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yplus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                End If
                                            ElseIf start.Y = ende.Y Then
                                                If ende.X < start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xminus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                ElseIf ende.X > start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xplus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                        If ende = DirectCast(myElemente(i), Bauteil).getSnappoint(k).p Then
                                            If start.X = ende.X Then
                                                If ende.Y < start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yplus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                ElseIf ende.Y > start.Y AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Yminus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                End If
                                            ElseIf start.Y = ende.Y Then
                                                If ende.X < start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xplus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                ElseIf ende.X > start.X AndAlso DirectCast(myElemente(i), Bauteil).getSnappoint(k).Xminus = 0 Then
                                                    nurEinWireAnSnappoint = True
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                If Not nurEinWireAnSnappoint Then
                                    Return False
                                End If
                            End If
                        End If
                    End With
                End If
            End If
        Next
        Return True
    End Function

    Private Function darfHierRouten_wirdWireOderPinBerührt(startVorher As Point, endeVorher As Point, start As Point, ende As Point, wireNr As Integer, connections() As List(Of Integer)) As Boolean
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Wire AndAlso connections(i).Count = 1 AndAlso connections(i)(0) <> wireNr Then
                'Nur Wires, die nicht im gleichen Netz sind wie das zu verschiebende prüfen.
                If schneidenSichWires(DirectCast(myElemente(i), Wire), start, ende) Then
                    Return False
                End If
                If schneidenSichWires(DirectCast(myElemente(i), Wire), startVorher, start) Then
                    Return False
                End If
                If schneidenSichWires(DirectCast(myElemente(i), Wire), endeVorher, ende) Then
                    Return False
                End If
            ElseIf TypeOf myElemente(i) Is Bauteil Then
                'prüfen ob ein falscher Pin kontaktiert wird!
                For snap As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                    If connections(i)(snap) <> wireNr Then
                        'nur Pins beachten, die in einem anderen Netz sind!
                        Dim p As Point = DirectCast(myElemente(i), Bauteil).getSnappoint(snap).p
                        If liegtAufGeraderLinie(p, start, ende) Then
                            Return False
                        End If
                        If liegtAufGeraderLinie(p, startVorher, start) Then
                            Return False
                        End If
                        If liegtAufGeraderLinie(p, endeVorher, ende) Then
                            Return False
                        End If
                    End If
                Next
            End If
        Next
        Return True
    End Function

    Private Function hatWireHier(start As Point, ende As Point) As Boolean
        For i As Integer = 0 To ElementListe.Count - 1
            If TypeOf ElementListe(i) Is Wire Then
                Dim w As Wire = DirectCast(ElementListe(i), Wire)
                If liegenPunkteInnherhalbVonWire(w, start, ende) Then Return True
            End If
        Next
        Return False
    End Function

    Private Function schneidenSichWires(w As Wire, startTest As Point, endeTest As Point) As Boolean
        If startTest.X = endeTest.X AndAlso w.getStart().X = w.getEnde().X AndAlso w.getStart().X = startTest.X Then
            Dim minY1 As Integer = Math.Min(startTest.Y, endeTest.Y)
            Dim maxY1 As Integer = Math.Max(startTest.Y, endeTest.Y)
            Dim minY2 As Integer = Math.Min(w.getStart().Y, w.getEnde().Y)
            Dim maxY2 As Integer = Math.Max(w.getStart().Y, w.getEnde().Y)
            If Not (minY1 - 1 = minY2 AndAlso maxY1 + 1 = maxY2) Then
                If Not (maxY1 < minY2 OrElse minY1 > maxY2) Then
                    Return True
                End If
            End If
        ElseIf startTest.Y = endeTest.Y AndAlso w.getStart().Y = w.getEnde().Y AndAlso w.getStart().Y = startTest.Y Then
            Dim minX1 As Integer = Math.Min(startTest.X, endeTest.X)
            Dim maxX1 As Integer = Math.Max(startTest.X, endeTest.X)
            Dim minX2 As Integer = Math.Min(w.getStart().X, w.getEnde().X)
            Dim maxX2 As Integer = Math.Max(w.getStart().X, w.getEnde().X)
            If Not (minX1 - 1 = minX2 AndAlso maxX1 + 1 = maxX2) Then
                If Not (maxX1 < minX2 OrElse minX1 > maxX2) Then
                    Return True
                End If
            End If
        End If
        Return False
    End Function

    Private Function liegenPunkteInnherhalbVonWire(w As Wire, startTest As Point, endeTest As Point) As Boolean
        If startTest.X = endeTest.X AndAlso w.getStart().X = w.getEnde().X AndAlso w.getStart().X = startTest.X Then
            Dim minY1 As Integer = Math.Min(startTest.Y, endeTest.Y)
            Dim maxY1 As Integer = Math.Max(startTest.Y, endeTest.Y)
            Dim minY2 As Integer = Math.Min(w.getStart().Y, w.getEnde().Y)
            Dim maxY2 As Integer = Math.Max(w.getStart().Y, w.getEnde().Y)
            If minY1 >= minY2 AndAlso maxY1 <= maxY2 Then
                Return True
            End If
        ElseIf startTest.Y = endeTest.Y AndAlso w.getStart().Y = w.getEnde().Y AndAlso w.getStart().Y = startTest.Y Then
            Dim minX1 As Integer = Math.Min(startTest.X, endeTest.X)
            Dim maxX1 As Integer = Math.Max(startTest.X, endeTest.X)
            Dim minX2 As Integer = Math.Min(w.getStart().X, w.getEnde().X)
            Dim maxX2 As Integer = Math.Max(w.getStart().X, w.getEnde().X)
            If minX1 >= minX2 AndAlso maxX1 <= maxX2 Then
                Return True
            End If
        End If
        Return False
    End Function

    Private Function liegtAufGeraderLinie(p As Point, startTest As Point, endeTest As Point) As Boolean
        If startTest.X = endeTest.X Then
            If p.X = startTest.X AndAlso p.Y >= Math.Min(startTest.Y, endeTest.Y) AndAlso p.Y <= Math.Max(startTest.Y, endeTest.Y) Then
                Return True
            End If
        ElseIf startTest.Y = endeTest.Y Then
            If p.Y = startTest.Y AndAlso p.X >= Math.Min(startTest.X, endeTest.X) AndAlso p.X <= Math.Max(startTest.X, endeTest.X) Then
                Return True
            End If
        Else
            Throw New NotImplementedException()
        End If
        Return False
    End Function

    Private Sub _DEBUG_markiereNetsInVerschiedeneneFarben()
        Dim nets() As List(Of Integer) = detectConnectedNets()
        Dim rnd As New Random()

        Dim farbe(nets.Length - 1) As Farbe
        For i As Integer = 0 To farbe.Count - 1
            farbe(i) = New Farbe(255, CByte(rnd.Next(0, 256)), CByte(rnd.Next(0, 256)), CByte(rnd.Next(0, 256)))
        Next

        For i As Integer = 0 To nets.Count - 1
            If nets(i) IsNot Nothing AndAlso TypeOf myElemente(i) Is IWire Then
                DirectCast(myElemente(i), ElementLinestyle).linestyle = myLineStyles.Count
                myLineStyles.add(New LineStyle(farbe(nets(i)(0)), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 0.2, New DashStyle(0)))
            ElseIf nets(i) IsNot Nothing AndAlso TypeOf myElemente(i) Is Bauteil Then
                For s As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                    Dim p As Point = DirectCast(myElemente(i), Bauteil).getSnappoint(s).p
                    Me.addElement(New Element_Ellipse(Me.getNewID(), New Point(p.X - 50, p.Y - 50), 100, 100, myLineStyles.Count, 0))
                    myLineStyles.add(New LineStyle(farbe(nets(i)(s)), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 0.2, New DashStyle(0)))
                Next
            End If
        Next
        Me.Invalidate()
    End Sub

    Private Function detectConnectedNets() As List(Of Integer)()
        Dim erg(myElemente.Count - 1) As List(Of Integer)
        For i As Integer = 0 To erg.Length - 1
            erg(i) = Nothing
        Next
        Dim nummer As Integer = 0
        For i As Integer = 0 To myElemente.Count - 1
            If erg(i) Is Nothing Then
                If TypeOf myElemente(i) Is IWire Then
                    erg(i) = New List(Of Integer)(1)
                    erg(i).Add(nummer)
                    detectAllWiresConnectedTo(DirectCast(myElemente(i), IWire).getStart(), erg, nummer)
                    detectAllWiresConnectedTo(DirectCast(myElemente(i), IWire).getEnde(), erg, nummer)
                    nummer += 1
                End If
            End If
        Next

        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Bauteil Then
                With DirectCast(myElemente(i), Bauteil)
                    erg(i) = New List(Of Integer)(.NrOfSnappoints())
                    For s As Integer = 0 To .NrOfSnappoints() - 1
                        Dim p As Point = .getSnappoint(s).p
                        Dim hatWire As Boolean = False
                        For w As Integer = 0 To myElemente.Count - 1
                            If TypeOf myElemente(w) Is IWire Then
                                Dim wStart As Point = DirectCast(myElemente(w), IWire).getStart()
                                Dim wEnde As Point = DirectCast(myElemente(w), IWire).getEnde()
                                If (wStart.X = p.X AndAlso wStart.Y = p.Y) OrElse (wEnde.X = p.X AndAlso wEnde.Y = p.Y) Then
                                    erg(i).Add(erg(w)(0))
                                    hatWire = True
                                    Exit For
                                End If
                            End If
                        Next
                        If Not hatWire Then
                            erg(i).Add(nummer)
                            nummer += 1
                        End If
                    Next
                    If erg(i).Count <> .NrOfSnappoints Then
                        Throw New Exception("Falsche Anzahl an Pins!")
                    End If
                End With
            End If
        Next

        Return erg
    End Function

    Private Sub detectAllWiresConnectedTo(pos As Point, conn() As List(Of Integer), nummer As Integer)
        For i As Integer = 0 To myElemente.Count - 1
            If conn(i) Is Nothing Then
                If TypeOf myElemente(i) Is IWire Then
                    With DirectCast(myElemente(i), IWire)
                        If .getStart() = pos Then
                            conn(i) = New List(Of Integer)(1)
                            conn(i).Add(nummer)
                            detectAllWiresConnectedTo(DirectCast(myElemente(i), IWire).getEnde(), conn, nummer)
                        ElseIf .getEnde() = pos Then
                            conn(i) = New List(Of Integer)(1)
                            conn(i).Add(nummer)
                            detectAllWiresConnectedTo(DirectCast(myElemente(i), IWire).getStart(), conn, nummer)
                        End If
                    End With
                End If
            End If
        Next
    End Sub
#End Region

#Region "Reihenfolge"
    Public Sub Selected_Elemente_to_fore_back(to_background As Boolean)
        If Me.has_selection() Then
            Dim rück As New RückgängigGrafik()
            rück.speicherVorherZustand(Me.getRückArgs())
            If to_background Then
                rück.setText("In den Hintergrund")
            Else
                rück.setText("In den Vordergrund")
            End If

            Dim myNeueCircuitElemente As New List(Of ElementMaster)(myElemente.Count)
            For i As Integer = 0 To myElemente.Count - 1
                If myElemente(i).hasSelection = to_background Then
                    myNeueCircuitElemente.Add(myElemente(i))
                End If
            Next
            For i As Integer = 0 To myElemente.Count - 1
                If myElemente(i).hasSelection = Not to_background Then
                    myNeueCircuitElemente.Add(myElemente(i))
                End If
            Next
            Me.myElemente = myNeueCircuitElemente

            rück.speicherNachherZustand(Me.getRückArgs())
            If rück.istNotwendig() Then
                addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
            Me.Invalidate()
        End If

    End Sub

    Public Sub Selected_Elemente_one_level_fore_back(backwards As Boolean)
        If Me.has_selection() Then
            Dim rück As New RückgängigGrafik()
            rück.speicherVorherZustand(Me.getRückArgs())
            If backwards Then
                rück.setText("Eine Ebene nach Hinten")
            Else
                rück.setText("Eine Ebene nach Vorne")
            End If

            Dim myNeueCircuitElemente(myElemente.Count - 1) As ElementMaster
            'Zuerst die Markierten Elemente verschieben
            If backwards Then
                For i As Integer = 0 To myElemente.Count - 1
                    If myElemente(i).hasSelection Then
                        Dim iNeu As Integer = Math.Max(i - 1, 0)
                        If myNeueCircuitElemente(iNeu) Is Nothing Then
                            myNeueCircuitElemente(iNeu) = myElemente(i)
                        Else
                            'kann nicht nach hinten verschieben, da es schon ganz hinten ist!
                            myNeueCircuitElemente(i) = myElemente(i)
                        End If
                    End If
                Next
            Else
                For i As Integer = myElemente.Count - 1 To 0 Step -1
                    If myElemente(i).hasSelection Then
                        Dim iNeu As Integer = Math.Min(i + 1, myNeueCircuitElemente.Length - 1)
                        If myNeueCircuitElemente(iNeu) Is Nothing Then
                            myNeueCircuitElemente(iNeu) = myElemente(i)
                        Else
                            'kann nicht nach vorne verschieben, da es schon ganz vorne ist!
                            myNeueCircuitElemente(i) = myElemente(i)
                        End If
                    End If
                Next
            End If
            'Jetzt die nicht markierten elemente in der richtigen Reihenfolge in die Lücken auffüllen
            Dim nächsterFreierPlatz As Integer = 0
            For i As Integer = 0 To myElemente.Count - 1
                If Not myElemente(i).hasSelection Then
                    For j As Integer = nächsterFreierPlatz To myNeueCircuitElemente.Length - 1
                        If myNeueCircuitElemente(j) Is Nothing Then
                            myNeueCircuitElemente(j) = myElemente(i)
                            nächsterFreierPlatz = j + 1
                            Exit For
                        End If
                    Next
                End If
            Next
            For i As Integer = 0 To Me.myElemente.Count - 1
                Me.myElemente(i) = myNeueCircuitElemente(i)
            Next
            myNeueCircuitElemente = Nothing

            rück.speicherNachherZustand(Me.getRückArgs())
            If rück.istNotwendig() Then
                addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If
            Me.Invalidate()
        End If
    End Sub
#End Region

#Region "Gravity"
    Protected Sub OnGravityChanged()
        If myTools.Count > 0 Then
            myTools.Peek.OnGravityChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Public Function getGravityPointsX(notSelected As Boolean, selected As Boolean) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Element Then
                If (myElemente(i).hasSelection AndAlso selected) OrElse (Not myElemente(i).hasSelection AndAlso notSelected) Then
                    For s As Integer = 0 To DirectCast(myElemente(i), Element).NrOfSnappoints - 1
                        Dim p As Snappoint = DirectCast(myElemente(i), Element).getSnappoint(s)
                        erg.Add(New GravityPoint(p.p.X, p.p.Y, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
                    Next
                End If
            End If
        Next
        erg.Sort()
        Return erg
    End Function

    Public Function getGravityPointsY(notSelected As Boolean, selected As Boolean) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Element Then
                If (myElemente(i).hasSelection() AndAlso selected) OrElse (Not myElemente(i).hasSelection() AndAlso notSelected) Then
                    For s As Integer = 0 To DirectCast(myElemente(i), Element).NrOfSnappoints - 1
                        Dim p As Snappoint = DirectCast(myElemente(i), Element).getSnappoint(s)
                        erg.Add(New GravityPoint(p.p.Y, p.p.X, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
                    Next
                End If
            End If
        Next
        erg.Sort()
        Return erg
    End Function

    Public Function getGravityPointsX_Template(elemente As List(Of ElementMaster)) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)
        For i As Integer = 0 To elemente.Count - 1
            If TypeOf elemente(i) Is Element Then
                For s As Integer = 0 To DirectCast(elemente(i), Element).NrOfSnappoints - 1
                    Dim p As Snappoint = DirectCast(elemente(i), Element).getSnappoint(s)
                    erg.Add(New GravityPoint(p.p.X, p.p.Y, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
                Next
            End If
        Next
        erg.Sort()
        Return erg
    End Function

    Public Function getGravityPointsY_Template(elemente As List(Of ElementMaster)) As List(Of GravityPoint)
        Dim erg As New List(Of GravityPoint)
        For i As Integer = 0 To elemente.Count - 1
            If TypeOf elemente(i) Is Element Then
                For s As Integer = 0 To DirectCast(elemente(i), Element).NrOfSnappoints - 1
                    Dim p As Snappoint = DirectCast(elemente(i), Element).getSnappoint(s)
                    erg.Add(New GravityPoint(p.p.Y, p.p.X, p.Xplus, p.Xminus, p.Yplus, p.Yminus))
                Next
            End If
        Next
        erg.Sort()
        Return erg
    End Function

    Public Function FindGravity(gravityPoints As List(Of GravityPoint), refpoint As GravityPoint, minScoreVal As Integer, minMalusVal As Integer, malusFaktor As Integer) As GravityResult
        Dim minDelta As Integer = minScoreVal
        Dim minDeltaIndex As Integer = -1
        Dim minMalus As Integer = minMalusVal

        Dim posMin As GravityPoint = Nothing
        Dim posMax As GravityPoint = Nothing
        'erstmal grob suchen (die liste ist sortiert), sodass man nicht die ganze liste durchsuchen muss
        Dim firstTrefferIndex As Integer = -1
        Dim lastTefferIndx As Integer = -1
        For i As Integer = 0 To gravityPoints.Count - 1 Step 8
            Dim delta As Integer = Math.Abs(gravityPoints(i).posSnap - refpoint.posSnap) * 2 + 1
            If delta < minDelta Then
                If firstTrefferIndex = -1 Then firstTrefferIndex = i
                If i > lastTefferIndx Then lastTefferIndx = i
            End If
        Next
        firstTrefferIndex = Math.Max(0, firstTrefferIndex - 8)
        If lastTefferIndx = -1 Then
            lastTefferIndx = gravityPoints.Count - 1
        Else
            lastTefferIndx = Math.Min(gravityPoints.Count - 1, lastTefferIndx + 8)
        End If

        For i As Integer = firstTrefferIndex To lastTefferIndx
            Dim delta As Integer = Math.Abs(gravityPoints(i).posSnap - refpoint.posSnap) * 2
            If gravityPoints(i).posSnap - refpoint.posSnap < 0 Then delta += 1

            Dim malus As Integer = refpoint.calcMalus(gravityPoints(i), malusFaktor)

            If delta = minDelta AndAlso posMin IsNot Nothing Then
                'Zusammenfügen. Die angezeigte Linie sollte maximal lang sein!
                If gravityPoints(i).posAndereKoordinate < posMin.posAndereKoordinate Then
                    posMin = gravityPoints(i)
                End If
                If gravityPoints(i).posAndereKoordinate > posMax.posAndereKoordinate Then
                    posMax = gravityPoints(i)
                End If
                minMalus = Math.Min(minMalus, malus)
            ElseIf delta + malus <= minDelta + minMalus Then
                minDelta = delta
                minDeltaIndex = i
                posMin = gravityPoints(i)
                posMax = gravityPoints(i)
                minMalus = malus
            End If
        Next

        If minDeltaIndex <> -1 Then
            Return New GravityResult(minDelta, minMalus, gravityPoints(minDeltaIndex).posSnap - refpoint.posSnap, refpoint, posMin, posMax)
        End If
        Return Nothing
    End Function

    Public Function performGravity(keinX As Boolean, keinY As Boolean, posOnGrid As Point, pos As Point, lastGravityPointsTemplateX As List(Of GravityPoint), lastGravityPointsTemplateY As List(Of GravityPoint), lastGravityPointsX As List(Of GravityPoint), lastGravityPointsY As List(Of GravityPoint)) As GravityDrawState
        Dim minScore As Integer = CInt(Math.Min(gravityStärke / faktor, 1000.0) + 1) * 2
        Dim malusFaktor As Integer = CInt(1 + CLng(minScore) * 80 \ 100)
        Dim minMalus As Integer = 0

        Dim resultX As New List(Of GravityResult)
        If keinX Then
            minScore = 0
        End If
        If Not keinX OrElse posOnGrid.X = pos.X Then
            For i As Integer = 0 To lastGravityPointsTemplateX.Count - 1
                Dim grav As GravityPoint = lastGravityPointsTemplateX(i).copy()
                grav.posSnap += pos.X
                grav.posAndereKoordinate += pos.Y
                Dim result As GravityResult = FindGravity(lastGravityPointsX, grav, minScore, minMalus, malusFaktor)

                If result IsNot Nothing AndAlso result.score = minScore Then
                    resultX.Add(result)
                    minMalus = Math.Min(minMalus, result.malus)
                ElseIf result IsNot Nothing AndAlso result.score + result.malus < minScore + minMalus Then
                    resultX.Clear()
                    resultX.Add(result)
                    minScore = result.score
                    minMalus = result.malus
                End If
            Next
        End If

        minScore = CInt(Math.Min(gravityStärke / faktor, 1000.0) + 1) * 2
        minMalus = 0
        Dim resultY As New List(Of GravityResult)
        If keinY Then
            minScore = 0
        End If
        If Not keinY OrElse posOnGrid.Y = pos.Y Then
            For i As Integer = 0 To lastGravityPointsTemplateY.Count - 1
                Dim grav As GravityPoint = lastGravityPointsTemplateY(i).copy()
                grav.posSnap += pos.Y
                grav.posAndereKoordinate += pos.X
                Dim result As GravityResult = FindGravity(lastGravityPointsY, grav, minScore, minMalus, malusFaktor)

                If result IsNot Nothing AndAlso result.score = minScore Then
                    resultY.Add(result)
                    minMalus = Math.Min(minMalus, result.malus)
                ElseIf result IsNot Nothing AndAlso result.score + result.malus < minScore + minMalus Then
                    resultY.Clear()
                    resultY.Add(result)
                    minScore = result.score
                    minMalus = result.malus
                End If
            Next
        End If
        Dim deltaX As Integer = 0
        Dim deltaY As Integer = 0
        If resultX.Count > 0 Then
            deltaX = resultX(0).delta
            pos.X += deltaX
        Else
            pos.X = posOnGrid.X
        End If
        If resultY.Count > 0 Then
            deltaY = resultY(0).delta
            pos.Y += deltaY
        Else
            pos.Y = posOnGrid.Y
        End If

        Return New GravityDrawState(resultX, resultY, deltaX, deltaY, pos)
    End Function
#End Region

#Region "Gruppe"
    Public Function kannGruppieren() As Boolean
        Dim cnt As Integer = 0
        For Each element As ElementMaster In myElemente
            If element.hasSelection() Then
                cnt += 1
                If cnt >= 2 Then Return True
            End If
        Next
        Return False
    End Function

    Public Sub gruppeErstellen()
        Dim cnt As Integer = 0
        For Each element As ElementMaster In myElemente
            If element.hasSelection() Then
                cnt += 1
            End If
        Next
        If cnt = 0 Then
            MessageBox.Show("Es müssen Elemente markiert sein, um eine Gruppe erstellen zu können!")
        ElseIf cnt = 1 Then
            MessageBox.Show("Es müssen mindestens zwei Elemente ausgewählt sein, um daraus eine Gruppe zu erstellen!")
        Else

            Dim rück As New RückgängigGrafik()
            rück.setText("Gruppe erstellt")
            rück.speicherVorherZustand(getRückArgs())

            Dim gruppe As New ElementGruppe(getNewID())
            For i As Integer = 0 To myElemente.Count - 1
                If myElemente(i).hasSelection() Then
                    gruppe.addElement(myElemente(i))
                End If
            Next
            Dim lastElement As Boolean = True
            For i As Integer = myElemente.Count - 1 To 0 Step -1
                If myElemente(i).hasSelection() Then
                    If lastElement Then
                        myElemente(i) = gruppe
                        lastElement = False
                    Else
                        myElemente.RemoveAt(i)
                    End If
                End If
            Next
            gruppe.isSelected = True

            rück.speicherNachherZustand(getRückArgs())
            addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

            Me.Invalidate()
        End If
    End Sub

    Public Function kannGruppeAuflösen() As Boolean
        For i As Integer = 0 To myElemente.Count - 1
            If myElemente(i).hasSelection() Then
                If TypeOf myElemente(i) Is ElementGruppe Then
                    Return True
                End If
            End If
        Next
        Return False
    End Function

    Public Sub gruppeAuflösen()
        Dim hatChanges As Boolean = False

        Dim rück As New RückgängigGrafik()
        rück.setText("Gruppe aufgelöst")
        rück.speicherVorherZustand(getRückArgs())

        For i As Integer = myElemente.Count - 1 To 0 Step -1
            If myElemente(i).hasSelection() Then
                If TypeOf myElemente(i) Is ElementGruppe Then
                    Dim neueElemente As List(Of ElementMaster) = DirectCast(myElemente(i), ElementGruppe).getElemente()
                    Dim dx As Integer = DirectCast(myElemente(i), ElementGruppe).position.X
                    Dim dy As Integer = DirectCast(myElemente(i), ElementGruppe).position.Y
                    For Each e As ElementMaster In neueElemente
                        If TypeOf e Is Element Then
                            DirectCast(e, Element).position = New Point(DirectCast(e, Element).position.X + dx, DirectCast(e, Element).position.Y + dy)
                        ElseIf TypeOf e Is SnapableElement Then
                            For s As Integer = 0 To DirectCast(e, SnapableElement).getNrOfSnappoints() - 1
                                DirectCast(e, SnapableElement).getSnappoint(s).move(dx, dy)
                            Next
                        End If
                    Next
                    myElemente.RemoveAt(i)
                    myElemente.InsertRange(i, neueElemente)
                    hatChanges = True
                End If
            End If
        Next

        If hatChanges Then
            rück.speicherNachherZustand(getRückArgs())
            addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))

            Me.Invalidate()
        Else
            MessageBox.Show("Es muss eine Gruppe ausgewählt sein, damit diese aufgelöst werden kann!")
        End If
    End Sub

#End Region

    Protected Sub OnMultiSelectChanged()
        If myTools.Count > 0 Then
            myTools.Peek.OnMultiSelectChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Public Sub kreuzungAuflösen(pos As Point)
        Dim wiresAnKreuzung As New List(Of Wire)
        For Each e In myElemente
            If TypeOf e Is Wire Then
                With DirectCast(e, Wire)
                    If .getStart() = pos Then
                        wiresAnKreuzung.Add(DirectCast(e, Wire))
                    End If
                    If .getEnde() = pos Then
                        wiresAnKreuzung.Add(DirectCast(e, Wire))
                    End If
                End With
            End If
        Next

        Dim ecken As New Dictionary(Of Point, Integer)
        ecken.Add(pos, 2)
        For i As Integer = wiresAnKreuzung.Count - 1 To 0 Step -1
            For j As Integer = i - 1 To 0 Step -1
                If wiresAnKreuzung(j).AddIfPossible(wiresAnKreuzung(i), ecken) Then
                    myElemente.Remove(wiresAnKreuzung(i))
                    Exit For
                End If
            Next
        Next

    End Sub

    Public Function hatWireAnPosition(start As Point, ende As Point, ignoreStartpunkt As Boolean, ignoreEndpunkt As Boolean) As Boolean
        If start.X = ende.X Then
            If ignoreStartpunkt Then
                If start.Y < ende.Y Then
                    start.Y += 1
                ElseIf start.Y > ende.Y Then
                    start.Y -= 1
                End If
            End If
            If ignoreEndpunkt Then
                If ende.Y < start.Y Then
                    ende.Y += 1
                ElseIf ende.Y > start.Y Then
                    ende.Y -= 1
                End If
            End If
        ElseIf start.Y = ende.Y Then
            If ignoreStartpunkt Then
                If start.X < ende.X Then
                    start.X += 1
                ElseIf start.X > ende.X Then
                    start.X -= 1
                End If
            End If
            If ignoreEndpunkt Then
                If ende.X < start.X Then
                    ende.X += 1
                ElseIf ende.X > start.X Then
                    ende.X -= 1
                End If
            End If
        End If

        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Wire Then
                With DirectCast(myElemente(i), Wire)
                    If start.X = ende.X AndAlso .getStart().X = .getEnde().X AndAlso .getStart().X = start.X Then
                        Dim minY1 As Integer = Math.Min(start.Y, ende.Y)
                        Dim maxY1 As Integer = Math.Max(start.Y, ende.Y)
                        Dim minY2 As Integer = Math.Min(.getStart().Y, .getEnde().Y)
                        Dim maxY2 As Integer = Math.Max(.getStart().Y, .getEnde().Y)
                        If Not (ignoreStartpunkt AndAlso ignoreEndpunkt AndAlso minY1 - 1 = minY2 AndAlso maxY1 + 1 = maxY2) Then
                            'Wenn Start und Ende beides toleriert werden darf, dann kann auch ein Wire, welches genau vom Start zum Ende geht toleriert werden!
                            If Not (maxY1 < minY2 OrElse minY1 > maxY2) Then
                                Return True
                            End If
                        Else
                            Dim a As Integer = 2
                        End If
                    ElseIf start.Y = ende.Y AndAlso .getStart().Y = .getEnde().Y AndAlso .getStart().Y = start.Y Then
                        Dim minX1 As Integer = Math.Min(start.X, ende.X)
                        Dim maxX1 As Integer = Math.Max(start.X, ende.X)
                        Dim minX2 As Integer = Math.Min(.getStart().X, .getEnde().X)
                        Dim maxX2 As Integer = Math.Max(.getStart().X, .getEnde().X)
                        If Not (ignoreStartpunkt AndAlso ignoreEndpunkt AndAlso minX1 - 1 = minX2 AndAlso maxX1 + 1 = maxX2) Then
                            'Wenn Start und Ende beides toleriert werden darf, dann kann auch ein Wire, welches genau vom Start zum Ende geht toleriert werden!
                            If Not (maxX1 < minX2 OrElse minX1 > maxX2) Then
                                Return True
                            End If
                        End If
                    End If
                End With
            End If
        Next
        Return False
    End Function

    Public Function getNextPosOnWire(pos As Point) As WireSnappoint
        Dim minAbstand As Double = Double.MaxValue
        Dim minElement As Element = Nothing
        Dim abstand As Double
        For Each e As ElementMaster In myElemente
            If TypeOf e Is IWire Then
                abstand = DirectCast(e, Element).getSelection().distanceToBoundsWithoutClip(pos, Me.faktor)
                If abstand < minAbstand Then
                    minAbstand = abstand
                    minElement = DirectCast(e, Element)
                End If
            End If
        Next
        If minElement IsNot Nothing Then
            If TypeOf minElement Is Wire Then
                Return New WireSnappoint(pos, DirectCast(minElement, Wire))
            ElseIf TypeOf minElement Is WireLuftlinie Then
                Return New WireSnappoint(pos, DirectCast(minElement, WireLuftlinie))
            Else
                Return Nothing
            End If
        Else
            Return Nothing
        End If
    End Function

    Public Function getNextScaleKante(pos As Point) As ScaleKante
        Dim kante As Kante
        Dim dist As Double
        Dim minDist As Double = Double.MaxValue
        Dim minKante As ScaleKante = Nothing

        Dim has_selection As Boolean = Me.has_selection

        For Each element As ElementMaster In myElemente
            If TypeOf element Is Element AndAlso (Not has_selection OrElse element.hasSelection()) Then
                For k As Integer = 0 To DirectCast(element, Element).getScaleKantenCount() - 1
                    kante = DirectCast(element, Element).getScaleKante(k, Nothing)
                    If kante.isOnlyStartpunkt Then
                        'Punkte werden bevorzugt, damit z.B. Eckpunkte in der Ecke eher genommen werden als direkt die Kante!
                        dist = 0.5 * Math.Sqrt(Mathe.abstandQuadrat(pos, kante.start))
                    Else
                        dist = Mathe.calcDist(kante, pos)
                    End If
                    If dist < minDist Then
                        minDist = dist
                        minKante = New ScaleKanteBasicGrafikElement(kante)
                    End If
                Next
                If TypeOf element Is Wire Then
                    dist = Math.Sqrt(Mathe.abstandQuadrat(DirectCast(element, Wire).getStart(), pos))
                    If dist < minDist Then
                        minDist = dist
                        minKante = New ScaleKante_Wire(DirectCast(element, Wire).getStart())
                    End If
                    dist = Math.Sqrt(Mathe.abstandQuadrat(DirectCast(element, Wire).getEnde(), pos))
                    If dist < minDist Then
                        minDist = dist
                        minKante = New ScaleKante_Wire(DirectCast(element, Wire).getEnde())
                    End If
                ElseIf TypeOf element Is WireLuftlinie Then
                    dist = Math.Sqrt(Mathe.abstandQuadrat(DirectCast(element, WireLuftlinie).getStart(), pos))
                    If dist < minDist Then
                        minDist = dist
                        minKante = New ScaleKante_Wire(DirectCast(element, WireLuftlinie).getStart())
                    End If
                    dist = Math.Sqrt(Mathe.abstandQuadrat(DirectCast(element, WireLuftlinie).getEnde(), pos))
                    If dist < minDist Then
                        minDist = dist
                        minKante = New ScaleKante_Wire(DirectCast(element, WireLuftlinie).getEnde())
                    End If
                End If
            End If
        Next

        If minDist < Double.MaxValue Then
            If Not My.Computer.Keyboard.CtrlKeyDown Then
                If TypeOf minKante Is ScaleKanteBasicGrafikElement Then
                    Dim k1 As Kante = DirectCast(minKante, ScaleKanteBasicGrafikElement).mykanten(0)
                    If TypeOf k1.sender Is Basic_Linie AndAlso k1.isOnlyStartpunkt Then
                        'Weitere Linien, die den gleichen Start / Endpunkt haben zusammen verschieben, sodass verbundene Linien verbunden bleiben!
                        For Each element As ElementMaster In myElemente
                            If TypeOf element Is Basic_Linie AndAlso Not element.Equals(k1.sender) AndAlso (Not has_selection OrElse element.hasSelection) Then
                                For k As Integer = 0 To DirectCast(element, Basic_Linie).getScaleKantenCount() - 1
                                    kante = DirectCast(element, Basic_Linie).getScaleKante(k, Nothing)
                                    If kante.isOnlyStartpunkt Then
                                        If kante.start = k1.start Then
                                            DirectCast(minKante, ScaleKanteBasicGrafikElement).mykanten.Add(kante)
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    ElseIf TypeOf k1.sender Is Basic_Bezier AndAlso k1.isOnlyStartpunkt AndAlso k1.KantenIndex <= 1 Then
                        'Bezier start oder endpunkt!
                        'Weitere Bezierkurven, die den gleichen Start / Endpunkt haben zusammen verschieben, sodass verbundene Kurven verbunden bleiben!
                        For Each element As ElementMaster In myElemente
                            If TypeOf element Is Basic_Bezier AndAlso Not element.Equals(k1.sender) AndAlso (Not has_selection OrElse element.hasSelection) Then
                                For k As Integer = 0 To DirectCast(element, Basic_Bezier).getScaleKantenCount() - 1
                                    kante = DirectCast(element, Basic_Bezier).getScaleKante(k, Nothing)
                                    If kante.isOnlyStartpunkt AndAlso kante.KantenIndex <= 1 Then
                                        If kante.start = k1.start Then
                                            DirectCast(minKante, ScaleKanteBasicGrafikElement).mykanten.Add(kante)
                                        End If
                                    End If
                                Next
                            End If
                        Next
                    ElseIf TypeOf k1.sender Is Basic_Bezier AndAlso k1.isOnlyStartpunkt AndAlso (k1.KantenIndex = 2 OrElse k1.KantenIndex = 3) Then
                        'Bezier kontrollpunkt!
                        Dim passenderEndpunkt As Point
                        If k1.KantenIndex = 2 Then 'p2
                            passenderEndpunkt = DirectCast(k1.sender, Basic_Bezier).getStart()
                        ElseIf k1.KantenIndex = 3 Then 'p3
                            passenderEndpunkt = DirectCast(k1.sender, Basic_Bezier).getEnde()
                        End If
                        For Each element As ElementMaster In myElemente
                            If TypeOf element Is Basic_Bezier AndAlso Not element.Equals(k1.sender) AndAlso (Not has_selection OrElse element.hasSelection) Then
                                If passenderEndpunkt = DirectCast(element, Basic_Bezier).getStart() Then
                                    kante = DirectCast(element, Basic_Bezier).getScaleKante(2, Nothing)
                                    kante.gespiegeltXY = True
                                    DirectCast(minKante, ScaleKanteBasicGrafikElement).mykanten.Add(kante)
                                ElseIf passenderEndpunkt = DirectCast(element, Basic_Bezier).getEnde() Then
                                    kante = DirectCast(element, Basic_Bezier).getScaleKante(3, Nothing)
                                    kante.gespiegeltXY = True
                                    DirectCast(minKante, ScaleKanteBasicGrafikElement).mykanten.Add(kante)
                                End If
                            End If
                        Next
                    End If
                End If
            End If
            Return minKante
        End If
        Return Nothing
    End Function

    Public Function isElementInRect(element As Element, r As Rectangle) As Boolean
        If darfSelektieren(element) Then
            Dim selection As Selection = element.getSelection()
            Return selection.isInRect(r)
        Else
            Return False
        End If
    End Function

    Public Function isElementInRect(element As SnapableElement, index As Integer, r As Rectangle) As Boolean
        If darfSelektieren(element) Then
            Return element.getSnappoint(index).getSelection().isInRect(r)
        Else
            Return False
        End If
    End Function

    Public Sub setToolHilfeText(sender As Tool, text As String)
        If myTools.Count > 0 AndAlso myTools.Peek().Equals(sender) Then
            RaiseEvent ToolInfoTextChanged(Me, New ToolInfoTextEventArgs(text))
        End If
    End Sub

    Public Function fitToGridX(x As Integer) As Integer
        If x >= 0 Then
            Return (x + GridX \ 2) \ GridX
        Else
            Return (x - GridX \ 2) \ GridX
        End If
    End Function

    Public Function fitToGridY(y As Integer) As Integer
        If y >= 0 Then
            Return (y + GridY \ 2) \ GridY
        Else
            Return (y - GridY \ 2) \ GridY
        End If
    End Function

    Public Function fitPointToGrid(p As Point) As Point
        Return New Point(GridX * fitToGridX(p.X), GridY * fitToGridY(p.Y))
    End Function

    Public Sub schalteBeschriftungsPosDurch()
        Dim changed As Boolean = False
        Dim rück As New RückgängigGrafik()
        rück.setText("Beschriftungsposition geändert")
        rück.speicherVorherZustand(getRückArgs())
        For Each e In myElemente
            If e.hasSelection() AndAlso TypeOf e Is BauteilAusDatei Then
                If DirectCast(e, BauteilAusDatei).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            ElseIf e.hasSelection() AndAlso TypeOf e Is Basic_Spannungspfeil Then
                If DirectCast(e, Basic_Spannungspfeil).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            ElseIf TypeOf e Is SnapableCurrentArrow AndAlso e.hasSelection Then
                If DirectCast(e, SnapableCurrentArrow).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            ElseIf TypeOf e Is SnapableLabel AndAlso e.hasSelection Then
                If DirectCast(e, SnapableLabel).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            ElseIf TypeOf e Is SnapableBusTap AndAlso e.hasSelection Then
                If DirectCast(e, SnapableBusTap).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            ElseIf TypeOf e Is SnapableImpedanceArrow AndAlso e.hasSelection Then
                If DirectCast(e, SnapableImpedanceArrow).schalteBeschriftungsPosDurch() Then
                    changed = True
                End If
            End If
        Next
        If changed Then
            rück.speicherNachherZustand(getRückArgs())
            addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            Me.Invalidate()
        End If
    End Sub

#Region "Kopieren, Einfügen"
    Public Sub inZwischenablageKopieren(bib As Bibliothek)
        If Me.has_selection() Then
            Try
                Dim data As CopyClipboard = New CopyClipboard(Me.myElemente, Me.myLineStyles, Me.myFillStyles, Me.myFonts)
                data = data.copy()
                Dim pos0 As Point = Me.getDrehpunkt(False)
                data.transform(-pos0.X, -pos0.Y)
                My.Computer.Clipboard.SetData("cir_sch_copy", data.getByteArray(bib))
            Catch ex As Exception
                MessageBox.Show("Fehler beim Kopieren: " & ex.Message)
            End Try
        End If
    End Sub

    Public Function ausZwischenablageEinfügen(bib As Bibliothek) As Boolean
        Dim hatGeladen As Boolean = False
        If My.Computer.Clipboard.ContainsData("cir_sch_copy") Then
            Dim myRückArgs_o As Object = My.Computer.Clipboard.GetData("cir_sch_copy")
            If TypeOf myRückArgs_o Is Byte() Then
                Try
                    Dim bytes() As Byte = DirectCast(myRückArgs_o, Byte())
                    Dim myRückArgs As CopyClipboard = CopyClipboard.Einlesen(Me, bib, bytes)
                    If myRückArgs IsNot Nothing Then
                        Dim c1 As New ToolCopy()
                        Me.startTool(c1)
                        c1.set_copy_elemente(Me, myRückArgs)
                        hatGeladen = True
                    End If
                Catch ex As Exception
                    MessageBox.Show("Fehler beim Einfügen: " & ex.Message)
                End Try
            End If
        End If
        Return hatGeladen
    End Function
#End Region

#Region "Text anpassen"
    Public Sub beschriftungNichtKursiv()
        Dim rück As New RückgängigGrafik()
        rück.speicherVorherZustand(Me.getRückArgs())
        rück.setText("Beschriftung nicht Kursiv")

        For Each element As ElementMaster In myElemente
            If element.hasSelection() Then
                If TypeOf element Is BauteilAusDatei Then
                    DirectCast(element, BauteilAusDatei).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, BauteilAusDatei).getBeschriftung_Text()))
                ElseIf TypeOf element Is Basic_Spannungspfeil Then
                    DirectCast(element, Basic_Spannungspfeil).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, Basic_Spannungspfeil).getBeschriftung_Text()))
                ElseIf TypeOf element Is SnapableCurrentArrow Then
                    DirectCast(element, SnapableCurrentArrow).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, SnapableCurrentArrow).getBeschriftung_Text()))
                ElseIf TypeOf element Is SnapableBusTap Then
                    DirectCast(element, SnapableBusTap).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, SnapableBusTap).getBeschriftung_Text()))
                ElseIf TypeOf element Is SnapableLabel Then
                    DirectCast(element, SnapableLabel).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, SnapableLabel).getBeschriftung_Text()))
                ElseIf TypeOf element Is SnapableImpedanceArrow Then
                    DirectCast(element, SnapableImpedanceArrow).setBeschriftung_Text(Me._text_Nicht_Kursiv(DirectCast(element, SnapableImpedanceArrow).getBeschriftung_Text()))
                End If
            End If
        Next
        rück.speicherNachherZustand(Me.getRückArgs())
        If rück.istNotwendig() Then
            addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
        End If
    End Sub

    Private Function _text_Nicht_Kursiv(str As String) As String
        Try
            Dim tokens As New AdvancedTextRenderer.getNextTokenString(str)
            Dim matheModus As Boolean = False
            Dim klammerpos As Integer = 0
            Dim erg As String = ""
            Dim hatAddText As Boolean = False

            Dim lastTokenWas_text As Boolean = False

            Dim NoText_UntilKlammerpos As Integer = -1

            While tokens.hatNochToken()
                Dim myToken As AdvancedTextRenderer.Token = tokens.getNextToken()
                If myToken.isChar Then
                    If myToken.str = "$" Then
                        matheModus = Not matheModus
                    ElseIf myToken.str = "{" Then
                        If lastTokenWas_text Then
                            If NoText_UntilKlammerpos = -1 Then
                                NoText_UntilKlammerpos = klammerpos
                            End If
                        End If
                        klammerpos += 1
                    ElseIf myToken.str = "}" Then
                        klammerpos -= 1
                        If NoText_UntilKlammerpos = klammerpos Then
                            NoText_UntilKlammerpos = -1
                        End If
                    End If
                    lastTokenWas_text = False
                Else
                    If myToken.str = "text" Then
                        lastTokenWas_text = True
                    Else
                        lastTokenWas_text = False
                    End If
                End If
                If myToken.isChar AndAlso "abcdefghijklmnopqrstuvwxyzöäüßABCDEFGHIJKLMNOPQRSTUVWXYZÖÄÜ".Contains(myToken.str) Then
                    If matheModus AndAlso Not hatAddText AndAlso NoText_UntilKlammerpos = -1 Then
                        erg &= "\text{"
                        hatAddText = True
                    End If
                Else
                    If hatAddText Then
                        erg &= "}"
                        hatAddText = False
                    End If
                End If
                If myToken.isChar Then
                    erg &= myToken.str
                Else
                    erg &= "\" & myToken.str
                End If
            End While
            Return erg
        Catch ex As Exception
            MessageBox.Show("Fehler beim Konvertieren des Latex-Codes (" & str & "): " & ex.Message)
        End Try
        Return str
    End Function

#End Region

#Region "ausgewähltes Bauteil"
    Public Property currentPlaceBauteil As BauteilCell
        Get
            Return _currentPlaceBauteil
        End Get
        Set(value As BauteilCell)
            If value Is Nothing AndAlso _currentPlaceBauteil IsNot Nothing Then
                _currentPlaceBauteil = value
                updateCurrentTemplate()
            ElseIf value IsNot Nothing AndAlso _currentPlaceBauteil Is Nothing Then
                _currentPlaceBauteil = value
                updateCurrentTemplate()
            ElseIf value IsNot Nothing AndAlso _currentPlaceBauteil IsNot Nothing Then
                If value.Name <> _currentPlaceBauteil.Name Then
                    _currentPlaceBauteil = value
                    updateCurrentTemplate()
                End If
            End If
        End Set
    End Property

    Private Sub updateCurrentTemplate()
        If _currentPlaceBauteil Is Nothing Then
            Me.currentPlaceTemplate = Nothing
        Else
            Dim viewList = {"eu"}
            Dim view As BauteilView = Nothing
            For i As Integer = 0 To viewList.Count - 1
                If _currentPlaceBauteil.hasView(viewList(i)) Then
                    view = _currentPlaceBauteil.getView(viewList(i))
                    Exit For
                End If
            Next
            If view Is Nothing AndAlso _currentPlaceBauteil.getViewCount() > 0 Then
                view = _currentPlaceBauteil.getFirst()
            End If
            If view Is Nothing Then
                currentPlaceTemplate = Nothing
            Else
                currentPlaceTemplate = view.template
            End If
        End If

        If myTools.Count > 0 Then
            myTools.Peek().OnSelectedBauteilTemplateChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Public Function getCurrentTemplate() As TemplateAusDatei
        Return currentPlaceTemplate
    End Function

    Public Function getCurrentTemplateBauteilName() As String
        If currentPlaceTemplate IsNot Nothing Then
            Dim name As String = currentPlaceTemplate.getDefaultNaming()
            Return getNeuerName(name, currentPlaceTemplate.getNameMitNummer())
        Else
            Return ""
        End If
    End Function

    Public Function getNeuerName(Stammname As String, mitNummer As Boolean) As String
        If Not mitNummer Then
            Return Stammname
        Else
            If Stammname <> "" Then
                Dim nummer As Integer = 1
                While NameExistiert(getBauteilName(Stammname, nummer))
                    nummer += 1
                End While
                Return getBauteilName(Stammname, nummer)
            Else
                Return ""
            End If
        End If
    End Function

    Private Function getBauteilName(first As String, nr As Integer) As String
        Dim nrStr As String = nr.ToString()
        If nrStr.Length > 1 Then
            Return "$" & first & "_{" & nrStr & "}$"
        Else
            Return "$" & first & "_" & nrStr & "$"
        End If
    End Function

    Private Function NameExistiert(name As String) As Boolean
        For Each e As ElementMaster In myElemente
            If TypeOf e Is BauteilAusDatei AndAlso DirectCast(e, BauteilAusDatei).getBeschriftung_Text() = name Then
                Return True
            End If
        Next
        Return False
    End Function
#End Region

#Region "Rückgängig"
    Public Sub VorRückVorToolAbbrechen()
        If myTools.Count > 0 Then
            myTools.Peek().abortAction(Me)
        End If
    End Sub

    Public Sub addNeuesRückgängig(e As NeuesRückgängigEventArgs)
        RaiseEvent NeuesRückgängigElement(Me, e)
    End Sub

    Public Function getRückArgs() As RückgängigArgs
        Return New RückgängigArgs(myElemente, myLineStyles, myFillStyles, myFonts)
    End Function
#End Region

    Public Sub delete_selected(undoOhneMarkierungVorher As Boolean)
        If Me.has_selection() Then
            Dim rück As New RückgängigGrafik()
            rück.setText("Entfernen")
            rück.speicherVorherZustand(Me.getRückArgs())
            If undoOhneMarkierungVorher Then
                rück.unselectAllElementsVorher()
            End If
            For i As Integer = myElemente.Count - 1 To 0 Step -1
                If myElemente(i).hasSelection() Then
                    myElemente.RemoveAt(i)
                End If
            Next

            simplifyWires()

            rück.speicherNachherZustand(Me.getRückArgs())
            'Löschen von evtl. nicht mehr benötigten Linestyles!
            Dim rück2 As New RückgängigLineStyle()
            rück2.speicherVorher(getRückArgs())
            Dim rück3 As New RückgängigFillStyles()
            rück3.speicherVorher(getRückArgs())

            removeNichtBenötigteStyles()

            rück2.speicherNachher(getRückArgs())
            rück3.speicherNachher(getRückArgs())
            Dim r2Benötigt As Boolean = rück2.RückBenötigt()
            Dim r3Benötigt As Boolean = rück3.RückBenötigt()
            If r2Benötigt OrElse r3Benötigt Then
                Dim rGesamt As New RückgängigMulti()
                rGesamt.setText(rück.getText())
                rGesamt.Rück.Add(rück)
                If r2Benötigt Then rGesamt.Rück.Add(rück2)
                If r3Benötigt Then rGesamt.Rück.Add(rück3)
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rGesamt))
            Else
                Me.addNeuesRückgängig(New NeuesRückgängigEventArgs(rück))
            End If

            Me.Invalidate()
        End If
    End Sub

#Region "Speichern, Laden"
    Public Sub speicherGrafik(writer As BinaryWriter)

        myLineStyles.speichern(writer)
        myFillStyles.speichern(writer)
        myFonts.speichern(writer)

        writer.Write(Me.drawDotsOnIntersectingWires)
        writer.Write(Me.drawZeilensprünge)
        writer.Write(Me.radius_Zeilensprünge)

        writer.Write(Me.MarginExport.Left)
        writer.Write(Me.MarginExport.Right)
        writer.Write(Me.MarginExport.Top)
        writer.Write(Me.MarginExport.Bottom)

        writer.Write(MM_PER_INT)

        writer.Write(myElemente.Count)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is Element Then
                SpeicherCircuitElement(DirectCast(myElemente(i), Element), writer)
            ElseIf TypeOf myElemente(i) Is SnapableElement Then
                SpeicherSnapElement(DirectCast(myElemente(i), SnapableElement), writer)
            End If
        Next
    End Sub

    Public Shared Sub SpeicherCircuitElement(e As Element, writer As BinaryWriter)
        writer.Write(_SPEICHERN_StartElement)
        If TypeOf e Is BauteilAusDatei Then
            writer.Write(0)
            e.speichern(writer)
        ElseIf TypeOf e Is Wire Then
            writer.Write(1)
            e.speichern(writer)
        ElseIf TypeOf e Is WireLuftlinie Then
            writer.Write(2)
            e.speichern(writer)
        ElseIf TypeOf e Is Blackbox Then
            writer.Write(3)
            e.speichern(writer)
        ElseIf TypeOf e Is Element_Rect Then
            writer.Write(4)
            e.speichern(writer)
        ElseIf TypeOf e Is Element_Ellipse Then
            writer.Write(5)
            e.speichern(writer)
        ElseIf TypeOf e Is Element_Textfeld Then
            writer.Write(6)
            e.speichern(writer)
        ElseIf TypeOf e Is Element_RoundRect Then
            writer.Write(7)
            e.speichern(writer)
        ElseIf TypeOf e Is Basic_Spannungspfeil Then
            writer.Write(8)
            e.speichern(writer)
        ElseIf TypeOf e Is Basic_Linie Then
            writer.Write(9)
            e.speichern(writer)
        ElseIf TypeOf e Is Element_Graph Then
            writer.Write(10)
            e.speichern(writer)
        ElseIf TypeOf e Is Basic_Bezier Then
            writer.Write(11)
            e.speichern(writer)
        ElseIf TypeOf e Is ElementGruppe Then
            writer.Write(12)
            e.speichern(writer)
        Else
            writer.Write(-1)
        End If
    End Sub
    Public Shared Function LadeCircuitElement(v As Vektor_Picturebox, reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean, fillStile_VersionKleiner27_transparent As Integer, ElementTYP As Integer) As Element
        If ElementTYP <> _SPEICHERN_StartElement Then
            Throw New Exception("Falsches Dateiformat (Fehler B1000).")
        End If
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 0
                Return BauteilAusDatei.Einlesen(v, reader, version, bib, lokaleBib, kompatibilität, fillStile_VersionKleiner27_transparent)
            Case 1
                Return Wire.Einlesen(v, reader, version)
            Case 2
                Return WireLuftlinie.Einlesen(v, reader, version)
            Case 3
                Return Blackbox.Einlesen(v, reader, version)
            Case 4
                Return Element_Rect.Einlesen(v, reader, version)
            Case 5
                Return Element_Ellipse.Einlesen(v, reader, version)
            Case 6
                Return Element_Textfeld.Einlesen(v, reader, version)
            Case 7
                Return Element_RoundRect.Einlesen(v, reader, version)
            Case 8
                Return Basic_Spannungspfeil.Einlesen(v, reader, version)
            Case 9
                Return Basic_Linie.Einlesen(v, reader, version)
            Case 10
                Return Element_Graph.Einlesen(v, reader, version)
            Case 11
                Return Basic_Bezier.Einlesen(v, reader, version)
            Case 12
                Return ElementGruppe.Einlesen(v, reader, version, bib, lokaleBib, kompatibilität, fillStile_VersionKleiner27_transparent)
            Case -1
                Return Nothing
            Case Else
                Throw New Exception("Nicht unterstützes Elementeformat. Diese Datei wurde wahrscheinlich in einer neueren Version von Vektorgrafik erstellt und ist nicht einlesbar.")
        End Select
    End Function

    ''' <summary>
    ''' Nur noch zur Kompatibilität mit Version kleiner 16!!!
    ''' </summary>
    ''' <param name="v"></param>
    ''' <param name="reader"></param>
    ''' <param name="version"></param>
    ''' <returns></returns>
    Public Shared Function LadeDrawingElement(v As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element
        If reader.ReadInt32() <> _SPEICHERN_StartElementDrawing Then
            Throw New Exception("Falsches Dateiformat (Fehler B1001).")
        End If
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 0
                Return Element_Rect.Einlesen(v, reader, version)
            Case 1
                Return Element_Ellipse.Einlesen(v, reader, version)
            Case 2
                Return Element_Textfeld.Einlesen(v, reader, version)
            Case 3
                Return Element_RoundRect.Einlesen(v, reader, version)
            Case 4
                Return Basic_Spannungspfeil.Einlesen(v, reader, version)
            Case 5
                Return Basic_Linie.Einlesen(v, reader, version)
            Case -1
                Return Nothing
            Case Else
                Throw New Exception("Nicht unterstützes Elementeformat. Diese Datei wurde wahrscheinlich in einer neueren Version von Vektorgrafik erstellt und ist nicht einlesbar.")
        End Select
    End Function

    Public Shared Sub SpeicherSnapElement(e As SnapableElement, writer As BinaryWriter)
        writer.Write(_SPEICHERN_StartElementSnapping)
        If TypeOf e Is SnapableCurrentArrow Then
            writer.Write(0)
            e.speichern(writer)
            '1 war mal testweise ein snapableVoltageArrow
        ElseIf TypeOf e Is SnapableLabel Then
            writer.Write(2)
            e.speichern(writer)
        ElseIf TypeOf e Is SnapableBusTap Then
            writer.Write(3)
            e.speichern(writer)
        ElseIf TypeOf e Is SnapableImpedanceArrow Then
            writer.Write(4)
            e.speichern(writer)
        Else
            writer.Write(-1)
        End If
    End Sub
    Public Shared Function LadeSnapElement(v As Vektor_Picturebox, reader As BinaryReader, version As Integer, ElementTYP As Integer) As SnapableElement
        If ElementTYP <> _SPEICHERN_StartElementSnapping Then
            Throw New Exception("Falsches Dateiformat (Fehler B1002).")
        End If
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 0
                Return SnapableCurrentArrow.Einlesen(v, reader, version)
            Case 1
                Throw New Exception("Dieses Symbol (SnapableVoltageArrow) wird nicht mehr unterstützt!")
            Case 2
                Return SnapableLabel.Einlesen(v, reader, version)
            Case 3
                Return SnapableBusTap.Einlesen(v, reader, version)
            Case 4
                Return SnapableImpedanceArrow.Einlesen(v, reader, version)
            Case -1
                Return Nothing
            Case Else
                Throw New Exception("Nicht unterstützes Elementeformat. Diese Datei wurde wahrscheinlich in einer neueren Version von Vektorgrafik erstellt und ist nicht einlesbar.")
        End Select
    End Function

    Public Sub ladeGrafik(reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean)
        Dim maxID_old As ULong = Me.MAX_ID 'Falls das Laden fehlschlägt wird die alte ID wieder verwendet!
        Me.MAX_ID = 0 'Beim Laden einer neuen Datei immer wieder von ID=0 anfangen!

        Try
            ladeGrafikIntern(reader, version, bib, lokaleBib, kompatibilität)
        Catch ex As Exception
            Me.MAX_ID = maxID_old
            Throw ex
        End Try
    End Sub

    Private Sub ladeGrafikIntern(reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean)
        Dim neueLineStyles As LineStyleList = LineStyleList.Einlesen(reader, version)
        Dim neueFillStyles As FillStyleList = FillStyleList.Einlesen(reader, version)
        Dim neueFontStyles As FontList = FontList.Einlesen(reader, version)

        Dim _drawDotsOnIntersectingWires As Boolean = True
        Dim _drawZeilensprünge As Boolean = False
        Dim _radius_Zeilensprünge As Integer = DEFAULT_RADIUS_ZEILENSPRÜNGE
        If version >= 12 Then
            _drawDotsOnIntersectingWires = reader.ReadBoolean()
            _drawZeilensprünge = reader.ReadBoolean()
            _radius_Zeilensprünge = reader.ReadInt32()
        End If

        Dim ml, mr, mt, mb As Integer
        If version >= 26 Then
            ml = reader.ReadInt32()
            mr = reader.ReadInt32()
            mt = reader.ReadInt32()
            mb = reader.ReadInt32()
        Else
            ml = 200
            mr = 200
            mb = 200
            mt = 200
        End If

        Dim mm_per_int As Single = DEFAULT_MM_PER_INT
        If version >= 30 Then
            mm_per_int = reader.ReadSingle()
        End If

        Dim fillStile_VersionKleiner27_transparent As Integer = 0
        If version < 27 Then
            'Hier gab es noch keinen Fillstyle bei elementen!
            'Bauteile müssen also einen Transparenten Hintergrund bekommen, auch wenn es diesen Fillstil noch nicht gibt
            fillStile_VersionKleiner27_transparent = neueFillStyles.getNumberOfNewFillStyle(New FillStyle(New Farbe(0, 255, 255, 255)))
        End If

        Dim neueElemente() As ElementMaster
        If version <= 30 Then
            'In Version <= 30 waren elemente und snapableElemente getrennt! Also wurden diese nacheinander gespeichert und eingelesen!
            Dim anzahlElemente As Integer = reader.ReadInt32()
            If anzahlElemente < 0 Then
                Throw New Exception("Fehler in der Datei. Die Anzahl der Elemente kann nicht negativ sein.")
            End If
            ReDim neueElemente(anzahlElemente - 1)
            For i As Integer = 0 To anzahlElemente - 1
                neueElemente(i) = LadeCircuitElement(Me, reader, version, bib, lokaleBib, kompatibilität, fillStile_VersionKleiner27_transparent, reader.ReadInt32())
            Next

            If version >= 1 AndAlso version < 16 Then 'Lese DrawingElemente (sind ab Version 16 auch als Element gespeichert! Früher war das eine seperate Klasse!)
                'lese Drawing Objecte!
                Dim anzahlDrawing As Integer = reader.ReadInt32()
                If anzahlDrawing < 0 Then
                    Throw New Exception("Fehler in der Datei. Die Anzahl der Elemente kann nicht negativ sein.")
                End If
                Dim offset As Integer = neueElemente.Length
                ReDim Preserve neueElemente(neueElemente.Length + anzahlDrawing - 1)
                For i As Integer = 0 To anzahlDrawing - 1
                    neueElemente(offset + i) = LadeDrawingElement(Me, reader, version)
                Next
            End If

            If version >= 3 Then
                'lese Snaping Elemente
                Dim anzahlSnaping As Integer = reader.ReadInt32()
                If anzahlSnaping < 0 Then
                    Throw New Exception("Fehler in der Datei. Die Anzahl der Elemente kann nicht negativ sein.")
                End If
                Dim offset As Integer = neueElemente.Length
                ReDim Preserve neueElemente(neueElemente.Length + anzahlSnaping - 1)
                For i As Integer = 0 To anzahlSnaping - 1
                    neueElemente(offset + i) = LadeSnapElement(Me, reader, version, reader.ReadInt32())
                Next
            End If
        Else
            'Ab Version 31 sind alle Bauteile in einer Liste gespeichert!
            Dim anzahlElemente As Integer = reader.ReadInt32()
            If anzahlElemente < 0 Then
                Throw New Exception("Fehler in der Datei. Die Anzahl der Elemente kann nicht negativ sein.")
            End If
            ReDim neueElemente(anzahlElemente - 1)
            For i As Integer = 0 To anzahlElemente - 1
                Dim typ As Integer = reader.ReadInt32()
                If typ = _SPEICHERN_StartElement Then
                    neueElemente(i) = LadeCircuitElement(Me, reader, version, bib, lokaleBib, kompatibilität, fillStile_VersionKleiner27_transparent, typ)
                ElseIf typ = _SPEICHERN_StartElementSnapping Then
                    neueElemente(i) = LadeSnapElement(Me, reader, version, typ)
                End If
            Next
        End If

        myElemente.Clear()
        For i As Integer = 0 To neueElemente.Length - 1
            If neueElemente(i) IsNot Nothing Then
                neueElemente(i).deselect()
                myElemente.Add(neueElemente(i))
            End If
        Next

        myLineStyles = neueLineStyles
        myFillStyles = neueFillStyles
        myFonts = neueFontStyles

        Me.drawZeilensprünge = _drawZeilensprünge
        Me.drawDotsOnIntersectingWires = _drawDotsOnIntersectingWires
        Me.radius_Zeilensprünge = _radius_Zeilensprünge
        Me.MarginExport = New Padding(ml, mt, mr, mb)
        Me.MM_PER_INT = mm_per_int

        'Workaround für fit_to_screen.
        'fit_to_screen wird immer den letzen Renderzustand berücksichtigen.
        'Daher einmal "rendern", damit es wieder die bounds richtig berechnen kann!
        WORKAROUND_FitToScreen_AFTERLOAD()

        Me.Invalidate()
    End Sub

    Public Sub WORKAROUND_FitToScreen_AFTERLOAD()
        'Workaround für fit_to_screen.
        'fit_to_screen wird immer den letzen Renderzustand berücksichtigen.
        'Daher einmal "rendern", damit es wieder die bounds richtig berechnen kann!
        Using g As Graphics = Graphics.FromImage(New Bitmap(1, 1))
            Me.OnPaint(New PaintEventArgs(g, New Rectangle(0, 0, Me.Width, Me.Height)))
        End Using

        Me.fit_to_screen()
    End Sub
#End Region

#Region "Exportieren"
    Public Sub exportierenAlsEMF(pfad As String)
        Dim f As Single = 0.1

        Dim pixelPerMM As Single = f / MM_PER_INT

        Dim box As Rectangle = getBoundingBoxWithMarginExport()


        Dim ox As Single = -box.X * f
        Dim oy As Single = -box.Y * f

        Dim args As New GrafikDrawArgs(myLineStyles, myFillStyles, myFonts, pixelPerMM, False)
        args.faktorX = f
        args.faktorY = f
        args.offsetX = ox
        args.offsetY = oy

        Try
            Dim size As New Size(CInt(box.Width * f), CInt(box.Height * f))
            Dim bmp As New Bitmap(size.Width, size.Height)

            Dim gRef As Graphics = Graphics.FromImage(bmp)
            gRef.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            gRef.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
            gRef.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            Dim hdc As IntPtr = gRef.GetHdc()
            Dim meta As New Imaging.Metafile(hdc, Imaging.EmfType.EmfPlusDual, "...")

            Using g As Graphics = Graphics.FromImage(meta)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

                Dim myElementeKomplett As List(Of DO_Grafik) = getExportElemente()

                For Each element As DO_Grafik In myElementeKomplett
                    element.drawGraphics(g, args)
                Next
            End Using

            gRef.ReleaseHdc(hdc)
            gRef.Dispose()
            EMF_Speicherer.SaveAsEmf(meta, pfad)
        Catch ex As Exception
            MessageBox.Show("Export als EMF fehlgeschlagen: " + ex.Message)
        End Try

    End Sub

    Public Sub exportierenAlsIMG(pfad As String, format As Imaging.ImageFormat, transparent As Boolean, exportSize As Size)
        Dim box As Rectangle = getBoundingBoxWithMarginExport()

        Dim f_w As Double = exportSize.Width / box.Width
        Dim f_h As Double = exportSize.Height / box.Height

        Dim f As Single = CSng(Math.Min(f_w, f_h))

        Dim pixelPerMM As Single = f / MM_PER_INT

        Dim ox As Single = -box.X * f
        Dim oy As Single = -box.Y * f

        Dim args As New GrafikDrawArgs(myLineStyles, myFillStyles, myFonts, pixelPerMM, False)
        args.faktorX = f
        args.faktorY = f
        args.offsetX = ox
        args.offsetY = oy

        Try
            Dim size As New Size(exportSize.Width, exportSize.Height)
            Dim bmp As New Bitmap(size.Width, size.Height)

            Using g As Graphics = Graphics.FromImage(bmp)
                If Not transparent Then
                    g.Clear(Color.White)
                End If

                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

                Dim myElementeKomplett As List(Of DO_Grafik) = getExportElemente()

                For Each element As DO_Grafik In myElementeKomplett
                    element.drawGraphics(g, args)
                Next
            End Using

            bmp.Save(pfad, format)
        Catch ex As Exception
            MessageBox.Show("Export als Bild fehlgeschlagen: " + ex.Message)
        End Try
    End Sub

    Public Sub exportierenAlsPDF(pfad As String)
        exportiereAlsPdf_PDFSharp(pfad, False)
    End Sub

    Private Function exportiereAlsPdf_PDFSharp(pfad As String, ohneText As Boolean) As GrafikPDFSharp_DrawArgs
        Dim box As Rectangle = getBoundingBoxWithMarginExport()

        Dim maßeinheit As Single = MM_PER_INT 'Anzahl mm pro länge (10mm pro 600 Einheiten)

        Dim breite As Double = GrafikPDFSharp_DrawArgs.mmToPoint(maßeinheit * box.Width)
        Dim höhe As Double = GrafikPDFSharp_DrawArgs.mmToPoint(maßeinheit * box.Height)

        Dim args As New GrafikPDFSharp_DrawArgs(maßeinheit, New Point(-box.X, -box.Y), New Point(-box.X, box.Y + box.Height), myLineStyles, myFillStyles, myFonts)
        args.OhneText = ohneText
        args.breitePDF_inINT = box.Width
        args.höhePDF_inINT = box.Height

        Dim doc As New PdfSharp.Pdf.PdfDocument()
        doc.Info.Title = "Schematic created with CircuitDrawing"
        doc.Info.Author = ""
        doc.Info.Subject = ""
        doc.Info.Keywords = ""

        doc.Options.FlateEncodeMode = PdfSharp.Pdf.PdfFlateEncodeMode.BestCompression
        doc.Options.NoCompression = False
        doc.Options.CompressContentStreams = True

        Dim outputStream As FileStream = Nothing
        Try

            Dim page As PdfSharp.Pdf.PdfPage = doc.AddPage()
            page.Width = New PdfSharp.Drawing.XUnit(breite, PdfSharp.Drawing.XGraphicsUnit.Point)
            page.Height = New PdfSharp.Drawing.XUnit(höhe, PdfSharp.Drawing.XGraphicsUnit.Point)

            Dim gfx As PdfSharp.Drawing.XGraphics = PdfSharp.Drawing.XGraphics.FromPdfPage(page)
            gfx.SmoothingMode = PdfSharp.Drawing.XSmoothingMode.HighQuality
            args.initTextFormatter(gfx)

            Dim myElementeKomplett As List(Of DO_Grafik) = getExportElemente()
            For Each element As DO_Grafik In myElementeKomplett
                element.drawPDFSharp(gfx, args)
            Next

            outputStream = New FileStream(pfad, FileMode.Create, FileAccess.Write)
            doc.Save(outputStream)
            doc.Close()
            outputStream.Close()
            If Not ohneText Then
                args.showWarnings()
            End If
        Catch ex As Exception
            MessageBox.Show("Export als PDF fehlgeschlagen: " + ex.Message)
            Return Nothing
        Finally
            If doc IsNot Nothing Then
                doc.Close()
            End If
            If outputStream IsNot Nothing Then
                outputStream.Close()
            End If
        End Try
        Return args
    End Function

    Public Sub exportierenAlsTEX(pfad As String, eps As Boolean)
        Dim dateiname As String = New FileInfo(pfad).Name
        If dateiname.EndsWith(".tex") Then
            dateiname = dateiname.Substring(0, dateiname.Length - 4)
        End If
        Dim directoryName As String = New FileInfo(pfad).DirectoryName

        Dim dateinamePDF As String
        If eps Then
            dateinamePDF = dateiname & "epsDrawing"
        Else
            dateinamePDF = dateiname & "pdfDrawing"
        End If

        Dim PDF_maßeinheit As Single
        Dim PDF_offset As Point
        Dim PDF_breiteINT As Integer
        Dim PDF_hoeheINT As Integer
        Dim hatPDF As Boolean = False
        Dim argsPdfSharp As GrafikPDFSharp_DrawArgs = Nothing
        If eps Then
            Dim argsEPS = exportiereAlsEPS(directoryName & "/" & dateinamePDF & ".eps", True)
            If argsEPS IsNot Nothing Then
                PDF_maßeinheit = argsEPS.maßeinheit
                PDF_offset = argsEPS.offsetTEX
                PDF_breiteINT = argsEPS.breitePDF_inINT
                PDF_hoeheINT = argsEPS.höhePDF_inINT
                hatPDF = True
            End If
        Else
            Dim argsPDF = exportiereAlsPdf_PDFSharp(directoryName & "/" & dateinamePDF & ".pdf", True)
            If argsPDF IsNot Nothing Then
                PDF_maßeinheit = argsPDF.maßeinheit
                PDF_offset = argsPDF.offsetTEX
                PDF_breiteINT = argsPDF.breitePDF_inINT
                PDF_hoeheINT = argsPDF.höhePDF_inINT
                hatPDF = True
                argsPdfSharp = argsPDF
            End If
        End If
        If hatPDF Then
            Dim args As New GrafikTEX_DrawArgs(PDF_maßeinheit, PDF_offset, Me.myFonts)

            Dim outputStream As FileStream = Nothing
            Try
                outputStream = New FileStream(pfad, FileMode.Create, FileAccess.Write)
                Dim enc As System.Text.Encoding = Nothing
                If Settings.getSettings().Encoding = Settings.myEncodings.ANSI Then
                    enc = System.Text.Encoding.Default
                ElseIf Settings.getSettings().Encoding = Settings.myEncodings.UTF8 Then
                    enc = System.Text.Encoding.UTF8
                Else
                    Throw New Exception("Diese Kodierung ist nicht bekannt")
                End If
                Dim writer As New StreamWriter(outputStream, enc)

                writer.WriteLine("\begingroup")
                writer.WriteLine("\makeatletter\ifx\scaleSchematic\@SimonBuhr@undefined@\def\scaleSchematic{1}\fi")
                writer.WriteLine("\ifx\pathSchematic\@SimonBuhr@undefined@\def\pathSchematic{}\fi")
                writer.WriteLine("\setlength{\unitlength}{" & args.maßeinheit.ToString(myNumberFormatInfoTEX) & "mm}")
                writer.WriteLine("\setlength{\unitlength}{\scaleSchematic\unitlength}")
                writer.WriteLine("\begin{picture}(" & PDF_breiteINT.ToString(myNumberFormatInfoTEX) & "," & PDF_hoeheINT.ToString(myNumberFormatInfoTEX) & ")%")
                writer.WriteLine("\put(0,0){\includegraphics[scale=\scaleSchematic]{\pathSchematic " & dateinamePDF & "}}%")

                For Each e As ElementMaster In myElemente
                    e.getGrafik().drawTEX_Text(writer, args)
                Next

                writer.WriteLine("\end{picture}%")
                writer.WriteLine("\makeatother")
                writer.WriteLine("\endgroup")
                writer.Flush()
                If argsPdfSharp IsNot Nothing Then
                    argsPdfSharp.showWarnings()
                End If
            Catch ex As Exception
                MessageBox.Show("Export als TEX fehlgeschlagen: " + ex.Message)
            Finally
                If outputStream IsNot Nothing Then
                    outputStream.Close()
                End If
            End Try
        End If
    End Sub

    Public Function exportiereAlsEPS(pfad As String, ohneText As Boolean) As GrafikEPS_DrawArgs
        Dim box As Rectangle = getBoundingBoxWithMarginExport()

        Dim maßeinheit As Single = MM_PER_INT 'Anzahl mm pro länge (10mm pro 600 Einheiten)

        Dim breite As Double = GrafikEPS_DrawArgs.mmToPoint(maßeinheit * box.Width)
        Dim höhe As Double = GrafikEPS_DrawArgs.mmToPoint(maßeinheit * box.Height)

        Dim args As New GrafikEPS_DrawArgs(maßeinheit, New Point(-box.X, box.Y + box.Height), New Point(-box.X, box.Y + box.Height), myLineStyles, myFillStyles, myFonts)
        args.OhneText = ohneText
        args.breitePDF_inINT = box.Width
        args.höhePDF_inINT = box.Height

        Dim outputStream As FileStream = Nothing
        Try
            outputStream = New FileStream(pfad, FileMode.Create, FileAccess.Write)
            Dim utf8WithoutBom As New Text.UTF8Encoding(False)
            Dim writer As New StreamWriter(outputStream, utf8WithoutBom)
            writer.WriteLine("%!PS-Adobe-3.0 EPSF-3.0")
            writer.WriteLine("%%BoundingBox: " & args.writePoint(New Point(box.X, box.Y + box.Height)) & " " & args.writePoint(New Point(box.X + box.Width, box.Y)))
            writer.WriteLine("%%Creator: CircuitDrawing")
            writer.WriteLine("%%EndComments")

            writer.WriteLine("/ellipse {")
            writer.WriteLine("/endangle exch def")
            writer.WriteLine("/startangle exch def")
            writer.WriteLine("/yrad exch def")
            writer.WriteLine("/xrad exch def")
            writer.WriteLine("/y exch def")
            writer.WriteLine("/x exch def")
            writer.WriteLine("/savematrix matrix currentmatrix def")
            writer.WriteLine("x y translate")
            writer.WriteLine("xrad yrad scale")
            writer.WriteLine("0 0 1 startangle endangle arc")
            writer.WriteLine("savematrix setmatrix")
            writer.WriteLine("} def")

            Dim myElementeKomplett As List(Of DO_Grafik) = getExportElemente()
            For Each element As DO_Grafik In myElementeKomplett
                element.drawEPS(writer, args)
            Next

            'writer.WriteLine("showpage")
            'writer.WriteLine("%%EOF")

            writer.Flush()
            writer.Close()
        Catch ex As Exception
            MessageBox.Show("Export als PDF fehlgeschlagen: " + ex.Message)
            Return Nothing
        Finally
            If outputStream IsNot Nothing Then
                outputStream.Close()
            End If
        End Try
        Return args
    End Function

    Public Function getBoundingBoxWithMarginExport() As Rectangle
        Dim box As Rectangle
        'If myCircuitElemente.Count > 0 Then
        '    box = myCircuitElemente(0).getGrafik.getBoundingBox()

        '    For i As Integer = 1 To myCircuitElemente.Count - 1
        '        box = Mathe.Union(box, myCircuitElemente(i).getGrafik.getBoundingBox())
        '    Next
        'Else
        '    box = New Rectangle(0, 0, 0, 0)
        'End If
        'If myDrawingElemente.Count > 0 Then
        '    For i As Integer = 0 To myDrawingElemente.Count - 1
        '        box = Mathe.Union(box, myDrawingElemente(i).getGrafik.getBoundingBox())
        '    Next
        'End If

        box = Me.myLastDrawingBoundingBox

        box.X -= MarginExport.Left
        box.Y -= MarginExport.Top
        box.Width += MarginExport.Left + MarginExport.Right
        box.Height += MarginExport.Top + MarginExport.Bottom

        Return box
    End Function

    Private Function getExportElemente() As List(Of DO_Grafik)
        Dim erg As New List(Of DO_Grafik)(myElemente.Count)

        Dim wires As List(Of IWire) = Nothing
        If drawZeilensprünge Then
            wires = New List(Of IWire)
            For Each el As ElementMaster In myElemente
                If TypeOf el Is IWire Then wires.Add(DirectCast(el, IWire))
            Next
        End If

        Dim dots As List(Of Zorder_DO_Grafik) = Nothing
        Dim dotsCurrentZorder As Integer = myElemente.Count
        Dim dotsNr As Integer = 0
        If Me.drawDotsOnIntersectingWires Then
            dots = drawDots()
            If dots.Count > 0 Then
                dotsCurrentZorder = dots(0).ZOrder
            End If
        End If

        For i As Integer = 0 To myElemente.Count - 1
            If Not (Me.drawZeilensprünge AndAlso TypeOf myElemente(i) Is IWire) Then
                erg.Add(myElemente(i).getGrafik())
            Else
                erg.Add(DirectCast(myElemente(i), IWire).getGrafikMitZeilensprüngen(radius_Zeilensprünge, wires))
            End If
            If Me.drawDotsOnIntersectingWires Then
                'Dots malen
                While dotsCurrentZorder = i
                    erg.Add(dots(dotsNr).myGrafik)

                    dotsNr += 1
                    If dotsNr < dots.Count Then
                        dotsCurrentZorder = dots(dotsNr).ZOrder
                    Else
                        dotsCurrentZorder = myElemente.Count
                    End If
                End While
            End If
        Next
        Return erg
    End Function
#End Region

    Public Sub copySelectedElements(ByRef neueElemente As List(Of ElementMaster))
        If neueElemente Is Nothing Then
            neueElemente = New List(Of ElementMaster)
        Else
            neueElemente.Clear()
        End If

        For Each element As ElementMaster In myElemente
            If element.hasSelection() Then
                neueElemente.Add(element.Clone())
            End If
        Next
    End Sub

    Public Function getGridX() As Integer
        Return GridX
    End Function

    Public Function getGridY() As Integer
        Return GridY
    End Function

    Public Sub deleteAll()
        myElemente.Clear()
    End Sub

#End Region

#Region "Rendern"
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        Dim pixelPerMM As Single = calcPixelPerMM()

        Dim args As New GrafikDrawArgs(myLineStyles, myFillStyles, myFonts, pixelPerMM, Me.TextVorschauMode)
        Dim argsSelection As New GrafikDrawArgs(myLineStylesSelectionDrawing, myFillStyles, myFonts, pixelPerMM, Me.TextVorschauMode)

        setViewArgs(args)
        setViewArgs(argsSelection)

        args.clip_range = New Rectangle(0, 0, Me.Width, Me.Height)

        e.Graphics.Clear(Color.White)
        e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        drawGrid(e)
        e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        e.Graphics.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit

        Dim grafik As DO_Grafik

        Dim boundingBox As New Rectangle(0, 0, 0, 0)

        If _cursorInside AndAlso crossCursor Then
            drawCrossCursorAtPosition(e, _cursorPos)
        End If

        If myElemente IsNot Nothing Then
            'male Elemente

            Dim wires As List(Of IWire) = Nothing
            If drawZeilensprünge Then
                wires = New List(Of IWire)
                For Each el As ElementMaster In myElemente
                    If TypeOf el Is IWire Then wires.Add(DirectCast(el, IWire))
                Next
            End If

            Dim dots As List(Of Zorder_DO_Grafik) = Nothing
            Dim dotsCurrentZorder As Integer = myElemente.Count
            Dim dotsNr As Integer = 0
            If Me.drawDotsOnIntersectingWires Then
                dots = drawDots()
                If dots.Count > 0 Then
                    dotsCurrentZorder = dots(0).ZOrder
                End If
            End If

            For i As Integer = 0 To myElemente.Count - 1
                If Not (Me.drawZeilensprünge AndAlso TypeOf myElemente(i) Is IWire) Then
                    grafik = myElemente(i).getGrafik()
                Else
                    grafik = DirectCast(myElemente(i), IWire).getGrafikMitZeilensprüngen(radius_Zeilensprünge, wires)
                End If
                grafik.drawGraphics(e.Graphics, args)
                If Me.drawDotsOnIntersectingWires Then
                    'Dots malen
                    While dotsCurrentZorder = i
                        dots(dotsNr).myGrafik.drawGraphics(e.Graphics, args)
                        boundingBox = Mathe.Union(boundingBox, dots(dotsNr).myGrafik.getBoundingBox())

                        dotsNr += 1
                        If dotsNr < dots.Count Then
                            dotsCurrentZorder = dots(dotsNr).ZOrder
                        Else
                            dotsCurrentZorder = myElemente.Count
                        End If
                    End While
                End If

                Dim bBox As Rectangle = grafik.getBoundingBox()
                boundingBox = Mathe.Union(boundingBox, bBox)
                myElemente(i).AfterDrawingGDI(bBox)
            Next

            'male snappoints
            If Me.showSnappoints OrElse Me.snappoinsImmerAnzeigen Then
                For Each element In myElemente
                    If TypeOf element Is Element Then
                        For sn As Integer = 0 To DirectCast(element, Element).NrOfSnappoints() - 1
                            Dim p As PointF = args.toPictureboxPoint(DirectCast(element, Element).getSnappoint(sn).p)
                            e.Graphics.DrawLine(Pens.Blue, p.X - 25 * faktor, p.Y - 25 * faktor, p.X + 25 * faktor, p.Y + 25 * faktor)
                            e.Graphics.DrawLine(Pens.Blue, p.X - 25 * faktor, p.Y + 25 * faktor, p.X + 25 * faktor, p.Y - 25 * faktor)
                        Next
                    End If
                Next
            End If

            'male selections
            Dim selection As Selection
            For Each element In myElemente
                If element.hasSelection Then
                    If TypeOf element Is Element Then
                        selection = DirectCast(element, Element).getSelection
                        selection.getGrafik().drawGraphics(e.Graphics, argsSelection)
                    ElseIf TypeOf element Is SnapableElement Then
                        For i As Integer = 0 To DirectCast(element, SnapableElement).getNrOfSnappoints() - 1
                            If DirectCast(element, SnapableElement).getSnappoint(i).isSelected Then
                                selection = DirectCast(element, SnapableElement).getSnappoint(i).getSelection
                                selection.getGrafik().drawGraphics(e.Graphics, argsSelection)
                            End If
                        Next
                    End If
                End If
            Next
        End If

        'Maus anzeigen
        If _cursorInside Then
            drawCursorAtPosition(e, _cursorPos, CursorStyle.Rectangle, True)
        End If

        'Tool Overlay anzeigen
        If myTools.Count > 0 Then
            myTools.Peek().OnDraw(Me, New ToolPaintEventArgs(e, args, argsSelection))
        End If

        'BoundingBox nach dem Malen Messen!
        myLastDrawingBoundingBox = boundingBox

        If Me.showBoarder Then
            Dim rb As RectangleF = args.toPictureboxRect(Me.getBoundingBoxWithMarginExport())
            e.Graphics.DrawRectangle(Pens.Blue, rb.X, rb.Y, rb.Width, rb.Height)
        End If
    End Sub

    Private Sub drawGrid(e As PaintEventArgs)
        Dim Grid_Mult_X As Integer
        Dim Grid_Mult_Y As Integer
        Grid_Mult_X = CInt(40.0F / faktor) 'Wieviel 'Int' sind 10.0F Pixel auf dem Bildschirm?
        Grid_Mult_Y = Grid_Mult_X

        'fit to grid!
        Grid_Mult_X = fitToGridX(Grid_Mult_X)
        Grid_Mult_Y = fitToGridY(Grid_Mult_Y)

        'Auf Log2 Werte runden!
        If Grid_Mult_X > 0 Then
            Dim ldX As Integer = CInt(Math.Round(Math.Log(Grid_Mult_X) / Math.Log(2)))
            Grid_Mult_X = CInt(Math.Pow(2, ldX))
        Else
            Grid_Mult_X = 0
        End If
        If Grid_Mult_Y > 0 Then
            Dim ldY As Integer = CInt(Math.Round(Math.Log(Grid_Mult_Y) / Math.Log(2)))
            Grid_Mult_Y = CInt(Math.Pow(2, ldY))
        Else
            Grid_Mult_Y = 0
        End If

        'Minimum Wert auf 2*Grid setzen!
        Grid_Mult_X = Math.Max(Grid_Mult_X, 1)
        Grid_Mult_Y = Math.Max(Grid_Mult_Y, 1)

        'Maximum Wert auf 16*Grid setzen!
        If Grid_Mult_X <= 16 AndAlso Grid_Mult_Y <= 16 Then

            Grid_Mult_X *= GridX
            Grid_Mult_Y *= GridY

            Dim start_X As Integer = CInt((-offsetX) / faktor)
            Dim start_Y As Integer = CInt((-offsetY) / faktor)
            Dim ende_X As Integer = CInt((Me.Width - offsetX) / faktor)
            Dim ende_Y As Integer = CInt((Me.Height - offsetY) / faktor)

            'Runden auf Grid_Mult!
            start_X = (start_X \ Grid_Mult_X) * Grid_Mult_X
            start_Y = (start_Y \ Grid_Mult_Y) * Grid_Mult_Y
            ende_X = (ende_X \ Grid_Mult_X) * Grid_Mult_X
            ende_Y = (ende_Y \ Grid_Mult_Y) * Grid_Mult_Y

            Dim px As Integer
            Dim py As Integer

            Dim B As Byte = Color.LightGray.B
            Dim G As Byte = Color.LightGray.G
            Dim R As Byte = Color.LightGray.R

            Dim breite As Integer = Me.Width
            Dim bpl As Integer = 4 * breite

            If _GRID_IMAGE_LARGE Is Nothing OrElse _GRID_IMAGE_LARGE.Width <> breite Then
                _GRID_IMAGE_LARGE = New Bitmap(breite, 5)
                ReDim _GRID_BYTES_LARGE(_GRID_IMAGE_LARGE.Width * _GRID_IMAGE_LARGE.Height * 4 - 1)
            End If
            If _GRID_IMAGE_SMALL Is Nothing OrElse _GRID_IMAGE_SMALL.Width <> breite Then
                _GRID_IMAGE_SMALL = New Bitmap(breite, 3)
                ReDim _GRID_BYTES_SMALL(_GRID_IMAGE_SMALL.Width * _GRID_IMAGE_SMALL.Height * 4 - 1)
            End If

            For i As Integer = 0 To _GRID_BYTES_SMALL.Length - 1 Step 4
                _GRID_BYTES_SMALL(i) = 255 'B
                _GRID_BYTES_SMALL(i + 1) = 255 'G
                _GRID_BYTES_SMALL(i + 2) = 255 'R
                _GRID_BYTES_SMALL(i + 3) = 255 'A
            Next
            For i As Integer = 0 To _GRID_BYTES_LARGE.Length - 1 Step 4
                _GRID_BYTES_LARGE(i) = 255 'B
                _GRID_BYTES_LARGE(i + 1) = 255 'G
                _GRID_BYTES_LARGE(i + 2) = 255 'R
                _GRID_BYTES_LARGE(i + 3) = 255 'A
            Next
            Dim startindex As Integer
            For i As Integer = start_X To ende_X Step Grid_Mult_X
                px = CInt(faktor * i + offsetX)
                startindex = px * 4
                If i Mod (2 * Grid_Mult_X) = 0 Then
                    'LARGE
                    If px >= 0 AndAlso px < breite Then
                        _GRID_BYTES_LARGE(startindex) = B
                        _GRID_BYTES_LARGE(startindex + 1) = G
                        _GRID_BYTES_LARGE(startindex + 2) = R

                        _GRID_BYTES_LARGE(startindex + bpl) = B
                        _GRID_BYTES_LARGE(startindex + bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 2 * bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 3 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 3 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 3 * bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 4 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 4 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 4 * bpl + 2) = R
                    End If
                    If px - 1 >= 0 AndAlso px - 1 < breite Then
                        _GRID_BYTES_LARGE(startindex - 4 + bpl) = B
                        _GRID_BYTES_LARGE(startindex - 4 + bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex - 4 + bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex - 4 + 3 * bpl) = B
                        _GRID_BYTES_LARGE(startindex - 4 + 3 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex - 4 + 3 * bpl + 2) = R
                    End If
                    If px + 1 >= 0 AndAlso px + 1 < breite Then
                        _GRID_BYTES_LARGE(startindex + 4 + bpl) = B
                        _GRID_BYTES_LARGE(startindex + 4 + bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 4 + bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 4 + 3 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 4 + 3 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 4 + 3 * bpl + 2) = R
                    End If
                    If px - 2 >= 0 AndAlso px - 2 < breite Then
                        _GRID_BYTES_LARGE(startindex - 8 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex - 8 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex - 8 + 2 * bpl + 2) = R
                    End If
                    If px + 2 >= 0 AndAlso px + 2 < breite Then
                        _GRID_BYTES_LARGE(startindex + 8 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 8 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 8 + 2 * bpl + 2) = R
                    End If
                Else
                    'Small
                    If px >= 0 AndAlso px < breite Then
                        _GRID_BYTES_LARGE(startindex + bpl) = B
                        _GRID_BYTES_LARGE(startindex + bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 2 * bpl + 2) = R

                        _GRID_BYTES_LARGE(startindex + 3 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 3 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 3 * bpl + 2) = R
                    End If
                    If px - 1 >= 0 AndAlso px - 1 < breite Then
                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex - 4 + 2 * bpl + 2) = R
                    End If
                    If px + 1 >= 0 AndAlso px + 1 < breite Then
                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl) = B
                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl + 1) = G
                        _GRID_BYTES_LARGE(startindex + 4 + 2 * bpl + 2) = R
                    End If
                End If
                If px >= 0 AndAlso px < breite Then
                    _GRID_BYTES_SMALL(startindex) = B
                    _GRID_BYTES_SMALL(startindex + 1) = G
                    _GRID_BYTES_SMALL(startindex + 2) = R

                    _GRID_BYTES_SMALL(startindex + bpl) = B
                    _GRID_BYTES_SMALL(startindex + bpl + 1) = G
                    _GRID_BYTES_SMALL(startindex + bpl + 2) = R

                    _GRID_BYTES_SMALL(startindex + 2 * bpl) = B
                    _GRID_BYTES_SMALL(startindex + 2 * bpl + 1) = G
                    _GRID_BYTES_SMALL(startindex + 2 * bpl + 2) = R
                End If
                If px - 1 >= 0 AndAlso px - 1 < breite Then
                    _GRID_BYTES_SMALL(startindex - 4 + bpl) = B
                    _GRID_BYTES_SMALL(startindex - 4 + bpl + 1) = G
                    _GRID_BYTES_SMALL(startindex - 4 + bpl + 2) = R
                End If
                If px + 1 >= 0 AndAlso px + 1 < breite Then
                    _GRID_BYTES_SMALL(startindex + 4 + bpl) = B
                    _GRID_BYTES_SMALL(startindex + 4 + bpl + 1) = G
                    _GRID_BYTES_SMALL(startindex + 4 + bpl + 2) = R
                End If
            Next

            Dim bmpLock = _GRID_IMAGE_SMALL.LockBits(New Rectangle(0, 0, _GRID_IMAGE_SMALL.Width, _GRID_IMAGE_SMALL.Height), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppArgb)
            Runtime.InteropServices.Marshal.Copy(_GRID_BYTES_SMALL, 0, bmpLock.Scan0, _GRID_BYTES_SMALL.Length)
            _GRID_IMAGE_SMALL.UnlockBits(bmpLock)

            bmpLock = _GRID_IMAGE_LARGE.LockBits(New Rectangle(0, 0, _GRID_IMAGE_LARGE.Width, _GRID_IMAGE_LARGE.Height), Imaging.ImageLockMode.WriteOnly, Imaging.PixelFormat.Format32bppArgb)
            Runtime.InteropServices.Marshal.Copy(_GRID_BYTES_LARGE, 0, bmpLock.Scan0, _GRID_BYTES_LARGE.Length)
            _GRID_IMAGE_LARGE.UnlockBits(bmpLock)

            e.Graphics.CompositingMode = Drawing2D.CompositingMode.SourceCopy
            For j As Integer = start_Y To ende_Y Step Grid_Mult_Y
                py = CInt(faktor * j + offsetY)
                If j Mod (2 * Grid_Mult_Y) = 0 Then
                    e.Graphics.DrawImageUnscaled(_GRID_IMAGE_LARGE, New Point(0, py - 2))
                Else
                    e.Graphics.DrawImageUnscaled(_GRID_IMAGE_SMALL, New Point(0, py - 1))
                End If
            Next
            e.Graphics.CompositingMode = Drawing2D.CompositingMode.SourceOver
        End If
    End Sub

    Public Function drawDots() As List(Of Zorder_DO_Grafik)
        Dim points As New Dictionary(Of Point, Tuple(Of Integer, Integer, Integer)) 'Pos, Anzahl Vorkommen, Min. Linestyle, Max. zOrder
        Dim points_el As New Dictionary(Of Point, Integer)

        Dim w As IWire
        Dim t As Tuple(Of Integer, Integer, Integer)
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is IWire Then
                w = DirectCast(myElemente(i), IWire)
                If points.ContainsKey(w.getStart) Then
                    t = points(w.getStart())
                    points(w.getStart) = New Tuple(Of Integer, Integer, Integer)(t.Item1 + 1, Math.Min(t.Item2, DirectCast(myElemente(i), ElementLinestyle).linestyle), i)
                Else
                    points.Add(w.getStart, New Tuple(Of Integer, Integer, Integer)(1, DirectCast(myElemente(i), ElementLinestyle).linestyle, i))
                End If
                If points.ContainsKey(w.getEnde) Then
                    t = points(w.getEnde())
                    points(w.getEnde) = New Tuple(Of Integer, Integer, Integer)(t.Item1 + 1, Math.Min(t.Item2, DirectCast(myElemente(i), ElementLinestyle).linestyle), i)
                Else
                    points.Add(w.getEnde, New Tuple(Of Integer, Integer, Integer)(1, DirectCast(myElemente(i), ElementLinestyle).linestyle, i))
                End If
            ElseIf TypeOf myElemente(i) Is Bauteil Then
                points_el.Clear()
                For k As Integer = 0 To DirectCast(myElemente(i), Bauteil).NrOfSnappoints() - 1
                    If Not DirectCast(myElemente(i), Bauteil).getSnappoint(k).KeinWireDot Then
                        Dim p As Point = DirectCast(myElemente(i), Bauteil).getSnappoint(k).p
                        If Not points_el.ContainsKey(p) Then
                            points_el.Add(p, 1)
                            If points.ContainsKey(p) Then
                                t = points(p)
                                points(p) = New Tuple(Of Integer, Integer, Integer)(t.Item1 + 1, Math.Min(t.Item2, DirectCast(myElemente(i), Bauteil).linestyle), i)
                            Else
                                points.Add(p, New Tuple(Of Integer, Integer, Integer)(1, DirectCast(myElemente(i), Bauteil).linestyle, i))
                            End If
                        End If
                    End If
                Next
            End If
        Next

        Dim grafik As New List(Of Zorder_DO_Grafik)
        For Each pair As KeyValuePair(Of Point, Tuple(Of Integer, Integer, Integer)) In points
            If pair.Value.Item1 > 2 Then
                grafik.Add(New Zorder_DO_Grafik(New DO_Dot(pair.Key, RADIUS_DOT, RADIUS_DOT, pair.Value.Item2, True), pair.Value.Item3))
            End If
        Next
        grafik.Sort()
        Return grafik
    End Function

    Public Sub drawCursorAtPosition(e As PaintEventArgs, p As Point, style As CursorStyle, isMainCursor As Boolean)
        Dim pos As PointF = toPictureboxPoint(p)
        Dim pen As Pen = Pens.Red

        If style = CursorStyle.Circle Then
            e.Graphics.DrawEllipse(pen, pos.X - CURSORBREITE / 2, pos.Y - CURSORHÖHE / 2, CURSORBREITE, CURSORHÖHE)
        ElseIf style = CursorStyle.Rectangle Then
            e.Graphics.DrawRectangle(pen, pos.X - CURSORBREITE / 2, pos.Y - CURSORHÖHE / 2, CURSORBREITE, CURSORHÖHE)
        ElseIf style = CursorStyle.FatCross Then
            Dim pen_Fat As New Pen(Color.Red, 3)
            e.Graphics.DrawLine(pen_Fat, pos.X - CURSORBREITE / 2, pos.Y + CURSORHÖHE / 2, pos.X + CURSORBREITE / 2, pos.Y - CURSORHÖHE / 2)
            e.Graphics.DrawLine(pen_Fat, pos.X - CURSORBREITE / 2, pos.Y - CURSORHÖHE / 2, pos.X + CURSORBREITE / 2, pos.Y + CURSORHÖHE / 2)
        Else
            Throw New NotImplementedException("Unbekannter Cursor Style")
        End If

        If isMainCursor Then
            If myTools.Count > 0 Then
                myTools.Peek().OnDrawCursorExtension(Me, New PaintCursorEventArgs(e.Graphics, e.ClipRectangle, pos, CURSORBREITE, CURSORHÖHE, pen))
            End If
        End If

    End Sub

    Public Sub drawCrossCursorAtPosition(e As PaintEventArgs, p As Point)
        Dim pos As PointF = toPictureboxPoint(p)
        Dim pen As Pen = New Pen(Color.FromArgb(255, 255, 200, 200))

        e.Graphics.DrawLine(pen, 0, pos.Y, pos.X - (CURSORBREITE / 2 + CROSSCURSOR_EXTENTION), pos.Y)
        e.Graphics.DrawLine(pen, pos.X + (CURSORBREITE / 2 + CROSSCURSOR_EXTENTION), pos.Y, Me.Width, pos.Y)
        e.Graphics.DrawLine(pen, pos.X, 0, pos.X, pos.Y - (CURSORBREITE / 2 + CROSSCURSOR_EXTENTION))
        e.Graphics.DrawLine(pen, pos.X, pos.Y + (CURSORBREITE / 2 + CROSSCURSOR_EXTENTION), pos.X, Me.Height)
    End Sub

    Public Sub löscheEmptyWires()
        Dim w As Wire
        Dim wl As WireLuftlinie
        For i As Integer = myElemente.Count - 1 To 0 Step -1
            If TypeOf myElemente(i) Is Wire Then
                w = DirectCast(myElemente(i), Wire)
                If w.vector.X = 0 AndAlso w.vector.Y = 0 Then
                    myElemente.RemoveAt(i)
                End If
            ElseIf TypeOf myElemente(i) Is WireLuftlinie Then
                wl = DirectCast(myElemente(i), WireLuftlinie)
                If wl.vector.X = 0 AndAlso wl.vector.Y = 0 Then
                    myElemente.RemoveAt(i)
                End If
            End If
        Next
    End Sub

    Public Sub simplifyWires()
        löscheEmptyWires()

        Dim wi, wj As Wire
        Dim wli, wlj As WireLuftlinie

        Dim i, j As Integer

        'Fall 1: Schnittpunkt zwischen Wire-Ende und anderem Wire!
        'Aufteilen der Wires in 2 Stück
        i = 0
        While i < myElemente.Count
            If TypeOf myElemente(i) Is Wire Then
                wi = DirectCast(myElemente(i), Wire)
                j = 0
                While j < myElemente.Count
                    If j <> i Then
                        If TypeOf myElemente(j) Is Wire Then
                            wj = DirectCast(myElemente(j), Wire)

                            LöseSchnittpunkt(wj.getStart(), wi)
                            LöseSchnittpunkt(wj.getEnde(), wi)
                        ElseIf TypeOf myElemente(j) Is Bauteil OrElse TypeOf myElemente(j) Is WireLuftlinie Then
                            For k As Integer = 0 To DirectCast(myElemente(j), Element).NrOfSnappoints - 1
                                LöseSchnittpunkt(DirectCast(myElemente(j), Element).getSnappoint(k).p, wi)
                            Next
                        End If
                    End If

                    j += 1
                End While
            End If
            i += 1
        End While

        'Fall 2: Löschen von gleichen Wires
        For i = myElemente.Count - 1 To 1 Step -1
            If TypeOf myElemente(i) Is Wire Then
                wi = DirectCast(myElemente(i), Wire)
                For j = i - 1 To 0 Step -1
                    If TypeOf myElemente(j) Is Wire Then
                        wj = DirectCast(myElemente(j), Wire)

                        If (wi.getStart = wj.getStart AndAlso wi.getEnde = wj.getEnde) OrElse
                           (wi.getEnde = wj.getStart AndAlso wi.getStart = wj.getEnde) Then

                            'Immer den löschen der nicht selected ist.
                            'Daher beim löschen von i-ten segment die selection auf segment j weitergeben!
                            If myElemente(i).hasSelection Then
                                DirectCast(myElemente(j), Element).isSelected = True
                            End If

                            'Pfeil kombinieren!
                            If wi.pfeilStart.pfeilArt > -1 Then
                                If wi.getStart() = wj.getStart() Then
                                    wj.pfeilStart = selectPreferedPfeil(wi.pfeilStart, wj.pfeilStart)
                                Else
                                    wj.pfeilEnde = selectPreferedPfeil(wi.pfeilStart, wj.pfeilEnde)
                                End If
                            End If
                            If wi.pfeilEnde.pfeilArt > -1 Then
                                If wi.getEnde = wj.getStart() Then
                                    wj.pfeilStart = selectPreferedPfeil(wi.pfeilEnde, wj.pfeilStart)
                                Else
                                    wj.pfeilEnde = selectPreferedPfeil(wi.pfeilEnde, wj.pfeilEnde)
                                End If
                            End If

                            myElemente.RemoveAt(i)
                            Exit For

                        End If

                    End If
                Next
            ElseIf TypeOf myElemente(i) Is WireLuftlinie Then
                wli = DirectCast(myElemente(i), WireLuftlinie)
                For j = i - 1 To 0 Step -1
                    If TypeOf myElemente(j) Is WireLuftlinie Then
                        wlj = DirectCast(myElemente(j), WireLuftlinie)

                        If (wli.getStart = wlj.getStart AndAlso wli.getEnde = wlj.getEnde) OrElse
                           (wli.getEnde = wlj.getStart AndAlso wli.getStart = wlj.getEnde) Then

                            'Immer den löschen der nicht selected ist.
                            'Daher beim löschen von i-ten segment die selection auf segment j weitergeben!
                            If myElemente(i).hasSelection Then
                                DirectCast(myElemente(j), Element).isSelected = True
                            End If

                            'Pfeil kombinieren!
                            If wli.pfeilStart.pfeilArt > -1 Then
                                If wli.getStart() = wlj.getStart() Then
                                    wlj.pfeilStart = selectPreferedPfeil(wli.pfeilStart, wlj.pfeilStart)
                                Else
                                    wlj.pfeilEnde = selectPreferedPfeil(wli.pfeilStart, wlj.pfeilEnde)
                                End If
                            End If
                            If wli.pfeilEnde.pfeilArt > -1 Then
                                If wli.getEnde = wlj.getStart() Then
                                    wlj.pfeilStart = selectPreferedPfeil(wli.pfeilEnde, wlj.pfeilStart)
                                Else
                                    wlj.pfeilEnde = selectPreferedPfeil(wli.pfeilEnde, wlj.pfeilEnde)
                                End If
                            End If

                            myElemente.RemoveAt(i)
                            Exit For

                        End If
                    End If
                Next
            End If
        Next

        'Alle Eckpunkte
        Dim points As New Dictionary(Of Point, Integer)
        Dim w As Wire
        For Each element In myElemente
            If TypeOf element Is Wire Then
                w = DirectCast(element, Wire)
                If points.ContainsKey(w.getStart) Then
                    points(w.getStart) = points(w.getStart) + 1
                Else
                    points.Add(w.getStart, 1)
                End If
                If points.ContainsKey(w.getEnde) Then
                    points(w.getEnde) = points(w.getEnde) + 1
                Else
                    points.Add(w.getEnde, 1)
                End If
            ElseIf TypeOf element Is element Then
                For k As Integer = 0 To DirectCast(element, Element).NrOfSnappoints - 1
                    Dim s As Snappoint = DirectCast(element, Element).getSnappoint(k)
                    If points.ContainsKey(s.p) Then
                        points(s.p) = points(s.p) + 1
                    Else
                        points.Add(s.p, 1)
                    End If
                Next
            End If
        Next

        'Fall 3: Zusammenfügen von übereinander liegenden Wires
        Dim start_i, ende_i As Point
        Dim start_j, ende_j As Point
        For i = myElemente.Count - 1 To 1 Step -1
            If TypeOf myElemente(i) Is Wire Then
                wi = DirectCast(myElemente(i), Wire)
                start_i = wi.getStart()
                ende_i = wi.getEnde()
                For j = i - 1 To 0 Step -1
                    If TypeOf myElemente(j) Is Wire Then
                        wj = DirectCast(myElemente(j), Wire)

                        If (wi.isSelected AndAlso wj.isSelected) OrElse
                           (wi.isSelected = False AndAlso wj.isSelected = False) Then
                            'Nur Teile zusammenfügen, die entweder beide markiert sind oder die gar nicht markiert sind!
                            start_j = wj.getStart()
                            ende_j = wj.getEnde()
                            If wj.AddIfPossible(wi, points) Then
                                myElemente.RemoveAt(i)
                                changeSnapingElementsWhenWiresAreCombined(start_i, ende_i, wj)
                                changeSnapingElementsWhenWiresAreCombined(start_j, ende_j, wj)
                                Exit For
                            End If

                        End If
                    End If
                Next
            End If
        Next

        'Fall 4: Schnittpunkt zwischen Wire-Ende und anderem Wire!
        'Aufteilen der Wires in 2 Stück
        i = 0
        While i < myElemente.Count
            If TypeOf myElemente(i) Is Wire Then
                wi = DirectCast(myElemente(i), Wire)
                j = 0
                While j < myElemente.Count
                    If i <> j Then
                        If TypeOf myElemente(j) Is Wire Then
                            wj = DirectCast(myElemente(j), Wire)

                            If (wi.vector.X = 0 AndAlso wj.vector.Y = 0) OrElse (wi.vector.Y = 0 AndAlso wj.vector.X = 0) Then
                                'Nur Auftrennen, wenn sich ein wire senkrecht dazu schneidet!
                                LöseSchnittpunkt(wj.getStart(), wi)
                                LöseSchnittpunkt(wj.getEnde(), wi)
                            End If
                        ElseIf TypeOf myElemente(j) Is Bauteil OrElse TypeOf myElemente(j) Is WireLuftlinie Then
                            For k As Integer = 0 To DirectCast(myElemente(j), Element).NrOfSnappoints - 1
                                LöseSchnittpunkt(DirectCast(myElemente(j), Element).getSnappoint(k).p, wi)
                            Next
                        End If
                    End If
                    j += 1
                End While
            End If
            i += 1
        End While

        'Letzer Schritt: Snapingpoints, die nicht an ein Wire gesnapt sind wieder auf das wire setzen (wenn pos stimmt)
        For Each e As ElementMaster In myElemente
            If TypeOf e Is SnapableElement Then
                For k As Integer = 0 To DirectCast(e, SnapableElement).getNrOfSnappoints() - 1
                    adjustWhenNotOnWire(DirectCast(e, SnapableElement), k)
                Next
            End If
        Next
    End Sub

    Private Sub adjustWhenNotOnWire(e As SnapableElement, index As Integer)
        Dim pos As WireSnappoint = e.getSnappoint(index)
        Dim wStart As Point
        Dim wEnde As Point
        For Each w As ElementMaster In myElemente
            If TypeOf w Is Wire OrElse TypeOf w Is WireLuftlinie Then
                If TypeOf w Is Wire Then
                    wStart = DirectCast(w, Wire).getStart()
                    wEnde = DirectCast(w, Wire).getEnde()
                Else
                    wStart = DirectCast(w, WireLuftlinie).getStart()
                    wEnde = DirectCast(w, WireLuftlinie).getEnde()
                End If
                If pos.liegtAufWire(wStart, wEnde) Then
                    Return 'Alles ist gut! Liegt auf einem Wire.
                End If
            End If
        Next
        'Wenn es bishier ankommt gibt es kein Wire wo es draufliegt...
        Dim posInt As Point = pos.getMitteInt()
        Dim posF As PointF = pos.getMitteF()
        Dim vectorVorher As Point = pos.getLastDirectionVector()
        Dim vectorWire As Point
        Dim cosAlphaScaled As Single
        Dim minNeu As WireSnappoint = Nothing
        Dim maxWinkel As Single = Single.MinValue
        Dim lWire As Single
        For Each w As ElementMaster In myElemente
            If TypeOf w Is Wire Then
                If DirectCast(w, Wire).liegtAufWire(posInt) Then
                    wStart = DirectCast(w, Wire).getStart()
                    wEnde = DirectCast(w, Wire).getEnde()

                    vectorWire = New Point(wEnde.X - wStart.X, wEnde.Y - wStart.Y)
                    cosAlphaScaled = (vectorWire.X * vectorVorher.X + vectorWire.Y * vectorVorher.Y)
                    'eigentlich muss noch durch Läenge(vectorWire) geteilt werden. Das ist aber immer gleich lang daher weglassen!
                    lWire = CSng(Math.Sqrt(vectorWire.X * vectorWire.X + vectorWire.Y * vectorWire.Y))
                    If lWire > 0 Then
                        cosAlphaScaled /= lWire
                    Else
                        cosAlphaScaled = 0 'Wenn das Wire in keine Richtung geht wird angenommen, dass der Winkel max. schelcht ist (90°), sodass andere bevorzugt werden!
                    End If
                    If cosAlphaScaled < 0 Then
                        'cos(Winkel zwischen den Vektoren) < 0 ==> Winkel > als 180° ==> besser drehen!
                        If -cosAlphaScaled > maxWinkel Then
                            maxWinkel = -cosAlphaScaled
                            minNeu = pos.setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(wEnde, wStart, 0, 0)
                        End If
                    Else
                        'Winkel < 180° alles ist gut!
                        If cosAlphaScaled > maxWinkel Then
                            maxWinkel = cosAlphaScaled
                            minNeu = pos.setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(wStart, wEnde, 0, 0)
                        End If
                    End If
                End If
            ElseIf TypeOf w Is WireLuftlinie Then
                If DirectCast(w, WireLuftlinie).abstand(posF) < 1.0 Then
                    wStart = DirectCast(w, WireLuftlinie).getStart()
                    wEnde = DirectCast(w, WireLuftlinie).getEnde()

                    vectorWire = New Point(wEnde.X - wStart.X, wEnde.Y - wStart.Y)
                    cosAlphaScaled = (vectorWire.X * vectorVorher.X + vectorWire.Y * vectorVorher.Y)
                    'eigentlich muss noch durch Läenge(vectorWire) geteilt werden. Das ist aber immer gleich lang daher weglassen!
                    lWire = CSng(Math.Sqrt(vectorWire.X * vectorWire.X + vectorWire.Y * vectorWire.Y))
                    If lWire > 0 Then
                        cosAlphaScaled /= lWire
                    Else
                        cosAlphaScaled = 0 'Wenn das Wire in keine Richtung geht wird angenommen, dass der Winkel max. schelcht ist (90°), sodass andere bevorzugt werden!
                    End If
                    If cosAlphaScaled < 0 Then
                        'cos(Winkel zwischen den Vektoren) < 0 ==> Winkel > als 180° ==> besser drehen!
                        If -cosAlphaScaled > maxWinkel Then
                            maxWinkel = -cosAlphaScaled
                            minNeu = pos.setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(wEnde, wStart, 0, 0)
                        End If
                    Else
                        'Winkel < 180° alles ist gut!
                        If cosAlphaScaled > maxWinkel Then
                            maxWinkel = cosAlphaScaled
                            minNeu = pos.setzeVerschobenenMittelpunktAufWireSoDassEsMöglichstGutPasst(wStart, wEnde, 0, 0)
                        End If
                    End If
                End If
            End If
        Next
        If minNeu IsNot Nothing Then
            e.setSnappoint(index, minNeu)
            Return
        End If
    End Sub

    Private Function selectPreferedPfeil(p1 As ParamArrow, p2 As ParamArrow) As ParamArrow
        If p1.pfeilArt > -1 AndAlso p2.pfeilArt > -1 Then
            If p1.pfeilArt <= p2.pfeilArt Then
                Return p1
            Else
                Return p2
            End If
        ElseIf p1.pfeilArt >= p2.pfeilArt Then
            Return p1
        Else
            Return p2
        End If
    End Function

    Private Sub LöseSchnittpunkt(p1 As Point, w As Wire)
        If w.liegtInMitteVonWire(p1) Then
            Dim pfeilEnde As ParamArrow = w.pfeilEnde
            Dim start As Point = w.getStart()
            Dim ende As Point = w.getEnde()

            'Prüfen ob ein Pfeil hier liegt! Wenn ja diesen mit aufteilen!
            Dim pos As WireSnappoint
            For i As Integer = 0 To myElemente.Count - 1
                If TypeOf myElemente(i) Is SnapableElement Then
                    For k As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                        pos = DirectCast(myElemente(i), SnapableElement).getSnappoint(k)
                        If pos.liegtAufWire(w.getStart(), w.getEnde()) Then
                            Dim invert As Boolean = False
                            If pos.wireStart = w.getEnde() Then
                                pos.flipIt()
                                invert = True
                            End If

                            Dim laenge1 As Integer
                            Dim laenge2 As Integer
                            If w.vector.X = 0 Then
                                laenge1 = Math.Abs(p1.Y - start.Y)
                                laenge2 = Math.Abs(p1.Y - ende.Y)
                            ElseIf w.vector.Y = 0 Then
                                laenge1 = Math.Abs(p1.X - start.X)
                                laenge2 = Math.Abs(p1.X - ende.X)
                            Else
                                Throw New Exception("Ein Wire sollte immer rechtwinklig sein!")
                            End If
                            If pos.alpha < laenge1 / (laenge1 + laenge2) Then
                                'liegt in erster Hälfte!
                                pos.alpha = pos.alpha * (laenge1 + laenge2) / laenge1
                                If pos.wireEnd = ende Then
                                    pos.wireEnd = p1
                                Else
                                    Throw New Exception("Das sollte nicht sein!")
                                End If
                            Else
                                'liegt in zweiter Hälfte!
                                pos.alpha = (pos.alpha * (laenge1 + laenge2) - laenge1) / laenge2
                                If pos.wireStart = start Then
                                    pos.wireStart = p1
                                Else
                                    Throw New Exception("Das sollte nicht sein!")
                                End If
                            End If

                            If invert Then
                                pos.flipIt()
                            End If
                        End If
                    Next
                End If
            Next

            w.vector = New Point(p1.X - start.X, p1.Y - start.Y)
            w.pfeilEnde = New ParamArrow(-1, 100) 'Pfeil am Ende vom ersten Stück löschen!

            Dim wNeu As New Wire(Me.getNewID(), p1, ende)
            wNeu.pfeilEnde = pfeilEnde
            wNeu.isSelected = w.isSelected
            wNeu.linestyle = w.linestyle
            myElemente.Add(wNeu)
        End If
    End Sub

    Private Sub changeSnapingElementsWhenWiresAreCombined(wStart As Point, wEnde As Point, w_res As Wire)
        Dim pos As WireSnappoint
        For i As Integer = 0 To myElemente.Count - 1
            If TypeOf myElemente(i) Is SnapableElement Then
                For k As Integer = 0 To DirectCast(myElemente(i), SnapableElement).getNrOfSnappoints() - 1
                    pos = DirectCast(myElemente(i), SnapableElement).getSnappoint(k)
                    If pos.liegtAufWire(wStart, wEnde) Then

                        Dim mitte As PointF = pos.getMitteF()
                        Dim vectorVorher As Point = pos.getVector()
                        If w_res.vector.X = 0 Then
                            pos.alpha = (mitte.Y - w_res.getStart().Y) / (w_res.getEnde().Y - w_res.getStart().Y)
                        Else
                            pos.alpha = (mitte.X - w_res.getStart().X) / (w_res.getEnde().X - w_res.getStart().X)
                        End If
                        pos.wireStart = w_res.getStart()
                        pos.wireEnd = w_res.getEnde()
                        Dim vectorNachher As Point = pos.getVector()
                        If vectorNachher.X * vectorVorher.X <= 0 AndAlso vectorVorher.Y * vectorNachher.Y <= 0 Then
                            pos.flipIt()
                        End If
                    End If
                Next
            End If
        Next
    End Sub

    Public Function toPictureboxPoint(p As Point) As PointF
        Return New PointF(faktor * p.X + offsetX, faktor * p.Y + offsetY)
    End Function

    Public Function toPictureboxScale(dist As Integer) As Single
        Return faktor * dist
    End Function

    Public Function toPictureboxScale(dist As Double) As Double
        Return faktor * dist
    End Function

    Public Sub setViewArgs(args As GrafikDrawArgs)
        args.faktorX = faktor
        args.faktorY = faktor
        args.offsetX = offsetX
        args.offsetY = offsetY
    End Sub

    Public Function calcPixelPerMM() As Single
        Return Me.faktor / MM_PER_INT
    End Function
#End Region

#Region "Tastatur"

    Public Sub KeyUpRaised(e As KeyEventArgs)
        If myTools.Count > 0 Then
            Dim tke As New ToolKeyEventArgs(e.KeyCode)
            myTools.Peek.KeyUp(Me, tke)
            If Not tke.Handled Then
                defaultOnKeyUp(e)
            End If
        Else
            defaultOnKeyUp(e)
        End If
    End Sub

    Private Sub defaultOnKeyUp(e As KeyEventArgs)
    End Sub

    Public Sub KeyDownRaised(e As KeyEventArgs)
        If myTools.Count > 0 Then
            Dim tke As New ToolKeyEventArgs(e.KeyCode)
            myTools.Peek.KeyDown(Me, tke)
            If Not tke.Handled Then
                defaultOnKeyDown(e)
            End If
        Else
            defaultOnKeyDown(e)
        End If
    End Sub

    Private Sub defaultOnKeyDown(e As KeyEventArgs)
        Dim k As Key_Settings = Key_Settings.getSettings()

        Dim invalidate As Boolean = False

        'If e.KeyCode = Keys.F1 Then
        '_DEBUG_markiereNetsInVerschiedeneneFarben()
        'End If

        If e.KeyCode = Keys.Left Then
            offsetX += KEY_SCROLL_OFFSET
            invalidate = True
        ElseIf e.KeyCode = Keys.Up Then
            offsetY += KEY_SCROLL_OFFSET
            invalidate = True
        ElseIf e.KeyCode = Keys.Right Then
            offsetX -= KEY_SCROLL_OFFSET
            invalidate = True
        ElseIf e.KeyCode = Keys.Down Then
            offsetY -= KEY_SCROLL_OFFSET
            invalidate = True
        ElseIf k.keyToolAbbrechen.isDown(e) Then
            If myTools.Count > 0 Then
                If Not myTools.Peek().abortAction(Me) Then
                    cancelCurrentTool()
                End If
            Else
                cancelCurrentTool()
            End If
        ElseIf k.keySchalteBeschriftungsPosDurch.isDown(e) Then
            schalteBeschriftungsPosDurch()
        ElseIf k.keyFitToScreen.isDown(e) Then
            fit_to_screen()
        ElseIf k.keyToolMove.isDown(e) Then
            startToolMove()
        ElseIf k.keyToolCopy.isDown(e) Then
            startToolCopy()
        ElseIf k.keyToolWire.isDown(e) Then
            startToolWire()
        ElseIf k.keyToolAddInstance.isDown(e) Then
            startToolPlace()
        ElseIf k.keyToolScale.isDown(e) Then
            startToolScale()
        ElseIf k.keyToolAddStrom.isDown(e) Then
            startToolAddCurrentArrow()
        ElseIf k.keyToolAddSpannung.isDown(e) Then
            startToolAddVoltageArrow()
        ElseIf k.keyToolAddLabel.isDown(e) Then
            startToolAddLabel()
        ElseIf k.keyToolAddBusTap.isDown(e) Then
            startToolAddBusTap()
        ElseIf k.keyToolAddImpedanceArrow.isDown(e) Then
            startToolAddImpedanceArrow()
        ElseIf k.keyMarkierungAufheben.isDown(e) Then
            deselect_All()
        ElseIf k.keyAllesMarkieren.isDown(e) Then
            selectAll()
        ElseIf k.keyToolDelete.isDown(e) Then
            Me.deleteOrStartToolDelete()
        ElseIf k.keyRoutingOptimieren.isDown(e) Then
            Me.RoutingVerbessern(True, True, Integer.MaxValue)
        ElseIf k.keyToolAddInstanceMirrorX.isDown(e) Then
            Me.dreheSelectedSpiegelnHorizontal(False, False)
        ElseIf k.keyToolAddInstanceMirrorY.isDown(e) Then
            Me.dreheSelectedSpiegelnVertikal(False, False)
        End If

        If invalidate Then
            updateCursorIfRequiered()
            Me.Invalidate()
        End If
    End Sub

    Private Sub updateCursorIfRequiered()
        Dim cursorPos = Me.PointToClient(Cursor.Position)
        If cursorPos.X >= 0 AndAlso cursorPos.Y >= 0 AndAlso cursorPos.X < Me.Width AndAlso cursorPos.Y < Me.Height Then
            If (Not _cursorInside) Then
                'Mouse Enter
                OnMouseEnter(EventArgs.Empty)
            Else
                Dim pos As Point = getPointAtPos(cursorPos)
                If pos.X <> _cursorPos.X OrElse pos.Y <> _cursorPos.Y Then
                    CursorMove(New CursorEventArgs(pos.X, pos.Y, MouseButtons.None))
                End If
            End If
        Else
            If _cursorInside Then
                'MouseLeave
                OnMouseLeave(EventArgs.Empty)
            End If
        End If
    End Sub

#End Region

#Region "Maus"
    Private _cursorPos As Point
    Private _cursorInside As Boolean

    Public Function GetCursorPos() As Point
        Dim pos As Point = Me.PointToClient(Cursor.Position)
        Return getPointAtPos(pos)
    End Function

    Public Function GetCursorPosOffgrid() As Point
        Dim pos As Point = Me.PointToClient(Cursor.Position)
        Return getPointAtPosOffgrid(pos)
    End Function

    Public Function isCursorInside() As Boolean
        Return _cursorInside
    End Function

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)
        Dim faktorNeu As Single
        If e.Delta > 0 Then
            faktorNeu = faktor * 1.1F
        Else
            faktorNeu = faktor / 1.1F
        End If
        Dim faktorAlt As Single = faktor
        Dim pos As Point = GetCursorPos()

        faktor = faktorNeu
        Dim oxNeu As Single = faktorAlt * pos.X + offsetX - faktor * pos.X
        Dim oyNeu As Single = faktorAlt * pos.Y + offsetY - faktor * pos.Y

        Me.offsetX = oxNeu
        Me.offsetY = oyNeu
        Me.Invalidate()
    End Sub

#Region "Internal Mouse Events"
    Protected Function getPointAtPos(pos As Point) As Point
        Dim mousePosition As Point = New Point(CInt((pos.X - offsetX) / faktor), CInt((pos.Y - offsetY) / faktor))
        'zum Grid fitten:
        mousePosition.X = fitToGridX(mousePosition.X) * GridX
        mousePosition.Y = fitToGridY(mousePosition.Y) * GridY
        Return mousePosition
    End Function

    Protected Function getPointAtPosOffgrid(pos As Point) As Point
        Dim mousePosition As Point = New Point(CInt((pos.X - offsetX) / faktor), CInt((pos.Y - offsetY) / faktor))
        Return mousePosition
    End Function

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        MyBase.OnMouseEnter(e)

        Dim pos As Point = Me.PointToClient(Cursor.Position)
        pos = getPointAtPos(pos)

        CursorMove(New CursorEventArgs(pos.X, pos.Y, MouseButtons.None))
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        Dim pos As Point = Me.PointToClient(Cursor.Position)

        pos = getPointAtPos(pos)

        CursorMove(New CursorEventArgs(pos.X, pos.Y, MouseButtons.None))
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)

        CursorLeave()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)

        Dim pos As Point = Me.PointToClient(Cursor.Position)
        pos = getPointAtPos(pos)

        CursorDown(New CursorEventArgs(pos.X, pos.Y, e.Button))
        Me.Select()
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)

        Dim pos As Point = Me.PointToClient(Cursor.Position)
        pos = getPointAtPos(pos)

        CursorUp(New CursorEventArgs(pos.X, pos.Y, e.Button))
    End Sub
#End Region

#Region "Cursor Events"
    Public Sub performNewCursorMove()
        Dim pos As Point = Me.PointToClient(Cursor.Position)
        pos = getPointAtPos(pos)
        _cursorPos.X = pos.X - 1 'Damit erzwungenerweise ein move ausgeführt wird!
        CursorMove(New CursorEventArgs(pos.X, pos.Y, MouseButtons.None))
    End Sub

    Protected Sub CursorDown(e As CursorEventArgs)
        If myTools.Count > 0 Then
            Dim tme As New ToolMouseEventArgs(New Point(e.X, e.Y), e.Button)
            myTools.Peek.MouseDown(Me, tme)
        End If
    End Sub

    Protected Sub CursorUp(e As CursorEventArgs)
        If myTools.Count > 0 Then
            Dim tme As New ToolMouseEventArgs(New Point(e.X, e.Y), e.Button)
            myTools.Peek.MouseUp(Me, tme)
        End If
    End Sub

    Protected Sub CursorMove(e As CursorEventArgs)
        If _cursorPos.X <> e.X OrElse _cursorPos.Y <> e.Y Then
            _cursorPos = New Point(e.X, e.Y)
            _cursorInside = True

            If myTools.Count > 0 Then
                Dim tme As New ToolMouseEventArgs(New Point(e.X, e.Y))
                myTools.Peek.MouseMove(Me, tme)
                myTools.Peek.MouseMoveOffgrid(Me, New ToolMouseMoveOffgridEventArgs(New Point(e.X, e.Y), True))
            End If

            Me.Invalidate()
        Else
            If myTools.Count > 0 Then
                Dim tme As New ToolMouseMoveOffgridEventArgs(New Point(e.X, e.Y), False)
                myTools.Peek.MouseMoveOffgrid(Me, tme)
            End If
        End If
    End Sub

    Protected Sub CursorLeave()
        _cursorInside = False
        Me.Invalidate()
    End Sub
#End Region

#End Region

#Region "Events"
    Public Event ToolInfoTextChanged(sender As Vektor_Picturebox, e As ToolInfoTextEventArgs)
    Public Event NeuesRückgängigElement(sender As Vektor_Picturebox, r As NeuesRückgängigEventArgs)
    Public Event SelectionChanged(sender As Vektor_Picturebox, e As EventArgs)
#End Region

    Public Enum SelectionMode
        SelectOnlyNewElements
        AddSelection
        SubtractFromSelection
    End Enum

    Public Enum Richtung
        Xplus = 0
        Yplus = 1
        Xminus = 2
        Yminus = 3
    End Enum

    Public Enum MoveRichtung
        AlleRichtungen
        NurRechtwinklig
    End Enum

    Public Enum CursorStyle
        Circle
        Rectangle
        FatCross
    End Enum
End Class

Public Class CursorEventArgs
    Inherits EventArgs

    Public X As Integer
    Public Y As Integer
    Public Button As MouseButtons
    Public Sub New(x As Integer, y As Integer, Button As MouseButtons)
        Me.X = x
        Me.Y = y

        Me.Button = Button
    End Sub
End Class

Public Class ToolInfoTextEventArgs
    Inherits EventArgs

    Public Text As String
    Public Sub New(t As String)
        Me.Text = t
    End Sub
End Class

Public Class NeuesRückgängigEventArgs
    Inherits EventArgs

    Public R As Rückgängig

    Public Sub New(r As Rückgängig)
        Me.R = r
    End Sub
End Class

Public Class WireVerschiebung
    Public start As Point
    Public ende As Point
    Public schieben As Integer
    Public Sub New(s As Point, e As Point, schieben As Integer)
        Me.start = s
        Me.ende = e
        Me.schieben = schieben
    End Sub
End Class
