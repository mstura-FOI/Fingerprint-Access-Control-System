using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using System.ComponentModel.DataAnnotations;

namespace FingerPrintSystem.Core.Entities;

public class BiometricTemplate : EntityBase<Guid> {
    public Guid ApplicationUserId { get; set; }
    public required ApplicationUser ApplicationUser { get; set; }

    public required byte[] Ciphertext { get; set; }
    public required byte[] Nonce { get; set; }
    public required byte[] Tag { get; set; }
    public required byte[] WrappedDek { get; set; }
    [MaxLength(256)] public required string KekKeyId { get; set; }
}