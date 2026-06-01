namespace GuardianEye.Admin;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;
    private Guna.UI2.WinForms.Guna2Panel panelCard;
    private Guna.UI2.WinForms.Guna2TextBox textBoxUsername;
    private Guna.UI2.WinForms.Guna2TextBox textBoxPassword;
    private Guna.UI2.WinForms.Guna2Button buttonLogin;
    private Label labelError;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        panelCard = new Guna.UI2.WinForms.Guna2Panel();
        textBoxUsername = new Guna.UI2.WinForms.Guna2TextBox();
        textBoxPassword = new Guna.UI2.WinForms.Guna2TextBox();
        buttonLogin = new Guna.UI2.WinForms.Guna2Button();
        labelError = new Label();
        panelCard.SuspendLayout();
        SuspendLayout();

        panelCard.BackColor = Color.Transparent;
        panelCard.FillColor = UIStyles.GlassCard;
        panelCard.BorderColor = UIStyles.GlassBorder;
        panelCard.BorderRadius = 16;
        panelCard.BorderThickness = 1;
        panelCard.ShadowDecoration.Parent = panelCard;
        panelCard.ShadowDecoration.Shadow = new Padding(0, 0, 12, 12);
        panelCard.ShadowDecoration.BorderRadius = 16;
        panelCard.ShadowDecoration.Enabled = true;
        panelCard.ShadowDecoration.Depth = 20;
        panelCard.Size = new Size(340, 260);
        panelCard.Location = new Point(30, 30);

        var labelTitle = new Label();
        labelTitle.AutoSize = false;
        labelTitle.Size = new Size(340, 40);
        labelTitle.Location = new Point(0, 20);
        labelTitle.Text = "GuardianEye Admin";
        labelTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        labelTitle.ForeColor = UIStyles.TextPrimary;
        labelTitle.TextAlign = ContentAlignment.MiddleCenter;

        textBoxUsername.Size = new Size(290, 40);
        textBoxUsername.Location = new Point(25, 75);
        textBoxUsername.PlaceholderText = "Username";
        textBoxUsername.PasswordChar = '\0';
        UIStyles.StyleGunaInput(textBoxUsername);
        textBoxUsername.KeyPress += textBox_KeyPress;

        textBoxPassword.Size = new Size(290, 40);
        textBoxPassword.Location = new Point(25, 125);
        textBoxPassword.PlaceholderText = "Password";
        textBoxPassword.PasswordChar = '●';
        UIStyles.StyleGunaInput(textBoxPassword);
        textBoxPassword.KeyPress += textBox_KeyPress;

        buttonLogin.Size = new Size(290, 42);
        buttonLogin.Location = new Point(25, 178);
        buttonLogin.Text = "Login";
        UIStyles.StyleGunaButton(buttonLogin, UIStyles.AccentBlue);
        buttonLogin.Click += buttonLogin_Click;

        labelError.AutoSize = false;
        labelError.Size = new Size(290, 20);
        labelError.Location = new Point(25, 228);
        labelError.ForeColor = UIStyles.AccentRed;
        labelError.TextAlign = ContentAlignment.MiddleCenter;
        labelError.Visible = false;
        labelError.BackColor = Color.Transparent;

        panelCard.Controls.Add(labelTitle);
        panelCard.Controls.Add(textBoxUsername);
        panelCard.Controls.Add(textBoxPassword);
        panelCard.Controls.Add(buttonLogin);
        panelCard.Controls.Add(labelError);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(400, 320);
        FormBorderStyle = FormBorderStyle.None;
        Controls.Add(panelCard);
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GuardianEye - Admin Login";
        panelCard.ResumeLayout(false);
        ResumeLayout(false);
    }
}
