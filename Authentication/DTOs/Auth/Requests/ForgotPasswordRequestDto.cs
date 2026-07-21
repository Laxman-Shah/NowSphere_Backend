using System.ComponentModel.DataAnnotations;
namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

    }
}
