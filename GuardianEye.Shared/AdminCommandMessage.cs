using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public enum AdminCommandType
    {
        AddTime,
        ForceLogout,
        ResetTimer,
        PauseSession,
        ResumeSession,
        EndSession
    }

    public class AdminCommandMessage : MessageBase
    {
        [JsonPropertyName("command")]
        public AdminCommandType Command { get; set; }

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("minutesToAdd")]
        public int MinutesToAdd { get; set; } // For AddTime command

        public AdminCommandMessage()
        {
            Type = MessageType.AdminCommand;
        }
    }
}