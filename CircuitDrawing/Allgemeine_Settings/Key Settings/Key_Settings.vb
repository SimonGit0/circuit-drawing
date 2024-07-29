Public Class Key_Settings

    Public ReadOnly keyUndo As ShortcutKey
    Public ReadOnly keyRedo As ShortcutKey

    Public ReadOnly keyToolAbbrechen As ShortcutKey

    Public ReadOnly keyToolDrawSnap As ShortcutKey
    Public ReadOnly keyToolChangeArrow As ShortcutKey

    Public ReadOnly keyToolDelete As ShortcutKey
    Public ReadOnly keyToolAddInstance As ShortcutKey
    Public ReadOnly keyToolAddInstanceMirrorX As ShortcutKey
    Public ReadOnly keyToolAddInstanceMirrorY As ShortcutKey
    Public ReadOnly keyToolMove As ShortcutKey
    Public ReadOnly keyToolCopy As ShortcutKey
    Public ReadOnly keyToolWire As ShortcutKey
    Public ReadOnly keyToolAddStrom As ShortcutKey
    Public ReadOnly keyToolAddSpannung As ShortcutKey
    Public ReadOnly keyToolAddLabel As ShortcutKey
    Public ReadOnly keyToolAddBusTap As ShortcutKey
    Public ReadOnly keyToolAddImpedanceArrow As ShortcutKey
    Public ReadOnly keyToolScale As ShortcutKey

    Public ReadOnly keySchalteBeschriftungsPosDurch As ShortcutKey

    Public ReadOnly keyChangeMoveMode As ShortcutKey
    Public ReadOnly keyChangeGravity As ShortcutKey
    Public ReadOnly keyMultiSelect As ShortcutKey

    Public ReadOnly keyFitToScreen As ShortcutKey

    Public ReadOnly keyRotate90 As ShortcutKey
    Public ReadOnly keyRotateMinus90 As ShortcutKey

    Public ReadOnly keyShowElementEinstellungen As ShortcutKey

    Public ReadOnly keyMarkierungAufheben As ShortcutKey
    Public ReadOnly keyAllesMarkieren As ShortcutKey

    Public ReadOnly keyDateiSpeichern As ShortcutKey

    Public ReadOnly keyRoutingOptimieren As ShortcutKey

    Public ReadOnly keyCopy As ShortcutKey
    Public ReadOnly keyPaste As ShortcutKey

    Public ReadOnly keyExportiereAlsTEX As ShortcutKey
    Public ReadOnly keyExportiereAlsEMF As ShortcutKey

    Public ReadOnly keyEbeneNachVorne As ShortcutKey
    Public ReadOnly keyEbeneNachHinten As ShortcutKey

    Private alleKeys As List(Of ShortcutKey)

    Private Sub New()
        alleKeys = New List(Of ShortcutKey)

        keyUndo = New ShortcutKey(Keys.U)
        alleKeys.Add(keyUndo)
        keyRedo = New ShortcutKey(Keys.U, False, True)
        alleKeys.Add(keyRedo)

        keyToolAbbrechen = New ShortcutKey(Keys.Escape)
        alleKeys.Add(keyToolAbbrechen)

        keyToolDrawSnap = New ShortcutKey(Keys.S)
        alleKeys.Add(keyToolDrawSnap)
        keyToolChangeArrow = New ShortcutKey(Keys.A)
        alleKeys.Add(keyToolChangeArrow)

        keyToolDelete = New ShortcutKey(Keys.Delete)
        alleKeys.Add(keyToolDelete)
        keyToolAddInstance = New ShortcutKey(Keys.I)
        alleKeys.Add(keyToolAddInstance)
        keyToolAddInstanceMirrorX = New ShortcutKey(Keys.X)
        alleKeys.Add(keyToolAddInstanceMirrorX)
        keyToolAddInstanceMirrorY = New ShortcutKey(Keys.Y)
        alleKeys.Add(keyToolAddInstanceMirrorY)
        keyToolMove = New ShortcutKey(Keys.M)
        alleKeys.Add(keyToolMove)
        keyToolCopy = New ShortcutKey(Keys.C)
        alleKeys.Add(keyToolCopy)
        keyToolWire = New ShortcutKey(Keys.W)
        alleKeys.Add(keyToolWire)
        keyToolAddStrom = New ShortcutKey(Keys.A)
        alleKeys.Add(keyToolAddStrom)
        keyToolAddSpannung = New ShortcutKey(Keys.V)
        alleKeys.Add(keyToolAddSpannung)
        keyToolAddLabel = New ShortcutKey(Keys.L)
        alleKeys.Add(keyToolAddLabel)
        keyToolAddBusTap = New ShortcutKey(Keys.B)
        alleKeys.Add(keyToolAddBusTap)
        keyToolAddImpedanceArrow = New ShortcutKey(Keys.Z)
        alleKeys.Add(keyToolAddImpedanceArrow)
        keyToolScale = New ShortcutKey(Keys.S)
        alleKeys.Add(keyToolScale)

        keySchalteBeschriftungsPosDurch = New ShortcutKey(Keys.Space)
        alleKeys.Add(keySchalteBeschriftungsPosDurch)

        keyChangeMoveMode = New ShortcutKey(Keys.N)
        alleKeys.Add(keyChangeMoveMode)
        keyChangeGravity = New ShortcutKey(Keys.G)
        alleKeys.Add(keyChangeGravity)
        keyMultiSelect = New ShortcutKey(Keys.R)
        alleKeys.Add(keyMultiSelect)

        keyFitToScreen = New ShortcutKey(Keys.F)
        alleKeys.Add(keyFitToScreen)

        keyRotate90 = New ShortcutKey(Keys.R, True)
        alleKeys.Add(keyRotate90)
        keyRotateMinus90 = New ShortcutKey(Keys.R, True, True)
        alleKeys.Add(keyRotateMinus90)

        keyShowElementEinstellungen = New ShortcutKey(Keys.Q)
        alleKeys.Add(keyShowElementEinstellungen)

        keyMarkierungAufheben = New ShortcutKey(Keys.D, True)
        alleKeys.Add(keyMarkierungAufheben)
        keyAllesMarkieren = New ShortcutKey(Keys.A, True)
        alleKeys.Add(keyAllesMarkieren)

        keyDateiSpeichern = New ShortcutKey(Keys.S, True)
        alleKeys.Add(keyDateiSpeichern)

        keyExportiereAlsTEX = New ShortcutKey(Keys.T, True)
        alleKeys.Add(keyExportiereAlsTEX)
        keyExportiereAlsEMF = New ShortcutKey(Keys.E, True)
        alleKeys.Add(keyExportiereAlsEMF)

        keyRoutingOptimieren = New ShortcutKey(Keys.P)
        alleKeys.Add(keyRoutingOptimieren)

        keyCopy = New ShortcutKey(Keys.C, True)
        alleKeys.Add(keyCopy)
        keyPaste = New ShortcutKey(Keys.V, True)
        alleKeys.Add(keyPaste)

        keyEbeneNachVorne = New ShortcutKey(Keys.Oemplus, True)
        alleKeys.Add(keyEbeneNachVorne)
        keyEbeneNachHinten = New ShortcutKey(Keys.OemMinus, True)
        alleKeys.Add(keyEbeneNachHinten)
    End Sub

    Public Function hasKey(k As ShortcutKey) As Boolean
        For Each keyHat As ShortcutKey In alleKeys
            If k.isEqual(keyHat) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function hasKey_ElementHinzufügen(k As ShortcutKey) As Boolean
        For Each keyHat As ShortcutKeySelectInstance In Settings.getSettings().KeysSelectInstance
            If k.isEqual(keyHat.key) Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Shared mySingleton As Key_Settings
    Public Shared Function getSettings() As Key_Settings
        If mySingleton Is Nothing Then
            'load key settings
            mySingleton = New Key_Settings()
        End If
        Return mySingleton
    End Function

End Class
