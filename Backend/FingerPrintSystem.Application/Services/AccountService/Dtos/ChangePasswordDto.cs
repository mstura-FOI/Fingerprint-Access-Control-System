using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccountService.Dtos;

public class ChangePasswordDto : DtoTemplate
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}