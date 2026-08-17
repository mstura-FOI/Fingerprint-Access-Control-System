using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.RoomShiftService.Dtos;

public class RoomShiftUpdateDto : DtoTemplate
{
    public Guid RoomId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid ManagerId { get; set; }
}