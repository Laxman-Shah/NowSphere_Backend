using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public sealed class VerifyLoginOtpRequestDto
    {
        [Required]
        public Guid ChallengeId { get; set; }

        [Required]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage =
                "OTP must contain exactly 6 digits."
        )]
        public string Otp { get; set; } =
            string.Empty;
    }
}