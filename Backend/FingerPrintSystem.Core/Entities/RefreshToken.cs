using System.ComponentModel.DataAnnotations;
using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Entities.Identity;

namespace FingerPrintSystem.Core.Entities;

public class RefreshToken : EntityBase<Guid>
{
    public Guid ApplicationUserId { get; set; }
    public required ApplicationUser ApplicationUser { get; set; }

    [MaxLength(128)] public required string TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
    [MaxLength(64)] public string? RevokedByIp { get; set; }

    [MaxLength(256)]
    public string? RevokedReason { get; set; } // "rotated" | "reuse-detected" | "logout" | "rights-revoked"

    [MaxLength(128)] public string? ReplacedByTokenHash { get; set; }

    [MaxLength(64)] public string? CreatedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;
}