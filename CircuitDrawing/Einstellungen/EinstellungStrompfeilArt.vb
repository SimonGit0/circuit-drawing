Public Class EinstellungStrompfeilArt
    Inherits ElementEinstellung
    Private nichtAufUIEingabenReagieren As Boolean = False

    Public myArt As SnapableCurrentArrow.StromPfeilArt

    Private variousArt As Boolean = False
    Public changedArt As Boolean = False

    Private myCmb As JoSiCombobox

    Public Sub New(name As String, myart As SnapableCurrentArrow.StromPfeilArt)
        MyBase.New(New Multi_Lang_String(name, Nothing))
        Me.myArt = myart
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung)
        Dim e As EinstellungStrompfeilArt = DirectCast(e2, EinstellungStrompfeilArt)
        If e.myArt <> Me.myArt Then
            variousArt = True
        End If
    End Sub

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        nichtAufUIEingabenReagieren = True
        With DirectCast(e2, EinstellungStrompfeilArt)
            Me.myArt = .myArt
            Me.variousArt = .variousArt
            Me.changedArt = .changedArt
            If myCmb IsNot Nothing Then
                myCmb.Various = Me.variousArt
                If Me.myArt = SnapableCurrentArrow.StromPfeilArt.OnWire Then
                    myCmb.SelectedIndex = 0
                ElseIf myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire Then
                    myCmb.SelectedIndex = 1
                Else
                    Throw New Exception("Unbekannte Art: " + myArt.ToString())
                End If
            End If
        End With
        nichtAufUIEingabenReagieren = False
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim liste As New List(Of List(Of Control))(1)

        Dim lbl1 As Label = Nothing
        Dim cmb1 As New JoSiCombobox()
        Me.createLabelCombobox(lbl1, cmb1)
        lbl1.Text = My.Resources.Strings.Einstellung_StrompfeilArt
        cmb1.Items.Add(My.Resources.Strings.AufVerbinder)
        cmb1.Items.Add(My.Resources.Strings.NebenVerbinder)
        cmb1.Various = Me.variousArt
        If Me.myArt = SnapableCurrentArrow.StromPfeilArt.OnWire Then
            cmb1.SelectedIndex = 0
        ElseIf myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire Then
            cmb1.SelectedIndex = 1
        Else
            Throw New Exception("Unbekannte Art: " + myArt.ToString())
        End If
        myCmb = cmb1
        AddHandler cmb1.SelectedIndexChanged, AddressOf ArtChanged

        Dim zeile1 As New List(Of Control)(2)
        zeile1.Add(lbl1)
        zeile1.Add(cmb1)
        liste.Add(zeile1)
        Return Me.createGroupbox(liste)
    End Function

    Private Sub ArtChanged(sender As Object, e As EventArgs)
        If Not nichtAufUIEingabenReagieren Then
            Dim cmb As JoSiCombobox = DirectCast(sender, JoSiCombobox)
            If cmb.SelectedIndex = 0 Then
                changedArt = True
                cmb.Various = False
                myArt = SnapableCurrentArrow.StromPfeilArt.OnWire
                OnEinstellungLiveChanged()
            ElseIf cmb.SelectedIndex = 1 Then
                changedArt = True
                cmb.Various = False
                myArt = SnapableCurrentArrow.StromPfeilArt.NextToWire
                OnEinstellungLiveChanged()
            End If
        End If
    End Sub
End Class
