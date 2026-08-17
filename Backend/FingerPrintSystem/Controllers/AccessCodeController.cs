using System.Security.Claims;
using FingerPrintSystem.Application.Services.AccessCodeService;
using FingerPrintSystem.Application.Services.AccessCodeService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccessCodeController(IAccessCodeService accessCodes) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<CodeDto>> Generate(CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var code = await accessCodes.GenerateAsync(userId, ct);
        return Ok(new CodeDto { Code = code });
    }
}