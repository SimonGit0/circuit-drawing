Imports System.IO
Public Class Precompiled_SetFillColor
    Inherits Precompiled_Grafik

    Private forecolor As Boolean
    Public Sub New(forecolor As Boolean)
        Me.forecolor = forecolor
    End Sub

    Public Overrides Sub compile(args As AusrechnenArgs, erg As CompileArgs)
        erg.use_forecolor_fill = forecolor
        If forecolor = False Then
            erg.benötigt_fillStil = True
        End If
    End Sub

    Public Sub speichern(writer As BinaryWriter)
        writer.Write(forecolor)
    End Sub

    Public Shared Function laden(reader As BinaryReader, version As Integer) As Precompiled_SetFillColor
        Return New Precompiled_SetFillColor(reader.ReadBoolean())
    End Function

    Public Sub export(writer As Export_StreamWriter)
        Dim line As String
        If forecolor Then
            line = "fill_forecolor()"
        Else
            line = "fill_backcolor()"
        End If
        writer.WriteLine(line)
    End Sub
End Class
