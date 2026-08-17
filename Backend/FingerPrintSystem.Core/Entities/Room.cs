using FingerPrintSystem.Base.Entities;
using System.ComponentModel.DataAnnotations;

namespace FingerPrintSystem.Core.Entities;

public class Room : EntityBaseSnapshot<Guid> {
    [MaxLength(256)]
    public required string Name { get; set; }

    public ICollection<RoomShift> RoomShifts { get; set; } = [];

    public ICollection<Device> Devices { get; set; } = [];

    public ICollection<AccessRight> AccessRights { get; set; } = [];
}