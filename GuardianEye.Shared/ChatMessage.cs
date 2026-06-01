using System.Text.Json.Serialization;

namespace GuardianEye.Shared;

public class ChatMessage : MessageBase
{
    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("isBroadcast")]
    public bool IsBroadcast { get; set; }

    public ChatMessage()
    {
        Type = MessageType.Chat;
    }
}
