namespace smartApi.Utility.PasswordHasher_Security;

public interface IPasswordHasher
{
    PasswordHashResult HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}