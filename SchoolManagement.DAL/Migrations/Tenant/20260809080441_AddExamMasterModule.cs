using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddExamMasterModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exam_halls",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    hall_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    no_of_seats = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_halls", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_terms",
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
                    table.PrimaryKey("PK_exam_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mark_distributions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mark_distributions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    exam_term_id = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_result_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams", x => x.id);
                    table.ForeignKey(
                        name: "FK_exams_exam_terms_exam_term_id",
                        column: x => x.exam_term_id,
                        principalSchema: "tenant_template",
                        principalTable: "exam_terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "exam_mark_distributions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mark_distribution_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_mark_distributions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_mark_distributions_exams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "tenant_template",
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_mark_distributions_mark_distributions_mark_distributio~",
                        column: x => x.mark_distribution_id,
                        principalSchema: "tenant_template",
                        principalTable: "mark_distributions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_schedules",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_schedules_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_schedules_exams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "tenant_template",
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_schedules_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mark_entries",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_absent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    written_mark = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    mcq_mark = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    total_mark = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mark_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_mark_entries_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mark_entries_exams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "tenant_template",
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mark_entries_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mark_entries_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_mark_entries_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_schedule_subjects",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_date = table.Column<DateTime>(type: "date", nullable: false),
                    starting_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    ending_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    hall_id = table.Column<Guid>(type: "uuid", nullable: true),
                    written_full_mark = table.Column<int>(type: "integer", nullable: true),
                    written_pass_mark = table.Column<int>(type: "integer", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_schedule_subjects", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_schedule_subjects_exam_halls_hall_id",
                        column: x => x.hall_id,
                        principalSchema: "tenant_template",
                        principalTable: "exam_halls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_exam_schedule_subjects_exam_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "tenant_template",
                        principalTable: "exam_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_schedule_subjects_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_halls_hall_no",
                schema: "tenant_template",
                table: "exam_halls",
                column: "hall_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_mark_distributions_exam_id_mark_distribution_id",
                schema: "tenant_template",
                table: "exam_mark_distributions",
                columns: new[] { "exam_id", "mark_distribution_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_mark_distributions_mark_distribution_id",
                schema: "tenant_template",
                table: "exam_mark_distributions",
                column: "mark_distribution_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_subjects_hall_id",
                schema: "tenant_template",
                table: "exam_schedule_subjects",
                column: "hall_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_subjects_schedule_id",
                schema: "tenant_template",
                table: "exam_schedule_subjects",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedule_subjects_subject_id",
                schema: "tenant_template",
                table: "exam_schedule_subjects",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedules_class_id",
                schema: "tenant_template",
                table: "exam_schedules",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedules_exam_id_class_id_section_id",
                schema: "tenant_template",
                table: "exam_schedules",
                columns: new[] { "exam_id", "class_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_schedules_section_id",
                schema: "tenant_template",
                table: "exam_schedules",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_terms_name",
                schema: "tenant_template",
                table: "exam_terms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exams_exam_term_id",
                schema: "tenant_template",
                table: "exams",
                column: "exam_term_id");

            migrationBuilder.CreateIndex(
                name: "IX_exams_name",
                schema: "tenant_template",
                table: "exams",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mark_distributions_name",
                schema: "tenant_template",
                table: "mark_distributions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_mark_entries_exam",
                schema: "tenant_template",
                table: "mark_entries",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "idx_mark_entries_student",
                schema: "tenant_template",
                table: "mark_entries",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "idx_mark_entries_subject",
                schema: "tenant_template",
                table: "mark_entries",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_mark_entries_class_id",
                schema: "tenant_template",
                table: "mark_entries",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_mark_entries_exam_id_class_id_section_id_subject_id_student~",
                schema: "tenant_template",
                table: "mark_entries",
                columns: new[] { "exam_id", "class_id", "section_id", "subject_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mark_entries_section_id",
                schema: "tenant_template",
                table: "mark_entries",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_mark_distributions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_schedule_subjects",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "mark_entries",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "mark_distributions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_halls",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_schedules",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exams",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_terms",
                schema: "tenant_template");
        }
    }
}
