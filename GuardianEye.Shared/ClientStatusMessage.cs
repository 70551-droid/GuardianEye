using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public enum ClientStatusType
    {
        Online,
        Offline,
        Active,
        Idle
    }

    public class ClientStatusMessage : MessageBase
    {
        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("status")]
        public ClientStatusType Status { get; set; }

        [JsonPropertyName("remainingTimeSeconds")]
        public int RemainingTimeSeconds { get; set; }

        public ClientStatusMessage()
        {
            Type = MessageType.ClientStatus;
        }
    }
}