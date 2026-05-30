using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    /// <summary>
    /// Monitors network connectivity. If all network interfaces go down
    /// (student unplugged ethernet, disabled Wi-Fi, etc.), fires NetworkLost.
    /// Polls every 2 seconds.
    /// </summary>
    public class NetworkWatchdog : IDisposable
    {
        private readonly System.Windows.Forms.Timer _pollTimer;
        private bool _isDisposed;
        private bool _wasConnected = true;

        /// <summary>
        /// Fires when network connectivity is lost.
        /// </summary>
        public event EventHandler NetworkLost;

        /// <summary>
        /// Fires when network connectivity is restored.
        /// </summary>
        public event EventHandler NetworkRestored;

        public NetworkWatchdog()
        {
            _pollTimer = new System.Windows.Forms.Timer { Interval = 2000 }; // Every 2 seconds
            _pollTimer.Tick += PollTimer_Tick;
        }

        public void Start()
        {
            _wasConnected = NetworkInterface.GetIsNetworkAvailable();
            _pollTimer.Start();
        }

        public void Stop()
        {
            _pollTimer.Stop();
        }

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                bool isConnected = NetworkInterface.GetIsNetworkAvailable();

                if (_wasConnected && !isConnected)
                {
                    // Network just went down
                    _wasConnected = false;
                    NetworkLost?.Invoke(this, EventArgs.Empty);
                }
                else if (!_wasConnected && isConnected)
                {
                    // Network came back
                    _wasConnected = true;
                    NetworkRestored?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                // If we can't even check, assume network is down
                if (_wasConnected)
                {
                    _wasConnected = false;
                    NetworkLost?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _pollTimer.Stop();
            _pollTimer.Dispose();
        }
    }
}
