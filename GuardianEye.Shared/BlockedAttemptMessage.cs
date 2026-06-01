using System.Text.Json.Serialization;

namespace GuardianEye.Shared;

public enum AttemptType
{
    Website,
    Process,
    Browser
}

public class BlockedAttemptMessage : MessageBase
{
    [JsonPropertyName("attemptType")]
    public AttemptType AttemptType { get; set; }

    [JsonPropertyName("targetName")]
    public string TargetName { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    public BlockedAttemptMessage()
    {
        Type = MessageType.BlockedAttempt;
    }
}
