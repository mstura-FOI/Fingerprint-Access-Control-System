using FingerPrintSystem.Application.Services.FingerprintScanService;
using FingerPrintSystem.Application.Services.FingerprintScanService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.AspNetCore.Authorization;

namespace FingerPrintSystem.WebApi.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class FingerprintScanController(FingerprintScanService service)
    : FxControllerBaseSnapshot<FingerprintScan, FingerprintScanGetDto, FingerprintScanService, ApplicationDbContext>(
        service);