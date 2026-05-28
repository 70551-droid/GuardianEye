namespace GuardianEye.Client;

partial class TimeRequestForm
{
    /// <summary>
    ///  Required designer variable.
    /// </>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </param>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.labelMinutes = new System.Windows.Forms.Label();
        this.numericUpDownMinutes = new System.Windows.Forms.NumericUpDown();
        this.labelReason = new System.Windows.Forms.Label();
        this.textBoxReason = new System.Windows.Forms.TextBox();
        this.buttonOK = new System.Windows.Forms.Button();
        this.buttonCancel = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).BeginInit();
        this.SuspendLayout();
        // 
        // labelMinutes
        // 
        this.labelMinutes.AutoSize = true;
        this.labelMinutes.Location = new System.Drawing.Point(12, 15);
        this.labelMinutes.Name = "labelMinutes";
        this.labelMinutes.Size = new System.Drawing.Size(47, 15);
        this.labelMinutes.TabIndex = 0;
        this.labelMinutes.Text = "Minutes:";
        // 
        // numericUpDownMinutes
        // 
        this.numericUpDownMinutes.Location = new System.Drawing.Point(78, 13);
        this.numericUpDownMinutes.Maximum = new decimal(new int[] {
        60,
        0,
        0,
        0});
        this.numericUpDownMinutes.Name = "numericUpDownMinutes";
        this.numericUpDownMinutes.Size = new System.Drawing.Size(120, 23);
        this.numericUpDownMinutes.TabIndex = 1;
        // 
        // labelReason
        // 
        this.labelReason.AutoSize = true;
        this.labelReason.Location = new System.Drawing.Point(12, 44);
        this.labelReason.Name = "labelReason";
        this.labelReason.Size = new System.Drawing.Size(45, 15);
        this.labelReason.TabIndex = 2;
        this.labelReason.Text = "Reason:";
        // 
        // textBoxReason
        // 
        this.textBoxReason.Location = new System.Drawing.Point(78, 42);
        this.textBoxReason.Name = "textBoxReason";
        this.textBoxReason.Size = new System.Drawing.Size(200, 23);
        this.textBoxReason.TabIndex = 3;
        // 
        // buttonOK
        // 
        this.buttonOK.Location = new System.Drawing.Point(78, 71);
        this.buttonOK.Name = "buttonOK";
        this.buttonOK.Size = new System.Drawing.Size(75, 23);
        this.buttonOK.TabIndex = 4;
        this.buttonOK.Text = "OK";
        this.buttonOK.UseVisualStyleBackColor = true;
        this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
        // 
        // buttonCancel
        // 
        this.buttonCancel.Location = new System.Drawing.Point(159, 71);
        this.buttonCancel.Name = "buttonCancel";
        this.buttonCancel.Size = new System.Drawing.Size(75, 23);
        this.buttonCancel.TabIndex = 5;
        this.buttonCancel.Text = "Cancel";
        this.buttonCancel.UseVisualStyleBackColor = true;
        this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
        // 
        // TimeRequestForm
        // 
        this.AcceptButton = this.buttonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.buttonCancel;
        this.ClientSize = new System.Drawing.Size(292, 106);
        this.Controls.Add(this.buttonCancel);
        this.Controls.Add(this.buttonOK);
        this.Controls.Add(this.textBoxReason);
        this.Controls.Add(this.labelReason);
        this.Controls.Add(this.numericUpDownMinutes);
        this.Controls.Add(this.labelMinutes);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "TimeRequestForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Request More Time";
        ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinutes)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label labelMinutes;
    private System.Windows.Forms.NumericUpDown numericUpDownMinutes;
    private System.Windows.Forms.Label labelReason;
    private System.Windows.Forms.TextBox textBoxReason;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
}