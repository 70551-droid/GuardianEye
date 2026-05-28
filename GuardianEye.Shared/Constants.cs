using System;

namespace GuardianEye.Shared
{
    public static class Constants
    {
        // UDP broadcast port for discovery
        public const int UdpBroadcastPort = 50000;
        // TCP port for communication (admin listens on this)
        public const int TcpPort = 50001;
        // Broadcast address (local network)
        public const string BroadcastAddress = "255.255.255.255";
        // Message delimiter or size prefix? We'll use JSON and send as UTF8 bytes.
        // We'll send the length as a prefix (4 bytes) then the JSON bytes.
        public const int MessageLengthPrefixSize = 4;
    }
}