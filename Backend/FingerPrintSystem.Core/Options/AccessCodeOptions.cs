namespace FingerPrintSystem.Core.Options;

public class AccessCodeOptions
{
    public const string SectionName = "AccessCode";

    public int LifetimeSeconds { get; set; } = 90;

    public string Pepper { get; set; } = string.Empty;
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutSeconds { get; set; } = 300;
}