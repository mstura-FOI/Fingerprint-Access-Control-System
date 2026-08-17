namespace FingerPrintSystem.Application.Services.AccessCodeService;

public interface IAccessCodeService
{
    Task<string> GenerateAsync(Guid userId, CancellationToken ct = default);

    Task<Guid> VerifyAsync(string code, CancellationToken ct = default);

    Task ConsumeAsync(Guid userId, CancellationToken ct = default);
}