using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginTwoFactorAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "login_challenge_id",
                table: "email_otp_tokens",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_email_otp_tokens_login_challenges_login_challenge_id",
                table: "email_otp_tokens",
                column: "login_challenge_id",
                principalTable: "login_challenges",
                principalColumn: "login_challenge_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_otp_tokens_login_challenges_login_challenge_id",
                table: "email_otp_tokens");

            migrationBuilder.DropTable(
                name: "login_challenges");

            migrationBuilder.DropIndex(
                name: "ix_email_otp_tokens_challenge_id_purpose",
                table: "email_otp_tokens");

            migrationBuilder.DropColumn(
                name: "login_challenge_id",
                table: "email_otp_tokens");
        }
    }
}
