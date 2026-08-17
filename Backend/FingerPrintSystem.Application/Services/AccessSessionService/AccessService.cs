using FingerPrintSystem.Application.Services.AccessCodeService;
using FingerPrintSystem.Application.Services.AccessSessionService;
using FingerPrintSystem.Application.Services.AccessSessionService.Dtos;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Enums;
using FingerPrintSystem.Infrastructure.EFCore;
using FingerPrintSystem.Infrastructure.Security.ITemplateCipher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class AccessService(
    ApplicationDbContext context,
    ITemplateCipher cipher,
    IAccessCodeService accessCodes,
    IMemoryCache cache) : IAccessService {
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromSeconds(60);
    private static string Key(Guid sessionId) => $"access-session:{sessionId}";

    public async Task<AccessPrepareDto> PrepareAsync(string code, Guid deviceId, CancellationToken ct = default) {
        var userId = await accessCodes.VerifyAsync(code, ct);

        var device = await context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId, ct)
            ?? throw new InvalidOperationException("Nepoznat uređaj.");

        var hasRight = await context.AccessRights
            .AnyAsync(a => a.ApplicationUserId == userId && a.RoomId == device.RoomId, ct);

        var template = await context.BiometricTemplates
            .FirstOrDefaultAsync(t => t.ApplicationUserId == userId, ct);

        var session = new AccessSession {
            ApplicationUserId = userId,
            DeviceId = deviceId,
            RoomId = device.RoomId,
            HasAccessRight = hasRight,
            IsEnrollment = template is null,
        };

        cache.Set(Key(session.Id), session, SessionLifetime);

        if (template is null)
            return new AccessPrepareDto { Mode = AccessMode.Enroll, SessionId = session.Id };

        var raw = await cipher.DecryptAsync(
            new EncryptedTemplate(template.Ciphertext, template.Nonce, template.Tag,
                                  template.WrappedDek, template.KekKeyId),
            aad: userId.ToString(), ct);

        return new AccessPrepareDto { Mode = AccessMode.Verify, Template = raw, SessionId = session.Id };
    }

    public async Task<bool> ResultAsync(Guid sessionId, bool matched, Guid deviceId, CancellationToken ct = default) {
        if (!cache.TryGetValue(Key(sessionId), out AccessSession? session) || session is null)
            throw new InvalidOperationException("Nevaljana ili istekla sesija.");

        cache.Remove(Key(sessionId));   // potrošena

        if (session.DeviceId != deviceId)
            throw new InvalidOperationException("Sesija pripada drugom uređaju.");

        var granted = matched && session.HasAccessRight && !session.IsEnrollment;

        var reason = !matched ? "Otisak se ne poklapa"
                   : !session.HasAccessRight ? "Nema prava na prostoriju"
                   : session.IsEnrollment ? "Enrollment, ne pristup"
                   : null;

        context.FingerprintScans.Add(new FingerprintScan {
            ApplicationUserId = session.ApplicationUserId,
            DeviceId = session.DeviceId,
            RoomId = session.RoomId,
            ScanTime = DateTime.UtcNow,
            IsSuccessful = granted,
            DenialReason = reason,
        });

        if (granted) {
            context.Attendances.Add(new Attendance
            {
                ApplicationUserId = session.ApplicationUserId,
                RoomId = session.RoomId,
                AttendanceType = AttendanceType.Enter,
                AttendanceDateTime = DateTime.UtcNow,
                ApplicationUser = null!,
                Room = null!,
            });
            await accessCodes.ConsumeAsync(session.ApplicationUserId, ct);
        }

        await context.SaveChangesAsync(ct);
        return granted;
    }
}