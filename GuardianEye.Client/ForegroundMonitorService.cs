using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GuardianEye.Shared;

namespace GuardianEye.Client;

public class ForegroundMonitorService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private TcpCommunication _tcpClient;
    private string _username;
    private string _lastProcess;
    private string _lastTitle;
    private System.Threading.Timer _pollTimer;
    private bool _isDisposed;

    public ForegroundMonitorService(TcpCommunication tcpClient, string username)
    {
        _tcpClient = tcpClient;
        _username = username;
    }

    public void SetTcpClient(TcpCommunication client, string username)
    {
        _tcpClient = client;
        _username = username;
    }

    public void Start()
    {
        _pollTimer = new System.Threading.Timer(PollForeground, null, 0, 1000);
    }

    private void PollForeground(object state)
    {
        if (_isDisposed) return;

        try
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return;

            var titleBuilder = new StringBuilder(256);
            GetWindowText(hWnd, titleBuilder, 256);
            string title = titleBuilder.ToString().Trim();
            if (string.IsNullOrEmpty(title)) return;

            GetWindowThreadProcessId(hWnd, out uint pid);
            string processName = "";
            try
            {
                using var proc = Process.GetProcessById((int)pid);
                processName = proc.ProcessName;
            }
            catch { return; }

            if (processName == _lastProcess && title == _lastTitle)
                return;

            _lastProcess = processName;
            _lastTitle = title;

            if (_tcpClient?.IsConnected == true)
            {
                _tcpClient.SendMessage(new ForegroundStatusMessage
                {
                    Username = _username,
                    ProcessName = processName,
                    WindowTitle = title
                });
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _pollTimer?.Dispose();
    }
}
