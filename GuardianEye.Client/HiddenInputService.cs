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
        private bool _isWaitingForPassword = false;
        private string _passwordInput = "";
        private const string _password = "iamhere";
        private bool _isDisposed;
        private Action<int> _addTimeCallback;
        private Action _unlockScreenCallback;
        private Action _fiveMinuteBypassCallback;
        private string _logPath;

        public HiddenInputService(Action<int> addTimeCallback = null, Action unlockScreenCallback = null, Action fiveMinuteBypassCallback = null)
        {
            _addTimeCallback = addTimeCallback;
            _unlockScreenCallback = unlockScreenCallback;
            _fiveMinuteBypassCallback = fiveMinuteBypassCallback;
            _logPath = Path.Combine(Path.GetTempPath(), "GuardianEye_HiddenInput.log");
            try
            {
                File.WriteAllText(_logPath, $"HiddenInputService started at {DateTime.Now}{Environment.NewLine}");
            }
            catch { }
            InitializeKeyboardHook();
            CreateOverrideWindow();
        }

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(_logPath, $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
            catch { }
        }

        private void InitializeKeyboardHook()
        {
            Log("Initializing keyboard hook");
            _keyboardHook = new GlobalKeyboardHook();
            _keyboardHook.KeyDown += KeyboardHook_KeyDown;
            _keyboardHook.Hook();
            Log("Keyboard hook initialized");
        }

        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isDisposed) return;

            // Handle escape key to kill process immediately
            if (e.KeyCode == Keys.Escape)
            {
                Log("Escape pressed, disposing");
                Dispose();
                return;
            }

            // If we're waiting for password input
            if (_isWaitingForPassword)
            {
                HandlePasswordInput(e.KeyCode, (Control.ModifierKeys & Keys.Shift) != 0);
                return;
            }

            // Check for activation combination: Ctrl+Shift+Alt+* (Multiply on numeric keypad)
            bool ctrlShiftAlt = (Control.ModifierKeys & (Keys.Control | Keys.Shift | Keys.Alt)) == (Keys.Control | Keys.Shift | Keys.Alt);
            if (ctrlShiftAlt && e.KeyCode == Keys.Multiply)
            {
                // Activation sequence detected
                Log("Activation sequence detected (Ctrl+Shift+Alt+*)");
                // Debug: show a message box to confirm activation
                MessageBox.Show("Activation detected! Now type 'iamhere'.", "Hidden Input Service", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _isWaitingForPassword = true;
                _passwordInput = "";
                Log("Waiting for password input");
                // Start auto-close timer (10 seconds)
                StartAutoCloseTimer();
            }
            else
            {
                // If any other key pressed while not waiting, reset (shouldn't happen)
                // but we can ignore
                Log($"Key ignored while not waiting: {e.KeyCode}");
            }
        }

        private void HandlePasswordInput(Keys key, bool shiftPressed)
        {
            char c = '\0';
            // Only accept letters and numbers, convert to lowercase
            if (key >= Keys.A && key <= Keys.Z)
            {
                c = (char)('a' + (key - Keys.A));
            }
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                c = (char)('0' + (key - Keys.D0));
            }
            else if (key == Keys.Oemplus || key == Keys.Add) // + key
            {
                c = '+';
            }
            // Ignore other keys (including Shift alone)

            if (c != '\0')
            {
                Log($"KeyDown: {key}, shift:{shiftPressed}, char: {c}");
                _passwordInput += c;
                Log($"Password input so far: {_passwordInput}");
                
                // Check if password matches
                if (_passwordInput == _password)
                {
                    Log("Password matched, showing override window");
                    ShowOverrideWindow();
                    ResetSequence();
                }
                else if (!_password.StartsWith(_passwordInput))
                {
                    // If current input doesn't match start of password, reset
                    Log($"Input {_passwordInput} does not match start of {_password}, resetting");
                    ResetSequence();
                }
            }
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
                        Log("Auto-close timer triggered");
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

        private void ResetSequence()
        {
            _isWaitingForPassword = false;
            _passwordInput = "";
            StopAutoCloseTimer();
            HideOverrideWindow();
            Log("Sequence reset");
        }

        private void CreateOverrideWindow()
        {
            Log("Creating override window");
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
                Log("+7 button clicked");
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
                Log("Unlock button clicked");
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
                Log("5m button clicked");
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
            Log("Override window created");
        }

        private void ShowOverrideWindow()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                Log("Showing override window");
                _overrideWindow.Show();
                // Reset auto-close timer when showing
                StartAutoCloseTimer();
            }
        }

        private void HideOverrideWindow()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                Log("Hiding override window");
                _overrideWindow.Hide();
                StopAutoCloseTimer();
            }
        }

        private void HideOverrideWindowImmediately()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                Log("Hiding override window immediately");
                _overrideWindow.Hide();
                StopAutoCloseTimer();
            }
        }

        private void ExecuteAction(Action<int> actionWithInt, int value)
        {
            Log($"Executing action with value {value}");
            actionWithInt?.Invoke(value);
        }

        private void ExecuteAction(Action action)
        {
            Log("Executing action");
            action?.Invoke();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Log("Disposing HiddenInputService");
            StopAutoCloseTimer();
            _keyboardHook?.Dispose();
            _keyboardHook = null;
            
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                Log("Disposing override window");
                _overrideWindow.Close();
                _overrideWindow.Dispose();
            }
        }
    }
}