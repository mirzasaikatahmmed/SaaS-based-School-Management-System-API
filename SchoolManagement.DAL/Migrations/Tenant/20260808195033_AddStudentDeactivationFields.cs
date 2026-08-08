using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddStudentDeactivationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "deactivate_reason",
                schema: "tenant_template",
                table: "students",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deactivated_at",
                schema: "tenant_template",
                table: "students",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deactivated_by",
                schema: "tenant_template",
                table: "students",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deactivate_reason",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropColumn(
                name: "deactivated_at",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropColumn(
                name: "deactivated_by",
                schema: "tenant_template",
                table: "students");
        }
    }
}
