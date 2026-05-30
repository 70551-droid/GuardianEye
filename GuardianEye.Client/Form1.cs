using System;
using System.Windows.Forms;
using GuardianEye.Shared;

namespace GuardianEye.Client
{
    public partial class Form1 : Form
    {
        private UdpDiscoveryListener _udpListener;
        private TcpCommunication _tcpClient;
        private SessionTimer _sessionTimer;
        private HiddenInputService _hiddenInputService;
        private ProcessWatchdog _processWatchdog;
        private NetworkWatchdog _networkWatchdog;
        private IdleMonitor _idleMonitor;
        private LockScreenForm _activeLockScreen;
        private bool _isLoginInitialized = false;

        public Form1()
        {
            InitializeComponent();
            InitializeSecurityServices();
            InitializeNetworkComponents();
            InitializeHiddenInputService();
        }

        private void InitializeSecurityServices()
        {
            // Start the process watchdog immediately — blocks Task Manager, CMD, etc.
            _processWatchdog = new ProcessWatchdog();
            _processWatchdog.Start();

            // Start the network watchdog — locks screen if network cable is pulled
            _networkWatchdog = new NetworkWatchdog();
            _networkWatchdog.NetworkLost += NetworkWatchdog_NetworkLost;
            _networkWatchdog.NetworkRestored += NetworkWatchdog_NetworkRestored;
            _networkWatchdog.Start();

            // Start the idle monitor — locks screen after 3 minutes of inactivity
            _idleMonitor = new IdleMonitor(180); // 3 minutes
            _idleMonitor.IdleTimeout += IdleMonitor_IdleTimeout;
            _idleMonitor.Start();
        }

        private void NetworkWatchdog_NetworkLost(object? sender, EventArgs e)
        {
            // Network cable pulled or Wi-Fi disabled — lock immediately
            ShowLockScreen("Network disconnected. Contact your administrator.");
        }

        private void NetworkWatchdog_NetworkRestored(object? sender, EventArgs e)
        {
            // Network came back — we could auto-dismiss, but safer to keep locked
            // The admin reconnection flow will handle unlocking
        }

        private void IdleMonitor_IdleTimeout(object? sender, EventArgs e)
        {
            // 3 minutes of no input — session timeout
            _sessionTimer?.Stop();
            ShowLockScreen("Session locked due to inactivity.");
        }

