Public Class Einstellung_Fontstyle
    Inherits ElementEinstellung

    Private OLDstyle As FontStyle

    Private neueFarbe As Farbe
    Private variousFarbe As Boolean = False
    Private farbeChanged As Boolean = False

    Private aWert As Byte
    Private variousA As Boolean = False
    Private aWertChanged As Boolean = False

    Private fontname As String
    Private variousFontName As Boolean = False
    Private fontNameChanged As Boolean = False

    Private fontSize As Single
    Private variousFontSize As Boolean = False
    Private fontSizeChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _cmbStyle As FontCombobox
    Private _txtSize As TextBox
    Private _aWert_txt As TextBox
    Private Color_Picturebox As PictureBox
    Public Sub New(name As String, style As Integer, liste As FontList)
        MyBase.New(name)
        Me.OLDstyle = liste.getFontStyle(style).copy()

        fontSize = Me.OLDstyle.getSizeInPoints()
        fontname = Me.OLDstyle.getFontName()
        aWert = Me.OLDstyle.farbe.Color_A
        neueFarbe = Me.OLDstyle.farbe
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Fontstyle = DirectCast(e2, Einstellung_Fontstyle)
        If e.OLDstyle.getFontName() <> Me.OLDstyle.getFontName() Then
            variousFontName = True
        End If
        If e.OLDstyle.farbe.Color_R <> Me.OLDstyle.farbe.Color_R OrElse
           e.OLDstyle.farbe.Color_B <> Me.OLDstyle.farbe.Color_B OrElse
           e.OLDstyle.farbe.Color_G <> Me.OLDstyle.farbe.Color_G Then
            variousFarbe = True
        End If
        If e.OLDstyle.farbe.Color_A <> Me.OLDstyle.farbe.Color_A Then
            variousA = True
        End If
        If e.OLDstyle.getSizeInPoints() <> Me.OLDstyle.getSizeInPoints() Then
            variousFontSize = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Fontstyle)
            Me.OLDstyle = .OLDstyle

            Me.variousFontName = .variousFontName
            Me.fontname = .fontname
            Me.fontNameChanged = .fontNameChanged

            Me.neueFarbe = .neueFarbe
            Me.variousFarbe = .variousFarbe
            Me.farbeChanged = .farbeChanged

            Me.aWert = .aWert
            Me.variousA = .variousA
            Me.aWertChanged = .aWertChanged

            Me.variousFontSize = .variousFontSize
            Me.fontSize = .fontSize
            Me.fontSizeChanged = .fontSizeChanged

            If _txtSize IsNot Nothing Then
                If variousFontSize Then
                    _txtSize.Text = VARIOUS_STRING
                Else
                    _txtSize.Text = fontSize.ToString()
                End If
            End If

            If _aWert_txt IsNot Nothing Then
                If variousA Then
                    _aWert_txt.Text = VARIOUS_STRING
                Else
                    _aWert_txt.Text = CInt(aWert).ToString()
                End If
            End If

            If Color_Picturebox IsNot Nothing Then
                If variousFarbe Then
                    Color_Picturebox.Image = My.Resources.NoColor24
                Else
                    Color_Picturebox.Image = Nothing
                    Color_Picturebox.BackColor = neueFarbe.toColorOhne_A()
                End If
            End If

            If _cmbStyle IsNot Nothing Then
                _cmbStyle.selectFont(fontname)
                If variousFontName Then
                    _cmbStyle.Various = True
                Else
                    _cmbStyle.Various = False
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Function getNewFontstyle(old_fontstyle As Integer, liste As FontList, ByRef changed As Boolean, justOverwriteOldStyle As Boolean) As Integer
        If fontNameChanged Or farbeChanged Or fontSizeChanged Or aWertChanged Then
            changed = True
            If fontNameChanged Then
                OLDstyle.setFontName(fontname)
            Else
                OLDstyle.setFontName(liste.getFontStyle(old_fontstyle).getFontName())
            End If

            If farbeChanged AndAlso aWertChanged Then
                OLDstyle.farbe = New Farbe(aWert, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf farbeChanged Then
                OLDstyle.farbe = New Farbe(liste.getFontStyle(old_fontstyle).farbe.Color_A, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf aWertChanged Then
                Dim farbeOld As Farbe = liste.getFontStyle(old_fontstyle).farbe
                OLDstyle.farbe = New Farbe(aWert, farbeOld.Color_R, farbeOld.Color_G, farbeOld.Color_B)
            Else
                OLDstyle.farbe = liste.getFontStyle(old_fontstyle).farbe
            End If

            If fontSizeChanged Then
                If fontSize < 1 Then
                    fontSize = 1
                    If _txtSize IsNot Nothing Then
                        _txtSize.Text = "1"
                    End If
                End If
                OLDstyle.setSizeInPoints(fontSize)
            Else
                OLDstyle.setSizeInPoints(liste.getFontStyle(old_fontstyle).getSizeInPoints())
            End If

            If justOverwriteOldStyle Then
                liste.replaceStyle(old_fontstyle, OLDstyle.copy())
                Return old_fontstyle
            Else
                Return liste.getNumberOfNewFontStyle(OLDstyle)
            End If
        Else
            Return old_fontstyle
        End If
    End Function

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(4)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New FontCombobox()
        Me.createLabelCombobox(lbl1, cmb1)
        lbl1.Text = My.Resources.Strings.Einstellung_FontArt
        cmb1.selectFont(OLDstyle.getFontName())
        cmb1.Various = variousFontName
        _cmbStyle = cmb1
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        AddHandler cmb1.SelectedIndexChanged, AddressOf FontNameSelectedIndexChanged
        liste.Add(zeile1)

        Dim lbl2 As Label = Nothing
        Dim txt2 As TextBox = Nothing
        Me.createLabelTextbox(lbl2, txt2, True)
        lbl2.Text = My.Resources.Strings.Einstellung_Fontsize
        If variousFontSize Then
            txt2.Text = VARIOUS_STRING
        Else
            txt2.Text = OLDstyle.getSizeInPoints().ToString()
        End If
        _txtSize = txt2
        Dim zeile2 As New List(Of Control)(2)
        zeile2.Add(lbl2)
        zeile2.Add(txt2)
        AddHandler txt2.TextChanged, AddressOf FontSizeTextChanged
        liste.Add(zeile2)

        'A-Wert
        Dim lblAWert As Label = Nothing
        Dim txtAWert As TextBox = Nothing
        Me.createLabelTextbox(lblAWert, txtAWert, True)
        lblAWert.Text = My.Resources.Strings.Einstellung_Deckkraft
        If variousA Then
            txtAWert.Text = VARIOUS_STRING
        Else
            txtAWert.Text = CInt(OLDstyle.farbe.Color_A).ToString()
        End If
        _aWert_txt = txtAWert
        Dim zeileA As New List(Of Control)(2)
        zeileA.Add(lblAWert)
        zeileA.Add(txtAWert)
        liste.Add(zeileA)
        AddHandler txtAWert.TextChanged, AddressOf AWertTextChanged

        'Farbe
        Dim lblFarbe As Label = Nothing
        Dim pic_farbe As PictureBox = Nothing
        Dim btn_farbe As Button = Nothing
        Me.createLabelColorPictureboxButton(lblFarbe, pic_farbe, btn_farbe)
        lblFarbe.Text = My.Resources.Strings.Einstellung_Farbe
        neueFarbe = OLDstyle.farbe
        btn_farbe.Text = My.Resources.Strings.Einstellung_Farbe_Ändern
        Dim zeile4 As New List(Of Control)(3)
        zeile4.Add(lblFarbe)
        zeile4.Add(pic_farbe)
        zeile4.Add(btn_farbe)
        liste.Add(zeile4)
        If variousFarbe Then
            pic_farbe.Image = My.Resources.NoColor24
        Else
            pic_farbe.BackColor = neueFarbe.toColorOhne_A()
        End If
        Me.Color_Picturebox = pic_farbe
        AddHandler btn_farbe.Click, AddressOf btn_farbe_Ändern_Click

        Return Me.createGroupbox(liste)
    End Function

    Private Sub FontNameSelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As FontCombobox = DirectCast(sender, FontCombobox)
            If cmb.SelectedIndex <> -1 Then
                fontname = cmb.getSelectedFontName()
                fontNameChanged = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Private Sub FontSizeTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            Dim value As Single
            If Single.TryParse(txt.Text, value) Then
                fontSize = value
                fontSizeChanged = True
            End If
        End If
    End Sub

    Private Sub btn_farbe_Ändern_Click(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim c As New ColorDialog()
            c.Color = neueFarbe.toColorOhne_A()
            If c.ShowDialog = DialogResult.OK Then
                Color_Picturebox.BackColor = c.Color
                Color_Picturebox.Image = Nothing

                neueFarbe.Color_B = c.Color.B
                neueFarbe.Color_R = c.Color.R
                neueFarbe.Color_G = c.Color.G
                farbeChanged = True
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Private Sub AWertTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            Dim value As Integer
            If Integer.TryParse(txt.Text, value) Then
                If value < 0 Then value = 0
                If value > 255 Then value = 255
                Me.aWert = CByte(value)

                aWertChanged = True
            End If
        End If
    End Sub

End Class
