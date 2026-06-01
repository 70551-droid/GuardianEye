using System.Diagnostics;
using GuardianEye.Shared;

namespace GuardianEye.Client;

public class BrowserRestrictionService : IDisposable
{
    private CancellationTokenSource _cts;
    private Task _watchTask;
    private bool _isDisposed;
    private TcpCommunication _tcpClient;
    private string _username;

    private static readonly string[] KnownBrowsers = new[]
    {
        "firefox", "msedge", "iexplore", "opera", "brave",
        "vivaldi", "tor", "whale", "safari", "seamonkey",
        "maxthon", "waterfox", "palemoon", "epiphany", "midori",
        "netscape", "k-meleon", "otter", "falkon", "qutebrowser"
    };

    public void SetTcpClient(TcpCommunication tcpClient, string username)
    {
        _tcpClient = tcpClient;
        _username = username;
    }

    public void Start()
    {
        if (_watchTask != null) return;
        _cts = new CancellationTokenSource();
        _watchTask = WatchLoopAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _watchTask = null;
    }

    private async Task WatchLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try { KillNonChromeBrowsers(); }
            catch { }
            try { await Task.Delay(1000, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
        }
    }

    private void KillNonChromeBrowsers()
    {
        foreach (string name in KnownBrowsers)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName(name))
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(500);
                        ReportBlocked(AttemptType.Browser, name);
                    }
                    catch { }
                    finally { proc.Dispose(); }
                }
            }
            catch { }
        }
    }

    private void ReportBlocked(AttemptType type, string target)
    {
        if (_tcpClient?.IsConnected == true)
        {
            _tcpClient.SendMessage(new BlockedAttemptMessage
            {
                AttemptType = type,
                TargetName = target,
                Timestamp = DateTime.UtcNow,
                Username = _username
            });
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Stop();
        _cts?.Dispose();
    }
}
