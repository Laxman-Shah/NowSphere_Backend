namespace smartApi.Authentication.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(
            string toEmail,
            string otp,
            string purpose,
            CancellationToken cancellationToken = default);

        Task SendEmailVerificationSuccessAsync(
            string toEmail,
            string fullName,
            CancellationToken cancellationToken = default);

        Task SendLoginSuccessEmailAsync(
            string toEmail,
            string receiverName,
            DateTime loginTime,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default);

        Task SendPasswordChangedEmailAsync(
            string email,
            string displayName,
            DateTime changedAt,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default);
    }
}