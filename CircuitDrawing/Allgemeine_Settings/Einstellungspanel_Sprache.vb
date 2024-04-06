Public Class Einstellungspanel_Sprache
    Implements IEinstellungspanel

    Public Sub New()
        InitializeComponent()

        ComboBox1.Items.Add("Deutsch")
        ComboBox1.Items.Add("Englisch")
    End Sub

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().sprache = Vektorgrafik_Sprache.Deutsch
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        If Settings.getSettings().sprache = Vektorgrafik_Sprache.Deutsch Then
            ComboBox1.SelectedIndex = 0
        ElseIf Settings.getSettings().sprache = Vektorgrafik_Sprache.Englisch Then
            ComboBox1.SelectedIndex = 1
        Else
            Throw New Exception("Diese Sprache wird nicht unterstützt.")
        End If
    End Sub

    Public Sub OnShown() Implements IEinstellungspanel.OnShown
        'Braucht keine speziellen Initialisierungen
    End Sub

    Public Function getPanel() As Panel Implements IEinstellungspanel.getPanel
        Return Panel1
    End Function

    Public Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean Implements IEinstellungspanel.SpeicherValues
        If ComboBox1.SelectedIndex = 0 Then
            Settings.getSettings().sprache = Vektorgrafik_Sprache.Deutsch
        ElseIf ComboBox1.SelectedIndex = 1 Then
            Settings.getSettings().sprache = Vektorgrafik_Sprache.Englisch
        Else
            Throw New NotImplementedException("Diese Sprache wird nicht unterstützt.")
        End If
        Return True
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.Sprache
    End Function
End Class
