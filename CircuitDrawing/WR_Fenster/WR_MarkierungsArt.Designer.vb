<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WR_MarkierungsArt
    Inherits System.Windows.Forms.UserControl

    'UserControl überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WR_MarkierungsArt))
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.Ckb_Drawing = New CircuitDrawing.NoKeyCheckbox()
        Me.Ckb_CurrentArrow = New CircuitDrawing.NoKeyCheckbox()
        Me.Ckb_Wire = New CircuitDrawing.NoKeyCheckbox()
        Me.Ckb_Bauelemente = New CircuitDrawing.NoKeyCheckbox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Btn_NS = New System.Windows.Forms.Button()
        Me.btn_AS = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.Ckb_Drawing, 1, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.Ckb_CurrentArrow, 1, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.Ckb_Wire, 1, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Ckb_Bauelemente, 1, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Label6, 0, 4)
        Me.TableLayoutPanel1.Controls.Add(Me.Label4, 0, 3)
        Me.TableLayoutPanel1.Controls.Add(Me.Label3, 0, 2)
        Me.TableLayoutPanel1.Controls.Add(Me.Label1, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.Label2, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.Panel2, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'Ckb_Drawing
        '
        resources.ApplyResources(Me.Ckb_Drawing, "Ckb_Drawing")
        Me.Ckb_Drawing.Name = "Ckb_Drawing"
        Me.Ckb_Drawing.UseVisualStyleBackColor = True
        '
        'Ckb_CurrentArrow
        '
        resources.ApplyResources(Me.Ckb_CurrentArrow, "Ckb_CurrentArrow")
        Me.Ckb_CurrentArrow.Name = "Ckb_CurrentArrow"
        Me.Ckb_CurrentArrow.UseVisualStyleBackColor = True
        '
        'Ckb_Wire
        '
        resources.ApplyResources(Me.Ckb_Wire, "Ckb_Wire")
        Me.Ckb_Wire.Name = "Ckb_Wire"
        Me.Ckb_Wire.UseVisualStyleBackColor = True
        '
        'Ckb_Bauelemente
        '
        resources.ApplyResources(Me.Ckb_Bauelemente, "Ckb_Bauelemente")
        Me.Ckb_Bauelemente.Name = "Ckb_Bauelemente"
        Me.Ckb_Bauelemente.UseVisualStyleBackColor = True
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.Btn_NS)
        Me.Panel2.Controls.Add(Me.btn_AS)
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Name = "Panel2"
        '
        'Btn_NS
        '
        resources.ApplyResources(Me.Btn_NS, "Btn_NS")
        Me.Btn_NS.Name = "Btn_NS"
        Me.Btn_NS.UseVisualStyleBackColor = True
        '
        'btn_AS
        '
        resources.ApplyResources(Me.btn_AS, "btn_AS")
        Me.btn_AS.Name = "btn_AS"
        Me.btn_AS.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.TableLayoutPanel1)
        Me.Panel1.Name = "Panel1"
        '
        'WR_MarkierungsArt
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.Panel1)
        Me.Name = "WR_MarkierungsArt"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Ckb_Drawing As NoKeyCheckbox
    Friend WithEvents Ckb_CurrentArrow As NoKeyCheckbox
    Friend WithEvents Ckb_Wire As NoKeyCheckbox
    Friend WithEvents Ckb_Bauelemente As NoKeyCheckbox
    Friend WithEvents Label6 As Label
    Friend WithEvents Panel1 As Panel
    Friend WithEvents Panel2 As Panel
    Friend WithEvents Btn_NS As Button
    Friend WithEvents btn_AS As Button
End Class
