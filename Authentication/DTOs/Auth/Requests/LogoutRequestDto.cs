namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public class LogoutRequestDto
    {
        public string RawRefreshToken { get; set; } = null!;
    }
}