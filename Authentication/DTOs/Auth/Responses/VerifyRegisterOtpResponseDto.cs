namespace smartApi.Authentication.DTOs.Auth.Responses
{
    public class VerifyRegisterOtpResponseDto
    {
        public long UserId { get; set; }

        public string Email { get; set; } = null!;

        public string AccountStatus { get; set; } = null!;

        public bool EmailVerified { get; set; }

        public string Message { get; set; } = null!;
    }
}