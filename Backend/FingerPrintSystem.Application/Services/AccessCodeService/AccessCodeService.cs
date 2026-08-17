using System.Security.Cryptography;
using System.Text;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Options;
using FingerPrintSystem.Infrastructure.EFCore;
using FingerPrintSystem.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FingerPrintSystem.Application.Services.AccessCodeService;

public class AccessCodeService(
    ApplicationDbContext context,
    IOptions<AccessCodeOptions> options) : IAccessCodeService {
    private readonly AccessCodeOptions _opts = options.Value;

    public async Task<string> GenerateAsync(Guid userId, CancellationToken ct = default) {
        // 1. Opoziva sve prethodne aktivne kodove tog korisnika (jedan aktivan kod po korisniku).
        var active = await context.OneTimeCodes
            .Where(c => c.ApplicationUserId == userId && c.UsedAt == null && c.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var c in active) c.RevokedAt = DateTime.UtcNow;

        // 2. Kriptografski nasumičan 6-znamenkasti kod.
        var code = GenerateNumericCode(6);

        context.OneTimeCodes.Add(new OneTimeCode {
            ApplicationUserId = userId,
            CodeHash = Hash(code),
            ExpiresAt = DateTime.UtcNow.AddSeconds(_opts.LifetimeSeconds),
        });

        await context.SaveChangesAsync(ct);
        return code;
    }

    public async Task<Guid> VerifyAsync(string code, CancellationToken ct = default) {
        if (code is null || code.Length != 6 || !code.All(char.IsDigit))
            throw new InvalidCodeException("Neispravan format koda.");

        var hash = Hash(code);
        var now = DateTime.UtcNow;

        var entry = await context.OneTimeCodes
            .FirstOrDefaultAsync(c => c.CodeHash == hash, ct);

        if (entry is null || entry.UsedAt != null || entry.RevokedAt != null || now >= entry.ExpiresAt)
            throw new InvalidCodeException("Kod nije valjan ili je istekao.");

        return entry.ApplicationUserId;
    }

    public async Task ConsumeAsync(Guid userId, CancellationToken ct = default) {
        // Troši trenutno aktivni kod tog korisnika (na kraju uspješnog toka).
        var active = await context.OneTimeCodes
            .Where(c => c.ApplicationUserId == userId && c.UsedAt == null && c.RevokedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (active is not null) {
            active.UsedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
    }

    // --- helpers ---
    private static string GenerateNumericCode(int digits) {
        // Kriptografski nasumično, bez modulo-bias (rejection na granici).
        var max = (int)Math.Pow(10, digits);
        int value = RandomNumberGenerator.GetInt32(0, max);   // 0..999999
        return value.ToString().PadLeft(digits, '0');
    }

    private string Hash(string code)
        => HashingHelper.HashLowEntropy(code, Encoding.UTF8.GetBytes(_opts.Pepper));
}

public class InvalidCodeException(string message) : Exception(message);