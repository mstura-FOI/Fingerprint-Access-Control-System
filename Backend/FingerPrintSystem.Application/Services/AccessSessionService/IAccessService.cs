using FingerPrintSystem.Application.Services.AccessSessionService.Dtos;

namespace FingerPrintSystem.Application.Services.AccessSessionService;

public interface IAccessService
{
    Task<AccessPrepareDto> PrepareAsync(string code, Guid deviceId, CancellationToken ct = default);

    Task<bool> ResultAsync(Guid sessionId, bool matched, Guid deviceId, CancellationToken ct = default);
}