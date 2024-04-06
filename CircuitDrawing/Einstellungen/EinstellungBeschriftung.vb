Public Class EinstellungBeschriftung
    Inherits ElementEinstellung

    Private b As Beschriftung
    Private variousText As Boolean = False
    Private textChanged As Boolean = False

    Private variousAbstand As Boolean = False
    Private variousQuer As Boolean = False

    Public changedAbstand As Boolean = False
    Public changedQuer As Boolean = False

    Private variousIndex As Boolean = False
    Private indexChanged As Boolean = False

    Private variousRot As Boolean = False
    Private RotChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _txt As TextBox
    Private _cmb As JoSiCombobox

    Private _txtAbstand As TextBox
    Private _txtQuer As TextBox

    Public Sub New(name As String, b As Beschriftung)
        MyBase.New(name)
        Me.b = b
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As EinstellungBeschriftung = DirectCast(e2, EinstellungBeschriftung)
        If b.text <> e.b.text Then
            variousText = True
        End If
        If b.positionIndex <> e.b.positionIndex Then
            variousIndex = True
        End If
        If b.textRot <> e.b.textRot Then
            variousRot = True
        End If
        If b.abstand <> e.b.abstand Then
            variousAbstand = True
        End If
        If b.abstandQuer <> e.b.abstandQuer Then
            variousQuer = True
        End If
    End Sub

    Public Function getNewValue(old As Beschriftung, ByRef changed As Boolean) As Beschriftung
        Dim newValue As Beschriftung = old
        If Me.textChanged Then
            newValue.text = b.text
            changed = True
        End If
        If Me.indexChanged Then
            newValue.positionIndex = b.positionIndex
            changed = True
        End If
        If Me.RotChanged Then
            newValue.textRot = b.textRot
            changed = True
        End If
        If Me.changedAbstand Then
            newValue.abstand = b.abstand
            changed = True
        End If
        If Me.changedQuer Then
            newValue.abstandQuer = b.abstandQuer
            changed = True
        End If
        Return newValue
    End Function

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, EinstellungBeschriftung)
            Me.b = .b
            Me.variousText = .variousText
            Me.textChanged = .textChanged

            Me.variousIndex = .variousIndex
            Me.indexChanged = .indexChanged

            Me.variousRot = .variousRot
            Me.RotChanged = .RotChanged

            Me.variousAbstand = .variousAbstand
            Me.changedAbstand = .changedAbstand

            Me.variousQuer = .variousQuer
            Me.changedQuer = .changedQuer

            If _txt IsNot Nothing Then
                If variousText Then _txt.Text = VARIOUS_STRING Else _txt.Text = Me.b.text
            End If

            If _cmb IsNot Nothing Then
                If variousRot Then
                    _cmb.Various = True
                    _cmb.SelectedIndex = 0
                Else
                    _cmb.Various = False
                    Select Case b.textRot
                        Case DO_Text.TextRotation.Normal
                            _cmb.SelectedIndex = 0
                        Case DO_Text.TextRotation.Rot90
                            _cmb.SelectedIndex = 1
                        Case DO_Text.TextRotation.Rot180
                            _cmb.SelectedIndex = 2
                        Case DO_Text.TextRotation.Rot270
                            _cmb.SelectedIndex = 3
                    End Select
                End If
            End If

            If _txtAbstand IsNot Nothing Then
                If variousAbstand Then _txtAbstand.Text = VARIOUS_STRING Else _txtAbstand.Text = Me.b.abstand.ToString()
            End If

            If _txtQuer IsNot Nothing Then
                If variousQuer Then _txtQuer.Text = VARIOUS_STRING Else _txtQuer.Text = Me.b.abstandQuer.ToString()
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(3)

        Dim txt1 As TextBox = Nothing
        Dim lbl1 As Label = Nothing
        Me.createLabelTextbox(lbl1, txt1, True)
        If variousText Then txt1.Text = VARIOUS_STRING Else txt1.Text = Me.b.text
        _txt = txt1
        lbl1.Text = "Text:"
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(txt1)
        liste.Add(zeile1)
        AddHandler txt1.TextChanged, AddressOf TextboxTextChanged

        Dim lbl2 As Label = Nothing
        Dim cmb2 As New JoSiCombobox()
        Me.createLabelCombobox(lbl2, cmb2)
        lbl2.Text = My.Resources.Strings.Einstellung_Textausrichtung
        cmb2.Items.Add(My.Resources.Strings.Rotation_Normal)
        cmb2.Items.Add(My.Resources.Strings.Rotation90)
        cmb2.Items.Add(My.Resources.Strings.Rotation180)
        cmb2.Items.Add(My.Resources.Strings.Rotation270)
        If variousRot Then
            cmb2.Various = True
            cmb2.SelectedIndex = 0
        Else
            Select Case b.textRot
                Case DO_Text.TextRotation.Normal
                    cmb2.SelectedIndex = 0
                Case DO_Text.TextRotation.Rot90
                    cmb2.SelectedIndex = 1
                Case DO_Text.TextRotation.Rot180
                    cmb2.SelectedIndex = 2
                Case DO_Text.TextRotation.Rot270
                    cmb2.SelectedIndex = 3
            End Select
        End If
        _cmb = cmb2
        Dim zeile2 As New List(Of Control)(2)
        zeile2.Add(lbl2)
        zeile2.Add(cmb2)
        liste.Add(zeile2)
        AddHandler cmb2.SelectedIndexChanged, AddressOf RotSelectedIndexChanged

        Dim lbl3 As Label = Nothing
        Dim txtAbstand As TextBox = Nothing
        Dim txtQuer As TextBox = Nothing
        Me.createLabelTextbox2(lbl3, txtAbstand, txtQuer, True)
        If variousAbstand Then txtAbstand.Text = VARIOUS_STRING Else txtAbstand.Text = b.abstand.ToString()
        If variousQuer Then txtQuer.Text = VARIOUS_STRING Else txtQuer.Text = b.abstandQuer.ToString()
        _txtAbstand = txtAbstand
        _txtQuer = txtQuer
        lbl3.Text = My.Resources.Strings.Einstellung_Abstand
        Dim zeile3 As New List(Of Control)(3)
        zeile3.Add(lbl3)
        zeile3.Add(txtAbstand)
        zeile3.Add(txtQuer)
        liste.Add(zeile3)
        AddHandler txtAbstand.TextChanged, AddressOf AbstandTextChanged
        AddHandler txtQuer.TextChanged, AddressOf QuerTextChanged

        Return Me.createGroupbox(liste)
    End Function

    Private Sub TextboxTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            b.text = txt.Text
            Me.textChanged = True
        End If
    End Sub

    Private Sub AbstandTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            Dim value As Integer
            If Integer.TryParse(txt.Text, value) Then
                Me.b.abstand = value
                Me.changedAbstand = True
            End If
        End If
    End Sub

    Private Sub QuerTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            Dim value As Integer
            If Integer.TryParse(txt.Text, value) Then
                Me.b.abstandQuer = value
                Me.changedQuer = True
            End If
        End If
    End Sub

    Private Sub RotSelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex <> -1 Then
                cmb.Various = False
                RotChanged = True
                Select Case cmb.SelectedIndex
                    Case 0
                        b.textRot = DO_Text.TextRotation.Normal
                        OnEinstellungLiveChanged()
                    Case 1
                        b.textRot = DO_Text.TextRotation.Rot90
                        OnEinstellungLiveChanged()
                    Case 2
                        b.textRot = DO_Text.TextRotation.Rot180
                        OnEinstellungLiveChanged()
                    Case 3
                        b.textRot = DO_Text.TextRotation.Rot270
                        OnEinstellungLiveChanged()
                End Select
            End If
        End If
    End Sub
End Class
