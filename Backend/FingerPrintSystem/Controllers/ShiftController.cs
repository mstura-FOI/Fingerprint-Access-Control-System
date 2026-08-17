using FingerPrintSystem.Application.Services.ShiftService;
using FingerPrintSystem.Application.Services.ShiftService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;

namespace FingerPrintSystem.WebApi.Controllers;

public class ShiftController(ShiftService service)
    : FxControllerBaseCrud<Shift, ShiftGetDto, ShiftCreateDto, ShiftUpdateDto, ShiftDeleteDto, ShiftService,
        ApplicationDbContext>(service)
{
}