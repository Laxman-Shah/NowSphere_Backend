using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using smartApi.Authentication.Services.Interfaces;

namespace smartApi.Authentication.Services
{
    public class OtpService : IOtpService
    {
        public string GenerateOtp()
        {
            var number = RandomNumberGenerator.GetInt32(100000, 1000000);

            return number.ToString(CultureInfo.InvariantCulture);
        }

        public string HashOtp(string otp)
        {
            var normalizedOtp = NormalizeOtp(otp);
            var otpBytes = Encoding.UTF8.GetBytes(normalizedOtp);
            var hashBytes = SHA256.HashData(otpBytes);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyOtp(string otp, string otpHash)
        {
            if (string.IsNullOrWhiteSpace(otp) ||
                string.IsNullOrWhiteSpace(otpHash))
            {
                return false;
            }

            var incomingOtpHash = HashOtp(otp);
            var incomingBytes = Encoding.UTF8.GetBytes(incomingOtpHash);
            var storedBytes = Encoding.UTF8.GetBytes(otpHash.Trim());

            if (incomingBytes.Length != storedBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                incomingBytes,
                storedBytes
            );
        }

        private static string NormalizeOtp(string otp)
        {
            // Keep only ASCII digits so Unicode lookalike digits cannot
            // pass validation while hashing to a different value.
            return string.Create(
                otp.Count(static c => c is >= '0' and <= '9'),
                otp,
                static (span, value) =>
                {
                    var index = 0;

                    foreach (var character in value)
                    {
                        if (character is >= '0' and <= '9')
                        {
                            span[index++] = character;
                        }
                    }
                }
            );
        }
    }
}