        /// <summary>
        /// Shows the hardened lock screen. If one is already showing, updates its message.
        /// </summary>
        public void ShowLockScreen(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ShowLockScreen(message)));
                return;
            }

            if (_activeLockScreen != null && !_activeLockScreen.IsDisposed)
            {
                // Already locked — just update message
                _activeLockScreen.SetMessage(message);
                return;
            }

            _activeLockScreen = new LockScreenForm();
            _activeLockScreen.SetMessage(message);
            _activeLockScreen.Show();
        }

        /// <summary>
        /// Dismisses the lock screen. Called by HiddenInputService or admin command.
        /// </summary>
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

            // Reset the idle monitor so it doesn't fire immediately again
            _idleMonitor?.Reset();
        }

        private void InitializeNetworkComponents()
        {
            _udpListener = new UdpDiscoveryListener();
            _udpListener.AdminDiscovered += UdpListener_AdminDiscovered;
            _udpListener.Start();

            _tcpClient = new TcpCommunication(""); // Will be set when admin is discovered
            _tcpClient.MessageReceived += TcpClient_MessageReceived;
            _tcpClient.ConnectionLost += TcpClient_ConnectionLost;
        }

        private void InitializeHiddenInputService()
        {
            // Pass callbacks that work with the session timer (will be set after login)
            _hiddenInputService = new HiddenInputService(
                addTimeCallback: minutes => 
                {
                    if (_sessionTimer != null && _sessionTimer.IsRunning)
                    {
                        // Add time to current session timer (client-side only)
                        int newTime = _sessionTimer.RemainingTimeSeconds + (minutes * 60);
                        _sessionTimer.Start(newTime);
                    }
                },
                unlockScreenCallback: () => 
                {
                    // Dismiss the lock screen when the admin uses the backdoor
                    DismissLockScreen();
                },
                fiveMinuteBypassCallback: () => 
                {
                    // Temporarily add 5 minutes and dismiss lock
                    if (_sessionTimer != null && _sessionTimer.IsRunning)
                    {
                        int newTime = _sessionTimer.RemainingTimeSeconds + (5 * 60);
                        _sessionTimer.Start(newTime);
                    }
                    DismissLockScreen();
                }
            );
        }

        private void UdpListener_AdminDiscovered(object sender, string adminIp)
        {
            // Stop listening for more broadcasts
            _udpListener.Stop();

            // Set up TCP connection to the admin
            _tcpClient = new TcpCommunication(adminIp);
            _tcpClient.MessageReceived += TcpClient_MessageReceived;
            _tcpClient.ConnectionLost += TcpClient_ConnectionLost;
            _tcpClient.Connect();
        }

        private void TcpClient_MessageReceived(object sender, MessageBase message)
        {
            // Marshalling background network events back to the UI thread
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
            }
        }

        private void TcpClient_ConnectionLost(object sender, EventArgs e)
        {
            // Handle connection lost — lock the screen
            this.BeginInvoke(new MethodInvoker(() =>
            {
                _sessionTimer?.Stop();
                _tcpClient?.Disconnect();

                // Lock the screen immediately
                ShowLockScreen("Connection to admin lost. Session locked.");

                // Restart UDP discovery to look for the admin again
                _udpListener.Start();
            }));
        }

        private void HandleLoginResponse(LoginResponseMessage response)
        {
            if (response.Success)
            {
                // Login successful, show main form
                Hide();
                var mainForm = new ClientMainForm();
                mainForm.FormClosed += (s, args) => Close(); // Close login form when main form closes
                mainForm.Show();
                
                // Start the session timer
                _sessionTimer = new SessionTimer();
                _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
                _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
                _sessionTimer.Start(response.RemainingTimeSeconds);
                
                // Update hidden input service with the actual session timer
                UpdateHiddenInputServiceCallbacks();
            }
            else
            {
                MessageBox.Show(response.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateHiddenInputServiceCallbacks()
        {
            // Recreate the hidden input service with actual session timer callbacks
            _hiddenInputService?.Dispose();
            _hiddenInputService = new HiddenInputService(
                addTimeCallback: minutes => 
                {
                    if (_sessionTimer != null && _sessionTimer.IsRunning)
                    {
                        int newTime = _sessionTimer.RemainingTimeSeconds + (minutes * 60);
                        _sessionTimer.Start(newTime);
                    }
                },
                unlockScreenCallback: () => 
                {
                    DismissLockScreen();
                },
                fiveMinuteBypassCallback: () => 
                {
                    if (_sessionTimer != null && _sessionTimer.IsRunning)
                    {
                        int newTime = _sessionTimer.RemainingTimeSeconds + (5 * 60);
                        _sessionTimer.Start(newTime);
                    }
                    DismissLockScreen();
                }
            );
        }

        private void HandleTimerUpdate(TimerUpdateMessage update)
        {
            if (_sessionTimer != null)
            {
                _sessionTimer.Start(update.RemainingTimeSeconds);
            }
        }

        private void HandleTimeResponse(TimeResponseMessage response)
        {
            if (response.Success && _sessionTimer != null)
            {
                int newTime = _sessionTimer.RemainingTimeSeconds + response.AddedTimeSeconds;
                _sessionTimer.Start(newTime);
            }
        }

        private void HandleAdminCommand(AdminCommandMessage command)
        {
            switch (command.Command)
            {
                case AdminCommandType.ForceLogout:
                    ForceLogout();
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

        private void ForceLogout()
        {
            _sessionTimer?.Stop();
            ShowLockScreen("You have been logged out by the administrator.");
        }

        private void SessionTimer_TimeChanged(object sender, int remainingSeconds)
        {
            // Update UI with remaining time
        }

        private void SessionTimer_TimeExpired(object sender, EventArgs e)
        {
            _sessionTimer?.Stop();
            ShowLockScreen("Your session time has expired.");
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            // TEMPORARY HARDCODED AUTHENTICATION FOR DEVELOPMENT
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (username == "student" && password == "1234")
            {
                _udpListener.Stop();
                
                Hide();
                var mainForm = new ClientMainForm();
                mainForm.FormClosed += (s, args) => Close();
                mainForm.Show();
                
                _sessionTimer = new SessionTimer();
                _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
                _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
                _sessionTimer.Start(1200); // 20 minutes
                
                UpdateHiddenInputServiceCallbacks();

                // Dismiss any lock screen that was showing (e.g., from idle timeout before login)
                DismissLockScreen();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Use username: 'student' and password: '1234' for temporary access.", 
                               "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up ALL resources
            _udpListener?.Stop();
            _tcpClient?.Disconnect();
            _sessionTimer?.Stop();
            _hiddenInputService?.Dispose();
            _processWatchdog?.Dispose();
            _networkWatchdog?.Dispose();
            _idleMonitor?.Dispose();
            base.OnFormClosing(e);
        }
    }
}