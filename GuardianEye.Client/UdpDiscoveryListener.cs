using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GuardianEye.Client
{
    public class UdpDiscoveryListener
    {
        private UdpClient _udpClient;
        private Thread _listenThread;
        private bool _isListening;
        public event EventHandler<string> AdminDiscovered; // IP address of admin

        public UdpDiscoveryListener(int port = Constants.UdpBroadcastPort)
        {
            _udpClient = new UdpClient(port);
            _udpClient.EnableBroadcast = true;
        }

        public void Start()
        {
            if (_isListening) return;
            _isListening = true;
            _listenThread = new Thread(Listen);
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        public void Stop()
        {
            _isListening = false;
            _udpClient.Close();
            if (_listenThread != null && _listenThread.IsAlive)
            {
                _listenThread.Join();
            }
        }

        private void Listen()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                while (_isListening)
                {
                    byte[] data = _udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(data);
                    // We expect a simple beacon message, e.g., "GuardianEyeAdmin"
                    if (!string.IsNullOrEmpty(message) && message == "GuardianEyeAdmin")
                    {
                        AdminDiscovered?.Invoke(this, remoteEndPoint.Address.ToString());
                    }
                }
            }
            catch (SocketException)
            {
                // Ignore if stopped
            }
            catch (ObjectDisposedException)
            {
                // Ignore if stopped
            }
        }
    }
}