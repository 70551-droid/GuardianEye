using System;
using System.Diagnostics;
using System.Threading;

namespace GuardianEye.Client
{
    /// <summary>
    /// Continuously monitors and kills forbidden processes (Task Manager, CMD, PowerShell).
    /// Runs on a dedicated background thread with a 500ms polling interval.
    /// </summary>
    public class ProcessWatchdog : IDisposable
    {
        private Thread _watchThread;
        private volatile bool _isRunning;
        private bool _isDisposed;

        // Every process name a student could use to kill GuardianEye or access system tools
        private static readonly string[] ForbiddenProcesses = new[]
        {
            "Taskmgr",          // Task Manager
            "cmd",              // Command Prompt
            "powershell",       // Windows PowerShell
            "pwsh",             // PowerShell Core
            "ProcessHacker",    // 3rd party process manager
            "procexp",          // Sysinternals Process Explorer
            "procexp64",        // Sysinternals Process Explorer 64-bit
            "taskkill",         // taskkill.exe command
            "wmic",             // WMI command line
            "msconfig",         // System Configuration
            "regedit",          // Registry Editor
            "mmc",              // Management Console (services, etc.)
        };

        public ProcessWatchdog()
        {
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _watchThread = new Thread(WatchLoop)
            {
                IsBackground = true,
                Name = "ProcessWatchdog",
                Priority = ThreadPriority.AboveNormal
            };
            _watchThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void WatchLoop()
        {
            while (_isRunning)
            {
                try
                {
                    KillForbiddenProcesses();
                }
                catch
                {
                    // Swallow all exceptions — this watchdog must never crash
                }
                Thread.Sleep(500);
            }
        }

        private void KillForbiddenProcesses()
        {
            foreach (string name in ForbiddenProcesses)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(name);
                    foreach (Process proc in processes)
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit(500);
                        }
                        catch
                        {
                            // Process may have exited already or access denied
                        }
                        finally
                        {
                            proc.Dispose();
                        }
                    }
                }
                catch
                {
                    // GetProcessesByName can throw — ignore and continue
                }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Stop();
        }
    }
}
