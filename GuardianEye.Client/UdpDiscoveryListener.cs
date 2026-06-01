using System.Net;
using System.Net.Sockets;
using System.Text;
using GuardianEye.Shared;

namespace GuardianEye.Client;

public class UdpDiscoveryListener
{
    private UdpClient _udpClient;
    private CancellationTokenSource _cts;
    public event EventHandler<string> AdminDiscovered;

    public UdpDiscoveryListener(int port = Constants.UdpBroadcastPort)
    {
    }

    public void Start()
    {
        Stop();
        _cts = new CancellationTokenSource();
        _udpClient = new UdpClient(Constants.UdpBroadcastPort);
        _udpClient.EnableBroadcast = true;
        _ = ListenAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _udpClient?.Close();
        _udpClient = null;
    }

    private async Task ListenAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync(ct).ConfigureAwait(false);
                string message = Encoding.UTF8.GetString(result.Buffer);
                if (message == "GuardianEyeAdmin")
                {
                    AdminDiscovered?.Invoke(this, result.RemoteEndPoint.Address.ToString());
                }
            }
        }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
        catch (SocketException) { }
    }
}
