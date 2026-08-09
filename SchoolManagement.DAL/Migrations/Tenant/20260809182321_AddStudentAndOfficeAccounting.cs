using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddStudentAndOfficeAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounting_accounts",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    opening_balance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, defaultValue: 0m),
                    current_balance = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false, defaultValue: 0m),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fees_groups",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fees_reminders",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    dlt_template_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notify_student = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notify_guardian = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fees_types",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fee_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "offline_payment_types",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offline_payment_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_heads",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher_heads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fees_allocations",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fees_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees_allocations", x => x.id);
                    table.ForeignKey(
                        name: "FK_fees_allocations_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fees_allocations_fees_groups_fees_group_id",
                        column: x => x.fees_group_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fees_allocations_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fees_group_items",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fees_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees_group_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_fees_group_items_fees_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fees_group_items_fees_types_fees_type_id",
                        column: x => x.fees_type_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fine_setups",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fees_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fine_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fine_value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    late_fee_frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fine_setups", x => x.id);
                    table.ForeignKey(
                        name: "FK_fine_setups_fees_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fine_setups_fees_types_fees_type_id",
                        column: x => x.fees_type_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "offline_payments",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    trx_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_type_id = table.Column<Guid>(type: "uuid", nullable: true),
                    class_id = table.Column<Guid>(type: "uuid", nullable: true),
                    section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_date = table.Column<DateTime>(type: "date", nullable: false),
                    submit_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_offline_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_offline_payments_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_offline_payments_offline_payment_types_payment_type_id",
                        column: x => x.payment_type_id,
                        principalSchema: "tenant_template",
                        principalTable: "offline_payment_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_offline_payments_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_offline_payments_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounting_deposits",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_head_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ref_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    deposit_date = table.Column<DateTime>(type: "date", nullable: false),
                    pay_via = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    attachment_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_deposits", x => x.id);
                    table.ForeignKey(
                        name: "FK_accounting_deposits_accounting_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "tenant_template",
                        principalTable: "accounting_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_accounting_deposits_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_accounting_deposits_voucher_heads_voucher_head_id",
                        column: x => x.voucher_head_id,
                        principalSchema: "tenant_template",
                        principalTable: "voucher_heads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "accounting_expenses",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_head_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ref_no = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    expense_date = table.Column<DateTime>(type: "date", nullable: false),
                    pay_via = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    attachment_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounting_expenses", x => x.id);
                    table.ForeignKey(
                        name: "FK_accounting_expenses_accounting_accounts_account_id",
                        column: x => x.account_id,
                        principalSchema: "tenant_template",
                        principalTable: "accounting_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_accounting_expenses_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_accounting_expenses_voucher_heads_voucher_head_id",
                        column: x => x.voucher_head_id,
                        principalSchema: "tenant_template",
                        principalTable: "voucher_heads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_fee_invoices",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fees_allocation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fees_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    paid_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    fine_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    due_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Unpaid"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_fee_invoices", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_fee_invoices_classes_class_id",
                        column: x => x.class_id,
                        principalSchema: "tenant_template",
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_fee_invoices_fees_allocations_fees_allocation_id",
                        column: x => x.fees_allocation_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_allocations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_fee_invoices_fees_groups_fees_group_id",
                        column: x => x.fees_group_id,
                        principalSchema: "tenant_template",
                        principalTable: "fees_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_fee_invoices_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "tenant_template",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_fee_invoices_students_student_id",
                        column: x => x.student_id,
                        principalSchema: "tenant_template",
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_deposits_account",
                schema: "tenant_template",
                table: "accounting_deposits",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_deposits_created_by",
                schema: "tenant_template",
                table: "accounting_deposits",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_deposits_voucher_head_id",
                schema: "tenant_template",
                table: "accounting_deposits",
                column: "voucher_head_id");

            migrationBuilder.CreateIndex(
                name: "idx_expenses_account",
                schema: "tenant_template",
                table: "accounting_expenses",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_expenses_created_by",
                schema: "tenant_template",
                table: "accounting_expenses",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_accounting_expenses_voucher_head_id",
                schema: "tenant_template",
                table: "accounting_expenses",
                column: "voucher_head_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_allocations_class_id_section_id_fees_group_id_academic~",
                schema: "tenant_template",
                table: "fees_allocations",
                columns: new[] { "class_id", "section_id", "fees_group_id", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fees_allocations_fees_group_id",
                schema: "tenant_template",
                table: "fees_allocations",
                column: "fees_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_allocations_section_id",
                schema: "tenant_template",
                table: "fees_allocations",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_group_items_fees_type_id",
                schema: "tenant_template",
                table: "fees_group_items",
                column: "fees_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_group_items_group_id",
                schema: "tenant_template",
                table: "fees_group_items",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_types_fee_code",
                schema: "tenant_template",
                table: "fees_types",
                column: "fee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fine_setups_fees_type_id",
                schema: "tenant_template",
                table: "fine_setups",
                column: "fees_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_fine_setups_group_id_fees_type_id",
                schema: "tenant_template",
                table: "fine_setups",
                columns: new[] { "group_id", "fees_type_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_offline_payments_class_id",
                schema: "tenant_template",
                table: "offline_payments",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_offline_payments_payment_type_id",
                schema: "tenant_template",
                table: "offline_payments",
                column: "payment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_offline_payments_section_id",
                schema: "tenant_template",
                table: "offline_payments",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_offline_payments_student_id",
                schema: "tenant_template",
                table: "offline_payments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_offline_payments_trx_id",
                schema: "tenant_template",
                table: "offline_payments",
                column: "trx_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_invoices_status",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_invoices_student",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_fee_invoices_class_id",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_fee_invoices_fees_allocation_id",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "fees_allocation_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_fee_invoices_fees_group_id",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "fees_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_fee_invoices_section_id",
                schema: "tenant_template",
                table: "student_fee_invoices",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounting_deposits",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "accounting_expenses",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fees_group_items",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fees_reminders",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fine_setups",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "offline_payments",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "student_fee_invoices",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "accounting_accounts",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "voucher_heads",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fees_types",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "offline_payment_types",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fees_allocations",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "fees_groups",
                schema: "tenant_template");
        }
    }
}
