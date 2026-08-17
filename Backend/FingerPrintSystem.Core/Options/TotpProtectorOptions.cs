namespace FingerPrintSystem.Core.Options;

public class TotpProtectorOptions {
    public const string SectionName = "TotpProtector";
    public string KeyBase64 { get; set; } = default!; 
}