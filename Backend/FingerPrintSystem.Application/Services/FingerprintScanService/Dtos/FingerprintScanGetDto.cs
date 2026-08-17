using FingerPrintSystem.Base.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FingerPrintSystem.Application.Services.FingerprintScanService.Dtos {
    public class FingerprintScanGetDto : DtoTemplate {
        public Guid? ApplicationUserId { get; set; }
        public Guid DeviceId { get; set; }
        public Guid? RoomId { get; set; }
        public DateTime? ScanTime { get; set; }
        public bool? IsSuccessful { get; set; }
        public string? DenialReason { get; set; }
    }
}
