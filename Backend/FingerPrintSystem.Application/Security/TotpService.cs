using System.Security.Cryptography;
using FingerPrintSystem.Application.Security.Dtos;
using FingerPrintSystem.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using OtpNet;

namespace FingerPrintSystem.Application.Security;

public class TotpService(UserManager<ApplicationUser> userManager, ITotpSecretProtector protector) : ITotpService
{
    public async Task<TotpSetupDto> BeginSetupAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
                   ?? throw new InvalidOperationException("Korisnik ne postoji.");

        var secretBytes = RandomNumberGenerator.GetBytes(20);
        var base32Secret = Base32Encoding.ToString(secretBytes);

        user.TotpSecret = protector.Protect(base32Secret);
        user.TotpEnabled = false;
        await userManager.UpdateAsync(user);

        var issuer = "FingerPrintSystem";
        var uri = $"otpauth://totp/{issuer}:{Uri.EscapeDataString(user.Email!)}"
                  + $"?secret={base32Secret}&issuer={issuer}&digits=6&period=30";

        return new TotpSetupDto(base32Secret, uri);
    }

    public async Task<bool> SetupVerifyAsync(Guid userId, string code, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user?.TotpSecret is null) return false;

        var secret = protector.Unprotect(user.TotpSecret);
        if (!VerifyCode(secret, code)) return false;

        user.TotpEnabled = true;
        await userManager.UpdateAsync(user);
        return true;
    }

    public async Task<bool> VerifyLoginAsync(ApplicationUser user, string code, CancellationToken ct = default) {
        if (!user.TotpEnabled || user.TotpSecret is null)
            return false;

        var secret = protector.Unprotect(user.TotpSecret);
        return await Task.FromResult(VerifyCode(secret, code));
    }

    public bool VerifyCode(string base32Secret, string code, CancellationToken ct = default)
    {
        var secretBytes = Base32Encoding.ToBytes(base32Secret);
        var totp = new Totp(secretBytes);
        return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
    }
}
