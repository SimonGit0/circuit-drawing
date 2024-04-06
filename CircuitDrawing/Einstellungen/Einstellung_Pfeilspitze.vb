Public Class Einstellung_Pfeilspitze
    Inherits ElementEinstellung

    Public pfeilStart As ParamArrow
    Public pfeilEnde As ParamArrow

    Public startChanged As Boolean = False
    Private variousStart As Boolean = False

    Public startSizeChanged As Boolean = False
    Private startSizeVarious As Boolean = False

    Public endeChanged As Boolean = False
    Private variousEnde As Boolean = False

    Public endeSizeChanged As Boolean = False
    Private endeSizeVarious As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _cmbStart, _cmbEnde As PfeilCombobox
    Private _txtStart, _txtEnde As Textbox_mitUnit

    Public Sub New(name As String, pfeilStart As ParamArrow, pfeilEnde As ParamArrow)
        MyBase.New(name)
        Me.pfeilStart = pfeilStart.CopyPfeil()
        Me.pfeilEnde = pfeilEnde.CopyPfeil()
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Pfeilspitze = DirectCast(e2, Einstellung_Pfeilspitze)
        If Me.pfeilEnde.pfeilArt <> e.pfeilEnde.pfeilArt Then
            variousEnde = True
        End If
        If Me.pfeilEnde.pfeilSize <> e.pfeilEnde.pfeilSize Then
            endeSizeVarious = True
        End If
        If Me.pfeilStart.pfeilArt <> e.pfeilStart.pfeilArt Then
            variousStart = True
        End If
        If Me.pfeilStart.pfeilSize <> e.pfeilStart.pfeilSize Then
            startSizeVarious = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Pfeilspitze)
            Me.pfeilStart = .pfeilStart.CopyPfeil()
            Me.pfeilEnde = .pfeilEnde.CopyPfeil()
            Me.startChanged = .startChanged
            Me.variousStart = .variousStart
            Me.endeChanged = .endeChanged
            Me.variousEnde = .variousEnde
            Me.startSizeVarious = .startSizeVarious
            Me.endeSizeVarious = .endeSizeVarious
            Me.startSizeChanged = .startSizeChanged
            Me.endeSizeChanged = .endeSizeChanged

            If _cmbStart IsNot Nothing Then
                If variousStart Then
                    _cmbStart.Various = True
                Else
                    _cmbStart.Various = False
                    _cmbStart.SelectedIndex = pfeilStart.pfeilArt + 1
                End If
            End If

            If _cmbEnde IsNot Nothing Then
                If variousEnde Then
                    _cmbEnde.Various = True
                Else
                    _cmbEnde.Various = False
                    _cmbEnde.SelectedIndex = pfeilEnde.pfeilArt + 1
                End If
            End If

            If _txtStart IsNot Nothing Then
                If startSizeVarious Then
                    _txtStart.setVarious()
                Else
                    _txtStart.setText_ohneUnit(pfeilStart.pfeilSize.ToString())
                End If
            End If

            If _txtEnde IsNot Nothing Then
                If endeSizeVarious Then
                    _txtEnde.setVarious()
                Else
                    _txtEnde.setText_ohneUnit(pfeilEnde.pfeilSize.ToString())
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(2)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New PfeilCombobox(True)
        Dim txt1 As Textbox_mitUnit = Nothing
        createLabelComboboxTextbox(lbl1, cmb1, txt1, True)
        lbl1.Text = "Start:"
        txt1.unit = "%"
        If variousStart Then
            cmb1.Various = True
        Else
            cmb1.SelectedIndex = pfeilStart.pfeilArt + 1
        End If
        If startSizeVarious Then
            txt1.setVarious()
        Else
            txt1.setText_ohneUnit(pfeilStart.pfeilSize.ToString())
        End If
        _cmbStart = cmb1
        _txtStart = txt1
        AddHandler cmb1.SelectedIndexChanged, AddressOf StartSelectedIndexChanged
        AddHandler txt1.TextChanged, AddressOf StartSizeTextChanged
        Dim zeile1 As New List(Of Control)(3)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        zeile1.Add(txt1)
        liste.Add(zeile1)

        Dim lbl2 As Label = Nothing
        Dim cmb2 As New PfeilCombobox(True)
        Dim txt2 As Textbox_mitUnit = Nothing
        createLabelComboboxTextbox(lbl2, cmb2, txt2, True)
        lbl2.Text = My.Resources.Strings.Ende & ":"
        txt2.unit = "%"
        If variousEnde Then
            cmb2.Various = True
        Else
            cmb2.SelectedIndex = pfeilEnde.pfeilArt + 1
        End If
        If endeSizeVarious Then
            txt2.setVarious()
        Else
            txt2.setText_ohneUnit(pfeilEnde.pfeilSize.ToString())
        End If
        _cmbEnde = cmb2
        _txtEnde = txt2
        AddHandler cmb2.SelectedIndexChanged, AddressOf EndeSelectedIndexChanged
        AddHandler txt2.TextChanged, AddressOf EndeSizeTextChanged
        Dim zeile2 As New List(Of Control)(3)
        zeile2.Add(lbl2)
        zeile2.Add(cmb2)
        zeile2.Add(txt2)
        liste.Add(zeile2)

        Return createGroupbox(liste)
    End Function

    Public Sub StartSelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As PfeilCombobox = DirectCast(sender, PfeilCombobox)
            If cmb.SelectedIndex <> -1 Then
                startChanged = True
                cmb.Various = False
                pfeilStart.pfeilArt = CShort(cmb.SelectedIndex - 1)
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Public Sub EndeSelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As PfeilCombobox = DirectCast(sender, PfeilCombobox)
            If cmb.SelectedIndex <> -1 Then
                endeChanged = True
                cmb.Various = False
                pfeilEnde.pfeilArt = CShort(cmb.SelectedIndex - 1)
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Public Sub StartSizeTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Integer
            If Integer.TryParse(txt.getText_ohneUnit(), value) Then
                If value < 0 Then
                    value = 0
                End If
                If value > UShort.MaxValue Then
                    value = UShort.MaxValue
                End If
                Me.pfeilStart.pfeilSize = CUShort(value)
                startSizeChanged = True
            End If
        End If
    End Sub

    Public Sub EndeSizeTextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Integer
            If Integer.TryParse(txt.getText_ohneUnit(), value) Then
                If value < 0 Then
                    value = 0
                End If
                If value > UShort.MaxValue Then
                    value = UShort.MaxValue
                End If
                Me.pfeilEnde.pfeilSize = CUShort(value)
                endeSizeChanged = True
            End If
        End If
    End Sub
End Class
