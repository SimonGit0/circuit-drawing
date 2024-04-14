Public MustInherit Class ElementEinstellung

    Public Const DIST_BETWEEN_ELEMENTS As Integer = 1

    Public Const BREITE_INITIAL As Integer = 300

    Public Const MARGIN_GRB_TOP As Integer = 17 + DIST_BETWEEN_ELEMENTS
    Public Const MARGIN_GRB_LEFT As Integer = 6
    Public Const MARGIN_GRB_RIGHT As Integer = 4
    Public Const MARGIN_GRB_BOTTOM As Integer = 4

    Public Shared ReadOnly VARIOUS_STRING As String = My.Resources.Strings.Einstellung_VariousString

    Public Const TAB_X1 As Integer = 100
    Public Const TAB_X2 As Integer = 175
    Public ReadOnly Name As Multi_Lang_String

    Public Sub New(name As Multi_Lang_String)
        Me.Name = name
    End Sub

    Public MustOverride Sub CombineValues(e2 As ElementEinstellung)

    Public Shared MyFont As Font = New Font(SystemFonts.DefaultFont.FontFamily, 9.75F, Drawing.FontStyle.Regular)
    Public Shared MyFont_Header As Font = New Font(SystemFonts.DefaultFont.FontFamily, 9.75F, Drawing.FontStyle.Bold)


    Public MustOverride Function getGroupbox() As GroupBox

    Public MustOverride Sub aktualisiere(e2 As ElementEinstellung)

    Protected Function createGroupbox(listeOfElemente As List(Of List(Of Control))) As GroupBox
        Dim posY As Integer = MARGIN_GRB_TOP
        Dim neuesPosY As Integer = posY
        For i As Integer = 0 To listeOfElemente.Count - 1
            For j As Integer = 0 To listeOfElemente(i).Count - 1
                listeOfElemente(i)(j).Location = New Point(listeOfElemente(i)(j).Location.X, listeOfElemente(i)(j).Location.Y + posY)
                neuesPosY = Math.Max(neuesPosY, listeOfElemente(i)(j).Location.Y + listeOfElemente(i)(j).Height)
            Next
            posY = neuesPosY + DIST_BETWEEN_ELEMENTS
        Next

        Dim g As New GroupBox()
        g.Text = Name.get_str()
        g.Font = MyFont_Header
        g.Size = New Size(BREITE_INITIAL, posY + MARGIN_GRB_BOTTOM)
        For i As Integer = 0 To listeOfElemente.Count - 1
            For j As Integer = 0 To listeOfElemente(i).Count - 1
                g.Controls.Add(listeOfElemente(i)(j))
            Next
        Next
        g.Anchor = AnchorStyles.Right Or AnchorStyles.Left Or AnchorStyles.Top
        Return g
    End Function

    Protected Sub createLabelTextbox(ByRef ergLbl As Label, ByRef ergTxt As TextBox, addEinstellungChangedEvent As Boolean)
        ergTxt = New Textbox_Einstellungen()
        ergTxt.Location = New Point(TAB_X1, 0)
        ergTxt.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ergTxt.Width = BREITE_INITIAL - TAB_X1 - MARGIN_GRB_RIGHT
        ergTxt.Font = MyFont

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergTxt.Location.X - ergLbl.Location.X - 6, ergTxt.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        If addEinstellungChangedEvent Then
            Me.addEinstellungLiveChangedEvent(ergTxt)
        End If
    End Sub

    Protected Sub createLabelTextbox(ByRef ergLbl As Label, ByRef ergTxt As Textbox_mitUnit, addEinstellungChangedEvent As Boolean)
        ergTxt = New Textbox_mitUnit()
        ergTxt.Location = New Point(TAB_X1, 0)
        ergTxt.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ergTxt.Width = BREITE_INITIAL - TAB_X1 - MARGIN_GRB_RIGHT
        ergTxt.Font = MyFont

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergTxt.Location.X - ergLbl.Location.X - 6, ergTxt.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        If addEinstellungChangedEvent Then
            Me.addEinstellungLiveChangedEvent(ergTxt)
        End If
    End Sub

    Protected Sub createLabelTextbox2(ByRef ergLbl As Label, ByRef ergTxt As TextBox, ByRef ergTxt2 As TextBox, addEinstellungChangedEvent As Boolean)
        ergTxt = New TextBox_EinstellungParentEvent()
        ergTxt.Location = New Point(TAB_X1, 0)
        ergTxt.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergTxt.Width = CInt((BREITE_INITIAL - TAB_X1 - MARGIN_GRB_RIGHT - 3) / 2)
        ergTxt.Font = MyFont

        ergTxt2 = New TextBox_EinstellungParentEvent()
        ergTxt2.Location = New Point(ergTxt.Location.X + ergTxt.Width + 3, 0)
        ergTxt2.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergTxt2.Width = BREITE_INITIAL - ergTxt2.Location.X - MARGIN_GRB_RIGHT
        ergTxt2.Font = MyFont

        AddHandler DirectCast(ergTxt2, TextBox_EinstellungParentEvent).ParentWidthChanged, AddressOf txtParentWidthRechtsChanged
        AddHandler DirectCast(ergTxt, TextBox_EinstellungParentEvent).ParentWidthChanged, AddressOf txtParentWidthLinksChanged

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergTxt.Location.X - ergLbl.Location.X - 6, ergTxt.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        If addEinstellungChangedEvent Then
            Me.addEinstellungLiveChangedEvent(ergTxt)
            Me.addEinstellungLiveChangedEvent(ergTxt2)
        End If
    End Sub

    Private Sub txtParentWidthLinksChanged(sender As Object, new_width As Integer)
        If sender IsNot Nothing AndAlso TypeOf sender Is TextBox Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            txt.Width = CInt((new_width - TAB_X1 - MARGIN_GRB_RIGHT - 3) / 2)
            txt.Location = New Point(TAB_X1, txt.Location.Y)
        End If
    End Sub

    Private Sub txtParentWidthRechtsChanged(sender As Object, new_width As Integer)
        If sender IsNot Nothing AndAlso TypeOf sender Is TextBox Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            txt.Location = New Point(TAB_X1 + CInt((new_width - TAB_X1 - MARGIN_GRB_RIGHT - 3) / 2) + 3, txt.Location.Y)
            txt.Width = new_width - txt.Location.X - MARGIN_GRB_RIGHT
        End If
    End Sub

    Protected Sub createLabelColorPictureboxButton(ByRef ergLbl As Label, ByRef ergPic As PictureBox, ByRef ergBtn As Button)
        ergPic = New PictureBox()
        ergPic.Location = New Point(TAB_X1, 0)
        ergPic.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergPic.Width = 24
        ergPic.Height = 24

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergPic.Location.X - ergLbl.Location.X - 6, ergPic.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        ergBtn = New Button()

        ergBtn.Font = MyFont
        ergBtn.Location = New Point(ergPic.Location.X + ergPic.Width + 6, ergPic.Location.Y + (ergPic.Height - ergBtn.Height) \ 2)
        ergBtn.Width = BREITE_INITIAL - MARGIN_GRB_RIGHT - ergBtn.Location.X
        ergBtn.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
    End Sub

    Protected Sub createLabelCombobox(ByRef ergLbl As Label, ByVal ergCmb As JoSiCombobox)
        ergCmb.Location = New Point(TAB_X1, 0)
        ergCmb.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ergCmb.Width = BREITE_INITIAL - TAB_X1 - MARGIN_GRB_RIGHT
        ergCmb.Font = MyFont

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergCmb.Location.X - ergLbl.Location.X - 6, ergCmb.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True
    End Sub

    Protected Sub createLabelComboboxTextbox(ByRef ergLbl As Label, ByVal ergCmb As JoSiCombobox, ByRef ergTxt As TextBox, addEinstellungChangedEvent As Boolean)
        ergCmb.Location = New Point(TAB_X1, 0)
        ergCmb.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergCmb.Width = TAB_X2 - TAB_X1 - 6
        ergCmb.Font = MyFont

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergCmb.Location.X - ergLbl.Location.X - 6, ergCmb.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        ergTxt = New Textbox_Einstellungen()
        ergTxt.Font = MyFont
        ergTxt.Location = New Point(TAB_X2, 0)
        ergTxt.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ergTxt.Width = BREITE_INITIAL - TAB_X2 - MARGIN_GRB_RIGHT


        If addEinstellungChangedEvent Then
            Me.addEinstellungLiveChangedEvent(ergTxt)
        End If
    End Sub

    Protected Sub createLabelComboboxTextbox(ByRef ergLbl As Label, ByVal ergCmb As JoSiCombobox, ByRef ergTxt As Textbox_mitUnit, addEinstellungChangedEvent As Boolean)
        ergCmb.Location = New Point(TAB_X1, 0)
        ergCmb.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergCmb.Width = TAB_X2 - TAB_X1 - 6
        ergCmb.Font = MyFont

        ergLbl = New Label()
        ergLbl.Location = New Point(MARGIN_GRB_LEFT, 0)
        ergLbl.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        ergLbl.Font = MyFont
        ergLbl.AutoSize = False
        ergLbl.Size = New Size(ergCmb.Location.X - ergLbl.Location.X - 6, ergCmb.Height)
        ergLbl.TextAlign = ContentAlignment.MiddleLeft
        ergLbl.AutoEllipsis = True

        ergTxt = New Textbox_mitUnit()
        ergTxt.Font = MyFont
        ergTxt.Location = New Point(TAB_X2, 0)
        ergTxt.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        ergTxt.Width = BREITE_INITIAL - TAB_X2 - MARGIN_GRB_RIGHT


        If addEinstellungChangedEvent Then
            Me.addEinstellungLiveChangedEvent(ergTxt)
        End If
    End Sub

    Protected Sub addEinstellungLiveChangedEvent(txt As TextBox)
        AddHandler txt.KeyDown, AddressOf TxtKeyDown
    End Sub

    Private Sub TxtKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            OnEinstellungLiveChanged()
        End If
    End Sub

    Protected Sub OnEinstellungLiveChanged()
        RaiseEvent EinstellungLiveChanged(Me, EventArgs.Empty)
    End Sub

    Public Event EinstellungLiveChanged(sender As Object, e As EventArgs)
End Class
