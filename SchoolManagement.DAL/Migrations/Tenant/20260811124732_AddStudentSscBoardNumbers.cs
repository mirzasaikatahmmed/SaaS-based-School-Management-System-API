using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddStudentSscBoardNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ssc_registration_no",
                schema: "tenant_template",
                table: "students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ssc_roll",
                schema: "tenant_template",
                table: "students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_ssc_roll",
                schema: "tenant_template",
                table: "students",
                column: "ssc_roll",
                unique: true,
                filter: "ssc_roll IS NOT NULL AND btrim(ssc_roll) <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_students_ssc_roll",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropColumn(
                name: "ssc_registration_no",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropColumn(
                name: "ssc_roll",
                schema: "tenant_template",
                table: "students");
        }
    }
}
