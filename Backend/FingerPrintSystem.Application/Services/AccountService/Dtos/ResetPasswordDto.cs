using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Base.Dtos;

namespace FingerPrintSystem.Application.Services.AccountService.Dtos {
    public class ResetPasswordDto : DtoTemplate
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
