using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    [JsonDerivedType(typeof(LoginRequestMessage), typeDiscriminator: "LoginRequest")]
    [JsonDerivedType(typeof(LoginResponseMessage), typeDiscriminator: "LoginResponse")]
    [JsonDerivedType(typeof(TimerUpdateMessage), typeDiscriminator: "TimerUpdate")]
    [JsonDerivedType(typeof(TimeRequestMessage), typeDiscriminator: "TimeRequest")]
    [JsonDerivedType(typeof(TimeResponseMessage), typeDiscriminator: "TimeResponse")]
    [JsonDerivedType(typeof(AdminCommandMessage), typeDiscriminator: "AdminCommand")]
    [JsonDerivedType(typeof(ClientStatusMessage), typeDiscriminator: "ClientStatus")]
    [JsonDerivedType(typeof(FilterUpdateMessage), typeDiscriminator: "FilterUpdate")]
    [JsonDerivedType(typeof(ChatMessage), typeDiscriminator: "Chat")]
    [JsonDerivedType(typeof(BlockedAttemptMessage), typeDiscriminator: "BlockedAttempt")]
    [JsonDerivedType(typeof(ForegroundStatusMessage), typeDiscriminator: "ForegroundStatus")]
    public abstract class MessageBase
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; protected set; }
    }

    public enum MessageType
    {
        LoginRequest,
        LoginResponse,
        TimerUpdate,
        TimeRequest,
        TimeResponse,
        AdminCommand,
        ClientStatus,
        FilterUpdate,
        Chat,
        BlockedAttempt,
        ForegroundStatus
    }
}