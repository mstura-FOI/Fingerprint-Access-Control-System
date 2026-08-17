using FingerPrintSystem.Application.Services.AccessRightService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.AccessRightService;

internal interface IAccessRightService : IFxServiceCrud<AccessRightGetDto, AccessRightCreateDto, AccessRightUpdateDto,
    AccessRightDeleteDto>
{
}