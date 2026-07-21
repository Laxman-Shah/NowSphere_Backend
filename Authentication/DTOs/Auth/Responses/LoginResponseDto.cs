namespace smartApi.Authentication.DTOs.Auth.Responses
{
    public class LoginResponseDto
    {
        public long UserId { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? FullName { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime AccessTokenExpiresAt { get; set; }

        public DateTime RefreshTokenExpiresAt { get; set; }

        public string Message { get; set; } = "Login successful.";
    }
}