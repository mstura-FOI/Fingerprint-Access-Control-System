using FingerPrintSystem.Core.Entities.Identity;

namespace FingerPrintSystem.Application.Auth.TokenService;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    (string raw, string hash) CreateRefreshToken();

    string HashRefreshToken(string raw);
}