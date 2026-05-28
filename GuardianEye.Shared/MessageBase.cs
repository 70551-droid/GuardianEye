using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public abstract class MessageBase
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; protected set; }
    }

    public enum MessageType
    {
        LoginRequest,
        LoginResponse,
        TimerUpdate,
        TimeRequest,
        TimeResponse,
        AdminCommand,
        ClientStatus
    }
}