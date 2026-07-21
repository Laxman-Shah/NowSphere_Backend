using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class reloading_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_otp_tokens", x => x.email_otp_token_id);
                    table.ForeignKey(
                        name: "FK_email_otp_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_otp_tokens");
        }
    }
}
