using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPasswordRevealAndStudentReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password_reveal_encrypted",
                schema: "tenant_template",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "activated_gateway",
                schema: "tenant_template",
                table: "sms_settings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "bulksmsbd",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "textlocal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_reveal_encrypted",
                schema: "tenant_template",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "activated_gateway",
                schema: "tenant_template",
                table: "sms_settings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "textlocal",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "bulksmsbd");
        }
    }
}
