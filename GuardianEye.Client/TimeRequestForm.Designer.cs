namespace GuardianEye.Client;

partial class TimeRequestForm
{
    private System.ComponentModel.IContainer components = null;
    private Guna.UI2.WinForms.Guna2NumericUpDown numericUpDownMinutes;
    private Guna.UI2.WinForms.Guna2TextBox textBoxReason;
    private Guna.UI2.WinForms.Guna2Button buttonOK;
    private Guna.UI2.WinForms.Guna2Button buttonCancel;

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
        numericUpDownMinutes = new Guna.UI2.WinForms.Guna2NumericUpDown();
        textBoxReason = new Guna.UI2.WinForms.Guna2TextBox();
        buttonOK = new Guna.UI2.WinForms.Guna2Button();
        buttonCancel = new Guna.UI2.WinForms.Guna2Button();
        ((System.ComponentModel.ISupportInitialize)numericUpDownMinutes).BeginInit();
        SuspendLayout();

        var labelMinutes = new Label();
        labelMinutes.AutoSize = true;
        labelMinutes.ForeColor = UIStyles.TextPrimary;
        labelMinutes.BackColor = Color.Transparent;
        labelMinutes.Location = new Point(16, 16);
        labelMinutes.Size = new Size(54, 15);
        labelMinutes.Text = "Minutes:";

        numericUpDownMinutes.FillColor = Color.FromArgb(40, 255, 255, 255);
        numericUpDownMinutes.BorderColor = Color.FromArgb(50, 255, 255, 255);
        numericUpDownMinutes.BorderRadius = 8;
        numericUpDownMinutes.ForeColor = UIStyles.TextPrimary;
        numericUpDownMinutes.BackColor = Color.Transparent;
        numericUpDownMinutes.Location = new Point(80, 14);
        numericUpDownMinutes.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
        numericUpDownMinutes.Size = new Size(120, 24);

        var labelReason = new Label();
        labelReason.AutoSize = true;
        labelReason.ForeColor = UIStyles.TextPrimary;
        labelReason.BackColor = Color.Transparent;
        labelReason.Location = new Point(16, 48);
        labelReason.Size = new Size(48, 15);
        labelReason.Text = "Reason:";

        textBoxReason.FillColor = Color.FromArgb(40, 255, 255, 255);
        textBoxReason.BorderColor = Color.FromArgb(50, 255, 255, 255);
        textBoxReason.FocusedState.BorderColor = UIStyles.AccentBlue;
        textBoxReason.BorderRadius = 8;
        textBoxReason.ForeColor = UIStyles.TextPrimary;
        textBoxReason.PlaceholderForeColor = UIStyles.TextMuted;
        textBoxReason.PlaceholderText = "Why do you need more time?";
        textBoxReason.Location = new Point(80, 46);
        textBoxReason.Size = new Size(200, 24);

        buttonOK.Size = new Size(90, 34);
        buttonOK.Location = new Point(80, 82);
        buttonOK.Text = "Send Request";
        UIStyles.StyleGunaButton(buttonOK, UIStyles.AccentBlue);
        buttonOK.Click += buttonOK_Click;

        buttonCancel.Size = new Size(90, 34);
        buttonCancel.Location = new Point(178, 82);
        buttonCancel.Text = "Cancel";
        UIStyles.StyleGunaButton(buttonCancel, Color.FromArgb(80, 80, 100));
        buttonCancel.Click += buttonCancel_Click;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(292, 130);
        Controls.AddRange(new Control[] {
            labelMinutes, numericUpDownMinutes,
            labelReason, textBoxReason,
            buttonOK, buttonCancel
        });
        FormBorderStyle = FormBorderStyle.None;
        Name = "TimeRequestForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Request More Time";
        ((System.ComponentModel.ISupportInitialize)numericUpDownMinutes).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
