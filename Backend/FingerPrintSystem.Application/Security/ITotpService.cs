using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Application.Security.Dtos;

namespace FingerPrintSystem.Application.Security;

public interface ITotpService {
    Task<TotpSetupDto> BeginSetupAsync(Guid userId, CancellationToken ct = default);
    Task<bool> SetupVerifyAsync(Guid userId, string code, CancellationToken ct = default);
    Task<bool> VerifyLoginAsync(ApplicationUser user, string code, CancellationToken ct = default);
    bool VerifyCode(string base32Secret, string code, CancellationToken ct = default);
}