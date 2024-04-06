Public Class Einstellung_SinglePfeilspitze
    Inherits ElementEinstellung

    Public pfeil As ParamArrow

    Public Changed As Boolean = False
    Private various As Boolean = False
    Public changedSize As Boolean = False
    Private variousSize As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private cmbPfeil As PfeilCombobox
    Private txtSize As Textbox_mitUnit

    Public Sub New(name As String, pfeil As ParamArrow)
        MyBase.New(name)
        Me.pfeil = pfeil.CopyPfeil()
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_SinglePfeilspitze = DirectCast(e2, Einstellung_SinglePfeilspitze)
        If Me.pfeil.pfeilArt <> e.pfeil.pfeilArt Then
            various = True
        End If
        If Me.pfeil.pfeilSize <> e.pfeil.pfeilSize Then
            variousSize = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_SinglePfeilspitze)
            Me.pfeil = .pfeil.CopyPfeil()
            Me.Changed = .Changed
            Me.various = .various
            Me.changedSize = .changedSize
            Me.variousSize = .variousSize

            If cmbPfeil IsNot Nothing Then
                If various Then
                    cmbPfeil.Various = True
                Else
                    cmbPfeil.Various = False
                    cmbPfeil.SelectedIndex = pfeil.pfeilArt
                End If
            End If

            If txtSize IsNot Nothing Then
                If variousSize Then
                    txtSize.setVarious()
                Else
                    txtSize.setText_ohneUnit(pfeil.pfeilSize.ToString())
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(1)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New PfeilCombobox(False)
        Dim txt1 As Textbox_mitUnit = Nothing
        createLabelComboboxTextbox(lbl1, cmb1, txt1, True)
        lbl1.Text = "Pfeil:"
        txt1.unit = "%"
        If various Then
            cmb1.Various = True
        Else
            cmb1.SelectedIndex = pfeil.pfeilArt
        End If
        If variousSize Then
            txt1.setVarious()
        Else
            txt1.setText_ohneUnit(pfeil.pfeilSize.ToString())
        End If
        cmbPfeil = cmb1
        txtSize = txt1
        AddHandler cmb1.SelectedIndexChanged, AddressOf SelectedIndexChanged
        AddHandler txt1.TextChanged, AddressOf TextChanged
        Dim zeile1 As New List(Of Control)(3)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        zeile1.Add(txt1)
        liste.Add(zeile1)

        Return createGroupbox(liste)
    End Function

    Public Sub SelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As PfeilCombobox = DirectCast(sender, PfeilCombobox)
            If cmb.SelectedIndex <> -1 Then
                Changed = True
                cmb.Various = False
                pfeil.pfeilArt = CShort(cmb.SelectedIndex)
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
                Me.pfeil.pfeilSize = CUShort(value)
                Me.changedSize = True
            End If
        End If
    End Sub
End Class
