using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccountService.Dtos;

public class AccountDeleteDto : DtoTemplate
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}