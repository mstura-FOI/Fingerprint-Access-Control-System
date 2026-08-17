using FingerPrintSystem.Application.Services.AccountService;
using FingerPrintSystem.Application.Services.AccountService.Dtos;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FingerPrintSystem.WebApi.Controllers;

public class AccountController(AccountService service)
    : FxControllerBaseCrud<
        ApplicationUser,
        AccountGetDto,
        AccountCreateDto,
        AccountUpdateDto,
        AccountDeleteDto,
        AccountService,
        ApplicationDbContext>(service)
{
    [Authorize(Roles = Roles.Administrator)]
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        dto.Id = id;
        var result = await service.ResetPasswordAsync(dto);
        return result.Succeeded ? NoContent() : BadRequest(Errors(result));
    }
    [Authorize(Roles = Roles.Administrator)]
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await service.SetActiveAsync(id, true);
        return result.Succeeded ? NoContent() : BadRequest(Errors(result));
    }
    [Authorize(Roles = Roles.Administrator)]
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await service.SetActiveAsync(id, false);
        return result.Succeeded ? NoContent() : BadRequest(Errors(result));
    }

    [Authorize(Roles = $"{Roles.Administrator},{Roles.User}")]
    [HttpGet("{id:guid}")]
    public override Task<ActionResult<AccountGetDto>> Get(
        Guid id,
        CancellationToken cancellationToken = default) {
        return base.Get(id, cancellationToken);
    }

    private static IEnumerable<string> Errors(IdentityResult r)
    {
        return r.Errors.Select(e => e.Description);
    }
}