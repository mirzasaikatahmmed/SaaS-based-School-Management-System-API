using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAdmissionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classes",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    numeric_name = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hostels",
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
                    table.PrimaryKey("PK_hostels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "student_categories",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transport_routes",
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
                    table.PrimaryKey("PK_transport_routes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_sections_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hostel_rooms",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    hostel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_rooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_hostel_rooms_hostels_hostel_id",
                        column: x => x.hostel_id,
                        principalSchema: "tenant_template",
                        principalTable: "hostels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    register_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    roll = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    admission_date = table.Column<DateTime>(type: "date", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    blood_group = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    mother_tongue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    religion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    caste = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mobile_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    present_address = table.Column<string>(type: "text", nullable: true),
                    permanent_address = table.Column<string>(type: "text", nullable: true),
                    profile_picture_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fathers_nid_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mothers_nid_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    birth_registration_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    previous_school_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    previous_school_qualification = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    transport_route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    hostel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                    table.ForeignKey(
                        name: "FK_students_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_hostel_rooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "tenant_template",
                        principalTable: "hostel_rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_hostels_hostel_id",
                        column: x => x.hostel_id,
                        principalSchema: "tenant_template",
                        principalTable: "hostels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_student_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "tenant_template",
                        principalTable: "student_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_transport_routes_transport_route_id",
                        column: x => x.transport_route_id,
                        principalSchema: "tenant_template",
                        principalTable: "transport_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardians",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    relation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    father_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mother_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    occupation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    income = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    education = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mobile_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    profile_picture_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardians", x => x.id);
                    table.ForeignKey(
                        name: "FK_guardians_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guardians_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guardians_student_id",
                schema: "tenant_template",
                table: "guardians",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_guardians_user_id",
                schema: "tenant_template",
                table: "guardians",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_hostel_rooms_hostel_id",
                schema: "tenant_template",
                table: "hostel_rooms",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_sections_class_id",
                schema: "tenant_template",
                table: "sections",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_category_id",
                schema: "tenant_template",
                table: "students",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_class_id",
                schema: "tenant_template",
                table: "students",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_hostel_id",
                schema: "tenant_template",
                table: "students",
                column: "hostel_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_register_no",
                schema: "tenant_template",
                table: "students",
                column: "register_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_room_id",
                schema: "tenant_template",
                table: "students",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_section_id",
                schema: "tenant_template",
                table: "students",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_transport_route_id",
                schema: "tenant_template",
                table: "students",
                column: "transport_route_id");

            migrationBuilder.CreateIndex(
                name: "IX_students_user_id",
                schema: "tenant_template",
                table: "students",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guardians",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "students",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "hostel_rooms",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "sections",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "student_categories",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "transport_routes",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "hostels",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "classes",
                schema: "tenant_template");
        }
    }
}
