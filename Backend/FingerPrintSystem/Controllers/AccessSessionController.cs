using FingerPrintSystem.Application.Services.AccessSessionService;
using FingerPrintSystem.Application.Services.EnrollmentService;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using FingerPrintSystem.Application.Services.AccessSessionService.Dtos;

namespace FingerPrintSystem.WebApi.Controllers;

[ApiController]
[Route("api/device")]
[AllowAnonymous]                         
[EnableRateLimiting("device-code-attempts")]
public class AccessSessionController(
    IAccessService accessService,
    IEnrollmentService enrollmentService,
    ApplicationDbContext context) : ControllerBase {

    [HttpPost("prepare")]
    public async Task<ActionResult<AccessPrepareDto>> Prepare(
        [FromBody] PrepareRequest req, CancellationToken ct) {
        var deviceId = await ResolveDeviceAsync(ct);
        var result = await accessService.PrepareAsync(req.Code, deviceId, ct);
        return Ok(result);
    }


    [HttpPost("access/result")]
    public async Task<ActionResult<bool>> AccessResult(
        [FromBody] AccessResultRequest req, CancellationToken ct) {
        var deviceId = await ResolveDeviceAsync(ct);
        var granted = await accessService.ResultAsync(req.SessionId, req.Matched, deviceId, ct);
        return Ok(granted);
    }


    [HttpPost("enroll/complete")]
    public async Task<IActionResult> EnrollComplete(
        [FromBody] EnrollCompleteRequest req, CancellationToken ct) {
        await enrollmentService.CompleteAsync(req.Code, req.Template, CancellationToken.None);
        return NoContent();
    }


    private async Task<Guid> ResolveDeviceAsync(CancellationToken ct) {
        var cert = HttpContext.Connection.ClientCertificate
            ?? throw new UnauthorizedAccessException("Nema klijentskog certifikata.");

        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.CertThumbprint == cert.Thumbprint, ct)
            ?? throw new UnauthorizedAccessException("Uređaj nije registriran ili nije aktivan.");

        return device.Id;
    }
}

public record PrepareRequest(string Code);
public record AccessResultRequest(Guid SessionId, bool Matched);
public record EnrollCompleteRequest(string Code, byte[] Template);