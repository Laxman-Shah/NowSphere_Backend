using smartApi.Authentication.DTOs.Users;

namespace smartApi.Authentication.DTOs.Auth.Responses;

public class RegisterResponseDto
{
    public UserResponseDto User { get; set; } = new();
}
