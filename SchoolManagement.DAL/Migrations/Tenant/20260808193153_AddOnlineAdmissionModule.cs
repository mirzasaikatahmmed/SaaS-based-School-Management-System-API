using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddOnlineAdmissionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "online_admissions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reference_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    class_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    blood_group = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    religion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mobile_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    present_address = table.Column<string>(type: "text", nullable: true),
                    permanent_address = table.Column<string>(type: "text", nullable: true),
                    birth_registration_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    profile_picture_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    guardian_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    guardian_relation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    guardian_mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    guardian_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    father_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mother_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    previous_school_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    previous_school_qualification = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Apply"),
                    payment_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid"),
                    payment_amount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    payment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payment_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decline_reason = table.Column<string>(type: "text", nullable: true),
                    student_id = table.Column<Guid>(type: "uuid", nullable: true),
                    apply_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_online_admissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_online_admissions_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_online_admissions_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_online_admissions_users_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_class_id",
                schema: "tenant_template",
                table: "online_admissions",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_reference_no",
                schema: "tenant_template",
                table: "online_admissions",
                column: "reference_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_reviewed_by",
                schema: "tenant_template",
                table: "online_admissions",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_status",
                schema: "tenant_template",
                table: "online_admissions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_online_admissions_student_id",
                schema: "tenant_template",
                table: "online_admissions",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "online_admissions",
                schema: "tenant_template");
        }
    }
}
