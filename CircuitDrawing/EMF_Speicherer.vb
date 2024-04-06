Imports System.Drawing.Imaging
Imports System.IO
Public Class EMF_Speicherer
    Public Shared Sub SaveAsEmf(metafile As Metafile, pfad As String)
        Dim enfMetafileHandle As Integer = metafile.GetHenhmetafile().ToInt32()
        Dim bufferSize As Integer = GetEnhMetaFileBits(enfMetafileHandle, 0, Nothing) 'Get required buffer size.  
        Dim buffer(bufferSize - 1) As Byte 'Allocate sufficient buffer  
        If GetEnhMetaFileBits(enfMetafileHandle, bufferSize, buffer) <= 0 Then ' Get raw metafile data.  
            Throw New SystemException("Failed to get raw metafile data")
        End If

        Dim ms As FileStream = Nothing
        Try
            ms = New FileStream(pfad, FileMode.Create, FileAccess.Write)
            ms.Write(buffer, 0, bufferSize)
        Catch ex As Exception
            MessageBox.Show("Export als EMF fehlgeschlagen: " & ex.Message)
        Finally
            If ms IsNot Nothing Then
                ms.Close()
                ms.Dispose()
            End If
        End Try

        If Not DeleteEnhMetaFile(enfMetafileHandle) Then 'free handle  
            Throw New SystemException("Failed to Free metafile")
        End If
    End Sub

    Public Declare Function GetEnhMetaFileBits Lib "gdi32.dll" (hemf As Integer, cbBuffer As Integer, lpbBuffer() As Byte) As Integer

    Public Declare Function DeleteEnhMetaFile Lib "gdi32.dll" (hemfBitHandle As Integer) As Boolean
End Class
