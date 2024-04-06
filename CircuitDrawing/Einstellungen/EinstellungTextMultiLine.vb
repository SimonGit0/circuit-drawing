Public Class EinstellungTextMultiLine
    Inherits ElementEinstellung

    Public neuerText As String
    Public textChanged As Boolean = False
    Public variousText As Boolean = False

    Private nichtAufUIEingabenReagieren As Boolean = False
    Private _txt As TextBox

    Public Sub New(name As String, text As String)
        MyBase.New(name)
        Me.neuerText = text
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As EinstellungTextMultiLine = DirectCast(e2, EinstellungTextMultiLine)
        If e.neuerText <> Me.neuerText Then
            variousText = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, EinstellungTextMultiLine)
            Me.neuerText = .neuerText
            Me.textChanged = .textChanged
            Me.variousText = .variousText

            If _txt IsNot Nothing Then
                If Me.variousText Then
                    _txt.Text = VARIOUS_STRING
                Else
                    _txt.Text = neuerText
                End If
            End If

        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim txt1 As New TextBox()
        txt1.Location = New Point(MARGIN_GRB_LEFT, 0)
        txt1.Width = BREITE_INITIAL - MARGIN_GRB_LEFT - MARGIN_GRB_RIGHT
        txt1.Height = 150
        txt1.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
        txt1.Multiline = True
        txt1.Font = MyFont
        If Me.variousText Then
            txt1.Text = VARIOUS_STRING
        Else
            txt1.Text = neuerText
        End If
        _txt = txt1
        AddHandler txt1.TextChanged, AddressOf txtChanged

        Dim zeile1 As New List(Of Control)(1)
        zeile1.Add(txt1)

        Dim liste As New List(Of List(Of Control))(1)
        liste.Add(zeile1)

        Return Me.createGroupbox(liste)
    End Function

    Private Sub txtChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim txt As TextBox = DirectCast(sender, TextBox)
            If txt.Text <> neuerText Then
                neuerText = txt.Text
                textChanged = True
            End If
        End If
    End Sub
End Class
