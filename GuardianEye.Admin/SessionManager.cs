using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using GuardianEye.Shared;

namespace GuardianEye.Admin;

public class ClientInfo
{
    public TcpClient TcpClient { get; set; }
    public NetworkStream Stream { get; set; }
    public Guid SessionId { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string GroupName { get; set; }
    public string CurrentProcess { get; set; }
    public string CurrentWindow { get; set; }
    public int RemainingTimeSeconds { get; set; }
    public ClientStatusType Status { get; set; } = ClientStatusType.Online;
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
}

public class SessionManager : IDisposable
{
    private readonly ConcurrentDictionary<string, ClientInfo> _clients = new();
    private readonly object _lock = new();

    public IReadOnlyCollection<ClientInfo> Clients
    {
        get { lock (_lock) return _clients.Values.ToList().AsReadOnly(); }
    }

    public void AddClient(string username, ClientInfo info)
    {
        lock (_lock) _clients[username] = info;
    }

    public bool TryGetClient(string username, out ClientInfo info)
    {
        lock (_lock) return _clients.TryGetValue(username, out info);
    }

    public void RemoveClient(string username)
    {
        lock (_lock) _clients.TryRemove(username, out _);
    }

    public ClientInfo GetClient(string username)
    {
        lock (_lock) return _clients.GetValueOrDefault(username);
    }

    public ClientInfo GetClientBySessionId(Guid sessionId)
    {
        lock (_lock) return _clients.Values.FirstOrDefault(c => c.SessionId == sessionId);
    }

    public void SendMessage(string username, MessageBase message)
    {
        if (!TryGetClient(username, out var client)) return;
        try
        {
            string json = JsonSerializer.Serialize(message, message.GetType());
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
            client.Stream.Write(lengthBytes, 0, lengthBytes.Length);
            client.Stream.Write(jsonBytes, 0, jsonBytes.Length);
            client.Stream.Flush();
        }
        catch { RemoveClient(username); }
    }

    public void BroadcastMessage(MessageBase message)
    {
        List<string> keys;
        lock (_lock) keys = _clients.Keys.ToList();
        foreach (var key in keys)
            SendMessage(key, message);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var client in _clients.Values)
            {
                client.Stream?.Close();
                client.TcpClient?.Close();
            }
            _clients.Clear();
        }
    }
}
