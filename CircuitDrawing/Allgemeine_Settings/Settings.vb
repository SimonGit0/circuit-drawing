Imports System.IO
Imports System.Xml
Public Class Settings
    Private Const _FileName As String = "settings.xml"

    Private Const SETTINGS_HEADER As String = "settings"

    Private Shared mySettings As Settings

    Private Const CadenceDetektTechnologie_NAME As String = "cadence_detect_technology"
    Public CadenceDetektTechnologie As String = ""

    Private Const CadenceDetekt_Terminal_Namespace_NAME As String = "cadence_detect_terminal_namespace"
    Public CadenceDetekt_Terminal_Namespace As String = "Basic"

    Private Const CadenceDetekt_Terminal_Name_NAME As String = "cadence_detect_terminal_name"
    Public CadenceDetekt_Terminal_Name As String = "Terminal"

    Private Const Pfade_Bib_NAME As String = "path_sym"
    Public Pfade_Bib() As String = {"ET"}

    Public Const Default_Params_NAME As String = "default_param"
    Public default_params() As default_Parameter = {New default_Parameter("*.style", "us")}

    Public Const Skill_Detect_removeFloatingElements_NAME As String = "text_detect_remove_floating_elements"
    Public Skill_Detect_removeFloatingElements As Boolean = True

    Public Const Skill_Detect_removeFloatingWires_NAME As String = "text_detect_remove_wire_stubs"
    Public Skill_Detect_removeFloatingWires As Boolean = True

    Public Const Skill_Detect_removeDummys_NAME As String = "text_detect_remove_dummys"
    Public Skill_Detect_removeDummys As Boolean = True

    Public Const Raster_NAME As String = "display_grid"
    Public Raster As Integer = 50

    Public Const Gravity_NAME As String = "gravity"
    Public Gravity As Boolean = True

    Public Const MAX_GRAVITY_STÄRKE As Integer = 100
    Public Const GravityStärke_NAME As String = "gravity_strength"
    Public Gravity_Stärke As Integer = 20

    Public Const Sprache_NAME As String = "language"
    Public sprache As Vektorgrafik_Sprache = Vektorgrafik_Sprache.Deutsch

    Public Const Encoding_NAME As String = "encoding"
    Public Encoding As myEncodings = myEncodings.UTF8

    Public Const SelectBauteil_NAME As String = "select_element"
    Public KeysSelectInstance As List(Of ShortcutKeySelectInstance)

    Public Const CrossCursor_NAME As String = "cross_cursor"
    Public CrossCursor As Boolean = False

    Public Const SnappointsImmerAnzeigen_NAME As String = "snappoints_always_visible"
    Public SnappointsImmerAnzeigen As Boolean = False

    Public Const RandAnzeigen_NAME As String = "show_boarder"
    Public RandAnzeigen As Boolean = False

    Public Const TextVorschauMode_NAME As String = "text_preview"
    Public TextVorschauMode As Boolean = False

    Public Const BezierKurveAbweichung_NAME As String = "bezier_deviation"
    Public BezierKurveAbweichung As Integer = 50

    Public Const MultiSelect_NAME As String = "multiselect"
    Public MultiSelect As Boolean = False

    Public Const kursiverTextImMatheModus_NAME As String = "math_mode_italic"
    Public kursiverTextImMatheModus As Boolean = False

    Public Shared lang_str As String = ""
    Private Sub New()
        KeysSelectInstance = New List(Of ShortcutKeySelectInstance)
    End Sub

    Public Shared Function getSettings() As Settings
        Return mySettings
    End Function

    Private Shared Function FileName() As String
        Return Application.StartupPath & "\" & _FileName
    End Function

    Public Function getFull_Pfade_Bib() As String()
        Dim erg(Pfade_Bib.Length - 1) As String
        Dim preString As String = Application.StartupPath() & "\"
        For i As Integer = 0 To erg.Length - 1
            erg(i) = preString & Pfade_Bib(i)
        Next
        Return erg
    End Function

    Public Shared Sub loadSettings()
        If File.Exists(FileName) Then
            Try
                mySettings = New Settings()
                mySettings.loadFromFile(FileName)
            Catch ex As Exception
                mySettings = New Settings()
            End Try
        Else
            mySettings = New Settings()
        End If
    End Sub

    Private Sub loadFromFile(filename As String)
        Dim reader As XmlTextReader = Nothing
        Try
            reader = New XmlTextReader(filename)
            Dim doc As New Xml.XmlDocument()
            doc.Load(reader)
            If doc.HasChildNodes() Then
                ReDim Pfade_Bib(-1)
                Dim default_parameterListe As New List(Of default_Parameter)
                Dim shortCutKeysSelectElement As New List(Of ShortcutKeySelectInstance)

                For i As Integer = 0 To doc.ChildNodes.Count - 1
                    If doc.ChildNodes(i).Name.ToLower = SETTINGS_HEADER Then
                        ladeSettings(doc.ChildNodes(i), default_parameterListe, shortCutKeysSelectElement)
                    End If
                Next

                Me.default_params = default_parameterListe.ToArray()
                Me.KeysSelectInstance = shortCutKeysSelectElement
            End If
        Catch ex As Exception
            Throw ex
        Finally
            If reader IsNot Nothing Then
                reader.Close()
            End If
        End Try
    End Sub

    Private Sub ladeSettings(node As XmlNode, default_parameterListe As List(Of default_Parameter), shortCutKeysSelectElement As List(Of ShortcutKeySelectInstance))
        If node.HasChildNodes Then
            For i As Integer = 0 To node.ChildNodes.Count - 1
                Select Case node.ChildNodes(i).Name.ToLower
                    Case CadenceDetektTechnologie_NAME
                        readValue(node.ChildNodes(i), CadenceDetektTechnologie)
                    Case CadenceDetekt_Terminal_Namespace_NAME
                        readValue(node.ChildNodes(i), CadenceDetekt_Terminal_Namespace)
                    Case CadenceDetekt_Terminal_Name_NAME
                        readValue(node.ChildNodes(i), CadenceDetekt_Terminal_Name)
                    Case Skill_Detect_removeDummys_NAME
                        readValue(node.ChildNodes(i), Skill_Detect_removeDummys)
                    Case Skill_Detect_removeFloatingElements_NAME
                        readValue(node.ChildNodes(i), Skill_Detect_removeFloatingElements)
                    Case Skill_Detect_removeFloatingWires_NAME
                        readValue(node.ChildNodes(i), Skill_Detect_removeFloatingWires)
                    Case Raster_NAME
                        readValue(node.ChildNodes(i), Raster)
                        If Raster < 1 Then
                            Raster = 1
                        End If
                        If Raster > 10000 Then
                            Raster = 10000
                        End If
                    Case Gravity_NAME
                        readValue(node.ChildNodes(i), Gravity)
                    Case GravityStärke_NAME
                        readValue(node.ChildNodes(i), Gravity_Stärke)
                        If Gravity_Stärke < 1 Then Gravity_Stärke = 1
                        If Gravity_Stärke > MAX_GRAVITY_STÄRKE Then
                            Gravity_Stärke = MAX_GRAVITY_STÄRKE
                        End If
                    Case Encoding_NAME
                        readValue(node.ChildNodes(i), Encoding)
                    Case Pfade_Bib_NAME
                        Dim pfadNeu As String = ""
                        readValue(node.ChildNodes(i), pfadNeu)
                        If pfadNeu <> "" Then
                            ReDim Preserve Pfade_Bib(Pfade_Bib.Length)
                            Pfade_Bib(Pfade_Bib.Length - 1) = pfadNeu
                        End If
                    Case Default_Params_NAME
                        read_default_param(node.ChildNodes(i), default_parameterListe)
                    Case SelectBauteil_NAME
                        read_SelectBauteil(node.ChildNodes(i), shortCutKeysSelectElement)
                    Case Sprache_NAME
                        readValue(node.ChildNodes(i), sprache)
                    Case CrossCursor_NAME
                        readValue(node.ChildNodes(i), CrossCursor)
                    Case SnappointsImmerAnzeigen_NAME
                        readValue(node.ChildNodes(i), SnappointsImmerAnzeigen)
                    Case RandAnzeigen_NAME
                        readValue(node.ChildNodes(i), RandAnzeigen)
                    Case TextVorschauMode_NAME
                        readValue(node.ChildNodes(i), TextVorschauMode)
                    Case BezierKurveAbweichung_NAME
                        readValue(node.ChildNodes(i), BezierKurveAbweichung)
                    Case MultiSelect_NAME
                        readValue(node.ChildNodes(i), MultiSelect)
                    Case kursiverTextImMatheModus_NAME
                        readValue(node.ChildNodes(i), kursiverTextImMatheModus)
                End Select
            Next
        End If
    End Sub

