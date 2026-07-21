using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenUserSessionRelationship
        : Migration
    {
        /// <inheritdoc />
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            // ========================================================
            // ADD USER SESSION FOREIGN-KEY COLUMN
            // ========================================================

            migrationBuilder.AddColumn<Guid>(
                name: "user_session_id",
                table: "refresh_tokens",
                type: "uuid",
                nullable: true);


            // ========================================================
            // CREATE USER SESSION INDEX
            // ========================================================

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_user_session",
                table: "refresh_tokens",
                column: "user_session_id");


            // ========================================================
            // CREATE USER SESSION FOREIGN KEY
            // ========================================================

            migrationBuilder.AddForeignKey(
                name:
                    "FK_refresh_tokens_user_sessions_user_session_id",
                table: "refresh_tokens",
                column: "user_session_id",
                principalTable: "user_sessions",
                principalColumn: "user_session_id",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            // ========================================================
            // REMOVE USER SESSION FOREIGN KEY
            // ========================================================

            migrationBuilder.DropForeignKey(
                name:
                    "FK_refresh_tokens_user_sessions_user_session_id",
                table: "refresh_tokens");


            // ========================================================
            // REMOVE USER SESSION INDEX
            // ========================================================

            migrationBuilder.DropIndex(
                name: "idx_refresh_tokens_user_session",
                table: "refresh_tokens");


            // ========================================================
            // REMOVE USER SESSION COLUMN
            // ========================================================

            migrationBuilder.DropColumn(
                name: "user_session_id",
                table: "refresh_tokens");
        }
    }
}