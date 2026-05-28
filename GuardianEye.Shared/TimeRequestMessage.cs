using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public class TimeRequestMessage : MessageBase
    {
        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("requestedMinutes")]
        public int RequestedMinutes { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        public TimeRequestMessage()
        {
            Type = MessageType.TimeRequest;
        }
    }
}