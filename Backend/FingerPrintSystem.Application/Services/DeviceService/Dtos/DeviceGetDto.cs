using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.DeviceService.Dtos;

public class DeviceGetDto : DtoTemplate
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
}