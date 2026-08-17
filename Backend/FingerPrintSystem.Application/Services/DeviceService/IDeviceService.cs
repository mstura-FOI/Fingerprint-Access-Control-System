using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Application.Services.DeviceService.Dtos;
using FingerPrintSystem.Base.Services;

namespace FingerPrintSystem.Application.Services.DeviceService {
    public interface IDeviceService : IFxServiceSnapshot<DeviceGetDto> {
    }
}
