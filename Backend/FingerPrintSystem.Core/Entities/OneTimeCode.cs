using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FingerPrintSystem.Core.Entities;

public class OneTimeCode : EntityBase<Guid> {
    public Guid ApplicationUserId { get; set; }

    [MaxLength(128)] public required string CodeHash { get; set; }   

    public CodePurpose Purpose { get; set; }

    public Guid? RoomId { get; set; }     

    public DateTime ExpiresAt { get; set; } 
    public DateTime? UsedAt { get; set; }  
    public DateTime? RevokedAt { get; set; } 

    public bool IsUsable =>
        UsedAt is null && RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}