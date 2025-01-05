Public Class Einstellung_Multi
    Inherits ElementEinstellung

    Private liste As List(Of Einstellung_TemplateParam)
    Private hatSchließenButton As Boolean
    Public istGeschlossen As Boolean = False

    Public Sub New(name As String, hatSchließenButton As Boolean)
        MyBase.New(SortierTyp.ElementEinstellung, New Multi_Lang_String(name, Nothing))
        Me.hatSchließenButton = hatSchließenButton
        If hatSchließenButton Then
            MyBase.sortID = SortierTyp.ElementEinstellungSubelement
        End If
        liste = New List(Of Einstellung_TemplateParam)
    End Sub

    Public Function getListe() As List(Of Einstellung_TemplateParam)
        Return liste
    End Function

    Public Sub add(e As Einstellung_TemplateParam)
        Me.liste.Add(e)
        AddHandler e.EinstellungLiveChanged, AddressOf ChildEinstellungChanged
    End Sub

    Public Overrides Sub CombineValues(e2 As ElementEinstellung, mode As combineModus)
        'For i As Integer = 0 To liste.Count - 1
        '    If liste(i).Name = e2.Name Then
        '        liste(i).CombineValues(e2)
        '    End If
        'Next
        Dim e As Einstellung_Multi = DirectCast(e2, Einstellung_Multi)
        Dim hatEinstellung As Boolean

        Dim hatEinstellung_e2() As Boolean = Nothing
        If mode = combineModus.AlleEinstellungenAnzeigen Then
            ReDim hatEinstellung_e2(e.liste.Count - 1)
        End If

        For i As Integer = liste.Count - 1 To 0 Step -1
            hatEinstellung = False
            For j As Integer = 0 To e.liste.Count - 1
                If liste(i).isSameParameter(e.liste(j)) Then
                    hatEinstellung = True
                    liste(i).CombineValues(e.liste(j), mode)
                    If mode = combineModus.AlleEinstellungenAnzeigen Then
                        hatEinstellung_e2(j) = True
                    End If
                    Exit For
                End If
            Next
            If Not hatEinstellung AndAlso mode <> combineModus.AlleEinstellungenAnzeigen Then
                RemoveHandler liste(i).EinstellungLiveChanged, AddressOf ChildEinstellungChanged
                liste.RemoveAt(i)
            End If
        Next
        If mode = combineModus.AlleEinstellungenAnzeigen Then
            For i As Integer = 0 To e.liste.Count - 1
                If Not hatEinstellung_e2(i) Then
                    RemoveHandler e.liste(i).EinstellungLiveChanged, AddressOf e.ChildEinstellungChanged
                    Me.add(e.liste(i))
                End If
            Next
        End If
    End Sub

    Public Function sindGleicheEinstellungen(e2 As ElementEinstellung) As Boolean
        If TypeOf e2 IsNot Einstellung_Multi Then
            Return False
        End If
        With DirectCast(e2, Einstellung_Multi)
            If .liste.Count <> Me.liste.Count Then
                Return False
            End If
            For i As Integer = 0 To .liste.Count - 1
                If Not .liste(i).isSameParameter(Me.liste(i)) Then
                    Return False
                End If
            Next
        End With
        Return True
    End Function

    Public Overrides Sub aktualisiere(e2 As ElementEinstellung)
        With DirectCast(e2, Einstellung_Multi)
            For i As Integer = 0 To liste.Count - 1
                liste(i).aktualisiere(.liste(i))
            Next
        End With
    End Sub

    Public Overrides Function getGroupbox() As GroupBox
        Dim erg As New List(Of List(Of Control))

        For i As Integer = 0 To liste.Count - 1
            erg.AddRange(liste(i).getControlListe())
        Next

        Dim box As GroupBox = createGroupbox(erg)
        If hatSchließenButton Then
            Dim btnCross As New Button_X()
            btnCross.Location = New Point(box.Width - btnCross.Width, 0)
            btnCross.Anchor = AnchorStyles.Right Or AnchorStyles.Top
            box.Controls.Add(btnCross)
            AddHandler btnCross.Click, AddressOf SchließenClick
        End If
        Return box
    End Function

    Private Sub ChildEinstellungChanged(sender As Object, e As EventArgs)
        OnEinstellungLiveChanged()
    End Sub

    Private Sub SchließenClick(sender As Object, e As EventArgs)
        Me.istGeschlossen = True
        OnEinstellungLiveChanged()
    End Sub

End Class