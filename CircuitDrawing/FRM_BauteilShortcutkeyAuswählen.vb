Public Class FRM_BauteilShortcutkeyAuswählen

    Private myBib As Bibliothek

    Public result As ShortcutKeySelectInstance

    Private myKey As ShortcutKey

    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
    End Sub

    Public Sub init(bib As Bibliothek)
        Me.myBib = bib
        For Each _namespace As KeyValuePair(Of String, BauteileNamespace) In bib
            ComboBox1.Items.Add(_namespace.Key)
        Next

        ComboBox1.SelectedIndex = 0

        myKey = Nothing
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        result = getSelectedElement()
        If result IsNot Nothing Then
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Dim ns As BauteileNamespace = myBib.getNamespace(ComboBox1.SelectedItem.ToString())
        ComboBox2.Items.Clear()
        Dim cells() As String = ns.getCellsNamen()
        For i As Integer = 0 To cells.Count - 1
            ComboBox2.Items.Add(cells(i))
        Next
        If cells.Count > 0 Then
            ComboBox2.SelectedIndex = 0
        End If
    End Sub

    Private Function getSelectedElement() As ShortcutKeySelectInstance
        Dim ns As BauteileNamespace = myBib.getNamespace(ComboBox1.SelectedItem.ToString())
        If ComboBox2.Items.Count = 0 Then
            MessageBox.Show(My.Resources.Strings.Bauteil_Nicht_Vorhanden, My.Resources.Strings.Fehler)
            Return Nothing
        End If
        Dim bt As String = ComboBox2.SelectedItem.ToString()

        If myKey Is Nothing Then
            MessageBox.Show(My.Resources.Strings.keinShortCutKeySelected, My.Resources.Strings.Fehler)
            Return Nothing
        End If

        Return New ShortcutKeySelectInstance(myKey, ns.Name, bt)
    End Function

    Private Sub TextBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Alt OrElse e.KeyCode = Keys.Control OrElse e.KeyCode = Keys.Shift OrElse e.KeyCode = Keys.ShiftKey OrElse e.KeyCode = Keys.RShiftKey OrElse e.KeyCode = Keys.LShiftKey OrElse e.KeyCode = Keys.ControlKey OrElse e.KeyCode = Keys.LControlKey OrElse e.KeyCode = Keys.RControlKey OrElse e.KeyCode = Keys.Menu OrElse e.KeyCode = Keys.LMenu OrElse e.KeyCode = Keys.RMenu OrElse
           e.KeyCode = Keys.Return OrElse e.KeyCode = Keys.NumLock OrElse e.KeyCode = Keys.Back OrElse e.KeyCode = Keys.Escape OrElse e.KeyCode = Keys.Print OrElse e.KeyCode = Keys.PrintScreen Then
            Return
        Else
            Dim sk As New ShortcutKey(e.KeyCode, My.Computer.Keyboard.CtrlKeyDown, My.Computer.Keyboard.ShiftKeyDown, My.Computer.Keyboard.AltKeyDown)
            If Key_Settings.getSettings().hasKey(sk) OrElse Key_Settings.getSettings().hasKey_ElementHinzufügen(sk) Then
                MessageBox.Show(My.Resources.Strings.KeySchonBenutzt, My.Resources.Strings.Fehler)
            Else
                Me.myKey = sk
                TextBox1.Text = sk.getMenuString()
            End If
        End If
    End Sub
End Class