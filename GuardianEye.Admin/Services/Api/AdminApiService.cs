using GuardianEye.Shared.Dtos;
using System.Net.Http.Json;
using System.Net.Http;

namespace GuardianEye.Admin.Services.Api
{
    public class AdminApiService : IAdminApiService
    {
        private readonly HttpClient _httpClient;

        public AdminApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponseDto> AdminLoginAsync(AdminLoginRequestDto loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/admin-login", loginRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>() 
                   ?? new LoginResponseDto { Success = false, Message = "Failed to parse response" };
        }
    }
}