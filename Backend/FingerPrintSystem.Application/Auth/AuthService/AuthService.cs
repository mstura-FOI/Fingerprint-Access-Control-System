using FingerPrintSystem.Application.Auth.TokenService;
using FingerPrintSystem.Application.Auth.TokenService.Dtos;
using FingerPrintSystem.Application.Security;
using FingerPrintSystem.Application.Security.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Core.Options;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OtpNet;

namespace FingerPrintSystem.Application.Auth.AuthService;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db, ITotpService totp,
    ITokenService tokenService,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<LoginResultDto?> LoginAsync(LoginRequestDto dto, string? ip, CancellationToken ct = default) {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null) return null;
        if (await userManager.IsLockedOutAsync(user)) return null;

        if (!await userManager.CheckPasswordAsync(user, dto.Password)) {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (user.TotpEnabled)
            return new LoginResultDto( true, null );

        var tokens = await IssueTokensAsync(user, ip, ct);
        return new LoginResultDto (false, tokens );
    }
    public async Task<TokenResponseDto?> LoginTotpAsync(LoginTotpRequestDto dto, string? ip, CancellationToken ct) {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, dto.Password))
            return null;

        if (!user.TotpEnabled)
            return null;

        if (!await totp.VerifyLoginAsync(user, dto.Code)) {
            await userManager.AccessFailedAsync(user); 
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, ip, ct); 
    }

    public async Task<TokenResponseDto?> RefreshAsync(string refreshToken, string? ip, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(refreshToken);

        var existing = await db.Set<RefreshToken>()
            .Include(r => r.ApplicationUser)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

        if (existing is null) return null;

        if (!existing.IsActive)
        {
            await RevokeAllUserTokensAsync(existing.ApplicationUserId, ip, "reuse-detected", ct);
            return null;
        }

        var (rawRefresh, refreshHash) = tokenService.CreateRefreshToken();
        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = ip;
        existing.RevokedReason = "rotated";
        existing.ReplacedByTokenHash = refreshHash;

        var user = existing.ApplicationUser;
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessExpires) = tokenService.CreateAccessToken(user, roles);

        var newRefresh = new RefreshToken
        {
            ApplicationUser = user,
            ApplicationUserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedByIp = ip
        };
        db.Set<RefreshToken>().Add(newRefresh);
        await db.SaveChangesAsync(ct);

        return Build(accessToken, accessExpires, rawRefresh, newRefresh.ExpiresAt);
    }

    public async Task<bool> LogoutAsync(string refreshToken, string? ip, CancellationToken ct = default)
    {
        var hash = tokenService.HashRefreshToken(refreshToken);
        var token = await db.Set<RefreshToken>().FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
        if (token is null || !token.IsActive) return false;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ip;
        token.RevokedReason = "logout";
        await db.SaveChangesAsync(ct);
        return true;
    }

    private async Task<TokenResponseDto> IssueTokensAsync(ApplicationUser user, string? ip, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessExpires) = tokenService.CreateAccessToken(user, roles);
        var (rawRefresh, refreshHash) = tokenService.CreateRefreshToken();

        db.Set<RefreshToken>().Add(new RefreshToken
        {
            ApplicationUser = user,
            ApplicationUserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedByIp = ip
        });
        await db.SaveChangesAsync(ct);

        return Build(accessToken, accessExpires, rawRefresh, DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays));
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, string? ip, string reason, CancellationToken ct)
    {
        var active = await db.Set<RefreshToken>()
            .Where(r => r.ApplicationUserId == userId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var t in active)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedByIp = ip;
            t.RevokedReason = reason;
        }

        await db.SaveChangesAsync(ct);
    }

    private static TokenResponseDto Build(string access, DateTime accessExp, string refresh, DateTime refreshExp)
    {
        return new TokenResponseDto
        {
            AccessToken = access,
            AccessTokenExpiresAtUtc = accessExp,
            RefreshToken = refresh,
            RefreshTokenExpiresAtUtc = refreshExp
        };
    }
}