Public Class Einstellung_Int
    Inherits ElementEinstellung

    Private name_lbl As String

    Public value As Integer
    Private various As Boolean = False
    Public changed As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Public myTxt As TextBox

    Public Sub New(name As String, name_lbl As String, value As Integer)
        MyBase.New(New Multi_Lang_String(name, Nothing))
        Me.name_lbl = name_lbl
        Me.value = value
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As Einstellung_Int = DirectCast(e2, Einstellung_Int)
        If e.value <> Me.value Then
            various = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, Einstellung_Int)
            value = .value
            various = .various
            changed = .changed
            If myTxt IsNot Nothing Then
                If various Then myTxt.Text = VARIOUS_STRING Else myTxt.Text = value.ToString()
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim txt1 As TextBox = Nothing
        Dim lbl1 As Label = Nothing
        Me.createLabelTextbox(lbl1, txt1, True)
        If various Then txt1.Text = VARIOUS_STRING Else txt1.Text = value.ToString()
        lbl1.Text = Me.name_lbl
        AddHandler txt1.TextChanged, AddressOf Value_Changed

        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(txt1)
        Dim liste As New List(Of List(Of Control))(1)
        liste.Add(zeile1)

        myTxt = txt1
        Return Me.createGroupbox(liste)
    End Function

    Private Sub Value_Changed(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            Dim value As Integer
            If Integer.TryParse(txt.Text, value) Then
                Me.value = value
                changed = True
            End If
        End If
    End Sub


End Class
