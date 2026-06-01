using System.Diagnostics;
using System.Net.Sockets;
using GuardianEye.Shared;

namespace GuardianEye.Client;

public partial class Form1 : Form
{
    private UdpDiscoveryListener _udpListener;
    private TcpCommunication _tcpClient;
    private SessionTimer _sessionTimer;
    private ProcessWatchdog _processWatchdog;
    private NetworkWatchdog _networkWatchdog;
    private IdleMonitor _idleMonitor;
    private LockScreenForm _activeLockScreen;
    private ClientMainForm _mainForm;
    private WebsiteFilterService _websiteFilter;
    private BrowserRestrictionService _browserRestriction;
    private ForegroundMonitorService _foregroundMonitor;
    private FileStream _selfFileLock;

    public Form1()
    {
        InitializeComponent();
        _sessionTimer = new SessionTimer();
        _sessionTimer.FiveMinuteWarning += (_, _) => MessageBox.Show("Only 5 minutes remaining!", "Time Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        _sessionTimer.TwoMinuteWarning += (_, _) => MessageBox.Show("Only 2 minutes remaining!", "Time Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        InitializeSecurityServices();
        InitializeNetworkComponents();

        _websiteFilter = new WebsiteFilterService();
        _browserRestriction = new BrowserRestrictionService();
        _browserRestriction.Start();
        _foregroundMonitor = new ForegroundMonitorService(_tcpClient, "");

        LockOwnExe();
        ShowLockScreen("System locked. Login to begin your session.");

        Load += (_, _) => UIStyles.EnableRoundedCorners(Handle);
    }

    private void LockOwnExe()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            _selfFileLock = new FileStream(exePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch { }
    }

    private void InitializeSecurityServices()
    {
        _processWatchdog = new ProcessWatchdog();
        _processWatchdog.Start();
        _processWatchdog.SetTcpClient(_tcpClient, "");

        _networkWatchdog = new NetworkWatchdog();
        _networkWatchdog.NetworkLost += NetworkWatchdog_NetworkLost;
        _networkWatchdog.NetworkRestored += NetworkWatchdog_NetworkRestored;
        _networkWatchdog.Start();

        _idleMonitor = new IdleMonitor(180);
        _idleMonitor.IdleTimeout += IdleMonitor_IdleTimeout;
        _idleMonitor.Start();
    }

    private void NetworkWatchdog_NetworkLost(object? sender, EventArgs e)
    {
        EndSession("Network disconnected. Contact your administrator.");
    }

    private void NetworkWatchdog_NetworkRestored(object? sender, EventArgs e)
    {
    }

    private void IdleMonitor_IdleTimeout(object? sender, EventArgs e)
    {
        _sessionTimer?.Stop();
        EndSession("Session locked due to inactivity.");
    }

    public void EndSession(string message)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => EndSession(message)));
            return;
        }

        _mainForm?.Close();
        _mainForm = null;
        _sessionTimer?.Stop();
        Show();
        ShowLockScreen(message);
    }

    public void ShowLockScreen(string message)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => ShowLockScreen(message)));
            return;
        }

        if (_activeLockScreen != null && !_activeLockScreen.IsDisposed)
        {
            _activeLockScreen.SetMessage(message);
            return;
        }

        _activeLockScreen = new LockScreenForm();
        _activeLockScreen.SetMessage(message);
        _activeLockScreen.Show();
        _activeLockScreen.BringToFront();
    }

    public void DismissLockScreen()
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(DismissLockScreen));
            return;
        }

        if (_activeLockScreen != null && !_activeLockScreen.IsDisposed)
        {
            _activeLockScreen.UnlockAndClose();
            _activeLockScreen = null;
        }

        _idleMonitor?.Reset();
    }

    private void InitializeNetworkComponents()
    {
        _udpListener = new UdpDiscoveryListener();
        _udpListener.AdminDiscovered += UdpListener_AdminDiscovered;
        _udpListener.Start();

        _tcpClient = new TcpCommunication("");
        _tcpClient.MessageReceived += TcpClient_MessageReceived;
        _tcpClient.ConnectionLost += TcpClient_ConnectionLost;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep(2000);
            _foregroundMonitor?.Start();
        });
    }

    private void HandleChatMessage(ChatMessage chat)
    {
        var notif = new ChatNotificationForm(chat.Sender, chat.Message);
        notif.Show();
    }

    private void UdpListener_AdminDiscovered(object? sender, string adminIp)
    {
        _udpListener.Stop();
        _tcpClient = new TcpCommunication(adminIp);
        _tcpClient.MessageReceived += TcpClient_MessageReceived;
        _tcpClient.ConnectionLost += TcpClient_ConnectionLost;
        _tcpClient.Connect();
        _browserRestriction.SetTcpClient(_tcpClient, "");
        _processWatchdog.SetTcpClient(_tcpClient, "");
        _foregroundMonitor.SetTcpClient(_tcpClient, "");
    }

    private void TcpClient_MessageReceived(object? sender, MessageBase message)
    {
        if (this.InvokeRequired)
        {
            this.BeginInvoke(new Action(() => TcpClient_MessageReceived(sender, message)));
            return;
        }

        switch (message.Type)
        {
            case MessageType.LoginResponse:
                HandleLoginResponse((LoginResponseMessage)message);
                break;
            case MessageType.TimerUpdate:
                HandleTimerUpdate((TimerUpdateMessage)message);
                break;
            case MessageType.TimeResponse:
                HandleTimeResponse((TimeResponseMessage)message);
                break;
            case MessageType.AdminCommand:
                HandleAdminCommand((AdminCommandMessage)message);
                break;
            case MessageType.FilterUpdate:
                HandleFilterUpdate((FilterUpdateMessage)message);
                break;
            case MessageType.Chat:
                HandleChatMessage((ChatMessage)message);
                break;
        }
    }

    private void TcpClient_ConnectionLost(object? sender, EventArgs e)
    {
        this.BeginInvoke(new MethodInvoker(() =>
        {
            _sessionTimer?.Stop();
            _tcpClient?.Disconnect();
            EndSession("Connection to admin lost. Session locked.");
            _udpListener.Start();
        }));
    }

    private void HandleLoginResponse(LoginResponseMessage response)
    {
        if (response.Success)
        {
            _browserRestriction.SetTcpClient(_tcpClient, response.StudentName);
            _processWatchdog.SetTcpClient(_tcpClient, response.StudentName);
            _foregroundMonitor.SetTcpClient(_tcpClient, response.StudentName);
            Hide();
            _mainForm = new ClientMainForm(response.RemainingTimeSeconds, _sessionTimer);
            _mainForm.FormClosed += (s, args) =>
            {
                Show();
                ShowLockScreen("Session ended. Login again to continue.");
            };
            _mainForm.Show();

            _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
            _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
            _sessionTimer.Start(response.RemainingTimeSeconds);

            DismissLockScreen();
        }
        else
        {
            MessageBox.Show(response.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleTimerUpdate(TimerUpdateMessage update)
    {
        if (_sessionTimer != null)
            _sessionTimer.Start(update.RemainingTimeSeconds);
    }

    private void HandleTimeResponse(TimeResponseMessage response)
    {
        if (response.Success && _sessionTimer != null)
        {
            int newTime = _sessionTimer.RemainingTimeSeconds + response.AddedTimeSeconds;
            _sessionTimer.Start(newTime);
        }
    }

    private void HandleFilterUpdate(FilterUpdateMessage update)
    {
        _websiteFilter?.UpdateBlockedDomains(update.BlockedDomains);
        _processWatchdog?.UpdateDenylist(update.BlockedProcesses);
    }

    private void HandleAdminCommand(AdminCommandMessage command)
    {
        switch (command.Command)
        {
            case AdminCommandType.ForceLogout:
                EndSession("You have been logged out by the administrator.");
                break;
            case AdminCommandType.AddTime:
                if (_sessionTimer != null)
                {
                    int newTime = _sessionTimer.RemainingTimeSeconds + (command.MinutesToAdd * 60);
                    _sessionTimer.Start(newTime);
                }
                break;
        }
    }

    private void SessionTimer_TimeChanged(object? sender, int remainingSeconds)
    {
        _mainForm?.UpdateTimeDisplay(remainingSeconds);
    }

    private void SessionTimer_TimeExpired(object? sender, EventArgs e)
    {
        _sessionTimer?.Stop();
        EndSession("Your session time has expired.");
    }

    private async void buttonLogin_Click(object sender, EventArgs e)
    {
        string username = textBoxUsername.Text.Trim();
        string password = textBoxPassword.Text.Trim();

        if (_tcpClient != null && _tcpClient.IsConnected)
        {
            var tcs = new TaskCompletionSource<LoginResponseMessage>();
            EventHandler<MessageBase> handler = null;
            handler = (s, msg) =>
            {
                if (msg.Type == MessageType.LoginResponse)
                    tcs.TrySetResult((LoginResponseMessage)msg);
            };

            _tcpClient.MessageReceived += handler;
            _tcpClient.SendMessage(new LoginRequestMessage { Username = username, Password = password });

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
            _tcpClient.MessageReceived -= handler;

            if (completed == tcs.Task && tcs.Task.Result.Success)
            {
                HandleLoginResponse(tcs.Task.Result);
                return;
            }
        }

        if (username == "student" && password == "1234")
        {
            _udpListener.Stop();
            _browserRestriction.SetTcpClient(_tcpClient, "student");
            _processWatchdog.SetTcpClient(_tcpClient, "student");
            _foregroundMonitor.SetTcpClient(_tcpClient, "student");

            Hide();
            _sessionTimer = new SessionTimer();
            _sessionTimer.FiveMinuteWarning += (_, args) => MessageBox.Show("Only 5 minutes remaining!", "Time Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sessionTimer.TwoMinuteWarning += (_, args) => MessageBox.Show("Only 2 minutes remaining!", "Time Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
            _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
            _sessionTimer.Start(1200);

            _mainForm = new ClientMainForm(1200, _sessionTimer);
            _mainForm.FormClosed += (s, args) =>
            {
                Show();
                ShowLockScreen("Session ended. Login again to continue.");
            };
            _mainForm.Show();

            DismissLockScreen();
        }
        else
        {
            MessageBox.Show("Invalid credentials. Use username: 'student' and password: '1234' for local access.",
                           "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
        {
            _selfFileLock?.Dispose();
            _udpListener?.Stop();
            _tcpClient?.Disconnect();
            _sessionTimer?.Stop();
            _processWatchdog?.Dispose();
            _networkWatchdog?.Dispose();
            _idleMonitor?.Dispose();
            _websiteFilter?.Dispose();
            _browserRestriction?.Dispose();
            _foregroundMonitor?.Dispose();
            base.OnFormClosing(e);
            return;
        }

        e.Cancel = true;
        if (_activeLockScreen == null || _activeLockScreen.IsDisposed)
            ShowLockScreen("GuardianEye cannot be closed.");
    }
}