#Region "Read Value"
    Private Sub readValue(node As XmlNode, ByRef result As myEncodings)
        Dim erg As String = ""
        readValue(node, erg)
        erg = erg.Trim()
        If erg = "ANSI" Then
            result = myEncodings.ANSI
        ElseIf erg = "UTF8" Then
            result = myEncodings.UTF8
        End If
        'Keine Exception!!! Sonst wird alles verworfen und wieder die Default-werte genommen! 
        'Einfach nur diesen Wert nicht einlesen!
    End Sub

    Private Sub readValue(node As XmlNode, ByRef result As Vektorgrafik_Sprache)
        Dim erg As String = ""
        readValue(node, erg)
        erg = erg.Trim()
        If erg = "de-DE" Then
            result = Vektorgrafik_Sprache.Deutsch
        ElseIf erg = "en" Then
            result = Vektorgrafik_Sprache.Englisch
        End If
        'Keine Exception!!! Sonst wird alles verworfen und wieder die Default-werte genommen! 
        'Einfach nur diesen Wert nicht einlesen!
    End Sub

    Private Sub readValue(node As XmlNode, ByRef result As Integer)
        Dim erg As String = ""
        readValue(node, erg)
        erg = erg.ToLower()
        Dim erg_int As Integer
        If Integer.TryParse(erg, erg_int) Then
            result = erg_int
            Return
        End If
        'Keine Exception!!! Sonst wird alles verworfen und wieder die Default-werte genommen! 
        'Einfach nur diesen Wert nicht einlesen!

        'Throw New Exception("Falscher Wert für ein Integer: " & erg)
    End Sub

    Private Sub readValue(node As XmlNode, ByRef result As Boolean)
        Dim erg As String = ""
        readValue(node, erg)
        erg = erg.ToLower()
        If erg = "true" Then
            result = True
        ElseIf erg = "false" Then
            result = False
        Else
            'Keine Exception!!! Sonst wird alles verworfen und wieder die Default-werte genommen! 
            'Einfach nur diesen Wert nicht einlesen!

            'Throw New Exception("Falscher Wert für ein Boolean: " & erg)
        End If
    End Sub

    Private Sub readValue(node As XmlNode, ByRef result As String)
        For Each a As XmlAttribute In node.Attributes
            If a.Name.ToLower = "value" Then
                result = a.Value
                Exit For
            End If
        Next
    End Sub
