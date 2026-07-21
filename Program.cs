using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using smartApi.Authentication.Repositories;
using smartApi.Authentication.Repositories.Interface;
using smartApi.Authentication.Services;
using smartApi.Authentication.Services.Interfaces;
using smartApi.Data;
using smartApi.ExceptionHandling.Handlers;
using smartApi.Utility.Configurations;
using smartApi.Utility.Current_User;
using smartApi.Utility.Device_Information;
using smartApi.Utility.Http_Request;
using smartApi.Utility.PasswordHasher_Security;
using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();


// ============================================================
// JWT CONFIGURATION
// ============================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>();

if (jwtSettings == null)
{
    throw new InvalidOperationException(
        "JWT settings are missing."
    );
}

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JWT signing key is missing."
    );
}

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
{
    throw new InvalidOperationException(
        "JWT issuer is missing."
    );
}

if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "JWT audience is missing."
    );
}


// ============================================================
// DATABASE CONFIGURATION
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection"
    );

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection connection string is missing."
    );
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
    {
        options.UseNpgsql(connectionString);
    }
);


// ============================================================
// CORS CONFIGURATION
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendPolicy",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:5174",
                    "http://localhost:5175"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(
                    TimeSpan.FromMinutes(10)
                );
        }
    );
});


// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                jwtSettings.Issuer,

            ValidAudience =
                jwtSettings.Audience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        jwtSettings.Key
                    )
                ),

            /*
             * Access tokens become invalid immediately after
             * their expiration time.
             */
            ClockSkew = TimeSpan.Zero
        };
});


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// TWO-FACTOR AUTHENTICATION OPTIONS
// ============================================================

builder.Services
    .AddOptions<TwoFactorAuthenticationOptions>()
    .Bind(
        builder.Configuration.GetSection(
            TwoFactorAuthenticationOptions.SectionName
        )
    )
    .Validate(
        options =>
            options.LoginOtpExpiryMinutes > 0 &&
            options.LoginChallengeExpiryMinutes > 0 &&
            options.LoginOtpMaxAttempts > 0 &&
            options.LoginOtpMaxResends >= 0 &&
            options.LoginOtpResendCooldownSeconds > 0 &&
            options.MaximumFailedPasswordAttempts > 0 &&
            options.AccountLockDurationMinutes > 0 &&
            !string.IsNullOrWhiteSpace(
                options.DummyPasswordHash
            ),
        "Two-factor authentication configuration is invalid."
    )
    .ValidateOnStart();


// ============================================================
// RATE LIMITING
// ============================================================

builder.Services.AddRateLimiter(options =>
{
    /*
     * Default status code returned when a request exceeds
     * a rate-limit policy.
     */
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    /*
     * Custom response returned for rate-limited requests.
     */
    options.OnRejected = async (
        context,
        cancellationToken) =>
    {
        var retryAfterSeconds = 60;

        /*
         * Some limiters provide RetryAfter metadata.
         */
        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter,
            out var retryAfter))
        {
            retryAfterSeconds =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        retryAfter.TotalSeconds
                    )
                );
        }

        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        context.HttpContext.Response.Headers.RetryAfter =
            retryAfterSeconds.ToString(
                CultureInfo.InvariantCulture
            );

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                status =
                    StatusCodes.Status429TooManyRequests,

                title =
                    "Too many requests",

                detail =
                    "Please wait before trying again.",

                retryAfterSeconds
            },
            cancellationToken: cancellationToken
        );
    };


    // ========================================================
    // LOGIN PASSWORD POLICY
    //
    // Applied to:
    // POST /api/auth/login
    // ========================================================

    options.AddPolicy(
        "login-password-policy",
        httpContext =>
        {
            var partitionKey =
                GetRateLimitPartitionKey(
                    httpContext,
                    "login-password"
                );

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            /*
                             * Maximum five login requests from
                             * one IP address every minute.
                             */
                            PermitLimit = 5,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst,

                            QueueLimit = 0,

                            AutoReplenishment = true
                        }
                );
        }
    );


    // ========================================================
    // LOGIN OTP VERIFICATION POLICY
    //
    // Applied to:
    // POST /api/auth/login/verify-otp
    // ========================================================

    options.AddPolicy(
        "login-otp-policy",
        httpContext =>
        {
            var partitionKey =
                GetRateLimitPartitionKey(
                    httpContext,
                    "login-otp"
                );

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            /*
                             * The endpoint limiter protects
                             * the API.
                             *
                             * The OTP database record separately
                             * enforces MaxAttempts for a challenge.
                             */
                            PermitLimit = 10,

                            Window =
                                TimeSpan.FromMinutes(5),

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst,

                            QueueLimit = 0,

                            AutoReplenishment = true
                        }
                );
        }
    );


    // ========================================================
    // LOGIN OTP RESEND POLICY
    //
    // Applied to:
    // POST /api/auth/login/resend-otp
    // ========================================================

    options.AddPolicy(
        "login-resend-policy",
        httpContext =>
        {
            var partitionKey =
                GetRateLimitPartitionKey(
                    httpContext,
                    "login-resend"
                );

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            /*
                             * Maximum five resend requests from
                             * one IP address every ten minutes.
                             *
                             * AuthService also enforces:
                             *
                             * - Resend cooldown
                             * - Maximum resend count
                             */
                            PermitLimit = 5,

                            Window =
                                TimeSpan.FromMinutes(10),

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst,

                            QueueLimit = 0,

                            AutoReplenishment = true
                        }
                );
        }
    );


    // ========================================================
    // FORGOT PASSWORD POLICY
    //
    // Applied to:
    // POST /api/auth/forgot-password
    // ========================================================

    options.AddPolicy(
        "forgot-password-policy",
        httpContext =>
        {
            var partitionKey =
                GetRateLimitPartitionKey(
                    httpContext,
                    "forgot-password"
                );

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            /*
                             * Maximum three forgot-password
                             * requests from one IP address every
                             * fifteen minutes.
                             */
                            PermitLimit = 3,

                            Window =
                                TimeSpan.FromMinutes(15),

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst,

                            QueueLimit = 0,

                            AutoReplenishment = true
                        }
                );
        }
    );


    // ========================================================
    // RESET PASSWORD POLICY
    //
    // Applied to:
    // POST /api/auth/reset-password
    // ========================================================

    options.AddPolicy(
        "reset-password-policy",
        httpContext =>
        {
            var partitionKey =
                GetRateLimitPartitionKey(
                    httpContext,
                    "reset-password"
                );

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey,
                    _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            /*
                             * Maximum five password-reset attempts
                             * from one IP address every fifteen
                             * minutes.
                             */
                            PermitLimit = 5,

                            Window =
                                TimeSpan.FromMinutes(15),

                            QueueProcessingOrder =
                                QueueProcessingOrder
                                    .OldestFirst,

                            QueueLimit = 0,

                            AutoReplenishment = true
                        }
                );
        }
    );
});


