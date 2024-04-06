Public Class FRM_ImgSizeSelection

    Public W As Integer = 1
    Public H As Integer = 1
    Public transparent As Boolean = True

    Private bbox As Size
    Private noTextChangedEvent As Boolean

    Public Sub New(Bbox As Size, defaul_factor As Double)
        noTextChangedEvent = True
        InitializeComponent()
        Me.bbox = Bbox
        Txt_H.unit = " Pixel"
        Txt_W.unit = " Pixel"

        Txt_W.setText_ohneUnit(CStr(CInt(Bbox.Width * defaul_factor)))
        Txt_H.setText_ohneUnit(CStr(CInt(Bbox.Height * defaul_factor)))
        noTextChangedEvent = False
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.W = CInt(Txt_W.getText_ohneUnit)
        Me.H = CInt(Txt_H.getText_ohneUnit)
        Me.transparent = CheckBox1.Checked
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Txt_H_TextChanged(sender As Object, e As EventArgs) Handles Txt_H.TextChanged
        If Not noTextChangedEvent Then
            Dim str As String = Txt_H.getText_ohneUnit()
            Dim value As Integer = -1
            If Integer.TryParse(str, value) Then
                If value >= 1 Then
                    Dim factor As Double = value / bbox.Height
                    Dim w As Integer = CInt(bbox.Width * factor)
                    If w < 1 Then w = 1
                    noTextChangedEvent = True
                    Txt_W.setText_ohneUnit(w.ToString)
                    Txt_H.setText_ohneUnit(value.ToString())
                    noTextChangedEvent = False
                End If
            End If
        End If
    End Sub

    Private Sub Txt_W_TextChanged(sender As Object, e As EventArgs) Handles Txt_W.TextChanged
        If Not noTextChangedEvent Then
            Dim str As String = Txt_W.getText_ohneUnit()
            Dim value As Integer = -1
            If Integer.TryParse(str, value) Then
                If value >= 1 Then
                    Dim factor As Double = value / bbox.Width
                    Dim h As Integer = CInt(bbox.Height * factor)
                    If h < 1 Then h = 1
                    noTextChangedEvent = True
                    Txt_H.setText_ohneUnit(h.ToString)
                    Txt_W.setText_ohneUnit(value.ToString)
                    noTextChangedEvent = False
                End If
            End If
        End If
    End Sub
End Class