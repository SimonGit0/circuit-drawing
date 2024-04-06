Public Class Einstellungspanel_ShortcutKeysSelectBauteil
    Implements IEinstellungspanel

    Public bib As Bibliothek

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().KeysSelectInstance.Clear()
    End Sub

    Public Sub OnShown() Implements IEinstellungspanel.OnShown
        'braucht keine spezielle Initialisierung wenn es angezeigt wird!
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        UserDrawListbox1.Clear()
        For Each k As ShortcutKeySelectInstance In Settings.getSettings().KeysSelectInstance
            addKey(k)
        Next
        UserDrawListbox1_SelectedIndexChanged(Nothing, EventArgs.Empty)
    End Sub

    Private Sub addKey(k As ShortcutKeySelectInstance)
        Dim str As String = k.key.getMenuString() & "  ->  " & k._namespace & "." & k._cell
        Dim item As New ListboxItemText(str)
        item.Tag = k
        item.Height += 6
        UserDrawListbox1.addItem(item)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim frm As New FRM_BauteilShortcutkeyAuswählen()
        frm.init(bib)
        If frm.ShowDialog() = DialogResult.OK Then
            addKey(frm.result)
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If UserDrawListbox1.SelectedIndex >= 0 AndAlso UserDrawListbox1.SelectedIndex < UserDrawListbox1.Count Then
            UserDrawListbox1.RemoveItemAtIndex(UserDrawListbox1.SelectedIndex)
        End If
    End Sub

    Private Sub UserDrawListbox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles UserDrawListbox1.SelectedIndexChanged
        Button2.Enabled = UserDrawListbox1.SelectedIndex >= 0 AndAlso UserDrawListbox1.SelectedIndex < UserDrawListbox1.Count
    End Sub

    Public Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean Implements IEinstellungspanel.SpeicherValues
        Dim listeNeu As New List(Of ShortcutKeySelectInstance)(UserDrawListbox1.Count)
        For i As Integer = 0 To UserDrawListbox1.Count - 1
            listeNeu.Add(DirectCast(DirectCast(UserDrawListbox1.Items(i), ListboxItemText).Tag, ShortcutKeySelectInstance))
        Next
        Settings.getSettings().KeysSelectInstance.Clear()
        Settings.getSettings().KeysSelectInstance.AddRange(listeNeu)
        Return True
    End Function

    Public Function getPanel() As Panel Implements IEinstellungspanel.getPanel
        Return Panel1
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.Einstellungspanel_ShortcutKeyBauteile
    End Function

End Class

