using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using smartApi.Authentication.DTOs.Auth.Requests;
using smartApi.Authentication.DTOs.Auth.Responses;
using smartApi.Authentication.DTOs.Users;
using smartApi.Data;
using smartApi.Entity;
using smartApi.Enums;
using smartApi.Authentication.Repositories.Interface;
using smartApi.Authentication.Services.Interfaces;
using smartApi.Utility.Configurations;
using smartApi.Utility.Current_User;
using smartApi.Utility.Device_Information;
using smartApi.Utility.Http_Request;
using smartApi.Utility.PasswordHasher_Security;
// Exception aliases
using AppAuthenticationException =
    smartApi.ExceptionHandling.Exceptions.Security.AuthenticationException;
using AppBadRequestException =
    smartApi.ExceptionHandling.Exceptions.Common.BadRequestException;
using AppConflictException =
    smartApi.ExceptionHandling.Exceptions.Common.ConflictException;
using AppExternalServiceException =
    smartApi.ExceptionHandling.Exceptions.Infrastructure.ExternalServiceException;
using AppNotFoundException =
    smartApi.ExceptionHandling.Exceptions.Common.NotFoundException;
// Response DTO aliases
using LoginResponseDto =
    smartApi.Authentication.DTOs.Auth.Responses.LoginResponseDto;
using RegisterResponseDto =
    smartApi.Authentication.DTOs.Auth.Responses.RegisterResponseDto;

namespace smartApi.Authentication.Services
{
    public class AuthService : IAuthService
    {
        // ============================================================
        // OTP PURPOSES
        // ============================================================

        private const string RegisterOtpPurpose =
            "REGISTER_EMAIL_VERIFICATION";

        private const string LoginOtpPurpose =
            "LOGIN_TWO_FACTOR_AUTHENTICATION";

        // ============================================================
        // REGISTRATION OTP SETTINGS
        // ============================================================

        private const int RegisterOtpExpiryMinutes = 10;

        private const int RegisterOtpMaxAttempts = 5;







        private const string PasswordResetOtpPurpose =
    "PASSWORD_RESET";

        private const int PasswordResetOtpExpiryMinutes = 15;

        private const int PasswordResetOtpMaxAttempts = 5;

        private const string GenericForgotPasswordMessage =
            "If an eligible account exists for this email address, " +
            "a password reset code has been sent.";






















      
            // ============================================================
            // DEPENDENCIES
            // ============================================================

            private readonly IUserRepository _userRepository;
            private readonly IPasswordHasher _passwordHasher;
            private readonly ITokenService _tokenService;
            private readonly ApplicationDbContext _dbContext;
            private readonly IOtpService _otpService;
            private readonly IEmailService _emailService;
            private readonly ILogger<AuthService> _logger;
            private readonly IHostEnvironment _hostEnvironment;

            private readonly TwoFactorAuthenticationOptions
                _twoFactorOptions;

            private readonly JwtSettings _jwtSettings;

            // ============================================================
            // SESSION, DEVICE, AND LOGIN ACTIVITY HELPERS
            // ============================================================

            private readonly RequestInformationHelper
                _requestInformation;

            private readonly CurrentUserHelper
                _currentUser;

            private readonly DeviceInformationParser
                _deviceParser;

            private readonly DeviceFingerprintHelper
                _fingerprintHelper;


            // ============================================================
            // CONSTRUCTOR
            // ============================================================

            public AuthService(
                IUserRepository userRepository,
                IPasswordHasher passwordHasher,
                ITokenService tokenService,
                IOtpService otpService,
                IEmailService emailService,
                ApplicationDbContext dbContext,
                ILogger<AuthService> logger,
                IHostEnvironment hostEnvironment,
                IOptions<TwoFactorAuthenticationOptions> twoFactorOptions,
                IOptions<JwtSettings> jwtOptions,
                RequestInformationHelper requestInformation,
                CurrentUserHelper currentUser,
                DeviceInformationParser deviceParser,
                DeviceFingerprintHelper fingerprintHelper)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _tokenService = tokenService;
                _otpService = otpService;
                _emailService = emailService;
                _dbContext = dbContext;
                _logger = logger;
                _hostEnvironment = hostEnvironment;

                _twoFactorOptions = twoFactorOptions.Value;
                _jwtSettings = jwtOptions.Value;

                _requestInformation = requestInformation;
                _currentUser = currentUser;
                _deviceParser = deviceParser;
                _fingerprintHelper = fingerprintHelper;
            }









        // ============================================================
        // REGISTER
        // ============================================================

        public async Task<RegisterResponseDto> RegisterAsync(
            RegisterRequestDto request,
            string? ipAddress,
            string? userAgent)
        {
            ValidateRegisterRequest(request);

            var normalizedEmail =
                request.Email.Trim().ToLowerInvariant();

            var normalizedUsername =
                request.Username.Trim().ToLowerInvariant();

            var currentTime = DateTime.UtcNow;

            User? registrationUser = null;
            string? rawOtp = null;

            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                registrationUser = await _dbContext.Users
                    .Include(user => user.Credential)
                    .Include(user => user.UserRoles)
                    .FirstOrDefaultAsync(
                        user =>
                            user.Email.ToLower() ==
                            normalizedEmail
                    );

                if (registrationUser == null)
                {
                    await EnsureUsernameIsAvailableAsync(
                        normalizedUsername,
                        currentUserId: null
                    );

                    var defaultRole =
                        await _dbContext.Roles
                            .FirstOrDefaultAsync(
                                role =>
                                    role.RoleName == "USER"
                            );

                    if (defaultRole == null)
                    {
                        _logger.LogCritical(
                            "The default USER role was not found."
                        );

                        throw new AppNotFoundException(
                            "Default USER role was not found."
                        );
                    }

                    var passwordHashResult =
                        _passwordHasher.HashPassword(
                            request.Password
                        );

                    registrationUser = new User
                    {
                        Username =
                            request.Username.Trim(),

                        Email =
                            normalizedEmail,

                        FullName =
                            NormalizeOptionalValue(
                                request.FullName
                            ),

                        PhoneNumber =
                            NormalizeOptionalValue(
                                request.PhoneNumber
                            ),

                        EmailVerified = false,

                        AccountStatus =
                            AccountStatus.Pending,

                        FailedLoginCount = 0,
                        LockedUntil = null,

                        CreatedAt = currentTime,
                        UpdatedAt = currentTime,

                        Credential = new UserCredential
                        {
                            PasswordHash =
                                passwordHashResult.Hash,

                            PasswordSalt =
                                passwordHashResult.Salt,

                            PasswordAlgorithm =
                                passwordHashResult.Algorithm,

                            PasswordCreatedAt =
                                currentTime,

                            MustChangePassword = false
                        },

                        UserRoles = new List<UserRole>
                        {
                            new UserRole
                            {
                                RoleId =
                                    defaultRole.RoleId,

                                AssignedAt =
                                    currentTime,

                                IsActive = true
                            }
                        }
                    };

                    _dbContext.Users.Add(
                        registrationUser
                    );

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    ValidateExistingAccountForRegistration(
                        registrationUser
                    );

                    await EnsureUsernameIsAvailableAsync(
                        normalizedUsername,
                        registrationUser.UserId
                    );

                    UpdatePendingRegistration(
                        registrationUser,
                        request,
                        normalizedEmail,
                        currentTime
                    );
                }

                await RevokeExistingRegistrationOtpsAsync(
                    registrationUser.UserId,
                    currentTime
                );

                rawOtp = _otpService.GenerateOtp();

                var otpHash =
                    _otpService.HashOtp(rawOtp);

                var emailOtpToken =
                    new email_otp_tokens
                    {
                        UserId =
                            registrationUser.UserId,

                        LoginChallengeId = null,

                        SentToEmail =
                            registrationUser.Email,

                        TokenHash =
                            otpHash,

                        Purpose =
                            RegisterOtpPurpose,

                        ExpiresAt =
                            currentTime.AddMinutes(
                                RegisterOtpExpiryMinutes
                            ),

                        UsedAt = null,
                        RevokedAt = null,
                        RevokedReason = null,

                        AttemptCount = 0,

                        MaxAttempts =
                            RegisterOtpMaxAttempts,

                        ResendCount = 0,

                        LastSentAt =
                            currentTime,

                        CreatedByIp =
                            NormalizeOptionalValue(
                                ipAddress
                            ),

                        UserAgent =
                            NormalizeOptionalValue(
                                userAgent
                            ),

                        CreatedAt =
                            currentTime
                    };

                _dbContext.EmailOtpTokens.Add(
                    emailOtpToken
                );

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    CancellationToken.None
                );

                _logger.LogWarning(
                    exception,
                    "Registration concurrency conflict for email {Email}.",
                    normalizedEmail
                );

