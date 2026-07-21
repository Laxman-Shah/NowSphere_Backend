using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "OTP must contain exactly six digits."
        )]
        public string Otp { get; set; } = string.Empty;

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character."
        )]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(
            nameof(NewPassword),
            ErrorMessage = "Password confirmation does not match."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}