using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    /// <summary>
    /// Monitors system-wide keyboard and mouse activity using Win32 GetLastInputInfo.
    /// Fires IdleTimeout when no input is detected for the configured duration.
    /// </summary>
    public class IdleMonitor : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private readonly System.Windows.Forms.Timer _pollTimer;
        private readonly int _idleThresholdMs;
        private bool _isDisposed;
        private bool _hasFiredIdle;

        /// <summary>
        /// Fires when the system has been idle for the configured threshold.
        /// Only fires once until input is detected again.
        /// </summary>
        public event EventHandler IdleTimeout;

        /// <summary>
        /// Creates an IdleMonitor.
        /// </summary>
        /// <param name="idleThresholdSeconds">Seconds of inactivity before firing IdleTimeout. Default: 180 (3 minutes).</param>
        public IdleMonitor(int idleThresholdSeconds = 180)
        {
            _idleThresholdMs = idleThresholdSeconds * 1000;
            _pollTimer = new System.Windows.Forms.Timer { Interval = 5000 }; // Poll every 5 seconds
            _pollTimer.Tick += PollTimer_Tick;
        }

        public void Start()
        {
            _hasFiredIdle = false;
            _pollTimer.Start();
        }

        public void Stop()
        {
            _pollTimer.Stop();
        }

        /// <summary>
        /// Reset the idle fire flag so it can fire again after being handled.
        /// </summary>
        public void Reset()
        {
            _hasFiredIdle = false;
        }

        private void PollTimer_Tick(object? sender, EventArgs e)
        {
            int idleMs = GetIdleTimeMs();

            if (idleMs >= _idleThresholdMs && !_hasFiredIdle)
            {
                // Before firing idle, check if audio is playing (e.g., watching a video)
                // If audio is active, the student is still using the PC — don't lock
                if (AudioDetector.IsAudioPlaying())
                    return;

                _hasFiredIdle = true;
                IdleTimeout?.Invoke(this, EventArgs.Empty);
            }
            else if (idleMs < _idleThresholdMs && _hasFiredIdle)
            {
                // User came back, reset the flag so it can fire again next time
                _hasFiredIdle = false;
            }
        }

        /// <summary>
        /// Returns the number of milliseconds since the last keyboard or mouse input.
        /// </summary>
        public static int GetIdleTimeMs()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

            if (GetLastInputInfo(ref lii))
            {
                uint tickCount = (uint)Environment.TickCount;
                return (int)(tickCount - lii.dwTime);
            }

            return 0; // Fail safe: assume not idle
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
