using System.Security.Cryptography;
using System.Text;

namespace smartApi.Utility.Device_Information;

public sealed class DeviceFingerprintHelper
{
    public string CreateHash(string? clientDeviceId, string? userAgent)
    {
        string normalizedDeviceId = string.IsNullOrWhiteSpace(clientDeviceId)
            ? "MISSING_DEVICE_ID"
            : clientDeviceId.Trim();

        string normalizedUserAgent = string.IsNullOrWhiteSpace(userAgent)
            ? "MISSING_USER_AGENT"
            : userAgent.Trim();

        string input = $"{normalizedDeviceId}|{normalizedUserAgent}";
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}