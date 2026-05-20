using GuardianEye.Shared.Dtos;
using System.Net.Http.Json;

namespace GuardianEye.Client.Services.Api
{
    public interface IAuthApiService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<SessionValidationDto> ValidateSessionAsync(SessionValidationDto validationRequest);
    }
}