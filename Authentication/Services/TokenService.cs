using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using smartApi.Utility.Configurations;
using smartApi.Entity;
using smartApi.Authentication.Services.Interfaces;



namespace smartApi.Authentication.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(
        User user,
        IList<string> roles,
        out DateTime accessTokenExpiresAt,
        Guid? sessionId = null
    )
    {
        accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            _jwtSettings.AccessTokenMinutes
        );

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("account_status", user.AccountStatus.ToString())
        };

        if (sessionId.HasValue)
        {
            claims.Add(
                new Claim("sid", sessionId.Value.ToString())
            );
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: accessTokenExpiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }

    public RefreshToken CreateRefreshTokenEntity(
        long userId,
        string rawRefreshToken,
        string? ipAddress,
        string? userAgent,
        string? tokenFamilyId = null,
        Guid? userSessionId = null
    )
    {
        return new RefreshToken
        {
            UserId = userId,
            UserSessionId = userSessionId,
            TokenHash = HashToken(rawRefreshToken),
            TokenFamilyId = tokenFamilyId ?? Guid.NewGuid().ToString("N"),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            IsRevoked = false
        };
    }
}
