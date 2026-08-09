using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAwardModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "awards",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    award_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gift_item = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cash_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    award_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    given_date = table.Column<DateTime>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_awards", x => x.id);
                    table.CheckConstraint("chk_award_recipient", "(employee_id IS NOT NULL AND student_id IS NULL) OR (employee_id IS NULL AND student_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_awards_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_awards_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_awards_employee_id",
                schema: "tenant_template",
                table: "awards",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_awards_given_date",
                schema: "tenant_template",
                table: "awards",
                column: "given_date");

            migrationBuilder.CreateIndex(
                name: "IX_awards_student_id",
                schema: "tenant_template",
                table: "awards",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "awards",
                schema: "tenant_template");
        }
    }
}
