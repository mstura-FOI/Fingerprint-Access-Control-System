using System.Security.Cryptography;
using FingerPrintSystem.Application.Services.AccessCodeService;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;
using FingerPrintSystem.Infrastructure.Security.ITemplateCipher;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Application.Services.EnrollmentService;

public class EnrollmentService(
    ApplicationDbContext context,
    ITemplateCipher cipher,
    IAccessCodeService accessCodes) : IEnrollmentService
{
    public async Task CompleteAsync(string code, byte[] rawTemplate, CancellationToken ct = default)
    {
        if (rawTemplate is null || rawTemplate.Length == 0)
            throw new InvalidOperationException("Prazan template.");

        var userId = await accessCodes.VerifyAsync(code, ct);

        try
        {
            var exists = await context.BiometricTemplates
                .AnyAsync(t => t.ApplicationUserId == userId, ct);
            if (exists)
                throw new InvalidOperationException(
                    "Korisnik već ima registriran otisak — ponovna registracija nije dozvoljena ovim tokom.");

            var enc = await cipher.EncryptAsync(rawTemplate, userId.ToString(), ct);

            await using var tx = await context.Database.BeginTransactionAsync(ct);

            context.BiometricTemplates.Add(new BiometricTemplate
            {
                ApplicationUserId = userId,
                Ciphertext = enc.Ciphertext,
                Nonce = enc.Nonce,
                Tag = enc.Tag,
                WrappedDek = enc.WrappedDek,
                KekKeyId = enc.KekKeyId,
                ApplicationUser = null!
            });
            await context.SaveChangesAsync(ct);

            await accessCodes.ConsumeAsync(userId, ct);

            await tx.CommitAsync(ct);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(rawTemplate);
        }
    }
}