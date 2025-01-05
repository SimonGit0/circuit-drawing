<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WR_Einstellungsform
    Inherits System.Windows.Forms.UserControl

    'UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WR_Einstellungsform))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.cmbShowStyle = New CircuitDrawing.JoSiCombobox()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'cmbShowStyle
        '
        resources.ApplyResources(Me.cmbShowStyle, "cmbShowStyle")
        Me.cmbShowStyle.BackColor = System.Drawing.Color.White
        Me.cmbShowStyle.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.cmbShowStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbShowStyle.EndEllipses = False
        Me.cmbShowStyle.FärbenStärke = 0
        Me.cmbShowStyle.FärbenStärkeSelected = 0
        Me.cmbShowStyle.ForeColor = System.Drawing.Color.Black
        Me.cmbShowStyle.FormattingEnabled = True
        Me.cmbShowStyle.Items.AddRange(New Object() {resources.GetString("cmbShowStyle.Items"), resources.GetString("cmbShowStyle.Items1")})
        Me.cmbShowStyle.Name = "cmbShowStyle"
        Me.cmbShowStyle.renderColor = System.Drawing.Color.Black
        Me.cmbShowStyle.RenderColorSelected = System.Drawing.Color.Black
        Me.cmbShowStyle.Various = False
        '
        'WR_Einstellungsform
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.cmbShowStyle)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.Name = "WR_Einstellungsform"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Button1 As Button
    Friend WithEvents cmbShowStyle As JoSiCombobox
End Class
