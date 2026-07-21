using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Auth.Requests;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email or username is required.")]
    [StringLength(100, ErrorMessage = "Email or username cannot be longer than 100 characters.")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    public string Password { get; set; } = string.Empty;
}
