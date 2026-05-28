namespace GuardianEye.Client;

partial class Form1
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
        this.labelUsername = new System.Windows.Forms.Label();
        this.textBoxUsername = new System.Windows.Forms.TextBox();
        this.labelPassword = new System.Windows.Forms.Label();
        this.textBoxPassword = new System.Windows.Forms.TextBox();
        this.buttonLogin = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // labelUsername
        // 
        this.labelUsername.AutoSize = true;
        this.labelUsername.Location = new System.Drawing.Point(12, 9);
        this.labelUsername.Name = "labelUsername";
        this.labelUsername.Size = new System.Drawing.Size(60, 15);
        this.labelUsername.TabIndex = 0;
        this.labelUsername.Text = "Username:";
        // 
        // textBoxUsername
        // 
        this.textBoxUsername.Location = new System.Drawing.Point(78, 6);
        this.textBoxUsername.Name = "textBoxUsername";
        this.textBoxUsername.Size = new System.Drawing.Size(200, 23);
        this.textBoxUsername.TabIndex = 1;
        // 
        // labelPassword
        // 
        this.labelPassword.AutoSize = true;
        this.labelPassword.Location = new System.Drawing.Point(12, 38);
        this.labelPassword.Name = "labelPassword";
        this.labelPassword.Size = new System.Drawing.Size(57, 15);
        this.labelPassword.TabIndex = 2;
        this.labelPassword.Text = "Password:";
        // 
        // textBoxPassword
        // 
        this.textBoxPassword.Location = new System.Drawing.Point(78, 35);
        this.textBoxPassword.Name = "textBoxPassword";
        this.textBoxPassword.PasswordChar = '*';
        this.textBoxPassword.Size = new System.Drawing.Size(200, 23);
        this.textBoxPassword.TabIndex = 3;
        // 
        // buttonLogin
        // 
        this.buttonLogin.Location = new System.Drawing.Point(78, 64);
        this.buttonLogin.Name = "buttonLogin";
        this.buttonLogin.Size = new System.Drawing.Size(75, 23);
        this.buttonLogin.TabIndex = 4;
        this.buttonLogin.Text = "Login";
        this.buttonLogin.UseVisualStyleBackColor = true;
        this.buttonLogin.Click += new System.EventHandler(this.buttonLogin_Click);
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(284, 101);
        this.Controls.Add(this.buttonLogin);
        this.Controls.Add(this.textBoxPassword);
        this.Controls.Add(this.labelPassword);
        this.Controls.Add(this.textBoxUsername);
        this.Controls.Add(this.labelUsername);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "GuardianEye - Student Login";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label labelUsername;
    private System.Windows.Forms.TextBox textBoxUsername;
    private System.Windows.Forms.Label labelPassword;
    private System.Windows.Forms.TextBox textBoxPassword;
    private System.Windows.Forms.Button buttonLogin;
}