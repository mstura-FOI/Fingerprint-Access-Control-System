using FingerPrintSystem.Application.Services.FingerprintScanService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;

namespace FingerPrintSystem.Application.Services.FingerprintScanService;

public class FingerprintScanService(ApplicationDbContext context)
    : FxServiceBaseSnapshot<FingerprintScan, FingerprintScanGetDto, ApplicationDbContext>(context), IFingerprintScanService {
    protected override IQueryable<FingerprintScanGetDto> ProjectToGet(IQueryable<FingerprintScan> query)
        => query
            .OrderByDescending(s => s.ScanTime)
            .Select(s => new FingerprintScanGetDto {
                Id = s.Id,
                ApplicationUserId = s.ApplicationUserId,
                DeviceId = s.DeviceId,
                RoomId = s.RoomId,
                ScanTime = s.ScanTime,
                IsSuccessful = s.IsSuccessful,
                DenialReason = s.DenialReason
            });
}