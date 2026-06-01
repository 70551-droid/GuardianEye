using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace GuardianEye.Client;

static class Program
{
    private const string MutexId = "GuardianEyeClient-SingleInstance-7A3F9B1E";
    private const string WatchdogTaskName = "GuardianEye Watchdog";
    private const string BackupDir = @"C:\Windows\Temp\GuardianEye";

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "--install":
                    InstallAutoStart();
                    return;
                case "--remove":
                    RemoveAutoStart();
                    return;
            }
        }

        using var mutex = new Mutex(true, MutexId, out bool createdNew);
        if (!createdNew)
            return;

        EnsureHiddenCopy();
        EnsureWatchdogTask();
        InstallRegistryRun();

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static void EnsureHiddenCopy()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            Directory.CreateDirectory(BackupDir);
            string destPath = Path.Combine(BackupDir, "GE_Client.exe");
            if (!File.Exists(destPath))
                File.Copy(exePath, destPath, true);
        }
        catch { }
    }

    private static void EnsureWatchdogTask()
    {
        try
        {
            string copyPath = Path.Combine(BackupDir, "GE_Client.exe");
            if (!File.Exists(copyPath)) return;

            var psi = new ProcessStartInfo("schtasks")
            {
                Arguments = $"/create /tn \"{WatchdogTaskName}\" /tr \"\\\"{copyPath}\\\"\" /sc minute /mo 5 /f",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(10000);
        }
        catch { }
    }

    private static void InstallRegistryRun()
    {
        try
        {
            string copyPath = Path.Combine(BackupDir, "GE_Client.exe");
            if (!File.Exists(copyPath)) return;

            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key.SetValue("GuardianEye Client", copyPath);
        }
        catch { }
    }

    private static void InstallAutoStart()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key.SetValue("GuardianEye Client", exePath);
            MessageBox.Show("GuardianEye Client will start automatically on next boot.", "Auto-Start Installed",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to install auto-start: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void RemoveAutoStart()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key.GetValue("GuardianEye Client") != null)
                key.DeleteValue("GuardianEye Client");

            var psi = new ProcessStartInfo("schtasks")
            {
                Arguments = $"/delete /tn \"{WatchdogTaskName}\" /f",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(10000);

            MessageBox.Show("GuardianEye Client auto-start and watchdog removed.", "Removed",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to remove: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
