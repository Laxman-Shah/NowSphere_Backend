using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Auth.Requests;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can contain only letters, numbers, and underscore.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
    [Phone(ErrorMessage = "Phone number format is invalid.")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
    public string? FullName { get; set; }
}
