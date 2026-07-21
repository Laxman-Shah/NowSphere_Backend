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

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings is null)
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

            ValidIssuer = jwtSettings.Issuer,

            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        jwtSettings.Key
                    )
                ),

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
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (
        context,
        cancellationToken) =>
    {
        var retryAfterSeconds = 60;

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

builder.Services.AddScoped<
    IUserRepository,
    UserRepository
>();


// ============================================================
// AUTHENTICATION SERVICES
// ============================================================

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IProfileService,
    ProfileService
>();

builder.Services.AddScoped<
    ITokenService,
    TokenService
>();

builder.Services.AddScoped<
    IOtpService,
    OtpService
>();


// ============================================================
// SESSION, DEVICE, AND REQUEST HELPERS
// ============================================================

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
// DATABASE MIGRATIONS
// ============================================================

var applyMigrations = string.Equals(
    Environment.GetEnvironmentVariable(
        "APPLY_MIGRATIONS"
    ),
    "true",
    StringComparison.OrdinalIgnoreCase
);

if (applyMigrations)
{
    using var scope = app.Services.CreateScope();

    try
    {
        app.Logger.LogInformation(
            "Checking for pending database migrations."
        );

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var pendingMigrations = (
            await dbContext.Database
                .GetPendingMigrationsAsync()
        ).ToList();

        if (pendingMigrations.Count > 0)
        {
            app.Logger.LogInformation(
                "Applying {MigrationCount} pending database migrations.",
                pendingMigrations.Count
            );

            foreach (var migration in pendingMigrations)
            {
                app.Logger.LogInformation(
                    "Pending migration: {MigrationName}",
                    migration
                );
            }

            await dbContext.Database.MigrateAsync();

            app.Logger.LogInformation(
                "Database migrations completed successfully."
            );
        }
        else
        {
            app.Logger.LogInformation(
                "No pending database migrations were found."
            );
        }
    }
    catch (Exception exception)
    {
        app.Logger.LogCritical(
            exception,
            "Database migration failed during application startup."
        );

        throw;
    }
}


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

app.UseExceptionHandler();

/*
 * Render manages HTTPS outside the application container.
 * HTTPS redirection is used only while developing locally.
 */
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("FrontendPolicy");

app.UseAuthentication();

app.UseRateLimiter();

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
    var ipAddress =
        httpContext.Connection
            .RemoteIpAddress?
            .ToString()
        ?? "unknown";

    return $"{policyPrefix}:{ipAddress}";
}
