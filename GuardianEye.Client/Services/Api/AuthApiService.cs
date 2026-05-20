using GuardianEye.Shared.Dtos;
using System.Net.Http.Json;

namespace GuardianEye.Client.Services.Api
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>() 
                   ?? new LoginResponseDto { Success = false, Message = "Failed to parse response" };
        }

        public async Task<SessionValidationDto> ValidateSessionAsync(SessionValidationDto validationRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/validate-session", validationRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SessionValidationDto>() 
                   ?? new SessionValidationDto { CanLogin = false, Reason = "Failed to parse response" };
        }
    }
}