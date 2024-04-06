<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SkillDetektionForm
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(SkillDetektionForm))
        Me.Vektor_Picturebox1 = New CircuitDrawing.Vektor_Picturebox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.Button3 = New System.Windows.Forms.Button()
        Me.nudSkalierung = New System.Windows.Forms.NumericUpDown()
        Me.lblSkalierung = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Combobox_Sortieren2 = New CircuitDrawing.Combobox_Sortieren()
        Me.Ckb_Wire = New System.Windows.Forms.CheckBox()
        Me.ckb_Terminals = New System.Windows.Forms.CheckBox()
        Me.Button5 = New System.Windows.Forms.Button()
        CType(Me.nudSkalierung, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Vektor_Picturebox1
        '
        resources.ApplyResources(Me.Vektor_Picturebox1, "Vektor_Picturebox1")
        Me.Vektor_Picturebox1.currentPlaceBauteil = Nothing
        Me.Vektor_Picturebox1.Name = "Vektor_Picturebox1"
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Button4
        '
        resources.ApplyResources(Me.Button4, "Button4")
        Me.Button4.Name = "Button4"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Button3
        '
        resources.ApplyResources(Me.Button3, "Button3")
        Me.Button3.Name = "Button3"
        Me.Button3.UseVisualStyleBackColor = True
        '
        'nudSkalierung
        '
        resources.ApplyResources(Me.nudSkalierung, "nudSkalierung")
        Me.nudSkalierung.Increment = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudSkalierung.Maximum = New Decimal(New Integer() {10000, 0, 0, 0})
        Me.nudSkalierung.Minimum = New Decimal(New Integer() {100, 0, 0, 0})
        Me.nudSkalierung.Name = "nudSkalierung"
        Me.nudSkalierung.Value = New Decimal(New Integer() {1700, 0, 0, 0})
        '
        'lblSkalierung
        '
        resources.ApplyResources(Me.lblSkalierung, "lblSkalierung")
        Me.lblSkalierung.Name = "lblSkalierung"
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Combobox_Sortieren2
        '
        resources.ApplyResources(Me.Combobox_Sortieren2, "Combobox_Sortieren2")
        Me.Combobox_Sortieren2.BackColor = System.Drawing.Color.White
        Me.Combobox_Sortieren2.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.Combobox_Sortieren2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.Combobox_Sortieren2.EndEllipses = False
        Me.Combobox_Sortieren2.FärbenStärke = 0
        Me.Combobox_Sortieren2.FärbenStärkeSelected = 0
        Me.Combobox_Sortieren2.ForeColor = System.Drawing.Color.Black
        Me.Combobox_Sortieren2.FormattingEnabled = True
        Me.Combobox_Sortieren2.Name = "Combobox_Sortieren2"
        Me.Combobox_Sortieren2.renderColor = System.Drawing.Color.Black
        Me.Combobox_Sortieren2.RenderColorSelected = System.Drawing.Color.Black
        Me.Combobox_Sortieren2.Various = False
        '
        'Ckb_Wire
        '
        resources.ApplyResources(Me.Ckb_Wire, "Ckb_Wire")
        Me.Ckb_Wire.Checked = True
        Me.Ckb_Wire.CheckState = System.Windows.Forms.CheckState.Checked
        Me.Ckb_Wire.Name = "Ckb_Wire"
        Me.Ckb_Wire.UseVisualStyleBackColor = True
        '
        'ckb_Terminals
        '
        resources.ApplyResources(Me.ckb_Terminals, "ckb_Terminals")
        Me.ckb_Terminals.Checked = True
        Me.ckb_Terminals.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ckb_Terminals.Name = "ckb_Terminals"
        Me.ckb_Terminals.UseVisualStyleBackColor = True
        '
        'Button5
        '
        resources.ApplyResources(Me.Button5, "Button5")
        Me.Button5.Name = "Button5"
        Me.Button5.UseVisualStyleBackColor = True
        '
        'SkillDetektionForm
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Button5)
        Me.Controls.Add(Me.ckb_Terminals)
        Me.Controls.Add(Me.Ckb_Wire)
        Me.Controls.Add(Me.Combobox_Sortieren2)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.nudSkalierung)
        Me.Controls.Add(Me.lblSkalierung)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.Button3)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Vektor_Picturebox1)
        Me.KeyPreview = True
        Me.Name = "SkillDetektionForm"
        CType(Me.nudSkalierung, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Vektor_Picturebox1 As Vektor_Picturebox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button4 As Button
    Friend WithEvents Button3 As Button
    Friend WithEvents nudSkalierung As NumericUpDown
    Friend WithEvents lblSkalierung As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents Combobox_Sortieren2 As Combobox_Sortieren
    Friend WithEvents Ckb_Wire As CheckBox
    Friend WithEvents ckb_Terminals As CheckBox
    Friend WithEvents Button5 As Button
End Class
