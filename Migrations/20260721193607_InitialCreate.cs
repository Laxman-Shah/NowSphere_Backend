using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    account_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SecurityVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "login_challenges",
                columns: table => new
                {
                    login_challenge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resend_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_otp_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concurrency_token = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_challenges", x => x.login_challenge_id);
                    table.ForeignKey(
                        name: "FK_login_challenges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_credentials",
                columns: table => new
                {
                    credential_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "BCrypt"),
                    password_salt = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    password_created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    password_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_credentials", x => x.credential_id);
                    table.ForeignKey(
                        name: "FK_user_credentials_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    user_device_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    device_fingerprint_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    device_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    device_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "UNKNOWN"),
                    operating_system = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operating_system_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    browser_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    browser_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    last_user_agent = table.Column<string>(type: "text", nullable: true),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    is_trusted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    trusted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trust_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trust_revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trust_revoked_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deactivation_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_devices", x => x.user_device_id);
                    table.ForeignKey(
                        name: "FK_user_devices_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_role_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    assigned_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.user_role_id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_revoked_by_user_id",
                        column: x => x.revoked_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "email_otp_tokens",
                columns: table => new
                {
                    email_otp_token_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    sent_to_email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    purpose = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    resend_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    login_challenge_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_otp_tokens", x => x.email_otp_token_id);
                    table.ForeignKey(
                        name: "FK_email_otp_tokens_login_challenges_login_challenge_id",
                        column: x => x.login_challenge_id,
                        principalTable: "login_challenges",
                        principalColumn: "login_challenge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_email_otp_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    user_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_device_id = table.Column<long>(type: "bigint", nullable: false),
                    login_challenge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE"),
                    authentication_level = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PASSWORD_OTP"),
                    authentication_methods = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    otp_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    otp_verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    login_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    login_user_agent = table.Column<string>(type: "text", nullable: true),
                    last_ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    last_user_agent = table.Column<string>(type: "text", nullable: true),
                    login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    logged_out_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    logout_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    revoked_by = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concurrency_token = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.user_session_id);
                    table.ForeignKey(
                        name: "FK_user_sessions_login_challenges_login_challenge_id",
                        column: x => x.login_challenge_id,
                        principalTable: "login_challenges",
                        principalColumn: "login_challenge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_sessions_user_devices_user_device_id",
                        column: x => x.user_device_id,
                        principalTable: "user_devices",
                        principalColumn: "user_device_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginActivities",
                columns: table => new
                {
                    LoginActivityId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserSessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserDeviceId = table.Column<long>(type: "bigint", nullable: true),
                    LoginChallengeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Outcome = table.Column<string>(type: "text", nullable: false),
                    AttemptedIdentifier = table.Column<string>(type: "text", nullable: true),
                    FailureCode = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    OperatingSystem = table.Column<string>(type: "text", nullable: true),
                    BrowserName = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginActivities", x => x.LoginActivityId);
                    table.ForeignKey(
                        name: "FK_LoginActivities_login_challenges_LoginChallengeId",
                        column: x => x.LoginChallengeId,
                        principalTable: "login_challenges",
                        principalColumn: "login_challenge_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoginActivities_user_devices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "user_devices",
                        principalColumn: "user_device_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoginActivities_user_sessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "user_sessions",
                        principalColumn: "user_session_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LoginActivities_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    refresh_token_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    user_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    token_family_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    replaced_by_token_id = table.Column<long>(type: "bigint", nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.refresh_token_id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_refresh_tokens_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "refresh_token_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_user_sessions_user_session_id",
                        column: x => x.user_session_id,
                        principalTable: "user_sessions",
                        principalColumn: "user_session_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "created_at", "description", "is_system_role", "role_name", "updated_at" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator with full access", true, "ADMIN", null },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Default normal user role", true, "USER", null },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Manager role with limited administrative access", true, "MANAGER", null },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Support role for customer or user assistance", true, "SUPPORT", null }
                });

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_created_at",
                table: "email_otp_tokens",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_expiry",
                table: "email_otp_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_lookup",
                table: "email_otp_tokens",
                columns: new[] { "user_id", "purpose", "used_at" });

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_revoked_at",
                table: "email_otp_tokens",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_sent_to_email",
                table: "email_otp_tokens",
                column: "sent_to_email");

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_user_purpose_expiry",
                table: "email_otp_tokens",
                columns: new[] { "user_id", "purpose", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_email_otp_tokens_user_purpose_revoked",
                table: "email_otp_tokens",
                columns: new[] { "user_id", "purpose", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "ix_email_otp_tokens_challenge_id_purpose",
                table: "email_otp_tokens",
                columns: new[] { "login_challenge_id", "purpose" });

            migrationBuilder.CreateIndex(
                name: "ix_login_challenges_expires_at",
                table: "login_challenges",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_challenges_status_expires_at",
                table: "login_challenges",
                columns: new[] { "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_login_challenges_user_id_status",
                table: "login_challenges",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginActivities_LoginChallengeId",
                table: "LoginActivities",
                column: "LoginChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginActivities_UserDeviceId",
                table: "LoginActivities",
                column: "UserDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginActivities_UserId",
                table: "LoginActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginActivities_UserSessionId",
                table: "LoginActivities",
                column: "UserSessionId");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_family",
                table: "refresh_tokens",
                column: "token_family_id");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_revoked_at",
                table: "refresh_tokens",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token_lookup",
                table: "refresh_tokens",
                columns: new[] { "token_hash", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_active",
                table: "refresh_tokens",
                columns: new[] { "user_id", "is_revoked" });

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_expiry",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_session",
                table: "refresh_tokens",
                column: "user_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_replaced_by_token_id",
                table: "refresh_tokens",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "uq_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_name",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_credentials_user_id",
                table: "user_credentials",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_devices_last_seen",
                table: "user_devices",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_devices_trust_expiry",
                table: "user_devices",
                column: "trust_expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_devices_user_active",
                table: "user_devices",
                columns: new[] { "user_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "idx_user_devices_user_trusted",
                table: "user_devices",
                columns: new[] { "user_id", "is_trusted" });

            migrationBuilder.CreateIndex(
                name: "uq_user_devices_user_fingerprint",
                table: "user_devices",
                columns: new[] { "user_id", "device_fingerprint_hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_assigned_by_user_id",
                table: "user_roles",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_revoked_by_user_id",
                table: "user_roles",
                column: "revoked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_device_status",
                table: "user_sessions",
                columns: new[] { "user_device_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_expiry",
                table: "user_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_last_activity",
                table: "user_sessions",
                column: "last_activity_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_revoked",
                table: "user_sessions",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_user_expiry",
                table: "user_sessions",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_user_sessions_user_status",
                table: "user_sessions",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_user_sessions_login_challenge",
                table: "user_sessions",
                column: "login_challenge_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_otp_tokens");

            migrationBuilder.DropTable(
                name: "LoginActivities");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_credentials");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "login_challenges");

            migrationBuilder.DropTable(
                name: "user_devices");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
