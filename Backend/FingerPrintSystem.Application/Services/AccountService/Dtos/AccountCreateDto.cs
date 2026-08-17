using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccountService.Dtos;

public class AccountCreateDto : DtoCreateTemplate 
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}