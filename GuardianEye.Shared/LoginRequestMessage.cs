using System;
using System.Text.Json.Serialization;

namespace GuardianEye.Shared
{
    public class LoginRequestMessage : MessageBase
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        public LoginRequestMessage()
        {
            Type = MessageType.LoginRequest;
        }
    }
}