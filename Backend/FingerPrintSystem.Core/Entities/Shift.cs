using FingerPrintSystem.Base.Entities;

namespace FingerPrintSystem.Core.Entities;

public class Shift : EntityBase<Guid> {
    public required string Name { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public ICollection<RoomShift> RoomShifts { get; set; } = [];
}