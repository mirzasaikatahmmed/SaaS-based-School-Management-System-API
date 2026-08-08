using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddGuardianReferenceNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "student_id",
                schema: "tenant_template",
                table: "guardians",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "tenant_template",
                table: "guardians",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_login_active",
                schema: "tenant_template",
                table: "guardians",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_no",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_guardians_reference_no",
                schema: "tenant_template",
                table: "guardians",
                column: "reference_no",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guardians_reference_no",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "is_login_active",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "reference_no",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.AlterColumn<Guid>(
                name: "student_id",
                schema: "tenant_template",
                table: "guardians",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
