using FingerPrintSystem.Application.Services.AttendanceService.Dtos;
using FingerPrintSystem.Base.Services;
using FingerPrintSystem.Core.Entities;
using FingerPrintSystem.Core.Enums;
using FingerPrintSystem.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;

namespace FingerPrintSystem.Application.Services.AttendanceService;

public class AttendanceService(ApplicationDbContext context)
    : FxServiceBaseSnapshot<Attendance, AttendanceGetDto, ApplicationDbContext>(context),
        IAttendanceService
{
    protected override IQueryable<AttendanceGetDto> ProjectToGet(IQueryable<Attendance> query)
    {
        return query
            .OrderByDescending(a => a.AttendanceDateTime)
            .Select(a => new AttendanceGetDto
            {
                Id = a.Id,
                ApplicationUserId = a.ApplicationUserId,
                AttendanceDateTime = a.AttendanceDateTime,
                AttendanceType = a.AttendanceType,
                RoomId = a.RoomId
            });
    }

    public async Task CheckOutAsync(Guid managerId, AttendanceExitDto dto, CancellationToken ct) {
        // 1. Je li ovaj korisnik MANAGER te sobe? (RoomShift.ManagerId)
        var isManagerOfRoom = await context.RoomShifts.AnyAsync(
            rs => rs.RoomId == dto.RoomId && rs.ManagerId == managerId, ct);

        if (!isManagerOfRoom)
            throw new UnauthorizedAccessException("Niste zaduženi za tu prostoriju.");

        // 2. Mora postojati otvoreni Enter (Enter bez uparenog Exita) za tog korisnika
        var lastEnter = await context.Attendances
            .Where(a => a.ApplicationUserId == dto.ApplicationUserId
                        && a.RoomId == dto.RoomId
                        && a.AttendanceType == AttendanceType.Enter)
            .OrderByDescending(a => a.AttendanceDateTime)
            .FirstOrDefaultAsync(ct);

        if (lastEnter is null)
            throw new InvalidOperationException("Nema zabilježenog ulaska za tu osobu u toj prostoriji.");

        var alreadyExited = await context.Attendances.AnyAsync(a =>
            a.ApplicationUserId == dto.ApplicationUserId && a.RoomId == dto.RoomId &&
            a.AttendanceType == AttendanceType.Exit &&
            a.AttendanceDateTime > lastEnter.AttendanceDateTime, ct);

        if (alreadyExited)
            throw new InvalidOperationException("Izlazak je već zabilježen.");

        // 3. Server postavlja vrijeme
        context.Attendances.Add(new Attendance
        {
            ApplicationUserId = dto.ApplicationUserId,
            RoomId = dto.RoomId,
            AttendanceType = AttendanceType.Exit,
            AttendanceDateTime = DateTime.UtcNow,
            ApplicationUser = null!,
            Room = null!, 
            CreatedBy = managerId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(ct);
    }
}