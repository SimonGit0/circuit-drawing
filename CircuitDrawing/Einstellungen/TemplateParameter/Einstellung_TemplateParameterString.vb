Public Class Einstellung_TemplateParameterString
    Inherits Einstellung_TemplateParam

    Private myParam As TemplateParameter_String
    Public myStr As String

    Private variousStr As Boolean = False
    Public txtChanged As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _txtNr As TextBox

    Public Sub New(param As TemplateParameter_String, str As String)
        MyBase.New(param.name)
        Me.myParam = param
        myStr = str
    End Sub

    Public Overrides Function isSameParameter(e2 As Einstellung_TemplateParam) As Boolean
        If TypeOf e2 IsNot Einstellung_TemplateParameterString Then Return False
        Dim e2_i As Einstellung_TemplateParameterString = DirectCast(e2, Einstellung_TemplateParameterString)
        If Me.Name <> e2_i.Name Then Return False
        Return True
    End Function

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_TemplateParameterString = DirectCast(e2, Einstellung_TemplateParameterString)
        If myStr <> e.myStr Then
            variousStr = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_TemplateParameterString)
            Me.myParam = .myParam
            Me.myStr = .myStr
            Me.variousStr = .variousStr
            Me.txtChanged = .txtChanged

            If _txtNr IsNot Nothing Then
                If variousStr Then
                    _txtNr.Text = VARIOUS_STRING
                Else
                    _txtNr.Text = myStr
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
        Dim txt1 As TextBox = Nothing
        Me.createLabelTextbox(lbl1, txt1, True)
        lbl1.Text = myParam.name & ":"
        If variousStr Then
            txt1.Text = VARIOUS_STRING
        Else
            txt1.Text = myStr
        End If
        Me._txtNr = txt1
        AddHandler txt1.KeyPress, AddressOf Txt1KeyPress
        AddHandler txt1.TextChanged, AddressOf TextChanged
        AddHandler txt1.MouseUp, AddressOf Txt1MouseUp
        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(txt1)
        liste.Add(zeile1)

        Return liste
    End Function

    Private Sub TextChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            If myParam.istErlaubt(txt.Text) Then
                myStr = txt.Text
                txtChanged = True
            Else
                nichtAufUIEingabenReagieren = True
                txt.Text = myStr
                Try
                    txt.Select(Me.myStartSel, Me.myLenSel)
                Catch ex As Exception
                End Try
                'Dim t1 As New ToolTip()
                't1.Show("Zeichen nicht erlaubt!", txt, 1000)
                nichtAufUIEingabenReagieren = False
            End If
        End If
    End Sub

    Private myStartSel, myLenSel As Integer
    Private Sub Txt1KeyPress(sender As Object, e As KeyPressEventArgs)
        myStartSel = _txtNr.SelectionStart
        myLenSel = _txtNr.SelectionLength
    End Sub

    Private Sub Txt1MouseUp(sender As Object, e As MouseEventArgs)
        myStartSel = _txtNr.SelectionStart
        myLenSel = _txtNr.SelectionLength
    End Sub
End Class
