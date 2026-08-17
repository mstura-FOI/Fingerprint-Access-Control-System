namespace FingerPrintSystem.Application.Services.AccessSessionService.Dtos;

public class AccessPrepareDto
{
    public required AccessMode Mode { get; set; }
    public byte[]? Template { get; set; } 
    public Guid SessionId { get; set; }
}