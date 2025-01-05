Public Class Einstellung_TemplateParameter
    Inherits Einstellung_TemplateParam

    Private myParam As TemplateParameter_Param
    Public myNr As Integer

    Private variousNr As Boolean = False
    Public nrChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _cmbNr As JoSiCombobox

    Public Sub New(p As TemplateParameter_Param, nr As Integer)
        MyBase.New(p.name)
        Me.myParam = p
        myNr = nr
    End Sub

    Public Overrides Function isSameParameter(e2 As Einstellung_TemplateParam) As Boolean
        If TypeOf e2 IsNot Einstellung_TemplateParameter Then Return False
        Dim e2_i As Einstellung_TemplateParameter = DirectCast(e2, Einstellung_TemplateParameter)
        If Not myParam.name.is_equal(e2_i.myParam.name) Then Return False
        If myParam.options.Length <> e2_i.myParam.options.Length Then Return False
        For i As Integer = 0 To myParam.options.Length - 1
            If Not myParam.options(i).is_equal(e2_i.myParam.options(i)) Then Return False
        Next
        Return True
    End Function

    Public Overrides Sub CombineValues(e2 As ElementEinstellung, mode As combineModus)
        Dim e As Einstellung_TemplateParameter = DirectCast(e2, Einstellung_TemplateParameter)
        If myNr <> e.myNr Then
            variousNr = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_TemplateParameter)
            Me.myParam = .myParam
            Me.myNr = .myNr
            Me.variousNr = .variousNr
            Me.nrChanged = .nrChanged

            If _cmbNr IsNot Nothing Then
                _cmbNr.SelectedIndex = myNr
                _cmbNr.Various = variousNr
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
        Dim cmb1 As New JoSiCombobox
        Me.createLabelCombobox(lbl1, cmb1)
        lbl1.Text = myParam.name.get_str() & ":"
        For i As Integer = 0 To myParam.options.Length - 1
            cmb1.Items.Add(myParam.options(i).get_str())
        Next
        cmb1.SelectedIndex = myNr
        cmb1.Various = variousNr
        _cmbNr = cmb1
        AddHandler cmb1.SelectedIndexChanged, AddressOf SelectedIndexChanged
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        liste.Add(zeile1)

        Return liste
    End Function

    Private Sub SelectedIndexChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex <> -1 Then
                myNr = cmb.SelectedIndex
                cmb.Various = False
                nrChanged = True
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub
End Class
