Imports System.IO

Public Class Bibliothek
    Implements IEnumerable(Of KeyValuePair(Of String, BauteileNamespace))

    Public Const NAMESPACE_LOKAL As String = "Lokale Bauteile"

    Private myBauteile As Dictionary(Of String, BauteileNamespace)

    Private Sub New()
        myBauteile = New Dictionary(Of String, BauteileNamespace)
    End Sub

    Public Sub New(pfad() As String)
        myBauteile = New Dictionary(Of String, BauteileNamespace)

        _EINLESEN_fehler = New List(Of CompileException)

        For i As Integer = 0 To pfad.Length - 1
            If Directory.Exists(pfad(i)) Then
                einlesen(pfad(i))
            Else
                _EINLESEN_fehler.Add(New CompileException("Warnung", "Der Pfad '" & pfad(i) & "' existiert nicht. Es konnten keine Dateien von diesem Pfad geladen werden", ""))
            End If

        Next

        If _EINLESEN_fehler.Count > 0 Then
            Dim frm_error As New FRM_Einlesefehler()
            frm_error.setErrors(_EINLESEN_fehler)
            frm_error.ShowDialog()
        End If
    End Sub

    Private _EINLESEN_fehler As List(Of CompileException)

    Private Sub einlesen(pfad As String)

        Dim dateien() As FileInfo = New DirectoryInfo(pfad).GetFiles()
        For i As Integer = 0 To dateien.Length - 1
            If dateien(i).Extension = ".sym" AndAlso Not dateien(i).Name.StartsWith("_") Then
                Dim fehler As CompileException = ladeDatei(dateien(i).FullName)
                If fehler IsNot Nothing Then
                    _EINLESEN_fehler.Add(fehler)
                End If
            End If
        Next

        Dim folder() As DirectoryInfo = New DirectoryInfo(pfad).GetDirectories()
        For i As Integer = 0 To folder.Length - 1
            einlesen(folder(i).FullName)
        Next
    End Sub

    Private Function ladeDatei(p As String) As CompileException
        Try
            Dim b As TemplateAusDatei
            b = New TemplateAusDatei(p)

            addTemplate(b)

            Return Nothing 'Kein Fehler!
        Catch cex As CompileException
            Return cex
        Catch ex As Exception
            Return New CompileException("Allgemeiner beim Einlesen der Datei: " & ex.Message, "In Datei: " & p)
        End Try
    End Function

    Private Sub addTemplate(tmpl As TemplateAusDatei)
        Dim ns As BauteileNamespace
        If myBauteile.ContainsKey(tmpl.getNameSpace()) Then
            ns = myBauteile(tmpl.getNameSpace())
        Else
            ns = New BauteileNamespace(tmpl.getNameSpace())
            myBauteile.Add(tmpl.getNameSpace(), ns)
        End If

        ns.add(tmpl)
    End Sub

    Public Function hasBauteil(_namespace As String, _name As String, _view As String) As Boolean
        If myBauteile.ContainsKey(_namespace) Then
            Return myBauteile(_namespace).hasBauteil(_name, _view)
        Else
            Return False
        End If
    End Function

    Public Function sucheBauteil_Compatibility(_namespace As String, _name As String) As TemplateCompatibility
        For Each pair As KeyValuePair(Of String, BauteileNamespace) In myBauteile
            Dim erg As TemplateCompatibility = pair.Value.sucheBauteil_Compatibility(_namespace, _name)
            If erg IsNot Nothing Then
                Return erg
            End If
        Next
        Return Nothing
    End Function

    Public Function getBauteil(_namespace As String, _name As String, _view As String) As TemplateAusDatei
        Return myBauteile(_namespace).getBauteil(_name, _view)
    End Function

    Public Function getNamespace(name As String) As BauteileNamespace
        Return myBauteile(name)
    End Function

    Public Function getNamespaceCount() As Integer
        Return myBauteile.Count()
    End Function

    Public Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, BauteileNamespace)) Implements IEnumerable(Of KeyValuePair(Of String, BauteileNamespace)).GetEnumerator
        Return myBauteile.GetEnumerator()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return myBauteile.GetEnumerator()
    End Function

    Public Sub reload_default_params()
        For Each b As BauteileNamespace In myBauteile.Values
            b.reload_default_params()
        Next
    End Sub

    Public Function try_translate_param_value(param As String, value As String, str_to_ID As Boolean) As KeyValuePair(Of String, String)?
        For Each b As BauteileNamespace In myBauteile.Values
            Dim erg = b.try_translate_param_value(param, value, str_to_ID)
            If erg IsNot Nothing Then Return erg
        Next
        Return Nothing
    End Function

