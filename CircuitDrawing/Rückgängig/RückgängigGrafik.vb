Public Class RückgängigGrafik
    Inherits Rückgängig

    Private _Speicher_Auch_Compiled_Grafik As Boolean

    Private vor() As ElementMaster
    Private nach() As ElementMaster
    Private notwenig_Normal As Boolean = True

    Public Sub New()
        Me.New(False)
    End Sub

    Public Sub New(_Speicher_Auch_Compiled_Grafik As Boolean)
        MyBase.New("Grafik geändert")
        Me._Speicher_Auch_Compiled_Grafik = _Speicher_Auch_Compiled_Grafik
    End Sub

    Public Sub speicherVorherZustand(args As RückgängigArgs)
        speicherZustand(vor, args)
    End Sub

    Public Sub speicherNachherZustand(args As RückgängigArgs)
        speicherZustand(nach, args)

        If isNotwendig_E() Then
            notwenig_Normal = True
        Else
            notwenig_Normal = False
            vor = Nothing
            nach = Nothing
        End If
    End Sub

    Private Sub speicherZustand(ByRef speicher() As ElementMaster, args As RückgängigArgs)
        ReDim speicher(args.ElementeListe.Count - 1)
        For i As Integer = 0 To speicher.Length - 1
            speicher(i) = args.ElementeListe(i).Clone()
            If _Speicher_Auch_Compiled_Grafik AndAlso TypeOf args.ElementeListe(i) Is BauteilAusDatei Then
                DirectCast(args.ElementeListe(i), BauteilAusDatei).FlatCopyCompiledTemplate(DirectCast(speicher(i), BauteilAusDatei))
            End If
        Next
    End Sub

    Private Sub ladeZustand(ByRef vonSpeicher() As ElementMaster, toArgs As RückgängigArgs)
        Dim neuerVonSpeicher(vonSpeicher.Length - 1) As ElementMaster
        For i As Integer = 0 To vonSpeicher.Length - 1
            If vonSpeicher(i) Is Nothing Then
                neuerVonSpeicher(i) = toArgs.ElementeListe(i)
            Else
                neuerVonSpeicher(i) = vonSpeicher(i).Clone()
                If _Speicher_Auch_Compiled_Grafik AndAlso TypeOf vonSpeicher(i) Is BauteilAusDatei Then
                    DirectCast(vonSpeicher(i), BauteilAusDatei).FlatCopyCompiledTemplate(DirectCast(neuerVonSpeicher(i), BauteilAusDatei))
                End If
                neuerVonSpeicher(i).deselect()
                For Each e In toArgs.ElementeListe
                    If e.ID = vonSpeicher(i).ID Then
                        If TypeOf e Is Element Then
                            DirectCast(neuerVonSpeicher(i), Element).isSelected = DirectCast(e, Element).isSelected
                        ElseIf TypeOf e Is SnapableElement Then
                            If DirectCast(e, SnapableElement).getNrOfSnappoints() = DirectCast(neuerVonSpeicher(i), SnapableElement).getNrOfSnappoints() Then
                                For k As Integer = 0 To DirectCast(e, SnapableElement).getNrOfSnappoints() - 1
                                    DirectCast(neuerVonSpeicher(i), SnapableElement).getSnappoint(k).isSelected = DirectCast(e, SnapableElement).getSnappoint(k).isSelected
                                Next
                            End If
                        End If
                        Exit For
                    End If
                Next
            End If
        Next

        toArgs.ElementeListe.Clear()
        For i As Integer = 0 To vonSpeicher.Length - 1
            toArgs.ElementeListe.Add(neuerVonSpeicher(i))
        Next
        neuerVonSpeicher = Nothing
    End Sub

    Private Sub ladeZustandMitSelection(ByRef vonSpeicher() As ElementMaster, toArgs As RückgängigArgs)
        toArgs.ElementeListe.Clear()
        For i As Integer = 0 To vonSpeicher.Length - 1
            If vonSpeicher(i) Is Nothing Then
                Throw New NotImplementedException()
            Else
                toArgs.ElementeListe.Add(vonSpeicher(i).Clone())
                If _Speicher_Auch_Compiled_Grafik AndAlso TypeOf vonSpeicher(i) Is BauteilAusDatei Then
                    DirectCast(vonSpeicher(i), BauteilAusDatei).FlatCopyCompiledTemplate(DirectCast(toArgs.ElementeListe(toArgs.ElementeListe.Count - 1), BauteilAusDatei))
                End If
            End If
        Next
    End Sub

    Public Sub unselectAllElementsVorher()
        For i As Integer = 0 To vor.Length - 1
            vor(i).deselect()
        Next
    End Sub

    Public Function istNotwendig() As Boolean
        Return notwenig_Normal
    End Function

    Private Function isNotwendig_E() As Boolean
        If vor.Length <> nach.Length Then Return True
        Dim notwendig As Boolean = False
        For i As Integer = 0 To vor.Length - 1
            If Not vor(i).isEqualExceptSelection(nach(i)) Then
                notwendig = True
            Else
                vor(i) = Nothing
                nach(i) = Nothing
            End If
        Next
        Return notwendig
    End Function

    Public Overrides Sub macheRückgängig(args As RückgängigArgs)
        If notwenig_Normal Then ladeZustand(vor, args)
    End Sub

    Public Sub machRückgängigInklusiveKorrekterSelection(args As RückgängigArgs)
        If notwenig_Normal Then ladeZustandMitSelection(vor, args)
    End Sub

    Public Overrides Sub macheVorgängig(args As RückgängigArgs)
        If notwenig_Normal Then ladeZustand(nach, args)
    End Sub
End Class