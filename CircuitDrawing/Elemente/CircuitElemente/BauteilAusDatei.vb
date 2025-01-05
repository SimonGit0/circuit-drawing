Imports System.IO

Public Class BauteilAusDatei
    Inherits Bauteil
    Implements IElementWithFill
    Implements IElementWithFont

    Public Const DEFAULT_ABSTAND_TEXT As Integer = 0
    Public Const DEFAULT_ABSTAND_QUER As Integer = 0
    Public Const DefaultFillstyle As Integer = 0

    Private myTemplate As TemplateAusDatei
    Private myDrehmatrix As Drehmatrix
    Private myBeschriftung As Beschriftung
    Private paramEinstellungen() As ParamValue

    Private _myTemplate_compiled As Template_Compiled

    Private mydeko As List(Of Deko_Bauteil)

    Private myfontstyle As Integer
    Private myfillstyle As Integer

    Public Function getDeko() As List(Of Deko_Bauteil)
        Return mydeko
    End Function

    Public ReadOnly Property template_compiled As Template_Compiled
        Get
            If _myTemplate_compiled Is Nothing Then
                recompile()
            End If
            Return _myTemplate_compiled
        End Get
    End Property

    Public Sub FlatCopyCompiledTemplate(toElement As BauteilAusDatei)
        toElement._myTemplate_compiled = _myTemplate_compiled
    End Sub

    Private Function recompileScaling(callbackNr As Integer, scale As Integer, scale_normiert As Integer) As Boolean
        Return myTemplate.recompileScaling(callbackNr, scale, scale_normiert, Me, paramEinstellungen, myBeschriftung.positionIndex, Nothing)
    End Function

    Private Sub recompile()
        myTemplate.recompile(paramEinstellungen, myBeschriftung.positionIndex, _myTemplate_compiled, Nothing)
        If mydeko IsNot Nothing Then
            For i As Integer = 0 To mydeko.Count - 1
                mydeko(i).recompile()
            Next
        End If
    End Sub

    Public Sub New(ID As ULong, pos As Point, template As TemplateAusDatei, drehung As Drehmatrix, myBeschriftung As Beschriftung, fontstyle As Integer, fillstyles As Integer)
        MyBase.New(ID, 0)
        Me.myTemplate = template
        Me.position = pos
        Me.myDrehmatrix = drehung
        Me.myBeschriftung = myBeschriftung
        'ReDim paramEinstellungen(template.getNrOfParams() - 1)
        paramEinstellungen = template.getDefaultParameters_copy()
        _myTemplate_compiled = Nothing
        mydeko = Nothing
        Me.myfontstyle = fontstyle
        Me.myfillstyle = fillstyles
    End Sub

    Public Sub setParams(p() As ParamValue)
        Me.paramEinstellungen = p
    End Sub

    Public Function getDrehmatrix() As Drehmatrix
        Return myDrehmatrix
    End Function

    Public Function getTemplate() As TemplateAusDatei
        Return myTemplate
    End Function

    Public Sub addDeko(d As Deko_Bauteil)
        If mydeko Is Nothing Then
            mydeko = New List(Of Deko_Bauteil)(1)
            mydeko.Add(d)
            d.setParent(Me)
        Else
            mydeko.Add(d)
            d.setParent(Me)
        End If
    End Sub

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        Dim g As DO_MultiGrafik = DirectCast(template_compiled.getGrafik(), DO_MultiGrafik)
        If mydeko IsNot Nothing AndAlso mydeko.Count > 0 Then
            For i As Integer = 0 To mydeko.Count - 1
                g.childs.Add(mydeko(i).getGrafik())
            Next
        End If

        Dim t As New TransformMulti()
        t.add(New Transform_rotate(myDrehmatrix))
        t.add(New Transform_translate(position))
        g.transform(t)

        AddBeschriftungToGrafik(g, myBeschriftung, myTemplate, template_compiled, Me.position, myDrehmatrix)

        g.lineStyle.linestyle = Me.linestyle
        g.setLineStyleRekursiv(Me.linestyle)
        g.fillstyle = Me.myfillstyle
        g.setFillStyleRekursiv(Me.myfillstyle)

        g.setFontstyleRekursiv(Me.myfontstyle)

        Return g
    End Function

    Public Shared Sub AddBeschriftungToGrafik(g As DO_MultiGrafik, b As Beschriftung, template As TemplateAusDatei, Template_Compiled As Template_Compiled, bauteilPos As Point, drehmatrix As Drehmatrix)
        If b.positionIndex < template.getNrOfTextPoints() Then
            Dim tp As TextPoint = Template_Compiled.getTextpos(b.positionIndex)
            tp = drehmatrix.dreheTextPoint(tp)
            Dim pos_text As New Point(tp.pos.X + bauteilPos.X, tp.pos.Y + bauteilPos.Y)

            pos_text.X += b.abstand * tp.vektorAbstand.X
            pos_text.Y += b.abstand * tp.vektorAbstand.Y

            Dim normal As Point = New Point(-tp.vektorAbstand.Y, tp.vektorAbstand.X)
            pos_text.X += b.abstandQuer * normal.X
            pos_text.Y += b.abstandQuer * normal.Y

            Dim ah As DO_Text.AlignH
            If tp.xDir = 0 Then
                ah = DO_Text.AlignH.Mitte
            ElseIf tp.xDir < 0 Then
                ah = DO_Text.AlignH.Rechts
            Else
                ah = DO_Text.AlignH.Links
            End If
            Dim av As DO_Text.AlignV
            If tp.yDir = 0 Then
                av = DO_Text.AlignV.Mitte
            ElseIf tp.yDir < 0 Then
                av = DO_Text.AlignV.Unten
            Else
                av = DO_Text.AlignV.Oben
            End If

            Dim gText As New DO_Text(b.text, 0, pos_text, ah, av, b.textRot, False)

            g.childs.Add(gText)
        End If
    End Sub

    Public Overrides Function getSelection() As Selection
        Dim r As Rectangle = template_compiled.getSelectionRect()
        r = myDrehmatrix.transformRect(r)
        r.X += position.X
        r.Y += position.Y
        Return New SelectionRect(r)
    End Function

    Public Overrides Function NrOfSnappoints() As Integer
        Dim anzahl As Integer = template_compiled.getNrOfSnappoints()
        If mydeko IsNot Nothing Then
            For i As Integer = 0 To mydeko.Count - 1
                anzahl += mydeko(i).NrOfSnappoints()
            Next
        End If
        Return anzahl
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Dim p As Snappoint = Nothing
        If i < template_compiled.getNrOfSnappoints() Then
            p = template_compiled.getSnappoint(i)
        ElseIf mydeko IsNot Nothing Then
            i -= template_compiled.getNrOfSnappoints()
            For j As Integer = 0 To mydeko.Count - 1
                If i < mydeko(j).NrOfSnappoints() Then
                    p = mydeko(j).getSnappoint(i)
                    Exit For
                End If
                i -= mydeko(j).NrOfSnappoints()
            Next
            If p Is Nothing Then
                Throw New ArgumentException("Index i ist zu groß! Bei 'getSnappoint(i as Integer) as Snappoint'.")
            End If
        Else
            Throw New ArgumentException("Index i ist zu groß! Bei 'getSnappoint(i as Integer) as Snappoint'.")
        End If
        myDrehmatrix.transform(p)
        p.p.X += position.X
        p.p.Y += position.Y
        Return p
    End Function

    Public Function schalteBeschriftungsPosDurch() As Boolean
        Dim old As Integer = myBeschriftung.positionIndex
        myBeschriftung.positionIndex += 1
        If myBeschriftung.positionIndex >= myTemplate.getNrOfTextPoints() Then
            myBeschriftung.positionIndex = 0
        End If
        If old <> myBeschriftung.positionIndex Then
            'recompile!
            recompile()
            Return True
        End If
        Return False
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Public Function Clone_intern(newID As ULong) As ElementMaster
        Dim b As New BauteilAusDatei(newID, Me.position, myTemplate, myDrehmatrix, myBeschriftung, myfontstyle, myfillstyle)
        b.isSelected = Me.isSelected
        b.linestyle = Me.linestyle

        For i As Integer = 0 To Me.paramEinstellungen.Length - 1
            b.paramEinstellungen(i) = Me.paramEinstellungen(i).Copy()
        Next
        'Die Kopie hat erstmal kein _template_compiled, da es dann später recompiled wird.
        'Insbesondere für Rückgängig wichtig, um nicht unnötig speicherplatz mit zahlreichen kompilierten Templates zu verschwenden.
        'Sobald ein Rückgängig neu geladen wird, dann muss es halt nochmal neu kompiliert werden!
        b._myTemplate_compiled = Nothing

        If mydeko Is Nothing Then
            b.mydeko = Nothing
        Else
            b.mydeko = New List(Of Deko_Bauteil)(mydeko.Count)
            For i As Integer = 0 To mydeko.Count - 1
                b.addDeko(mydeko(i).Clone())
            Next
        End If

        Return b
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, d As Drehmatrix)
        Dim vector As New Point(position.X - drehpunkt.X, position.Y - drehpunkt.Y)
        Me.myDrehmatrix.drehen(d)
        vector = d.transformPoint(vector)
        Me.position = New Point(drehpunkt.X + vector.X, drehpunkt.Y + vector.Y)
    End Sub

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(isSelected)
        writer.Write(position.X)
        writer.Write(position.Y)
        writer.Write(myTemplate.getNameSpace())
        writer.Write(myTemplate.getName())
        writer.Write(myTemplate.getView())
        myDrehmatrix.speichern(writer)
        writer.Write(Me.linestyle)
        myBeschriftung.speichern(writer)
        writer.Write(myfontstyle)
        writer.Write(myfillstyle)

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
                writer.Write(-1) 'unbekannte (leere) Art!
            End If
        Next

        If mydeko Is Nothing Then
            writer.Write(-192635) 'Magic number für keine Deko!
        Else
            writer.Write(mydeko.Count)
            For i As Integer = 0 To mydeko.Count - 1
                mydeko(i).speichern(writer)
            Next
        End If
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean, fillStile_VersionKleiner27_transparent As Integer) As BauteilAusDatei
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim _namespace As String = reader.ReadString()
        Dim _name As String = reader.ReadString()
        Dim _view As String = reader.ReadString()
        Dim template As TemplateAusDatei = Nothing
        Dim templateCompatibility As TemplateCompatibility = Nothing

        Dim hatSchongeladen As Boolean = False
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
                hatSchongeladen = True
            End If
        End If
        If Not hatSchongeladen Then
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
        Dim drehung As Drehmatrix = Drehmatrix.Einlesen(reader, version)
        Dim linestyle As Integer = reader.ReadInt32()
        Dim text As Beschriftung = Beschriftung.Einlesen(reader, version)
        Dim fontstyle As Integer = 0
        If version >= 11 Then
            fontstyle = reader.ReadInt32()
        End If
        Dim fillstyle As Integer = fillStile_VersionKleiner27_transparent
        If version >= 27 Then
            fillstyle = reader.ReadInt32()
        End If

        Dim b As BauteilAusDatei
        If template IsNot Nothing Then
            b = New BauteilAusDatei(sender.getNewID(), New Point(posX, posY), template, drehung, text, fontstyle, fillstyle)
            '"old" param values laden (Zur Kompatibilität mit "old" definiert). Wenn es eine neue Datei ist werden diese Parameter gleich wieder überschrieben!
            For i As Integer = 0 To b.paramEinstellungen.Count - 1
                If TypeOf template.getParameter(i) Is TemplateParameter_Param Then
                    With DirectCast(template.getParameter(i), TemplateParameter_Param)
                        If .oldOption >= 0 AndAlso .oldOption < .options.Length Then
                            b.paramEinstellungen(i) = New ParamInt(.oldOption)
                        End If
                    End With
                End If
            Next
        Else
            Dim dx As Integer = templateCompatibility.dx
            Dim dy As Integer = templateCompatibility.dy
            b = New BauteilAusDatei(sender.getNewID(), New Point(posX + dx, posY + dy), templateCompatibility.parent, drehung, text, fontstyle, fillstyle)
            template = b.myTemplate
            '"old" param values laden (Zur Kompatibilität mit "old" definiert). Wenn es eine neue Datei ist werden diese Parameter gleich wieder überschrieben!
            For i As Integer = 0 To b.paramEinstellungen.Count - 1
                If TypeOf template.getParameter(i) Is TemplateParameter_Param Then
                    With DirectCast(template.getParameter(i), TemplateParameter_Param)
                        If .oldOption >= 0 AndAlso .oldOption < .options.Length Then
                            b.paramEinstellungen(i) = New ParamInt(.oldOption)
                        End If
                    End With
                End If
            Next
            'Laden der Template Compatibility Parameter erst nachdem die old-Values gelesen wurden! Da Template-Compatibility ja die Old-Werte überschreiben muss!
            templateCompatibility.LoadParams(b.paramEinstellungen)
        End If

        b.linestyle = linestyle
        b.isSelected = isSelected

        Dim anzahlEinstellungen As Integer = reader.ReadInt32()
        For i As Integer = 0 To anzahlEinstellungen - 1
            Dim paramArt As Integer = 0
            If version >= 6 Then
                paramArt = reader.ReadInt32()
            End If
            If paramArt = 0 Then 'Combobox Param
                Dim nameE As String = reader.ReadString()
                Dim valueE As Integer = reader.ReadInt32()
                For k As Integer = 0 To b.myTemplate.getNrOfParams() - 1
                    If TypeOf b.myTemplate.getParameter(k) Is TemplateParameter_Param Then
                        If DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Param).name.get_ID() = nameE AndAlso DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Param).options.Length > valueE Then
                            b.paramEinstellungen(k) = New ParamInt(valueE)
                        End If
                    End If
                Next
            ElseIf paramArt = 1 Then 'Arrow Param
                Dim nameE As String = reader.ReadString()
                Dim valueE As ParamArrow
                If version >= 10 Then
                    Dim art As Integer = reader.ReadInt32()
                    Dim size As Integer = reader.ReadInt32()
                    valueE = New ParamArrow(CShort(art), CUShort(size))
                Else
                    valueE = New ParamArrow(CShort(reader.ReadInt32()), 100)
                End If
                For k As Integer = 0 To b.myTemplate.getNrOfParams() - 1
                    If TypeOf b.myTemplate.getParameter(k) Is TemplateParameter_Arrow Then
                        If DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Arrow).name.get_ID() = nameE Then
                            Dim intervall As Intervall = DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Arrow).intervall
                            If valueE.pfeilArt >= intervall.min AndAlso valueE.pfeilArt <= intervall.max Then
                                b.paramEinstellungen(k) = valueE
                            End If
                        End If
                    End If
                Next
            ElseIf paramArt = 2 Then 'Int Param
                Dim nameE As String = reader.ReadString()
                Dim valueE As Integer = reader.ReadInt32()
                For k As Integer = 0 To b.myTemplate.getNrOfParams() - 1
                    If TypeOf b.myTemplate.getParameter(k) Is TemplateParameter_Int Then
                        If DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Int).name.get_ID() = nameE Then
                            Dim intervall As Intervall = DirectCast(b.myTemplate.getParameter(k), TemplateParameter_Int).intervall
                            If valueE >= intervall.min AndAlso valueE <= intervall.max AndAlso (CLng(valueE) - intervall.min) Mod CLng(intervall._step) = 0 Then
                                b.paramEinstellungen(k) = New ParamInt(valueE)
                            End If
                        End If
                    End If
                Next
            ElseIf paramArt = 3 Then 'String Param
                Dim nameE As String = reader.ReadString()
                Dim valueE As String = reader.ReadString()
                For k As Integer = 0 To b.myTemplate.getNrOfParams() - 1
                    If TypeOf b.myTemplate.getParameter(k) Is TemplateParameter_String Then
                        If DirectCast(b.myTemplate.getParameter(k), TemplateParameter_String).name.get_ID() = nameE Then
                            b.paramEinstellungen(k) = New ParamString(valueE)
                        End If
                    End If
                Next
            End If
        Next

        If version >= 8 Then
            'Einlesen der Deko!
            Dim dekoCount As Integer = reader.ReadInt32()
            If dekoCount = -192635 Then 'Magic number für keine Deko!
                b.mydeko = Nothing
            ElseIf dekoCount < 0 Then
                Throw New Exception("Fehler beim Einlesen. Fehler D7000.")
            ElseIf dekoCount = 0 Then
                b.mydeko = Nothing
            Else
                b.mydeko = New List(Of Deko_Bauteil)(dekoCount)
                For i As Integer = 0 To dekoCount - 1
                    b.addDeko(Deko_Bauteil.Einlesen(sender, reader, version, bib, lokaleBib, kompatibilität))
                Next
            End If
        Else
            b.mydeko = Nothing
        End If

        Return b
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot BauteilAusDatei Then Return False
        If e2.ID <> Me.ID Then Return False
        With DirectCast(e2, BauteilAusDatei)
            If Me.position <> .position Then Return False
            If Not Me.myTemplate.Equals(.myTemplate) Then Return False
            If Not myDrehmatrix.isEqual(.myDrehmatrix) Then Return False
            If Me.linestyle <> .linestyle Then Return False
            If Not Me.myBeschriftung.isEqual(.myBeschriftung) Then Return False
            If Me.myfontstyle <> .myfontstyle Then Return False
            If Me.myfillstyle <> .myfillstyle Then Return False
            If Me.paramEinstellungen.Length <> .paramEinstellungen.Length Then Return False
            For i As Integer = 0 To .paramEinstellungen.Length - 1
                If Not Me.paramEinstellungen(i).isEqual(.paramEinstellungen(i)) Then
                    Return False
                End If
            Next

            If (Me.mydeko Is Nothing) <> (.mydeko Is Nothing) Then Return False
            If Me.mydeko IsNot Nothing Then
                If Me.mydeko.Count <> .mydeko.Count Then Return False
                For i As Integer = 0 To Me.mydeko.Count - 1
                    If Not Me.mydeko(i).isEqualExceptSelection(.mydeko(i)) Then
                        Return False
                    End If
                Next
            End If


        End With
        Return True
    End Function

    Public Function getBeschriftung_Text() As String
        Return myBeschriftung.text
    End Function

    Public Sub setBeschriftung_Text(val As String)
        myBeschriftung.text = val
    End Sub

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox, mode As ElementEinstellung.combineModus) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        If myTemplate.getNrOfParams() > 0 Then
            Dim e1 As New Einstellung_Multi("Parameter", False)
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
            l.Add(e1)
        End If
        myBeschriftung.addEinstellungen(l)
        l.Add(New Einstellung_Fontstyle(EINSTELLUNG_FONTSTYLE, Me.myfontstyle, sender.myFonts))
        If template_compiled.benötigt_Fillstil() Then
            l.Add(New Einstellung_Fillstil(EINSTELLUNG_FILLSTYLE, Me.myfillstyle, sender.myFillStyles))
        End If

        l.AddRange(MyBase.getEinstellungen(sender, mode))
        If mydeko IsNot Nothing Then
            For i As Integer = 0 To mydeko.Count - 1
                Dim e1 As New Einstellung_Multi(getDekoParamName(i), True)
                mydeko(i).getEinstellungen(sender, e1)
                l.Add(e1)
            Next
        End If
        Return l
    End Function

    Private Function getDekoParamName(i As Integer) As String
        Return "Subelement " & (i + 1) & ": " & mydeko(i).getTemplate().getName()
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        Dim params_changed As Boolean = False
        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Multi Then
                If e.Name.get_ID() = "Parameter" Then
                    For Each eSub As Einstellung_TemplateParam In DirectCast(e, Einstellung_Multi).getListe()
                        If TypeOf eSub Is Einstellung_TemplateParameter Then
                            With DirectCast(eSub, Einstellung_TemplateParameter)
                                If .nrChanged Then
                                    For i As Integer = 0 To myTemplate.getNrOfParams - 1
                                        If TypeOf myTemplate.getParameter(i) Is TemplateParameter_Param Then
                                            If DirectCast(myTemplate.getParameter(i), TemplateParameter_Param).name.get_ID() = .Name.get_ID() Then
                                                Me.paramEinstellungen(i) = New ParamInt(.myNr)
                                                changed = True
                                                params_changed = True
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
                                                    params_changed = True
                                                End If
                                                If .sizeChanged Then
                                                    DirectCast(Me.paramEinstellungen(i), ParamArrow).pfeilSize = .myNr.pfeilSize
                                                    changed = True
                                                    params_changed = True
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
                                                params_changed = True
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
                                                params_changed = True
                                                Exit For
                                            End If
                                        End If
                                    Next
                                End If
                            End With
                        End If
                    Next
                Else
                    If mydeko IsNot Nothing Then
                        For i As Integer = 0 To mydeko.Count - 1
                            If e.Name.get_ID() = getDekoParamName(i) Then
                                If DirectCast(e, Einstellung_Multi).istGeschlossen Then
                                    mydeko.RemoveAt(i)
                                    changed = True
                                Else
                                    If mydeko(i).setEinstellungen(sender, DirectCast(e, Einstellung_Multi)) Then
                                        changed = True
                                    End If
                                End If
                                Exit For
                            End If
                        Next
                    End If
                End If
            ElseIf TypeOf e Is Einstellung_Fontstyle AndAlso e.Name.get_ID() = EINSTELLUNG_FONTSTYLE Then
                Me.myfontstyle = DirectCast(e, Einstellung_Fontstyle).getNewFontstyle(Me.myfontstyle, sender.myFonts, changed, False)
            ElseIf TypeOf e Is Einstellung_Fillstil AndAlso e.Name.get_ID() = EINSTELLUNG_FILLSTYLE Then
                Me.myfillstyle = DirectCast(e, Einstellung_Fillstil).getNewFillstil(Me.myfillstyle, sender.myFillStyles, changed)
            Else
                If myBeschriftung.setEinstellung(e) Then
                    changed = True
                    params_changed = True
                End If
            End If
        Next

        If params_changed Then
            'recompile!
            recompile()
        End If
        Return changed
    End Function

    Private Function getScaling(index As Integer) As ScalingLinie
        Dim s As ScalingLinie = template_compiled.getScalingLinie(index)
        s.transform(New Transform_rotate(myDrehmatrix))
        s.transform(New Transform_translate(position))
        Return s
    End Function

    Public Overrides Function getScaleKantenCount() As Integer
        Return template_compiled.getNrOfScalingLinien()
    End Function

    Public Overrides Function getScaleKante(index As Integer, alteKante As Kante) As Kante
        Dim s As ScalingLinie = getScaling(index)

        If alteKante Is Nothing Then
            Return New Kante(s.p1, s.p2, index, False, Me, True)
        Else
            alteKante.start = s.p1
            alteKante.ende = s.p2
            Return alteKante
        End If
    End Function

    Public Overrides Function ScaleKante(kante As Kante, dx As Integer, dy As Integer, ByRef out_invalidate_screen As Boolean) As Boolean
        dx += kante.temp_remainingX
        dy += kante.temp_remainingY

        Dim s As ScalingLinie = getScaling(kante.KantenIndex)

        'Projektion von (dx,dy) auf s.vec
        Dim scalar As Long = CLng(s.vec.X) * dx + CLng(s.vec.Y) * dy
        Dim len As Long = CLng(s.vec.X) * s.vec.X + CLng(s.vec.Y) * s.vec.Y

        If len = 0 Then
            Return False
        End If

        Dim alpha As Integer = CInt(Math.Round(scalar / len))

        'jetzt muss alpha auf das Interval angepasst werden!
        If alpha < s.min Then
            alpha = s.min
        ElseIf alpha > s.max Then
            alpha = s.max
        Else
            alpha -= s.min
            alpha = CInt(Math.Round(alpha / s._step)) * s._step
            alpha += s.min
            If alpha < s.min Then alpha = s.min
            If alpha > s.max Then alpha = s.max
        End If

        If alpha <> 0 Then
            'jetzt muss das Callback für diesen Wert von alpha ausgeführt werden und dann das template neu kompiliert werden!
            If recompileScaling(s.callbackNr, alpha, alpha * CInt(Math.Round(Math.Sqrt(len)))) Then
                recompile()
                out_invalidate_screen = True
            End If
        End If

        kante.temp_remainingX = dx - alpha * s.vec.X
        kante.temp_remainingY = dy - alpha * s.vec.Y
        Return True
    End Function

    Public Overrides Sub markAllUsedFillstyles(usedFillstyles() As Boolean)
        MyBase.markAllUsedFillstyles(usedFillstyles)
        usedFillstyles(Me.myfillstyle) = True
    End Sub

    Public Function get_fillstyle() As Integer Implements IElementWithFill.get_fillstyle
        Return myfillstyle
    End Function

    Public Sub set_fillstyle(fs As Integer) Implements IElementWithFill.set_fillstyle
        Me.myfillstyle = fs
    End Sub

    Public Function get_fontstyle() As Integer Implements IElementWithFont.get_fontstyle
        Return Me.myfontstyle
    End Function

    Public Sub set_fontstyle(fs As Integer) Implements IElementWithFont.set_fontstyle
        Me.myfontstyle = fs
    End Sub
