using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.RoomShiftService.Dtos;

public class RoomShiftGetDto : DtoTemplate
{
    public Guid RoomId { get; set; }
    public Guid ShiftId { get; set; }
    public Guid ManagerId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
}