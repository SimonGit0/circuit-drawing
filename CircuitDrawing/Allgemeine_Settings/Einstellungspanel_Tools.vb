Public Class Einstellungspanel_Tools
    Implements IEinstellungspanel

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().BezierKurveAbweichung = 50
        Settings.getSettings().kursiverTextImMatheModus = False
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        Dim val As Integer = Settings.getSettings().BezierKurveAbweichung
        If val < NumericUpDown1.Minimum Then val = CInt(NumericUpDown1.Minimum)
        If val > NumericUpDown1.Maximum Then val = CInt(NumericUpDown1.Maximum)
        NumericUpDown1.Value = val
        CheckBox1.Checked = Settings.getSettings().kursiverTextImMatheModus
    End Sub

    Public Sub OnShown() Implements IEinstellungspanel.OnShown
        'Braucht keine speziellen Initialisierungen
    End Sub

    Public Function getPanel() As Panel Implements IEinstellungspanel.getPanel
        Return Panel1
    End Function

    Public Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean Implements IEinstellungspanel.SpeicherValues
        Settings.getSettings().BezierKurveAbweichung = CInt(NumericUpDown1.Value)
        Settings.getSettings().kursiverTextImMatheModus = CheckBox1.Checked
        Return True
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.Tools
    End Function
End Class
