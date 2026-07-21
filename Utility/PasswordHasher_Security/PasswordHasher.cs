namespace smartApi.Utility.PasswordHasher_Security;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public PasswordHashResult HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        string salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);

        string hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        return new PasswordHashResult
        {
            Hash = hash,
            Salt = salt,
            Algorithm = "BCrypt"
        };
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}