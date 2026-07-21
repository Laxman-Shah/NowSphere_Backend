using System.Security.Claims;

namespace smartApi.Utility.Current_User;

public sealed class CurrentUserHelper
{
    private const string UserIdClaimName = "sub";
    private const string SessionIdClaimName = "sid";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetRequiredUserId()
    {
        ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

        string? value = principal?.FindFirstValue(UserIdClaimName)
            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(value, out long userId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated user identifier is missing.");
        }

        return userId;
    }

    public Guid GetRequiredSessionId()
    {
        string? value = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(SessionIdClaimName);

        if (!Guid.TryParse(value, out Guid sessionId))
        {
            throw new UnauthorizedAccessException(
                "Authenticated session identifier is missing.");
        }

        return sessionId;
    }
}