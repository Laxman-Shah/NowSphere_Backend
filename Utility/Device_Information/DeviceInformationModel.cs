namespace smartApi.Utility.Device_Information;

public sealed class DeviceInformationModel
{
    public string DeviceName { get; set; } = "Unknown Device";
    public string DeviceType { get; set; } = "UNKNOWN";
    public string? OperatingSystem { get; set; }
    public string? OperatingSystemVersion { get; set; }
    public string? BrowserName { get; set; }
    public string? BrowserVersion { get; set; }
}