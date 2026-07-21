using System.ComponentModel.DataAnnotations;

namespace smartApi.Authentication.DTOs.Auth.Requests
{
    public sealed class ResendLoginOtpRequestDto
    {
        [Required]
        public Guid ChallengeId { get; set; }
    }
}