#End Region

    Private Sub read_default_param(node As XmlNode, default_parameterListe As List(Of default_Parameter))
        Dim value As String = ""
        Dim param As String = ""
        Dim hatValue As Boolean = False
        Dim hatParam As Boolean = False
        For Each a As XmlAttribute In node.Attributes
            If a.Name.ToLower = "value" AndAlso Not hatValue Then
                value = a.Value
                hatValue = True
            End If
            If a.Name.ToLower = "param" AndAlso Not hatParam Then
                param = a.Value
                hatParam = True
            End If
        Next
        If hatParam AndAlso hatValue Then
            default_parameterListe.Add(New default_Parameter(param, value))
        End If
    End Sub

    Private Sub read_SelectBauteil(node As XmlNode, shortCutKeysSelectedElement As List(Of ShortcutKeySelectInstance))
        Dim _namespace As String = ""
        Dim _name As String = ""
        Dim _key As String = ""

        Dim _ctrl As Boolean = False
        Dim _shift As Boolean = False
        Dim _alt As Boolean = False

        Dim hat_namespace As Boolean = False
        Dim hat_name As Boolean = False
        Dim hat_key As Boolean = False
        For Each a As XmlAttribute In node.Attributes
            If a.Name.ToLower = "namespace" Then
                _namespace = a.Value
                hat_namespace = True
            ElseIf a.Name.ToLower = "name" Then
                _name = a.Value
                hat_name = True
            ElseIf a.Name.ToLower = "key" Then
                _key = a.Value
                hat_key = True
            ElseIf a.Name.ToLower = "ctrl" Then
                If a.Value.ToLower() = "true" Then
                    _ctrl = True
                End If
            ElseIf a.Name.ToLower = "alt" Then
                If a.Value.ToLower() = "true" Then
                    _alt = True
                End If
            ElseIf a.Name.ToLower = "shift" Then
                If a.Value.ToLower() = "true" Then
                    _shift = True
                End If
            End If
        Next
        If hat_namespace AndAlso hat_name AndAlso hat_key Then
            Dim myConv As New KeysConverter()
            Dim myK As Keys = CType(myConv.ConvertFromString(Nothing, New Globalization.CultureInfo("en"), _key), Keys)
            Dim k As New ShortcutKey(myK, _ctrl, _shift, _alt)
            If Not Key_Settings.getSettings().hasKey(k) Then
                shortCutKeysSelectedElement.Add(New ShortcutKeySelectInstance(k, _namespace, _name))
            End If
        End If
    End Sub

    Public Sub saveSettings()
        Dim xmlWriter As XmlTextWriter = Nothing
        Try
            xmlWriter = New Xml.XmlTextWriter(FileName, New System.Text.UnicodeEncoding())

            With xmlWriter
                .Formatting = Xml.Formatting.Indented
                .Indentation = 4

                .WriteStartDocument()
                .WriteComment("Settings von Circuit Drawing, gespeichert am " & Now.ToString())
                .WriteStartElement(SETTINGS_HEADER)

                speicherSettings(xmlWriter)

                .WriteEndElement()

                .WriteEndDocument()
                .Close()
            End With

        Catch ex As Exception
            Debug.Print("Fehler beim Speichern der Settings: " & ex.ToString)
        Finally
            If xmlWriter IsNot Nothing Then
                xmlWriter.Close()
            End If
        End Try

    End Sub

    Private Sub speicherSettings(xmlwriter As XmlTextWriter)
        speicherValue(xmlwriter, Sprache_NAME, sprache)

        speicherValue(xmlwriter, CadenceDetektTechnologie_NAME, CadenceDetektTechnologie)
        speicherValue(xmlwriter, CadenceDetekt_Terminal_Namespace_NAME, CadenceDetekt_Terminal_Namespace)
        speicherValue(xmlwriter, CadenceDetekt_Terminal_Name_NAME, CadenceDetekt_Terminal_Name)

        speicherValue(xmlwriter, Skill_Detect_removeFloatingElements_NAME, Skill_Detect_removeFloatingElements)
        speicherValue(xmlwriter, Skill_Detect_removeFloatingWires_NAME, Skill_Detect_removeFloatingWires)
        speicherValue(xmlwriter, Skill_Detect_removeDummys_NAME, Skill_Detect_removeDummys)

        speicherValue(xmlwriter, Raster_NAME, Raster.ToString())

        speicherValue(xmlwriter, Gravity_NAME, Gravity)
        speicherValue(xmlwriter, GravityStärke_NAME, Gravity_Stärke.ToString())
        speicherValue(xmlwriter, CrossCursor_NAME, CrossCursor)
        speicherValue(xmlwriter, SnappointsImmerAnzeigen_NAME, SnappointsImmerAnzeigen)
        speicherValue(xmlwriter, RandAnzeigen_NAME, RandAnzeigen)
        speicherValue(xmlwriter, TextVorschauMode_NAME, TextVorschauMode)
        speicherValue(xmlwriter, BezierKurveAbweichung_NAME, BezierKurveAbweichung.ToString())
        speicherValue(xmlwriter, MultiSelect_NAME, MultiSelect)
        speicherValue(xmlwriter, kursiverTextImMatheModus_NAME, kursiverTextImMatheModus)

        speicherValue(xmlwriter, Encoding_NAME, Encoding)

        speicherListe(xmlwriter, Pfade_Bib_NAME, Pfade_Bib)
        speicher_Default_Parameters(xmlwriter, Default_Params_NAME)

        speicher_SelectElementShortcutkeys(xmlwriter, SelectBauteil_NAME)

    End Sub

    Private Sub speicherValue(xmlWriter As XmlTextWriter, name As String, value As Vektorgrafik_Sprache)
        If value = Vektorgrafik_Sprache.Deutsch Then
            speicherValue(xmlWriter, name, "de-DE")
        Else
            speicherValue(xmlWriter, name, "en")
        End If
    End Sub

    Private Sub speicherValue(xmlWriter As XmlTextWriter, name As String, value As myEncodings)
        If value = myEncodings.ANSI Then
            speicherValue(xmlWriter, name, "ANSI")
        ElseIf value = myEncodings.UTF8 Then
            speicherValue(xmlWriter, name, "UTF8")
        End If
    End Sub

    Private Sub speicherValue(xmlWriter As XmlTextWriter, name As String, value As Boolean)
        If value Then
            speicherValue(xmlWriter, name, "true")
        Else
            speicherValue(xmlWriter, name, "false")
        End If
    End Sub

    Private Sub speicherValue(xmlwriter As XmlTextWriter, name As String, wert As String)
        xmlwriter.WriteStartElement(name)
        xmlwriter.WriteAttributeString("value", wert)
        xmlwriter.WriteEndElement()
    End Sub

    Private Sub speicherListe(xmlwriter As XmlTextWriter, name As String, liste() As String)
        For i As Integer = 0 To liste.Length - 1
            speicherValue(xmlwriter, name, liste(i))
        Next
    End Sub

    Private Sub speicher_Default_Parameters(xmlwriter As XmlTextWriter, name As String)
        For i As Integer = 0 To default_params.Length - 1
            xmlwriter.WriteStartElement(name)
            xmlwriter.WriteAttributeString("param", default_params(i).param)
            xmlwriter.WriteAttributeString("value", default_params(i).value)
            xmlwriter.WriteEndElement()
        Next
    End Sub

    Private Sub speicher_SelectElementShortcutkeys(xmlwriter As XmlTextWriter, name As String)
        Dim myConv As New KeysConverter()
        For i As Integer = 0 To KeysSelectInstance.Count - 1
            xmlwriter.WriteStartElement(name)
            xmlwriter.WriteAttributeString("namespace", KeysSelectInstance(i)._namespace)
            xmlwriter.WriteAttributeString("name", KeysSelectInstance(i)._cell)
            xmlwriter.WriteAttributeString("key", myConv.ConvertToString(Nothing, New Globalization.CultureInfo("en"), KeysSelectInstance(i).key.keyValue))
            xmlwriter.WriteAttributeString("ctrl", booleanToString(KeysSelectInstance(i).key.hatCtrl))
            xmlwriter.WriteAttributeString("shift", booleanToString(KeysSelectInstance(i).key.hatShift))
            xmlwriter.WriteAttributeString("alt", booleanToString(KeysSelectInstance(i).key.hatAlt))
            xmlwriter.WriteEndElement()
        Next
    End Sub

    Private Function booleanToString(b As Boolean) As String
        If b Then
            Return "True"
        Else
            Return "False"
        End If
    End Function

    Public Enum myEncodings
        UTF8
        ANSI
    End Enum

End Class

Public Class default_Parameter
    Public param As String
    Public value As String
    Public Sub New(param As String, value As String)
        Me.param = param
        Me.value = value
    End Sub
End Class

Public Enum Vektorgrafik_Sprache
    Deutsch
    Englisch
End Enum