// ============================================================
// HTTP CONTEXT ACCESS
// ============================================================

/*
 * IHttpContextAccessor allows helper classes and AuthService
 * dependencies to safely access the current HttpContext.
 *
 * It is used for:
 *
 * - Reading the client IP address
 * - Reading User-Agent
 * - Reading X-Device-Id
 * - Reading authenticated user claims
 * - Reading the current sid session claim
 */
builder.Services.AddHttpContextAccessor();


// ============================================================
// PASSWORD HASHER
// ============================================================

builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher
>();


// ============================================================
// REPOSITORY
// ============================================================

/*
 * The existing UserRepository manages user-related authentication
 * data, including:
 *
 * - Users
 * - User devices
 * - User sessions
 * - Login activities
 *
 * Separate repositories are not registered for the new tables.
 */
builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();


// ============================================================
// AUTHENTICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    smartApi.Authentication.Services.Interfaces.IAuthService,
    smartApi.Authentication.Services.AuthService
>();

builder.Services.AddScoped<
    smartApi.Authentication.Services.Interfaces.IProfileService,
    smartApi.Authentication.Services.ProfileService
>();

builder.Services.AddScoped<
    smartApi.Authentication.Services.Interfaces.ITokenService,
    smartApi.Authentication.Services.TokenService
>();

builder.Services.AddScoped<
    smartApi.Authentication.Services.Interfaces.IOtpService,
    smartApi.Authentication.Services.OtpService
>();



// ============================================================
// SESSION, DEVICE, AND REQUEST HELPER CLASSES
// ============================================================

/*
 * These helper classes support AuthService.
 *
 * No separate service interfaces are required for these helpers.
 */

builder.Services.AddScoped<
    RequestInformationHelper
>();

builder.Services.AddScoped<
    CurrentUserHelper
>();

builder.Services.AddScoped<
    DeviceInformationParser
>();

builder.Services.AddScoped<
    DeviceFingerprintHelper
>();


// ============================================================
// EMAIL CONFIGURATION AND SERVICE
// ============================================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        "EmailSettings"
    )
);

builder.Services.AddScoped<
    IEmailService,
    EmailService
>();


// ============================================================
// GLOBAL EXCEPTION HANDLING
// ============================================================

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    ValidationExceptionHandler
>();

builder.Services.AddExceptionHandler<
    SecurityExceptionHandler
>();

builder.Services.AddExceptionHandler<
    DomainExceptionHandler
>();

builder.Services.AddExceptionHandler<
    InfrastructureExceptionHandler
>();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler
>();


// ============================================================
// OPENAPI
// ============================================================

builder.Services.AddOpenApi();


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// DEVELOPMENT OPENAPI
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

/*
 * Global exception handling should run early so exceptions
 * generated by later middleware and controllers can be handled.
 */
app.UseExceptionHandler();

app.UseHttpsRedirection();

/*
 * Endpoint routing is enabled before middleware that reads
 * endpoint metadata, including named rate-limit policies.
 */
app.UseRouting();

app.UseCors("FrontendPolicy");

/*
 * Authentication must run before authorization.
 *
 * Authentication validates the JWT and creates HttpContext.User.
 */
app.UseAuthentication();

/*
 * Rate limiting runs before controller actions execute.
 *
 * EnableRateLimiting attributes on controller actions select
 * the appropriate named policies.
 */
app.UseRateLimiter();

/*
 * Authorization checks Authorize attributes and access rules.
 */
app.UseAuthorization();

app.MapControllers();

app.Run();


// ============================================================
// RATE-LIMIT PARTITION HELPER
// ============================================================

static string GetRateLimitPartitionKey(
    HttpContext httpContext,
    string policyPrefix)
{
    string ipAddress =
        httpContext.Connection
            .RemoteIpAddress?
            .ToString()
        ?? "unknown";

    /*
     * The policy prefix prevents different endpoints from
     * sharing the same rate-limit counter.
     *
     * Examples:
     *
     * login-password:127.0.0.1
     * login-otp:127.0.0.1
     * login-resend:127.0.0.1
     * forgot-password:127.0.0.1
     * reset-password:127.0.0.1
     */
    return $"{policyPrefix}:{ipAddress}";
}