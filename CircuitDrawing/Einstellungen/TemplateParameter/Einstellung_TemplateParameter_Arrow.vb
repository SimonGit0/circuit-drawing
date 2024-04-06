Public Class Einstellung_TemplateParameter_Arrow
    Inherits Einstellung_TemplateParam

    Private myParam As TemplateParameter_Arrow
    Public myNr As ParamArrow

    Private variousNr As Boolean = False
    Public nrChanged As Boolean = False

    Private sizeVarious As Boolean = False
    Public sizeChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _cmbNr As JoSiCombobox
    Private _txt As Textbox_mitUnit

    Public Sub New(param As TemplateParameter_Arrow, nr As ParamArrow)
        MyBase.New(param.name)
        Me.myParam = param
        myNr = nr.CopyPfeil()
    End Sub

    Public Overrides Function isSameParameter(e2 As Einstellung_TemplateParam) As Boolean
        If TypeOf e2 IsNot Einstellung_TemplateParameter_Arrow Then Return False
        Dim e2_i As Einstellung_TemplateParameter_Arrow = DirectCast(e2, Einstellung_TemplateParameter_Arrow)
        If Me.Name <> e2_i.Name Then Return False
        If Me.myParam.intervall.min <> e2_i.myParam.intervall.min Then Return False
        If Me.myParam.intervall.max <> e2_i.myParam.intervall.max Then Return False
        If Me.myParam.intervall._step <> e2_i.myParam.intervall._step Then Return False
        Return True
    End Function

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_TemplateParameter_Arrow = DirectCast(e2, Einstellung_TemplateParameter_Arrow)
        If myNr.pfeilArt <> e.myNr.pfeilArt Then
            variousNr = True
        End If
        If myNr.pfeilSize <> e.myNr.pfeilSize Then
            sizeVarious = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_TemplateParameter_Arrow)
            Me.myParam = .myParam
            Me.myNr = .myNr.CopyPfeil()
            Me.variousNr = .variousNr
            Me.nrChanged = .nrChanged
            Me.sizeVarious = .sizeVarious
            Me.sizeChanged = .sizeChanged

            If _cmbNr IsNot Nothing Then
                _cmbNr.SelectedIndex = CInt(myNr.pfeilArt - myParam.intervall.min)
                _cmbNr.Various = variousNr
            End If

            If _txt IsNot Nothing Then
                If sizeVarious Then
                    _txt.setVarious()
                Else
                    _txt.setText_ohneUnit(myNr.pfeilSize.ToString())
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Return Me.createGroupbox(getControlListe())
    End Function

    Public Overrides Function getControlListe() As List(Of List(Of Control))
        Dim liste As New List(Of List(Of Control))(1)

        Dim lbl1 As Label = Nothing
        Dim txt1 As Textbox_mitUnit = Nothing
        Dim cmb1 As New PfeilCombobox(myParam.intervall.min, myParam.intervall.max)
        Me.createLabelComboboxTextbox(lbl1, cmb1, txt1, True)
        lbl1.Text = myParam.name & ":"
        txt1.unit = "%"
        cmb1.SelectedIndex = myNr.pfeilArt - myParam.intervall.min
        cmb1.Various = variousNr
        If sizeVarious Then
            txt1.setVarious()
        Else
            txt1.setText_ohneUnit(myNr.pfeilSize.ToString())
        End If
        _cmbNr = cmb1
        _txt = txt1
        AddHandler cmb1.SelectedIndexChanged, AddressOf SelectedIndexChanged
        AddHandler txt1.TextChanged, AddressOf TextChanged
        Dim zeile1 As New List(Of Control)(3)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        zeile1.Add(txt1)
        liste.Add(zeile1)

        Return liste
    End Function

    Private Sub SelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex <> -1 Then
                myNr.pfeilArt = CShort(cmb.SelectedIndex + myParam.intervall.min)
                cmb.Various = False
                nrChanged = True
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Private Sub TextChanged(sender As Object, e As EventArgs)
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
                Me.myNr.pfeilSize = CUShort(value)
                Me.sizeChanged = True
            End If
        End If
    End Sub
End Class
