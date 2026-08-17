using FingerPrintSystem.Application.Services.ShiftService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.ShiftService;

public interface IShiftService : IFxServiceCrud<ShiftGetDto, ShiftCreateDto, ShiftUpdateDto, ShiftDeleteDto>
{
}