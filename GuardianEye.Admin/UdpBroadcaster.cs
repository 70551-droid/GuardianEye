using System.Net;
using System.Net.Sockets;
using System.Text;
using GuardianEye.Shared;

namespace GuardianEye.Admin;

public class UdpBroadcaster : IDisposable
{
    private UdpClient _udpClient;
    private CancellationTokenSource _cts;
    private bool _isDisposed;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _udpClient = new UdpClient();
        _udpClient.EnableBroadcast = true;
        _ = BroadcastLoopAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _udpClient?.Close();
    }

    private async Task BroadcastLoopAsync(CancellationToken ct)
    {
        byte[] data = Encoding.UTF8.GetBytes("GuardianEyeAdmin");
        var endpoint = new IPEndPoint(IPAddress.Broadcast, Constants.UdpBroadcastPort);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _udpClient.SendAsync(data, data.Length, endpoint).ConfigureAwait(false);
                await Task.Delay(3000, ct).ConfigureAwait(false);
            }
            catch (ObjectDisposedException) { break; }
            catch (OperationCanceledException) { break; }
            catch { /* ignore */ }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Stop();
        _udpClient?.Dispose();
    }
}
