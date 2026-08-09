using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddEmployeeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "designations",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_designations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_import_batches",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    total_rows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    success_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    failed_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    imported_by = table.Column<Guid>(type: "uuid", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_import_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    designation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    joining_date = table.Column<DateTime>(type: "date", nullable: false),
                    qualification = table.Column<string>(type: "text", nullable: true),
                    experience_details = table.Column<string>(type: "text", nullable: true),
                    total_experience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    religion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    blood_group = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    mobile_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    present_address = table.Column<string>(type: "text", nullable: true),
                    permanent_address = table.Column<string>(type: "text", nullable: true),
                    nid_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    profile_picture_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    twitter_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    linkedin_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    skip_bank_details = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    holder_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_branch = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    bank_address = table.Column<string>(type: "text", nullable: true),
                    ifsc_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    account_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                    table.ForeignKey(
                        name: "FK_employees_departments_department_id",
                        column: x => x.department_id,
                        principalSchema: "tenant_template",
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employees_designations_designation_id",
                        column: x => x.designation_id,
                        principalSchema: "tenant_template",
                        principalTable: "designations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_employees_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employee_import_batch_rows",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    raw_data = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_import_batch_rows", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_import_batch_rows_employee_import_batches_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "tenant_template",
                        principalTable: "employee_import_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_import_batch_rows_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_import_batch_rows_batch_id",
                schema: "tenant_template",
                table: "employee_import_batch_rows",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_import_batch_rows_employee_id",
                schema: "tenant_template",
                table: "employee_import_batch_rows",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_department_id",
                schema: "tenant_template",
                table: "employees",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_designation_id",
                schema: "tenant_template",
                table: "employees",
                column: "designation_id");

            migrationBuilder.CreateIndex(
                name: "IX_employees_role",
                schema: "tenant_template",
                table: "employees",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_employees_staff_id",
                schema: "tenant_template",
                table: "employees",
                column: "staff_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_user_id",
                schema: "tenant_template",
                table: "employees",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_import_batch_rows",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "employee_import_batches",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "departments",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "designations",
                schema: "tenant_template");
        }
    }
}
