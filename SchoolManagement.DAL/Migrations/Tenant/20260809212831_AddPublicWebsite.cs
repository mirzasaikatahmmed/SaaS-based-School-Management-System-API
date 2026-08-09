using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPublicWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "website_cms_settings",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    school_name_bn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    youtube_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    facebook_page_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    portal_url = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, defaultValue: "/portal"),
                    copyright_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    online_admission_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    eiin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    established_year = table.Column<int>(type: "integer", nullable: true),
                    school_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    classes_offered = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    total_students_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    history_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    history_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    history_title_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    history_sections_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    founding_committee_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    contact_page_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_box_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    contact_box_description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    contact_map_iframe_html = table.Column<string>(type: "text", nullable: true),
                    contact_submit_button_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "Send"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_cms_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_committee_members",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mobile_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_committee_members", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_contact_messages",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    message = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_contact_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_documents",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    title_bn = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "other"),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_footer_links",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    column_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    column_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    column_title_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    label_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_footer_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_gallery_categories",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_gallery_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_important_links",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_important_links", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_menu_items",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    open_in_new_tab = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_website_menu_items_website_menu_items_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "tenant_template",
                        principalTable: "website_menu_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "website_notices",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body_html = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_notices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_slider_items",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    button_text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    button_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_slider_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_speeches",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    designation_bn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    message_html = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    facebook_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_speeches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_tenure_people",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    designation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    joined_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    left_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_tenure_people", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_visitor_daily",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    visit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    views = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_visitor_daily", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_gallery_items",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    thumb_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    extra_images_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    event_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_gallery_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_website_gallery_items_website_gallery_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "tenant_template",
                        principalTable: "website_gallery_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_website_gallery_items_category_id",
                schema: "tenant_template",
                table: "website_gallery_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_website_menu_items_parent_id",
                schema: "tenant_template",
                table: "website_menu_items",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_website_speeches_role",
                schema: "tenant_template",
                table: "website_speeches",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_website_tenure_people_kind",
                schema: "tenant_template",
                table: "website_tenure_people",
                column: "kind");

            migrationBuilder.CreateIndex(
                name: "IX_website_visitor_daily_visit_date",
                schema: "tenant_template",
                table: "website_visitor_daily",
                column: "visit_date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "website_cms_settings",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_committee_members",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_contact_messages",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_documents",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_footer_links",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_gallery_items",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_important_links",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_menu_items",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_notices",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_slider_items",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_speeches",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_tenure_people",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_visitor_daily",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_gallery_categories",
                schema: "tenant_template");
        }
    }
}
