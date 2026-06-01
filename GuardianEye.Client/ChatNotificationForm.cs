namespace GuardianEye.Client;

public partial class ChatNotificationForm : Form
{
    private System.Windows.Forms.Timer _closeTimer;

    public ChatNotificationForm(string sender, string message)
    {
        InitializeComponent();
        labelSender.Text = sender;
        labelMessage.Text = message;

        _closeTimer = new System.Windows.Forms.Timer { Interval = 8000 };
        _closeTimer.Tick += (_, _) => { _closeTimer.Stop(); Close(); };
        _closeTimer.Start();

        Load += (_, _) => UIStyles.EnableRoundedCorners(Handle);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var screen = Screen.PrimaryScreen.WorkingArea;
        Location = new Point(screen.Right - Width - 10, screen.Bottom - Height - 10);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        _closeTimer?.Stop();
        Close();
    }
}
