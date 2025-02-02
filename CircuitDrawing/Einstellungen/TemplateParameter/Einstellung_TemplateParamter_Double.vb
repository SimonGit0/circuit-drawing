﻿Public Class Einstellung_TemplateParameter_Double
    Inherits Einstellung_TemplateParam

    Private myParam As TemplateParameter_Double
    Public myNr As Double

    Private variousNr As Boolean = False
    Public nrChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _txtNr As Textbox_mitUnit

    Public Sub New(param As TemplateParameter_Double, nr As Double)
        MyBase.New(param.name)
        Me.myParam = param
        myNr = nr
    End Sub

    Public Overrides Function isSameParameter(e2 As Einstellung_TemplateParam) As Boolean
        If TypeOf e2 IsNot Einstellung_TemplateParameter_Double Then Return False
        Dim e2_i As Einstellung_TemplateParameter_Double = DirectCast(e2, Einstellung_TemplateParameter_Double)
        If Me.Name.get_ID() <> e2_i.Name.get_ID() Then Return False
        Return True
    End Function

    Public Overrides Sub CombineValues(e2 As ElementEinstellung, mode As combineModus)
        Dim e As Einstellung_TemplateParameter_Double = DirectCast(e2, Einstellung_TemplateParameter_Double)
        If myNr <> e.myNr Then
            variousNr = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_TemplateParameter_Double)
            Me.myParam = .myParam
            Me.myNr = .myNr
            Me.variousNr = .variousNr
            Me.nrChanged = .nrChanged

            If _txtNr IsNot Nothing Then
                If variousNr Then
                    _txtNr.setVarious()
                Else
                    _txtNr.setText_ohneUnit(myNr.ToString())
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
        Me.createLabelTextbox(lbl1, txt1, True)
        lbl1.Text = myParam.name.get_str() & ":"
        txt1.unit = myParam.unit
        If variousNr Then
            txt1.setVarious()
        Else
            txt1.setText_ohneUnit(myNr.ToString())
        End If
        Me._txtNr = txt1
        AddHandler txt1.TextChanged, AddressOf TextChanged
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(txt1)
        liste.Add(zeile1)

        Return liste
    End Function

    Private Sub TextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Double
            If Double.TryParse(txt.getText_ohneUnit(), value) Then
                myNr = value
                nrChanged = True
            End If
        End If
    End Sub
End Class
