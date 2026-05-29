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
        private Action<int> _addTimeCallback;
        private Action _unlockScreenCallback;
        private Action _fiveMinuteBypassCallback;

        public HiddenInputService(Action<int> addTimeCallback = null, Action unlockScreenCallback = null, Action fiveMinuteBypassCallback = null)
        {
            _addTimeCallback = addTimeCallback;
            _unlockScreenCallback = unlockScreenCallback;
            _fiveMinuteBypassCallback = fiveMinuteBypassCallback;
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
            HideOverrideWindow();
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
                Width = 180,
                Height = 100,
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopMost = true,
                BackColor = Color.Black,
                Opacity = 0.8f,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Text = "",
                // Make it click-through when not hovering
                Region = new Region(new RectangleF(0, 0, 180, 100))
            };

            // Add buttons with minimal styling
            var btnAdd7Mins = new Button
            {
                Text = "+7",
                Width = 50,
                Height = 25,
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnAdd7Mins.FlatAppearance.BorderSize = 0;
            btnAdd7Mins.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 0, 100, 0);
            btnAdd7Mins.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 100, 0);
            btnAdd7Mins.Click += (s, e) => 
            {
                ExecuteAction(_addTimeCallback, 7);
                HideOverrideWindowImmediately();
                ResetSequence();
            };

            var btnUnlockScreen = new Button
            {
                Text = "Unlock",
                Width = 50,
                Height = 25,
                Location = new Point(115, 10),
                FlatStyle = FlatStyle.Flat
            };
            btnUnlockScreen.FlatAppearance.BorderSize = 0;
            btnUnlockScreen.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 0, 100, 0);
            btnUnlockScreen.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 100, 0);
            btnUnlockScreen.Click += (s, e) => 
            {
                ExecuteAction(_unlockScreenCallback);
                HideOverrideWindowImmediately();
                ResetSequence();
            };

            var btn5MinBypass = new Button
            {
                Text = "5m",
                Width = 50,
                Height = 25,
                Location = new Point(10, 45),
                FlatStyle = FlatStyle.Flat
            };
            btn5MinBypass.FlatAppearance.BorderSize = 0;
            btn5MinBypass.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 0, 100, 0);
            btn5MinBypass.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 0, 100, 0);
            btn5MinBypass.Click += (s, e) => 
            {
                ExecuteAction(_fiveMinuteBypassCallback);
                HideOverrideWindowImmediately();
                ResetSequence();
            };

            _overrideWindow.Controls.AddRange(new Control[] { btnAdd7Mins, btnUnlockScreen, btn5MinBypass });
            
            // Position window at bottom-right corner
            var screen = Screen.PrimaryScreen.WorkingArea;
            _overrideWindow.Location = new Point(
                screen.Right - _overrideWindow.Width - 10,
                screen.Bottom - _overrideWindow.Height - 10
            );
            
            // Make window click-through when not interacting with buttons
            _overrideWindow.MouseEnter += (s, e) => _overrideWindow.Region = new Region(new RectangleF(0, 0, 180, 100));
            _overrideWindow.MouseLeave += (s, e) => 
            {
                // Create region with holes for buttons only
                var region = new Region(new RectangleF(0, 0, 180, 100));
                region.Exclude(new RectangleF(10, 10, 50, 25)); // +7 button
                region.Exclude(new RectangleF(115, 10, 50, 25)); // Unlock button
                region.Exclude(new RectangleF(10, 45, 50, 25)); // 5m button
                _overrideWindow.Region = region;
            };
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

        private void HideOverrideWindowImmediately()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                _overrideWindow.Hide();
                StopAutoCloseTimer();
            }
        }

        private void ExecuteAction(Action<int> actionWithInt, int value)
        {
            actionWithInt?.Invoke(value);
        }

        private void ExecuteAction(Action action)
        {
            action?.Invoke();
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