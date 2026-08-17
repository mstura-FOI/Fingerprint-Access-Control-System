using FingerPrintSystem.Application.Services.AccountService.Dtos;
using FingerPrintSystem.Base.Dtos;
using FingerPrintSystem.Base.Services;
using Microsoft.AspNetCore.Identity;

namespace FingerPrintSystem.Application.Services.AccountService;

public interface IAccountService : IFxServiceCrud<AccountGetDto, AccountCreateDto, AccountUpdateDto,AccountDeleteDto>
{
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);

    Task<IdentityResult> SetActiveAsync(Guid id, bool active);

    Task<IdentityResult> ChangeOwnPasswordAsync(Guid userId, ChangePasswordDto dto);
}