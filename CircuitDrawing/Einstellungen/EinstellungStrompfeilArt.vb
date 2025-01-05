Public Class EinstellungStrompfeilArt
    Inherits ElementEinstellung
    Private nichtAufUIEingabenReagieren As Boolean = False

    Public myArt As SnapableCurrentArrow.StromPfeilArt
    Public pfeil As ParamArrow

    Private variousArt As Boolean = False
    Public changedArt As Boolean = False
    Public ChangedPfeil As Boolean = False
    Private variousPfeil As Boolean = False
    Public changedSize As Boolean = False
    Private variousSize As Boolean = False

    Private myCmb As JoSiCombobox
    Private cmbPfeil As PfeilCombobox
    Private txtSize As Textbox_mitUnit

    Public Sub New(name As String, myart As SnapableCurrentArrow.StromPfeilArt, pfeil As ParamArrow)
        MyBase.New(SortierTyp.ElementEinstellung_Speziell, New Multi_Lang_String(name, Nothing))
        Me.myArt = myart
        Me.pfeil = pfeil.CopyPfeil()
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung, mode As combineModus)
        Dim e As EinstellungStrompfeilArt = DirectCast(e2, EinstellungStrompfeilArt)
        If e.myArt <> Me.myArt Then
            variousArt = True
        End If
        If Me.pfeil.pfeilArt <> e.pfeil.pfeilArt Then
            variousPfeil = True
        End If
        If Me.pfeil.pfeilSize <> e.pfeil.pfeilSize Then
            variousSize = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, EinstellungStrompfeilArt)
            Me.myArt = .myArt
            Me.variousArt = .variousArt
            Me.changedArt = .changedArt
            If myCmb IsNot Nothing Then
                myCmb.Various = Me.variousArt
                If Me.myArt = SnapableCurrentArrow.StromPfeilArt.OnWire Then
                    myCmb.SelectedIndex = 0
                ElseIf myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire Then
                    myCmb.SelectedIndex = 1
                Else
                    Throw New Exception("Unbekannte Art: " + myArt.ToString())
                End If
            End If

            Me.pfeil = .pfeil.CopyPfeil()
            Me.ChangedPfeil = .ChangedPfeil
            Me.variousPfeil = .variousPfeil
            Me.changedSize = .changedSize
            Me.variousSize = .variousSize
            If cmbPfeil IsNot Nothing Then
                If variousPfeil Then
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
        Dim liste As New List(Of List(Of Control))(2)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New JoSiCombobox()
        Me.createLabelCombobox(lbl1, cmb1)
        lbl1.Text = My.Resources.Strings.Einstellung_StrompfeilArt
        cmb1.Items.Add(My.Resources.Strings.AufVerbinder)
        cmb1.Items.Add(My.Resources.Strings.NebenVerbinder)
        cmb1.Various = Me.variousArt
        If Me.myArt = SnapableCurrentArrow.StromPfeilArt.OnWire Then
            cmb1.SelectedIndex = 0
        ElseIf myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire Then
            cmb1.SelectedIndex = 1
        Else
            Throw New Exception("Unbekannte Art: " + myArt.ToString())
        End If
        myCmb = cmb1
        AddHandler cmb1.SelectedIndexChanged, AddressOf ArtChanged
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        liste.Add(zeile1)

        Dim lbl2 As Label = Nothing
        Dim cmb2 As New PfeilCombobox(False)
        Dim txt2 As Textbox_mitUnit = Nothing
        createLabelComboboxTextbox(lbl2, cmb2, txt2, True)
        lbl2.Text = My.Resources.Strings.Einstellung_Pfeilspitze & ":"
        txt2.unit = "%"
        If variousPfeil Then
            cmb2.Various = True
        Else
            cmb2.SelectedIndex = pfeil.pfeilArt
        End If
        If variousSize Then
            txt2.setVarious()
        Else
            txt2.setText_ohneUnit(pfeil.pfeilSize.ToString())
        End If
        cmbPfeil = cmb2
        txtSize = txt2
        AddHandler cmb2.SelectedIndexChanged, AddressOf SelectedIndexChanged
        AddHandler txt2.TextChanged, AddressOf TextChanged
        Dim zeile2 As New List(Of Control)(3)
        zeile2.Add(lbl2)
        zeile2.Add(cmb2)
        zeile2.Add(txt2)
        liste.Add(zeile2)

        Return Me.createGroupbox(liste)
    End Function

    Private Sub ArtChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex = 0 Then
                changedArt = True
                cmb.Various = False
                myArt = SnapableCurrentArrow.StromPfeilArt.OnWire
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 1 Then
                changedArt = True
                cmb.Various = False
                myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub

    Public Sub SelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As PfeilCombobox = DirectCast(sender, PfeilCombobox)
            If cmb.SelectedIndex <> -1 Then
                ChangedPfeil = True
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
