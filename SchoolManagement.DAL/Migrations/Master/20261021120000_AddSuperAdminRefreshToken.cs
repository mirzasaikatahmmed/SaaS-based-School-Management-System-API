using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddSuperAdminRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                schema: "public",
                table: "super_admins",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "refresh_token_expires_at",
                schema: "public",
                table: "super_admins",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refresh_token",
                schema: "public",
                table: "super_admins");

            migrationBuilder.DropColumn(
                name: "refresh_token_expires_at",
                schema: "public",
                table: "super_admins");
        }
    }
}
