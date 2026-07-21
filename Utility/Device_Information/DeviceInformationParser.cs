namespace smartApi.Utility.Device_Information;

public sealed class DeviceInformationParser
{
    public DeviceInformationModel Parse(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new DeviceInformationModel();
        }

        string operatingSystem = DetectOperatingSystem(userAgent);
        string browserName = DetectBrowser(userAgent);
        string deviceType = DetectDeviceType(userAgent);

        return new DeviceInformationModel
        {
            DeviceName = $"{browserName} on {operatingSystem}",
            DeviceType = deviceType,
            OperatingSystem = operatingSystem,
            BrowserName = browserName
        };
    }

    private static string DetectOperatingSystem(string value)
    {
        if (Contains(value, "Windows NT")) return "Windows";
        if (Contains(value, "Android")) return "Android";
        if (Contains(value, "iPhone") || Contains(value, "iPad")) return "iOS";
        if (Contains(value, "Mac OS X") || Contains(value, "Macintosh")) return "macOS";
        if (Contains(value, "Linux")) return "Linux";
        return "Unknown";
    }

    private static string DetectBrowser(string value)
    {
        if (Contains(value, "Edg/")) return "Edge";
        if (Contains(value, "OPR/") || Contains(value, "Opera")) return "Opera";
        if (Contains(value, "Chrome/") && !Contains(value, "Edg/")) return "Chrome";
        if (Contains(value, "Firefox/")) return "Firefox";
        if (Contains(value, "Safari/") && !Contains(value, "Chrome/")) return "Safari";
        return "Unknown";
    }

    private static string DetectDeviceType(string value)
    {
        if (Contains(value, "bot") ||
            Contains(value, "crawler") ||
            Contains(value, "spider"))
        {
            return "BOT";
        }

        if (Contains(value, "iPad") || Contains(value, "tablet"))
        {
            return "TABLET";
        }

        if (Contains(value, "Mobile") ||
            Contains(value, "Android") ||
            Contains(value, "iPhone"))
        {
            return "MOBILE";
        }

        return "DESKTOP";
    }

    private static bool Contains(string source, string value)
    {
        return source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}