Public Class Einstellungspanel_Bauelemente
    Implements IEinstellungspanel

    Private myIntelisense As Intelisense
    Private myStruktur As List(Of IntelisenseEntry)
    Public myBib As Bibliothek

    Public Sub New()
        InitializeComponent()

        myStruktur = New List(Of IntelisenseEntry)
        myIntelisense = Nothing
    End Sub

    Private Sub ladeIntelisense()
        myStruktur.Clear()
        If myBib IsNot Nothing Then
            For Each el As KeyValuePair(Of String, BauteileNamespace) In myBib
                Dim ns As New IntelisenseEntryNamespace(el.Value.Name)
                For Each cell As KeyValuePair(Of String, BauteilCell) In el.Value.getBauteile()
                    Dim nsc As New IntelisenseEntryNamespace(cell.Value.Name)
                    For i As Integer = 0 To cell.Value.getFirst().template.getNrOfParams() - 1
                        nsc.childs.Add(New IntelisenseParameter(cell.Value.getFirst().template.getParameter(i)))
                    Next
                    ns.childs.Add(nsc)
                Next
                myStruktur.Add(ns)
            Next
        End If
    End Sub

    Public Sub Set_Default() Implements IEinstellungspanel.Set_Default
        Settings.getSettings().Pfade_Bib = {"ET"}
        Settings.getSettings().default_params = {New default_Parameter("*.style", "eu")}
    End Sub

    Public Sub InitValues() Implements IEinstellungspanel.InitValues
        With Settings.getSettings()
            Dim txt As String = ""
            For i As Integer = 0 To .Pfade_Bib.Length - 1
                If i <> 0 Then txt &= vbCrLf
                txt &= .Pfade_Bib(i)
            Next
            TextBox1.Text = txt

            txt = ""
            For i As Integer = 0 To .default_params.Length - 1
                If i <> 0 Then txt &= vbCrLf
                txt &= .default_params(i).param & " = " & .default_params(i).value
            Next
            txtDefaultParam.Text = txt

        End With
    End Sub

    Public Sub OnShown() Implements IEinstellungspanel.OnShown
        'Die Intelisense nur bei OnShow initialisieren, da die parent form benötigt wird!! (mit findForm!)
        If myIntelisense Is Nothing Then
            myIntelisense = New Intelisense(txtDefaultParam, myStruktur)
            myIntelisense.Like_Operator_Nutzen = True
            ladeIntelisense()
        End If
    End Sub

    Public Function SpeicherValues(args As EinstellungSpeichernArgs) As Boolean Implements IEinstellungspanel.SpeicherValues
        With Settings.getSettings()
            Dim liste As New List(Of String)
            For i As Integer = 0 To TextBox1.Lines.Count - 1
                Dim line As String = TextBox1.Lines(i).Trim
                If line <> "" Then
                    liste.Add(line)
                End If
            Next
            .Pfade_Bib = liste.ToArray()

            Try
                Dim default_Val As New List(Of default_Parameter)
                For i As Integer = 0 To txtDefaultParam.Lines.Count - 1
                    Dim line As String = txtDefaultParam.Lines(i).Trim
                    If line <> "" Then
                        readParameter(line, default_Val)
                    End If
                Next
                .default_params = default_Val.ToArray()
                args.bib.reload_default_params()
                args.FRM_BauelementeAuswahl.refresh_Liste(args.bib)
            Catch ex As Exception
                MessageBox.Show("Fehler beim Übernehmen der Default-Parameter: " & ex.Message, "Fehler beim Übernehmen")
                Return False
            End Try

        End With
        Return True
    End Function

    Public Shared Sub readParameter(line As String, liste As List(Of default_Parameter))
        Dim values() As String = Mathe.splitString(line, "="c)
        If values.Length <> 2 Then
            Throw New Exception("Falsche Definition des Parameters: " & line)
        End If
        Dim param As String = Mathe.strToLower(values(0).Trim())
        Dim value As String = Mathe.strToLower(values(1).Trim())
        liste.Add(New default_Parameter(param, value))
    End Sub

    Public Function getPanel() As Panel Implements IEinstellungspanel.getPanel
        Return Panel1
    End Function

    Public Function getName() As String Implements IEinstellungspanel.getName
        Return My.Resources.Strings.Einstellungspanel_Bauelemente
    End Function
End Class
