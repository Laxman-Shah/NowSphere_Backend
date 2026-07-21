namespace smartApi.Utility.Configurations
{
    public sealed class TwoFactorAuthenticationOptions
    {
        public const string SectionName =
            "Authentication:TwoFactor";

        public int LoginOtpExpiryMinutes { get; set; } = 10;

        public int LoginChallengeExpiryMinutes { get; set; } = 10;

        public int LoginOtpMaxAttempts { get; set; } = 5;

        public int LoginOtpMaxResends { get; set; } = 3;

        public int LoginOtpResendCooldownSeconds { get; set; } = 60;

        public int MaximumFailedPasswordAttempts { get; set; } = 5;

        public int AccountLockDurationMinutes { get; set; } = 15;

        public string DummyPasswordHash { get; set; } =
            string.Empty;
    }
}
