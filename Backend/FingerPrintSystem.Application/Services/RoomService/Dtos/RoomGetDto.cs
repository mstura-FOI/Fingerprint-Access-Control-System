using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.RoomService.Dtos;

public class RoomGetDto : DtoTemplate
{
    public string? Name { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
}