using System.ComponentModel.DataAnnotations;
using FingerPrintSystem.Base.Entities;

namespace FingerPrintSystem.Core.Entities;

public class Device : EntityBaseSnapshot<Guid>
{
    [MaxLength(256)] public required string Name { get; set; }

    [MaxLength(256)] public required string SerialNumber { get; set; }

    [MaxLength(256)] public string? IpAddress { get; set; }

    [MaxLength(64)] public required string CertThumbprint { get; set; }

    public Guid RoomId { get; set; }

    public required Room Room { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = [];
    public ICollection<FingerprintScan> FingerprintScans { get; set; } = [];
}