using GuardianEye.Shared.Dtos;
using System.Net.Http.Json;

namespace GuardianEye.Admin.Services.Api
{
    public interface IAdminApiService
    {
        Task<LoginResponseDto> AdminLoginAsync(AdminLoginRequestDto loginRequest);
        // Additional methods for admin operations will be added later
    }
}