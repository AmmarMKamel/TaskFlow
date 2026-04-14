using TaskFlow.src.Application.DTOs.Auth;

namespace TaskFlow.src.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    }
}
