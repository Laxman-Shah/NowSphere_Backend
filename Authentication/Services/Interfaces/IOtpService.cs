namespace smartApi.Authentication.Services.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp();

        string HashOtp(string otp);

        bool VerifyOtp(string otp, string otpHash);
    }
}