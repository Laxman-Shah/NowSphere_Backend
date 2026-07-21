namespace smartApi.Authentication.DTOs.Auth.Requests
{

    public sealed class RevokeSessionRequest
    {
        public string? Reason { get; init; }
    }
}
