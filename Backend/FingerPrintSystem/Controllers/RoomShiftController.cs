using FingerPrintSystem.Application.Services.RoomShiftService;
using FingerPrintSystem.Application.Services.RoomShiftService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;

namespace FingerPrintSystem.WebApi.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class RoomShiftController(RoomShiftService service)
    : FxControllerBaseCrud<RoomShift, RoomShiftGetDto, RoomShiftCreateDto,
        RoomShiftUpdateDto, RoomShiftDeleteDto, RoomShiftService, ApplicationDbContext>(service);