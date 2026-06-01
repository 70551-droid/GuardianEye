using GuardianEye.Shared;

namespace GuardianEye.Client;

public partial class ClientMainForm : Form
{
    private SessionTimer _sessionTimer;
    private HiddenInputService _hiddenInputService;
    private TcpCommunication _tcpClient;
    private int _maxTime;

    public ClientMainForm(int initialTimeSeconds, SessionTimer existingTimer)
    {
        InitializeComponent();
        _maxTime = Math.Max(initialTimeSeconds, 1);
        UpdateTimeDisplay(initialTimeSeconds);

        if (existingTimer != null)
        {
            _sessionTimer = existingTimer;
            _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
            _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
        }

        if (Owner is Form1 form1)
        {
            var field = typeof(Form1).GetField("_tcpClient",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _tcpClient = field?.GetValue(form1) as TcpCommunication;
        }

        InitializeHiddenInputService();

        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
        };
    }

    private void InitializeHiddenInputService()
    {
        _hiddenInputService = new HiddenInputService(
            addTimeCallback: minutes =>
            {
                if (_sessionTimer != null && _sessionTimer.IsRunning)
                {
                    int newTime = _sessionTimer.RemainingTimeSeconds + (minutes * 60);
                    _sessionTimer.Start(newTime);
                }
            },
            unlockScreenCallback: () => { },
            fiveMinuteBypassCallback: () =>
            {
                if (_sessionTimer != null && _sessionTimer.IsRunning)
                {
                    int newTime = _sessionTimer.RemainingTimeSeconds + (5 * 60);
                    _sessionTimer.Start(newTime);
                }
            }
        );
    }

    public void UpdateTimeDisplay(int totalSeconds)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => UpdateTimeDisplay(totalSeconds)));
            return;
        }
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        labelTime.Text = $"{minutes:D2}:{seconds:D2}";
        _maxTime = Math.Max(_maxTime, totalSeconds);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int remaining = _sessionTimer?.RemainingTimeSeconds ?? 0;
        float pct = _maxTime > 0 ? Math.Clamp((float)remaining / _maxTime, 0, 1) : 0;

        int cx = ClientSize.Width / 2;
        int cy = 62;
        int r = 90;
        int thickness = 6;

        Color arcColor;
        if (pct > 0.5f) arcColor = Color.FromArgb(105, 240, 174);
        else if (pct > 0.25f) arcColor = Color.FromArgb(255, 213, 79);
        else arcColor = Color.FromArgb(255, 107, 107);

        using var pen = new Pen(Color.FromArgb(40, 255, 255, 255), thickness);
        e.Graphics.DrawArc(pen, cx - r, cy - r, r * 2, r * 2, 0, 360);

        using var arcPen = new Pen(arcColor, thickness);
        arcPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
        arcPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        float sweep = 360f * pct;
        e.Graphics.DrawArc(arcPen, cx - r, cy - r, r * 2, r * 2, 270, sweep);

        using var glowPen = new Pen(Color.FromArgb(60, arcColor), thickness + 4);
        e.Graphics.DrawArc(glowPen, cx - r, cy - r, r * 2, r * 2, 270, sweep);
    }

    private void buttonLogout_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result == DialogResult.Yes)
        {
            _sessionTimer?.Stop();
            if (_tcpClient?.IsConnected == true)
            {
                _tcpClient.SendMessage(new ClientStatusMessage
                {
                    SessionId = Guid.Empty,
                    Status = ClientStatusType.Offline,
                    RemainingTimeSeconds = _sessionTimer != null ? _sessionTimer.RemainingTimeSeconds : 0
                });
            }
            _hiddenInputService?.Dispose();
            Close();
        }
    }

    private void buttonRequestTime_Click(object sender, EventArgs e)
    {
        using var requestForm = new TimeRequestForm();
        if (requestForm.ShowDialog(this) == DialogResult.OK)
        {
            if (_tcpClient?.IsConnected == true)
            {
                _tcpClient.SendMessage(new TimeRequestMessage
                {
                    SessionId = Guid.Empty,
                    RequestedMinutes = requestForm.RequestedMinutes,
                    Reason = requestForm.Reason
                });
                MessageBox.Show("Time request sent to administrator.", "Request Sent",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (_sessionTimer != null && _sessionTimer.IsRunning)
                {
                    int newTime = _sessionTimer.RemainingTimeSeconds + (requestForm.RequestedMinutes * 60);
                    _sessionTimer.Start(newTime);
                    MessageBox.Show($"Added {requestForm.RequestedMinutes} minutes locally (offline mode).",
                        "Time Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }

    private void SessionTimer_TimeChanged(object? sender, int remainingSeconds)
    {
        UpdateTimeDisplay(remainingSeconds);
    }

    private void SessionTimer_TimeExpired(object? sender, EventArgs e)
    {
        _sessionTimer?.Stop();
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _sessionTimer?.Stop();
        _hiddenInputService?.Dispose();
        base.OnFormClosing(e);
    }
}
