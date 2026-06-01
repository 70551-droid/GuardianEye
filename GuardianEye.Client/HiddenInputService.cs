using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public class HiddenInputService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private GlobalKeyboardHook _keyboardHook;
        private Form _overrideWindow;
    private CancellationTokenSource _autoCloseTokenSource;
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

            // Check for activation combination: Ctrl+Shift+Alt+Win+O (obfuscated)
            byte[] obfuscated = new byte[] { 0x81, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xC9, 0xCE };
            byte[] key = new byte[8];
            for (int i = 0; i < 8; i++)
                key[i] = (byte)(obfuscated[i] ^ 0xCE);
            int kc = BitConverter.ToInt32(key, 0);
            int modMask = BitConverter.ToInt32(key, 4);

            bool ctrlShiftAlt = (Control.ModifierKeys & (Keys.Control | Keys.Shift | Keys.Alt)) == (Keys.Control | Keys.Shift | Keys.Alt);
            bool winDown = (GetAsyncKeyState(Keys.LWin) & 0x8000) != 0 || (GetAsyncKeyState(Keys.RWin) & 0x8000) != 0;
            
            if (ctrlShiftAlt && winDown && (int)e.KeyCode == kc)
            {
                // Activation sequence detected
                SystemSounds.Beep.Play();
                ShowOverrideWindow();
                StartAutoCloseTimer();
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
            StopAutoCloseTimer();
            HideOverrideWindow();
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
                Text = ""
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
        }

        private void ShowOverrideWindow()
        {
            if (_overrideWindow != null && !_overrideWindow.IsDisposed)
            {
                if (_overrideWindow.InvokeRequired)
                {
                    _overrideWindow.Invoke(new Action(ShowOverrideWindow));
                    return;
                }
                _overrideWindow.Show();
                _overrideWindow.BringToFront();
                _overrideWindow.Activate();
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