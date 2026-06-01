using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using GuardianEye.Shared;

namespace GuardianEye.Client;

public class TcpCommunication : IDisposable
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _receiveCts;
    private bool _isConnected;
    private readonly string _host;
    private readonly int _port;
    private bool _isDisposed;

    public event EventHandler<MessageBase> MessageReceived;
    public event EventHandler ConnectionLost;
    public event EventHandler Connected;

    public bool IsConnected => _isConnected;

    public TcpCommunication(string host, int port = Constants.TcpPort)
    {
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync(int timeoutMs = 5000)
    {
        Disconnect();
        _tcpClient = new TcpClient();

        var connectTask = _tcpClient.ConnectAsync(_host, _port);
        var timeoutTask = Task.Delay(timeoutMs);

        var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);
        if (completed == timeoutTask)
        {
            _tcpClient.Close();
            _isConnected = false;
            throw new TimeoutException("Connection timed out");
        }

        if (!_tcpClient.Connected)
        {
            _isConnected = false;
            throw new IOException("Failed to connect");
        }

        _stream = _tcpClient.GetStream();
        _isConnected = true;
        _receiveCts = new CancellationTokenSource();
        _ = ReceiveLoopAsync(_receiveCts.Token);
        Connected?.Invoke(this, EventArgs.Empty);
    }

    public void Connect()
    {
        try
        {
            ConnectAsync().GetAwaiter().GetResult();
        }
        catch { Disconnect(); }
    }

    public void Disconnect()
    {
        _isConnected = false;
        _receiveCts?.Cancel();
        _stream?.Close();
        _tcpClient?.Close();
        _stream = null;
        _tcpClient = null;
        ConnectionLost?.Invoke(this, EventArgs.Empty);
    }

    public void SendMessage(MessageBase message)
    {
        if (!_isConnected) return;
        try
        {
            string json = JsonSerializer.Serialize(message, message.GetType());
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
            _stream.Write(lengthBytes, 0, lengthBytes.Length);
            _stream.Write(jsonBytes, 0, jsonBytes.Length);
            _stream.Flush();
        }
        catch { Disconnect(); }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (_isConnected && !ct.IsCancellationRequested)
            {
                byte[] lengthBytes = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4 && !ct.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(lengthBytes.AsMemory(bytesRead, 4 - bytesRead), ct).ConfigureAwait(false);
                    if (read == 0) throw new IOException("Connection closed");
                    bytesRead += read;
                }
                if (!_isConnected) break;

                int jsonLength = BitConverter.ToInt32(lengthBytes, 0);
                if (jsonLength <= 0 || jsonLength > 65536) continue;

                byte[] jsonBytes = new byte[jsonLength];
                bytesRead = 0;
                while (bytesRead < jsonLength && !ct.IsCancellationRequested)
                {
                    int read = await _stream.ReadAsync(jsonBytes.AsMemory(bytesRead, jsonLength - bytesRead), ct).ConfigureAwait(false);
                    if (read == 0) throw new IOException("Connection closed");
                    bytesRead += read;
                }
                if (!_isConnected) break;

                string json = Encoding.UTF8.GetString(jsonBytes);
                MessageBase message;
                try { message = JsonSerializer.Deserialize<MessageBase>(json); }
                catch { continue; }
                if (message == null) continue;

                MessageBase typedMessage = message.Type switch
                {
                    MessageType.LoginRequest => JsonSerializer.Deserialize<LoginRequestMessage>(json),
                    MessageType.LoginResponse => JsonSerializer.Deserialize<LoginResponseMessage>(json),
                    MessageType.TimerUpdate => JsonSerializer.Deserialize<TimerUpdateMessage>(json),
                    MessageType.TimeRequest => JsonSerializer.Deserialize<TimeRequestMessage>(json),
                    MessageType.TimeResponse => JsonSerializer.Deserialize<TimeResponseMessage>(json),
                    MessageType.AdminCommand => JsonSerializer.Deserialize<AdminCommandMessage>(json),
                    MessageType.ClientStatus => JsonSerializer.Deserialize<ClientStatusMessage>(json),
                    MessageType.FilterUpdate => JsonSerializer.Deserialize<FilterUpdateMessage>(json),
                    MessageType.Chat => JsonSerializer.Deserialize<ChatMessage>(json),
                    MessageType.BlockedAttempt => JsonSerializer.Deserialize<BlockedAttemptMessage>(json),
                    _ => null
                };

                if (typedMessage != null)
                    MessageReceived?.Invoke(this, typedMessage);
            }
        }
        catch (IOException) { if (_isConnected) Disconnect(); }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
        catch { if (_isConnected) Disconnect(); }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Disconnect();
    }

}
