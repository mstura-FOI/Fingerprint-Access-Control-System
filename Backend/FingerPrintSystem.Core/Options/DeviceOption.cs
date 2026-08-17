namespace FingerPrintSystem.Core.Options;

public sealed class DeviceOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public Guid RoomId { get; set; }
    public string? CertThumbprint { get; set; }
}