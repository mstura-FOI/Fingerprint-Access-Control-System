using System.Security.Claims;
using FingerPrintSystem.Application.Services.AttendanceService;
using FingerPrintSystem.Application.Services.AttendanceService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintSystem.WebApi.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class AttendanceController(AttendanceService service)
    : FxControllerBaseSnapshot<Attendance, AttendanceGetDto, AttendanceService, ApplicationDbContext>(service)
{
    [Authorize(Roles = Roles.Administrator)]
    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] AttendanceExitDto dto, CancellationToken ct)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await service.CheckOutAsync(managerId, dto, ct);
        return NoContent();
    }
}