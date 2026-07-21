namespace smartApi.Utility.PasswordHasher_Security;

public class PasswordHashResult
{
    public string Hash { get; set; } = string.Empty;

    public string Salt { get; set; } = string.Empty;

    public string Algorithm { get; set; } = string.Empty;
}