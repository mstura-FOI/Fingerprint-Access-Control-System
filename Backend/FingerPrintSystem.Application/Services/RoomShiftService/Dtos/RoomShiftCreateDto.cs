using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.RoomShiftService.Dtos;

public class RoomShiftCreateDto : DtoCreateTemplate
{
    public Guid RoomId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid ManagerId { get; set; }
}