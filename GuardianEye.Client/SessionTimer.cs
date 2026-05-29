using System;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public class SessionTimer
    {
        private readonly System.Windows.Forms.Timer _timer;
        private int _remainingTimeSeconds;
        private bool _isRunning;
        public event EventHandler<int> TimeChanged; // remaining seconds
        public event EventHandler TimeExpired;

        public SessionTimer()
        {
            _timer = new System.Windows.Forms.Timer { Interval = 1000 }; // 1 second
            _timer.Tick += Timer_Tick;
        }

        public int RemainingTimeSeconds
        {
            get => _remainingTimeSeconds;
            private set
            {
                _remainingTimeSeconds = value;
                TimeChanged?.Invoke(this, _remainingTimeSeconds);
            }
        }

        public bool IsRunning => _isRunning;

        public void Start(int totalSeconds)
        {
            _remainingTimeSeconds = totalSeconds;
            _isRunning = true;
            _timer.Start();
            TimeChanged?.Invoke(this, _remainingTimeSeconds);
        }

        public void Pause()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
            }
        }

        public void Resume()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer.Start();
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_remainingTimeSeconds > 0)
            {
                _remainingTimeSeconds--;
                TimeChanged?.Invoke(this, _remainingTimeSeconds);
                if (_remainingTimeSeconds == 0)
                {
                    _timer.Stop();
                    _isRunning = false;
                    TimeExpired?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}