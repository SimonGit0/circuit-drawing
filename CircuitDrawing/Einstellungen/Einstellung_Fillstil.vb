Public Class Einstellung_Fillstil
    Inherits ElementEinstellung

    Private style As FillStyle

    Private neueFarbe As Farbe
    Private variousFarbe As Boolean = False
    Private farbeChanged As Boolean = False

    Private aWert As Byte
    Private variousA As Boolean = False
    Private aWertChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private Color_Picturebox As PictureBox
    Private aWert_txt As TextBox

    Public Sub New(name As String, nummer As Integer, liste As FillStyleList)
        MyBase.New(New Multi_Lang_String(name, Nothing))
        Me.style = liste.getFillStyle(nummer).copy()
        neueFarbe = Me.style.farbe
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Fillstil = DirectCast(e2, Einstellung_Fillstil)
        If e.style.farbe.Color_R <> Me.style.farbe.Color_R OrElse
           e.style.farbe.Color_B <> Me.style.farbe.Color_B OrElse
           e.style.farbe.Color_G <> Me.style.farbe.Color_G Then
            variousFarbe = True
        End If
        If e.style.farbe.Color_A <> Me.style.farbe.Color_A Then
            variousA = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Fillstil)
            Me.neueFarbe = .neueFarbe
            Me.variousFarbe = .variousFarbe
            Me.farbeChanged = .farbeChanged

            Me.aWert = .aWert
            Me.variousA = .variousA
            Me.aWertChanged = .aWertChanged

            If Color_Picturebox IsNot Nothing Then
                If variousFarbe Then
                    Color_Picturebox.Image = My.Resources.NoColor24
                Else
                    Color_Picturebox.Image = Nothing
                    Color_Picturebox.BackColor = neueFarbe.toColorOhne_A()
                End If
            End If
            If aWert_txt IsNot Nothing Then
                If variousA Then
                    aWert_txt.Text = VARIOUS_STRING
                Else
                    aWert_txt.Text = CInt(style.farbe.Color_A).ToString()
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Function getNewFillstil(old_fillstil As Integer, liste As FillStyleList, ByRef changed As Boolean) As Integer
        If farbeChanged OrElse aWertChanged Then
            changed = True

            If farbeChanged AndAlso aWertChanged Then
                style.farbe = New Farbe(aWert, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf farbeChanged Then
                style.farbe = New Farbe(liste.getFillStyle(old_fillstil).farbe.Color_A, neueFarbe.Color_R, neueFarbe.Color_G, neueFarbe.Color_B)
            ElseIf aWertChanged Then
                Dim farbeOld As Farbe = liste.getFillStyle(old_fillstil).farbe
                style.farbe = New Farbe(aWert, farbeOld.Color_R, farbeOld.Color_G, farbeOld.Color_B)
            Else
                style.farbe = liste.getFillStyle(old_fillstil).farbe
            End If

            Return liste.getNumberOfNewFillStyle(style)
        Else
            Return old_fillstil
        End If
    End Function

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(2)

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
        Me.aWert_txt = txtAWert
        Dim zeile0 As New List(Of Control)(2)
        zeile0.Add(lblAWert)
        zeile0.Add(txtAWert)
        liste.Add(zeile0)
        AddHandler txtAWert.TextChanged, AddressOf AWertTextChanged

        'Farbe
        Dim lblFarbe As Label = Nothing
        Dim pic_farbe As PictureBox = Nothing
        Dim btn_farbe As Button = Nothing
        Me.createLabelColorPictureboxButton(lblFarbe, pic_farbe, btn_farbe)
        lblFarbe.Text = My.Resources.Strings.Einstellung_Farbe
        neueFarbe = style.farbe
        btn_farbe.Text = My.Resources.Strings.Einstellung_Farbe_Ändern
        Dim zeile1 As New List(Of Control)(3)
        zeile1.Add(lblFarbe)
        zeile1.Add(pic_farbe)
        zeile1.Add(btn_farbe)
        liste.Add(zeile1)
        If variousFarbe Then
            pic_farbe.Image = My.Resources.NoColor24
        Else
            pic_farbe.BackColor = neueFarbe.toColorOhne_A()
        End If
        Me.Color_Picturebox = pic_farbe
        AddHandler btn_farbe.Click, AddressOf btn_farbe_Ändern_Click

        Return createGroupbox(liste)
    End Function

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
