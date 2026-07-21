namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public class VerifyRegisterOtpRequestDto
    {
        public long UserId { get; set; }

        public string Otp { get; set; } = null!;
    }
}