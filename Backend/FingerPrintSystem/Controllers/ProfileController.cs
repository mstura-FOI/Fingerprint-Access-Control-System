using System.Security.Claims;
using FingerPrintSystem.Application.Services.AccountService;
using FingerPrintSystem.Application.Services.AccountService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintSystem.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(AccountService service) : ControllerBase
{
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await service.ChangeOwnPasswordAsync(userId, dto);
        return result.Succeeded ? NoContent() : BadRequest(result.Errors.Select(e => e.Description));
    }
}