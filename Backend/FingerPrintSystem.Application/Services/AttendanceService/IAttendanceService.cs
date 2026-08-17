using FingerPrintSystem.Application.Services.AttendanceService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.AttendanceService;

public interface IAttendanceService : IFxServiceSnapshot<AttendanceGetDto>
{
    Task CheckOutAsync(Guid managerId, AttendanceExitDto dto, CancellationToken ct);
}