Imports System.IO
Public Class ElementGruppe
    Inherits Element

    Private mySubElements As List(Of ElementMaster)

    Public Sub New(ID As ULong)
        MyBase.New(ID)
        mySubElements = New List(Of ElementMaster)
    End Sub

    Public Sub addElement(e As ElementMaster)
        Me.mySubElements.Add(e)
    End Sub

    Public Function getElemente() As List(Of ElementMaster)
        Return mySubElements
    End Function

    Public Overrides Function getGrafik(args As getGrafikArgs) As DO_Grafik
        args.deltaX += position.X
        args.deltaY += position.Y

        Dim g As New DO_MultiGrafik()
        For i As Integer = 0 To mySubElements.Count - 1
            g.childs.Add(mySubElements(i).getGrafik(args))
        Next
        If Me.position.X <> 0 OrElse Me.position.Y <> 0 Then
            g.transform(New Transform_translate(Me.position))
        End If

        args.deltaX -= position.X
        args.deltaY -= position.Y
        Return g
    End Function

    Public Sub getWires(w As List(Of Tuple(Of Point, Point)), dx As Integer, dy As Integer)
        For Each el As ElementMaster In mySubElements
            If TypeOf el Is IWire Then w.Add(New Tuple(Of Point, Point)(DirectCast(el, IWire).getStart(New Point(dx + position.X, dy + position.Y)), DirectCast(el, IWire).getEnde(New Point(dx + position.X, dy + position.Y))))
            If TypeOf el Is ElementGruppe Then
                DirectCast(el, ElementGruppe).getWires(w, dx + position.X, dy + position.Y)
            End If
        Next
    End Sub

    Public Overrides Function getSelection() As Selection
        Dim r As Rectangle
        Dim first As Boolean = True
        For i As Integer = 0 To mySubElements.Count - 1
            If TypeOf mySubElements(i) Is Element Then
                Dim s As Selection = DirectCast(mySubElements(i), Element).getSelection()
                If first Then
                    r = s.getGrafik().getBoundingBox()
                    first = False
                Else
                    r = Mathe.Union(r, s.getGrafik().getBoundingBox())
                End If
            ElseIf TypeOf mySubElements(i) Is SnapableElement Then
                For j As Integer = 0 To DirectCast(mySubElements(i), SnapableElement).getNrOfSnappoints() - 1
                    If DirectCast(mySubElements(i), SnapableElement).getSnappoint(j).isSelected Then
                        Dim s As Selection = DirectCast(mySubElements(i), SnapableElement).getSnappoint(j).getSelection
                        If first Then
                            r = s.getGrafik().getBoundingBox()
                            first = False
                        Else
                            r = Mathe.Union(r, s.getGrafik().getBoundingBox())
                        End If
                    End If
                Next
            End If
        Next
        r.X += position.X
        r.Y += position.Y
        Return New SelectionRect(r)
    End Function

    Public Overrides Sub drehe(drehpunkt As Point, drehung As Drehmatrix)
        drehpunkt = New Point(drehpunkt.X - position.X, drehpunkt.Y - position.Y)
        For i As Integer = 0 To mySubElements.Count - 1
            mySubElements(i).drehe(drehpunkt, drehung)
        Next
    End Sub

    Public Overrides Function NrOfSnappoints() As Integer
        Dim nr As Integer = 0
        For i As Integer = 0 To mySubElements.Count - 1
            If TypeOf mySubElements(i) Is Element Then
                nr += DirectCast(mySubElements(i), Element).NrOfSnappoints()
            End If
        Next
        Return nr
    End Function

    Public Overrides Function getSnappoint(i As Integer) As Snappoint
        Dim nr As Integer = 0
        For el As Integer = 0 To mySubElements.Count - 1
            If TypeOf mySubElements(el) Is Element Then
                Dim n As Integer = DirectCast(mySubElements(el), Element).NrOfSnappoints()
                If i - nr < n Then
                    Dim s As Snappoint = DirectCast(mySubElements(el), Element).getSnappoint(i - nr)
                    s.p = New Point(s.p.X + position.X, s.p.Y + position.Y)
                    Return s
                End If
                nr += n
            End If
        Next
        Return Nothing
    End Function

    Public Overrides Function Clone() As ElementMaster
        Dim erg As New ElementGruppe(Me.ID)
        erg.position = Me.position
        erg.isSelected = Me.isSelected
        For i As Integer = 0 To Me.mySubElements.Count - 1
            erg.mySubElements.Add(mySubElements(i).Clone())
        Next
        Return erg
    End Function

    Public Overrides Function Clone(get_newID As Func(Of ULong)) As ElementMaster
        Dim erg As New ElementGruppe(get_newID())
        erg.position = Me.position
        erg.isSelected = Me.isSelected

        For i As Integer = 0 To Me.mySubElements.Count - 1
            erg.mySubElements.Add(mySubElements(i).Clone(get_newID))
        Next
        Return erg
    End Function

    Public Sub FlatCopyCompiledTemplates(newGruppe As ElementGruppe)
        For i As Integer = 0 To mySubElements.Count - 1
            If TypeOf mySubElements(i) Is BauteilAusDatei AndAlso TypeOf newGruppe.mySubElements(i) Is BauteilAusDatei Then
                DirectCast(mySubElements(i), BauteilAusDatei).FlatCopyCompiledTemplate(DirectCast(newGruppe.mySubElements(i), BauteilAusDatei))
            ElseIf TypeOf mySubElements(i) Is ElementGruppe AndAlso TypeOf newGruppe.mySubElements(i) Is ElementGruppe Then
                DirectCast(mySubElements(i), ElementGruppe).FlatCopyCompiledTemplates(DirectCast(newGruppe.mySubElements(i), ElementGruppe))
            End If
        Next
    End Sub

    Public Overrides Function isEqualExceptSelection(e2 As ElementMaster) As Boolean
        If TypeOf e2 IsNot ElementGruppe Then Return False
        With DirectCast(e2, ElementGruppe)
            If .position.X <> Me.position.X Then Return False
            If .position.Y <> Me.position.Y Then Return False
            If .mySubElements.Count <> Me.mySubElements.Count Then Return False
            For i As Integer = 0 To .mySubElements.Count - 1
                If Not Me.mySubElements(i).isEqualExceptSelection(.mySubElements(i)) Then Return False
            Next
        End With
        Return True
    End Function

    Public Overrides Function getEinstellungen(sender As Vektor_Picturebox) As List(Of ElementEinstellung)
        Dim l As List(Of ElementEinstellung) = Nothing
        For i As Integer = 0 To mySubElements.Count - 1
            Dim lsub As List(Of ElementEinstellung) = mySubElements(i).getEinstellungen(sender)
            For j As Integer = lsub.Count - 1 To 0 Step -1
                If lsub(j).Name.get_ID() = EINSTELLUNG_POS Then
                    lsub.RemoveAt(j)
                End If
            Next
            If l Is Nothing Then
                l = lsub
            Else
                Dim hatEinstellungInBeiden As Boolean = False
                For k As Integer = l.Count - 1 To 0 Step -1
                    hatEinstellungInBeiden = False
                    For j As Integer = 0 To lsub.Count - 1
                        If l(k).Name.get_ID() = lsub(j).Name.get_ID() AndAlso l(k).GetType().ToString() = lsub(j).GetType().ToString() Then
                            hatEinstellungInBeiden = True
                            l(k).CombineValues(lsub(j))
                            Exit For
                        End If
                    Next
                    If Not hatEinstellungInBeiden Then
                        l.RemoveAt(k)
                    ElseIf TypeOf l(k) Is Einstellung_Multi AndAlso DirectCast(l(k), Einstellung_Multi).getListe().Count = 0 Then
                        l.RemoveAt(k) 'Leere Liste löschen!
                    End If
                Next
            End If
        Next
        l.AddRange(MyBase.getEinstellungen(sender))
        Return l
    End Function

    Public Overrides Function setEinstellungen(sender As Vektor_Picturebox, einstellungen As List(Of ElementEinstellung)) As Boolean
        Dim changed As Boolean = MyBase.setEinstellungen(sender, einstellungen)
        For i As Integer = 0 To mySubElements.Count - 1
            If mySubElements(i).setEinstellungen(sender, einstellungen) Then
                changed = True
            End If
        Next
        Return changed
    End Function

    Public Overrides Sub speichern(writer As IO.BinaryWriter)
        writer.Write(Me.position.X)
        writer.Write(Me.position.Y)
        writer.Write(Me.mySubElements.Count)
        For i As Integer = 0 To mySubElements.Count - 1
            If TypeOf mySubElements(i) Is Element Then
                Vektor_Picturebox.SpeicherCircuitElement(DirectCast(mySubElements(i), Element), writer)
            ElseIf TypeOf mySubElements(i) Is SnapableElement Then
                Vektor_Picturebox.SpeicherSnapElement(DirectCast(mySubElements(i), SnapableElement), writer)
            End If
        Next
    End Sub

    Public Shared Function Einlesen(sender As Vektor_Picturebox, reader As BinaryReader, version As Integer, bib As Bibliothek, lokaleBib As LokaleBibliothek, kompatibilität As Boolean, fillStile_VersionKleiner27_transparent As Integer) As ElementGruppe
        Dim posX As Integer = reader.ReadInt32()
        Dim posY As Integer = reader.ReadInt32()
        Dim nrOfSubElements As Integer = reader.ReadInt32()
        Dim erg As New ElementGruppe(sender.getNewID())
        erg.position = New Point(posX, posY)
        For i As Integer = 0 To nrOfSubElements - 1
            Dim typ As Integer = reader.ReadInt32()
            If typ = Vektor_Picturebox._SPEICHERN_StartElement Then
                erg.mySubElements.Add(Vektor_Picturebox.LadeCircuitElement(sender, reader, version, bib, lokaleBib, kompatibilität, fillStile_VersionKleiner27_transparent, typ))
            ElseIf typ = Vektor_Picturebox._SPEICHERN_StartElementSnapping Then
                erg.mySubElements.Add(Vektor_Picturebox.LadeSnapElement(sender, reader, version, typ))
            End If
        Next

        Return erg
    End Function
End Class
