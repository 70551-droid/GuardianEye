using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public class LoginResponseMessage : MessageBase
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("remainingTimeSeconds")]
        public int RemainingTimeSeconds { get; set; }

        [JsonPropertyName("dailyLimitSeconds")]
        public int DailyLimitSeconds { get; set; }

        [JsonPropertyName("studentName")]
        public string StudentName { get; set; }

        public LoginResponseMessage()
        {
            Type = MessageType.LoginResponse;
        }
    }
}