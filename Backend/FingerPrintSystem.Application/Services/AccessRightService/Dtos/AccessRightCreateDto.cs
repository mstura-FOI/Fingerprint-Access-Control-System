using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccessRightService.Dtos;

public class AccessRightCreateDto : DtoCreateTemplate
{
    public Guid ApplicationUserId { get; set; }
    public Guid RoomId { get; set; }
}