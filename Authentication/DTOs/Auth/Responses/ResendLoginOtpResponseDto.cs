namespace smartApi.Authentication.DTOs.Auth.Responses
{
    public sealed class ResendLoginOtpResponseDto
    {
        public Guid ChallengeId { get; set; }

        public string MaskedEmail { get; set; } =
            string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime NextResendAvailableAt
        { get; set; }

        public int RemainingResends { get; set; }

        public string Message { get; set; } =
            string.Empty;
    }
}