using FingerPrintSystem.Application.Services.AccessRightService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Application.Services.AccessRightService;

public class AccessRightService(ApplicationDbContext context)
    : FxServiceBaseCrud<AccessRight, AccessRightGetDto, AccessRightCreateDto,
        AccessRightUpdateDto, AccessRightDeleteDto, ApplicationDbContext>(context)
{
    public override async Task<AccessRightGetDto> CreateAsync(
        AccessRightCreateDto dto, CancellationToken ct = default)
    {
        await ValidateForeignKeys(dto.ApplicationUserId, dto.RoomId, ct);
        await GuardDuplicate(dto.ApplicationUserId, dto.RoomId, ct);
        return await base.CreateAsync(dto, ct);
    }

    public override async Task<bool> UpdateAsync(
        AccessRightUpdateDto dto, CancellationToken ct = default)
    {
        await ValidateForeignKeys(dto.ApplicationUserId, dto.RoomId, ct);

        var clashes = await Context.AccessRights.AnyAsync(
            a => a.Id != dto.Id && a.ApplicationUserId == dto.ApplicationUserId && a.RoomId == dto.RoomId, ct);
        if (clashes)
            throw new InvalidOperationException("Pravo pristupa za tog korisnika i prostoriju već postoji.");

        return await base.UpdateAsync(dto, ct);
    }

    private async Task ValidateForeignKeys(Guid userId, Guid roomId, CancellationToken ct)
    {
        if (!await Context.Users.AnyAsync(u => u.Id == userId, ct))
            throw new InvalidOperationException($"Korisnik {userId} ne postoji.");

        if (!await Context.Rooms.AnyAsync(r => r.Id == roomId, ct))
            throw new InvalidOperationException($"Prostorija {roomId} ne postoji.");
    }

    private async Task GuardDuplicate(Guid userId, Guid roomId, CancellationToken ct)
    {
        if (await Context.AccessRights.AnyAsync(a => a.ApplicationUserId == userId && a.RoomId == roomId, ct))
            throw new InvalidOperationException("Pravo pristupa za tog korisnika i prostoriju već postoji.");
    }

    protected override IQueryable<AccessRightGetDto> ProjectToGet(IQueryable<AccessRight> q)
    {
        return q.Select(a => new AccessRightGetDto
        {
            Id = a.Id,
            ApplicationUserId = a.ApplicationUserId,
            RoomId = a.RoomId
        });
    }

    protected override AccessRight CreateEntity(AccessRightCreateDto dto)
    {
        return new AccessRight
        {
            ApplicationUserId = dto.ApplicationUserId,
            RoomId = dto.RoomId
        };
    }

    protected override void UpdateEntity(AccessRight entity, AccessRightUpdateDto dto)
    {
        entity.ApplicationUserId = dto.ApplicationUserId;
        entity.RoomId = dto.RoomId;
    }
}