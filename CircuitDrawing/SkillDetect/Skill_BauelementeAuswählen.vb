Imports CircuitDrawing.Detektion
Public Class Skill_BauelementeAuswählen
    Private myElemente As List(Of MasterElemente)

    Public NeueZuordnungen As New List(Of Skill_BauteilZuordnung)

    Private ctrs As List(Of Ctr_BauelementeAuswahl_Skill)
    Private Speicherpfad As String

    Public Sub New()
        InitializeComponent()

        Me.Icon = My.Resources.iconAlle
    End Sub

    Public Sub init(bib As Bibliothek, elemente As List(Of MasterElemente), zuOrdnungenInitial As List(Of Skill_BauteilZuordnung), Speicherpfad As String)
        Me.myElemente = elemente
        ctrs = New List(Of Ctr_BauelementeAuswahl_Skill)
        Me.Speicherpfad = Speicherpfad

        Dim posY As Integer = 3
        For i As Integer = 0 To myElemente.Count - 1

            Dim ctr As New Ctr_BauelementeAuswahl_Skill()
            ctr.Width = Panel1.Width - 6
            ctr.Location = New Point(3, posY)
            posY = ctr.Location.Y + ctr.Height + 3
            ctr.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
            If zuOrdnungenInitial IsNot Nothing Then
                ctr.init(bib, elemente(i), zuOrdnungenInitial(i))
            Else
                ctr.init(bib, elemente(i), Nothing)
            End If

            ctrs.Add(ctr)
            Me.Panel1.Controls.Add(ctr)
        Next

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Abort
        Me.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        NeueZuordnungen.Clear()

        For i As Integer = 0 To ctrs.Count - 1
            NeueZuordnungen.Add(ctrs(i).getZuordnung(Speicherpfad))
        Next

        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        For i As Integer = 0 To ctrs.Count - 1
            ctrs(i).ckb_speichern.Checked = True
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        For i As Integer = 0 To ctrs.Count - 1
            ctrs(i).ckb_speichern.Checked = False
        Next
    End Sub

    Public Shared Function getBewertungVonDrehung(drehung As Drehmatrix) As Integer
        Select Case drehung.getDrehung()
            Case Drehmatrix.Drehungen.Normal
                Return 8
            Case Drehmatrix.Drehungen.MirrorX
                Return 7
            Case Drehmatrix.Drehungen.Rot90
                Return 6
            Case Drehmatrix.Drehungen.MirrorXRot270
                Return 5
            Case Drehmatrix.Drehungen.MirrorXRot180
                Return 4
            Case Drehmatrix.Drehungen.Rot180
                Return 3
            Case Drehmatrix.Drehungen.Rot270
                Return 2
            Case Drehmatrix.Drehungen.MirrorXRot90
                Return 1
        End Select
        Return 0
    End Function
End Class