using FingerPrintSystem.Base.Dtos;
using FingerPrintSystem.Core.Enums;

namespace FingerPrintSystem.Application.Services.AttendanceService.Dtos;

public class AttendanceGetDto : DtoTemplate
{
    public Guid ApplicationUserId { get; set; }
    public DateTime AttendanceDateTime { get; set; }
    public AttendanceType AttendanceType { get; set; }
    public Guid RoomId { get; set; }
}