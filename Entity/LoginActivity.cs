namespace smartApi.Entity
{
   

    public sealed class LoginActivity
    {
        public long LoginActivityId { get; set; }

        public long? UserId { get; set; }

        public Guid? UserSessionId { get; set; }

        public long? UserDeviceId { get; set; }

        public Guid? LoginChallengeId { get; set; }

        public string EventType { get; set; } = null!;

        public string Outcome { get; set; } = null!;

        public string? AttemptedIdentifier { get; set; }

        public string? FailureCode { get; set; }

        public string? Description { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? DeviceType { get; set; }

        public string? OperatingSystem { get; set; }

        public string? BrowserName { get; set; }

        public DateTime OccurredAt { get; set; }

        public Guid? CorrelationId { get; set; }

        public User? User { get; set; }

        public UserSession? UserSession { get; set; }

        public UserDevice? UserDevice { get; set; }

        public LoginChallenge? LoginChallenge { get; set; }
    }
}
