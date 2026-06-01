using System.Text.Json.Serialization;

namespace GuardianEye.Shared;

public class FilterUpdateMessage : MessageBase
{
    [JsonPropertyName("blockedDomains")]
    public List<string> BlockedDomains { get; set; } = new();

    [JsonPropertyName("blockedProcesses")]
    public List<string> BlockedProcesses { get; set; } = new();

    public FilterUpdateMessage()
    {
        Type = MessageType.FilterUpdate;
    }
}
