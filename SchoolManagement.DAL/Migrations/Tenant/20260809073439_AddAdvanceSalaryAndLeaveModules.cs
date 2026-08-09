using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAdvanceSalaryAndLeaveModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "advance_salary_requests",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deduct_month = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reject_reason = table.Column<string>(type: "text", nullable: true),
                    applied_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_advance_salary_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_advance_salary_requests_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_advance_salary_requests_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "leave_categories",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leave_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_of_start = table.Column<DateTime>(type: "date", nullable: false),
                    date_of_end = table.Column<DateTime>(type: "date", nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    attachment_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    comments = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    apply_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_leave_requests_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leave_requests_leave_categories_leave_category_id",
                        column: x => x.leave_category_id,
                        principalSchema: "tenant_template",
                        principalTable: "leave_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leave_requests_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_advance_salary_requests_deduct_month",
                schema: "tenant_template",
                table: "advance_salary_requests",
                column: "deduct_month");

            migrationBuilder.CreateIndex(
                name: "IX_advance_salary_requests_employee_id",
                schema: "tenant_template",
                table: "advance_salary_requests",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_advance_salary_requests_reviewed_by",
                schema: "tenant_template",
                table: "advance_salary_requests",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_advance_salary_requests_status",
                schema: "tenant_template",
                table: "advance_salary_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_date_of_start_date_of_end",
                schema: "tenant_template",
                table: "leave_requests",
                columns: new[] { "date_of_start", "date_of_end" });

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_employee_id",
                schema: "tenant_template",
                table: "leave_requests",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_leave_category_id",
                schema: "tenant_template",
                table: "leave_requests",
                column: "leave_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_reviewed_by",
                schema: "tenant_template",
                table: "leave_requests",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_status",
                schema: "tenant_template",
                table: "leave_requests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "advance_salary_requests",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "leave_requests",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "leave_categories",
                schema: "tenant_template");
        }
    }
}
