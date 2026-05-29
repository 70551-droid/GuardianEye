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

        public Form1()
        {
            InitializeComponent();
            InitializeNetworkComponents();
        }

        private void InitializeNetworkComponents()
        {
            _udpListener = new UdpDiscoveryListener();
            _udpListener.AdminDiscovered += UdpListener_AdminDiscovered;
            _udpListener.Start();

            _tcpClient = new TcpCommunication(""); // Will be set when admin is discovered
            _tcpClient.MessageReceived += TcpClient_MessageReceived;
            _tcpClient.ConnectionLost += TcpClient_ConnectionLost;

            _sessionTimer = new SessionTimer();
            _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
            _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
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
            // Handle incoming messages from admin
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
            MessageBox.Show("Connection to admin lost.", "Connection Lost", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Try to reconnect or go back to login
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
                _sessionTimer.Start(response.RemainingTimeSeconds);
            }
            else
            {
                MessageBox.Show(response.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleTimerUpdate(TimerUpdateMessage update)
        {
            _sessionTimer.Start(update.RemainingTimeSeconds);
        }

        private void HandleTimeResponse(TimeResponseMessage response)
        {
            if (response.Success)
            {
                // Add time to the session
                _sessionTimer.Start(_sessionTimer.RemainingTimeSeconds + response.AddedTimeSeconds);
                MessageBox.Show(response.Message, "Time Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(response.Message, "Time Request Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    // Add time to session
                    _sessionTimer.Start(_sessionTimer.RemainingTimeSeconds + (command.MinutesToAdd * 60));
                    break;
                // Add other command handlers as needed
            }
        }

        private void ForceLogout()
        {
            _sessionTimer.Stop();
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
            _sessionTimer.Stop();
            var lockScreen = new LockScreenForm();
            lockScreen.SetMessage("Your session time has expired.");
            lockScreen.ShowDialog();
            // After lock screen closes, return to login
            lockScreen.Close();
            Show();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = textBoxUsername.Text.Trim();
            string password = textBoxPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Send login request to admin
            var loginRequest = new LoginRequestMessage
            {
                Username = username,
                Password = password
            };

            _tcpClient.SendMessage(loginRequest);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            _udpListener?.Stop();
            _tcpClient?.Disconnect();
            _sessionTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}