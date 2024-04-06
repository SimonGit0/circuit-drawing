Public Class FRM_Einlesefehler
    Private spalte1, spalte2, spalte3 As Panel
    Private panelAll As Panel

    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.Close()
    End Sub

    Public Sub setErrors(fehler As List(Of CompileException))
        Dim header1 As New Label
        Dim header2 As New Label
        Dim header3 As New Label
        header1.Text = ""
        header2.Text = "Beschreibung"
        header3.Text = "Aufrufliste"
        header1.AutoSize = True
        header2.AutoSize = True
        header3.AutoSize = True

        Dim myfont As Font = New Font(header1.Font.FontFamily, 10.0F)

        header1.Font = myfont
        header2.Font = myfont
        header3.Font = myfont

        panelAll = New Panel()

        spalte1 = New Panel
        spalte2 = New Panel
        spalte3 = New Panel

        header1.Location = New Point(0, 0)
        header2.Location = New Point(0, 0)
        header3.Location = New Point(0, 0)
        spalte1.Controls.Add(header1)
        spalte2.Controls.Add(header2)
        spalte3.Controls.Add(header3)

        Panel1.Controls.Add(panelAll)

        spalte1.Location = New Point(0, 0)
        spalte2.Location = New Point(0, 0)
        spalte3.Location = New Point(0, 0)

        panelAll.Controls.Add(spalte1)
        panelAll.Controls.Add(spalte2)
        panelAll.Controls.Add(spalte3)

        Dim y As Integer = header2.Height + 10

        For i As Integer = 0 To fehler.Count - 1
            Dim lbl1 As New Label
            lbl1.AutoSize = True
            lbl1.Text = fehler(i).art
            Dim lbl2 As New Label
            lbl2.AutoSize = True
            lbl2.Text = fehler(i).fehler
            Dim lbl3 As New Label
            lbl3.AutoSize = True
            lbl3.Text = fehler(i).fehlerOrt

            lbl1.Location = New Point(0, y)
            lbl2.Location = New Point(0, y)
            lbl3.Location = New Point(0, y)
            lbl1.Font = myfont
            lbl2.Font = myfont
            lbl3.Font = myfont

            spalte1.Controls.Add(lbl1)
            spalte2.Controls.Add(lbl2)
            spalte3.Controls.Add(lbl3)

            Dim picSep As New PictureBox()
            picSep.BackColor = SystemColors.ControlDark
            picSep.Size = New Size(panelAll.Width, 1)
            picSep.Location = New Point(0, y - 5)
            picSep.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            panelAll.Controls.Add(picSep)
            picSep.BringToFront()

            'y += 20 * Math.Max(zeilen(fehler(i).fehlerOrt), zeilen(fehler(i).fehler))
            y += Math.Max(lbl1.Height, Math.Max(lbl2.Height, lbl3.Height)) + 10
        Next

        panelAll.AutoSizeMode = AutoSizeMode.GrowAndShrink
        panelAll.AutoSize = True

        spalte1.AutoSizeMode = AutoSizeMode.GrowAndShrink
        spalte2.AutoSizeMode = AutoSizeMode.GrowAndShrink
        spalte3.AutoSizeMode = AutoSizeMode.GrowAndShrink

        spalte1.AutoSize = True
        spalte2.AutoSize = True
        spalte3.AutoSize = True


    End Sub

    Private Function zeilen(str As String) As Integer
        Dim erg As Integer = 1
        For i As Integer = 0 To str.Length - 1
            If str(i) = vbCrLf OrElse str(i) = vbCr Then
                erg += 1
            End If
        Next
        Return erg
    End Function

    Private Sub FRM_Einlesefehler_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        spalte1.Location = New Point(0, 0)
        spalte2.Location = New Point(spalte1.Location.X + spalte1.Width + 10, 0)
        spalte3.Location = New Point(spalte2.Location.X + spalte2.Width + 10, 0)

        Dim picSep1 As New PictureBox
        picSep1.BackColor = SystemColors.ControlDark
        picSep1.Location = New Point(spalte2.Location.X - 5, 0)
        picSep1.Size = New Size(1, Math.Max(spalte2.Height, Math.Max(spalte1.Height, spalte3.Height)))
        panelAll.Controls.Add(picSep1)

        Dim picSep2 As New PictureBox
        picSep2.BackColor = SystemColors.ControlDark
        picSep2.Location = New Point(spalte3.Location.X - 5, 0)
        picSep2.Size = New Size(1, Math.Max(spalte2.Height, Math.Max(spalte1.Height, spalte3.Height)))
        panelAll.Controls.Add(picSep2)
    End Sub
End Class