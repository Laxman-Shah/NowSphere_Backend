namespace smartApi.Authentication.DTOs.Auth.Requests;

public sealed class LoginActivityQueryRequestDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? EventType { get; set; }
    public string? Outcome { get; set; }
}