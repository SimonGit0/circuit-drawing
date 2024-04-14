Public Class Einstellung_Linienstil
    Inherits ElementEinstellung

    Private style As LineStyle

    Private variousDicke As Boolean = False
    Private neueDicke As Single
    Private dickeChanged As Boolean = False

    Private neueFarbe As Farbe
    Private variousFarbe As Boolean = False
    Private farbeChanged As Boolean = False

    Private aWert As Byte
    Private variousA As Boolean = False
    Private aWertChanged As Boolean = False

    Private variousDashStyle As Boolean = False
    Private neuerDashStyle As Integer
    Private DashStyleChanged As Boolean = False

    Private variousDashStyleScale As Boolean = False
    Private neuerDashStyleScale As Integer
    Private DashStyleScaleChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _dicke_txt As Textbox_mitUnit
    Private _aWert_txt As TextBox
    Private Color_Picturebox As PictureBox
    Private _cmbDash As DashStyleCombobox
    Private _txtScale As Textbox_mitUnit

    Public Sub New(name As String, nummer As Integer, liste As LineStyleList)
        MyBase.New(New Multi_Lang_String(name, Nothing))
        Me.style = liste.getLineStyle(nummer).copy()
        neueFarbe = Me.style.farbe
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Linienstil = DirectCast(e2, Einstellung_Linienstil)
        If e.style.Dicke <> Me.style.Dicke Then
            variousDicke = True
        End If
        If e.style.farbe.Color_R <> Me.style.farbe.Color_R OrElse
           e.style.farbe.Color_B <> Me.style.farbe.Color_B OrElse
           e.style.farbe.Color_G <> Me.style.farbe.Color_G Then
            variousFarbe = True
        End If
        If e.style.farbe.Color_A <> Me.style.farbe.Color_A Then
            variousA = True
        End If
        If e.style._DashStyle.art <> Me.style._DashStyle.art Then
            variousDashStyle = True
        End If
        If e.style._DashStyle.scale <> Me.style._DashStyle.scale Then
            variousDashStyleScale = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Linienstil)
            Me.style = .style

            Me.variousDicke = .variousDicke
            Me.neueDicke = .neueDicke
            Me.dickeChanged = .dickeChanged

            Me.neueFarbe = .neueFarbe
            Me.variousFarbe = .variousFarbe
            Me.farbeChanged = .farbeChanged

            Me.aWert = .aWert
            Me.variousA = .variousA
            Me.aWertChanged = .aWertChanged

            Me.variousDashStyle = .variousDashStyle
            Me.neuerDashStyle = .neuerDashStyle
            Me.DashStyleChanged = .DashStyleChanged

            Me.variousDashStyleScale = .variousDashStyleScale
            Me.neuerDashStyleScale = .neuerDashStyleScale
            Me.DashStyleScaleChanged = .DashStyleScaleChanged

            If _dicke_txt IsNot Nothing Then
                If variousDicke Then
                    _dicke_txt.setVarious()
                Else
                    _dicke_txt.setText_ohneUnit(style.Dicke.ToString())
                End If
            End If

            If _aWert_txt IsNot Nothing Then
                If variousA Then
                    _aWert_txt.Text = VARIOUS_STRING
                Else
                    _aWert_txt.Text = CInt(style.farbe.Color_A).ToString()
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

            If _cmbDash IsNot Nothing Then
                _cmbDash.SelectedIndex = style._DashStyle.art
                If variousDashStyle Then
                    _cmbDash.Various = True
                Else
                    _cmbDash.Various = False
                End If
            End If

            If _txtScale IsNot Nothing Then
                If variousDashStyleScale Then
                    _txtScale.setVarious()
                Else
                    _txtScale.setText_ohneUnit(style._DashStyle.scale.ToString())
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Function getNewLinienstil(old_linienstil As Integer, liste As LineStyleList, ByRef changed As Boolean, justOverwriteOldStyle As Boolean) As Integer
        If dickeChanged Or farbeChanged Or DashStyleChanged Or DashStyleScaleChanged Or aWertChanged Then
            changed = True
            If dickeChanged Then
                style.Dicke = neueDicke
            Else
                style.Dicke = liste.getLineStyle(old_linienstil).Dicke
            End If

            If farbeChanged AndAlso aWertChanged Then
                style.farbe = New Farbe(aWert, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf farbeChanged Then
                style.farbe = New Farbe(liste.getLineStyle(old_linienstil).farbe.Color_A, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf aWertChanged Then
                Dim farbeOld As Farbe = liste.getLineStyle(old_linienstil).farbe
                style.farbe = New Farbe(aWert, farbeOld.Color_R, farbeOld.Color_G, farbeOld.Color_B)
            Else
                style.farbe = liste.getLineStyle(old_linienstil).farbe
            End If

            If DashStyleChanged Then
                style._DashStyle.art = neuerDashStyle
            Else
                style._DashStyle.art = liste.getLineStyle(old_linienstil)._DashStyle.art
            End If

            If DashStyleScaleChanged Then
                style._DashStyle.scale = neuerDashStyleScale
            Else
                style._DashStyle.scale = liste.getLineStyle(old_linienstil)._DashStyle.scale
            End If

            If justOverwriteOldStyle Then
                liste.replaceStyle(old_linienstil, style.copy())
                Return old_linienstil
            Else
                Return liste.getNumberOfNewLinestyle(style)
            End If
        Else
            Return old_linienstil
        End If
    End Function

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(4)

        'Liniendicke
        Dim lbl_Dicke As Label = Nothing
        Dim txt_Dicke As Textbox_mitUnit = Nothing
        Me.createLabelTextbox(lbl_Dicke, txt_Dicke, True)
        lbl_Dicke.Text = My.Resources.Strings.Einstellung_Stärke
        txt_Dicke.unit = " mm"
        If variousDicke Then
            txt_Dicke.setVarious()
        Else
            txt_Dicke.setText_ohneUnit(style.Dicke.ToString())
        End If
        _dicke_txt = txt_Dicke
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl_Dicke)
        zeile1.Add(txt_Dicke)
        liste.Add(zeile1)
        AddHandler txt_Dicke.TextChanged, AddressOf DickeTextChanged


        'A-Wert
        Dim lblAWert As Label = Nothing
        Dim txtAWert As TextBox = Nothing
        Me.createLabelTextbox(lblAWert, txtAWert, True)
        lblAWert.Text = My.Resources.Strings.Einstellung_Deckkraft
        If variousA Then
            txtAWert.Text = VARIOUS_STRING
        Else
            txtAWert.Text = CInt(style.farbe.Color_A).ToString()
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
        neueFarbe = style.farbe
        btn_farbe.Text = My.Resources.Strings.Einstellung_Farbe_Ändern
        Dim zeile2 As New List(Of Control)(3)
        zeile2.Add(lblFarbe)
        zeile2.Add(pic_farbe)
        zeile2.Add(btn_farbe)
        liste.Add(zeile2)
        If variousFarbe Then
            pic_farbe.Image = My.Resources.NoColor24
        Else
            pic_farbe.BackColor = neueFarbe.toColorOhne_A()
        End If
        Me.Color_Picturebox = pic_farbe
        AddHandler btn_farbe.Click, AddressOf btn_farbe_Ändern_Click

        'DashStyle
        Dim lblDash As Label = Nothing
        Dim cmbDash As New DashStyleCombobox()
        Dim txtScale As Textbox_mitUnit = Nothing
        Me.createLabelComboboxTextbox(lblDash, cmbDash, txtScale, True)

        lblDash.Text = My.Resources.Strings.Einstellung_Stil
        cmbDash.SelectedIndex = style._DashStyle.art
        If variousDashStyle Then
            cmbDash.Various = True
        End If
        _cmbDash = cmbDash
        txtScale.unit = "%"
        If variousDashStyleScale Then
            txtScale.setVarious()
        Else
            txtScale.setText_ohneUnit(style._DashStyle.scale.ToString())
        End If
        _txtScale = txtScale

        Dim zeile3 As New List(Of Control)(3)
        zeile3.Add(lblDash)
        zeile3.Add(cmbDash)
        zeile3.Add(txtScale)
        liste.Add(zeile3)
        AddHandler cmbDash.SelectedIndexChanged, AddressOf DashStyleSelectedIndexChanged
        AddHandler txtScale.TextChanged, AddressOf DashStyleSizeTextChanged

        Return createGroupbox(liste)
    End Function

    Private Sub DickeTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Single
            If Single.TryParse(txt.getText_ohneUnit(), value) Then
                If value < 0 Then
                    value = 0
                End If
                neueDicke = value
                dickeChanged = True
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

    Private Sub DashStyleSelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As DashStyleCombobox = DirectCast(sender, DashStyleCombobox)
            If cmb.SelectedIndex <> -1 Then
                neuerDashStyle = cmb.SelectedIndex
                DashStyleChanged = True
                cmb.Various = False
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Private Sub DashStyleSizeTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Integer
            If Integer.TryParse(txt.getText_ohneUnit(), value) Then
                If value < 1 Then
                    value = 1
                End If
                neuerDashStyleScale = value
                DashStyleScaleChanged = True
            End If
        End If
    End Sub

End Class
