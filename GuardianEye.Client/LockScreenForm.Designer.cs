namespace GuardianEye.Client;

partial class LockScreenForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
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
        this.labelMessage = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // labelMessage
        // 
        this.labelMessage.AutoSize = true;
        this.labelMessage.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.labelMessage.ForeColor = System.Drawing.Color.White;
        this.labelMessage.Location = new System.Drawing.Point(12, 9);
        this.labelMessage.Name = "labelMessage";
        this.labelMessage.Size = new System.Drawing.Size(398, 45);
        this.labelMessage.TabIndex = 0;
        this.labelMessage.Text = "Session Locked - Time Expired";
        // 
        // LockScreenForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.Black;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.labelMessage);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "LockScreenForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
        this.Text = "LockScreenForm";
        this.TopMost = true;
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label labelMessage;
}