using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smartApi.Authentication.DTOs.Profile.Requests;
using smartApi.Authentication.DTOs.Profile.Responses;
using smartApi.Authentication.Services.Interfaces;

namespace smartApi.Controllers;

/// <summary>
/// Handles user profile and account management operations for
/// authenticated users, including profile retrieval, profile updates,
/// and password changes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    // ============================================================
    // DEPENDENCIES
    // ============================================================

    private readonly IProfileService _profileService;
    private readonly ILogger<ProfileController> _logger;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public ProfileController(
        IProfileService profileService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    // ============================================================
    // GET PROFILE
    // GET: /api/profile
    // ============================================================

    /// <summary>
    /// Returns the authenticated user's profile information,
    /// active roles, and a short security summary.
    /// </summary>
    /// <remarks>
    /// This endpoint:
    ///
    /// 1. Reads the authenticated user ID from the JWT.
    /// 2. Reads the current session ID from the JWT sid claim.
    /// 3. Loads the user's profile information.
    /// 4. Loads the user's active roles.
    /// 5. Counts active sessions.
    /// 6. Returns the current session summary when available.
    ///
    /// The user ID and session ID are obtained from the authenticated
    /// access token. The client does not submit these values.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<MyProfileResponseDto>> GetMyProfile(
        CancellationToken cancellationToken)
    {
        MyProfileResponseDto response =
            await _profileService.GetMyProfileAsync(cancellationToken);

        return Ok(response);
    }

    // ============================================================
    // UPDATE PROFILE
    // PUT: /api/profile
    // ============================================================

    /// <summary>
    /// Updates only the authenticated user's allowed profile fields.
    /// </summary>
    /// <remarks>
    /// This endpoint allows updating:
    ///
    /// - FullName
    /// - PhoneNumber
    ///
    /// These fields remain read-only:
    ///
    /// - UserId
    /// - Username
    /// - Email
    /// - AccountStatus
    /// - EmailVerified
    /// - Roles
    /// - SecurityVersion
    /// - FailedLoginCount
    /// - LockedUntil
    /// - CreatedAt
    /// - LastLoginAt
    /// - DeletedAt
    ///
    /// The user ID is obtained from the authenticated access token.
    /// </remarks>
    [HttpPut]
    public async Task<ActionResult<UpdateProfileResponseDto>> UpdateMyProfile(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        UpdateProfileResponseDto response =
            await _profileService.UpdateMyProfileAsync(
                request,
                cancellationToken);

        return Ok(response);
    }

    // ============================================================
    // CHANGE PASSWORD
    // POST: /api/profile/change-password
    // ============================================================

    /// <summary>
    /// Verifies the current password, securely updates the password,
    /// and revokes other active sessions.
    /// </summary>
    /// <remarks>
    /// This endpoint:
    ///
    /// 1. Reads the authenticated user ID from the JWT.
    /// 2. Reads the current session ID from the JWT sid claim.
    /// 3. Verifies the current password.
    /// 4. Validates the new password meets security requirements.
    /// 5. Updates the password hash.
    /// 6. Increments the security version.
    /// 7. Revokes all other active sessions.
    /// 8. Revokes refresh tokens belonging to those sessions.
    /// 9. Keeps the current session active.
    /// 10. Records the password-change security activity.
    ///
    /// The current session remains active after a successful password change.
    /// All other sessions are revoked for security.
    /// </remarks>
    [HttpPost("change-password")]
    public async Task<ActionResult<ChangePasswordResponseDto>> ChangeMyPassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        ChangePasswordResponseDto response =
            await _profileService.ChangeMyPasswordAsync(
                request,
                cancellationToken);

        return Ok(response);
    }
}
