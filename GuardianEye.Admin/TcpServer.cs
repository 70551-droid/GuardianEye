using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using GuardianEye.Shared;

namespace GuardianEye.Admin;

public class TcpServer : IDisposable
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly SessionManager _sessionManager;
    private readonly DatabaseService _database;
    private bool _isDisposed;

    public event EventHandler<ClientInfo> ClientConnected;
    public event EventHandler<string> ClientDisconnected;
    public event EventHandler<TimeRequestMessage> TimeRequestReceived;

    public TcpServer(SessionManager sessionManager, DatabaseService database)
    {
        _sessionManager = sessionManager;
        _database = database;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, Constants.TcpPort);
        _listener.Start();
        _ = AcceptClientsAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
    }

    private async Task AcceptClientsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                TcpClient tcpClient = await _listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                _ = HandleClientAsync(tcpClient, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch { }
        }
    }

    private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken ct)
    {
        NetworkStream stream = tcpClient.GetStream();
        string clientUsername = null;

        try
        {
            while (!ct.IsCancellationRequested)
            {
                byte[] lengthBytes = new byte[4];
                int bytesRead = 0;
                while (bytesRead < 4)
                {
                    int read = await stream.ReadAsync(lengthBytes.AsMemory(bytesRead, 4 - bytesRead), ct).ConfigureAwait(false);
                    if (read == 0) throw new IOException("Connection closed");
                    bytesRead += read;
                }

                int jsonLength = BitConverter.ToInt32(lengthBytes, 0);
                if (jsonLength <= 0 || jsonLength > 65536) continue;

                byte[] jsonBytes = new byte[jsonLength];
                bytesRead = 0;
                while (bytesRead < jsonLength)
                {
                    int read = await stream.ReadAsync(jsonBytes.AsMemory(bytesRead, jsonLength - bytesRead), ct).ConfigureAwait(false);
                    if (read == 0) throw new IOException("Connection closed");
                    bytesRead += read;
                }

                string json = Encoding.UTF8.GetString(jsonBytes);
                MessageBase message;
                try { message = JsonSerializer.Deserialize<MessageBase>(json); }
                catch { continue; }
                if (message == null) continue;

                switch (message.Type)
                {
                    case MessageType.LoginRequest:
                        var loginReq = JsonSerializer.Deserialize<LoginRequestMessage>(json);
                        if (loginReq != null)
                            await HandleLoginAsync(loginReq, tcpClient, stream, ct).ConfigureAwait(false);
                        break;

                    case MessageType.TimeRequest:
                        var timeReq = JsonSerializer.Deserialize<TimeRequestMessage>(json);
                        if (timeReq != null)
                            TimeRequestReceived?.Invoke(this, timeReq);
                        break;

                    case MessageType.ClientStatus:
                        var status = JsonSerializer.Deserialize<ClientStatusMessage>(json);
                        if (status != null)
                            UpdateClientStatus(status);
                        break;

                    case MessageType.BlockedAttempt:
                        var attempt = JsonSerializer.Deserialize<BlockedAttemptMessage>(json);
                        if (attempt != null)
                            _database.LogBlockedAttempt(attempt.Username, attempt.AttemptType.ToString(), attempt.TargetName);
                        break;

                    case MessageType.ForegroundStatus:
                        var fg = JsonSerializer.Deserialize<ForegroundStatusMessage>(json);
                        if (fg != null)
                        {
                            var client = _sessionManager.GetClient(fg.Username);
                            if (client != null)
                            {
                                client.CurrentProcess = fg.ProcessName;
                                client.CurrentWindow = fg.WindowTitle;
                            }
                        }
                        break;
                }
            }
        }
        catch { }
        finally
        {
            clientUsername = _sessionManager.Clients.FirstOrDefault(c => c.TcpClient == tcpClient)?.Username;
            if (clientUsername != null)
            {
                _sessionManager.RemoveClient(clientUsername);
                ClientDisconnected?.Invoke(this, clientUsername);
            }
            stream?.Close();
            tcpClient?.Close();
        }
    }

    private async Task HandleLoginAsync(LoginRequestMessage loginReq, TcpClient tcpClient, NetworkStream stream, CancellationToken ct)
    {
        var (success, displayName, dailyLimit, groupId, groupName) = _database.ValidateStudent(loginReq.Username, loginReq.Password);

        int effectiveLimit = dailyLimit;
        if (success && groupId > 0)
        {
            int groupLimit = _database.GetGroupDailyLimit(groupId);
            if (groupLimit > 0)
                effectiveLimit = Math.Min(dailyLimit, groupLimit);
        }

        int remainingTime = effectiveLimit;
        if (success)
        {
            int usedToday = _database.GetDailyUsedSeconds(loginReq.Username);
            remainingTime = Math.Max(0, effectiveLimit - usedToday);
            remainingTime = Math.Min(remainingTime, 7200);

            int savedTime = _database.GetSavedRemainingTime(loginReq.Username);
            if (savedTime > 0)
                remainingTime = Math.Min(remainingTime, savedTime);

            if (remainingTime == 0)
                success = false;
        }

        var response = new LoginResponseMessage
        {
            Success = success,
            Message = success ? "Login successful" : "Invalid credentials or daily limit reached",
            SessionId = success ? Guid.NewGuid() : Guid.Empty,
            RemainingTimeSeconds = success ? remainingTime : 0,
            DailyLimitSeconds = effectiveLimit,
            StudentName = success ? displayName : ""
        };

        string json = JsonSerializer.Serialize(response, typeof(LoginResponseMessage));
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
        await stream.WriteAsync(lengthBytes, ct).ConfigureAwait(false);
        await stream.WriteAsync(jsonBytes, ct).ConfigureAwait(false);
        await stream.FlushAsync(ct).ConfigureAwait(false);

        if (success)
        {
            var clientInfo = new ClientInfo
            {
                TcpClient = tcpClient,
                Stream = stream,
                SessionId = response.SessionId,
                Username = loginReq.Username,
                DisplayName = displayName,
                GroupName = groupName,
                RemainingTimeSeconds = remainingTime,
                Status = ClientStatusType.Active
            };
            _sessionManager.AddClient(loginReq.Username, clientInfo);
            _database.LogSession(loginReq.Username, response.SessionId, remainingTime);
            ClientConnected?.Invoke(this, clientInfo);
        }
    }

    private void UpdateClientStatus(ClientStatusMessage status)
    {
        var client = _sessionManager.GetClientBySessionId(status.SessionId);
        if (client != null)
        {
            client.Status = status.Status;
            client.RemainingTimeSeconds = status.RemainingTimeSeconds;
            client.LastHeartbeat = DateTime.UtcNow;

            if (status.Status == ClientStatusType.Offline && status.RemainingTimeSeconds > 0)
                _database.SaveRemainingTime(client.Username, status.RemainingTimeSeconds);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Stop();
    }
}
