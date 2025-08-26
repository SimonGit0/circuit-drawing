Imports System.IO
Public MustInherit Class Ausdruck
    Public MustOverride Function vereinfacheSoweitMöglich() As Ausdruck

    Public Shared Sub speichernAllgemein(ausdruck As Ausdruck, writer As BinaryWriter)
        If TypeOf ausdruck Is Ausdruck_Int Then
            writer.Write(1)
            Ausdruck_Int.speichern(DirectCast(ausdruck, Ausdruck_Int), writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Boolean Then
            writer.Write(2)
            Ausdruck_Boolean.speichern(DirectCast(ausdruck, Ausdruck_Boolean), writer)
        ElseIf TypeOf ausdruck Is Ausdruck_Pfeil Then
            writer.Write(3)
            Ausdruck_Pfeil.speichern(DirectCast(ausdruck, Ausdruck_Pfeil), writer)
        ElseIf TypeOf ausdruck Is AusdruckString Then
            writer.Write(4)
            AusdruckString.speichern(DirectCast(ausdruck, AusdruckString), writer)
        Else
            Throw New Exception("Fehler P0005: Kann diesen Ausdruck nicht speichern!")
        End If
    End Sub

    Public Shared Function ladenAllgemein(reader As BinaryReader, version As Integer) As Ausdruck
        Dim art As Integer = reader.ReadInt32()
        Select Case art
            Case 1 'Ausdruck_Int
                Return Ausdruck_Int.laden(reader, version)
            Case 2 'Ausdruck_Boolean
                Return Ausdruck_Boolean.laden(reader, version)
            Case 3 'Ausdruck_Pfeil
                Return Ausdruck_Pfeil.laden(reader, version)
            Case 4 'AusdruckString
                Return AusdruckString.laden(reader, version)
            Case Else
                Throw New Exception("Fehler L0005: Kann diesen Ausdruck nicht laden!")
        End Select
    End Function

    Public Shared Function exportAllgemein(ausdruck As Ausdruck, writer As Export_StreamWriter) As String
        If TypeOf ausdruck Is Ausdruck_Int Then
            Return Ausdruck_Int.export(DirectCast(ausdruck, Ausdruck_Int), writer).str
        ElseIf TypeOf ausdruck Is Ausdruck_Boolean Then
            Return Ausdruck_Boolean.export(DirectCast(ausdruck, Ausdruck_Boolean), writer).str
        ElseIf TypeOf ausdruck Is Ausdruck_Pfeil Then
            Return Ausdruck_Pfeil.export(DirectCast(ausdruck, Ausdruck_Pfeil), writer).str
        ElseIf TypeOf ausdruck Is AusdruckString Then
            Return AusdruckString.export(DirectCast(ausdruck, AusdruckString), writer).str
        Else
            Throw New Exception("Fehler P1005: Kann diesen Ausdruck nicht exportieren!")
        End If
    End Function

    Public Function getArt() As VariableEinlesen.VariableArt
        If TypeOf Me Is Ausdruck_Int Then
            Return VariableEinlesen.VariableArt.Int_
        ElseIf TypeOf Me Is Ausdruck_Boolean Then
            Return VariableEinlesen.VariableArt.Boolean_
        ElseIf TypeOf Me Is AusdruckString Then
            Return VariableEinlesen.VariableArt.String_
        Else
            Throw New Exception("Fehler P0009: Unbekannter Ausdruck")
        End If
    End Function

    Public Overridable Function ist_Konstante() As Boolean
        Return False
    End Function

#Region "Auswerten"
    Public Shared Function EinlesenAusdruck(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        str = Mathe.strToLower(Mathe.string_vorm_auswertenAufbereiten(str))
        Dim erg As Ausdruck = auswerten(str, konst_lokal, parameter, vars_Intern_lokal, params)
        Return erg.vereinfacheSoweitMöglich()
    End Function

    Private Shared Function auswerten(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        str = str.Trim()

        'Einfache Konstanten, oder direkt ausrechenbare Integer
        If str = "true" Then Return New Ausdruck_Boolean_Konstante(True)
        If str = "false" Then Return New Ausdruck_Boolean_Konstante(False)
        Dim resultParse As Integer
        If Integer.TryParse(str, resultParse) Then Return New Ausdruck_Konstante(resultParse)

        'Variablen ersetzen
        If vars_Intern_lokal IsNot Nothing Then
            For i As Integer = vars_Intern_lokal.Count - 1 To 0 Step -1
                If vars_Intern_lokal(i).name <> "" AndAlso vars_Intern_lokal(i).name = str Then
                    If vars_Intern_lokal(i).art = VariableEinlesen.VariableArt.Int_ Then
                        Return New Ausdruck_Variable(i, True)
                    ElseIf vars_Intern_lokal(i).art = VariableEinlesen.VariableArt.Boolean_ Then
                        Return New AusdruckBoolean_Variable(i, True)
                    ElseIf vars_Intern_lokal(i).art = VariableEinlesen.VariableArt.String_ Then
                        Return New AusdruckString_Variable(i, True)
                    End If
                End If
            Next
        End If
        'Konstanten ersetzen
        If konst_lokal IsNot Nothing AndAlso konst_lokal.ContainsKey(str) Then
            Return New Ausdruck_Konstante(konst_lokal(str))
        End If

        '|
        Dim erg As Ausdruck = auswertenOperatorOder(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        '&
        erg = auswertenOperatorUnd(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        'Vergleiche: <, >, <=, >=, =, !=
        'Außerdem werden direkt erkannt, wenn der Ausdruck komplett eingeklammert ist und die Klammern werden somit automatisch mit aufgelöst
        erg = auswertenOperatorVergleiche(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        '+, -
        erg = auswertenOperatorPlusMinus(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        '*, /
        erg = auswertenOperatorenMalGeteilt(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        'Anführungszeichen auflösen
        erg = auswertenOperatorenAnführungszeichen(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        'Nachdem alle Operatoren abgearbeitet sind nur noch prüfen ob es eine Funktion ist!
        erg = auswertenFunktionen(str, konst_lokal, parameter, vars_Intern_lokal, params)
        If erg IsNot Nothing Then
            Return erg
        End If

        'Keine Auflösung gefunden...
        Throw New NotImplementedException("Fehler beim ausrechnen des Wertes '" & str & "'.")
    End Function

    Private Shared Function getAusdruck_Bool(a As Ausdruck, b As Ausdruck, opCode As Integer, params As List(Of TemplateParameter)) As Ausdruck_Boolean
        Select Case opCode
            Case 0 '=
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_Gleich(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), False)
                ElseIf TypeOf a Is Ausdruck_Boolean AndAlso TypeOf b Is Ausdruck_Boolean Then
                    Return New Ausdruck_GleichBool(DirectCast(a, Ausdruck_Boolean), DirectCast(b, Ausdruck_Boolean), False)
                ElseIf TypeOf a Is AusdruckParam AndAlso TypeOf b Is AusdruckString_Konstante Then
                    Return getAusdruckEqual_Param(DirectCast(a, AusdruckParam), DirectCast(b, AusdruckString_Konstante), params, False)
                ElseIf TypeOf a Is AusdruckString_Konstante AndAlso TypeOf b Is AusdruckParam Then
                    Return getAusdruckEqual_Param(DirectCast(b, AusdruckParam), DirectCast(a, AusdruckString_Konstante), params, False)
                ElseIf TypeOf a Is AusdruckString AndAlso TypeOf b Is AusdruckString Then
                    Return New Ausdruck_GleichString(DirectCast(a, AusdruckString), DirectCast(b, AusdruckString), False)
                Else
                    Throw New Exception("Der Operator '=' ist für diese Typen nicht definiert!")
                End If
            Case 1 '!=
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_Gleich(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), True)
                ElseIf TypeOf a Is Ausdruck_Boolean AndAlso TypeOf b Is Ausdruck_Boolean Then
                    Return New Ausdruck_GleichBool(DirectCast(a, Ausdruck_Boolean), DirectCast(b, Ausdruck_Boolean), True)
                ElseIf TypeOf a Is AusdruckParam AndAlso TypeOf b Is AusdruckString_Konstante Then
                    Return getAusdruckEqual_Param(DirectCast(a, AusdruckParam), DirectCast(b, AusdruckString_Konstante), params, True)
                ElseIf TypeOf a Is AusdruckString_Konstante AndAlso TypeOf b Is AusdruckParam Then
                    Return getAusdruckEqual_Param(DirectCast(b, AusdruckParam), DirectCast(a, AusdruckString_Konstante), params, True)
                ElseIf TypeOf a Is AusdruckString AndAlso TypeOf b Is AusdruckString Then
                    Return New Ausdruck_GleichString(DirectCast(a, AusdruckString), DirectCast(b, AusdruckString), True)
                Else
                    Throw New Exception("Der Operator '!=' ist für diese Typen nicht definiert!")
                End If
            Case 2 '<
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_KleinerGrößer(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), True)
                Else
                    Throw New Exception("Der Operator '<' ist für diese Typen nicht definiert!")
                End If
            Case 3 '>
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_KleinerGrößer(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), False)
                Else
                    Throw New Exception("Der Operator '>' ist für diese Typen nicht definiert!")
                End If
            Case 4 '<=
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_KleinerGrößerGleich(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), True)
                Else
                    Throw New Exception("Der Operator '<=' ist für diese Typen nicht definiert!")
                End If
            Case 5 '>=
                If TypeOf a Is Ausdruck_Int AndAlso TypeOf b Is Ausdruck_Int Then
                    Return New Ausdruck_KleinerGrößerGleich(DirectCast(a, Ausdruck_Int), DirectCast(b, Ausdruck_Int), False)
                Else
                    Throw New Exception("Der Operator '>=' ist für diese Typen nicht definiert!")
                End If
        End Select
        Throw New Exception("Unbekannter Operator (OpCode = " & opCode & ")")
    End Function

    Private Shared Function getAusdruckEqual_Param(a As AusdruckParam, b As AusdruckString_Konstante, params As List(Of TemplateParameter), ungleich As Boolean) As Ausdruck_Gleich
        Dim value As String = b.Ausrechnen(Nothing).ToLower()
        Dim varNr As Integer = a.VarNr
        Dim param As TemplateParameter = params(DirectCast(a, AusdruckParam).ParamNr)
        If TypeOf param IsNot TemplateParameter_Param Then
            Throw New Exception("Parameter erwartet")
        End If
        Dim param_i As TemplateParameter_Param = DirectCast(param, TemplateParameter_Param)
        For i As Integer = 0 To param_i.options.Length - 1
            If param_i.options(i).get_ID().ToLower() = value Then
                Return New Ausdruck_Gleich(New Ausdruck_Variable(varNr, False), New Ausdruck_Konstante(i), ungleich)
            End If
        Next
        Throw New Exception("Die Option """ & value & """ ist für den Parameter """ & param_i.name.get_ID() & """ nicht definiert")
    End Function

    Public Shared Function getSubTermeAusdruck(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As List(Of Ausdruck)
        Dim strs() As String = Mathe.splitString(str, ","c)
        Dim subTerme As New List(Of Ausdruck)(strs.Length)
        For i As Integer = 0 To strs.Length - 1
            subTerme.Add(auswerten(strs(i), konst_lokal, parameter, vars_Intern_lokal, params))
        Next
        Return subTerme
    End Function

    Private Shared Function auswertenOperatorOder(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Dim str_ODER() As String = Mathe.splitString(str, "|"c)
        If str_ODER.Length > 1 Then
            Dim oders As New List(Of Ausdruck_Boolean)(str_ODER.Length)
            For i As Integer = 0 To str_ODER.Length - 1
                Dim ausdruck_i As Ausdruck = auswerten(str_ODER(i), konst_lokal, parameter, vars_Intern_lokal, params)
                If TypeOf ausdruck_i Is Ausdruck_Boolean Then
                    oders.Add(DirectCast(ausdruck_i, Ausdruck_Boolean))
                Else
                    Throw New Exception("Der Operator | ist nur für Boolean-Werte definiert ('" & str_ODER(i) & "')")
                End If
            Next
            Return New Ausdruck_ODER(oders)
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenOperatorUnd(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Dim str_UND() As String = Mathe.splitString(str, "&"c)
        If str_UND.Length > 1 Then
            Dim unds As New List(Of Ausdruck_Boolean)(str_UND.Length)
            For i As Integer = 0 To str_UND.Length - 1
                Dim ausdruck_i As Ausdruck = auswerten(str_UND(i), konst_lokal, parameter, vars_Intern_lokal, params)
                If TypeOf ausdruck_i Is Ausdruck_Boolean Then
                    unds.Add(DirectCast(ausdruck_i, Ausdruck_Boolean))
                Else
                    Throw New Exception("Der Operator & ist nur für Boolean-Werte definiert ('" & str_UND(i) & "')")
                End If
            Next
            Return New Ausdruck_UND(unds)
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenOperatorVergleiche(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Dim klammerpos As Integer = 0
        Dim inAnführungszeichen As Boolean = False
        Dim operatoren_und_leer As String = "<>=! "
        Dim op As String = ""
        Dim op_Code As Integer = -1 '0 =; 1 !=; 2 <; 3 >; 4 <=; 5 >=
        Dim operand As String = ""
        Dim operanden As New List(Of String)
        Dim opCodes As New List(Of Integer)
        Dim anzahlKlammerPos0 As Integer = 0
        Dim anzahlNichtInAnführungszeichen As Integer = 0
        Dim istOperator As Boolean
        For i As Integer = 0 To str.Length - 1
            istOperator = False

            If str(i) = """" Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                anzahlNichtInAnführungszeichen += 1
                If str(i) = "(" Then
                    klammerpos += 1
                ElseIf str(i) = ")" Then
                    klammerpos -= 1
                End If
                If klammerpos < 0 Then
                    Throw New Exception("Zu viele ')'")
                End If
                If klammerpos = 0 Then
                    anzahlKlammerPos0 += 1
                    If operatoren_und_leer.Contains(str(i)) Then
                        If str(i) <> " " Then
                            op &= str(i)
                        End If
                        istOperator = True
                    End If
                End If
            End If
            If Not istOperator Then
                If op <> "" Then
                    Select Case op
                        Case "="
                            op_Code = 0
                        Case "=="
                            op_Code = 0
                        Case "!="
                            op_Code = 1
                        Case "<"
                            op_Code = 2
                        Case ">"
                            op_Code = 3
                        Case "<="
                            op_Code = 4
                        Case "=<"
                            op_Code = 4
                        Case ">="
                            op_Code = 5
                        Case "=>"
                            op_Code = 5
                        Case Else
                            Throw New Exception("Unbekannter Operator '" & op & "'")
                    End Select
                    operanden.Add(operand)
                    opCodes.Add(op_Code)
                    op = ""
                    operand = ""
                End If
                operand &= str(i)
            End If
        Next

        If anzahlNichtInAnführungszeichen = 1 AndAlso str(0) = """" AndAlso str(str.Length - 1) = """" Then
            Return New AusdruckString_Konstante(str.Substring(1, str.Length - 2))
        End If

        If anzahlKlammerPos0 = 1 AndAlso str(0) = "(" AndAlso str(str.Length - 1) = ")" Then
            Return auswerten(str.Substring(1, str.Length - 2), konst_lokal, parameter, vars_Intern_lokal, params)
        End If

        If operand.Trim = "" Then
            Throw New Exception("Fehlender Operand am Ende der Anweisung '" & str & "'")
        End If
        operanden.Add(operand)

        If operanden.Count - 1 <> opCodes.Count Then
            Throw New Exception("Falsche Anzahl an Operatoren und Operanden")
        End If

        If operanden.Count > 1 Then
            Dim ausdruck_res As Ausdruck = auswerten(operanden(0).Trim(), konst_lokal, parameter, vars_Intern_lokal, params)
            Dim ausdruck_neu As Ausdruck
            For i As Integer = 0 To opCodes.Count - 1
                ausdruck_neu = auswerten(operanden(i + 1).Trim(), konst_lokal, parameter, vars_Intern_lokal, params)
                ausdruck_res = getAusdruck_Bool(ausdruck_res, ausdruck_neu, opCodes(i), params)
            Next
            If TypeOf ausdruck_res IsNot Ausdruck_Boolean Then
                Throw New Exception("Falscher Ergebnis-Typ des Vergleichs")
            End If
            Return ausdruck_res
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenOperatorPlusMinus(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        If str.StartsWith("+") Then
            Dim res As Ausdruck = auswerten(str.Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)
            If TypeOf res Is Ausdruck_Int Then
                Return res
            Else
                Throw New Exception("Der Operator '+' ist für diesen Datentyp nicht definiert. Integer erwartet.")
            End If
        End If
        Dim summanden() As String = Mathe.splitString(str, "+"c, "-"c)
        If summanden.Length > 1 Then
            Dim sum As New List(Of Tuple(Of Boolean, Ausdruck))
            For i As Integer = 0 To summanden.Count - 1
                If summanden(i).StartsWith("-") Then
                    sum.Add(New Tuple(Of Boolean, Ausdruck)(True, auswerten(summanden(i).Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)))
                ElseIf summanden(i).StartsWith("+") Then
                    sum.Add(New Tuple(Of Boolean, Ausdruck)(False, auswerten(summanden(i).Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)))
                Else
                    sum.Add(New Tuple(Of Boolean, Ausdruck)(False, auswerten(summanden(i), konst_lokal, parameter, vars_Intern_lokal, params)))
                End If
            Next
            Return New Ausdruck_Summe(sum)
        ElseIf summanden.Length = 1 Then
            If summanden(0).StartsWith("-") Then
                Dim sum As New List(Of Tuple(Of Boolean, Ausdruck))
                sum.Add(New Tuple(Of Boolean, Ausdruck)(True, auswerten(summanden(0).Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)))
                Return New Ausdruck_Summe(sum)
            End If
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenOperatorenMalGeteilt(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Dim faktoren() As String = Mathe.splitString(str, "*"c, "/"c)
        If faktoren.Length > 1 Then
            Dim fak As New List(Of Tuple(Of Boolean, Ausdruck))
            For i As Integer = 0 To faktoren.Count - 1
                If faktoren(i).StartsWith("/") Then
                    fak.Add(New Tuple(Of Boolean, Ausdruck)(True, auswerten(faktoren(i).Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)))
                ElseIf faktoren(i).StartsWith("*"c) Then
                    fak.Add(New Tuple(Of Boolean, Ausdruck)(False, auswerten(faktoren(i).Substring(1), konst_lokal, parameter, vars_Intern_lokal, params)))
                Else
                    fak.Add(New Tuple(Of Boolean, Ausdruck)(False, auswerten(faktoren(i), konst_lokal, parameter, vars_Intern_lokal, params)))
                End If
            Next
            Return New Ausdruck_Produkt(fak)
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenOperatorenAnführungszeichen(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        If str.StartsWith("""") AndAlso str.EndsWith("""") Then
            str = str.Substring(1, str.Length - 2)
            If str.IndexOf("""") <> -1 Then
                Return Nothing 'Es kommen mehrere Strings hier vor, also müssen noch andere Operatoren dabei sein...
            End If
            Return New AusdruckString_Konstante(str)
        End If
        Return Nothing
    End Function

    Private Shared Function auswertenFunktionen(str As String, konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Dim inAnführungszeichen As Boolean = False
        Dim klammerpos As Integer = 0
        Dim istFunktionsname As Boolean = True
        Dim Funktionsname As String = ""
        Dim intern As String = ""
        For i As Integer = 0 To str.Length - 1
            If str(i) = """" Then
                inAnführungszeichen = Not inAnführungszeichen
            End If
            If Not inAnführungszeichen Then
                If str(i) = "(" Then
                    If klammerpos >= 1 Then intern &= str(i)
                    klammerpos += 1
                ElseIf str(i) = ")" Then
                    klammerpos -= 1
                    If klammerpos >= 1 Then intern &= str(i)
                ElseIf klammerpos = 0 Then
                    If istFunktionsname Then
                        Funktionsname &= str(i)
                    Else
                        Return Nothing 'Fehler: Es kommen Buchstaben außerhalb der inneren Klammer und nach der ersten Klammer auf vor
                    End If
                Else
                    istFunktionsname = False
                    intern &= str(i)
                End If
            Else
                If klammerpos = 0 Then
                    Return Nothing
                Else
                    istFunktionsname = False
                    intern &= str(i)
                End If
            End If
        Next

        If klammerpos > 0 Then
            Throw New Exception("')' erwartet.") 'Die Klammern sind nicht ausgeglichen!
        End If
        If klammerpos < 0 Then
            Throw New Exception("Zu viele ')'.") 'Die Klammern sind nicht ausgeglichen!
        End If
        Dim ops As List(Of Ausdruck) = getSubTermeAusdruck(intern.Trim(), konst_lokal, parameter, vars_Intern_lokal, params)

        'Funktion: FunktionsName(Ops)
        Return auswertenFunktion(Funktionsname, ops, konst_lokal, parameter, vars_Intern_lokal, params)
    End Function

    Private Shared Function auswertenFunktion(name As String, operanden As List(Of Ausdruck), konst_lokal As Dictionary(Of String, Integer), parameter As List(Of ParamName), vars_Intern_lokal As List(Of VariableEinlesen), params As List(Of TemplateParameter)) As Ausdruck
        Select Case name
            Case "get"
                If operanden.Count = 1 AndAlso TypeOf operanden(0) Is AusdruckString_Konstante Then
                    Dim var As String = DirectCast(operanden(0), AusdruckString_Konstante).Ausrechnen(Nothing)
                    Dim varL As String = var.ToLower()
                    If parameter IsNot Nothing Then
                        For i As Integer = 0 To parameter.Count - 1
                            If parameter(i).name.ToLower() = varL Then
                                If parameter(i).art = ParamName.paramArt.Int Then
                                    Return New Ausdruck_Variable(i, False)
                                ElseIf parameter(i).art = ParamName.paramArt.Arrow Then
                                    Return New Ausdruck_Pfeil_Variable(i, False)
                                ElseIf parameter(i).art = ParamName.paramArt.ParamListe Then
                                    If params IsNot Nothing Then
                                        For j As Integer = 0 To params.Count - 1
                                            If TypeOf params(j) Is TemplateParameter_Param Then
                                                If DirectCast(params(j), TemplateParameter_Param).name.get_ID().ToLower() = varL Then
                                                    Return New AusdruckParam(j, i)
                                                End If
                                            End If
                                        Next
                                        Throw New Exception("Der Parameter '" & var & "' wurde nicht richtig definiert!")
                                    Else
                                        Throw New Exception("Der Parameter '" & var & "' wurde nicht richtig definiert!")
                                    End If
                                ElseIf parameter(i).art = ParamName.paramArt.Str Then
                                    Return New AusdruckString_Variable(i, False)
                                Else
                                    Throw New Exception("Falsche Parameter-Art!")
                                End If
                            End If
                        Next
                        Throw New Exception("Der Parameter """ & var & """ ist nicht definiert.")
                    Else
                        Throw New Exception("Der Parameter """ & var & """ ist nicht definiert.")
                    End If
                End If
            Case "substr"
                If operanden.Count = 3 Then
                    If TypeOf operanden(0) Is AusdruckString AndAlso TypeOf operanden(1) Is Ausdruck_Int AndAlso TypeOf operanden(2) Is Ausdruck_Int Then
                        Return New Ausdruck_Substring(DirectCast(operanden(0), AusdruckString), DirectCast(operanden(1), Ausdruck_Int), DirectCast(operanden(2), Ausdruck_Int))
                    Else
                        Throw New Exception("Falsche Parametertypen bei 'substr(""string"", start, length)'")
                    End If
                Else
                    Throw New Exception("Falsche Anzahl an Parametern bei 'substr(""string"", start, length)'")
                End If
            Case "strcat"
                If operanden.Count >= 2 Then
                    Dim opsNeu(operanden.Count - 1) As AusdruckString
                    For i As Integer = 0 To operanden.Count - 1
                        If TypeOf operanden(i) IsNot AusdruckString Then
                            Throw New Exception("Die Paremeter in der Funktion 'strcat(""string_1"", ..., ""string_n"")' müssen Strings sein!")
                        End If
                        opsNeu(i) = DirectCast(operanden(i), AusdruckString)
                    Next
                    Return New Ausdruck_StringCat(opsNeu)
                Else
                    Throw New Exception("Falsche Anzahl an Parametern in der Funktion 'strcat(""string_1"", ..., ""string_n"")'.")
                End If
            Case "length"
                If operanden.Count <> 1 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'length(""string"")'.")
                End If
                If TypeOf operanden(0) Is AusdruckString Then
                    Return New Ausdruck_StringLength(DirectCast(operanden(0), AusdruckString))
                Else
                    Throw New Exception("Der Parameter der Funktion 'length(""string"")' muss ein String sein.")
                End If
            Case "toint"
                If operanden.Count <> 1 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'toInt(ausdruck)'")
                End If
                Return New Ausdruck_ToInt(operanden(0))
            Case "tointhex"
                If operanden.Count <> 1 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'toIntHex(""string"")'")
                End If
                If TypeOf operanden(0) Is AusdruckString Then
                    Return New Ausdruck_ToIntHex(DirectCast(operanden(0), AusdruckString))
                Else
                    Throw New Exception("Der Parameter bei 'toIntHex(""string"")' muss ein String sein.")
                End If
            Case "tostr"
                If operanden.Count <> 1 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'toStr(ausdruck)'")
                End If
                Return New Ausdruck_ToString(operanden(0))
            Case "sqrt"
                If operanden.Count = 1 Then
                    If TypeOf operanden(0) Is Ausdruck_Int Then
                        Return New Ausdruck_SqrtAbs(DirectCast(operanden(0), Ausdruck_Int), Ausdruck_SqrtAbs.art.SQRT)
                    Else
                        Throw New Exception("Der Parameter der Funktion 'sqrt(x)' muss ein Integer sein.")
                    End If
                Else
                    Throw New Exception("Falsche Anzahl an Parametern in der Funktion 'sqrt(x)'.")
                End If
            Case "abs"
                If operanden.Count = 1 Then
                    If TypeOf operanden(0) Is Ausdruck_Int Then
                        Return New Ausdruck_SqrtAbs(DirectCast(operanden(0), Ausdruck_Int), Ausdruck_SqrtAbs.art.ABS)
                    Else
                        Throw New Exception("Der Parameter der Funktion 'abs(x)' muss ein Integer sein.")
                    End If
                Else
                    Throw New Exception("Falsche Anzahl an Parametern in der Funktion 'abs(x)'.")
                End If
            Case "max"
                If operanden.Count >= 1 Then
                    Return New Ausdruck_MaxMin(toListOfInts(operanden, "max(x1, x2, ..., xn)"), True)
                Else
                    Throw New Exception("Falsche Anzahl an Parametern in der Funktion 'max(x1, x2, ..., xn)'")
                End If
            Case "min"
                If operanden.Count >= 1 Then
                    Return New Ausdruck_MaxMin(toListOfInts(operanden, "min(x1, x2, ..., xn)"), False)
                Else
                    Throw New Exception("Falsche Anzahl an Parametern in der Funktion 'min(x1, x2, ..., xn)'")
                End If
            Case "sin"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'sin(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "sin(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Sin)
            Case "cos"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'cos(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "cos(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Cos)
            Case "tan"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'tan(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "tan(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Tan)
            Case "cot"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'cot(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "cot(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Cot)
            Case "csc"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'csc(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "csc(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Csc)
            Case "sec"
                If operanden.Count <> 2 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'sec(alpha, rad)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "sec(alpha, rad)")
                Return New Ausdruck_Trigonometry(sb_terme(0), sb_terme(1), Ausdruck_Trigonometry.Art.Sec)
            Case "findbezierx"
                If operanden.Count <> 9 Then
                    Throw New Exception("Falsche Anzahl der Parameter bei 'findBezierX(yToFind, x1, y1, x2, y2, x3, y3, x4, y4)'")
                End If
                Dim sb_terme As List(Of Ausdruck_Int) = toListOfInts(operanden, "findBezierX(yToFind, x1, y1, x2, y2, x3, y3, x4, y4)")
                Return New Ausdruck_findBezierX(sb_terme(0), sb_terme(1), sb_terme(2), sb_terme(3), sb_terme(4), sb_terme(5), sb_terme(6), sb_terme(7), sb_terme(8))
        End Select
        Return Nothing
    End Function

    Private Shared Function toListOfInts(operanden As List(Of Ausdruck), funktionsname As String) As List(Of Ausdruck_Int)
        Dim erg As New List(Of Ausdruck_Int)(operanden.Count)
        For i As Integer = 0 To operanden.Count - 1
            If TypeOf operanden(i) Is Ausdruck_Int Then
                erg.Add(DirectCast(operanden(i), Ausdruck_Int))
            Else
                Throw New Exception("Die Parameter der Funktion '" & funktionsname & "' müssen Integer sein.")
            End If
        Next
        Return erg
    End Function
#End Region
End Class
