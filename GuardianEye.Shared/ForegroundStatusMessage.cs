using System.Text.Json.Serialization;

namespace GuardianEye.Shared;

public class ForegroundStatusMessage : MessageBase
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("processName")]
    public string ProcessName { get; set; }

    [JsonPropertyName("windowTitle")]
    public string WindowTitle { get; set; }

    public ForegroundStatusMessage()
    {
        Type = MessageType.ForegroundStatus;
    }
}
