namespace smartApi.Utility.Configurations;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; }

    public string SenderName { get; set; } = "NowSphere";

    public string SenderEmail { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;

    public string WebAppBaseUrl { get; set; } = string.Empty;
}