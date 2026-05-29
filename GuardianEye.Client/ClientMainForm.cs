using System;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public partial class ClientMainForm : Form
    {
        private HiddenInputService _hiddenInputService;
        private SessionTimer _sessionTimer;

        public ClientMainForm()
        {
            InitializeComponent();
            // Initialize a default session timer for demonstration
            // In real implementation, this would come from login response
            _sessionTimer = new SessionTimer();
            _sessionTimer.TimeChanged += SessionTimer_TimeChanged;
            _sessionTimer.TimeExpired += SessionTimer_TimeExpired;
            _sessionTimer.Start(20 * 60); // 20 minutes default
            
            InitializeHiddenInputService();
        }

        private void InitializeHiddenInputService()
        {
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
                    // For example, we could extend the session timer by 5 minutes
                    // or set a flag to ignore timeout for 5 minutes
                    if (_sessionTimer != null && _sessionTimer.IsRunning)
                    {
                        // Add 5 minutes to current session as a simple bypass
                        int newTime = _sessionTimer.RemainingTimeSeconds + (5 * 60);
                        _sessionTimer.Start(newTime);
                    }
                }
            );
        }

        public void UpdateTimeDisplay(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            labelTime.Text = $"{minutes:D2}:{seconds:D2}";
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            // TODO: Implement logout logic
            MessageBox.Show("Logout clicked");
        }

        private void buttonRequestTime_Click(object sender, EventArgs e)
        {
            // TODO: Implement request time logic
            MessageBox.Show("Request time clicked");
        }

        private void SessionTimer_TimeChanged(object sender, int remainingSeconds)
        {
            UpdateTimeDisplay(remainingSeconds);
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
            // In a real app, we'd navigate back to login, but for now just close
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            _sessionTimer?.Stop();
            _hiddenInputService?.Dispose();
            base.OnFormClosing(e);
        }
    }
}