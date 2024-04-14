Public Class Intelisense
    Private myPopup As PopUpWindow
    Private myListbox As ListBox
    Private myTxt As TextBox

    Private myStruktur As List(Of IntelisenseEntry)

    Public Like_Operator_Nutzen As Boolean = False
    Public SmallLarge_Egal As Boolean = True

    Public Sub New(txt As TextBox, struktur As List(Of IntelisenseEntry))
        myPopup = New PopUpWindow()
        myTxt = txt
        'myStruktur = New List(Of IntelisenseEntry)
        'myStruktur.Add(New IntelisenseParameter(New TemplateParameter_Int("test1", New Intervall(0, 1000, 1, True, True), 0)))
        'myStruktur.Add(New IntelisenseParameter(New TemplateParameter_String("str1", "", False, "")))
        'myStruktur.Add(New IntelisenseParameter(New TemplateParameter_String("str2", "", True, "abc")))
        'Dim n1 As New IntelisenseEntryNamespace("namespace")
        'n1.childs.Add(New IntelisenseParameter(New TemplateParameter_Param("str1", {"abc", "adf", "hallo", "test"})))
        'n1.childs.Add(New IntelisenseParameter(New TemplateParameter_Arrow("abc", New Intervall(0, 1000, 1, True, True), 0)))
        'myStruktur.Add(n1)
        myStruktur = struktur

        myListbox = New ListBox()
        myListbox.Font = txt.Font
        myListbox.BorderStyle = BorderStyle.None
        'myListbox.Size = New Size(200, 100)
        'Dim p As New Control()
        'p.Size = New Size(400, 400)
        myListbox.MaximumSize = New Size(1024, 200)
        myListbox.HorizontalScrollbar = False
        myPopup.addControl(myListbox)

        AddHandler txt.KeyPress, AddressOf txtKeyPress
        AddHandler txt.TextChanged, AddressOf txtTextChanged
        AddHandler txt.MouseUp, AddressOf txtMouseUp
        AddHandler txt.Leave, AddressOf txtLeave
        AddHandler txt.SizeChanged, AddressOf txtLeave
        AddHandler txt.LocationChanged, AddressOf txtLeave
        AddHandler txt.FindForm().LocationChanged, AddressOf txtLeave
        AddHandler txt.FindForm().Deactivate, AddressOf txtLeave
        AddHandler txt.KeyDown, AddressOf txtKeyDown
    End Sub

    Private lastSelectedLine As Integer
    Private hatKeyPress As Boolean = False

    Private Sub txtKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Left OrElse e.KeyCode = Keys.Right OrElse e.KeyCode = Keys.PageUp OrElse e.KeyCode = Keys.PageDown Then
            closePopup()
        End If
        If e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then
            If myPopup.isOpen Then
                If myListbox.Items.Count > 0 AndAlso myListbox.SelectedIndex >= 0 AndAlso myListbox.SelectedIndex < myListbox.Items.Count Then
                    If e.KeyCode = Keys.Up Then
                        myListbox.SelectedIndex = Math.Max(0, myListbox.SelectedIndex - 1)
                    Else
                        myListbox.SelectedIndex = Math.Min(myListbox.Items.Count - 1, myListbox.SelectedIndex + 1)
                    End If
                    e.Handled = True
                ElseIf myListbox.Items.Count > 0 AndAlso myListbox.SelectedIndex = -1 Then
                    If e.KeyCode = Keys.Up Then
                        myListbox.SelectedIndex = myListbox.Items.Count - 1
                    Else
                        myListbox.SelectedIndex = 0
                    End If
                    e.Handled = True
                End If
            End If
        End If
    End Sub

    Private Sub txtKeyPress(sender As Object, e As KeyPressEventArgs)
        hatKeyPress = True

        lastSelectedLine = myTxt.GetFirstCharIndexOfCurrentLine()
        If e.KeyChar = "."c OrElse e.KeyChar = "="c OrElse e.KeyChar = vbCrLf OrElse e.KeyChar = vbLf OrElse e.KeyChar = vbCr Then
            Dim hatChange As Boolean = False
            If myPopup.isOpen() Then
                If myListbox.Items.Count > 0 AndAlso myListbox.SelectedIndex >= 0 AndAlso myListbox.SelectedIndex < myListbox.Items.Count Then
                    Dim sel As String = myListbox.Items(myListbox.SelectedIndex).ToString()
                    Dim lineNr As Integer = myTxt.GetLineFromCharIndex(myTxt.SelectionStart)
                    If lineNr >= 0 AndAlso lineNr < myTxt.Lines.Count Then
                        Dim line As String = myTxt.Lines(lineNr)
                        Dim linePos As Integer = myTxt.SelectionStart - myTxt.GetFirstCharIndexOfCurrentLine()
                        changeLine(line, linePos, sel, lineNr, myTxt.GetFirstCharIndexOfCurrentLine(), e.KeyChar = vbCrLf OrElse e.KeyChar = vbLf OrElse e.KeyChar = vbCr)
                        hatChange = True
                    End If
                End If
            End If
            If Not hatChange AndAlso (e.KeyChar = vbCrLf OrElse e.KeyChar = vbLf OrElse e.KeyChar = vbCr) Then
                Dim lineNr As Integer = myTxt.GetLineFromCharIndex(myTxt.SelectionStart)
                If lineNr >= 0 AndAlso lineNr < myTxt.Lines.Count Then
                    Dim line As String = myTxt.Lines(lineNr)
                    Dim lineNeu As String = optimiereNachEnter(line)
                    Dim txtNeu As String = ""
                    Dim startposLine As Integer = myTxt.GetFirstCharIndexOfCurrentLine()
                    If myTxt.SelectionStart - startposLine = line.Length Then
                        For i As Integer = 0 To myTxt.Lines.Count - 1
                            If i <> 0 Then
                                txtNeu &= vbCrLf
                            End If
                            If i = lineNr Then
                                txtNeu &= lineNeu
                            Else
                                txtNeu &= myTxt.Lines(i)
                            End If
                        Next
                        myTxt.Text = txtNeu
                        myTxt.Select(lineNeu.Length + startposLine, 0)
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub changeLine(line As String, linePos As Integer, newWord As String, lineNr As Integer, lineIndexStart As Integer, mitEnter As Boolean)
        Dim start As Integer = 0
        Dim ende As Integer = line.Length - 1
        For i As Integer = Math.Min(linePos, line.Length - 1) To 0 Step -1
            If line(i) = "." OrElse line(i) = "=" Then
                start = i + 1
                Exit For
            End If
        Next
        For i As Integer = linePos + 1 To line.Length - 1
            If line(i) = "." OrElse line(i) = "=" Then
                ende = i
                Exit For
            End If
        Next
        Dim lineNeu As String = ""
        If start > 0 Then
            lineNeu = line.Substring(0, start)
        End If
        lineNeu &= newWord
        Dim posEnde As Integer = lineNeu.Length
        If ende < line.Length - 1 Then
            lineNeu &= line.Substring(ende)
        End If
        If mitEnter Then
            lineNeu = optimiereNachEnter(lineNeu)
            posEnde = lineNeu.Length
        End If
        Dim textGesamt As String = ""
        For i As Integer = 0 To myTxt.Lines.Count - 1
            If i <> 0 Then
                textGesamt &= vbCrLf
            End If
            If i = lineNr Then
                textGesamt &= lineNeu
            Else
                textGesamt &= myTxt.Lines(i)
            End If
        Next
        myTxt.Text = textGesamt
        myTxt.Select(lineIndexStart + posEnde, 0)
    End Sub

    Private Function optimiereNachEnter(line As String) As String
        For i As Integer = 0 To line.Count - 1
            If line(i) = "=" Then
                Return line.Substring(0, i).Trim() & " = " & line.Substring(i + 1).Trim()
            End If
        Next
        Return line
    End Function

    Private Sub txtTextChanged(sender As Object, e As EventArgs)
        If hatKeyPress Then
            If lastSelectedLine = myTxt.GetFirstCharIndexOfCurrentLine() Then
                Dim lineNr As Integer = myTxt.GetLineFromCharIndex(myTxt.SelectionStart)
                If lineNr >= 0 AndAlso lineNr < myTxt.Lines.Count Then
                    Dim line As String = myTxt.Lines(lineNr)
                    If line = "" Then
                        closePopup()
                    Else
                        openPopup()
                    End If
                End If
            Else
                closePopup()
            End If
        Else
            closePopup()
        End If
    End Sub

    Private Sub txtMouseUp(sender As Object, e As MouseEventArgs)
        closePopup()
    End Sub

    Private Sub txtLeave(sender As Object, e As EventArgs)
        closePopup()
    End Sub

    Private Sub openPopup()
        Dim lineNr As Integer = myTxt.GetLineFromCharIndex(myTxt.SelectionStart)
        If lineNr >= 0 AndAlso lineNr < myTxt.Lines.Count Then
            Dim line As String = myTxt.Lines(lineNr)
            Dim startIndexLine As Integer = myTxt.SelectionStart - myTxt.GetFirstCharIndexOfCurrentLine()
            If startIndexLine < 0 Then
                closePopup()
                Return
            End If
            If startIndexLine >= line.Length Then
                startIndexLine = line.Length - 1
            End If
            Dim hatStart As Boolean = False
            For i As Integer = startIndexLine To 0 Step -1
                If line(i) = "." OrElse line(i) = "=" Then
                    startIndexLine = i
                    hatStart = True
                    Exit For
                End If
            Next
            If Not hatStart Then
                startIndexLine = 0
            End If
            startIndexLine = Math.Min(startIndexLine, line.Length - 1)
            If startIndexLine = -1 Then
                closePopup()
                Return
            End If

            Dim startIndex As Integer = startIndexLine + myTxt.GetFirstCharIndexOfCurrentLine()
            Dim txtPos As Point = myTxt.GetPositionFromCharIndex(startIndex)
            Dim screenpos As Point = myTxt.PointToScreen(txtPos)
            screenpos.Y += CInt(1.5F * myTxt.Font.GetHeight())
            If myPopup.isOpen Then
                If myPopup.Location <> screenpos Then
                    myPopup.Location = screenpos
                End If
                If Not ChangePopUpContent(line, startIndexLine) Then
                    closePopup()
                End If
            Else
                If ChangePopUpContent(line, startIndexLine) Then
                    myPopup.Open(screenpos, False)
                End If
            End If
        End If
    End Sub

    Private Sub closePopup()
        If myPopup.isOpen() Then
            myPopup.Close(True)
        End If
    End Sub

    Private Function ChangePopUpContent(line As String, startindex As Integer) As Boolean
        Dim currentModifier As String = ""
        Dim activeListe As List(Of IntelisenseEntry)
        activeListe = New List(Of IntelisenseEntry)
        activeListe.Add(New IntelisenseEntryNamespace(""))
        DirectCast(activeListe(0), IntelisenseEntryNamespace).childs.AddRange(Me.myStruktur)

        Dim hatGleich As Boolean = False
        For i As Integer = 0 To startindex
            If Not hatGleich AndAlso (line(i) = "." OrElse line(i) = "=") Then
                If line(i) = "=" Then
                    hatGleich = True
                End If
                currentModifier = currentModifier.Trim()
                If SmallLarge_Egal Then
                    currentModifier = currentModifier.ToLower()
                End If
                Dim neueListe As New List(Of IntelisenseEntry)
                For Each entry As IntelisenseEntry In activeListe
                    If TypeOf entry Is IntelisenseEntryNamespace Then
                        With DirectCast(entry, IntelisenseEntryNamespace)
                            For z As Integer = 0 To .childs.Count - 1
                                If SmallLarge_Egal Then
                                    If (Like_Operator_Nutzen AndAlso .childs(z).name.get_str().ToLower() Like currentModifier) OrElse
                                       (Not Like_Operator_Nutzen AndAlso .childs(z).name.get_str().ToLower() = currentModifier) Then
                                        neueListe.Add(.childs(z))
                                    End If
                                Else
                                    If (Like_Operator_Nutzen AndAlso .childs(z).name.get_str() Like currentModifier) OrElse
                                       (Not Like_Operator_Nutzen AndAlso .childs(z).name.get_str() = currentModifier) Then
                                        neueListe.Add(.childs(z))
                                    End If
                                End If
                            Next
                        End With
                    End If
                Next
                activeListe = neueListe
                currentModifier = ""
            Else
                currentModifier &= line(i)
            End If
            If activeListe Is Nothing Then
                Exit For
            End If
        Next

        Dim selectedString As String = ""
        If line(startindex) = "." OrElse line(startindex) = "=" Then
            startindex += 1
        End If
        For i As Integer = startindex To line.Length - 1
            selectedString &= line(i)
        Next
        selectedString.Trim()
        If SmallLarge_Egal Then
            selectedString = selectedString.ToLower()
        End If

        If activeListe Is Nothing Then
            Return False
        End If

        Dim möglichkeiten As New List(Of String)
        For Each entry As IntelisenseEntry In activeListe
            If TypeOf entry Is IntelisenseEntryNamespace AndAlso Not hatGleich Then
                With DirectCast(entry, IntelisenseEntryNamespace)
                    For i As Integer = 0 To .childs.Count - 1
                        möglichkeiten.Add(.childs(i).name.get_str())
                    Next
                End With
            ElseIf TypeOf entry Is IntelisenseParameter AndAlso hatGleich Then
                With DirectCast(entry, IntelisenseParameter)
                    If TypeOf .param Is TemplateParameter_Param Then
                        For Each o In DirectCast(.param, TemplateParameter_Param).options
                            möglichkeiten.Add(o.get_str())
                        Next
                    End If
                End With
            End If
        Next
        If möglichkeiten.Count = 0 Then
            Return False
        End If

        Dim möglichkeitenSingle As New List(Of String)
        For i As Integer = 0 To möglichkeiten.Count - 1
            If Not möglichkeitenSingle.Contains(möglichkeiten(i)) Then
                möglichkeitenSingle.Add(möglichkeiten(i))
            End If
        Next
        möglichkeiten = möglichkeitenSingle

        möglichkeiten.Sort()
        Dim breite As Integer = 0
        Dim maxText As String = ""
        For i As Integer = 0 To möglichkeiten.Count - 1
            breite = Math.Max(breite, möglichkeiten(i).Length)
            maxText = möglichkeiten(i)
        Next
        breite = TextRenderer.MeasureText(maxText, myListbox.Font).Width
        myListbox.SuspendLayout()
        myListbox.Items.Clear()
        For i As Integer = 0 To möglichkeiten.Count - 1
            myListbox.Items.Add(möglichkeiten(i))
        Next
        myListbox.Width = breite
        Dim höhe As Integer = myListbox.ItemHeight * möglichkeiten.Count
        If höhe > 200 Then
            höhe = 200
        End If
        myListbox.ResumeLayout()
        myListbox.Height = höhe

        Dim selectedIndex As Integer = -1
        Dim maxL As Integer = 0
        For i As Integer = 0 To möglichkeiten.Count - 1
            Dim anzahl As Integer = gleicheAnzahl(möglichkeiten(i), selectedString)
            If anzahl > maxL Then
                maxL = anzahl
                selectedIndex = i
            End If
        Next
        If selectedIndex <> -1 AndAlso maxL = selectedString.Length Then
            myListbox.SelectedIndex = selectedIndex
        End If

        Return True
    End Function

    Private Function gleicheAnzahl(str1 As String, str2 As String) As Integer
        If SmallLarge_Egal Then
            str1 = str1.ToLower()
            str2 = str2.ToLower()
        End If
        Dim minL As Integer = Math.Min(str1.Length, str2.Length)
        For i As Integer = 0 To minL - 1
            If str1(i) <> str2(i) Then
                Return i
            End If
        Next
        Return minL
    End Function
End Class

Public MustInherit Class IntelisenseEntry
    Public name As Multi_Lang_String
End Class

Public Class IntelisenseEntryNamespace
    Inherits IntelisenseEntry

    Public childs As List(Of IntelisenseEntry)
    Public Sub New(name As String)
        childs = New List(Of IntelisenseEntry)
        Me.name = New Multi_Lang_String(name)
    End Sub
End Class

Public Class IntelisenseParameter
    Inherits IntelisenseEntry

    Public param As TemplateParameter
    Public Sub New(param As TemplateParameter)
        Me.param = param
        If TypeOf param Is TemplateParameter_Param Then
            Me.name = DirectCast(param, TemplateParameter_Param).name
        ElseIf TypeOf param Is TemplateParameter_Arrow Then
            Me.name = DirectCast(param, TemplateParameter_Arrow).name
        ElseIf TypeOf param Is TemplateParameter_Int Then
            Me.name = DirectCast(param, TemplateParameter_Int).name
        ElseIf TypeOf param Is TemplateParameter_String Then
            Me.name = DirectCast(param, TemplateParameter_String).name
        Else
            Throw New NotImplementedException("Diese Parameterart ist nicht bekannt!")
        End If
    End Sub
End Class
