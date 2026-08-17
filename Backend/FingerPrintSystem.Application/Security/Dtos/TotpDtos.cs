using FingerPrintSystem.Application.Auth.TokenService.Dtos;

namespace FingerPrintSystem.Application.Security.Dtos;

public record LoginResultDto(bool RequiresTotp, TokenResponseDto? Tokens);

public record LoginTotpRequestDto(string Email, string Password, string Code);

public record TotpVerifyDto(string Code);

public record TotpSetupDto(string Secret, string OtpAuthUri);