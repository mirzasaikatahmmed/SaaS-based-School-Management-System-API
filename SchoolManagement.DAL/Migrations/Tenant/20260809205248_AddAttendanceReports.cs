using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAttendanceReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "weekend_days",
                schema: "tenant_template",
                table: "school_settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "5,6");

            migrationBuilder.CreateTable(
                name: "student_subject_attendance",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Present"),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_subject_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_subject_attendance_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_attendance_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_attendance_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_attendance_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_attendance_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "idx_student_subject_att_date",
                schema: "tenant_template",
                table: "student_subject_attendance",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_attendance_class_id",
                schema: "tenant_template",
                table: "student_subject_attendance",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_attendance_created_by",
                schema: "tenant_template",
                table: "student_subject_attendance",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_attendance_section_id",
                schema: "tenant_template",
                table: "student_subject_attendance",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_attendance_student_id_subject_id_attendance~",
                schema: "tenant_template",
                table: "student_subject_attendance",
                columns: new[] { "student_id", "subject_id", "attendance_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_attendance_subject_id",
                schema: "tenant_template",
                table: "student_subject_attendance",
                column: "subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_subject_attendance",
                schema: "tenant_template");

            migrationBuilder.DropColumn(
                name: "weekend_days",
                schema: "tenant_template",
                table: "school_settings");
        }
    }
}
