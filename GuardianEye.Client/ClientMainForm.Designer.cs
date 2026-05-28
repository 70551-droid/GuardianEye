namespace GuardianEye.Client;

partial class ClientMainForm
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
        this.labelTime = new System.Windows.Forms.Label();
        this.buttonLogout = new System.Windows.Forms.Button();
        this.buttonRequestTime = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // labelTime
        // 
        this.labelTime.AutoSize = true;
        this.labelTime.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.labelTime.Location = new System.Drawing.Point(12, 9);
        this.labelTime.Name = "labelTime";
        this.labelTime.Size = new System.Drawing.Size(102, 45);
        this.labelTime.TabIndex = 0;
        this.labelTime.Text = "20:00";
        // 
        // buttonLogout
        // 
        this.buttonLogout.Location = new System.Drawing.Point(12, 70);
        this.buttonLogout.Name = "buttonLogout";
        this.buttonLogout.Size = new System.Drawing.Size(100, 30);
        this.buttonLogout.TabIndex = 1;
        this.buttonLogout.Text = "Logout";
        this.buttonLogout.UseVisualStyleBackColor = true;
        this.buttonLogout.Click += new System.EventHandler(this.buttonLogout_Click);
        // 
        // buttonRequestTime
        // 
        this.buttonRequestTime.Location = new System.Drawing.Point(118, 70);
        this.buttonRequestTime.Name = "buttonRequestTime";
        this.buttonRequestTime.Size = new System.Drawing.Size(100, 30);
        this.buttonRequestTime.TabIndex = 2;
        this.buttonRequestTime.Text = "Request Time";
        this.buttonRequestTime.UseVisualStyleBackColor = true;
        this.buttonRequestTime.Click += new System.EventHandler(this.buttonRequestTime_Click);
        // 
        // ClientMainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(234, 112);
        this.Controls.Add(this.buttonRequestTime);
        this.Controls.Add(this.buttonLogout);
        this.Controls.Add(this.labelTime);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "ClientMainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "GuardianEye - Session";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label labelTime;
    private System.Windows.Forms.Button buttonLogout;
    private System.Windows.Forms.Button buttonRequestTime;
}