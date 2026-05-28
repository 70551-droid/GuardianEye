using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public class TimeResponseMessage : MessageBase
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("addedTimeSeconds")]
        public int AddedTimeSeconds { get; set; }

        public TimeResponseMessage()
        {
            Type = MessageType.TimeResponse;
        }
    }
}