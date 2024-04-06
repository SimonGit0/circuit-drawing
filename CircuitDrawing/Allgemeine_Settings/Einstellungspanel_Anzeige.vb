Public Class Einstellungspanel_Anzeige
    Implements IEinstellungspanel

    Public Sub New()
        InitializeComponent()
        NumericUpDown2.Maximum = Settings.MAX_GRAVITY_STÄRKE
    End Sub

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().Raster = 50
        Settings.getSettings().Gravity_Stärke = 20
        Settings.getSettings().Gravity = True
        Settings.getSettings().CrossCursor = False
        Settings.getSettings().SnappointsImmerAnzeigen = False
        Settings.getSettings().RandAnzeigen = False
        Settings.getSettings().TextVorschauMode = False
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        With Settings.getSettings()
            NumericUpDown1.Value = .Raster
            CheckBox1.Checked = .Gravity
            NumericUpDown2.Value = .Gravity_Stärke
            CheckBox2.Checked = .CrossCursor
            CheckBox3.Checked = .SnappointsImmerAnzeigen
            CheckBox4.Checked = .RandAnzeigen
            CheckBox5.Checked = .TextVorschauMode
        End With
    End Sub

    Public Sub OnShown() Implements IEinstellungspanel.OnShown
        'braucht keine spezielle Initialisierung wenn es angezeigt wird!
    End Sub

    Public Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean Implements IEinstellungspanel.SpeicherValues
        With Settings.getSettings()
            .Raster = CInt(NumericUpDown1.Value)
            args.Vektor_Picturebox.GridX = .Raster
            args.Vektor_Picturebox.GridY = .Raster
            .Gravity = CheckBox1.Checked
            .Gravity_Stärke = CInt(NumericUpDown2.Value)
            .CrossCursor = CheckBox2.Checked
            .SnappointsImmerAnzeigen = CheckBox3.Checked
            .RandAnzeigen = CheckBox4.Checked
            .TextVorschauMode = CheckBox5.Checked
        End With
        Return True
    End Function

    Public Function getPanel() As Panel Implements IEinstellungspanel.getPanel
        Return Panel1
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.Einstellungspanel_Anzeige
    End Function
End Class
