using FingerPrintSystem.Base.Entities;
using System.ComponentModel.DataAnnotations;

namespace FingerPrintSystem.Core.Entities;

public class FingerprintScan : EntityBaseSnapshot<Guid>
{
    public Guid? ApplicationUserId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid RoomId { get; set; }          
    public DateTime ScanTime { get; set; }
    public bool IsSuccessful { get; set; }
    [MaxLength(256)] public string? DenialReason { get; set; }
}