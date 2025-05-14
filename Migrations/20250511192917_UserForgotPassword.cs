using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS_backend.Migrations
{
    /// <inheritdoc />
    public partial class UserForgotPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetCode",
                table: "User",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetCodeExpiry",
                table: "User",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetCode",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PasswordResetCodeExpiry",
                table: "User");
        }
    }
}
