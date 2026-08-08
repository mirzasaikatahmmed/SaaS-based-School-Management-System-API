using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddStudentImportModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_online_admissions_users_reviewed_by",
                schema: "tenant_template",
                table: "online_admissions");

            migrationBuilder.DropIndex(
                name: "IX_online_admissions_reviewed_by",
                schema: "tenant_template",
                table: "online_admissions");

            migrationBuilder.CreateTable(
                name: "import_batches",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_import_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_batches_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_import_batches_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "import_batch_rows",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    raw_data = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_batch_rows", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_batch_rows_import_batches_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "tenant_template",
                        principalTable: "import_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_import_batch_rows_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_batch_rows_batch_id",
                schema: "tenant_template",
                table: "import_batch_rows",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batch_rows_status",
                schema: "tenant_template",
                table: "import_batch_rows",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_import_batch_rows_student_id",
                schema: "tenant_template",
                table: "import_batch_rows",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_class_id",
                schema: "tenant_template",
                table: "import_batches",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_section_id",
                schema: "tenant_template",
                table: "import_batches",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_batch_rows",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "import_batches",
                schema: "tenant_template");

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_reviewed_by",
                schema: "tenant_template",
                table: "online_admissions",
                column: "reviewed_by");

            migrationBuilder.AddForeignKey(
                name: "FK_online_admissions_users_reviewed_by",
                schema: "tenant_template",
                table: "online_admissions",
                column: "reviewed_by",
                principalSchema: "tenant_template",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
