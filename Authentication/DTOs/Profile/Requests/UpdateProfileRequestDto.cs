using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Profile.Requests;

public class UpdateProfileRequestDto
{
    [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
    public string? FullName { get; set; }

    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters.")]
    [Phone(ErrorMessage = "Phone number format is invalid.")]
    public string? PhoneNumber { get; set; }
}
