using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddGradesAttendanceLibraryEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book_categories",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employee_attendance",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_attendance_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_attendance_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "event_types",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    icon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_attendance",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Present"),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_attendance_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_attendance_exams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "tenant_template",
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_attendance_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_attendance_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_attendance_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "tenant_template",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_positions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    total_marks = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m),
                    full_marks = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    gpa = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 0m),
                    result = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "FAIL"),
                    position = table.Column<int>(type: "integer", nullable: true),
                    principal_comments = table.Column<string>(type: "text", nullable: true),
                    teacher_comments = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_positions_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_positions_exams_exam_id",
                        column: x => x.exam_id,
                        principalSchema: "tenant_template",
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_positions_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_positions_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_ranges",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    grade_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    grade_point = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    min_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    max_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    remarks = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_ranges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "student_attendance",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Present"),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_attendance", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_attendance_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_attendance_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_attendance_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_attendance_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "books",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    isbn_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    author = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    edition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    publisher = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    purchase_date = table.Column<DateTime>(type: "date", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    total_stock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    issued_copies = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.id);
                    table.ForeignKey(
                        name: "FK_books_book_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "tenant_template",
                        principalTable: "book_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "events",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_holiday = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    audience = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Everybody"),
                    date_of_start = table.Column<DateTime>(type: "date", nullable: false),
                    date_of_end = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    show_website = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_events_event_types_event_type_id",
                        column: x => x.event_type_id,
                        principalSchema: "tenant_template",
                        principalTable: "event_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_events_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "book_issues",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date_of_issue = table.Column<DateTime>(type: "date", nullable: false),
                    date_of_expiry = table.Column<DateTime>(type: "date", nullable: false),
                    return_date = table.Column<DateTime>(type: "date", nullable: true),
                    fine = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Issued"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_issues", x => x.id);
                    table.ForeignKey(
                        name: "FK_book_issues_books_book_id",
                        column: x => x.book_id,
                        principalSchema: "tenant_template",
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_book_issues_employees_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "tenant_template",
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_book_issues_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_book_categories_name",
                schema: "tenant_template",
                table: "book_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_book_issues_book",
                schema: "tenant_template",
                table: "book_issues",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "idx_book_issues_status",
                schema: "tenant_template",
                table: "book_issues",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_book_issues_employee_id",
                schema: "tenant_template",
                table: "book_issues",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_book_issues_student_id",
                schema: "tenant_template",
                table: "book_issues",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_books_category_id",
                schema: "tenant_template",
                table: "books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_employee_att_date",
                schema: "tenant_template",
                table: "employee_attendance",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_employee_attendance_created_by",
                schema: "tenant_template",
                table: "employee_attendance",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_employee_attendance_employee_id_attendance_date",
                schema: "tenant_template",
                table: "employee_attendance",
                columns: new[] { "employee_id", "attendance_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_event_types_name",
                schema: "tenant_template",
                table: "event_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_events_dates",
                schema: "tenant_template",
                table: "events",
                columns: new[] { "date_of_start", "date_of_end" });

            migrationBuilder.CreateIndex(
                name: "IX_events_created_by",
                schema: "tenant_template",
                table: "events",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_events_event_type_id",
                schema: "tenant_template",
                table: "events",
                column: "event_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_class_id",
                schema: "tenant_template",
                table: "exam_attendance",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_exam_id_subject_id_student_id",
                schema: "tenant_template",
                table: "exam_attendance",
                columns: new[] { "exam_id", "subject_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_section_id",
                schema: "tenant_template",
                table: "exam_attendance",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_student_id",
                schema: "tenant_template",
                table: "exam_attendance",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_attendance_subject_id",
                schema: "tenant_template",
                table: "exam_attendance",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_positions_class_id",
                schema: "tenant_template",
                table: "exam_positions",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_positions_exam_id_class_id_section_id_student_id",
                schema: "tenant_template",
                table: "exam_positions",
                columns: new[] { "exam_id", "class_id", "section_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_positions_section_id",
                schema: "tenant_template",
                table: "exam_positions",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_positions_student_id",
                schema: "tenant_template",
                table: "exam_positions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_ranges_grade_name",
                schema: "tenant_template",
                table: "grade_ranges",
                column: "grade_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_student_att_date",
                schema: "tenant_template",
                table: "student_attendance",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendance_class_id",
                schema: "tenant_template",
                table: "student_attendance",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendance_created_by",
                schema: "tenant_template",
                table: "student_attendance",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendance_section_id",
                schema: "tenant_template",
                table: "student_attendance",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendance_student_id_attendance_date",
                schema: "tenant_template",
                table: "student_attendance",
                columns: new[] { "student_id", "attendance_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_issues",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "employee_attendance",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "events",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_attendance",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "exam_positions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "grade_ranges",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "student_attendance",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "books",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "event_types",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "book_categories",
                schema: "tenant_template");
        }
    }
}
