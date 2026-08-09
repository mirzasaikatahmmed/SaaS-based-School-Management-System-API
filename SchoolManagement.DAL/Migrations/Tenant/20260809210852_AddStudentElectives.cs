using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddStudentElectives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "elective_group",
                schema: "tenant_template",
                table: "class_subject_assignment_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_elective",
                schema: "tenant_template",
                table: "class_subject_assignment_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "student_subject_enrollments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    elective_group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "4th"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_subject_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_subject_enrollments_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_enrollments_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_subject_enrollments_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_subject_enrollments_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_class_id_section_id_academic_ye~",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                columns: new[] { "class_id", "section_id", "academic_year", "elective_group" });

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_section_id",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_student_id_elective_group_acade~",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                columns: new[] { "student_id", "elective_group", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_student_id_subject_id_academic_~",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                columns: new[] { "student_id", "subject_id", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                column: "subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_subject_enrollments",
                schema: "tenant_template");

            migrationBuilder.DropColumn(
                name: "elective_group",
                schema: "tenant_template",
                table: "class_subject_assignment_items");

            migrationBuilder.DropColumn(
                name: "is_elective",
                schema: "tenant_template",
                table: "class_subject_assignment_items");
        }
    }
}
