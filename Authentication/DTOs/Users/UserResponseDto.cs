namespace smartApi.Authentication.DTOs.Users;

public class UserResponseDto
{
    public long UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? FullName { get; set; }

    public string AccountStatus { get; set; } = string.Empty;

    public bool EmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = null!;
}