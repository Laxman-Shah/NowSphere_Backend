namespace smartApi.Utility.Http_Request;

public sealed class RequestInformationHelper
{
    private const string DeviceIdHeaderName = "X-Device-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestInformationHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?
            .ToString();
    }

    public string? GetUserAgent()
    {
        string? value = _httpContextAccessor.HttpContext?
            .Request.Headers.UserAgent.ToString();

        return Limit(value, 1000);
    }

    public string? GetClientDeviceId()
    {
        string? value = _httpContextAccessor.HttpContext?
            .Request.Headers[DeviceIdHeaderName].ToString();

        return Limit(value, 200);
    }

    private static string? Limit(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();

        return trimmed.Length <= maximumLength
            ? trimmed
            : trimmed[..maximumLength];
    }
}