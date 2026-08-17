using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Entities.Identity;


namespace FingerPrintSystem.Core.Entities;

public class RoomShift : EntityBase<Guid> {
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }

    public Guid ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public Guid ManagerId { get; set; }
    public ApplicationUser? Manager { get; set; }
}