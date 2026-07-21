namespace smartApi.Authentication.DTOs.Auth.Responses
{
    public sealed class LoginStep1ResponseDto
    {
        public bool TwoFactorRequired { get; set; }

        public Guid ChallengeId { get; set; }

        public string MaskedEmail { get; set; } =
            string.Empty;

        public DateTime ExpiresAt { get; set; }

        public string Message { get; set; } =
            string.Empty;
    }
}