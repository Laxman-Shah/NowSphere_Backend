using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using smartApi.Authentication.DTOs.Profile.Requests;
using smartApi.Authentication.DTOs.Profile.Responses;
using smartApi.Authentication.Repositories.Interface;
using smartApi.Authentication.Services.Interfaces;
using smartApi.Data;
using smartApi.Entity;
using smartApi.Utility.Current_User;
using smartApi.Utility.PasswordHasher_Security;
// Exception aliases
using AppAuthenticationException =
    smartApi.ExceptionHandling.Exceptions.Security.AuthenticationException;
using AppBadRequestException =
    smartApi.ExceptionHandling.Exceptions.Common.BadRequestException;
using AppNotFoundException =
    smartApi.ExceptionHandling.Exceptions.Common.NotFoundException;

namespace smartApi.Authentication.Services;

public class ProfileService : IProfileService
{
    // ============================================================
    // DEPENDENCIES
    // ============================================================

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ApplicationDbContext _dbContext;
    private readonly CurrentUserHelper _currentUser;
    private readonly ILogger<ProfileService> _logger;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public ProfileService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ApplicationDbContext dbContext,
        CurrentUserHelper currentUser,
        ILogger<ProfileService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    // ============================================================
    // GET PROFILE
    // ============================================================

    public async Task<MyProfileResponseDto> GetMyProfileAsync(
        CancellationToken cancellationToken = default)
    {
        long userId = _currentUser.GetRequiredUserId();
        Guid currentSessionId = _currentUser.GetRequiredSessionId();

        User? user = await _userRepository.GetUserWithActiveRolesAsync(userId);

        if (user == null)
        {
            throw new AppNotFoundException(
                "User account not found.");
        }

        // Extract active role names
        List<string> activeRoles = user.UserRoles
            .Where(ur => ur.IsActive)
            .Select(ur => ur.Role.RoleName)
            .ToList();

        // Count active sessions
        int activeSessionCount = await _userRepository.CountActiveSessionsAsync(userId);

        // Load current session with device information
        UserSession? currentSession = await _userRepository.GetOwnedSessionWithTokensAsync(
            userId,
            currentSessionId);

        ProfileCurrentSessionResponseDto? currentSessionDto = null;

        if (currentSession != null && currentSession.UserDevice != null)
        {
            currentSessionDto = new ProfileCurrentSessionResponseDto
            {
                SessionId = currentSession.UserSessionId,
                DeviceName = currentSession.UserDevice.DeviceName ?? "Unknown Device",
                DeviceType = currentSession.UserDevice.DeviceType,
                OperatingSystem = currentSession.UserDevice.OperatingSystem,
                BrowserName = currentSession.UserDevice.BrowserName,
                IpAddress = currentSession.LoginIpAddress,
                LoginAt = currentSession.LoginAt,
                LastActivityAt = currentSession.LastActivityAt,
                ExpiresAt = currentSession.ExpiresAt
            };
        }

        return new MyProfileResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            AccountStatus = user.AccountStatus,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = activeRoles,
            ActiveSessionCount = activeSessionCount,
            CurrentSession = currentSessionDto
        };
    }

    // ============================================================
    // UPDATE PROFILE
    // ============================================================

    public async Task<UpdateProfileResponseDto> UpdateMyProfileAsync(
        UpdateProfileRequestDto request,
        CancellationToken cancellationToken = default)
    {
        long userId = _currentUser.GetRequiredUserId();

        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new AppNotFoundException(
                "User account not found.");
        }

        ValidateProfileUpdate(request);

        // Update only allowed fields
        if (request.FullName != null)
        {
            user.FullName = request.FullName;
        }

        if (request.PhoneNumber != null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Record profile update activity
        await RecordLoginActivityAsync(
            userId,
            null,
            null,
            "PROFILE_UPDATED",
            "SUCCESS",
            "Profile information updated by user.");

        return new UpdateProfileResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            UpdatedAt = user.UpdatedAt.Value,
            Message = "Profile updated successfully."
        };
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    public async Task<ChangePasswordResponseDto> ChangeMyPasswordAsync(
        ChangePasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        long userId = _currentUser.GetRequiredUserId();
        Guid currentSessionId = _currentUser.GetRequiredSessionId();

        ValidatePasswordChangeRequest(request);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            User? user = await _userRepository.GetUserWithCredentialAsync(userId);

            if (user == null)
            {
                throw new AppNotFoundException(
                    "User account not found.");
            }

            if (user.Credential == null)
            {
                throw new AppAuthenticationException(
                    "User credentials not found.");
            }

            // Verify current password
            bool currentPasswordValid = _passwordHasher.VerifyPassword(
                request.CurrentPassword,
                user.Credential.PasswordHash);

            if (!currentPasswordValid)
            {
                await RecordLoginActivityAsync(
                    userId,
                    currentSessionId,
                    null,
                    "PASSWORD_CHANGE_FAILED",
                    "FAILURE",
                    "Incorrect current password.");

                throw new AppAuthenticationException(
                    "Current password is incorrect.");
            }

            // Validate new password differs from current
            if (request.CurrentPassword == request.NewPassword)
            {
                throw new AppBadRequestException(
                    "New password must be different from current password.");
            }

            // Hash new password
            PasswordHashResult newHashResult = _passwordHasher.HashPassword(
                request.NewPassword);

            // Update credential
            user.Credential.PasswordHash = newHashResult.Hash;
            user.Credential.PasswordSalt = newHashResult.Salt;
            user.Credential.PasswordUpdatedAt = DateTime.UtcNow;
            user.Credential.MustChangePassword = false;

            // Increment security version to invalidate old tokens
            user.SecurityVersion += 1;
            user.UpdatedAt = DateTime.UtcNow;

            // Revoke other active sessions
            List<UserSession> otherSessions = await _userRepository.GetOtherActiveSessionsAsync(
                userId,
                currentSessionId);

            foreach (UserSession session in otherSessions)
            {
                session.Status = "REVOKED";
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = "PASSWORD_CHANGED";

                // Revoke refresh tokens for this session
                foreach (RefreshToken token in session.RefreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedReason = "PASSWORD_CHANGED";
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Record successful password change
            await RecordLoginActivityAsync(
                userId,
                currentSessionId,
                null,
                "PASSWORD_CHANGE_SUCCEEDED",
                "SUCCESS",
                "Password changed by user. Other sessions revoked.");

            return new ChangePasswordResponseDto
            {
                Message = "Password changed successfully. All other sessions have been revoked."
            };
        }
        catch
        {
            await RollbackIfActiveAsync(transaction, cancellationToken);
            throw;
        }
    }

    private static async Task RollbackIfActiveAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            if (transaction.GetDbTransaction().Connection is null)
            {
                return;
            }

            await transaction.RollbackAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Transaction was already committed or rolled back.
        }
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private void ValidateProfileUpdate(UpdateProfileRequestDto request)
    {
        // Additional validation if needed beyond DataAnnotations
        // Currently handled by DTO validation attributes
    }

    private void ValidatePasswordChangeRequest(ChangePasswordRequestDto request)
    {
        // Additional validation if needed beyond DataAnnotations
        // Currently handled by DTO validation attributes
    }

    private async Task RecordLoginActivityAsync(
        long userId,
        Guid? sessionId,
        long? deviceId,
        string eventType,
        string outcome,
        string? description)
    {
        var activity = new LoginActivity
        {
            UserId = userId,
            UserSessionId = sessionId,
            UserDeviceId = deviceId,
            EventType = eventType,
            Outcome = outcome,
            Description = description,
            OccurredAt = DateTime.UtcNow
        };

        await _userRepository.AddLoginActivityAsync(activity);
    }
}
