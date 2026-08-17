using FingerPrintSystem.Base.Entities;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Core.Enums;

namespace FingerPrintSystem.Core.Entities;

public class Attendance : EntityBaseSnapshot<Guid> {
    public Guid ApplicationUserId { get; set; }

    public required ApplicationUser ApplicationUser { get; set; }

    public DateTime AttendanceDateTime { get; set; }

    public AttendanceType AttendanceType { get; set; }

    public Guid RoomId { get; set; }

    public required Room Room { get; set; }
}