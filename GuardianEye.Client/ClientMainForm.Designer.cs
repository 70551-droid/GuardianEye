namespace GuardianEye.Client;

partial class ClientMainForm
{
    private System.ComponentModel.IContainer components = null;
    internal Label labelTime;
    private Guna.UI2.WinForms.Guna2Button buttonLogout;
    private Guna.UI2.WinForms.Guna2Button buttonRequestTime;

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
        labelTime = new Label();
        buttonLogout = new Guna.UI2.WinForms.Guna2Button();
        buttonRequestTime = new Guna.UI2.WinForms.Guna2Button();
        SuspendLayout();

        labelTime.AutoSize = false;
        labelTime.Size = new Size(200, 80);
        labelTime.Location = new Point(0, 24);
        labelTime.Text = "20:00";
        labelTime.Font = new Font("Segoe UI", 56F, FontStyle.Bold);
        labelTime.ForeColor = UIStyles.TextPrimary;
        labelTime.TextAlign = ContentAlignment.MiddleCenter;
        labelTime.BackColor = Color.Transparent;
        labelTime.Anchor = AnchorStyles.None;

        var labelRemaining = new Label();
        labelRemaining.AutoSize = false;
        labelRemaining.Size = new Size(200, 20);
        labelRemaining.Location = new Point(0, 104);
        labelRemaining.Text = "remaining";
        labelRemaining.Font = new Font("Segoe UI", 10F);
        labelRemaining.ForeColor = UIStyles.TextMuted;
        labelRemaining.TextAlign = ContentAlignment.MiddleCenter;
        labelRemaining.BackColor = Color.Transparent;
        labelRemaining.Anchor = AnchorStyles.None;

        buttonLogout.Size = new Size(120, 38);
        buttonLogout.Location = new Point(16, 148);
        buttonLogout.Text = "Logout";
        UIStyles.StyleGunaButton(buttonLogout, UIStyles.AccentRed);

        buttonRequestTime.Size = new Size(120, 38);
        buttonRequestTime.Location = new Point(144, 148);
        buttonRequestTime.Text = "Request Time";
        UIStyles.StyleGunaButton(buttonRequestTime, UIStyles.AccentBlue);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(280, 200);
        Controls.AddRange(new Control[] { labelTime, labelRemaining, buttonLogout, buttonRequestTime });
        FormBorderStyle = FormBorderStyle.None;
        Name = "ClientMainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GuardianEye - Session";
        ResumeLayout(false);
    }
}
