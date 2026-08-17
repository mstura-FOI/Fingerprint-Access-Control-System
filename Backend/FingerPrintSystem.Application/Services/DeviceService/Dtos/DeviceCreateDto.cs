using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.DeviceService.Dtos;

public class DeviceCreateDto : DtoCreateTemplate
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string CertThumbprint { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
}