End Class

Public Structure Drehmatrix
    Private drehung As Drehungen

    Public Shared Drehen90Grad As Drehmatrix = New Drehmatrix(Drehungen.Rot90)
    Public Shared DrehenMinus90Grad As Drehmatrix = New Drehmatrix(Drehungen.Rot270)
    Public Shared MirrorX As Drehmatrix = New Drehmatrix(Drehungen.MirrorX)
    Public Shared MirrorY As Drehmatrix = New Drehmatrix(Drehungen.MirrorXRot180)

    Public Sub New(d As Drehungen)
        Me.drehung = d
    End Sub

    Public Sub um90GradDrehen()
        Select Case drehung
            Case Drehungen.Normal
                Me.drehung = Drehungen.Rot90
            Case Drehungen.Rot90
                Me.drehung = Drehungen.Rot180
            Case Drehungen.Rot180
                Me.drehung = Drehungen.Rot270
            Case Drehungen.Rot270
                Me.drehung = Drehungen.Normal
            Case Drehungen.MirrorX
                Me.drehung = Drehungen.MirrorXRot90
            Case Drehungen.MirrorXRot90
                Me.drehung = Drehungen.MirrorXRot180
            Case Drehungen.MirrorXRot180
                Me.drehung = Drehungen.MirrorXRot270
            Case Drehungen.MirrorXRot270
                Me.drehung = Drehungen.MirrorX
        End Select
    End Sub

    Public Sub umMinus90GradDrehen()
        Select Case drehung
            Case Drehungen.Normal
                Me.drehung = Drehungen.Rot270
            Case Drehungen.Rot90
                Me.drehung = Drehungen.Normal
            Case Drehungen.Rot180
                Me.drehung = Drehungen.Rot90
            Case Drehungen.Rot270
                Me.drehung = Drehungen.Rot180
            Case Drehungen.MirrorX
                Me.drehung = Drehungen.MirrorXRot270
            Case Drehungen.MirrorXRot90
                Me.drehung = Drehungen.MirrorX
            Case Drehungen.MirrorXRot180
                Me.drehung = Drehungen.MirrorXRot90
            Case Drehungen.MirrorXRot270
                Me.drehung = Drehungen.MirrorXRot180
        End Select
    End Sub

    Public Sub SpielgenX()
        Select Case drehung
            Case Drehungen.Normal
                Me.drehung = Drehungen.MirrorX
            Case Drehungen.Rot90
                Me.drehung = Drehungen.MirrorXRot270
            Case Drehungen.Rot180
                Me.drehung = Drehungen.MirrorXRot180
            Case Drehungen.Rot270
                Me.drehung = Drehungen.MirrorXRot90
            Case Drehungen.MirrorX
                Me.drehung = Drehungen.Normal
            Case Drehungen.MirrorXRot90
                Me.drehung = Drehungen.Rot270
            Case Drehungen.MirrorXRot180
                Me.drehung = Drehungen.Rot180
            Case Drehungen.MirrorXRot270
                Me.drehung = Drehungen.Rot90
        End Select
    End Sub

    Public Sub SpielgenY()
        Select Case drehung
            Case Drehungen.Normal
                Me.drehung = Drehungen.MirrorXRot180
            Case Drehungen.Rot90
                Me.drehung = Drehungen.MirrorXRot90
            Case Drehungen.Rot180
                Me.drehung = Drehungen.MirrorX
            Case Drehungen.Rot270
                Me.drehung = Drehungen.MirrorXRot270
            Case Drehungen.MirrorX
                Me.drehung = Drehungen.Rot180
            Case Drehungen.MirrorXRot90
                Me.drehung = Drehungen.Rot90
            Case Drehungen.MirrorXRot180
                Me.drehung = Drehungen.Normal
            Case Drehungen.MirrorXRot270
                Me.drehung = Drehungen.Rot270
        End Select
    End Sub

    Public Sub drehen(d2 As Drehmatrix)
        Select Case d2.drehung
            Case Drehungen.Rot90
                um90GradDrehen()
            Case Drehungen.Rot270
                umMinus90GradDrehen()
            Case Drehungen.MirrorX
                SpielgenX()
            Case Drehungen.MirrorXRot180
                SpielgenY()
            Case Drehungen.Rot180
                um90GradDrehen()
                um90GradDrehen()
            Case Drehungen.MirrorXRot270
                SpielgenY()
                um90GradDrehen()
            Case Drehungen.MirrorXRot90
                SpielgenX()
                um90GradDrehen()
            Case Drehungen.Normal
                'mache nichts!
            Case Else
                Throw New NotImplementedException()
        End Select
    End Sub

    Public Sub transform(p As Snappoint)
        p.p = transformPoint(p.p)
        Dim xminus, xplus, yminus, yplus As Integer
        Select Case drehung
            Case Drehungen.Normal
                xplus = p.Xplus
                xminus = p.Xminus
                yplus = p.Yplus
                yminus = p.Yminus
            Case Drehungen.Rot90
                yplus = p.Xplus
                xminus = p.Yplus
                yminus = p.Xminus
                xplus = p.Yminus
            Case Drehungen.Rot180
                xminus = p.Xplus
                yminus = p.Yplus
                xplus = p.Xminus
                yplus = p.Yminus
            Case Drehungen.Rot270
                yminus = p.Xplus
                xplus = p.Yplus
                yplus = p.Xminus
                xminus = p.Yminus
            Case Drehungen.MirrorX
                xminus = p.Xplus
                yminus = p.Yminus
                xplus = p.Xminus
                yplus = p.Yplus
            Case Drehungen.MirrorXRot90
                yplus = p.Xminus
                xminus = p.Yplus
                yminus = p.Xplus
                xplus = p.Yminus
            Case Drehungen.MirrorXRot180
                xminus = p.Xminus
                yminus = p.Yplus
                xplus = p.Xplus
                yplus = p.Yminus
            Case Drehungen.MirrorXRot270
                yminus = p.Xminus
                xplus = p.Yplus
                yplus = p.Xplus
                xminus = p.Yminus
        End Select
        p.Xminus = xminus
        p.Xplus = xplus
        p.Yminus = yminus
        p.Yplus = yplus
    End Sub

    Public Function transformPoint(p As Point) As Point
        Select Case drehung
            Case Drehungen.Normal
                Return p
            Case Drehungen.Rot90
                Return New Point(-p.Y, p.X)
            Case Drehungen.Rot180
                Return New Point(-p.X, -p.Y)
            Case Drehungen.Rot270
                Return New Point(p.Y, -p.X)
            Case Drehungen.MirrorX
                Return New Point(-p.X, p.Y)
            Case Drehungen.MirrorXRot90
                Return New Point(-p.Y, -p.X)
            Case Drehungen.MirrorXRot180
                Return New Point(p.X, -p.Y)
            Case Drehungen.MirrorXRot270
                Return New Point(p.Y, p.X)
        End Select
    End Function

    Public Function transformPointF(p As PointF) As PointF
        Select Case drehung
            Case Drehungen.Normal
                Return p
            Case Drehungen.Rot90
                Return New PointF(-p.Y, p.X)
            Case Drehungen.Rot180
                Return New PointF(-p.X, -p.Y)
            Case Drehungen.Rot270
                Return New PointF(p.Y, -p.X)
            Case Drehungen.MirrorX
                Return New PointF(-p.X, p.Y)
            Case Drehungen.MirrorXRot90
                Return New PointF(-p.Y, -p.X)
            Case Drehungen.MirrorXRot180
                Return New PointF(p.X, -p.Y)
            Case Drehungen.MirrorXRot270
                Return New PointF(p.Y, p.X)
        End Select
    End Function

    Public Function transformRect(r As Rectangle) As Rectangle
        Dim p1 As Point = New Point(r.X, r.Y)
        Dim p2 As Point = New Point(r.X + r.Width, r.Y + r.Height)
        p1 = transformPoint(p1)
        p2 = transformPoint(p2)
        Dim minX As Integer = Math.Min(p1.X, p2.X)
        Dim minY As Integer = Math.Min(p1.Y, p2.Y)
        Dim maxX As Integer = Math.Max(p1.X, p2.X)
        Dim maxY As Integer = Math.Max(p1.Y, p2.Y)
        Return New Rectangle(minX, minY, maxX - minX, maxY - minY)
    End Function

    Public Function transformRectF(r As RectangleF) As RectangleF
        Dim p1 As PointF = New PointF(r.X, r.Y)
        Dim p2 As PointF = New PointF(r.X + r.Width, r.Y + r.Height)
        p1 = transformPointF(p1)
        p2 = transformPointF(p2)
        Dim minX As Single = Math.Min(p1.X, p2.X)
        Dim minY As Single = Math.Min(p1.Y, p2.Y)
        Dim maxX As Single = Math.Max(p1.X, p2.X)
        Dim maxY As Single = Math.Max(p1.Y, p2.Y)
        Return New RectangleF(minX, minY, maxX - minX, maxY - minY)
    End Function

    Public Function dreheTextPoint(tp As TextPoint) As TextPoint
        Dim pNeu As Point = transformPoint(tp.pos)
        Dim vNeu As Point = transformPoint(New Point(tp.xDir, tp.yDir))
        Dim v2Neu As Point = transformPoint(tp.vektorAbstand)
        Return New TextPoint(pNeu, vNeu.X, vNeu.Y, v2Neu)
    End Function

    Public Function isEqual(d As Drehmatrix) As Boolean
        Return d.drehung = Me.drehung
    End Function

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(CInt(drehung))
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As Drehmatrix
        Dim d As Integer = reader.ReadInt32()
        Return New Drehmatrix(CType(d, Drehungen))
    End Function

    Public Shared Function getIdentity() As Drehmatrix
        Return New Drehmatrix(Drehungen.Normal)
    End Function

    Public Function getDrehung() As Drehungen
        Return drehung
    End Function

    Public Function getInverse() As Drehmatrix
        Select Case drehung
            Case Drehungen.Normal
                Return New Drehmatrix(Drehungen.Normal)
            Case Drehungen.Rot90
                Return New Drehmatrix(Drehungen.Rot270)
            Case Drehungen.Rot180
                Return New Drehmatrix(Drehungen.Rot180)
            Case Drehungen.Rot270
                Return New Drehmatrix(Drehungen.Rot90)
            Case Drehungen.MirrorX
                Return New Drehmatrix(Drehungen.MirrorX)
            Case Drehungen.MirrorXRot90
                Return New Drehmatrix(Drehungen.MirrorXRot90)
            Case Drehungen.MirrorXRot180
                Return New Drehmatrix(Drehungen.MirrorXRot180)
            Case Drehungen.MirrorXRot270
                Return New Drehmatrix(Drehungen.MirrorXRot270)
        End Select
        Throw New NotImplementedException()
    End Function

    Public Function getMatrix() As Integer()
        Select Case drehung
            Case Drehungen.Normal
                Return {1, 0, 0, 1}
            Case Drehungen.Rot90
                Return {0, -1, 1, 0}
            Case Drehungen.Rot180
                Return {-1, 0, 0, -1}
            Case Drehungen.Rot270
                Return {0, 1, -1, 0}
            Case Drehungen.MirrorX
                Return {-1, 0, 0, 1}
            Case Drehungen.MirrorXRot90
                Return {0, -1, -1, 0}
            Case Drehungen.MirrorXRot180
                Return {1, 0, 0, -1}
            Case Drehungen.MirrorXRot270
                Return {0, 1, 1, 0}
        End Select
        Throw New NotImplementedException()
    End Function

    Public Shared Function getDrehmatrixFromNr(nr As Integer) As Drehmatrix
        Return New Drehmatrix(CType(nr, Drehungen))
    End Function

    Public Enum Drehungen 'Alle Drehungen gegen den Uhrzeigersinn im (X,Y) Koordinatensystem. Auf dem Bildschirm dann mit der Uhr, wegen y'=-y
        Normal = 0
        Rot90 = 1
        Rot180 = 2
        Rot270 = 3
        MirrorX = 4
        MirrorXRot90 = 5
        MirrorXRot180 = 6 'gleich MirrorY
        MirrorXRot270 = 7
    End Enum

