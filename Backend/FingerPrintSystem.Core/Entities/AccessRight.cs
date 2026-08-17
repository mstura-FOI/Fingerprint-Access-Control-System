using System;
using System.Collections.Generic;
using System.Text;
using FingerPrintSystem.Base.Entities;

namespace FingerPrintSystem.Core.Entities {
    public class AccessRight : EntityBase<Guid> {

        public Guid ApplicationUserId { get; set; }

        public Guid RoomId { get; set; }
    }
}
