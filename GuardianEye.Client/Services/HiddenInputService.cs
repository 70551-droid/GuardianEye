using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GuardianEye.Services
{
    public interface IHiddenInputService
    {
        event Action<string>? SequenceEntered;
        void StartListening();
        void StopListening();
        void Initialize();
    }

    public class HiddenInputService : IHiddenInputService, IDisposable
    {
        private readonly LowLevelKeyboardListener _listener;
        private bool _isListening = false;
        private DateTime _listenStartTime;
        private const int ListenTimeoutMs = 5000;
        private string _currentSequence = "";

        public event Action<string>? SequenceEntered;

        public HiddenInputService()
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += OnKeyPressed;
        }

        public void Initialize()
        {
        }

        public void StartListening()
        {
            _isListening = true;
            _listenStartTime = DateTime.UtcNow;
            _currentSequence = "";
        }

        public void StopListening()
        {
            _isListening = false;
            _currentSequence = "";
        }

        private void OnKeyPressed(object? sender, char keyChar)
        {
            if (!_isListening) return;

            if (DateTime.UtcNow.Subtract(_listenStartTime).TotalMilliseconds > ListenTimeoutMs)
            {
                _isListening = false;
                _currentSequence = "";
                return;
            }

            if (char.IsLetterOrDigit(keyChar) || keyChar == '_')
            {
                _currentSequence += keyChar.ToString().ToLowerInvariant();
                
                if (_currentSequence == "guardianoverride")
                {
                    _isListening = false;
                    SequenceEntered?.Invoke(_currentSequence);
                }
            }
            else if (keyChar == '\b')
            {
                if (_currentSequence.Length > 0)
                    _currentSequence = _currentSequence[..^1];
            }
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }

        private class LowLevelKeyboardListener : IDisposable
        {
            private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

            private readonly IntPtr _hookId = IntPtr.Zero;
            private readonly LowLevelKeyboardProc _proc;
            private bool _disposed;

            public event EventHandler<char>? OnKeyPressed;

            public LowLevelKeyboardListener()
            {
                _proc = HookCallback;
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule!;
                _hookId = SetHook(_proc);
            }

            private IntPtr SetHook(LowLevelKeyboardProc proc)
            {
                using var curProcess = Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule!;
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, 
                    GetModuleHandle(curModule.ModuleName), 0);
            }

            private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    
                    if (vkCode >= 65 && vkCode <= 90)
                    {
                        bool isShiftPressed = (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;
                        char keyChar = (char)(isShiftPressed ? vkCode : vkCode + 32);
                        OnKeyPressed?.Invoke(this, keyChar);
                    }
                    else if (vkCode >= 48 && vkCode <= 57)
                    {
                        char keyChar = (char)vkCode;
                        OnKeyPressed?.Invoke(this, keyChar);
                    }
                    else if (vkCode == VK_BACK)
                    {
                        OnKeyPressed?.Invoke(this, '\b');
                    }
                }

                return CallNextHookEx(_hookId, nCode, wParam, lParam);
            }

            public void Dispose()
            {
                if (!_disposed && _hookId != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(_hookId);
                    _disposed = true;
                }
            }

            private const int WH_KEYBOARD_LL = 13;
            private const int WM_KEYDOWN = 0x0100;
            private const int VK_SHIFT = 0x10;
            private const int VK_BACK = 0x08;

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc lpfn, 
                IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, 
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string? lpModuleName);

            [DllImport("user32.dll")]
            private static extern short GetAsyncKeyState(int vKey);
        }
    }
}