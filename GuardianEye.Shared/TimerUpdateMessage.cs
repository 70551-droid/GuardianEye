using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public class TimerUpdateMessage : MessageBase
    {
        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("remainingTimeSeconds")]
        public int RemainingTimeSeconds { get; set; }

        public TimerUpdateMessage()
        {
            Type = MessageType.TimerUpdate;
        }
    }
}