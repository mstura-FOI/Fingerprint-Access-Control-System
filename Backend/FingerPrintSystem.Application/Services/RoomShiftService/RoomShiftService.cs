using FingerPrintSystem.Application.Services.RoomShiftService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Application.Services.RoomShiftService;

public class RoomShiftService(ApplicationDbContext context)
    : FxServiceBaseCrud<RoomShift, RoomShiftGetDto, RoomShiftCreateDto,
        RoomShiftUpdateDto, RoomShiftDeleteDto, ApplicationDbContext>(context)
{
    public override async Task<RoomShiftGetDto> CreateAsync(
        RoomShiftCreateDto dto, CancellationToken ct = default)
    {
        await ValidateForeignKeys(dto.RoomId, dto.ShiftId, dto.ManagerId, ct);
        return await base.CreateAsync(dto, ct);
    }

    public override async Task<bool> UpdateAsync(
        RoomShiftUpdateDto dto, CancellationToken ct = default)
    {
        await ValidateForeignKeys(dto.RoomId, dto.ShiftId, dto.ManagerId, ct);
        return await base.UpdateAsync(dto, ct);
    }

    private async Task ValidateForeignKeys(Guid roomId, Guid shiftId, Guid managerId, CancellationToken ct)
    {
        if (!await Context.Rooms.AnyAsync(r => r.Id == roomId, ct))
            throw new InvalidOperationException($"Prostorija {roomId} ne postoji.");

        if (!await Context.Shifts.AnyAsync(s => s.Id == shiftId, ct))
            throw new InvalidOperationException($"Smjena {shiftId} ne postoji.");

        if (!await Context.Users.AnyAsync(u => u.Id == managerId, ct))
            throw new InvalidOperationException($"Korisnik (manager) {managerId} ne postoji.");
    }

    protected override IQueryable<RoomShiftGetDto> ProjectToGet(IQueryable<RoomShift> q)
    {
        return q.Select(rs => new RoomShiftGetDto
        {
            Id = rs.Id,
            RoomId = rs.RoomId,
            ShiftId = rs.ShiftId,
            ManagerId = rs.ManagerId,
            RoomName = rs.Room.Name,
            ShiftName = rs.Shift.Name
        });
    }

    protected override RoomShift CreateEntity(RoomShiftCreateDto dto)
    {
        return new RoomShift
        {
            RoomId = dto.RoomId,
            ShiftId = dto.ShiftId,
            ManagerId = dto.ManagerId,
            Room = null!,
            Shift = null!,
            Manager = null!
        };
    }

    protected override void UpdateEntity(RoomShift entity, RoomShiftUpdateDto dto)
    {
        entity.RoomId = dto.RoomId;
        entity.ShiftId = dto.ShiftId;
        entity.ManagerId = dto.ManagerId;
    }
}