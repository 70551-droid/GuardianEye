namespace GuardianEye.Client;

partial class ChatNotificationForm
{
    private System.ComponentModel.IContainer components = null;
    private Label labelSender;
    private Label labelMessage;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
            _closeTimer?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        labelSender = new Label();
        labelMessage = new Label();
        SuspendLayout();

        BackColor = Color.FromArgb(20, 22, 42);

        labelSender.AutoSize = true;
        labelSender.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        labelSender.ForeColor = UIStyles.AccentBlue;
        labelSender.Location = new Point(14, 12);
        labelSender.Size = new Size(50, 19);
        labelSender.Text = "Admin";
        labelSender.BackColor = Color.Transparent;

        labelMessage.AutoSize = true;
        labelMessage.Font = new Font("Segoe UI", 9F);
        labelMessage.ForeColor = UIStyles.TextPrimary;
        labelMessage.Location = new Point(14, 36);
        labelMessage.MaximumSize = new Size(270, 70);
        labelMessage.Size = new Size(270, 60);
        labelMessage.Text = "Message";
        labelMessage.BackColor = Color.Transparent;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(300, 100);
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ChatNotificationForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Text = "";
        TopMost = true;
        Controls.AddRange(new Control[] { labelSender, labelMessage });
        ResumeLayout(false);
        PerformLayout();
    }
}
