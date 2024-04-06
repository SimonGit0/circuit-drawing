<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FRM_ElementEinstellungen
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FRM_ElementEinstellungen))
        Me.Panel_Main = New System.Windows.Forms.Panel()
        Me.Btn_Ok = New System.Windows.Forms.Button()
        Me.Btn_Abbrechen = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Panel_Main
        '
        resources.ApplyResources(Me.Panel_Main, "Panel_Main")
        Me.Panel_Main.Name = "Panel_Main"
        '
        'Btn_Ok
        '
        resources.ApplyResources(Me.Btn_Ok, "Btn_Ok")
        Me.Btn_Ok.Name = "Btn_Ok"
        Me.Btn_Ok.UseVisualStyleBackColor = True
        '
        'Btn_Abbrechen
        '
        resources.ApplyResources(Me.Btn_Abbrechen, "Btn_Abbrechen")
        Me.Btn_Abbrechen.Name = "Btn_Abbrechen"
        Me.Btn_Abbrechen.UseVisualStyleBackColor = True
        '
        'FRM_ElementEinstellungen
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Btn_Abbrechen)
        Me.Controls.Add(Me.Btn_Ok)
        Me.Controls.Add(Me.Panel_Main)
        Me.Name = "FRM_ElementEinstellungen"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Panel_Main As Panel
    Friend WithEvents Btn_Ok As Button
    Friend WithEvents Btn_Abbrechen As Button
End Class
