using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddBiometricModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "biometric_devices",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    device_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "K40-H"),
                    exam_grace_minutes_before = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    exam_grace_minutes_after = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_devices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "biometric_user_maps",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_pin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    person_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Student"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_user_maps", x => x.id);
                    table.ForeignKey(
                        name: "FK_biometric_user_maps_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_biometric_user_maps_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "biometric_punch_logs",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    device_sn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_pin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    punch_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    punch_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unmapped"),
                    status_applied = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Present"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_line = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_punch_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_biometric_punch_logs_biometric_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "tenant_template",
                        principalTable: "biometric_devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_biometric_punch_logs_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_biometric_punch_logs_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_biometric_devices_serial_number",
                schema: "tenant_template",
                table: "biometric_devices",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_biometric_punch_logs_pin",
                schema: "tenant_template",
                table: "biometric_punch_logs",
                column: "device_pin");

            migrationBuilder.CreateIndex(
                name: "idx_biometric_punch_logs_time",
                schema: "tenant_template",
                table: "biometric_punch_logs",
                column: "punch_time");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_punch_logs_device_id",
                schema: "tenant_template",
                table: "biometric_punch_logs",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_punch_logs_employee_id",
                schema: "tenant_template",
                table: "biometric_punch_logs",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_punch_logs_student_id",
                schema: "tenant_template",
                table: "biometric_punch_logs",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_user_maps_device_pin",
                schema: "tenant_template",
                table: "biometric_user_maps",
                column: "device_pin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biometric_user_maps_employee_id",
                schema: "tenant_template",
                table: "biometric_user_maps",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_user_maps_student_id",
                schema: "tenant_template",
                table: "biometric_user_maps",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "biometric_punch_logs",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "biometric_user_maps",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "biometric_devices",
                schema: "tenant_template");
        }
    }
}
