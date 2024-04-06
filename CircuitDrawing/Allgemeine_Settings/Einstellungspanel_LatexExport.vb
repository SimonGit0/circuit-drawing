Public Class Einstellungspanel_LatexExport
    Implements IEinstellungspanel

    Public Sub New()
        InitializeComponent()

        ComboBox1.Items.Add("ANSI")
        ComboBox1.Items.Add("UTF8")
    End Sub

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().Encoding = Settings.myEncodings.UTF8
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        If Settings.getSettings().Encoding = Settings.myEncodings.ANSI Then
            ComboBox1.SelectedIndex = 0
        ElseIf Settings.getSettings().Encoding = Settings.myEncodings.UTF8 Then
            ComboBox1.SelectedIndex = 1
        Else
            Throw New Exception("Diese Kodierung wird nicht unterstützt.")
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
            Settings.getSettings().Encoding = Settings.myEncodings.ANSI
        ElseIf ComboBox1.SelectedIndex = 1 Then
            Settings.getSettings().Encoding = Settings.myEncodings.UTF8
        Else
            Throw New NotImplementedException("Diese Kodierung wird nicht unterstützt.")
        End If
        Return True
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.LatexExport
    End Function
End Class
