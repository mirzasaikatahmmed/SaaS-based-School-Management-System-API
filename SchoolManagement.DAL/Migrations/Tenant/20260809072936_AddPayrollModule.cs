using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPayrollModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "salary_templates",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    salary_grade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    basic_salary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    overtime_rate_per_hour = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    total_allowance = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    total_deduction = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    net_salary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_salary_assignments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_salary_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_salary_assignments_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_salary_assignments_salary_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "tenant_template",
                        principalTable: "salary_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_salary_assignments_users_assigned_by",
                        column: x => x.assigned_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "salary_allowances",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_allowances", x => x.id);
                    table.ForeignKey(
                        name: "FK_salary_allowances_salary_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "tenant_template",
                        principalTable: "salary_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "salary_deductions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_deductions", x => x.id);
                    table.ForeignKey(
                        name: "FK_salary_deductions_salary_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "tenant_template",
                        principalTable: "salary_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "salary_payments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_month = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    basic_salary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_allowance = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    total_deduction = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    net_salary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    overtime_hours = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false, defaultValue: 0m),
                    overtime_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    advance_deduction = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    final_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid"),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_note = table.Column<string>(type: "text", nullable: true),
                    paid_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_salary_payments_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salary_payments_salary_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "tenant_template",
                        principalTable: "salary_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salary_payments_users_paid_by",
                        column: x => x.paid_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_salary_assignments_assigned_by",
                schema: "tenant_template",
                table: "employee_salary_assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_employee_salary_assignments_employee_id",
                schema: "tenant_template",
                table: "employee_salary_assignments",
                column: "employee_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_salary_assignments_template_id",
                schema: "tenant_template",
                table: "employee_salary_assignments",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_salary_allowances_template_id",
                schema: "tenant_template",
                table: "salary_allowances",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_salary_deductions_template_id",
                schema: "tenant_template",
                table: "salary_deductions",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_employee_id",
                schema: "tenant_template",
                table: "salary_payments",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_employee_id_payment_month",
                schema: "tenant_template",
                table: "salary_payments",
                columns: new[] { "employee_id", "payment_month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_paid_by",
                schema: "tenant_template",
                table: "salary_payments",
                column: "paid_by");

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_payment_month",
                schema: "tenant_template",
                table: "salary_payments",
                column: "payment_month");

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_status",
                schema: "tenant_template",
                table: "salary_payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_salary_payments_template_id",
                schema: "tenant_template",
                table: "salary_payments",
                column: "template_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_salary_assignments",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "salary_allowances",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "salary_deductions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "salary_payments",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "salary_templates",
                schema: "tenant_template");
        }
    }
}
