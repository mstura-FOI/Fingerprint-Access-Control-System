using FingerPrintSystem.Application.Auth.TokenService.Dtos;
using FingerPrintSystem.Application.Security.Dtos;

namespace FingerPrintSystem.Application.Auth.AuthService;

public interface IAuthService
{
    Task<LoginResultDto?> LoginAsync(LoginRequestDto dto, string? ip, CancellationToken ct = default);
    Task<TokenResponseDto?> RefreshAsync(string refreshToken, string? ip, CancellationToken ct = default);
    Task<TokenResponseDto?> LoginTotpAsync(LoginTotpRequestDto dto, string? ip, CancellationToken ct = default);
    Task<bool> LogoutAsync(string refreshToken, string? ip, CancellationToken ct = default);
}