using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GuardianEye.Client
{
    public partial class LockScreenForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

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
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;

            Screen primary = Screen.PrimaryScreen;
            this.Bounds = primary.Bounds;

            labelMessage.AutoSize = false;
            labelMessage.TextAlign = ContentAlignment.MiddleCenter;
            labelMessage.Dock = DockStyle.Fill;
            labelMessage.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            labelMessage.ForeColor = Color.FromArgb(220, 50, 50);

            _lockKeyboardHook = new GlobalKeyboardHook();
            _lockKeyboardHook.IsLockdownActive = true;
            _lockKeyboardHook.Hook();
        }

        public void SetMessage(string message)
        {
            labelMessage.Text = message;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle,
                Color.FromArgb(10, 8, 20),
                Color.FromArgb(20, 16, 36),
                LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, ClientRectangle);

            for (int y = 0; y < ClientRectangle.Height; y += 4)
            {
                using var scanPen = new Pen(Color.FromArgb(6, 255, 255, 255));
                e.Graphics.DrawLine(scanPen, 0, y, ClientRectangle.Width, y);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            string text = labelMessage.Text;
            if (string.IsNullOrEmpty(text)) return;

            var font = labelMessage.Font;
            var textRect = new Rectangle(40, ClientRectangle.Height / 2 - 60, ClientRectangle.Width - 80, 120);

            using var glowBrush = new SolidBrush(Color.FromArgb(40, 255, 50, 50));
            var glowRect = textRect;
            glowRect.Offset(2, 2);
            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.DrawString(text, font, glowBrush, glowRect, fmt);
                glowRect.Offset(-4, -4);
                e.Graphics.DrawString(text, font, glowBrush, glowRect, fmt);
                glowRect.Offset(2, 2);
                using var textBrush = new SolidBrush(Color.FromArgb(255, 80, 80));
                e.Graphics.DrawString(text, font, textBrush, textRect, fmt);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Primary) continue;

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

            TrapMouse();
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

        public void UnlockAndClose()
        {
            _allowClose = true;
            ReleaseMouse();

            foreach (var overlay in _secondaryOverlays)
            {
                if (overlay != null && !overlay.IsDisposed)
                {
                    overlay.Close();
                    overlay.Dispose();
                }
            }
            _secondaryOverlays.Clear();

            _lockKeyboardHook?.Dispose();
            _lockKeyboardHook = null;

            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                e.Cancel = true;
                return;
            }

            ReleaseMouse();
            _lockKeyboardHook?.Dispose();
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4))
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (!_allowClose && !this.IsDisposed)
            {
                this.BringToFront();
                this.Activate();
                TrapMouse();
            }
        }
    }
}
