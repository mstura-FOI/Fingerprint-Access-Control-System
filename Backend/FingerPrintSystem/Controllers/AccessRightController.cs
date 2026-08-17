using FingerPrintSystem.Application.Services.AccessRightService;
using FingerPrintSystem.Application.Services.AccessRightService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;

namespace FingerPrintSystem.WebApi.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class AccessRightController(AccessRightService service)
    : FxControllerBaseCrud<AccessRight, AccessRightGetDto, AccessRightCreateDto,
        AccessRightUpdateDto, AccessRightDeleteDto, AccessRightService, ApplicationDbContext>(service);