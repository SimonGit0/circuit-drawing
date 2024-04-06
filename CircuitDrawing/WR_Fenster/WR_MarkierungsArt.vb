Public Class WR_MarkierungsArt
    Private nichtAufUIEingabenReagieren As Boolean = False

    Public Property Select_Bauelemente As Boolean
        Get
            Return Ckb_Bauelemente.Checked
        End Get
        Set(value As Boolean)
            nichtAufUIEingabenReagieren = True
            Ckb_Bauelemente.Checked = value
            nichtAufUIEingabenReagieren = False
        End Set
    End Property

    Public Property Select_Wires As Boolean
        Get
            Return Ckb_Wire.Checked
        End Get
        Set(value As Boolean)
            nichtAufUIEingabenReagieren = True
            Ckb_Wire.Checked = value
            nichtAufUIEingabenReagieren = False
        End Set
    End Property

    Public Property Select_Beschriftung As Boolean
        Get
            Return Ckb_CurrentArrow.Checked
        End Get
        Set(value As Boolean)
            nichtAufUIEingabenReagieren = True
            Ckb_CurrentArrow.Checked = value
            nichtAufUIEingabenReagieren = False
        End Set
    End Property

    Public Property Select_Drawings As Boolean
        Get
            Return Ckb_Drawing.Checked
        End Get
        Set(value As Boolean)
            nichtAufUIEingabenReagieren = True
            Ckb_Drawing.Checked = value
            nichtAufUIEingabenReagieren = False
        End Set
    End Property

    Private Sub Ckb_Bauelemente_CheckedChanged(sender As Object, e As EventArgs) Handles Ckb_Bauelemente.CheckedChanged, Ckb_Wire.CheckedChanged, Ckb_CurrentArrow.CheckedChanged, Ckb_Drawing.CheckedChanged
        If Not nichtAufUIEingabenReagieren Then
            RaiseEvent CheckedChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Public Event CheckedChanged(sender As Object, e As EventArgs)

    Private Sub btn_AS_Click(sender As Object, e As EventArgs) Handles btn_AS.Click
        Select_Drawings = True
        Select_Beschriftung = True
        Select_Bauelemente = True
        Select_Wires = True

        RaiseEvent CheckedChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub Btn_NS_Click(sender As Object, e As EventArgs) Handles Btn_NS.Click
        Select_Drawings = False
        Select_Beschriftung = False
        Select_Bauelemente = False
        Select_Wires = False

        RaiseEvent CheckedChanged(Me, EventArgs.Empty)
    End Sub
End Class

Public Class NoKeyCheckbox
    Inherits CheckBox

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        'MyBase.OnKeyDown(e)
        e.Handled = True
    End Sub

End Class