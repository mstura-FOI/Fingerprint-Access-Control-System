namespace FingerPrintSystem.Application.Services.AccessSessionService.Dtos;

public class AccessSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationUserId { get; set; }
    public Guid DeviceId { get; set; }
    public Guid RoomId { get; set; }
    public bool HasAccessRight { get; set; }
    public bool IsEnrollment { get; set; }
}