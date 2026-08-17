using FingerPrintSystem.Application.Services.RoomService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;

namespace FingerPrintSystem.Application.Services.RoomService;

public class RoomService(ApplicationDbContext context)
    : FxServiceBaseSnapshot<Room, RoomGetDto, ApplicationDbContext>(context), IRoomService
{
    protected override IQueryable<RoomGetDto> ProjectToGet(IQueryable<Room> query)
    {
        return query.Select(r => new RoomGetDto
        {
            Id = r.Id,
            Name = r.Name,
            CreatedAt = r.CreatedAt
        });
    }
}