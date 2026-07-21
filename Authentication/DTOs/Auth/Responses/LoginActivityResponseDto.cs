namespace smartApi.Authentication.DTOs.Auth.Responses;

public sealed class LoginActivityResponseDto
{
    public long LoginActivityId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceType { get; set; }
    public string? OperatingSystem { get; set; }
    public string? BrowserName { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsCurrentSession { get; set; }
}
