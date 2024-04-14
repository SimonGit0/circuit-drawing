Imports System.IO
Public Class Deko_Bauteil
    Private myTemplate As TemplateAusDatei
    Private paramEinstellungen() As ParamValue
    Private _myTemplate_compiled As Template_Compiled
    Private parent As BauteilAusDatei

    Public Sub setParent(b As BauteilAusDatei)
        Me.parent = b
    End Sub

    Private ReadOnly Property template_compiled As Template_Compiled
        Get
            If _myTemplate_compiled Is Nothing Then
                recompile()
            End If
            Return _myTemplate_compiled
        End Get
    End Property

    Public Sub recompile()
        myTemplate.recompile(paramEinstellungen, 0, _myTemplate_compiled, New CompileParentArgs(parent))
    End Sub

    Public Sub New(template As TemplateAusDatei, parent As BauteilAusDatei)
        Me.parent = parent
        Me.myTemplate = template
        paramEinstellungen = template.getDefaultParameters_copy()
        _myTemplate_compiled = Nothing
    End Sub

    Public Function getGrafik() As DO_MultiGrafik
        Return template_compiled.getGrafik()
    End Function

    Public Function NrOfSnappoints() As Integer
        Return template_compiled.getNrOfSnappoints()
    End Function

    Public Function getSnappoint(i As Integer) As Snappoint
        Return template_compiled.getSnappoint(i)
    End Function

    Public Function Clone() As Deko_Bauteil
        Dim d As New Deko_Bauteil(myTemplate, Nothing)
        For i As Integer = 0 To paramEinstellungen.Length - 1
            d.paramEinstellungen(i) = Me.paramEinstellungen(i).Copy()
        Next
        'Die Kopie hat erstmal kein _template_compiled, da es dann später recompiled wird.
        'Insbesondere für Rückgängig wichtig, um nicht unnötig speicherplatz mit zahlreichen kompilierten Templates zu verschwenden.
        'Sobald ein Rückgängig neu geladen wird, dann muss es halt nochmal neu kompiliert werden!
        d._myTemplate_compiled = Nothing

        Return d
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(myTemplate.getNameSpace())
        writer.Write(myTemplate.getName())
        writer.Write(myTemplate.getView())

        writer.Write(paramEinstellungen.Length)
        For i As Integer = 0 To paramEinstellungen.Length - 1
            If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Param Then
                writer.Write(0)
                writer.Write(DirectCast(myTemplate.getParameter(i), TemplateParameter_Param).name.get_ID())
                writer.Write(DirectCast(paramEinstellungen(i), ParamInt).myVal)
            ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_Arrow Then
                writer.Write(1)
                writer.Write(DirectCast(myTemplate.getParameter(i), TemplateParameter_Arrow).name.get_ID())
                writer.Write(CInt(DirectCast(paramEinstellungen(i), ParamArrow).pfeilArt))
                writer.Write(CInt(DirectCast(paramEinstellungen(i), ParamArrow).pfeilSize))
            ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_Int Then
                writer.Write(2)
                writer.Write(DirectCast(myTemplate.getParameter(i), TemplateParameter_Int).name.get_ID())
                writer.Write(DirectCast(paramEinstellungen(i), ParamInt).myVal)
            ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_String Then
                writer.Write(3)
                writer.Write(DirectCast(myTemplate.getParameter(i), TemplateParameter_String).name.get_ID())
                writer.Write(DirectCast(paramEinstellungen(i), ParamString).myVal)
            Else
                writer.Write(-1) 'unbekannte (leere) Art
            End If
        Next
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean) As Deko_Bauteil
        Dim _namespace As String = reader.ReadString()
        Dim _name As String = reader.ReadString()
        Dim _view As String = reader.ReadString()
        Dim template As TemplateAusDatei = Nothing
        Dim templateCompatibility As TemplateCompatibility = Nothing

        Dim hatschongeladen As Boolean = False
        If kompatibilität Then
            'wenn möglich erstmal aus der lokalen Bib laden!!
            If lokaleBib.hatBauteil(_namespace, _name) Then
                Dim bauteil = lokaleBib.getBauteil(_namespace, _name)
                If Not bauteil.istSchonGeladen Then
                    bauteil.neuerNamespaceName = Bibliothek.NAMESPACE_LOKAL
                    bauteil.neuerName = bib.getAvailableLocalName(_name, "")
                    bauteil.istSchonGeladen = True

                    bauteil.tmpl.__changeName(bauteil.neuerName)
                    bauteil.tmpl.__changeNamespace(Bibliothek.NAMESPACE_LOKAL)
                    bib.add_local_bauteil(bauteil.tmpl)

                    _name = bauteil.neuerName
                    _namespace = bauteil.neuerNamespaceName
                    template = bib.getBauteil(_namespace, _name, _view)
                Else
                    _name = bauteil.neuerName
                    _namespace = bauteil.neuerNamespaceName
                    template = bib.getBauteil(_namespace, _name, _view)
                End If
                hatschongeladen = True
            End If
        End If
        If Not hatschongeladen Then
            If bib.hasBauteil(_namespace, _name, _view) Then
                template = bib.getBauteil(_namespace, _name, _view)
            Else
                templateCompatibility = bib.sucheBauteil_Compatibility(_namespace, _name)
                If templateCompatibility Is Nothing Then
                    If lokaleBib.hatBauteil(_namespace, _name) Then
                        Dim bauteil = lokaleBib.getBauteil(_namespace, _name)
                        If Not bauteil.istSchonGeladen Then
                            bauteil.neuerNamespaceName = Bibliothek.NAMESPACE_LOKAL
                            Dim NameIstSchonWeg As String = ""
                            bauteil.neuerName = bib.getAvailableLocalName(_name, NameIstSchonWeg)
                            If NameIstSchonWeg <> "" AndAlso bauteil.tmpl.istGleich(bib.getBauteil(Bibliothek.NAMESPACE_LOKAL, NameIstSchonWeg, "eu")) Then
                                'Exakt dieses Bauteil ist schon in der Bibliothek da -> nicht nochmal einladen! (Kommt nur vor bei mehrfachen CTRL-V von Nicht-Lokalen Bauteilen!!!)
                                bauteil.tmpl = bib.getBauteil(Bibliothek.NAMESPACE_LOKAL, NameIstSchonWeg, "eu")
                                bauteil.neuerName = NameIstSchonWeg
                            Else
                                bauteil.tmpl.__changeName(bauteil.neuerName)
                                bauteil.tmpl.__changeNamespace(Bibliothek.NAMESPACE_LOKAL)
                                bib.add_local_bauteil(bauteil.tmpl)
                            End If
                            bauteil.istSchonGeladen = True
                            _name = bauteil.neuerName
                            _namespace = bauteil.neuerNamespaceName
                            template = bib.getBauteil(_namespace, _name, _view)
                        Else
                            _name = bauteil.neuerName
                            _namespace = bauteil.neuerNamespaceName
                            template = bib.getBauteil(_namespace, _name, _view)
                        End If
                    Else
                        Throw New Exception("Das Bauteil '" + _namespace + "." + _name + "." + _view + " ist in der aktuellen Bauteilbibliothek nicht vorhanden.")
                    End If
                End If
            End If
        End If

        Dim d As Deko_Bauteil
        If template IsNot Nothing Then
            d = New Deko_Bauteil(template, Nothing)
            '"old" param values laden (Zur Kompatibilität mit "old" definiert). Wenn es eine neue Datei ist werden diese Parameter gleich wieder überschrieben!
            For i As Integer = 0 To d.paramEinstellungen.Count - 1
                If TypeOf template.getParameter(i) Is TemplateParameter_Param Then
                    With DirectCast(template.getParameter(i), TemplateParameter_Param)
                        If .oldOption >= 0 AndAlso .oldOption < .options.Length Then
                            d.paramEinstellungen(i) = New ParamInt(.oldOption)
                        End If
                    End With
                End If
            Next
        Else
            d = New Deko_Bauteil(templateCompatibility.parent, Nothing)
            template = d.myTemplate
            '"old" param values laden (Zur Kompatibilität mit "old" definiert). Wenn es eine neue Datei ist werden diese Parameter gleich wieder überschrieben!
            For i As Integer = 0 To d.paramEinstellungen.Count - 1
                If TypeOf template.getParameter(i) Is TemplateParameter_Param Then
                    With DirectCast(template.getParameter(i), TemplateParameter_Param)
                        If .oldOption >= 0 AndAlso .oldOption < .options.Length Then
                            d.paramEinstellungen(i) = New ParamInt(.oldOption)
                        End If
                    End With
                End If
            Next
            'Laden der Template Compatibility Parameter erst nachdem die old-Values gelesen wurden! Da Template-Compatibility ja die Old-Werte überschreiben muss!
            templateCompatibility.LoadParams(d.paramEinstellungen)
        End If

        Dim anzahlEinstellungen As Integer = reader.ReadInt32()
        For i As Integer = 0 To anzahlEinstellungen - 1
            Dim paramArt As Integer = reader.ReadInt32()
            If paramArt = 0 Then
                Dim nameE As String = reader.ReadString()
                Dim valueE As Integer = reader.ReadInt32()
                For k As Integer = 0 To d.myTemplate.getNrOfParams() - 1
                    If TypeOf d.myTemplate.getParameter(k) Is TemplateParameter_Param Then
                        If DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Param).name.get_ID() = nameE AndAlso DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Param).options.Length > valueE Then
                            d.paramEinstellungen(k) = New ParamInt(valueE)
                        End If
                    End If
                Next
            ElseIf paramArt = 1 Then
                Dim nameE As String = reader.ReadString()
                Dim valueE As ParamArrow
                If version >= 10 Then
                    Dim art As Integer = reader.ReadInt32()
                    Dim size As Integer = reader.ReadInt32()
                    valueE = New ParamArrow(CShort(art), CUShort(size))
                Else
                    valueE = New ParamArrow(CShort(reader.ReadInt32()), 100)
                End If
                For k As Integer = 0 To d.myTemplate.getNrOfParams() - 1
                    If TypeOf d.myTemplate.getParameter(k) Is TemplateParameter_Arrow Then
                        If DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Arrow).name.get_ID() = nameE Then
                            Dim intervall As Intervall = DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Arrow).intervall
                            If valueE.pfeilArt >= intervall.min AndAlso valueE.pfeilArt <= intervall.max Then
                                d.paramEinstellungen(k) = valueE
                            End If
                        End If
                    End If
                Next
            ElseIf paramArt = 2 Then
                Dim nameE As String = reader.ReadString()
                Dim valueE As Integer = reader.ReadInt32()
                For k As Integer = 0 To d.myTemplate.getNrOfParams() - 1
                    If TypeOf d.myTemplate.getParameter(k) Is TemplateParameter_Int Then
                        If DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Int).name.get_ID() = nameE Then
                            Dim intervall As Intervall = DirectCast(d.myTemplate.getParameter(k), TemplateParameter_Int).intervall
                            If valueE >= intervall.min AndAlso valueE <= intervall.max AndAlso (CLng(valueE) - intervall.min) Mod CLng(intervall._step) = 0 Then
                                d.paramEinstellungen(k) = New ParamInt(valueE)
                            End If
                        End If
                    End If
                Next
            ElseIf paramArt = 3 Then 'String Param
                Dim nameE As String = reader.ReadString()
                Dim valueE As String = reader.ReadString()
                For k As Integer = 0 To d.myTemplate.getNrOfParams() - 1
                    If TypeOf d.myTemplate.getParameter(k) Is TemplateParameter_String Then
                        If DirectCast(d.myTemplate.getParameter(k), TemplateParameter_String).name.get_ID() = nameE Then
                            d.paramEinstellungen(k) = New ParamString(valueE)
                        End If
                    End If
                Next
            End If
        Next

        Return d
    End Function

    Public Function isEqualExceptSelection(e2 As Deko_Bauteil) As Boolean
        If Not Me.myTemplate.Equals(e2.myTemplate) Then Return False
        If Me.paramEinstellungen.Length <> e2.paramEinstellungen.Length Then Return False
        For i As Integer = 0 To e2.paramEinstellungen.Length - 1
            If Not Me.paramEinstellungen(i).isEqual(e2.paramEinstellungen(i)) Then
                Return False
            End If
        Next

        Return True
    End Function

    Public Function getTemplate() As TemplateAusDatei
        Return myTemplate
    End Function

    Public Sub getEinstellungen(sender As Vektor_Picturebox, e1 As Einstellung_Multi)
        For i As Integer = 0 To myTemplate.getNrOfParams() - 1
            If template_compiled.getParamVisible(i) Then
                If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Param Then
                    e1.add(New Einstellung_TemplateParameter(DirectCast(myTemplate.getParameter(i), TemplateParameter_Param), DirectCast(paramEinstellungen(i), ParamInt).myVal))
                ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_Arrow Then
                    e1.add(New Einstellung_TemplateParameter_Arrow(DirectCast(myTemplate.getParameter(i), TemplateParameter_Arrow), DirectCast(paramEinstellungen(i), ParamArrow)))
                ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_Int Then
                    e1.add(New Einstellung_TemplateParameter_Int(DirectCast(myTemplate.getParameter(i), TemplateParameter_Int), DirectCast(paramEinstellungen(i), ParamInt).myVal))
                ElseIf TypeOf myTemplate.getParameter(i) Is TemplateParameter_String Then
                    e1.add(New Einstellung_TemplateParameterString(DirectCast(myTemplate.getParameter(i), TemplateParameter_String), DirectCast(paramEinstellungen(i), ParamString).myVal))
                End If
            End If
        Next
    End Sub

    Public Function setEinstellungen(sender As Vektor_Picturebox, e1 As Einstellung_Multi) As Boolean
        Dim changed As Boolean = False
        For Each eSub As Einstellung_TemplateParam In DirectCast(e1, Einstellung_Multi).getListe()
            If TypeOf eSub Is Einstellung_TemplateParameter Then
                With DirectCast(eSub, Einstellung_TemplateParameter)
                    If .nrChanged Then
                        For i As Integer = 0 To myTemplate.getNrOfParams - 1
                            If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Param Then
                                If DirectCast(myTemplate.getParameter(i), TemplateParameter_Param).name.get_ID() = .Name.get_ID() Then
                                    Me.paramEinstellungen(i) = New ParamInt(.myNr)
                                    changed = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            ElseIf TypeOf eSub Is Einstellung_TemplateParameter_Arrow Then
                With DirectCast(eSub, Einstellung_TemplateParameter_Arrow)
                    If .nrChanged OrElse .sizeChanged Then
                        For i As Integer = 0 To myTemplate.getNrOfParams - 1
                            If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Arrow Then
                                If DirectCast(myTemplate.getParameter(i), TemplateParameter_Arrow).name.get_ID() = .Name.get_ID() Then
                                    If .nrChanged Then
                                        DirectCast(Me.paramEinstellungen(i), ParamArrow).pfeilArt = .myNr.pfeilArt
                                        changed = True
                                    End If
                                    If .sizeChanged Then
                                        DirectCast(Me.paramEinstellungen(i), ParamArrow).pfeilSize = .myNr.pfeilSize
                                        changed = True
                                    End If
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            ElseIf TypeOf eSub Is Einstellung_TemplateParameter_Int Then
                With DirectCast(eSub, Einstellung_TemplateParameter_Int)
                    If .nrChanged Then
                        For i As Integer = 0 To myTemplate.getNrOfParams - 1
                            If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Int Then
                                If DirectCast(myTemplate.getParameter(i), TemplateParameter_Int).name.get_ID() = .Name.get_ID() Then
                                    Me.paramEinstellungen(i) = New ParamInt(.myNr)
                                    changed = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            ElseIf TypeOf eSub Is Einstellung_TemplateParameterString Then
                With DirectCast(eSub, Einstellung_TemplateParameterString)
                    If .txtChanged Then
                        For i As Integer = 0 To myTemplate.getNrOfParams - 1
                            If TypeOf myTemplate.getParameter(i) Is TemplateParameter_String Then
                                If DirectCast(myTemplate.getParameter(i), TemplateParameter_String).name.get_ID() = .Name.get_ID() Then
                                    Me.paramEinstellungen(i) = New ParamString(.myStr)
                                    changed = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            End If
        Next
        If changed Then
            recompile()
        End If
        Return changed
    End Function

End Class
