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
        private bool _isLoginInitialized = false;

        public Form1()
        {
            InitializeComponent();
            InitializeNetworkComponents();
            InitializeHiddenInputService();
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
                    // This would be implemented to hide lock screen if showing
                    // For now, just a placeholder - actual implementation would depend on lock screen management
                },
                fiveMinuteBypassCallback: () => 
                {
                    // This would temporarily disable session timeout enforcement
                    // For now, just a placeholder
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
            // This was our first major fix: Marshalling background network 
            // events back to the UI thread to prevent crashes.
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
            // Handle connection lost
            this.BeginInvoke(new MethodInvoker(() =>
            {
                _sessionTimer?.Stop();
                _tcpClient?.Disconnect();

                // Restart UDP discovery to look for the admin again
                _udpListener.Start();

                // Return to the login screen if we were active
                Application.OpenForms["ClientMainForm"]?.Close();
                this.Show();
                MessageBox.Show("Connection to admin lost. Re-scanning network...", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        // Add time to current session timer (client-side only)
                        int newTime = _sessionTimer.RemainingTimeSeconds + (minutes * 60);
                        _sessionTimer.Start(newTime);
                    }
                },
                unlockScreenCallback: () => 
                {
                    // TODO: Implement actual screen unlock functionality
                    // This would hide the lock screen if it's currently showing
                },
                fiveMinuteBypassCallback: () => 
                {
                    // TODO: Implement 5-minute bypass functionality
                    // This would temporarily prevent session timeout
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
                // Add time to the session (client-side only)
                int newTime = _sessionTimer.RemainingTimeSeconds + response.AddedTimeSeconds;
                _sessionTimer.Start(newTime);
            }
        }

        private void HandleAdminCommand(AdminCommandMessage command)
        {
            // Handle admin commands like force logout, add time, etc.
            switch (command.Command)
            {
                case AdminCommandType.ForceLogout:
                    ForceLogout();
                    break;
                case AdminCommandType.AddTime:
                    // Add time to session (client-side only)
                    if (_sessionTimer != null)
                    {
                        int newTime = _sessionTimer.RemainingTimeSeconds + (command.MinutesToAdd * 60);
                        _sessionTimer.Start(newTime);
                    }
                    break;
                // Add other command handlers as needed
            }
        }

        private void ForceLogout()
        {
            _sessionTimer?.Stop();
            // Show lock screen or return to login
            var lockScreen = new LockScreenForm();
            lockScreen.SetMessage("You have been logged out by the administrator.");
            lockScreen.ShowDialog();
            // After lock screen closes, return to login
            lockScreen.Close();
            Show();
        }

        private void SessionTimer_TimeChanged(object sender, int remainingSeconds)
        {
            // Update UI with remaining time (this would be done in the main form)
            // For now, we'll just note that this event is handled
        }

        private void SessionTimer_TimeExpired(object sender, EventArgs e)
        {
            // Session time expired, show lock screen
            _sessionTimer?.Stop();
            var lockScreen = new LockScreenForm();
            lockScreen.SetMessage("Your session time has expired.");
            lockScreen.ShowDialog();
            // After lock screen closes, return to login
            lockScreen.Close();
            Show();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            // TEMPORARY HARDCODED AUTHENTICATION FOR DEVELOPMENT
            // REPLACE WITH REAL DATABASE-BASED AUTHENTICATION WHEN ADMIN SYSTEM IS READY
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (username == "student" && password == "1234")
            {
                // Simulate successful login
                // Stop UDP listener as we don't need it for this temporary auth
                _udpListener.Stop();
                
                // Hide login form and show main session form
                Hide();
                var mainForm = new ClientMainForm();
                mainForm.FormClosed += (s, args) => Close(); // Close login form when main form closes
                mainForm.Show();
                
                // Initialize temporary 20-minute session timer (20 * 60 = 1200 seconds)
                _sessionTimer = new SessionTimer();
                _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
                _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
                _sessionTimer.Start(1200);
                
                // Update hidden input service with the actual session timer
                UpdateHiddenInputServiceCallbacks();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Use username: 'student' and password: '1234' for temporary access.", 
                               "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            _udpListener?.Stop();
            _tcpClient?.Disconnect();
            _sessionTimer?.Stop();
            _hiddenInputService?.Dispose();
            base.OnFormClosing(e);
        }
    }
}