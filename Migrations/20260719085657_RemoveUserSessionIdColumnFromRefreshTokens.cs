using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smartApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserSessionIdColumnFromRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op - column already removed in previous migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op - column already removed in previous migration
        }
    }
}
