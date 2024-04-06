Imports System.Runtime.InteropServices
Public Class Combobox_Sortieren
    Inherits JoSiCombobox

    Public Sub New()
        MyBase.New()

        Me.Items.Add(Sortieren.LinksNachRechts_ObenNachUnten)
        Me.Items.Add(Sortieren.LinksNachRechts_UntenNachOben)
        Me.Items.Add(Sortieren.RechtsNachLinks_ObenNachUnten)
        Me.Items.Add(Sortieren.RechtsNachLinks_UntenNachOben)

        Me.Items.Add(Sortieren.ObenNachUnten_LinksNachRechts)
        Me.Items.Add(Sortieren.ObenNachUnten_RechtsNachLinks)
        Me.Items.Add(Sortieren.UntenNachOben_LinksNachRechts)
        Me.Items.Add(Sortieren.UntenNachOben_RechtsNachLinks)

        Me.IntegralHeight = False
        Me.ItemHeight = 48
        Me.Height = 23
        setHeigth(23 - 6)
    End Sub

    Protected Overrides Sub OnFontChanged(e As EventArgs)
        MyBase.OnFontChanged(e)
        Me.setHeigth(23 - 6)
    End Sub

    <DllImport("User32.dll")>
    Private Shared Function SendMessage(hWnd As IntPtr, Msg As UInt32, wParam As Int32, lParam As Int32) As IntPtr
    End Function
    Private Const CB_SETITEMHEIGHT As Int32 = 339 '0x153
    Private Sub SetComboBoxHeight(comboBoxHandle As IntPtr, comboBoxDesiredHeight As Int32)
        SendMessage(comboBoxHandle, CB_SETITEMHEIGHT, -1, comboBoxDesiredHeight)
    End Sub
    Private Sub setHeigth(h As Integer)
        SetComboBoxHeight(Me.Handle, h)
    End Sub

    Protected Overrides Sub OnDrawItemDropDownForeground(e As DrawItemEventArgs)
        Dim textColor As Color = e.ForeColor
        If (e.State And DrawItemState.Selected) <> 0 Then
            e.Graphics.FillRectangle(selectionColorBrush, e.Bounds)
            textColor = foreColorSelected
        End If

        Dim bmp As Bitmap = getBmp(e.Index)
        Dim txt As String = getText(e.Index)
        If bmp IsNot Nothing Then
            e.Graphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            e.Graphics.DrawImage(bmp, New Rectangle(e.Bounds.Location, New Size(e.Bounds.Height, e.Bounds.Height)))
        End If
        TextRenderer.DrawText(e.Graphics, txt, e.Font, New Rectangle(e.Bounds.X + e.Bounds.Height + Text_Render_Offset, e.Bounds.Y, e.Bounds.Width - e.Bounds.Height, e.Bounds.Height), textColor, TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)
    End Sub

    Protected Overrides Sub OnDrawItemSelectedForeground(e As DrawItemEventArgs, text As String)
        MyBase.OnDrawItemSelectedForeground(e, My.Resources.Strings.Nummerierung & ": " & getText(e.Index))
    End Sub

    Private Function getBmp(index As Integer) As Bitmap
        If index = -1 Then Return Nothing
        Dim sort As Sortieren = CType(Me.Items(index), Sortieren)
        Select Case sort
            Case Sortieren.LinksNachRechts_ObenNachUnten
                Return My.Resources.Sortieren_LO
            Case Sortieren.LinksNachRechts_UntenNachOben
                Return My.Resources.Sortieren_LU
            Case Sortieren.RechtsNachLinks_ObenNachUnten
                Return My.Resources.Sortieren_RO
            Case Sortieren.RechtsNachLinks_UntenNachOben
                Return My.Resources.Sortieren_RU
            Case Sortieren.ObenNachUnten_LinksNachRechts
                Return My.Resources.Sortieren_OL
            Case Sortieren.ObenNachUnten_RechtsNachLinks
                Return My.Resources.Sortieren_OR
            Case Sortieren.UntenNachOben_LinksNachRechts
                Return My.Resources.Sortieren_UL
            Case Sortieren.UntenNachOben_RechtsNachLinks
                Return My.Resources.Sortieren_UR
        End Select
        Return Nothing
    End Function

    Private Function getText(index As Integer) As String
        If index = -1 Then Return ""
        Dim sort As Sortieren = CType(Me.Items(index), Sortieren)
        Select Case sort
            Case Sortieren.LinksNachRechts_ObenNachUnten
                Return My.Resources.Strings.Links & " -> " & My.Resources.Strings.Rechts & "; " & My.Resources.Strings.Oben & " -> " & My.Resources.Strings.Unten
            Case Sortieren.LinksNachRechts_UntenNachOben
                Return My.Resources.Strings.Links & " -> " & My.Resources.Strings.Rechts & "; " & My.Resources.Strings.Unten & " -> " & My.Resources.Strings.Oben
            Case Sortieren.RechtsNachLinks_ObenNachUnten
                Return My.Resources.Strings.Rechts & " -> " & My.Resources.Strings.Links & "; " & My.Resources.Strings.Oben & " -> " & My.Resources.Strings.Unten
            Case Sortieren.RechtsNachLinks_UntenNachOben
                Return My.Resources.Strings.Rechts & " -> " & My.Resources.Strings.Links & "; " & My.Resources.Strings.Unten & " -> " & My.Resources.Strings.Oben
            Case Sortieren.ObenNachUnten_LinksNachRechts
                Return My.Resources.Strings.Oben & " -> " & My.Resources.Strings.Unten & "; " & My.Resources.Strings.Links & " -> " & My.Resources.Strings.Rechts
            Case Sortieren.ObenNachUnten_RechtsNachLinks
                Return My.Resources.Strings.Oben & " -> " & My.Resources.Strings.Unten & "; " & My.Resources.Strings.Rechts & " -> " & My.Resources.Strings.Links
            Case Sortieren.UntenNachOben_LinksNachRechts
                Return My.Resources.Strings.Unten & " -> " & My.Resources.Strings.Oben & "; " & My.Resources.Strings.Links & " -> " & My.Resources.Strings.Rechts
            Case Sortieren.UntenNachOben_RechtsNachLinks
                Return My.Resources.Strings.Unten & " -> " & My.Resources.Strings.Oben & "; " & My.Resources.Strings.Rechts & " -> " & My.Resources.Strings.Links
        End Select
        Return ""
    End Function

    Public Function getSortierung() As Sortieren
        Return DirectCast(Me.SelectedItem, Sortieren)
    End Function


    Public Enum Sortieren
        LinksNachRechts_ObenNachUnten
        LinksNachRechts_UntenNachOben
        RechtsNachLinks_ObenNachUnten
        RechtsNachLinks_UntenNachOben

        ObenNachUnten_LinksNachRechts
        ObenNachUnten_RechtsNachLinks
        UntenNachOben_LinksNachRechts
        UntenNachOben_RechtsNachLinks
    End Enum
End Class
