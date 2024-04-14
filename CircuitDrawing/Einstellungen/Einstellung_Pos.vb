Public Class Einstellung_Pos
    Inherits ElementEinstellung

    Public pos As Point

    Private xString As String
    Private yString As String

    Private xUnit As String
    Private yUnit As String

    Private variousX As Boolean = False
    Private variousY As Boolean = False

    Public changedX As Boolean = False
    Public changedY As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private txt_X, txt_Y As Textbox_mitUnit

    Public Sub New(name As String, p As Point, xString As String, yString As String)
        Me.New(name, p, xString, yString, "", "")
    End Sub

    Public Sub New(name As String, p As Point, xString As String, yString As String, xUnit As String, yUnit As String)
        MyBase.New(New Multi_Lang_String(name, Nothing))
        Me.pos = p

        Me.xString = xString
        Me.yString = yString

        Me.xUnit = xUnit
        Me.yUnit = yUnit
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Pos = DirectCast(e2, Einstellung_Pos)
        If pos.X <> e.pos.X Then
            variousX = True
        End If
        If pos.Y <> e.pos.Y Then
            variousY = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Pos)
            Me.pos = .pos
            Me.variousX = .variousX
            Me.variousY = .variousY
            Me.changedX = .changedX
            Me.changedY = .changedY

            If txt_X IsNot Nothing Then
                If variousX Then txt_X.setVarious() Else txt_X.setText_ohneUnit(pos.X.ToString())
            End If

            If txt_Y IsNot Nothing Then
                If variousY Then txt_Y.setVarious() Else txt_Y.setText_ohneUnit(pos.Y.ToString())
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim txt1 As Textbox_mitUnit = Nothing
        Dim txt2 As Textbox_mitUnit = Nothing
        Dim lbl1 As Label = Nothing
        Dim lbl2 As Label = Nothing
        Me.createLabelTextbox(lbl1, txt1, True)
        Me.createLabelTextbox(lbl2, txt2, True)
        txt1.unit = xUnit
        txt2.unit = yUnit
        If variousX Then txt1.setVarious() Else txt1.setText_ohneUnit(pos.X.ToString())
        If variousY Then txt2.setVarious() Else txt2.setText_ohneUnit(pos.Y.ToString())
        lbl1.Text = Me.xString
        lbl2.Text = Me.yString
        txt_X = txt1
        txt_Y = txt2

        AddHandler txt1.TextChanged, AddressOf PosX_Changed
        AddHandler txt2.TextChanged, AddressOf PosY_Changed

        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(txt1)
        Dim zeile2 As New List(Of Control)(2)
        zeile2.Add(lbl2)
        zeile2.Add(txt2)
        Dim liste As New List(Of List(Of Control))(2)
        liste.Add(zeile1)
        liste.Add(zeile2)

        Return Me.createGroupbox(liste)
    End Function

    Private Sub PosX_Changed(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Integer
            If Integer.TryParse(txt.getText_ohneUnit(), value) Then
                Me.pos.X = value
                changedX = True
            End If
        End If
    End Sub

    Private Sub PosY_Changed(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As Textbox_mitUnit = DirectCast(sender, Textbox_mitUnit)
            Dim value As Integer
            If Integer.TryParse(txt.getText_ohneUnit(), value) Then
                Me.pos.Y = value
                changedY = True
            End If
        End If
    End Sub
End Class
