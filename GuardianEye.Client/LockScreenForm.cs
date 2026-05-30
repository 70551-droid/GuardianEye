using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    /// <summary>
    /// A hardened, full-screen lock screen that covers ALL monitors.
    /// Blocks keyboard shortcuts, traps the mouse, and cannot be closed by the student.
    /// Only the HiddenInputService unlock callback or an admin command can dismiss it.
    /// </summary>
    public partial class LockScreenForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect); // Pass null to release

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private GlobalKeyboardHook _lockKeyboardHook;
        private List<Form> _secondaryOverlays = new List<Form>();
        private bool _allowClose = false;

        public LockScreenForm()
        {
            InitializeComponent();
            SetupLockScreen();
        }

        private void SetupLockScreen()
        {
            // Make this form an inescapable full-screen overlay on the primary monitor
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(15, 15, 15);
            this.StartPosition = FormStartPosition.Manual;

            // Cover the primary screen
            Screen primary = Screen.PrimaryScreen;
            this.Bounds = primary.Bounds;

            // Center the message label
            labelMessage.AutoSize = false;
            labelMessage.TextAlign = ContentAlignment.MiddleCenter;
            labelMessage.Dock = DockStyle.Fill;
            labelMessage.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            labelMessage.ForeColor = Color.FromArgb(220, 50, 50);

            // Set up keyboard hook in lockdown mode
            _lockKeyboardHook = new GlobalKeyboardHook();
            _lockKeyboardHook.IsLockdownActive = true;
            _lockKeyboardHook.Hook();
        }

        public void SetMessage(string message)
        {
            labelMessage.Text = message;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Cover ALL secondary monitors with black overlay forms
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Primary) continue; // Primary is already covered by this form

                var overlay = new Form
                {
                    FormBorderStyle = FormBorderStyle.None,
                    WindowState = FormWindowState.Normal,
                    TopMost = true,
                    ShowInTaskbar = false,
                    BackColor = Color.FromArgb(15, 15, 15),
                    StartPosition = FormStartPosition.Manual,
                    Bounds = screen.Bounds
                };

                // Add a matching message label
                var lbl = new Label
                {
                    Text = labelMessage.Text,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(220, 50, 50),
                    BackColor = Color.FromArgb(15, 15, 15)
                };
                overlay.Controls.Add(lbl);
                overlay.Show();
                _secondaryOverlays.Add(overlay);
            }

            // Trap the mouse cursor inside the primary screen
            TrapMouse();

            // Force focus so student can't click behind
            this.BringToFront();
            this.Activate();
            this.Focus();
        }

        private void TrapMouse()
        {
            Screen primary = Screen.PrimaryScreen;
            RECT rect = new RECT
            {
                Left = primary.Bounds.Left,
                Top = primary.Bounds.Top,
                Right = primary.Bounds.Right,
                Bottom = primary.Bounds.Bottom
            };
            ClipCursor(ref rect);
        }

        private void ReleaseMouse()
        {
            ClipCursor(IntPtr.Zero);
        }

        /// <summary>
        /// The ONLY way to close this lock screen. Must be called by the admin 
        /// unlock command or the HiddenInputService.
        /// </summary>
        public void UnlockAndClose()
        {
            _allowClose = true;
            ReleaseMouse();

            // Close secondary overlays
            foreach (var overlay in _secondaryOverlays)
            {
                if (overlay != null && !overlay.IsDisposed)
                {
                    overlay.Close();
                    overlay.Dispose();
                }
            }
            _secondaryOverlays.Clear();

            // Unhook keyboard
            _lockKeyboardHook?.Dispose();
            _lockKeyboardHook = null;

            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Block ALL close attempts unless we explicitly allowed it
            if (!_allowClose)
            {
                e.Cancel = true;
                return;
            }

            ReleaseMouse();
            _lockKeyboardHook?.Dispose();
            base.OnFormClosing(e);
        }

        // Prevent Alt+F4 from even reaching the form
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Block Alt+F4
            if (keyData == (Keys.Alt | Keys.F4))
                return true; // Swallow it

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // Prevent the form from being deactivated (clicked behind)
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            // Immediately reclaim focus
            if (!_allowClose && !this.IsDisposed)
            {
                this.BringToFront();
                this.Activate();
                TrapMouse();
            }
        }
    }
}