using smartApi.Entity;

namespace smartApi.Authentication.Services.Interfaces
{

    public interface ITokenService
    {
        string GenerateAccessToken(
            User user,
            IList<string> roles,
            out DateTime accessTokenExpiresAt,
            Guid? sessionId = null
        );

        string GenerateRefreshToken();

        string HashToken(string token);

        RefreshToken CreateRefreshTokenEntity(
            long userId,
            string rawRefreshToken,
            string? ipAddress,
            string? userAgent,
            string? tokenFamilyId = null,
            Guid? userSessionId = null
        );
    }
}
