using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAcademicModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sections_classes_class_id",
                schema: "tenant_template",
                table: "sections");

            migrationBuilder.AlterColumn<Guid>(
                name: "class_id",
                schema: "tenant_template",
                table: "sections",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "capacity",
                schema: "tenant_template",
                table: "sections",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "class_schedules",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_schedules", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_schedules_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_schedules_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class_sections",
                schema: "tenant_template",
                columns: table => new
                {
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_sections", x => new { x.class_id, x.section_id });
                    table.ForeignKey(
                        name: "FK_class_sections_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_sections_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class_subject_assignments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_subject_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_subject_assignments_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_subject_assignments_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "class_teacher_allocations",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_teacher_allocations", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_teacher_allocations_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_teacher_allocations_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_teacher_allocations_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_promotions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_academic_year = table.Column<int>(type: "integer", nullable: false),
                    from_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_roll = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    to_academic_year = table.Column<int>(type: "integer", nullable: false),
                    to_class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_roll = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Promoted"),
                    current_due_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    carry_forward_due = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    promoted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    promoted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_promotions", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_promotions_classes_from_class_id",
                        column: x => x.from_class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_promotions_classes_to_class_id",
                        column: x => x.to_class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_promotions_sections_from_section_id",
                        column: x => x.from_section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_promotions_sections_to_section_id",
                        column: x => x.to_section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_promotions_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_promotions_users_promoted_by",
                        column: x => x.promoted_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    subject_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Theory"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "class_schedule_periods",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_break = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starting_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    ending_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    class_room = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_schedule_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_schedule_periods_class_schedules_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "tenant_template",
                        principalTable: "class_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_schedule_periods_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_class_schedule_periods_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "class_subject_assignment_items",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_subject_assignment_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_subject_assignment_items_class_subject_assignments_as~",
                        column: x => x.assignment_id,
                        principalSchema: "tenant_template",
                        principalTable: "class_subject_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_subject_assignment_items_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_schedule_periods_employee_id",
                schema: "tenant_template",
                table: "class_schedule_periods",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_schedule_periods_schedule_id",
                schema: "tenant_template",
                table: "class_schedule_periods",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_schedule_periods_subject_id",
                schema: "tenant_template",
                table: "class_schedule_periods",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_schedules_class_id_section_id",
                schema: "tenant_template",
                table: "class_schedules",
                columns: new[] { "class_id", "section_id" });

            migrationBuilder.CreateIndex(
                name: "IX_class_schedules_class_id_section_id_day",
                schema: "tenant_template",
                table: "class_schedules",
                columns: new[] { "class_id", "section_id", "day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_schedules_section_id",
                schema: "tenant_template",
                table: "class_schedules",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_sections_section_id",
                schema: "tenant_template",
                table: "class_sections",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_assignment_items_assignment_id_subject_id",
                schema: "tenant_template",
                table: "class_subject_assignment_items",
                columns: new[] { "assignment_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_assignment_items_subject_id",
                schema: "tenant_template",
                table: "class_subject_assignment_items",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_assignments_class_id_section_id",
                schema: "tenant_template",
                table: "class_subject_assignments",
                columns: new[] { "class_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_subject_assignments_section_id",
                schema: "tenant_template",
                table: "class_subject_assignments",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_teacher_allocations_class_id_section_id",
                schema: "tenant_template",
                table: "class_teacher_allocations",
                columns: new[] { "class_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_teacher_allocations_employee_id",
                schema: "tenant_template",
                table: "class_teacher_allocations",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_teacher_allocations_section_id",
                schema: "tenant_template",
                table: "class_teacher_allocations",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_from_class_id",
                schema: "tenant_template",
                table: "student_promotions",
                column: "from_class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_from_section_id",
                schema: "tenant_template",
                table: "student_promotions",
                column: "from_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_promoted_by",
                schema: "tenant_template",
                table: "student_promotions",
                column: "promoted_by");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_student_id",
                schema: "tenant_template",
                table: "student_promotions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_to_class_id",
                schema: "tenant_template",
                table: "student_promotions",
                column: "to_class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_promotions_to_section_id",
                schema: "tenant_template",
                table: "student_promotions",
                column: "to_section_id");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_code",
                schema: "tenant_template",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_classes_class_id",
                schema: "tenant_template",
                table: "sections",
                column: "class_id",
                principalSchema: "tenant_template",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sections_classes_class_id",
                schema: "tenant_template",
                table: "sections");

            migrationBuilder.DropTable(
                name: "class_schedule_periods",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "class_sections",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "class_subject_assignment_items",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "class_teacher_allocations",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "student_promotions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "class_schedules",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "class_subject_assignments",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "tenant_template");

            migrationBuilder.DropColumn(
                name: "capacity",
                schema: "tenant_template",
                table: "sections");

            migrationBuilder.AlterColumn<Guid>(
                name: "class_id",
                schema: "tenant_template",
                table: "sections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_classes_class_id",
                schema: "tenant_template",
                table: "sections",
                column: "class_id",
                principalSchema: "tenant_template",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
