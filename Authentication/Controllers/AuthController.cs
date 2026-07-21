using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using smartApi.Authentication.DTOs.Auth.Requests;
using smartApi.Authentication.DTOs.Auth.Responses;
using smartApi.Authentication.Services.Interfaces;
using System.Security.Claims;

namespace smartApi.Controllers
{
    /// <summary>
    /// Handles user authentication operations including registration,
    /// registration email verification, two-factor login, login OTP
    /// verification, login OTP resend, refresh-token rotation, logout,
    /// password recovery, user-session management, and login activities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // ============================================================
        // DEPENDENCIES
        // ============================================================

        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;


        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        // ============================================================
        // REGISTER
        // POST: /api/auth/register
        // ============================================================

        /// <summary>
        /// Creates a new pending user registration or resumes an
        /// existing pending registration.
        /// </summary>
        /// <remarks>
        /// This endpoint:
        ///
        /// 1. Validates the registration request.
        /// 2. Creates or updates a Pending user account.
        /// 3. Creates a registration OTP.
        /// 4. Sends the OTP to the user's email address.
        /// 5. Returns the pending user's information.
        ///
        /// This endpoint does not activate the account.
        /// The account is activated through the registration OTP endpoint.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register(
            [FromBody] RegisterRequestDto request)
        {
            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            RegisterResponseDto response =
                await _authService.RegisterAsync(
                    request,
                    ipAddress,
                    userAgent);

            return Ok(response);
        }


        // ============================================================
        // VERIFY REGISTRATION OTP
        // POST: /api/auth/register/verify-otp
        // ============================================================

        /// <summary>
        /// Verifies the email OTP created during registration.
        /// </summary>
        /// <remarks>
        /// A successful verification:
        ///
        /// 1. Marks the registration OTP as used.
        /// 2. Sets EmailVerified to true.
        /// 3. Changes AccountStatus from Pending to Active.
        ///
        /// This endpoint is separate from login two-factor
        /// authentication.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("register/verify-otp")]
        public async Task<
            ActionResult<VerifyRegisterOtpResponseDto>>
            VerifyRegisterOtp(
                [FromBody]
                VerifyRegisterOtpRequestDto request)
        {
            VerifyRegisterOtpResponseDto response =
                await _authService.VerifyRegisterOtpAsync(
                    request);

            return Ok(response);
        }


        // ============================================================
        // LOGIN STEP 1
        // POST: /api/auth/login
        // ============================================================

        /// <summary>
        /// Verifies the user's email or username and password, then
        /// creates a login two-factor authentication challenge.
        /// </summary>
        /// <remarks>
        /// This endpoint:
        ///
        /// 1. Validates the login request.
        /// 2. Finds the user and credential.
        /// 3. Validates the account state.
        /// 4. Checks temporary account lockout.
        /// 5. Verifies the password.
        /// 6. Creates a login challenge.
        /// 7. Generates and sends a login OTP.
        /// 8. Returns the ChallengeId.
        ///
        /// This endpoint does not create an authenticated UserSession.
        /// The session and tokens are created only after successful
        /// login OTP verification.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("login-password-policy")]
        public async Task<ActionResult<LoginStep1ResponseDto>> Login(
            [FromBody] LoginRequestDto request,
            CancellationToken cancellationToken)
        {
            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            LoginStep1ResponseDto response =
                await _authService.LoginAsync(
                    request,
                    ipAddress,
                    userAgent,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // LOGIN STEP 2
        // POST: /api/auth/login/verify-otp
        // ============================================================

        /// <summary>
        /// Verifies the login OTP and completes two-factor
        /// authentication.
        /// </summary>
        /// <remarks>
        /// A successful operation:
        ///
        /// 1. Validates the ChallengeId and OTP.
        /// 2. Loads the pending login challenge.
        /// 3. Revalidates the user account state.
        /// 4. Verifies the login OTP.
        /// 5. Marks the OTP as used.
        /// 6. Marks the challenge as completed.
        /// 7. Finds or creates the user device.
        /// 8. Creates the authenticated UserSession.
        /// 9. Creates a refresh token connected to the session.
        /// 10. Creates an access token containing the sid claim.
        /// 11. Records successful login activity.
        /// 12. Returns the authenticated login response.
        ///
        /// This is the only normal login endpoint that creates a
        /// UserSession and issues tokens.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("login/verify-otp")]
        [EnableRateLimiting("login-otp-policy")]
        public async Task<ActionResult<LoginResponseDto>>
            VerifyLoginOtp(
                [FromBody]
                VerifyLoginOtpRequestDto request,
                CancellationToken cancellationToken)
        {
            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            LoginResponseDto response =
                await _authService.VerifyLoginOtpAsync(
                    request,
                    ipAddress,
                    userAgent,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // RESEND LOGIN OTP
        // POST: /api/auth/login/resend-otp
        // ============================================================

        /// <summary>
        /// Generates and sends a new OTP for an existing pending login
        /// challenge.
        /// </summary>
        /// <remarks>
        /// This endpoint:
        ///
        /// 1. Validates the ChallengeId.
        /// 2. Checks that the challenge is Pending.
        /// 3. Checks that the challenge has not expired.
        /// 4. Enforces the resend cooldown.
        /// 5. Enforces the maximum resend count.
        /// 6. Revokes the previous active login OTP.
        /// 7. Creates and sends a new login OTP.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("login/resend-otp")]
        [EnableRateLimiting("login-resend-policy")]
        public async Task<
            ActionResult<ResendLoginOtpResponseDto>>
            ResendLoginOtp(
                [FromBody]
                ResendLoginOtpRequestDto request,
                CancellationToken cancellationToken)
        {
            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            ResendLoginOtpResponseDto response =
                await _authService.ResendLoginOtpAsync(
                    request,
                    ipAddress,
                    userAgent,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // REFRESH TOKEN
        // POST: /api/auth/refresh-token
        // ============================================================

        /// <summary>
        /// Rotates a valid refresh token and returns a replacement
        /// access token and refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint:
        ///
        /// 1. Validates the submitted refresh token.
        /// 2. Validates the related UserSession.
        /// 3. Validates the related UserDevice.
        /// 4. Validates the associated user account.
        /// 5. Creates a new access token with the same sid claim.
        /// 6. Creates a new refresh token under the same session.
        /// 7. Revokes the previous refresh token.
        /// 8. Connects the old token to its replacement.
        ///
        /// Normal refresh-token rotation does not require another OTP.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDto>>
            RefreshToken(
                [FromBody]
                RefreshTokenRequestDto request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(
                    request.RefreshToken))
            {
                return BadRequest(new
                {
                    Message =
                        "Refresh token is required."
                });
            }

            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            LoginResponseDto response =
                await _authService.RefreshTokenAsync(
                    request.RefreshToken,
                    ipAddress,
                    userAgent);

            return Ok(response);
        }


        // ============================================================
        // EXISTING REFRESH-TOKEN LOGOUT
        // POST: /api/auth/logout
        // ============================================================

        /// <summary>
        /// Revokes the submitted refresh token belonging to the
        /// authenticated user.
        /// </summary>
        /// <remarks>
        /// This is the existing refresh-token-based logout endpoint.
        ///
        /// A valid Bearer access token and raw refresh token are
        /// required.
        ///
        /// The new session-aware logout endpoint is:
        ///
        /// POST /api/auth/sessions/current/logout
        /// </remarks>
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<LogoutResponseDto>> Logout(
            [FromBody] LogoutRequestDto request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(
                    request.RawRefreshToken))
            {
                return BadRequest(new
                {
                    Message =
                        "Refresh token is required."
                });
            }

            string? userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier
                )?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning(
                    "Logout request rejected because the " +
                    "authenticated user ID claim was missing."
                );

                return Unauthorized(new
                {
                    Message =
                        "Invalid authenticated user."
                });
            }

            string? ipAddress =
                GetClientIpAddress();

            LogoutResponseDto response =
                await _authService.RevokeTokenAsync(
                    request.RawRefreshToken,
                    userId,
                    ipAddress);

            return Ok(response);
        }


        // ============================================================
        // GET CURRENT USER SESSIONS
        // GET: /api/auth/sessions
        // ============================================================

        /// <summary>
        /// Returns all login sessions belonging to the authenticated
        /// user.
        /// </summary>
        /// <remarks>
        /// The response includes:
        ///
        /// - Session ID
        /// - Device name and type
        /// - Operating system
        /// - Browser
        /// - IP address
        /// - Session status
        /// - Login time
        /// - Last activity time
        /// - Session expiration
        /// - Current-session identification
        /// - Trusted-device state
        ///
        /// The user ID is obtained from the authenticated access token.
        /// </remarks>
        [Authorize]
        [HttpGet("sessions")]
        public async Task<
            ActionResult<IReadOnlyCollection<UserSessionResponse>>>
            GetMySessions(
                CancellationToken cancellationToken)
        {
            IReadOnlyCollection<UserSessionResponse> response =
                await _authService.GetMySessionsAsync(
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // REVOKE ONE SELECTED SESSION
        // DELETE: /api/auth/sessions/{sessionId}
        // ============================================================

        /// <summary>
        /// Revokes one selected session belonging to the authenticated
        /// user.
        /// </summary>
        /// <remarks>
        /// The service:
        ///
        /// 1. Reads the authenticated user ID.
        /// 2. Verifies that the session belongs to that user.
        /// 3. Sets the session status to REVOKED.
        /// 4. Sets the session revocation information.
        /// 5. Revokes active refresh tokens belonging to the session.
        /// 6. Records the session-revocation activity.
        ///
        /// A user cannot revoke another user's session.
        /// </remarks>
        [Authorize]
        [HttpDelete("sessions/{sessionId:guid}")]
        public async Task<IActionResult> RevokeSession(
            Guid sessionId,
            [FromBody] RevokeSessionRequest? request,
            CancellationToken cancellationToken)
        {
            await _authService.RevokeSessionAsync(
                sessionId,
                request?.Reason,
                cancellationToken);

            return Ok(new
            {
                Message =
                    "The selected session was revoked successfully."
            });
        }


        // ============================================================
        // REVOKE ALL OTHER SESSIONS
        // DELETE: /api/auth/sessions/others
        // ============================================================

        /// <summary>
        /// Revokes all active sessions except the current session.
        /// </summary>
        /// <remarks>
        /// The current session is identified using the sid claim and
        /// remains active.
        ///
        /// Active refresh tokens belonging to all other sessions are
        /// revoked.
        /// </remarks>
        [Authorize]
        [HttpDelete("sessions/others")]
        public async Task<IActionResult> RevokeOtherSessions(
            CancellationToken cancellationToken)
        {
            await _authService.RevokeOtherSessionsAsync(
                cancellationToken);

            return Ok(new
            {
                Message =
                    "All other active sessions were revoked successfully."
            });
        }


        // ============================================================
        // REVOKE ALL SESSIONS
        // DELETE: /api/auth/sessions/all
        // ============================================================

        /// <summary>
        /// Revokes all active sessions belonging to the authenticated
        /// user, including the current session.
        /// </summary>
        /// <remarks>
        /// Active refresh tokens belonging to all sessions are revoked.
        ///
        /// The existing access token may remain cryptographically valid
        /// until its normal expiration unless request-time session
        /// validation is added. However, the revoked sessions cannot
        /// obtain replacement access tokens through refresh.
        /// </remarks>
        [Authorize]
        [HttpDelete("sessions/all")]
        public async Task<IActionResult> RevokeAllSessions(
            CancellationToken cancellationToken)
        {
            await _authService.RevokeAllSessionsAsync(
                cancellationToken);

            return Ok(new
            {
                Message =
                    "All active sessions were revoked successfully."
            });
        }


        // ============================================================
        // LOG OUT CURRENT SESSION
        // POST: /api/auth/sessions/current/logout
        // ============================================================

        /// <summary>
        /// Logs out the session making the current authenticated
        /// request.
        /// </summary>
        /// <remarks>
        /// This is the new session-aware logout endpoint.
        ///
        /// A valid Bearer access token containing the authenticated user
        /// ID and sid session claim is required.
        ///
        /// No request body is required.
        ///
        /// The service:
        ///
        /// 1. Reads the user ID from the authenticated token.
        /// 2. Reads the current session ID from the sid claim.
        /// 3. Verifies that the session belongs to the user.
        /// 4. Sets the session status to LOGGED_OUT.
        /// 5. Revokes active refresh tokens belonging to the session.
        /// 6. Records successful logout activity.
        /// </remarks>
        [Authorize]
        [HttpPost("sessions/current/logout")]
        public async Task<IActionResult> LogoutCurrentSession(
            CancellationToken cancellationToken)
        {
            await _authService.LogoutCurrentSessionAsync(
                cancellationToken);

            return Ok(new
            {
                Message =
                    "Current session logged out successfully."
            });
        }


        // ============================================================
        // GET LOGIN ACTIVITY HISTORY
        // GET: /api/auth/login-activities
        // ============================================================

        /// <summary>
        /// Returns paginated authentication and session activity for
        /// the authenticated user.
        /// </summary>
        /// <remarks>
        /// Example:
        ///
        /// GET /api/auth/login-activities?page=1&amp;pageSize=20
        ///
        /// Optional query filters:
        ///
        /// - eventType
        /// - outcome
        ///
        /// The authenticated user ID is obtained from the access token.
        /// The client does not submit a user ID.
        /// </remarks>
        [Authorize]
        [HttpGet("login-activities")]
        public async Task<
            ActionResult<PagedLoginActivityResponseDto>>
            GetMyLoginActivities(
                [FromQuery]
                LoginActivityQueryRequestDto request,
                CancellationToken cancellationToken)
        {
            PagedLoginActivityResponseDto response =
                await _authService.GetMyLoginActivitiesAsync(
                    request,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // FORGOT PASSWORD
        // POST: /api/auth/forgot-password
        // ============================================================

        /// <summary>
        /// Creates and sends a password-reset OTP when an eligible
        /// account exists for the submitted email address.
        /// </summary>
        /// <remarks>
        /// This endpoint always returns the same generic response,
        /// whether or not the submitted email belongs to an account.
        /// This prevents account-email enumeration.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [EnableRateLimiting("forgot-password-policy")]
        public async Task<ActionResult<ForgotPasswordResponseDto>>
            ForgotPassword(
                [FromBody]
                ForgotPasswordRequestDto request,
                CancellationToken cancellationToken)
        {
            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            ForgotPasswordResponseDto response =
                await _authService.ForgotPasswordAsync(
                    request,
                    ipAddress,
                    userAgent,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // RESET PASSWORD
        // POST: /api/auth/reset-password
        // ============================================================

        /// <summary>
        /// Verifies a password-reset OTP and changes the user's
        /// password.
        /// </summary>
        /// <remarks>
        /// After successful password reset:
        ///
        /// 1. The password-reset OTP is marked as used.
        /// 2. Other active password-reset OTPs are revoked.
        /// 3. The user password is updated.
        /// 4. All refresh tokens are revoked.
        /// 5. Active sessions should be revoked.
        /// 6. Pending login challenges are revoked.
        /// 7. Active login OTPs are revoked.
        /// 8. The user must log in again.
        /// </remarks>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [EnableRateLimiting("reset-password-policy")]
        public async Task<ActionResult<ResetPasswordResponseDto>>
            ResetPassword(
                [FromBody]
                ResetPasswordRequestDto request,
                CancellationToken cancellationToken)
        {
            if (request is null)
            {
                return BadRequest(new
                {
                    Error =
                        "Request body is required."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x =>
                        x.Value?.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field =
                            x.Key,

                        Errors =
                            x.Value?
                                .Errors
                                .Select(error =>
                                    error.ErrorMessage)
                    })
                    .ToList();

                return BadRequest(new
                {
                    Error =
                        "Validation failed.",

                    Details =
                        errors
                });
            }

            string? ipAddress =
                GetClientIpAddress();

            string? userAgent =
                GetUserAgent();

            ResetPasswordResponseDto response =
                await _authService.ResetPasswordAsync(
                    request,
                    ipAddress,
                    userAgent,
                    cancellationToken);

            return Ok(response);
        }


        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        /// <summary>
        /// Returns the remote client IP address observed by ASP.NET
        /// Core.
        /// </summary>
        private string? GetClientIpAddress()
        {
            return HttpContext
                .Connection
                .RemoteIpAddress?
                .ToString();
        }


        /// <summary>
        /// Returns the current request's User-Agent.
        /// </summary>
        private string? GetUserAgent()
        {
            string userAgent =
                HttpContext
                    .Request
                    .Headers
                    .UserAgent
                    .ToString();

            return string.IsNullOrWhiteSpace(
                userAgent)
                    ? null
                    : userAgent;
        }
    }
}