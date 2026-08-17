using System;
using System.Collections.Generic;
using System.Text;

namespace FingerPrintSystem.Core.Options {
    public class EnvelopeCipherOptions {
        public const string SectionName = "Envelope";
        public string KekBase64 { get; set; } = string.Empty;
        public string KekVersion { get; set; } = "local-v1";
    }
}
