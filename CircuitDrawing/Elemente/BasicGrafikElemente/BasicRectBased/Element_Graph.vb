Imports System.IO

Public Class Element_Graph
    Inherits Element_BasicRectBased

    Private stützstellen As Integer

    Private function_as_string As String
    Private ausgewerteteLinienZüge As List(Of PointF())

    Private xMin As Double
    Private xMax As Double
    Private yMin As Double
    Private yMax As Double


    Public Sub New(ID As ULong, pos As Point, breite As Integer, höhe As Integer, linestyle As Integer, fillstyle As Integer)
        MyBase.New(ID, pos, breite, höhe, linestyle, fillstyle)
        Me.xMin = -1
        Me.xMax = 1
        Me.yMin = -1
        Me.yMax = 1
        Me.stützstellen = 200
        Me.function_as_string = "x^2"
        Me.CalcFunktion()
    End Sub

    Protected Overrides Sub OnSizeChanged(r As OnSizeChangedReason)
        If r <> OnSizeChangedReason.Konstruktor Then
            CalcFunktion()
        End If
    End Sub

    Private Sub CalcFunktion()
        Dim x, y As Double
        Dim xG, yG As Double
        Dim aktuelleListe As New List(Of PointF)
        ausgewerteteLinienZüge = New List(Of PointF())

        Dim lastX, lastY As Double
        lastX = Double.NaN
        lastY = Double.NaN
        If yMin < yMax AndAlso xMin < xMax AndAlso stützstellen > 1 Then
            For i As Integer = 0 To stützstellen - 1
                xG = i / (stützstellen - 1) * MyBase.s.Width
                x = i / (stützstellen - 1) * (xMax - xMin) + xMin
                y = auswerten(x)
                If y >= yMin AndAlso y <= yMax Then
                    If aktuelleListe.Count = 0 AndAlso Not Double.IsNaN(lastY) AndAlso Not Double.IsInfinity(lastY) Then
                        Dim xAlt, yAlt As Double
                        yAlt = Double.NaN
                        If lastY >= yMax Then
                            xAlt = (yMax - lastY) / (y - lastY) * (x - lastX) + lastX
                            yAlt = yMax
                        ElseIf lastY <= yMin Then
                            xAlt = (yMin - lastY) / (y - lastY) * (x - lastX) + lastX
                            yAlt = yMin
                        End If
                        If Not Double.IsNaN(yAlt) Then
                            xAlt = (xAlt - xMin) / (xMax - xMin) * MyBase.s.Width
                            yAlt = MyBase.s.Height - (yAlt - yMin) / (yMax - yMin) * MyBase.s.Height
                            aktuelleListe.Add(New PointF(CSng(xAlt), CSng(yAlt)))
                        End If
                    End If
                    yG = MyBase.s.Height - (y - yMin) / (yMax - yMin) * MyBase.s.Height
                    aktuelleListe.Add(New PointF(CSng(xG), CSng(yG)))
                ElseIf Double.IsNaN(y) OrElse Double.IsInfinity(y) Then
                    If aktuelleListe.Count >= 2 Then
                        ausgewerteteLinienZüge.Add(aktuelleListe.ToArray())
                    End If
                    aktuelleListe = New List(Of PointF)
                Else
                    If aktuelleListe.Count >= 1 Then
                        Dim hatPunkt As Boolean = False
                        If y >= yMax Then
                            x = (yMax - lastY) / (y - lastY) * (x - lastX) + lastX
                            y = yMax
                            hatPunkt = True
                        ElseIf y <= yMin Then
                            x = (yMin - lastY) / (y - lastY) * (x - lastX) + lastX
                            y = yMin
                            hatPunkt = True
                        End If
                        If hatPunkt Then
                            xG = (x - xMin) / (xMax - xMin) * MyBase.s.Width
                            yG = MyBase.s.Height - (y - yMin) / (yMax - yMin) * MyBase.s.Height
                            aktuelleListe.Add(New PointF(CSng(xG), CSng(yG)))
                        End If
                        If aktuelleListe.Count >= 2 Then
                            ausgewerteteLinienZüge.Add(aktuelleListe.ToArray())
                        End If
                    End If
                    aktuelleListe = New List(Of PointF)
                End If
                lastX = x
                lastY = y
            Next
            If aktuelleListe.Count >= 2 Then
                ausgewerteteLinienZüge.Add(aktuelleListe.ToArray())
            End If
        End If
    End Sub

    Private Function auswerten(x As Double) As Double
        Return MatheAusrechnen.ausrechnen(function_as_string, x)
    End Function

    Public Overrides Function getGrafik() As DO_Grafik
        If ausgewerteteLinienZüge Is Nothing Then
            CalcFunktion()
        End If

        Dim g As New DO_MultiGrafik()
        For i As Integer = 0 To ausgewerteteLinienZüge.Count - 1
            Dim ln As New DO_MultiLinie_pointf(CType(ausgewerteteLinienZüge(i).Clone(), PointF()), False)
            g.childs.Add(ln)
        Next
        g.transform(New Transform_translate(Me.position))

        'Dim r As New DO_Rechteck(New Rectangle(position, s), False, False)

        g.setFillStyleRekursiv(Me.fillstyle)
        g.setLineStyleRekursiv(Me.linestyle)
        g.setLineScalingRekursiv(1.0F)
        Return g
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As New List(Of ElementEinstellung)
        Dim e1 As New Einstellung_Multi("Parameter", False)

        Dim funktion As New TemplateParameter_String(New Multi_Lang_String(My.Resources.Strings.Einstellung_Funktion, Nothing), Me.function_as_string, False, "")
        e1.add(New Einstellung_TemplateParameterString(funktion, Me.function_as_string))

        Dim stützstellen As New TemplateParameter_Int(New Multi_Lang_String(My.Resources.Strings.Einstellung_Stützstellen, Nothing), New Intervall(2, 10000, 1, True, True, Intervall.OutOfRangeMode.ClipToBounds), Me.stützstellen, "")
        e1.add(New Einstellung_TemplateParameter_Int(stützstellen, Me.stützstellen))

        Dim xmin As New TemplateParameter_Double(New Multi_Lang_String("Min X", Nothing), Me.xMin, "")
        e1.add(New Einstellung_TemplateParameter_Double(xmin, Me.xMin))

        Dim xmax As New TemplateParameter_Double(New Multi_Lang_String("Max X", Nothing), Me.xMax, "")
        e1.add(New Einstellung_TemplateParameter_Double(xmax, Me.xMax))

        Dim ymin As New TemplateParameter_Double(New Multi_Lang_String("Min Y", Nothing), Me.yMin, "")
        e1.add(New Einstellung_TemplateParameter_Double(ymin, Me.yMin))

        Dim ymax As New TemplateParameter_Double(New Multi_Lang_String("Max Y", Nothing), Me.yMax, "")
        e1.add(New Einstellung_TemplateParameter_Double(ymax, Me.yMax))

        l.Add(e1)
        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        Dim reCalc As Boolean = False

        For Each e As ElementEinstellung In einstellungen
            If TypeOf e Is Einstellung_Multi Then
                If e.Name.get_ID() = "Parameter" Then
                    For Each eSub As Einstellung_TemplateParam In DirectCast(e, Einstellung_Multi).getListe()
                        If TypeOf eSub Is Einstellung_TemplateParameterString Then
                            With DirectCast(eSub, Einstellung_TemplateParameterString)
                                If .txtChanged Then
                                    If .Name.get_ID() = My.Resources.Strings.Einstellung_Funktion Then
                                        If .myStr <> Me.function_as_string Then
                                            Me.function_as_string = .myStr
                                            changed = True
                                            reCalc = True
                                        End If
                                    End If
                                End If
                            End With
                        ElseIf TypeOf eSub Is Einstellung_TemplateParameter_Int Then
                            With DirectCast(eSub, Einstellung_TemplateParameter_Int)
                                If .nrChanged Then
                                    If .Name.get_ID() = My.Resources.Strings.Einstellung_Stützstellen Then
                                        If .myNr <> Me.stützstellen Then
                                            Me.stützstellen = .myNr
                                            changed = True
                                            reCalc = True
                                        End If
                                    End If
                                End If
                            End With
                        ElseIf TypeOf eSub Is Einstellung_TemplateParameter_Double Then
                            With DirectCast(eSub, Einstellung_TemplateParameter_Double)
                                If .nrChanged Then
                                    If .Name.get_ID() = "Min X" Then
                                        If .myNr <> Me.xMin AndAlso Not Double.IsNaN(.myNr) AndAlso Not Double.IsInfinity(.myNr) AndAlso .myNr < Me.xMax Then
                                            Me.xMin = .myNr
                                            changed = True
                                            reCalc = True
                                        End If
                                    ElseIf .Name.get_ID() = "Max X" Then
                                        If .myNr <> Me.xMax AndAlso Not Double.IsNaN(.myNr) AndAlso Not Double.IsInfinity(.myNr) AndAlso .myNr > Me.xMin Then
                                            Me.xMax = .myNr
                                            changed = True
                                            reCalc = True
                                        End If
                                    ElseIf .Name.get_ID() = "Min Y" Then
                                        If .myNr <> Me.yMin AndAlso Not Double.IsNaN(.myNr) AndAlso Not Double.IsInfinity(.myNr) AndAlso .myNr < Me.yMax Then
                                            Me.yMin = .myNr
                                            changed = True
                                            reCalc = True
                                        End If
                                    ElseIf .Name.get_ID() = "Max Y" Then
                                        If .myNr <> Me.yMax AndAlso Not Double.IsNaN(.myNr) AndAlso Not Double.IsInfinity(.myNr) AndAlso .myNr > Me.yMin Then
                                            Me.yMax = .myNr
                                            changed = True
                                            reCalc = True
                                        End If
                                    End If
                                End If
                            End With
                        End If
                    Next
                End If
            End If
        Next
        If reCalc Then
            Me.CalcFunktion()
        End If
        Return changed
    End Function

    Public Overrides Sub speichern(writer As BinaryWriter)
        writer.Write(isSelected)
        writer.Write(position.X)
        writer.Write(position.Y)
        writer.Write(s.Width)
        writer.Write(s.Height)
        writer.Write(linestyle)
        writer.Write(fillstyle)
        writer.Write(stützstellen)
        writer.Write(xMin)
        writer.Write(xMax)
        writer.Write(yMin)
        writer.Write(yMax)
        writer.Write(function_as_string)
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer) As Element_Graph
        Dim isSelected As Boolean = reader.ReadBoolean()
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim w As Integer = reader.ReadInt32()
        Dim h As Integer = reader.ReadInt32()
        Dim ls As Integer = reader.ReadInt32()
        Dim fs As Integer = reader.ReadInt32()

        Dim N As Integer = reader.ReadInt32()
        Dim xmin As Double = reader.ReadDouble()
        Dim xmax As Double = reader.ReadDouble()
        Dim ymin As Double = reader.ReadDouble()
        Dim ymax As Double = reader.ReadDouble()

        Dim str As String = reader.ReadString()

        Dim g As New Element_Graph(sender.getNewID(), New Point(posX, posY), w, h, ls, fs)
        g.stützstellen = N
        g.xMin = xmin
        g.xMax = xmax
        g.yMin = ymin
        g.yMax = ymax
        g.isSelected = isSelected
        g.function_as_string = str
        g.CalcFunktion()
        Return g
    End Function

    Public Overrides Function Clone() As ElementMaster
        Return Clone_intern(Me.ID)
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Return Clone_intern(get_newID())
    End Function

    Public Function Clone_intern(newID As ULong) As ElementMaster
        Dim r As New Element_Graph(newID, Me.position, Me.s.Width, Me.s.Height, Me.linestyle, Me.fillstyle)
        r.stützstellen = Me.stützstellen
        r.xMin = Me.xMin
        r.xMax = Me.xMax
        r.yMin = Me.yMin
        r.yMax = Me.yMax
        r.isSelected = Me.isSelected
        r.function_as_string = Me.function_as_string
        r.ausgewerteteLinienZüge = Nothing
        Return r
    End Function

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot Element_Graph Then Return False
        If Me.stützstellen <> DirectCast(e2, Element_Graph).stützstellen Then Return False
        If Me.xMax <> DirectCast(e2, Element_Graph).xMax Then Return False
        If Me.xMin <> DirectCast(e2, Element_Graph).xMin Then Return False
        If Me.yMax <> DirectCast(e2, Element_Graph).yMax Then Return False
        If Me.yMin <> DirectCast(e2, Element_Graph).yMin Then Return False
        If Me.function_as_string <> DirectCast(e2, Element_Graph).function_as_string Then Return False

        Return MyBase.isEqualExceptSelection(e2)
    End Function
End Class
