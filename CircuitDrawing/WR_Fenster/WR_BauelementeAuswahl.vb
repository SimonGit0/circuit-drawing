Imports System.IO
Public Class WR_BauelementeAuswahl

    Private myBib As Bibliothek
    Private myLinestylelist As LineStyleList
    Private myFillstylelist As FillStyleList
    Private myFontList As FontList

    Private keinComboboxSelectedIndexChanged As Boolean = False
    Private keinListbox1SelectedIndexChanged As Boolean = False

    Private myBitmapTemp As Bitmap 'used to temporarily draw the element in order to calculate the size of text blocks!!

    Private selectCellNameBeiNächstenSelectindexChanged As String = ""

    Public Sub New()
        InitializeComponent()
        Me.myBitmapTemp = New Bitmap(1, 1)

        myLinestylelist = New LineStyleList()
        Dim ls As New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 0.2, New DashStyle(0))
        ls.alwaysUsePenWidthOfOne = True
        myLinestylelist.add(ls)

        myFillstylelist = New FillStyleList()
        Dim fs As New FillStyle(New Farbe(0, 0, 0, 0))
        myFillstylelist.add(fs)

        myFontList = New FontList()
        myFontList.add(New FontStyle(New Farbe(255, 0, 0, 0), "Times", 10.0, False, False))
    End Sub

    Public Sub init(bib As Bibliothek)
        Me.myBib = bib
        For Each _namespace As KeyValuePair(Of String, BauteileNamespace) In bib
            ComboBox1.Items.Add(_namespace.Key)
        Next

        If bib.getNamespaceCount() > 0 Then
            ComboBox1.SelectedIndex = 0
        End If

        UserDrawListbox1.SelectionMode = UserDrawListbox.SelectionModeEnum.ImmerEinItemMussSelectedSein


    End Sub

    Public Sub selectBauteil(_namespace As String, _name As String)
        If myBib.hasBauteil(_namespace, _name, "eu") Then
            Dim fehler As Boolean = True
            For i As Integer = 0 To ComboBox1.Items.Count - 1
                If ComboBox1.Items(i).ToString() = _namespace Then
                    ComboBox1.SelectedIndex = i
                    fehler = False
                    Exit For
                End If
            Next
            If Not fehler Then
                For i As Integer = 0 To UserDrawListbox1.Count - 1
                    If DirectCast(UserDrawListbox1.Items(i), ListboxItemImageAndText).Text = _name Then
                        UserDrawListbox1.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If
        End If
    End Sub

    Public Sub refresh_Liste(bib As Bibliothek)
        Me.myBib = bib

        Dim selectedIndex As Integer = ComboBox1.SelectedIndex
        Dim selectedString As String = ""
        selectCellNameBeiNächstenSelectindexChanged = ""
        If selectedIndex >= 0 AndAlso selectedIndex < ComboBox1.Items.Count Then
            selectedString = ComboBox1.Items(selectedIndex).ToString()

            If UserDrawListbox1.SelectedIndex >= 0 AndAlso UserDrawListbox1.SelectedIndex < UserDrawListbox1.Count Then
                selectCellNameBeiNächstenSelectindexChanged = DirectCast(UserDrawListbox1.Items(UserDrawListbox1.SelectedIndex), ListboxItemImageAndText).Text
            End If
        End If

        ComboBox1.SuspendLayout()
        keinComboboxSelectedIndexChanged = True
        ComboBox1.Items.Clear()
        Dim selectNachher As Integer = 0
        For Each _namespace As KeyValuePair(Of String, BauteileNamespace) In myBib
            ComboBox1.Items.Add(_namespace.Key)
            If _namespace.Key = selectedString Then
                selectNachher = ComboBox1.Items.Count - 1
            End If
        Next
        ComboBox1.SelectedIndex = selectNachher
        ComboBox1.ResumeLayout()
        keinComboboxSelectedIndexChanged = False

        ComboBox1_SelectedIndexChanged(ComboBox1, EventArgs.Empty)
        selectCellNameBeiNächstenSelectindexChanged = ""
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If keinComboboxSelectedIndexChanged Then
            Return
        End If

        keinListbox1SelectedIndexChanged = True
        If ComboBox1.Items.Count <= 0 Then Exit Sub

        Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))
        If _namespace.getCellCount() > 0 Then
            Dim cells() As String = _namespace.getCellsNamen()


            UserDrawListbox1.SuspendLayout()
            UserDrawListbox1.Clear()

            Dim selectNachher As Integer = 0
            For i As Integer = 0 To cells.Length - 1
                'UserDrawListbox1.addItem(New ListboxItemText(cells(i)))
                Dim bild As Bitmap = getBitmap(_namespace.getCell(cells(i)))

                If selectCellNameBeiNächstenSelectindexChanged <> "" AndAlso cells(i) = selectCellNameBeiNächstenSelectindexChanged Then
                    selectNachher = i
                End If

                Dim item As New ListboxItemImageAndText(bild.Height + 6)
                item.Image = bild
                item.Text = cells(i)
                UserDrawListbox1.addItem(item)
            Next

            UserDrawListbox1.ResumeLayout()

            UserDrawListbox1.SelectedIndex = selectNachher
            selectCellNameBeiNächstenSelectindexChanged = ""
        Else
            UserDrawListbox1.Clear()
        End If
        keinListbox1SelectedIndexChanged = False
        ListBox1_SelectedIndexChanged(UserDrawListbox1, EventArgs.Empty)
    End Sub

    Private Function getBitmap(cell As BauteilCell) As Bitmap
        Dim breite As Integer = 64
        Dim höhe As Integer = 64
        Dim rand As Integer = 4

        Dim view As BauteilView = cell.getFirst()
        Dim template As TemplateAusDatei = view.template
        Dim template_compiled As Template_Compiled = Nothing
        template.recompile(Nothing, 0, template_compiled, Nothing)
        Dim grafik As DO_Grafik = template_compiled.getGrafik()
        Dim faktor As Single
        Dim offsetX As Single
        Dim offsetY As Single

        Dim bild As Bitmap = Nothing
        For i As Integer = 0 To 1
            Dim bounds As Rectangle = grafik.getBoundingBox()
            If bounds.Width = 0 Then
                bounds.Width = 200
            End If
            If bounds.Height = 0 Then
                bounds.Height = 200
            End If
            faktor = CSng(Math.Min((breite - rand) / bounds.Width, (höhe - rand) / bounds.Height))
            offsetX = CSng(breite / 2 - (bounds.X + bounds.Width / 2) * faktor)
            offsetY = CSng(höhe / 2 - (bounds.Y + bounds.Height / 2) * faktor)

            If i = 0 Then
                bild = Me.myBitmapTemp
            Else
                bild = New Bitmap(breite, höhe)
            End If

            Using g As Graphics = Graphics.FromImage(bild)
                g.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                g.Clear(Color.Transparent)
                g.CompositingMode = Drawing2D.CompositingMode.SourceOver
                g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAliasGridFit
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

                Dim args As New GrafikDrawArgs(myLinestylelist, myFillstylelist, myFontList, faktor / Vektor_Picturebox.DEFAULT_MM_PER_INT, False)
                args.faktorX = faktor
                args.faktorY = faktor
                args.offsetX = offsetX
                args.offsetY = offsetY
                grafik.drawGraphics(g, args)
            End Using
        Next
        Return bild
    End Function

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles UserDrawListbox1.SelectedIndexChanged
        If Not keinListbox1SelectedIndexChanged Then
            If ComboBox1.SelectedIndex >= 0 AndAlso ComboBox1.Items.Count > 0 AndAlso UserDrawListbox1.Count > 0 AndAlso UserDrawListbox1.SelectedIndex >= 0 Then
                Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))

                'Dim name As String = DirectCast(UserDrawListbox1.Items(UserDrawListbox1.SelectedIndex), ListboxItemText).Text
                Dim name As String = DirectCast(UserDrawListbox1.Items(UserDrawListbox1.SelectedIndex), ListboxItemImageAndText).Text

                Dim _cell As BauteilCell = _namespace.getCell(name)

                RaiseEvent OnTemplateChanged(Me, _cell)
            Else
                RaiseEvent OnTemplateChanged(Me, Nothing)
            End If
        End If
    End Sub

    Public Event OnTemplateChanged(sender As Object, cell As BauteilCell)

    Private Sub UserDrawListbox1_MouseUp(sender As Object, e As MouseEventArgs) Handles UserDrawListbox1.MouseUp
        If e.Button = MouseButtons.Right Then
            If ComboBox1.SelectedIndex >= 0 AndAlso ComboBox1.Items.Count > 0 AndAlso UserDrawListbox1.Count > 0 AndAlso UserDrawListbox1.SelectedIndex >= 0 Then
                Dim _namespace As BauteileNamespace = myBib.getNamespace(CStr(ComboBox1.SelectedItem))
                If _namespace.Name = Bibliothek.NAMESPACE_LOKAL Then
                    'darf exportieren
                    Dim name As String = DirectCast(UserDrawListbox1.Items(UserDrawListbox1.SelectedIndex), ListboxItemImageAndText).Text
                    Dim _cell As BauteilCell = _namespace.getCell(name)
                    ExportiereSymbolToolStripMenuItem.Tag = _cell
                    ContextMenuStrip1.Show(UserDrawListbox1, e.Location)
                End If
            End If
        End If
    End Sub

    Private Sub ExportiereSymbolToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExportiereSymbolToolStripMenuItem.Click
        If ExportiereSymbolToolStripMenuItem.Tag IsNot Nothing AndAlso TypeOf ExportiereSymbolToolStripMenuItem.Tag Is BauteilCell Then
            Dim cell As BauteilCell = DirectCast(ExportiereSymbolToolStripMenuItem.Tag, BauteilCell)
            Dim s As New SaveFileDialog()
            s.Filter = "Symbol (*.sym)|*.sym"
            If s.ShowDialog = DialogResult.OK Then
                Dim outputStream As FileStream = Nothing
                Try
                    outputStream = New FileStream(s.FileName, FileMode.Create, FileAccess.Write)
                    Dim writer As New Export_StreamWriter(outputStream, System.Text.Encoding.UTF8)
                    cell.getFirst().template.export(writer, "Meine Bauteile")
                    writer.Flush()
                Catch ex As Exception
                    MessageBox.Show("Export des Symbols fehlgeschlagen: " + ex.Message)
                Finally
                    If outputStream IsNot Nothing Then
                        outputStream.Close()
                    End If
                End Try
            End If
        End If
    End Sub

    Private Sub LöschenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LöschenToolStripMenuItem.Click
        If ExportiereSymbolToolStripMenuItem.Tag IsNot Nothing AndAlso TypeOf ExportiereSymbolToolStripMenuItem.Tag Is BauteilCell Then
            Dim cell As BauteilCell = DirectCast(ExportiereSymbolToolStripMenuItem.Tag, BauteilCell)
            RaiseEvent DeleteLokalSymbol(Me, cell)
        End If
    End Sub
    Public Event DeleteLokalSymbol(sender As Object, cell As BauteilCell)
End Class

Public Class Combobox_OhneKey
    Inherits ComboBox

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        If e.KeyCode = Keys.Left OrElse e.KeyCode = Keys.Right OrElse e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then
            MyBase.OnKeyDown(e)
        Else
            e.Handled = True
        End If
    End Sub

    Protected Overrides Sub OnKeyPress(e As KeyPressEventArgs)
        'MyBase.OnKeyPress(e)
        e.Handled = True
    End Sub

End Class