End Structure

Public Structure Beschriftung
    Public text As String
    Public positionIndex As Integer
    Public textRot As DO_Text.TextRotation
    Public abstand As Integer
    Public abstandQuer As Integer

    Public Sub New(text As String, pos As Integer, textRot As DO_Text.TextRotation, abstand As Integer, abstandQuer As Integer)
        Me.text = text
        Me.positionIndex = pos
        Me.textRot = textRot
        Me.abstand = abstand
        Me.abstandQuer = abstandQuer
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(text)
        writer.Write(positionIndex)
        writer.Write(CInt(textRot))
        writer.Write(abstand)
        writer.Write(abstandQuer)
    End Sub

    Public Shared Function Einlesen(reader As BinaryReader, version As Integer) As Beschriftung
        Dim text As String = reader.ReadString()
        Dim pos As Integer = reader.ReadInt32()
        Dim rot As DO_Text.TextRotation = DO_Text.TextRotation.Normal
        If version >= 4 Then
            rot = CType(reader.ReadInt32(), DO_Text.TextRotation)
        End If
        Dim abstand As Integer = 0
        Dim abstandQuer As Integer = 0
        If version >= 5 Then
            abstand = reader.ReadInt32()
            abstandQuer = reader.ReadInt32()
        End If
        Return New Beschriftung(text, pos, rot, abstand, abstandQuer)
    End Function

    Public Function isEqual(b As Beschriftung) As Boolean
        If Me.text <> b.text Then Return False
        If Me.positionIndex <> b.positionIndex Then Return False
        If Me.textRot <> b.textRot Then Return False
        If Me.abstand <> b.abstand Then Return False
        If Me.abstandQuer <> b.abstandQuer Then Return False
        Return True
    End Function

    Public Sub addEinstellungen(l As List(Of ElementEinstellung))
        l.Add(New EinstellungBeschriftung(Element.EINSTELLUNG_BESCHRIFTUNG, Me))
        'l.Add(New Einstellung_Pos(Element.EINSTELLUNG_ABSTAND_BESCHRIFTUNG, New Point(abstand, abstandQuer), Element.EINSTELLUNGNAME_ABSTAND_BESCHRIFTUNG, Element.EINSTELLUNGNAME_ABSTAND_QUER))
    End Sub

    Public Function setEinstellung(e As ElementEinstellung) As Boolean
        Dim changed As Boolean = False
        If TypeOf e Is EinstellungBeschriftung AndAlso e.Name.get_ID() = Element.EINSTELLUNG_BESCHRIFTUNG Then
            Dim neu As Beschriftung = DirectCast(e, EinstellungBeschriftung).getNewValue(Me, changed)
            Me.text = neu.text
            Me.positionIndex = neu.positionIndex
            Me.textRot = neu.textRot
            Me.abstand = neu.abstand
            Me.abstandQuer = neu.abstandQuer
            'ElseIf TypeOf e Is Einstellung_Pos AndAlso e.Name = Element.EINSTELLUNG_ABSTAND_BESCHRIFTUNG Then
            '    With DirectCast(e, Einstellung_Pos)
            '        If .changedX Then
            '            abstand = .pos.X
            '            changed = True
            '        End If
            '        If .changedY Then
            '            abstandQuer = .pos.Y
            '            changed = True
            '        End If
            '    End With
        End If
        Return changed
    End Function
End Structure