                throw new AppConflictException(
                    "Another request updated this registration. " +
                    "Please try again."
                );
            }
            catch (DbUpdateException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    CancellationToken.None
                );

                _logger.LogWarning(
                    exception,
                    "Database conflict during registration for email {Email}.",
                    normalizedEmail
                );

                throw new AppConflictException(
                    "The email address or username is already registered."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    CancellationToken.None
                );

                throw;
            }

            if (registrationUser == null ||
                string.IsNullOrWhiteSpace(rawOtp))
            {
                throw new AppNotFoundException(
                    "Registration could not be completed."
                );
            }

            try
            {
                await _emailService.SendOtpEmailAsync(
                    registrationUser.Email,
                    rawOtp,
                    RegisterOtpPurpose
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Registration OTP email failed for UserId {UserId}.",
                    registrationUser.UserId
                );

                throw new AppExternalServiceException(
                    "Registration was saved, but the verification " +
                    "email could not be sent. Please request a new OTP.",
                    "EMAIL_SEND_FAILED",
                    exception
                );
            }

            return new RegisterResponseDto
            {
                User = MapUserResponse(
                    registrationUser
                )
            };
        }

        // ============================================================
        // VERIFY REGISTRATION OTP
        // ============================================================

        public async Task<VerifyRegisterOtpResponseDto>
            VerifyRegisterOtpAsync(
                VerifyRegisterOtpRequestDto request)
        {
            if (request == null)
            {
                throw new AppAuthenticationException(
                    "Verification request is required."
                );
            }

            if (request.UserId <= 0)
            {
                throw new AppAuthenticationException(
                    "Invalid user."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Otp))
            {
                throw new AppAuthenticationException(
                    "OTP is required."
                );
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(
                    existingUser =>
                        existingUser.UserId ==
                        request.UserId
                );

            if (user == null)
            {
                throw new AppNotFoundException(
                    "User was not found."
                );
            }

            if (user.EmailVerified &&
                user.AccountStatus ==
                AccountStatus.Active)
            {
                throw new AppConflictException(
                    "Email is already verified."
                );
            }

            if (user.AccountStatus !=
                AccountStatus.Pending)
            {
                throw new AppAuthenticationException(
                    "This account is currently unavailable."
                );
            }

            var otpToken =
                await _dbContext.EmailOtpTokens
                    .Where(token =>
                        token.UserId ==
                            user.UserId &&
                        token.LoginChallengeId ==
                            null &&
                        token.Purpose ==
                            RegisterOtpPurpose &&
                        token.UsedAt == null &&
                        token.RevokedAt == null
                    )
                    .OrderByDescending(token =>
                        token.CreatedAt
                    )
                    .FirstOrDefaultAsync();

            if (otpToken == null)
            {
                throw new AppAuthenticationException(
                    "No active OTP was found. " +
                    "Please request a new OTP."
                );
            }

            var currentTime = DateTime.UtcNow;

            if (otpToken.ExpiresAt <= currentTime)
            {
                otpToken.RevokedAt =
                    currentTime;

                otpToken.RevokedReason =
                    "EXPIRED";

                await _dbContext.SaveChangesAsync();

                throw new AppAuthenticationException(
                    "OTP has expired. Please request a new OTP."
                );
            }

            if (otpToken.AttemptCount >=
                otpToken.MaxAttempts)
            {
                otpToken.RevokedAt =
                    currentTime;

                otpToken.RevokedReason =
                    "TOO_MANY_ATTEMPTS";

                await _dbContext.SaveChangesAsync();

                throw new AppAuthenticationException(
                    "Maximum OTP attempts exceeded. " +
                    "Please request a new OTP."
                );
            }

            var otpIsValid =
                _otpService.VerifyOtp(
                    request.Otp.Trim(),
                    otpToken.TokenHash
                );

            if (!otpIsValid)
            {
                otpToken.AttemptCount += 1;

                if (otpToken.AttemptCount >=
                    otpToken.MaxAttempts)
                {
                    otpToken.RevokedAt =
                        currentTime;

                    otpToken.RevokedReason =
                        "TOO_MANY_ATTEMPTS";
                }

                await _dbContext.SaveChangesAsync();

                throw new AppAuthenticationException(
                    "Invalid OTP."
                );
            }

            user.EmailVerified = true;

            user.AccountStatus =
                AccountStatus.Active;

            user.UpdatedAt =
                currentTime;

            otpToken.UsedAt =
                currentTime;

            await _dbContext.SaveChangesAsync();

            try
            {
                await _emailService
                    .SendEmailVerificationSuccessAsync(
                        user.Email,
                        user.FullName ??
                            user.Username
                    );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Verification-success email failed for UserId {UserId}.",
                    user.UserId
                );
            }

            return new VerifyRegisterOtpResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,

                AccountStatus =
                    user.AccountStatus.ToString(),

                EmailVerified =
                    user.EmailVerified,

                Message =
                    "Email verified successfully. " +
                    "Registration completed."
            };
        }

        // ============================================================
        // LOGIN STEP 1
        // PASSWORD VERIFICATION + LOGIN CHALLENGE CREATION
        // ============================================================

        public async Task<LoginStep1ResponseDto> LoginAsync(
            LoginRequestDto request,
            string? ipAddress,
            string? userAgent,
            CancellationToken cancellationToken = default)
        {
            ValidateLoginRequest(request);

            var normalizedIdentity =
                request.EmailOrUsername
                    .Trim()
                    .ToLowerInvariant();

            var currentTime = DateTime.UtcNow;

            var user = await _dbContext.Users
                .Include(existingUser =>
                    existingUser.Credential)
                .FirstOrDefaultAsync(
                    existingUser =>
                        existingUser.Email.ToLower() ==
                            normalizedIdentity ||
                        existingUser.Username.ToLower() ==
                            normalizedIdentity,
                    cancellationToken
                );

            if (user == null ||
                user.Credential == null)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login failed for unknown identity from IP {IpAddress}.",
                    NormalizeOptionalValue(
                        ipAddress
                    )
                );

                throw CreateGenericLoginException();
            }

            if (user.DeletedAt.HasValue)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login rejected for deleted UserId {UserId}.",
                    user.UserId
                );

                throw CreateGenericLoginException();
            }

            if (user.AccountStatus ==
                AccountStatus.Pending)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login rejected for pending UserId {UserId}.",
                    user.UserId
                );

                throw CreateGenericLoginException();
            }

            if (user.AccountStatus !=
                AccountStatus.Active)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login rejected for unavailable UserId {UserId}.",
                    user.UserId
                );

                throw CreateGenericLoginException();
            }

            if (!user.EmailVerified)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login rejected for unverified UserId {UserId}.",
                    user.UserId
                );

                throw CreateGenericLoginException();
            }

            if (user.LockedUntil.HasValue &&
                user.LockedUntil.Value > currentTime)
            {
                PerformDummyPasswordVerification(
                    request.Password
                );

                _logger.LogWarning(
                    "Login rejected for locked UserId {UserId}.",
                    user.UserId
                );

                throw CreateGenericLoginException();
            }

            if (user.LockedUntil.HasValue &&
                user.LockedUntil.Value <= currentTime)
            {
                user.LockedUntil = null;
                user.FailedLoginCount = 0;
            }

            var passwordIsValid =
                _passwordHasher.VerifyPassword(
                    request.Password,
                    user.Credential.PasswordHash
                );

            if (!passwordIsValid)
            {
                user.FailedLoginCount += 1;
                user.UpdatedAt = currentTime;

                if (user.FailedLoginCount >=
                    _twoFactorOptions
                        .MaximumFailedPasswordAttempts)
                {
                    user.LockedUntil =
                        currentTime.AddMinutes(
                            _twoFactorOptions
                                .AccountLockDurationMinutes
                        );

                    _logger.LogWarning(
                        "UserId {UserId} reached the failed-password " +
                        "threshold and was temporarily locked.",
                        user.UserId
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Invalid password for UserId {UserId}.",
                        user.UserId
                    );
                }

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                throw CreateGenericLoginException();
            }

            LoginChallenge? challenge = null;
            string? rawOtp = null;

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                user.FailedLoginCount = 0;
                user.LockedUntil = null;
                user.UpdatedAt = currentTime;

                var previousChallenges =
                    await _dbContext.LoginChallenges
                        .Where(existingChallenge =>
                            existingChallenge.UserId ==
                                user.UserId &&
                            existingChallenge.Status ==
                                LoginChallengeStatus.Pending &&
                            existingChallenge.CompletedAt ==
                                null &&
                            existingChallenge.RevokedAt ==
                                null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var previousChallenge
                         in previousChallenges)
                {
                    previousChallenge.Status =
                        LoginChallengeStatus.Revoked;

                    previousChallenge.RevokedAt =
                        currentTime;

                    previousChallenge.RevokedReason =
                        "REPLACED_BY_NEW_LOGIN_ATTEMPT";

                    previousChallenge.ConcurrencyToken =
                        Guid.NewGuid();
                }

                var previousLoginOtps =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.UserId ==
                                user.UserId &&
                            token.Purpose ==
                                LoginOtpPurpose &&
                            token.UsedAt == null &&
                            token.RevokedAt == null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var previousOtp
                         in previousLoginOtps)
                {
                    previousOtp.RevokedAt =
                        currentTime;

                    previousOtp.RevokedReason =
                        "REPLACED_BY_NEW_LOGIN_OTP";
                }

                challenge = new LoginChallenge
                {
                    LoginChallengeId =
                        Guid.NewGuid(),

                    UserId =
                        user.UserId,

                    Status =
                        LoginChallengeStatus.Pending,

                    ExpiresAt =
                        currentTime.AddMinutes(
                            _twoFactorOptions
                                .LoginChallengeExpiryMinutes
                        ),

                    CompletedAt = null,
                    RevokedAt = null,
                    RevokedReason = null,

                    CreatedByIp =
                        NormalizeOptionalValue(
                            ipAddress
                        ),

                    UserAgent =
                        NormalizeOptionalValue(
                            userAgent
                        ),

                    CreatedAt =
                        currentTime,

                    ResendCount = 0,

                    LastOtpSentAt =
                        currentTime,

                    ConcurrencyToken =
                        Guid.NewGuid()
                };

                rawOtp =
                    _otpService.GenerateOtp();

                var otpHash =
                    _otpService.HashOtp(
                        rawOtp
                    );

                var otpExpiresAt =
                    currentTime.AddMinutes(
                        _twoFactorOptions
                            .LoginOtpExpiryMinutes
                    );

                if (otpExpiresAt >
                    challenge.ExpiresAt)
                {
                    otpExpiresAt =
                        challenge.ExpiresAt;
                }

                var loginOtp =
                    new email_otp_tokens
                    {
                        UserId =
                            user.UserId,

                        LoginChallengeId =
                            challenge.LoginChallengeId,

                        SentToEmail =
                            user.Email,

                        TokenHash =
                            otpHash,

                        Purpose =
                            LoginOtpPurpose,

                        ExpiresAt =
                            otpExpiresAt,

                        UsedAt = null,
                        RevokedAt = null,
                        RevokedReason = null,

                        AttemptCount = 0,

                        MaxAttempts =
                            _twoFactorOptions
                                .LoginOtpMaxAttempts,

                        ResendCount = 0,

                        LastSentAt =
                            currentTime,

                        CreatedByIp =
                            NormalizeOptionalValue(
                                ipAddress
                            ),

                        UserAgent =
                            NormalizeOptionalValue(
                                userAgent
                            ),

                        CreatedAt =
                            currentTime
                    };

                _dbContext.LoginChallenges.Add(
                    challenge
                );

                _dbContext.EmailOtpTokens.Add(
                    loginOtp
                );

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Login challenge concurrency conflict for " +
                    "UserId {UserId}.",
                    user.UserId
                );

                throw new AppConflictException(
                    "Another login request is already being processed."
                );
            }
            catch (DbUpdateException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Database conflict while creating login " +
                    "challenge for UserId {UserId}.",
                    user.UserId
                );

                throw new AppConflictException(
                    "Unable to start login verification. " +
                    "Please try again."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                throw;
            }

            if (challenge == null ||
                string.IsNullOrWhiteSpace(rawOtp))
            {
                throw new AppNotFoundException(
                    "Login verification could not be created."
                );
            }

            try
            {
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    rawOtp,
                    LoginOtpPurpose
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Login OTP email failed for UserId {UserId}, " +
                    "ChallengeId {ChallengeId}.",
                    user.UserId,
                    challenge.LoginChallengeId
                );

                throw new AppExternalServiceException(
                    "Login verification was created, but the OTP " +
                    "email could not be sent. Please request a new OTP.",
                    "LOGIN_OTP_EMAIL_SEND_FAILED",
                    exception
                );
            }

            if (_hostEnvironment.IsDevelopment())
            {
                _logger.LogInformation(
                    "Development login OTP for ChallengeId {ChallengeId}: {Otp}",
                    challenge.LoginChallengeId,
                    rawOtp
                );
            }

            return new LoginStep1ResponseDto
            {
                TwoFactorRequired = true,

                ChallengeId =
                    challenge.LoginChallengeId,

                MaskedEmail =
                    MaskEmail(user.Email),

                ExpiresAt =
                    challenge.ExpiresAt,

                Message =
                    "A login verification code has been sent."
            };
        }

        // ============================================================
        // LOGIN STEP 2
        // VERIFY LOGIN OTP + ISSUE TOKENS
        // ============================================================

        public async Task<LoginResponseDto>
            VerifyLoginOtpAsync(
                VerifyLoginOtpRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default)
        {
            ValidateVerifyLoginOtpRequest(
                request
            );

            var currentTime =
                DateTime.UtcNow;

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                var challenge =
                    await _dbContext.LoginChallenges
                        .Include(existingChallenge =>
                            existingChallenge.User)
                        .FirstOrDefaultAsync(
                            existingChallenge =>
                                existingChallenge
                                    .LoginChallengeId ==
                                request.ChallengeId,
                            cancellationToken
                        );

                if (challenge == null)
                {
                    throw CreateGenericLoginOtpException();
                }

                if (challenge.Status !=
                        LoginChallengeStatus.Pending ||
                    challenge.CompletedAt.HasValue ||
                    challenge.RevokedAt.HasValue)
                {
                    throw CreateGenericLoginOtpException();
                }

                var activeOtp =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.LoginChallengeId ==
                                challenge.LoginChallengeId &&
                            token.UserId ==
                                challenge.UserId &&
                            token.Purpose ==
                                LoginOtpPurpose &&
                            token.UsedAt == null &&
                            token.RevokedAt == null
                        )
                        .OrderByDescending(token =>
                            token.CreatedAt
                        )
                        .FirstOrDefaultAsync(
                            cancellationToken
                        );

                if (challenge.ExpiresAt <=
                    currentTime)
                {
                    challenge.Status =
                        LoginChallengeStatus.Expired;

                    challenge.RevokedReason =
                        "CHALLENGE_EXPIRED";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    if (activeOtp != null)
                    {
                        activeOtp.RevokedAt =
                            currentTime;

                        activeOtp.RevokedReason =
                            "CHALLENGE_EXPIRED";
                    }

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                var user =
                    challenge.User;

                if (user == null ||
                    user.DeletedAt.HasValue ||
                    user.AccountStatus !=
                        AccountStatus.Active ||
                    !user.EmailVerified ||
                    (
                        user.LockedUntil.HasValue &&
                        user.LockedUntil.Value >
                            currentTime
                    ))
                {
                    challenge.Status =
                        LoginChallengeStatus.Revoked;

                    challenge.RevokedAt =
                        currentTime;

                    challenge.RevokedReason =
                        "ACCOUNT_UNAVAILABLE";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    if (activeOtp != null)
                    {
                        activeOtp.RevokedAt =
                            currentTime;

                        activeOtp.RevokedReason =
                            "ACCOUNT_UNAVAILABLE";
                    }

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                if (activeOtp == null)
                {
                    throw CreateGenericLoginOtpException();
                }

                if (activeOtp.ExpiresAt <=
                    currentTime)
                {
                    activeOtp.RevokedAt =
                        currentTime;

                    activeOtp.RevokedReason =
                        "EXPIRED";

                    challenge.Status =
                        LoginChallengeStatus.Expired;

                    challenge.RevokedReason =
                        "OTP_EXPIRED";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                if (activeOtp.AttemptCount >=
                    activeOtp.MaxAttempts)
                {
                    activeOtp.RevokedAt =
                        currentTime;

                    activeOtp.RevokedReason =
                        "TOO_MANY_ATTEMPTS";

                    challenge.Status =
                        LoginChallengeStatus.Revoked;

                    challenge.RevokedAt =
                        currentTime;

                    challenge.RevokedReason =
                        "TOO_MANY_OTP_ATTEMPTS";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                var otpIsValid =
                    _otpService.VerifyOtp(
                        request.Otp.Trim(),
                        activeOtp.TokenHash
                    );

                if (!otpIsValid)
                {
                    activeOtp.AttemptCount += 1;

                    if (activeOtp.AttemptCount >=
                        activeOtp.MaxAttempts)
                    {
                        activeOtp.RevokedAt =
                            currentTime;

                        activeOtp.RevokedReason =
                            "TOO_MANY_ATTEMPTS";

                        challenge.Status =
                            LoginChallengeStatus.Revoked;

                        challenge.RevokedAt =
                            currentTime;

                        challenge.RevokedReason =
                            "TOO_MANY_OTP_ATTEMPTS";

                        challenge.ConcurrencyToken =
                            Guid.NewGuid();
                    }

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    _logger.LogWarning(
                        "Login OTP mismatch for ChallengeId {ChallengeId}. " +
                        "Attempt {AttemptCount}/{MaxAttempts}.",
                        request.ChallengeId,
                        activeOtp.AttemptCount,
                        activeOtp.MaxAttempts
                    );

                    throw CreateGenericLoginOtpException();
                }

                activeOtp.UsedAt =
                    currentTime;

                challenge.Status =
                    LoginChallengeStatus.Completed;

                challenge.CompletedAt =
                    currentTime;

                challenge.ConcurrencyToken =
                    Guid.NewGuid();

                user.LastLoginAt =
                    currentTime;

                user.FailedLoginCount = 0;
                user.LockedUntil = null;
                user.UpdatedAt = currentTime;

                var device =
                    await FindOrCreateCurrentDeviceAsync(
                        user.UserId
                    );

                var sessionExpiresAt =
                    currentTime.AddDays(
                        _jwtSettings.RefreshTokenDays
                    );

                var session =
                    await CreateUserSessionAsync(
                        user.UserId,
                        device.UserDeviceId,
                        challenge.LoginChallengeId,
                        otpVerified: true,
                        sessionExpiresAt
                    );

                var roles =
                    await _dbContext.UserRoles
                        .Where(userRole =>
                            userRole.UserId ==
                                user.UserId &&
                            userRole.IsActive
                        )
                        .Join(
                            _dbContext.Roles,
                            userRole =>
                                userRole.RoleId,
                            role =>
                                role.RoleId,
                            (userRole, role) =>
                                role.RoleName
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                var accessToken =
                    _tokenService
                        .GenerateAccessToken(
                            user,
                            roles,
                            out var accessTokenExpiresAt,
                            session.UserSessionId
                        );

                var rawRefreshToken =
                    _tokenService
                        .GenerateRefreshToken();

                var refreshTokenEntity =
                    _tokenService
                        .CreateRefreshTokenEntity(
                            user.UserId,
                            rawRefreshToken,
                            ipAddress,
                            userAgent,
                            userSessionId:
                                session.UserSessionId
                        );

                _dbContext.RefreshTokens.Add(
                    refreshTokenEntity
                );

                await RecordLoginActivityAsync(
                    eventType: "LOGIN_OTP_VERIFIED",
                    outcome: "SUCCESS",
                    userId: user.UserId,
                    userSessionId: session.UserSessionId,
                    userDeviceId: device.UserDeviceId,
                    loginChallengeId:
                        challenge.LoginChallengeId,
                    description:
                        "Login completed with email OTP."
                );

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );

                try
                {
                    await _emailService
                        .SendLoginSuccessEmailAsync(
                            user.Email,
                            user.FullName ??
                                user.Username,
                            currentTime,
                            ipAddress,
                            userAgent
                        );
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Login-success email failed for " +
                        "UserId {UserId}.",
                        user.UserId
                    );
                }

                return new LoginResponseDto
                {
                    UserId =
                        user.UserId,

                    Username =
                        user.Username,

                    Email =
                        user.Email,

                    FullName =
                        user.FullName,

                    AccessToken =
                        accessToken,

                    RefreshToken =
                        rawRefreshToken,

                    AccessTokenExpiresAt =
                        accessTokenExpiresAt,

                    RefreshTokenExpiresAt =
                        refreshTokenEntity.ExpiresAt,

                    Message =
                        "Login successful."
                };
            }
            catch (AppAuthenticationException)
            {
                // Expected auth failures may commit state (e.g. attempt
                // counts) before throwing. Do not roll those back.
                throw;
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Concurrent login OTP verification detected " +
                    "for ChallengeId {ChallengeId}.",
                    request.ChallengeId
                );

                throw new AppAuthenticationException(
                    "This login verification challenge has " +
                    "already been processed."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                throw;
            }
        }

        // ============================================================
        // RESEND LOGIN OTP
        // ============================================================

        public async Task<ResendLoginOtpResponseDto>
            ResendLoginOtpAsync(
                ResendLoginOtpRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default)
        {
            ValidateResendLoginOtpRequest(
                request
            );

            var currentTime =
                DateTime.UtcNow;

            LoginChallenge? challenge = null;
            User? user = null;
            string? rawOtp = null;

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                challenge =
                    await _dbContext.LoginChallenges
                        .Include(existingChallenge =>
                            existingChallenge.User)
                        .FirstOrDefaultAsync(
                            existingChallenge =>
                                existingChallenge
                                    .LoginChallengeId ==
                                request.ChallengeId,
                            cancellationToken
                        );

                if (challenge == null ||
                    challenge.Status !=
                        LoginChallengeStatus.Pending ||
                    challenge.CompletedAt.HasValue ||
                    challenge.RevokedAt.HasValue)
                {
                    throw CreateGenericLoginOtpException();
                }

                if (challenge.ExpiresAt <=
                    currentTime)
                {
                    challenge.Status =
                        LoginChallengeStatus.Expired;

                    challenge.RevokedReason =
                        "CHALLENGE_EXPIRED";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    await RevokeActiveLoginOtpsAsync(
                        challenge.LoginChallengeId,
                        "CHALLENGE_EXPIRED",
                        currentTime,
                        cancellationToken
                    );

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                user =
                    challenge.User;

                if (user == null ||
                    user.DeletedAt.HasValue ||
                    user.AccountStatus !=
                        AccountStatus.Active ||
                    !user.EmailVerified ||
                    (
                        user.LockedUntil.HasValue &&
                        user.LockedUntil.Value >
                            currentTime
                    ))
                {
                    challenge.Status =
                        LoginChallengeStatus.Revoked;

                    challenge.RevokedAt =
                        currentTime;

                    challenge.RevokedReason =
                        "ACCOUNT_UNAVAILABLE";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    await RevokeActiveLoginOtpsAsync(
                        challenge.LoginChallengeId,
                        "ACCOUNT_UNAVAILABLE",
                        currentTime,
                        cancellationToken
                    );

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericLoginOtpException();
                }

                if (challenge.LastOtpSentAt.HasValue)
                {
                    var nextAllowedTime =
                        challenge.LastOtpSentAt.Value
                            .AddSeconds(
                                _twoFactorOptions
                                    .LoginOtpResendCooldownSeconds
                            );

                    if (currentTime <
                        nextAllowedTime)
                    {
                        throw new AppConflictException(
                            "Please wait before requesting " +
                            "another verification code."
                        );
                    }
                }

                if (challenge.ResendCount >=
                    _twoFactorOptions
                        .LoginOtpMaxResends)
                {
                    challenge.Status =
                        LoginChallengeStatus.Revoked;

                    challenge.RevokedAt =
                        currentTime;

                    challenge.RevokedReason =
                        "MAXIMUM_RESENDS_REACHED";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();

                    await RevokeActiveLoginOtpsAsync(
                        challenge.LoginChallengeId,
                        "MAXIMUM_RESENDS_REACHED",
                        currentTime,
                        cancellationToken
                    );

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw new AppAuthenticationException(
                        "Maximum OTP resend attempts were reached. " +
                        "Please sign in again."
                    );
                }

                await RevokeActiveLoginOtpsAsync(
                    challenge.LoginChallengeId,
                    "RESENT_NEW_OTP",
                    currentTime,
                    cancellationToken
                );

                rawOtp =
                    _otpService.GenerateOtp();

                var otpHash =
                    _otpService.HashOtp(
                        rawOtp
                    );

                var otpExpiresAt =
                    currentTime.AddMinutes(
                        _twoFactorOptions
                            .LoginOtpExpiryMinutes
                    );

                if (otpExpiresAt >
                    challenge.ExpiresAt)
                {
                    otpExpiresAt =
                        challenge.ExpiresAt;
                }

                challenge.ResendCount += 1;

                challenge.LastOtpSentAt =
                    currentTime;

                challenge.ConcurrencyToken =
                    Guid.NewGuid();

                var newOtp =
                    new email_otp_tokens
                    {
                        UserId =
                            challenge.UserId,

                        LoginChallengeId =
                            challenge.LoginChallengeId,

                        SentToEmail =
                            user.Email,

                        TokenHash =
                            otpHash,

                        Purpose =
                            LoginOtpPurpose,

                        ExpiresAt =
                            otpExpiresAt,

                        UsedAt = null,
                        RevokedAt = null,
                        RevokedReason = null,

                        AttemptCount = 0,

                        MaxAttempts =
                            _twoFactorOptions
                                .LoginOtpMaxAttempts,

                        ResendCount =
                            challenge.ResendCount,

                        LastSentAt =
                            currentTime,

                        CreatedByIp =
                            NormalizeOptionalValue(
                                ipAddress
                            ),

                        UserAgent =
                            NormalizeOptionalValue(
                                userAgent
                            ),

                        CreatedAt =
                            currentTime
                    };

                _dbContext.EmailOtpTokens.Add(
                    newOtp
                );

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Concurrent OTP resend detected for " +
                    "ChallengeId {ChallengeId}.",
                    request.ChallengeId
                );

                throw new AppConflictException(
                    "Another OTP request has already been processed."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                throw;
            }

            if (challenge == null ||
                user == null ||
                string.IsNullOrWhiteSpace(rawOtp))
            {
                throw CreateGenericLoginOtpException();
            }

            try
            {
                await _emailService.SendOtpEmailAsync(
                    user.Email,
                    rawOtp,
                    LoginOtpPurpose
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Resent login OTP email failed for " +
                    "ChallengeId {ChallengeId}.",
                    challenge.LoginChallengeId
                );

                throw new AppExternalServiceException(
                    "A new OTP was created, but the email " +
                    "could not be delivered. Please try again later.",
                    "OTP_RESEND_EMAIL_SEND_FAILED",
                    exception
                );
            }

            if (_hostEnvironment.IsDevelopment())
            {
                _logger.LogInformation(
                    "Development resent login OTP for ChallengeId {ChallengeId}: {Otp}",
                    challenge.LoginChallengeId,
                    rawOtp
                );
            }

            return new ResendLoginOtpResponseDto
            {
                ChallengeId =
                    challenge.LoginChallengeId,

                MaskedEmail =
                    MaskEmail(user.Email),

                ExpiresAt =
                    challenge.ExpiresAt,

                NextResendAvailableAt =
                    currentTime.AddSeconds(
                        _twoFactorOptions
                            .LoginOtpResendCooldownSeconds
                    ),

                RemainingResends =
                    Math.Max(
                        0,
                        _twoFactorOptions
                            .LoginOtpMaxResends -
                        challenge.ResendCount
                    ),

                Message =
                    "A new login verification code has been sent."
            };
        }

        // ============================================================
        // REFRESH TOKEN
        // ============================================================

        public async Task<LoginResponseDto> RefreshTokenAsync(
            string refreshToken,
            string? ipAddress,
            string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(
                refreshToken))
            {
                throw new AppAuthenticationException(
                    "Refresh token is required."
                );
            }

            var refreshTokenHash =
                _tokenService.HashToken(
                    refreshToken
                );

            var storedRefreshToken =
                await _dbContext.RefreshTokens
                    .Include(token =>
                        token.User)
                    .FirstOrDefaultAsync(
                        token =>
                            token.TokenHash ==
                            refreshTokenHash
                    );

            if (storedRefreshToken == null)
            {
                throw new AppAuthenticationException(
                    "Invalid refresh token."
                );
            }

            if (storedRefreshToken.IsRevoked)
            {
                throw new AppAuthenticationException(
                    "Refresh token has been revoked."
                );
            }

            var currentTime =
                DateTime.UtcNow;

            if (storedRefreshToken.ExpiresAt <=
                currentTime)
            {
                throw new AppAuthenticationException(
                    "Refresh token has expired."
                );
            }

            var user =
                storedRefreshToken.User;

            if (user == null ||
                user.DeletedAt.HasValue)
            {
                throw new AppAuthenticationException(
                    "The refresh token user is unavailable."
                );
            }

            if (user.AccountStatus !=
                AccountStatus.Active ||
                !user.EmailVerified)
            {
                throw new AppAuthenticationException(
                    "User account is not active."
                );
            }

            if (user.LockedUntil.HasValue &&
                user.LockedUntil.Value >
                    currentTime)
            {
                throw new AppAuthenticationException(
                    "The account is temporarily locked."
                );
            }

            var roles =
                await _dbContext.UserRoles
                    .Where(userRole =>
                        userRole.UserId ==
                            user.UserId &&
                        userRole.IsActive
                    )
                    .Join(
                        _dbContext.Roles,
                        userRole =>
                            userRole.RoleId,
                        role =>
                            role.RoleId,
                        (userRole, role) =>
                            role.RoleName
                    )
                    .ToListAsync();

            var newAccessToken =
                _tokenService.GenerateAccessToken(
                    user,
                    roles,
                    out var accessTokenExpiresAt,
                    storedRefreshToken.UserSessionId
                );

            var newRawRefreshToken =
                _tokenService.GenerateRefreshToken();

            var newRefreshTokenEntity =
                _tokenService
                    .CreateRefreshTokenEntity(
                        user.UserId,
                        newRawRefreshToken,
                        ipAddress,
                        userAgent,
                        storedRefreshToken.TokenFamilyId,
                        storedRefreshToken.UserSessionId
                    );

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync();

            try
            {
                storedRefreshToken.IsRevoked =
                    true;

                storedRefreshToken.RevokedAt =
                    currentTime;

                storedRefreshToken.RevokedByIp =
                    NormalizeOptionalValue(
                        ipAddress
                    );

                storedRefreshToken.RevokedReason =
                    "TOKEN_ROTATED";

                _dbContext.RefreshTokens.Add(
                    newRefreshTokenEntity
                );

                await _dbContext.SaveChangesAsync();

                storedRefreshToken.ReplacedByTokenId =
                    newRefreshTokenEntity
                        .RefreshTokenId;

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    CancellationToken.None
                );

                _logger.LogWarning(
                    exception,
                    "Concurrent refresh-token rotation detected " +
                    "for UserId {UserId}.",
                    user.UserId
                );

                throw new AppAuthenticationException(
                    "The refresh token has already been used."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    CancellationToken.None
                );

                throw;
            }

            return new LoginResponseDto
            {
                UserId =
                    user.UserId,

                Username =
                    user.Username,

                Email =
                    user.Email,

                FullName =
                    user.FullName,

                AccessToken =
                    newAccessToken,

                RefreshToken =
                    newRawRefreshToken,

                AccessTokenExpiresAt =
                    accessTokenExpiresAt,

                RefreshTokenExpiresAt =
                    newRefreshTokenEntity.ExpiresAt,

                Message =
                    "Token refreshed successfully."
            };
        }

        // ============================================================
        // LOGOUT
        // ============================================================

        public async Task<LogoutResponseDto> RevokeTokenAsync(
            string rawRefreshToken,
            string userId,
            string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(
                rawRefreshToken))
            {
                throw new AppAuthenticationException(
                    "Refresh token is required."
                );
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new AppAuthenticationException(
                    "User identity is missing."
                );
            }

            if (!long.TryParse(
                userId,
                out var parsedUserId))
            {
                throw new AppAuthenticationException(
                    "Invalid user identity."
                );
            }

            var refreshTokenHash =
                _tokenService.HashToken(
                    rawRefreshToken
                );

            var storedRefreshToken =
                await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(
                        token =>
                            token.UserId ==
                                parsedUserId &&
                            token.TokenHash ==
                                refreshTokenHash
                    );

            if (storedRefreshToken == null)
            {
                throw new AppAuthenticationException(
                    "Invalid refresh token."
                );
            }

            if (storedRefreshToken.IsRevoked)
            {
                throw new AppAuthenticationException(
                    "Refresh token has already been revoked."
                );
            }

            var currentTime =
                DateTime.UtcNow;

            if (storedRefreshToken.ExpiresAt <=
                currentTime)
            {
                throw new AppAuthenticationException(
                    "Refresh token has already expired."
                );
            }

            storedRefreshToken.IsRevoked =
                true;

            storedRefreshToken.RevokedAt =
                currentTime;

            storedRefreshToken.RevokedByIp =
                NormalizeOptionalValue(
                    ipAddress
                );

            storedRefreshToken.RevokedReason =
                "LOGOUT";

            await _dbContext.SaveChangesAsync();

            return new LogoutResponseDto
            {
                Message =
                    "Logged out successfully."
            };
        }



        // ============================================================
        // FORGOT PASSWORD
        // GENERATE AND SEND PASSWORD RESET OTP
        // ============================================================

        public async Task<ForgotPasswordResponseDto>
            ForgotPasswordAsync(
                ForgotPasswordRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default)
        {
            ValidateForgotPasswordRequest(
                request
            );

            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var currentTime =
                DateTime.UtcNow;

            var user =
                await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        existingUser =>
                            existingUser.Email.ToLower() ==
                            normalizedEmail,
                        cancellationToken
                    );

            /*
             * Return the same public response when:
             *
             * - The email does not exist
             * - The account is deleted
             * - The account is not active
             * - The email is not verified
             */
            if (!IsUserEligibleForPasswordReset(
                    user))
            {
                _logger.LogInformation(
                    "Password reset requested for an unknown " +
                    "or ineligible account from IP {IpAddress}.",
                    NormalizeOptionalValue(
                        ipAddress
                    )
                );

                return CreateGenericForgotPasswordResponse();
            }

            string? rawOtp = null;

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                /*
                 * Revoke previous active password-reset OTPs.
                 * Only the newest password-reset OTP remains valid.
                 */
                var previousResetOtps =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.UserId ==
                                user!.UserId &&
                            token.LoginChallengeId ==
                                null &&
                            token.Purpose ==
                                PasswordResetOtpPurpose &&
                            token.UsedAt ==
                                null &&
                            token.RevokedAt ==
                                null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var previousOtp
                         in previousResetOtps)
                {
                    previousOtp.RevokedAt =
                        currentTime;

                    previousOtp.RevokedReason =
                        previousOtp.ExpiresAt <=
                        currentTime
                            ? "EXPIRED"
                            : "REPLACED_BY_NEW_PASSWORD_RESET_OTP";
                }

                rawOtp =
                    _otpService.GenerateOtp();

                var otpHash =
                    _otpService.HashOtp(
                        rawOtp
                    );

                var passwordResetOtp =
                    new email_otp_tokens
                    {
                        UserId =
                            user!.UserId,

                        LoginChallengeId =
                            null,

                        SentToEmail =
                            user.Email,

                        TokenHash =
                            otpHash,

                        Purpose =
                            PasswordResetOtpPurpose,

                        ExpiresAt =
                            currentTime.AddMinutes(
                                PasswordResetOtpExpiryMinutes
                            ),

                        UsedAt =
                            null,

                        RevokedAt =
                            null,

                        RevokedReason =
                            null,

                        AttemptCount =
                            0,

                        MaxAttempts =
                            PasswordResetOtpMaxAttempts,

                        ResendCount =
                            0,

                        LastSentAt =
                            currentTime,

                        CreatedByIp =
                            NormalizeOptionalValue(
                                ipAddress
                            ),

                        UserAgent =
                            NormalizeOptionalValue(
                                userAgent
                            ),

                        CreatedAt =
                            currentTime
                    };

                _dbContext.EmailOtpTokens.Add(
                    passwordResetOtp
                );

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Concurrent password-reset request detected " +
                    "for UserId {UserId}.",
                    user!.UserId
                );

                return CreateGenericForgotPasswordResponse();
            }
            catch (DbUpdateException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogError(
                    exception,
                    "Database error while creating a password-reset " +
                    "OTP for UserId {UserId}.",
                    user!.UserId
                );

                /*
                 * Do not reveal account or database information
                 * through this anonymous endpoint.
                 */
                return CreateGenericForgotPasswordResponse();
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                throw;
            }

            if (string.IsNullOrWhiteSpace(
                    rawOtp))
            {
                return CreateGenericForgotPasswordResponse();
            }

            try
            {
                await _emailService.SendOtpEmailAsync(
                    user!.Email,
                    rawOtp,
                    PasswordResetOtpPurpose
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Password-reset OTP email failed for " +
                    "UserId {UserId}.",
                    user!.UserId
                );

                /*
                 * Always preserve the generic response.
                 * Do not reveal account existence or email failure.
                 */
            }

            return CreateGenericForgotPasswordResponse();
        }

        // ============================================================
        // RESET PASSWORD
        // VERIFY OTP, UPDATE PASSWORD AND LOG OUT ALL DEVICES
        // ============================================================

        public async Task<ResetPasswordResponseDto>
            ResetPasswordAsync(
                ResetPasswordRequestDto request,
                string? ipAddress,
                string? userAgent,
                CancellationToken cancellationToken = default)
        {
            ValidateResetPasswordRequest(
                request
            );

            var normalizedEmail =
                request.Email
                    .Trim()
                    .ToLowerInvariant();

            var currentTime =
                DateTime.UtcNow;

            User? passwordResetUser =
                null;

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken
                    );

            try
            {
                passwordResetUser =
                    await _dbContext.Users
                        .Include(user =>
                            user.Credential
                        )
                        .FirstOrDefaultAsync(
                            user =>
                                user.Email.ToLower() ==
                                normalizedEmail,
                            cancellationToken
                        );

                if (!IsUserEligibleForPasswordReset(
                        passwordResetUser) ||
                    passwordResetUser!.Credential ==
                        null)
                {
                    throw CreateGenericPasswordResetException();
                }

                var resetOtp =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.UserId ==
                                passwordResetUser.UserId &&
                            token.LoginChallengeId ==
                                null &&
                            token.Purpose ==
                                PasswordResetOtpPurpose &&
                            token.UsedAt ==
                                null &&
                            token.RevokedAt ==
                                null
                        )
                        .OrderByDescending(token =>
                            token.CreatedAt
                        )
                        .FirstOrDefaultAsync(
                            cancellationToken
                        );

                if (resetOtp == null)
                {
                    throw CreateGenericPasswordResetException();
                }

                /*
                 * Reject and revoke an expired OTP.
                 */
                if (resetOtp.ExpiresAt <=
                    currentTime)
                {
                    resetOtp.RevokedAt =
                        currentTime;

                    resetOtp.RevokedReason =
                        "EXPIRED";

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericPasswordResetException();
                }

                /*
                 * Reject an OTP that has reached the maximum
                 * verification attempt count.
                 */
                if (resetOtp.AttemptCount >=
                    resetOtp.MaxAttempts)
                {
                    resetOtp.RevokedAt =
                        currentTime;

                    resetOtp.RevokedReason =
                        "TOO_MANY_ATTEMPTS";

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericPasswordResetException();
                }

                var otpIsValid =
                    _otpService.VerifyOtp(
                        request.Otp.Trim(),
                        resetOtp.TokenHash
                    );

                if (!otpIsValid)
                {
                    resetOtp.AttemptCount += 1;

                    if (resetOtp.AttemptCount >=
                        resetOtp.MaxAttempts)
                    {
                        resetOtp.RevokedAt =
                            currentTime;

                        resetOtp.RevokedReason =
                            "TOO_MANY_ATTEMPTS";
                    }

                    await _dbContext.SaveChangesAsync(
                        cancellationToken
                    );

                    await transaction.CommitAsync(
                        cancellationToken
                    );

                    throw CreateGenericPasswordResetException();
                }

                /*
                 * Do not allow the current password to be reused.
                 */
                var currentPasswordWasSubmitted =
                    _passwordHasher.VerifyPassword(
                        request.NewPassword,
                        passwordResetUser
                            .Credential
                            .PasswordHash
                    );

                if (currentPasswordWasSubmitted)
                {
                    throw new AppConflictException(
                        "The new password must be different " +
                        "from the current password."
                    );
                }

                /*
                 * Generate and store the new password hash.
                 */
                var passwordHashResult =
                    _passwordHasher.HashPassword(
                        request.NewPassword
                    );

                passwordResetUser
                    .Credential
                    .PasswordHash =
                        passwordHashResult.Hash;

                passwordResetUser
                    .Credential
                    .PasswordSalt =
                        passwordHashResult.Salt;

                passwordResetUser
                    .Credential
                    .PasswordAlgorithm =
                        passwordHashResult.Algorithm;

                passwordResetUser
                    .Credential
                    .PasswordUpdatedAt =
                        currentTime;

                passwordResetUser
                    .Credential
                    .MustChangePassword =
                        false;

                passwordResetUser.FailedLoginCount =
                    0;

                passwordResetUser.LockedUntil =
                    null;

                passwordResetUser.UpdatedAt =
                    currentTime;

                /*
                 * Mark the OTP as used.
                 */
                resetOtp.UsedAt =
                    currentTime;

                /*
                 * Revoke every other active password-reset OTP.
                 */
                var otherResetOtps =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.UserId ==
                                passwordResetUser.UserId &&
                            token.EmailOtpTokenId !=
                                resetOtp.EmailOtpTokenId &&
                            token.LoginChallengeId ==
                                null &&
                            token.Purpose ==
                                PasswordResetOtpPurpose &&
                            token.UsedAt ==
                                null &&
                            token.RevokedAt ==
                                null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var otherOtp
                         in otherResetOtps)
                {
                    otherOtp.RevokedAt =
                        currentTime;

                    otherOtp.RevokedReason =
                        "PASSWORD_RESET_COMPLETED";
                }

                /*
                 * Revoke every active refresh token.
                 * This prevents every existing device from
                 * obtaining a new access token.
                 */
                var activeRefreshTokens =
                    await _dbContext.RefreshTokens
                        .Where(token =>
                            token.UserId ==
                                passwordResetUser.UserId &&
                            !token.IsRevoked
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var refreshToken
                         in activeRefreshTokens)
                {
                    refreshToken.IsRevoked =
                        true;

                    refreshToken.RevokedAt =
                        currentTime;

                    refreshToken.RevokedByIp =
                        NormalizeOptionalValue(
                            ipAddress
                        );

                    refreshToken.RevokedReason =
                        "PASSWORD_RESET_GLOBAL_LOGOUT";
                }

                /*
                 * Revoke login challenges started before
                 * the password change.
                 */
                var pendingLoginChallenges =
                    await _dbContext.LoginChallenges
                        .Where(challenge =>
                            challenge.UserId ==
                                passwordResetUser.UserId &&
                            challenge.Status ==
                                LoginChallengeStatus.Pending &&
                            challenge.CompletedAt ==
                                null &&
                            challenge.RevokedAt ==
                                null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var challenge
                         in pendingLoginChallenges)
                {
                    challenge.Status =
                        LoginChallengeStatus.Revoked;

                    challenge.RevokedAt =
                        currentTime;

                    challenge.RevokedReason =
                        "PASSWORD_RESET_COMPLETED";

                    challenge.ConcurrencyToken =
                        Guid.NewGuid();
                }

                /*
                 * Revoke all active login OTPs.
                 */
                var activeLoginOtps =
                    await _dbContext.EmailOtpTokens
                        .Where(token =>
                            token.UserId ==
                                passwordResetUser.UserId &&
                            token.Purpose ==
                                LoginOtpPurpose &&
                            token.UsedAt ==
                                null &&
                            token.RevokedAt ==
                                null
                        )
                        .ToListAsync(
                            cancellationToken
                        );

                foreach (var loginOtp
                         in activeLoginOtps)
                {
                    loginOtp.RevokedAt =
                        currentTime;

                    loginOtp.RevokedReason =
                        "PASSWORD_RESET_COMPLETED";
                }

                await _dbContext.SaveChangesAsync(
                    cancellationToken
                );

                await transaction.CommitAsync(
                    cancellationToken
                );
            }
            catch (DbUpdateConcurrencyException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogWarning(
                    exception,
                    "Concurrent password reset detected " +
                    "for email {Email}.",
                    normalizedEmail
                );

                throw CreateGenericPasswordResetException();
            }
            catch (DbUpdateException exception)
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                _logger.LogError(
                    exception,
                    "Database error while resetting the password " +
                    "for email {Email}.",
                    normalizedEmail
                );

                throw new AppConflictException(
                    "The password reset could not be completed. " +
                    "Please request a new verification code."
                );
            }
            catch
            {
                await RollbackIfActiveAsync(
                    transaction,
                    cancellationToken
                );

                throw;
            }

            if (passwordResetUser == null)
            {
                throw CreateGenericPasswordResetException();
            }

            /*
             * Notification email failure must not restore
             * the previous password.
             */
            try
            {
                await _emailService
                    .SendPasswordChangedEmailAsync(
                        passwordResetUser.Email,
                        passwordResetUser.FullName ??
                            passwordResetUser.Username,
                        currentTime,
                        ipAddress,
                        userAgent
                    );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Password-change notification failed " +
                    "for UserId {UserId}.",
                    passwordResetUser.UserId
                );
            }

            _logger.LogInformation(
                "Password reset completed and all refresh tokens " +
                "were revoked for UserId {UserId}.",
                passwordResetUser.UserId
            );

            return new ResetPasswordResponseDto
            {
                Message =
                    "Password changed successfully. " +
                    "Please log in again with your new password."
            };
        }

        // ============================================================
        // REGISTRATION HELPERS
        // ============================================================

        private static void ValidateRegisterRequest(
            RegisterRequestDto request)
        {
            ArgumentNullException.ThrowIfNull(
                request
            );

            if (string.IsNullOrWhiteSpace(
                request.Email))
            {
                throw new ArgumentException(
                    "Email address is required.",
                    nameof(request.Email)
                );
            }

            if (string.IsNullOrWhiteSpace(
                request.Username))
            {
                throw new ArgumentException(
                    "Username is required.",
                    nameof(request.Username)
                );
            }

            if (string.IsNullOrWhiteSpace(
                request.Password))
            {
                throw new ArgumentException(
                    "Password is required.",
                    nameof(request.Password)
                );
            }
        }

        private async Task EnsureUsernameIsAvailableAsync(
            string normalizedUsername,
            long? currentUserId)
        {
            var usernameExists =
                await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(user =>
                        user.Username.ToLower() ==
                            normalizedUsername &&
                        (
                            !currentUserId.HasValue ||
                            user.UserId !=
                                currentUserId.Value
                        )
                    );

            if (usernameExists)
            {
                throw new AppConflictException(
                    "Username is already taken."
                );
            }
        }

        private static void
            ValidateExistingAccountForRegistration(
                User user)
        {
            if (user.AccountStatus ==
                AccountStatus.Pending)
            {
                return;
            }

            if (user.AccountStatus ==
                AccountStatus.Active)
            {
                throw new AppConflictException(
                    "An account with this email address " +
                    "already exists."
                );
            }

            throw new AppAuthenticationException(
                "This account is currently unavailable."
            );
        }

        private void UpdatePendingRegistration(
            User user,
            RegisterRequestDto request,
            string normalizedEmail,
            DateTime currentTime)
        {
            user.Username =
                request.Username.Trim();

            user.Email =
                normalizedEmail;

            user.FullName =
                NormalizeOptionalValue(
                    request.FullName
                );

            user.PhoneNumber =
                NormalizeOptionalValue(
                    request.PhoneNumber
                );

            user.EmailVerified = false;

            user.AccountStatus =
                AccountStatus.Pending;

            user.UpdatedAt =
                currentTime;

            var passwordHashResult =
                _passwordHasher.HashPassword(
                    request.Password
                );

            if (user.Credential == null)
            {
                user.Credential =
                    new UserCredential
                    {
                        UserId =
                            user.UserId,

                        PasswordHash =
                            passwordHashResult.Hash,

                        PasswordSalt =
                            passwordHashResult.Salt,

                        PasswordAlgorithm =
                            passwordHashResult.Algorithm,

                        PasswordCreatedAt =
                            currentTime,

                        MustChangePassword =
                            false
                    };
            }
            else
            {
                user.Credential.PasswordHash =
                    passwordHashResult.Hash;

                user.Credential.PasswordSalt =
                    passwordHashResult.Salt;

                user.Credential.PasswordAlgorithm =
                    passwordHashResult.Algorithm;

                user.Credential.PasswordUpdatedAt =
                    currentTime;

                user.Credential.MustChangePassword =
                    false;
            }
        }

        private async Task
            RevokeExistingRegistrationOtpsAsync(
                long userId,
                DateTime currentTime)
        {
            var existingOtpTokens =
                await _dbContext.EmailOtpTokens
                    .Where(token =>
                        token.UserId ==
                            userId &&
                        token.Purpose ==
                            RegisterOtpPurpose &&
                        token.UsedAt ==
                            null &&
                        token.RevokedAt ==
                            null
                    )
                    .ToListAsync();

            foreach (var otpToken
                     in existingOtpTokens)
            {
                otpToken.RevokedAt =
                    currentTime;

                otpToken.RevokedReason =
                    otpToken.ExpiresAt <=
                    currentTime
                        ? "EXPIRED"
                        : "REPLACED_BY_NEW_OTP";
            }
        }

        // ============================================================
        // LOGIN HELPERS
        // ============================================================

        private static void ValidateLoginRequest(
            LoginRequestDto request)
        {
            if (request == null)
            {
                throw new AppAuthenticationException(
                    "Login request is required."
                );
            }

            if (string.IsNullOrWhiteSpace(
                request.EmailOrUsername))
            {
                throw new AppAuthenticationException(
                    "Email or username is required."
                );
            }

            if (string.IsNullOrWhiteSpace(
                request.Password))
            {
                throw new AppAuthenticationException(
                    "Password is required."
                );
            }
        }

        private static void
            ValidateVerifyLoginOtpRequest(
                VerifyLoginOtpRequestDto request)
        {
            if (request == null ||
                request.ChallengeId ==
                    Guid.Empty)
            {
                throw CreateGenericLoginOtpException();
            }

            if (string.IsNullOrWhiteSpace(
                    request.Otp) ||
                request.Otp.Length != 6 ||
                !request.Otp.All(static character =>
                    character is >= '0' and <= '9'))
            {
                throw CreateGenericLoginOtpException();
            }
        }

        private static void
            ValidateResendLoginOtpRequest(
                ResendLoginOtpRequestDto request)
        {
            if (request == null ||
                request.ChallengeId ==
                    Guid.Empty)
            {
                throw CreateGenericLoginOtpException();
            }
        }

        private static AppAuthenticationException
            CreateGenericLoginException()
        {
            return new AppAuthenticationException(
                "Unable to complete sign-in with the " +
                "supplied credentials."
            );
        }

        private static AppAuthenticationException
            CreateGenericLoginOtpException()
        {
            return new AppAuthenticationException(
                "Invalid or expired login verification challenge."
            );
        }

        private void PerformDummyPasswordVerification(
            string submittedPassword)
        {
            try
            {
                _passwordHasher.VerifyPassword(
                    submittedPassword,
                    _twoFactorOptions
                        .DummyPasswordHash
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Dummy password verification failed. " +
                    "Check the configured DummyPasswordHash."
                );
            }
        }

        private async Task
            RevokeActiveLoginOtpsAsync(
                Guid challengeId,
                string reason,
                DateTime revokedAt,
                CancellationToken cancellationToken)
        {
            var activeOtps =
                await _dbContext.EmailOtpTokens
                    .Where(token =>
                        token.LoginChallengeId ==
                            challengeId &&
                        token.Purpose ==
                            LoginOtpPurpose &&
                        token.UsedAt ==
                            null &&
                        token.RevokedAt ==
                            null
                    )
                    .ToListAsync(
                        cancellationToken
                    );

            foreach (var otp in activeOtps)
            {
                otp.RevokedAt =
                    revokedAt;

                otp.RevokedReason =
                    reason;
            }
        }

        private static string MaskEmail(
            string email)
        {
            if (string.IsNullOrWhiteSpace(
                email))
            {
                return string.Empty;
            }

            var atIndex =
                email.IndexOf('@');

            if (atIndex <= 0 ||
                atIndex ==
                email.Length - 1)
            {
                return "***";
            }

            var localPart =
                email[..atIndex];

            var domain =
                email[(atIndex + 1)..];

            if (localPart.Length == 1)
            {
                return $"*@{domain}";
            }

            if (localPart.Length == 2)
            {
                return
                    $"{localPart[0]}*@{domain}";
            }

            var visibleStart =
                localPart[..2];

            var hiddenPart =
                new string(
                    '*',
                    Math.Max(
                        3,
                        localPart.Length - 2
                    )
                );

            return
                $"{visibleStart}{hiddenPart}@{domain}";
        }

        private static async Task
            RollbackIfActiveAsync(
                IDbContextTransaction transaction,
                CancellationToken cancellationToken)
        {
            try
            {
                // Already committed/rolled-back transactions have no
                // active connection; skip to avoid EF fail logs.
                if (transaction.GetDbTransaction()
                        .Connection is null)
                {
                    return;
                }

                await transaction.RollbackAsync(
                    cancellationToken
                );
            }
            catch (InvalidOperationException)
            {
                // Transaction was already committed or rolled back.
            }
        }

        // ============================================================
        // SHARED HELPERS
        // ============================================================

        private static string? NormalizeOptionalValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }

        private static UserResponseDto MapUserResponse(
            User user)
        {
            return new UserResponseDto
            {
                UserId =
                    user.UserId,

                Username =
                    user.Username,

                Email =
                    user.Email,

                FullName =
                    user.FullName,

                PhoneNumber =
                    user.PhoneNumber,

                EmailVerified =
                    user.EmailVerified,

                AccountStatus =
                    user.AccountStatus.ToString(),

                CreatedAt =
                    user.CreatedAt
            };
        }



        // ============================================================
        // PASSWORD RESET HELPERS
        // ============================================================

        private static void ValidateForgotPasswordRequest(
            ForgotPasswordRequestDto request)
        {
            if (request == null)
            {
                throw new ArgumentException(
                    "Password reset request is required."
                );
            }

            if (string.IsNullOrWhiteSpace(
                    request.Email))
            {
                throw new ArgumentException(
                    "Email address is required.",
                    nameof(request.Email)
                );
            }

            var normalizedEmail =
                request.Email.Trim();

            if (normalizedEmail.Length > 100 ||
                !normalizedEmail.Contains('@'))
            {
                throw new ArgumentException(
                    "A valid email address is required.",
                    nameof(request.Email)
                );
            }
        }

        private static void ValidateResetPasswordRequest(
            ResetPasswordRequestDto request)
        {
            if (request == null)
            {
                throw CreateGenericPasswordResetException();
            }

            if (string.IsNullOrWhiteSpace(
                    request.Email))
            {
                throw CreateGenericPasswordResetException();
            }

            var normalizedEmail =
                request.Email.Trim();

            if (normalizedEmail.Length > 100 ||
                !normalizedEmail.Contains('@'))
            {
                throw CreateGenericPasswordResetException();
            }

            if (string.IsNullOrWhiteSpace(
                    request.Otp))
            {
                throw CreateGenericPasswordResetException();
            }

            var normalizedOtp =
                request.Otp.Trim();

            if (normalizedOtp.Length != 6 ||
                !normalizedOtp.All(char.IsDigit))
            {
                throw CreateGenericPasswordResetException();
            }

            if (string.IsNullOrWhiteSpace(
                    request.NewPassword))
            {
                throw new AppBadRequestException(
                    "New password is required.",
                    "PASSWORD_REQUIRED"
                );
            }

            if (string.IsNullOrWhiteSpace(
                    request.ConfirmPassword))
            {
                throw new AppBadRequestException(
                    "Password confirmation is required.",
                    "PASSWORD_CONFIRMATION_REQUIRED"
                );
            }

            if (!string.Equals(
                    request.NewPassword,
                    request.ConfirmPassword,
                    StringComparison.Ordinal))
            {
                throw new AppBadRequestException(
                    "New password and confirmation password " +
                    "do not match.",
                    "PASSWORD_MISMATCH"
                );
            }

            if (request.NewPassword.Length < 8)
            {
                throw new AppBadRequestException(
                    "The new password must contain at least " +
                    "eight characters.",
                    "PASSWORD_TOO_SHORT"
                );
            }

            if (!request.NewPassword.Any(
                    char.IsUpper))
            {
                throw new AppBadRequestException(
                    "The new password must contain at least " +
                    "one uppercase letter.",
                    "PASSWORD_MISSING_UPPERCASE"
                );
            }

            if (!request.NewPassword.Any(
                    char.IsLower))
            {
                throw new AppBadRequestException(
                    "The new password must contain at least " +
                    "one lowercase letter.",
                    "PASSWORD_MISSING_LOWERCASE"
                );
            }

            if (!request.NewPassword.Any(
                    char.IsDigit))
            {
                throw new AppBadRequestException(
                    "The new password must contain at least " +
                    "one number.",
                    "PASSWORD_MISSING_DIGIT"
                );
            }

            if (!request.NewPassword.Any(
                    character =>
                        !char.IsLetterOrDigit(character)))
            {
                throw new AppBadRequestException(
                    "The new password must contain at least " +
                    "one special character.",
                    "PASSWORD_MISSING_SPECIAL_CHAR"
                );
            }
        }

        private static bool IsUserEligibleForPasswordReset(
            User? user)
        {
            return user != null &&
                   !user.DeletedAt.HasValue &&
                   user.AccountStatus ==
                       AccountStatus.Active &&
                   user.EmailVerified;
        }

        private static AppAuthenticationException
            CreateGenericPasswordResetException()
        {
            return new AppAuthenticationException(
                "Invalid or expired password reset verification."
            );
        }

        private static ForgotPasswordResponseDto
            CreateGenericForgotPasswordResponse()
        {
            return new ForgotPasswordResponseDto
            {
                Message =
                    GenericForgotPasswordMessage
            };
        }








        // =====================================================
        // PRIVATE HELPER: FIND OR CREATE CURRENT DEVICE
        // =====================================================
        private async Task<UserDevice> FindOrCreateCurrentDeviceAsync(long userId)
        {
            string? userAgent = _requestInformation.GetUserAgent();
            string? clientDeviceId = _requestInformation.GetClientDeviceId();
            string? ipAddress = _requestInformation.GetIpAddress();

            DeviceInformationModel deviceInformation = _deviceParser.Parse(userAgent);

            string fingerprintHash = _fingerprintHelper.CreateHash(clientDeviceId, userAgent);

            UserDevice? existingDevice = await _userRepository.GetUserDeviceByFingerprintAsync(userId, fingerprintHash);

            DateTime now = DateTime.UtcNow;

            if (existingDevice is not null)
            {
                existingDevice.DeviceName = deviceInformation.DeviceName;
                existingDevice.DeviceType = deviceInformation.DeviceType;
                existingDevice.OperatingSystem = deviceInformation.OperatingSystem;
                existingDevice.OperatingSystemVersion = deviceInformation.OperatingSystemVersion;
                existingDevice.BrowserName = deviceInformation.BrowserName;
                existingDevice.BrowserVersion = deviceInformation.BrowserVersion;
                existingDevice.LastIpAddress = ipAddress;
                existingDevice.LastUserAgent = userAgent;
                existingDevice.LastSeenAt = now;
                existingDevice.UpdatedAt = now;

                return existingDevice;
            }

            UserDevice newDevice = new()
            {
                UserId = userId,
                DeviceFingerprintHash = fingerprintHash,
                DeviceName = deviceInformation.DeviceName,
                DeviceType = deviceInformation.DeviceType,
                OperatingSystem = deviceInformation.OperatingSystem,
                OperatingSystemVersion = deviceInformation.OperatingSystemVersion,
                BrowserName = deviceInformation.BrowserName,
                BrowserVersion = deviceInformation.BrowserVersion,
                LastIpAddress = ipAddress,
                LastUserAgent = userAgent,
                FirstSeenAt = now,
                LastSeenAt = now,
                IsTrusted = false,
                IsActive = true,
                CreatedAt = now
            };

            return await _userRepository.AddUserDeviceAsync(newDevice);
        }


        // =====================================================
        // PRIVATE HELPER: CREATE USER SESSION
        // =====================================================
        private async Task<UserSession> CreateUserSessionAsync(
            long userId,
            long userDeviceId,
            Guid? loginChallengeId,
            bool otpVerified,
            DateTime expiresAt)
        {
            DateTime now = DateTime.UtcNow;

            UserSession session = new()
            {
                UserSessionId = Guid.NewGuid(),
                UserId = userId,
                UserDeviceId = userDeviceId,
                LoginChallengeId = loginChallengeId,
                Status = "ACTIVE",
                AuthenticationLevel = otpVerified ? "PASSWORD_OTP" : "PASSWORD",
                AuthenticationMethods = otpVerified ? "PASSWORD,EMAIL_OTP" : "PASSWORD",
                OtpVerified = otpVerified,
                OtpVerifiedAt = otpVerified ? now : null,
                LoginIpAddress = _requestInformation.GetIpAddress(),
                LoginUserAgent = _requestInformation.GetUserAgent(),
                LastIpAddress = _requestInformation.GetIpAddress(),
                LastUserAgent = _requestInformation.GetUserAgent(),
                LoginAt = now,
                LastActivityAt = now,
                ExpiresAt = expiresAt,
                CreatedAt = now,
                ConcurrencyToken = Guid.NewGuid()
            };

            return await _userRepository.AddUserSessionAsync(session);
        }


        // =====================================================
        // PRIVATE HELPER: RECORD LOGIN ACTIVITY
        // =====================================================
        private async Task<LoginActivity> RecordLoginActivityAsync(
            string eventType,
            string outcome,
            long? userId = null,
            Guid? userSessionId = null,
            long? userDeviceId = null,
            Guid? loginChallengeId = null,
            string? attemptedIdentifier = null,
            string? failureCode = null,
            string? description = null)
        {
            string? userAgent = _requestInformation.GetUserAgent();
            DeviceInformationModel deviceInformation = _deviceParser.Parse(userAgent);

            LoginActivity activity = new()
            {
                UserId = userId,
                UserSessionId = userSessionId,
                UserDeviceId = userDeviceId,
                LoginChallengeId = loginChallengeId,
                EventType = eventType,
                Outcome = outcome,
                AttemptedIdentifier = attemptedIdentifier,
                FailureCode = failureCode,
                Description = description,
                IpAddress = _requestInformation.GetIpAddress(),
                UserAgent = userAgent,
                DeviceType = deviceInformation.DeviceType,
                OperatingSystem = deviceInformation.OperatingSystem,
                BrowserName = deviceInformation.BrowserName,
                OccurredAt = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid()
            };

            return await _userRepository.AddLoginActivityAsync(activity);
        }


        // =====================================================
        // PUBLIC METHOD: GET MY SESSIONS
        // =====================================================
        public async Task<IReadOnlyCollection<UserSessionResponse>> GetMySessionsAsync(
            CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid currentSessionId = _currentUser.GetRequiredSessionId();
            DateTime now = DateTime.UtcNow;

            List<UserSession> sessions = await _userRepository.GetUserSessionsAsync(userId);

            return sessions
                .Select(session => new UserSessionResponse
                {
                    SessionId = session.UserSessionId,
                    DeviceName = session.UserDevice.DeviceName ?? "Unknown Device",
                    DeviceType = session.UserDevice.DeviceType,
                    OperatingSystem = session.UserDevice.OperatingSystem,
                    BrowserName = session.UserDevice.BrowserName,
                    IpAddress = session.LastIpAddress,
                    Status = session.Status == "ACTIVE" && session.ExpiresAt <= now ? "EXPIRED" : session.Status,
                    LoginAt = session.LoginAt,
                    LastActivityAt = session.LastActivityAt,
                    ExpiresAt = session.ExpiresAt,
                    IsCurrentSession = session.UserSessionId == currentSessionId,
                    IsTrustedDevice = session.UserDevice.IsTrusted
                })
                .ToArray();
        }


        // =====================================================
        // PUBLIC METHOD: REVOKE ONE SESSION
        // =====================================================
        public async Task RevokeSessionAsync(
            Guid sessionId,
            string? reason,
            CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid currentSessionId = _currentUser.GetRequiredSessionId();

            if (sessionId == currentSessionId)
            {
                await LogoutCurrentSessionAsync(cancellationToken);
                return;
            }

            UserSession session = await GetRequiredOwnedSessionAsync(userId, sessionId);

            RevokeSessionAndTokens(session, reason ?? "USER_REVOKED_SESSION");

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordLoginActivityAsync(
                eventType: "SESSION_REVOKED",
                outcome: "SUCCESS",
                userId: userId,
                userSessionId: session.UserSessionId,
                userDeviceId: session.UserDeviceId,
                description: "The selected user session was revoked.");
        }


        // =====================================================
        // PUBLIC METHOD: REVOKE OTHER SESSIONS
        // =====================================================
        public async Task RevokeOtherSessionsAsync(CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid currentSessionId = _currentUser.GetRequiredSessionId();

            List<UserSession> sessions = await _userRepository.GetOtherActiveSessionsAsync(userId, currentSessionId);

            foreach (UserSession session in sessions)
            {
                RevokeSessionAndTokens(session, "USER_REVOKED_OTHER_SESSIONS");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordLoginActivityAsync(
                eventType: "OTHER_SESSIONS_REVOKED",
                outcome: "SUCCESS",
                userId: userId,
                userSessionId: currentSessionId,
                description: "All other active sessions were revoked.");
        }


        // =====================================================
        // PUBLIC METHOD: REVOKE ALL SESSIONS
        // =====================================================
        public async Task RevokeAllSessionsAsync(CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid currentSessionId = _currentUser.GetRequiredSessionId();

            List<UserSession> sessions = await _userRepository.GetAllActiveSessionsAsync(userId);

            foreach (UserSession session in sessions)
            {
                RevokeSessionAndTokens(session, "USER_REVOKED_ALL_SESSIONS");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordLoginActivityAsync(
                eventType: "ALL_SESSIONS_REVOKED",
                outcome: "SUCCESS",
                userId: userId,
                userSessionId: currentSessionId,
                description: "All active user sessions were revoked.");
        }


        // =====================================================
        // PUBLIC METHOD: LOG OUT CURRENT SESSION
        // =====================================================
        public async Task LogoutCurrentSessionAsync(CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid sessionId = _currentUser.GetRequiredSessionId();

            UserSession session = await GetRequiredOwnedSessionAsync(userId, sessionId);
            DateTime now = DateTime.UtcNow;

            session.Status = "LOGGED_OUT";
            session.LoggedOutAt = now;
            session.LogoutReason = "USER_LOGOUT";
            session.UpdatedAt = now;
            session.ConcurrencyToken = Guid.NewGuid();

            RevokeRefreshTokens(session, now, "SESSION_LOGGED_OUT");

            await _dbContext.SaveChangesAsync(cancellationToken);

            await RecordLoginActivityAsync(
                eventType: "LOGOUT_SUCCEEDED",
                outcome: "SUCCESS",
                userId: userId,
                userSessionId: sessionId,
                userDeviceId: session.UserDeviceId,
                description: "The current session was logged out.");
        }


        // =====================================================
        // PUBLIC METHOD: GET MY LOGIN ACTIVITIES
        // =====================================================
        public async Task<PagedLoginActivityResponseDto> GetMyLoginActivitiesAsync(
            LoginActivityQueryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            long userId = _currentUser.GetRequiredUserId();
            Guid currentSessionId = _currentUser.GetRequiredSessionId();

            int page = request.Page < 1 ? 1 : request.Page;
            int pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

            var result = await _userRepository.GetUserLoginActivitiesAsync(
                userId,
                page,
                pageSize,
                request.EventType,
                request.Outcome);

            LoginActivityResponseDto[] items = result.Items
                .Select(activity => new LoginActivityResponseDto
                {
                    LoginActivityId = activity.LoginActivityId,
                    EventType = activity.EventType,
                    Outcome = activity.Outcome,
                    Description = activity.Description,
                    IpAddress = activity.IpAddress,
                    DeviceType = activity.DeviceType,
                    OperatingSystem = activity.OperatingSystem,
                    BrowserName = activity.BrowserName,
                    OccurredAt = activity.OccurredAt,
                    IsCurrentSession = activity.UserSessionId == currentSessionId
                })
                .ToArray();

            int totalPages = result.TotalCount == 0
                ? 0
                : (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            return new PagedLoginActivityResponseDto
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = result.TotalCount,
                TotalPages = totalPages
            };
        }


        // =====================================================
        // PRIVATE HELPER: GET OWNED SESSION
        // =====================================================
        private async Task<UserSession> GetRequiredOwnedSessionAsync(long userId, Guid sessionId)
        {
            UserSession? session = await _userRepository.GetOwnedSessionWithTokensAsync(userId, sessionId);

            if (session is null)
            {
                throw new KeyNotFoundException("The requested session was not found.");
            }

            return session;
        }


        // =====================================================
        // PRIVATE HELPER: REVOKE SESSION AND TOKENS
        // =====================================================
        private void RevokeSessionAndTokens(UserSession session, string reason)
        {
            if (session.Status != "ACTIVE")
            {
                return;
            }

            DateTime now = DateTime.UtcNow;

            session.Status = "REVOKED";
            session.RevokedAt = now;
            session.RevokedReason = reason;
            session.RevokedBy = "USER";
            session.UpdatedAt = now;
            session.ConcurrencyToken = Guid.NewGuid();

            RevokeRefreshTokens(session, now, reason);
        }


        // =====================================================
        // PRIVATE HELPER: REVOKE REFRESH TOKENS
        // =====================================================
        private void RevokeRefreshTokens(UserSession session, DateTime revokedAt, string reason)
        {
            string? revokedByIp = _requestInformation.GetIpAddress();

            foreach (RefreshToken token in session.RefreshTokens.Where(x => !x.IsRevoked))
            {
                token.IsRevoked = true;
                token.RevokedAt = revokedAt;
                token.RevokedReason = reason;
                token.RevokedByIp = revokedByIp;
            }
        }


    }
}