using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionDeviceLoginActivityManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginActivities");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "user_devices");
        }
    }
}
