using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccountService.Dtos {
    public class AccountUpdateDto : DtoTemplate
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
