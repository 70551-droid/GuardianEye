namespace GuardianEye.Admin;

partial class FilterConfigForm
{
    private System.ComponentModel.IContainer components = null;
    private Guna.UI2.WinForms.Guna2TextBox textBoxDomains;
    private Guna.UI2.WinForms.Guna2TextBox textBoxProcesses;
    private Guna.UI2.WinForms.Guna2Button buttonApply;
    private Guna.UI2.WinForms.Guna2Button buttonClose;

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
        textBoxDomains = new Guna.UI2.WinForms.Guna2TextBox();
        textBoxProcesses = new Guna.UI2.WinForms.Guna2TextBox();
        buttonApply = new Guna.UI2.WinForms.Guna2Button();
        buttonClose = new Guna.UI2.WinForms.Guna2Button();
        SuspendLayout();

        var labelDomains = new Label();
        labelDomains.AutoSize = true;
        labelDomains.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        labelDomains.ForeColor = UIStyles.TextPrimary;
        labelDomains.BackColor = Color.Transparent;
        labelDomains.Location = new Point(12, 12);
        labelDomains.Size = new Size(180, 19);
        labelDomains.Text = "Blocked Domains (one per line)";

        textBoxDomains.FillColor = Color.FromArgb(40, 255, 255, 255);
        textBoxDomains.BorderColor = Color.FromArgb(50, 255, 255, 255);
        textBoxDomains.FocusedState.BorderColor = UIStyles.AccentBlue;
        textBoxDomains.BorderRadius = 8;
        textBoxDomains.ForeColor = UIStyles.TextPrimary;
        textBoxDomains.PlaceholderForeColor = UIStyles.TextMuted;
        textBoxDomains.Location = new Point(12, 34);
        textBoxDomains.Multiline = true;
        textBoxDomains.Size = new Size(350, 140);
        textBoxDomains.Text = "facebook.com\nyoutube.com\ntiktok.com";
        textBoxDomains.Font = new Font("Consolas", 9F);
        textBoxDomains.ScrollBars = ScrollBars.Vertical;

        var labelProcesses = new Label();
        labelProcesses.AutoSize = true;
        labelProcesses.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        labelProcesses.ForeColor = UIStyles.TextPrimary;
        labelProcesses.BackColor = Color.Transparent;
        labelProcesses.Location = new Point(12, 185);
        labelProcesses.Size = new Size(190, 19);
        labelProcesses.Text = "Blocked Processes (one per line)";

        textBoxProcesses.FillColor = Color.FromArgb(40, 255, 255, 255);
        textBoxProcesses.BorderColor = Color.FromArgb(50, 255, 255, 255);
        textBoxProcesses.FocusedState.BorderColor = UIStyles.AccentBlue;
        textBoxProcesses.BorderRadius = 8;
        textBoxProcesses.ForeColor = UIStyles.TextPrimary;
        textBoxProcesses.PlaceholderForeColor = UIStyles.TextMuted;
        textBoxProcesses.Location = new Point(12, 207);
        textBoxProcesses.Multiline = true;
        textBoxProcesses.Size = new Size(350, 110);
        textBoxProcesses.Text = "chrome\notepad\nexplorer";
        textBoxProcesses.Font = new Font("Consolas", 9F);
        textBoxProcesses.ScrollBars = ScrollBars.Vertical;

        buttonApply.Size = new Size(110, 36);
        buttonApply.Location = new Point(12, 330);
        buttonApply.Text = "Apply Filters";
        UIStyles.StyleGunaButton(buttonApply, UIStyles.AccentBlue);
        buttonApply.Click += buttonApply_Click;

        buttonClose.Size = new Size(110, 36);
        buttonClose.Location = new Point(252, 330);
        buttonClose.Text = "Close";
        UIStyles.StyleGunaButton(buttonClose, Color.FromArgb(80, 80, 100));
        buttonClose.Click += buttonClose_Click;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(374, 380);
        Controls.AddRange(new Control[] { labelDomains, textBoxDomains, labelProcesses, textBoxProcesses, buttonApply, buttonClose });
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FilterConfigForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "GuardianEye - Filter Configuration";
        ResumeLayout(false);
        PerformLayout();
    }
}
