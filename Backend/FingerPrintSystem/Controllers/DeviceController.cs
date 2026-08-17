using FingerPrintSystem.Application.Services.DeviceService;
using FingerPrintSystem.Application.Services.DeviceService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;

namespace FingerPrintSystem.WebApi.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class DeviceController(DeviceService service)
    : FxControllerBaseSnapshot<Device, DeviceGetDto, DeviceCreateDto, DeviceService, ApplicationDbContext>(service);