#Region "Lokale Namespaces"
    Public Sub clear_local_namespace()
        If myBauteile.ContainsKey(NAMESPACE_LOKAL) Then
            myBauteile.Remove(NAMESPACE_LOKAL)
        End If
    End Sub

    Public Sub add_local_bauteil(tmpl As TemplateAusDatei)
        If Not myBauteile.ContainsKey(NAMESPACE_LOKAL) Then
            myBauteile.Add(NAMESPACE_LOKAL, New BauteileNamespace(NAMESPACE_LOKAL))
        End If
        myBauteile(NAMESPACE_LOKAL).add(tmpl)
    End Sub

    Public Sub deleteLocalBauteil(tmpl As TemplateAusDatei)
        If tmpl.getNameSpace() = NAMESPACE_LOKAL Then
            Dim ns As BauteileNamespace = Me.myBauteile(NAMESPACE_LOKAL)
            ns.deleteLocalBauteil(tmpl)
            If ns.getCellCount() = 0 Then
                Me.myBauteile.Remove(NAMESPACE_LOKAL)
            End If
        End If
    End Sub

    Public Function getAvailableLocalName(nameBasis As String, ByRef NameIstSchonWeg As String) As String
        If gibt_es_lokales_bauteil_schon(nameBasis) Then
            NameIstSchonWeg = nameBasis
            For i As Integer = 1 To Integer.MaxValue
                If Not gibt_es_lokales_bauteil_schon(nameBasis & i.ToString()) Then
                    Return nameBasis & i.ToString()
                Else
                    NameIstSchonWeg = nameBasis & i.ToString()
                End If
            Next
            Throw New Exception("Kann keinen Namen für das Bauteil finden")
        Else
            Return nameBasis
        End If
    End Function

    Public Function gibt_es_lokales_bauteil_schon(name As String) As Boolean
        Return gibt_es_namen_schon(NAMESPACE_LOKAL, name)
    End Function

    Public Function gibt_es_namen_schon(ns As String, name As String) As Boolean
        If myBauteile.ContainsKey(ns) Then
            Return myBauteile(ns).hasBauteil(name, "eu")
        End If
        Return False
    End Function

    Public Function FlatCopyOhneLocal() As Bibliothek
        Dim erg As New Bibliothek()
        For i As Integer = 0 To Me.myBauteile.Keys.Count - 1
            If myBauteile.Keys(i) <> NAMESPACE_LOKAL Then
                erg.myBauteile.Add(myBauteile.Keys(i), myBauteile(myBauteile.Keys(i)))
            End If
        Next
        Return erg
    End Function

    Public Sub speicherLokaleTemplates(writer As BinaryWriter)
        Dim anzahl As Integer = 0
        If myBauteile.ContainsKey(NAMESPACE_LOKAL) Then
            writer.Write(1)
            Dim ns As BauteileNamespace = myBauteile(NAMESPACE_LOKAL)
            ns.speicher(writer)
        Else
            writer.Write(-1)
        End If
    End Sub

    Public Sub ladeLokaleTemplates(reader As BinaryReader, version As Integer)
        If version < 13 Then
            'erst ab Version 13 werden die Parameter gespeichert!
            Return
        End If
        Dim hatBauteile As Integer = reader.ReadInt32()
        If hatBauteile = 1 Then
            Dim ns As New BauteileNamespace(NAMESPACE_LOKAL)
            myBauteile.Add(NAMESPACE_LOKAL, ns)
            ns.laden(reader, version)
        ElseIf hatBauteile = -1 Then
            'es gibt keine lokalen Bauteile!
            clear_local_namespace()
        Else
            Throw New Exception("Der Wert " & hatBauteile & " ist ungültig für 'hatBauteile'.")
        End If
    End Sub
#End Region

    Public Sub speicherVerwendeteTemplates(writer As BinaryWriter, elemente As List(Of ElementMaster), lokaleSpeichern As Boolean, nichtLokaleSpeichern As Boolean)
        Dim bauteile As New Dictionary(Of String, TemplateAusDatei)
        For Each e As ElementMaster In elemente
            If TypeOf e Is BauteilAusDatei Then
                With DirectCast(e, BauteilAusDatei)
                    Dim lokaleDatei As Boolean = .getTemplate().getNameSpace() = NAMESPACE_LOKAL
                    If (lokaleDatei AndAlso lokaleSpeichern) OrElse (Not lokaleDatei AndAlso nichtLokaleSpeichern) Then
                        Dim str As String = .getTemplate().getNameSpace() & "." & .getTemplate().getName()
                        If Not bauteile.ContainsKey(str) Then
                            bauteile.Add(str, .getTemplate())
                        End If
                    End If

                    Dim deko As List(Of Deko_Bauteil) = .getDeko()
                    If deko IsNot Nothing Then
                        For i As Integer = 0 To deko.Count - 1
                            lokaleDatei = deko(i).getTemplate().getNameSpace() = NAMESPACE_LOKAL
                            If (lokaleDatei AndAlso lokaleSpeichern) OrElse (Not lokaleDatei AndAlso nichtLokaleSpeichern) Then
                                Dim str As String = deko(i).getTemplate().getNameSpace() & "." & deko(i).getTemplate().getName()
                                If Not bauteile.ContainsKey(str) Then
                                    bauteile.Add(str, deko(i).getTemplate())
                                End If
                            End If
                        Next
                    End If

                End With
            End If
        Next

        Dim tmpls = bauteile.Values
        writer.Write(bauteile.Count)
        For i As Integer = 0 To bauteile.Count - 1
            tmpls(i).speichern(writer)
        Next
    End Sub

    Public Shared Function ladeGespeicherteSymboleAlsNeueBib(reader As BinaryReader, version As Integer) As LokaleBibliothek
        Dim erg As New LokaleBibliothek()
        If version < 14 Then
            'erst ab Version 14 werden die Parameter gespeichert!
            Return erg
        End If

        Dim anzahl As Integer = reader.ReadInt32()
        For i As Integer = 0 To anzahl - 1
            Dim tmpl As TemplateAusDatei = TemplateAusDatei.laden(reader, version)
            erg.add(tmpl)
        Next
        Return erg
    End Function
End Class