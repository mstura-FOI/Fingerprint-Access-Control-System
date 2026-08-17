using FingerPrintSystem.Application.Services.RoomShiftService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.RoomShiftService;

public interface
    IRoomShiftService : IFxServiceCrud<RoomShiftGetDto, RoomShiftCreateDto, RoomShiftUpdateDto, RoomShiftDeleteDto>
{
}