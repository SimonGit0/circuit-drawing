Public Class PfeilCombobox
    Inherits JoSiCombobox
    Private myLineStyles As LineStyleList

    Private Const faktor As Single = 0.1
    Private offsetIndex As Integer
    Public Sub New(can_select_no_Arrow As Boolean)
        If can_select_no_Arrow Then
            Me.Items.Add(-1) 'Kein Pfeil
            offsetIndex = 1
        Else
            offsetIndex = 0
        End If

        For i As Integer = 0 To Pfeil_Verwaltung.getVerwaltung().AnzahlPfeile - 1
            Me.Items.Add(i)
        Next
        Me.SelectedIndex = 0

        myLineStyles = New LineStyleList()
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 3.0F, New DashStyle(0)))

    End Sub

    Public Sub New(minArrow As Integer, maxArrow As Integer)
        Me.offsetIndex = -minArrow

        For i As Integer = minArrow To maxArrow
            Me.Items.Add(i)
        Next
        Me.SelectedIndex = 0

        myLineStyles = New LineStyleList()
        myLineStyles.add(New LineStyle(New Farbe(255, 0, 0, 0), Drawing2D.LineCap.Round, Drawing2D.LineCap.Round, Drawing2D.DashCap.Round, Drawing2D.LineJoin.Round, 3.0F, New DashStyle(0)))

    End Sub

    Protected Overrides Sub OnDrawItemDropDownForeground(e As DrawItemEventArgs)
        Dim lineColor As Color = e.ForeColor
        If (e.State And DrawItemState.Selected) <> 0 Then
            e.Graphics.FillRectangle(selectionColorBrush, e.Bounds)
            lineColor = foreColorSelected
        End If

        Dim args As New GrafikDrawArgs(myLineStyles, Nothing, Nothing, 1.0, False)
        args.offsetX = 0
        args.offsetY = 0
        args.faktorX = faktor
        args.faktorY = faktor

        Dim p1 As New Point(e.Bounds.X + 5, e.Bounds.Y + e.Bounds.Height \ 2)
        Dim p2 As New Point(e.Bounds.X + e.Bounds.Width - 5, e.Bounds.Y + e.Bounds.Height \ 2)

        p1.X = CInt(p1.X / args.faktorX)
        p1.Y = CInt(p1.Y / args.faktorY)
        p2.X = CInt(p2.X / args.faktorX)
        p2.Y = CInt(p2.Y / args.faktorY)

        Dim grafic As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, New ParamArrow(-1, 100), New ParamArrow(CShort(e.Index - offsetIndex), 100))
        myLineStyles.getLineStyle(0).farbe = New Farbe(lineColor.A, lineColor.R, lineColor.G, lineColor.B)

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        grafic.drawGraphics(e.Graphics, args)

    End Sub

    Protected Overrides Sub OnDrawItemSelectedForeground(e As DrawItemEventArgs, text As String)
        If _various Then
            MyBase.OnDrawItemSelectedForeground(e, ElementEinstellung.VARIOUS_STRING)
        Else
            Dim lineColor As Color
            If Me.Enabled Then
                If (e.State And DrawItemState.Focus) = 0 Then
                    lineColor = Me.ForeColor
                Else
                    lineColor = Color.Black
                End If
            Else
                lineColor = foreColor_EnabledFalse
            End If

            Dim args As New GrafikDrawArgs(myLineStyles, Nothing, Nothing, 1.0, False)
            args.offsetX = 0
            args.offsetY = 0
            args.faktorX = faktor
            args.faktorY = faktor

            Dim p1 As New Point(e.Bounds.X + 5, e.Bounds.Y + e.Bounds.Height \ 2)
            Dim p2 As New Point(e.Bounds.X + e.Bounds.Width - 5, e.Bounds.Y + e.Bounds.Height \ 2)

            p1.X = CInt(p1.X / args.faktorX)
            p1.Y = CInt(p1.Y / args.faktorY)
            p2.X = CInt(p2.X / args.faktorX)
            p2.Y = CInt(p2.Y / args.faktorY)

            Dim grafic As DO_Grafik = Pfeil_Verwaltung.getVerwaltung().getLineWithPfeil(p1, p2, New ParamArrow(-1, 100), New ParamArrow(CShort(e.Index - offsetIndex), 100))
            myLineStyles.getLineStyle(0).farbe = New Farbe(lineColor.A, lineColor.R, lineColor.G, lineColor.B)

            e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            grafic.drawGraphics(e.Graphics, args)
        End If
    End Sub
End Class
