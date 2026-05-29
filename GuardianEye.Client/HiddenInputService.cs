using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public class HiddenInputService : IDisposable
    {
        private GlobalKeyboardHook _keyboardHook;
        private Form _overrideWindow;
        private CancellationTokenSource _autoCloseTokenSource;
        private readonly List<Keys> _keyBuffer = new List<Keys>();
        private readonly Keys[] _activationSequence = { Keys.ControlKey, Keys.ShiftKey, Keys.Menu, Keys.Multiply }; // Ctrl+Shift+Alt+*
        private int _sequenceIndex = 0;
        private bool _isWaitingForPassword = false;
        private string _passwordInput = "";
        private const string _password = "iamhere";
        private bool _isDisposed;

        public HiddenInputService()
        {
            InitializeKeyboardHook();
            CreateOverrideWindow();
        }

        private void InitializeKeyboardHook()
        {
            _keyboardHook = new GlobalKeyboardHook();
            _keyboardHook.KeyDown += KeyboardHook_KeyDown;
            _keyboardHook.Hook();
        }

        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isDisposed) return;

            // Handle escape key to kill process immediately
            if (e.KeyCode == Keys.Escape)
            {
                Dispose();
                return;
            }

            // If we're waiting for password input
            if (_isWaitingForPassword)
            {
                HandlePasswordInput(e.KeyCode);
                return;
            }

            // Check for activation sequence
            if (IsActivationKey(e.KeyCode))
            {
                // Add to buffer if it's part of our sequence and in order
                if (_sequenceIndex < _activationSequence.Length && 
                    e.KeyCode == _activationSequence[_sequenceIndex])
                {
                    _sequenceIndex++;
                    
                    // If we've completed the sequence
                    if (_sequenceIndex >= _activationSequence.Length)
                    {
                        _sequenceIndex = 0;
                        _isWaitingForPassword = true;
                        _passwordInput = "";
                        // Start auto-close timer (10 seconds)
                        StartAutoCloseTimer();
                    }
                }
                else
                {
                    // Reset sequence if wrong key pressed
                    ResetSequence();
                }
            }
            else
            {
                // Reset sequence if non-activation key pressed
                ResetSequence();
            }
        }

        private bool IsActivationKey(Keys key)
        {
            return _activationSequence.Contains(key);
        }

        private void ResetSequence()
        {
            _sequenceIndex = 0;
            _isWaitingForPassword = false;
            _passwordInput = "";
            StopAutoCloseTimer();
        }

        private void HandlePasswordInput(Keys key)
        {
            // Convert key to character if possible
            char c = KeyToChar(key);
            if (c != '\0')
            {
                _passwordInput += c;
                
                // Check if password matches
                if (_passwordInput == _password)
                {
                    ShowOverrideWindow();
                    ResetSequence();
                }
                else if (!_password.StartsWith(_passwordInput))
                {
                    // If current input doesn't match start of password, reset
                    ResetSequence();
                }
            }
        }

        private char KeyToChar(Keys key)
        {
            // Only handle printable characters
            if (key >= Keys.A && key <= Keys.Z)
                return (char)('a' + (key - Keys.A));
            if (key >= Keys.D0 && key <= Keys.D9)
                return (char)('0' + (key - Keys.D0));
            if (key == Keys.Oemplus || key == Keys.Add) // + key
                return '+';
            
            return '\0';
        }

        private void StartAutoCloseTimer()
        {
            StopAutoCloseTimer();
            _autoCloseTokenSource = new CancellationTokenSource();
            Task.Delay(10000, _autoCloseTokenSource.Token)
                .ContinueWith(t => 
                {
                    if (!t.IsCanceled && !_isDisposed)
                    {
                        HideOverrideWindow();
                        ResetSequence();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void StopAutoCloseTimer()
        {
            _autoCloseTokenSource?.Cancel();
            _autoCloseTokenSource = null;
        }

        private void CreateOverrideWindow()
        {
            _overrideWindow = new Form
            {
                Width = 200,
                Height = 120,
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Text = "GuardianEye Override"
            };

            // Add buttons
            var btnAdd7Mins = new Button
            {
                Text = "+7 mins",
                Width = 80,
                Height = 30,
                Location = new Point(10, 10)
            };
            btnAdd7Mins.Click += (s, e) => 
            {
                AddTime(7);
                HideOverrideWindow();
                ResetSequence();
            };

            var btnUnlockScreen = new Button
            {
                Text = "Unlock Screen",
                Width = 80,
                Height = 30,
                Location = new Point(100, 10)
            };
            btnUnlockScreen.Click += (s, e) => 
            {
                UnlockScreen();
                HideOverrideWindow();
                ResetSequence();
            };

            var btn5MinBypass = new Button
            {
                Text = "5 min bypass",
                Width = 80,
                Height = 30,
                Location = new Point(10, 50)
            };
            btn5MinBypass.Click += (s, e) => 
            {
                FiveMinuteBypass();
                HideOverrideWindow();
                ResetSequence();
            };

            _overrideWindow.Controls.AddRange(new Control[] { btnAdd7Mins, btnUnlockScreen, btn5MinBypass });
            
            // Position window at bottom-right corner
            var screen = Screen.PrimaryScreen.WorkingArea;
            _overrideWindow.Location = new Point(
                screen.Right - _overrideWindow.Width - 10,
                screen.Bottom - _overrideWindow.Height - 10
            );
        }

        private void ShowOverrideWindow()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                _overrideWindow.Show();
                // Reset auto-close timer when showing
                StartAutoCloseTimer();
            }
        }

        private void HideOverrideWindow()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                _overrideWindow.Hide();
                StopAutoCloseTimer();
            }
        }

        private void AddTime(int minutes)
        {
            // This would normally communicate with the session timer
            // For now, we'll just show a confirmation
            MessageBox.Show($"{minutes} minutes added to session.", "Time Added", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Implement actual time addition to session timer
        }

        private void UnlockScreen()
        {
            // This would unlock the lock screen if active
            MessageBox.Show("Screen unlocked.", "Unlock Screen", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Implement actual screen unlock functionality
        }

        private void FiveMinuteBypass()
        {
            // This would bypass the timer for 5 minutes
            MessageBox.Show("5-minute bypass activated.", "Bypass Activated", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Implement actual bypass functionality
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            
            StopAutoCloseTimer();
            _keyboardHook?.Dispose();
            _keyboardHook = null;
            
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                _overrideWindow.Close();
                _overrideWindow.Dispose();
            }
        }
    }
}