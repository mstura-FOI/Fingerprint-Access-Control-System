namespace FingerPrintSystem.Core.Options;

public class FingerPrintSystemOptions {
    public const string SectionName = "FingerPrintSystem";

    public List<RoomOption> Rooms { get; init; } = [];
    public List<DeviceOption> Devices { get; init; } = [];

}