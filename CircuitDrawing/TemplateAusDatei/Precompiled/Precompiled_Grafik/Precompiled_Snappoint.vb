Imports System.IO
Public Class Precompiled_Snappoint
    Inherits Precompiled_Grafik

    Private x, y As Ausdruck_Int
    Private xmin, ymin, xmax, ymax As Ausdruck_Int
    Private dirX, dirY As Ausdruck_Int
    Private use_dir As Boolean
    Private onlyThisDir As Ausdruck_Boolean
    Private keinWireAnschluss As Ausdruck_Boolean

    Private Sub New()
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int)
        Me.New(x, y, New Ausdruck_Konstante(0), New Ausdruck_Konstante(0), New Ausdruck_Boolean_Konstante(False))
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, xmin As Ausdruck_Int, ymin As Ausdruck_Int, xmax As Ausdruck_Int, ymax As Ausdruck_Int)
        Me.x = x
        Me.y = y
        Me.xmin = xmin
        Me.xmax = xmax
        Me.ymin = ymin
        Me.ymax = ymax
        Me.keinWireAnschluss = New Ausdruck_Boolean_Konstante(False)
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, xmin As Ausdruck_Int, ymin As Ausdruck_Int, xmax As Ausdruck_Int, ymax As Ausdruck_Int, keinWireAnschluss As Ausdruck_Boolean)
        Me.x = x
        Me.y = y
        Me.xmin = xmin
        Me.xmax = xmax
        Me.ymin = ymin
        Me.ymax = ymax
        Me.keinWireAnschluss = keinWireAnschluss
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, dirX As Ausdruck_Int, dirY As Ausdruck_Int, onlyThisDir As Ausdruck_Boolean)
        Me.x = x
        Me.y = y
        Me.dirX = dirX
        Me.dirY = dirY
        Me.use_dir = True
        Me.onlyThisDir = onlyThisDir
        Me.keinWireAnschluss = New Ausdruck_Boolean_Konstante(False)
    End Sub

    Public Sub New(x As Ausdruck_Int, y As Ausdruck_Int, xmin As Integer, ymin As Integer, xmax As Integer, ymax As Integer)
        Me.x = x
        Me.y = y
        Me.xmin = New Ausdruck_Konstante(xmin)
        Me.xmax = New Ausdruck_Konstante(xmax)
        Me.ymin = New Ausdruck_Konstante(ymin)
        Me.ymax = New Ausdruck_Konstante(ymax)
        Me.keinWireAnschluss = New Ausdruck_Boolean_Konstante(False)
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        Dim pos As Point
        If x Is Nothing Then
            pos.X = 0
        Else
            pos.X = args.ausrechnen(x)
        End If
        If y Is Nothing Then
            pos.Y = 0
        Else
            pos.Y = args.ausrechnen(y)
        End If

        Dim Xminus, Xplus, Yminus, Yplus As Integer

        If use_dir Then
            Dim onlyThisDir As Boolean = args.ausrechnen(Me.onlyThisDir)
            Dim gox As Integer = args.ausrechnen(dirX)
            Dim goy As Integer = args.ausrechnen(dirY)
            If gox = 0 AndAlso goy > 0 Then
                'go Bottom!
                If onlyThisDir Then
                    Xminus = TemplateAusDatei.PENALTY2
                    Xplus = TemplateAusDatei.PENALTY2
                    Yminus = TemplateAusDatei.PENALTY2
                    Yplus = TemplateAusDatei.PENALTY0
                Else
                    Xminus = TemplateAusDatei.PENALTY1
                    Xplus = TemplateAusDatei.PENALTY1
                    Yminus = TemplateAusDatei.PENALTY2
                    Yplus = TemplateAusDatei.PENALTY0
                End If
            ElseIf gox = 0 AndAlso goy < 0 Then
                'go Top!
                If onlyThisDir Then
                    Xminus = TemplateAusDatei.PENALTY2
                    Xplus = TemplateAusDatei.PENALTY2
                    Yminus = TemplateAusDatei.PENALTY0
                    Yplus = TemplateAusDatei.PENALTY2
                Else
                    Xminus = TemplateAusDatei.PENALTY1
                    Xplus = TemplateAusDatei.PENALTY1
                    Yminus = TemplateAusDatei.PENALTY0
                    Yplus = TemplateAusDatei.PENALTY2
                End If
            ElseIf gox > 0 AndAlso goy = 0 Then
                'go right
                If onlyThisDir Then
                    Xminus = TemplateAusDatei.PENALTY2
                    Xplus = TemplateAusDatei.PENALTY0
                    Yminus = TemplateAusDatei.PENALTY2
                    Yplus = TemplateAusDatei.PENALTY2
                Else
                    Xminus = TemplateAusDatei.PENALTY2
                    Xplus = TemplateAusDatei.PENALTY0
                    Yminus = TemplateAusDatei.PENALTY1
                    Yplus = TemplateAusDatei.PENALTY1
                End If
            ElseIf gox < 0 AndAlso goy = 0 Then
                'go left
                If onlyThisDir Then
                    Xminus = TemplateAusDatei.PENALTY0
                    Xplus = TemplateAusDatei.PENALTY2
                    Yminus = TemplateAusDatei.PENALTY2
                    Yplus = TemplateAusDatei.PENALTY2
                Else
                    Xminus = TemplateAusDatei.PENALTY0
                    Xplus = TemplateAusDatei.PENALTY2
                    Yminus = TemplateAusDatei.PENALTY1
                    Yplus = TemplateAusDatei.PENALTY1
                End If
            Else
                Xminus = 0
                Xplus = 0
                Yminus = 0
                Yplus = 0
            End If
        Else
            If Me.xmin Is Nothing Then
                Xminus = 0
            Else
                Xminus = args.ausrechnen(Me.xmin)
            End If
            If Me.xmax Is Nothing Then
                Xplus = 0
            Else
                Xplus = args.ausrechnen(Me.xmax)
            End If
            If Me.ymin Is Nothing Then
                Yminus = 0
            Else
                Yminus = args.ausrechnen(Me.ymin)
            End If
            If Me.ymax Is Nothing Then
                Yplus = 0
            Else
                Yplus = args.ausrechnen(Me.ymax)
            End If
        End If

        Dim keinWireAnschluss As Boolean = args.ausrechnen(Me.keinWireAnschluss)

        erg.snaps.Add(New Snappoint(pos, Xminus, Xplus, Yminus, Yplus, keinWireAnschluss))
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(use_dir)
        Ausdruck_Int.speichern(x, writer)
        Ausdruck_Int.speichern(y, writer)
        Ausdruck_Int.speichern_kannNothingSein(xmin, writer)
        Ausdruck_Int.speichern_kannNothingSein(ymin, writer)
        Ausdruck_Int.speichern_kannNothingSein(xmax, writer)
        Ausdruck_Int.speichern_kannNothingSein(ymax, writer)
        Ausdruck_Int.speichern_kannNothingSein(dirX, writer)
        Ausdruck_Int.speichern_kannNothingSein(dirY, writer)

        Ausdruck_Boolean.speichern(keinWireAnschluss, writer)
        Ausdruck_Boolean.speichern_kannNothingSein(onlyThisDir, writer)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_Snappoint
        Dim erg As New Precompiled_Snappoint()
        erg.use_dir = reader.ReadBoolean()
        erg.x = Ausdruck_Int.laden(reader, version)
        erg.y = Ausdruck_Int.laden(reader, version)
        erg.xmin = Ausdruck_Int.laden_kannNothingSein(reader, version)
        erg.ymin = Ausdruck_Int.laden_kannNothingSein(reader, version)
        erg.xmax = Ausdruck_Int.laden_kannNothingSein(reader, version)
        erg.ymax = Ausdruck_Int.laden_kannNothingSein(reader, version)
        erg.dirX = Ausdruck_Int.laden_kannNothingSein(reader, version)
        erg.dirY = Ausdruck_Int.laden_kannNothingSein(reader, version)

        erg.keinWireAnschluss = Ausdruck_Boolean.laden(reader, version)
        erg.onlyThisDir = Ausdruck_Boolean.laden_kannNothingSein(reader, version)

        Return erg
    End Function

    Public Sub export(writer As Export_StreamWriter)
        If use_dir Then
            Dim line As String = "snap("
            line &= Ausdruck_Int.export(x, writer).str & ", "
            line &= Ausdruck_Int.export(y, writer).str & ", "
            line &= Ausdruck_Int.export(dirX, writer).str & ", "
            line &= Ausdruck_Int.export(dirY, writer).str
            If TypeOf onlyThisDir Is Ausdruck_Boolean_Konstante AndAlso onlyThisDir.Ausrechnen(Nothing) = True Then
                line &= ")"
            Else
                line &= ", " & Ausdruck_Boolean.export(onlyThisDir, writer).str & ")"
            End If
            writer.WriteLine(line)
        Else
            Dim line As String = "snap("
            line &= Ausdruck_Int.export(x, writer).str & ", "
            line &= Ausdruck_Int.export(y, writer).str
            If TypeOf xmin Is Ausdruck_Konstante AndAlso xmin.Ausrechnen(Nothing) = 0 AndAlso
               TypeOf ymin Is Ausdruck_Konstante AndAlso ymin.Ausrechnen(Nothing) = 0 AndAlso
               TypeOf xmax Is Ausdruck_Konstante AndAlso xmax.Ausrechnen(Nothing) = 0 AndAlso
               TypeOf ymax Is Ausdruck_Konstante AndAlso ymax.Ausrechnen(Nothing) = 0 AndAlso
               TypeOf keinWireAnschluss Is Ausdruck_Boolean_Konstante AndAlso keinWireAnschluss.Ausrechnen(Nothing) = False Then
                'alle richtungen Null! Einfache Lösung nehmen!
                line &= ")"
            Else
                line &= ", "
                line &= Ausdruck_Int.export(xmin, writer).str & ", "
                line &= Ausdruck_Int.export(ymin, writer).str & ", "
                line &= Ausdruck_Int.export(xmax, writer).str & ", "
                line &= Ausdruck_Int.export(ymax, writer).str
                If TypeOf keinWireAnschluss Is Ausdruck_Boolean_Konstante AndAlso keinWireAnschluss.Ausrechnen(Nothing) = False Then
                    line &= ")"
                Else
                    line &= ", " & Ausdruck_Boolean.export(keinWireAnschluss, writer).str & ")"
                End If
            End If
            writer.WriteLine(line)
        End If
    End Sub
End Class
