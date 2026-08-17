using FingerPrintSystem.Application.Auth;
using FingerPrintSystem.Application.Auth.AuthService;
using FingerPrintSystem.Application.Auth.TokenService.Dtos;
using FingerPrintSystem.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using FingerPrintSystem.Application.Security.Dtos;

namespace FingerPrintSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth, ITotpService totp) : ControllerBase {
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(LoginRequestDto dto, CancellationToken ct) {
        var result = await auth.LoginAsync(dto, Ip(), ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login/totp")]
    [EnableRateLimiting("totp-attempts")]
    public async Task<ActionResult<TokenResponseDto>> LoginTotp(LoginTotpRequestDto dto, CancellationToken ct) {
        var result = await auth.LoginTotpAsync(dto, Ip(), ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshRequestDto dto, CancellationToken ct) {
        var result = await auth.RefreshAsync(dto.RefreshToken, Ip(), ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequestDto dto, CancellationToken ct) {
        await auth.LogoutAsync(dto.RefreshToken, Ip(), ct);
        return NoContent();
    }


    [Authorize]
    [HttpPost("totp/begin-setup")]
    public async Task<ActionResult<TotpSetupDto>> BeginTotpSetup(CancellationToken ct) {
        return Ok(await totp.BeginSetupAsync(UserId(), ct));
    }

    [Authorize]
    [HttpPost("totp/verify-setup")]
    public async Task<IActionResult> VerifyTotpSetup(TotpVerifyDto dto, CancellationToken ct) {
        var ok = await totp.SetupVerifyAsync(UserId(), dto.Code, ct);
        return ok ? NoContent() : BadRequest(new { detail = "Neispravan kod." });
    }

    private Guid UserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
}