using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using GuardianEye.Shared;

namespace GuardianEye.Client
{
    public class TcpCommunication
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private bool _isConnected;
        private readonly string _host;
        private readonly int _port;

        public event EventHandler<MessageBase> MessageReceived;
        public event EventHandler ConnectionLost;
        public event EventHandler Connected;

        public bool IsConnected => _isConnected;

        public TcpCommunication(string host, int port = Constants.TcpPort)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            Disconnect(); // Ensure clean state
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_host, _port);
            _stream = _tcpClient.GetStream();
            _isConnected = true;
            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void Disconnect()
        {
            _isConnected = false;
            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join();
            }
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
                string json = JsonSerializer.Serialize(message);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
                byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
                // Send length first
                _stream.Write(lengthBytes, 0, lengthBytes.Length);
                // Then send JSON
                _stream.Write(jsonBytes, 0, jsonBytes.Length);
                _stream.Flush();
            }
            catch
            {
                Disconnect();
            }
        }

        private void ReceiveLoop()
        {
            try
            {
                while (_isConnected)
                {
                    // Read length prefix (4 bytes)
                    byte[] lengthBytes = new byte[4];
                    int bytesRead = 0;
                    while (bytesRead < 4 && _isConnected)
                    {
                        int read = _stream.Read(lengthBytes, bytesRead, 4 - bytesRead);
                        if (read == 0) throw new IOException("Connection closed");
                        bytesRead += read;
                    }
                    if (!_isConnected) break;
                    int jsonLength = BitConverter.ToInt32(lengthBytes, 0);
                    // Read JSON payload
                    byte[] jsonBytes = new byte[jsonLength];
                    bytesRead = 0;
                    while (bytesRead < jsonLength && _isConnected)
                    {
                        int read = _stream.Read(jsonBytes, bytesRead, jsonLength - bytesRead);
                        if (read == 0) throw new IOException("Connection closed");
                        bytesRead += read;
                    }
                    if (!_isConnected) break;
                    string json = Encoding.UTF8.GetString(jsonBytes);
                    MessageBase message = JsonSerializer.Deserialize<MessageBase>(json);
                    // Determine concrete type and deserialize again
                    MessageBase typedMessage = message.Type switch
                    {
                        MessageType.LoginRequest => JsonSerializer.Deserialize<LoginRequestMessage>(json),
                        MessageType.LoginResponse => JsonSerializer.Deserialize<LoginResponseMessage>(json),
                        MessageType.TimerUpdate => JsonSerializer.Deserialize<TimerUpdateMessage>(json),
                        MessageType.TimeRequest => JsonSerializer.Deserialize<TimeRequestMessage>(json),
                        MessageType.TimeResponse => JsonSerializer.Deserialize<TimeResponseMessage>(json),
                        MessageType.AdminCommand => JsonSerializer.Deserialize<AdminCommandMessage>(json),
                        MessageType.ClientStatus => JsonSerializer.Deserialize<ClientStatusMessage>(json),
                        _ => null
                    };
                    if (typedMessage != null)
                    {
                        MessageReceived?.Invoke(this, typedMessage);
                    }
                }
            }
            catch (IOException)
            {
                if (_isConnected)
                {
                    Disconnect();
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore if stopped
            }
            catch (Exception)
            {
                if (_isConnected)
                {
                    Disconnect();
                }
            }
        }
    }
}