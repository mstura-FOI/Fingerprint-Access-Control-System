using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FingerPrintSystem.Base.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class FxControllerBase : ControllerBase;
}
