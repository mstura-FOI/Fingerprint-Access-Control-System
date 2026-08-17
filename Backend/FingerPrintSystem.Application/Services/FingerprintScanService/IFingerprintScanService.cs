using FingerPrintSystem.Application.Services.FingerprintScanService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.FingerprintScanService;

public interface IFingerprintScanService : IFxServiceSnapshot<FingerprintScanGetDto>
{
}