using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.ShiftService.Dtos;

public class ShiftUpdateDto : DtoTemplate
{
    public string Name { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}