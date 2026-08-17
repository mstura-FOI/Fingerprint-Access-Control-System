using Microsoft.AspNetCore.Identity;

namespace FingerPrintSystem.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string EntryCode { get; set; } = string.Empty;

    public bool MustChangePassword { get; set; } = true;

    public string? TotpSecret { get; set; } 
    public bool TotpEnabled { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = [];

    public ICollection<RoomShift> ManagedRoomShifts { get; set; } = [];

    public ICollection<BiometricTemplate> BiometricTemplates { get; set; } = [];
    public ICollection<OneTimeCode> OneTimeCodes { get; set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}