using Microsoft.EntityFrameworkCore;
using SchoolManagement.Common.Constants;

namespace SchoolManagement.DAL.Context;

public interface ITenantSchemaProvisioner
{
    Task ProvisionAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureAdmissionModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureOnlineAdmissionModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureStudentImportModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureStudentDeactivationFieldsAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureDeactivateReasonMasterAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureGuardianParentFieldsAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureGuardianSocialAndAlternativeFieldsAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureEmployeeModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsurePayrollModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureAdvanceSalaryAndLeaveModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureAwardModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureAcademicModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureExamMasterModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task DropSchemaAsync(string schemaName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Creates per-tenant PostgreSQL schemas using ahskbera-compatible column naming
/// (password, mobileno, photo, active, last_login, roles.prefix, login_log).
/// </summary>
public class TenantSchemaProvisioner : ITenantSchemaProvisioner
{
    private readonly MasterDbContext _masterDbContext;

    public TenantSchemaProvisioner(MasterDbContext masterDbContext)
    {
        _masterDbContext = masterDbContext;
    }

    public async Task ProvisionAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE SCHEMA IF NOT EXISTS "{schemaName}";

            CREATE TABLE IF NOT EXISTS "{schemaName}".roles (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(50) NOT NULL,
                prefix VARCHAR(50) NOT NULL,
                is_system BOOLEAN NOT NULL DEFAULT TRUE,
                description VARCHAR(255),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_roles_name" ON "{schemaName}".roles (name);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_roles_prefix" ON "{schemaName}".roles (prefix);

            CREATE TABLE IF NOT EXISTS "{schemaName}".users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                email VARCHAR(255) NOT NULL,
                username VARCHAR(100) NOT NULL,
                password VARCHAR(250) NOT NULL,
                first_name VARCHAR(255) NOT NULL,
                last_name VARCHAR(255) NOT NULL,
                mobileno VARCHAR(100),
                photo VARCHAR(255),
                active BOOLEAN NOT NULL DEFAULT TRUE,
                is_email_verified BOOLEAN NOT NULL DEFAULT FALSE,
                last_login TIMESTAMPTZ,
                failed_login_attempts INT NOT NULL DEFAULT 0,
                lockout_end_at TIMESTAMPTZ,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_users_email" ON "{schemaName}".users (email);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_users_username" ON "{schemaName}".users (username);

            CREATE TABLE IF NOT EXISTS "{schemaName}".user_roles (
                user_id UUID NOT NULL,
                role_id UUID NOT NULL,
                CONSTRAINT "PK_{schemaName}_user_roles" PRIMARY KEY (user_id, role_id),
                CONSTRAINT "FK_{schemaName}_user_roles_users" FOREIGN KEY (user_id)
                    REFERENCES "{schemaName}".users (id) ON DELETE CASCADE,
                CONSTRAINT "FK_{schemaName}_user_roles_roles" FOREIGN KEY (role_id)
                    REFERENCES "{schemaName}".roles (id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".refresh_tokens (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL,
                token VARCHAR(500) NOT NULL,
                expires_at TIMESTAMPTZ NOT NULL,
                is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
                created_by_ip VARCHAR(50),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                revoked_at TIMESTAMPTZ,
                replaced_by_token VARCHAR(500),
                CONSTRAINT "FK_{schemaName}_refresh_tokens_users" FOREIGN KEY (user_id)
                    REFERENCES "{schemaName}".users (id) ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_refresh_tokens_token"
                ON "{schemaName}".refresh_tokens (token);

            CREATE TABLE IF NOT EXISTS "{schemaName}".login_log (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL,
                role VARCHAR(50) NOT NULL,
                ip VARCHAR(255) NOT NULL,
                browser VARCHAR(255),
                platform VARCHAR(255),
                timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                CONSTRAINT "FK_{schemaName}_login_log_users" FOREIGN KEY (user_id)
                    REFERENCES "{schemaName}".users (id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}"."__EFMigrationsHistory" (
                "MigrationId" VARCHAR(150) NOT NULL,
                "ProductVersion" VARCHAR(32) NOT NULL,
                CONSTRAINT "PK_{schemaName}___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809000000_InitialTenant_AhskberaFormat', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureAdmissionModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureAdmissionModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".classes (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                numeric_name INT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".sections (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id) ON DELETE CASCADE,
                name VARCHAR(50) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".student_categories (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".transport_routes (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN DEFAULT true,
                created_at TIMESTAMPTZ DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".hostels (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN DEFAULT true,
                created_at TIMESTAMPTZ DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".hostel_rooms (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                hostel_id UUID NOT NULL REFERENCES "{schemaName}".hostels(id),
                name VARCHAR(100) NOT NULL,
                is_active BOOLEAN DEFAULT true,
                created_at TIMESTAMPTZ DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".students (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL REFERENCES "{schemaName}".users(id),
                register_no VARCHAR(100) NOT NULL UNIQUE,
                roll VARCHAR(50),
                academic_year INT NOT NULL,
                admission_date DATE NOT NULL DEFAULT CURRENT_DATE,
                class_id UUID REFERENCES "{schemaName}".classes(id),
                section_id UUID REFERENCES "{schemaName}".sections(id),
                category_id UUID REFERENCES "{schemaName}".student_categories(id),
                first_name VARCHAR(100) NOT NULL,
                last_name VARCHAR(100),
                gender VARCHAR(20),
                blood_group VARCHAR(10),
                date_of_birth DATE,
                mother_tongue VARCHAR(100),
                religion VARCHAR(100),
                caste VARCHAR(100),
                mobile_no VARCHAR(20),
                email VARCHAR(255),
                city VARCHAR(100),
                state VARCHAR(100),
                present_address TEXT,
                permanent_address TEXT,
                profile_picture_url VARCHAR(500),
                fathers_nid_number VARCHAR(100),
                mothers_nid_number VARCHAR(100),
                birth_registration_number VARCHAR(100),
                previous_school_name VARCHAR(255),
                previous_school_qualification VARCHAR(255),
                remarks TEXT,
                transport_route_id UUID REFERENCES "{schemaName}".transport_routes(id),
                vehicle_no VARCHAR(50),
                hostel_id UUID REFERENCES "{schemaName}".hostels(id),
                room_id UUID REFERENCES "{schemaName}".hostel_rooms(id),
                is_active BOOLEAN NOT NULL DEFAULT true,
                deactivate_reason TEXT,
                deactivated_at TIMESTAMPTZ,
                deactivated_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".guardians (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID REFERENCES "{schemaName}".students(id) ON DELETE CASCADE,
                user_id UUID REFERENCES "{schemaName}".users(id),
                reference_no VARCHAR(50),
                name VARCHAR(200) NOT NULL,
                relation VARCHAR(100) NOT NULL,
                father_name VARCHAR(200),
                mother_name VARCHAR(200),
                occupation VARCHAR(200),
                income DECIMAL(12,2),
                education VARCHAR(200),
                city VARCHAR(100),
                state VARCHAR(100),
                mobile_no VARCHAR(20) NOT NULL,
                email VARCHAR(255),
                address TEXT,
                profile_picture_url VARCHAR(500),
                alternative_parent_name VARCHAR(200),
                alternative_parent_relation VARCHAR(100),
                alternative_parent_mobile VARCHAR(20),
                facebook_url VARCHAR(500),
                twitter_url VARCHAR(500),
                linkedin_url VARCHAR(500),
                is_primary BOOLEAN NOT NULL DEFAULT true,
                is_active BOOLEAN NOT NULL DEFAULT true,
                is_login_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_guardians_reference_no"
                ON "{schemaName}".guardians (reference_no) WHERE reference_no IS NOT NULL;

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809010000_AddAdmissionModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await SeedAdmissionLookupsAsync(schemaName, cancellationToken);
    }

    private async Task SeedAdmissionLookupsAsync(string schemaName, CancellationToken cancellationToken)
    {
        var seedSql = $"""
            INSERT INTO "{schemaName}".classes (id, name, numeric_name, is_active)
            SELECT gen_random_uuid(), v.name, v.n, true
            FROM (VALUES
                ('Class 1', 1), ('Class 2', 2), ('Class 3', 3), ('Class 4', 4), ('Class 5', 5),
                ('Class 6', 6), ('Class 7', 7), ('Class 8', 8), ('Class 9', 9), ('Class 10', 10)
            ) AS v(name, n)
            WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".classes);

            INSERT INTO "{schemaName}".sections (id, class_id, name, is_active)
            SELECT gen_random_uuid(), c.id, s.name, true
            FROM "{schemaName}".classes c
            CROSS JOIN (VALUES ('A'), ('B'), ('C')) AS s(name)
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".sections sec WHERE sec.class_id = c.id AND sec.name = s.name
            );

            INSERT INTO "{schemaName}".student_categories (id, name, is_active)
            SELECT gen_random_uuid(), v.name, true
            FROM (VALUES
                ('COMMON'),
                ('GENERAL'),
                ('FREEDOM FIGHTER'),
                ('TRIBAL'),
                ('SPECIAL NEEDS')
            ) AS v(name)
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".student_categories sc
                WHERE UPPER(sc.name) = UPPER(v.name)
            );
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(seedSql, cancellationToken);
        await EnsureOnlineAdmissionModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureOnlineAdmissionModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".online_admissions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                reference_no VARCHAR(50) NOT NULL UNIQUE,
                academic_year INT NOT NULL,
                class_id UUID REFERENCES "{schemaName}".classes(id),
                class_name VARCHAR(100),
                first_name VARCHAR(100) NOT NULL,
                last_name VARCHAR(100),
                gender VARCHAR(20),
                date_of_birth DATE,
                blood_group VARCHAR(10),
                religion VARCHAR(100),
                mobile_no VARCHAR(20) NOT NULL,
                email VARCHAR(255),
                present_address TEXT,
                permanent_address TEXT,
                birth_registration_number VARCHAR(100),
                profile_picture_url VARCHAR(500),
                guardian_name VARCHAR(200),
                guardian_relation VARCHAR(100),
                guardian_mobile VARCHAR(20),
                guardian_email VARCHAR(255),
                father_name VARCHAR(200),
                mother_name VARCHAR(200),
                previous_school_name VARCHAR(255),
                previous_school_qualification VARCHAR(255),
                status VARCHAR(20) NOT NULL DEFAULT 'Apply',
                payment_status VARCHAR(20) NOT NULL DEFAULT 'Unpaid',
                payment_amount DECIMAL(12,2),
                payment_date TIMESTAMPTZ,
                payment_reference VARCHAR(200),
                reviewed_by UUID,
                reviewed_at TIMESTAMPTZ,
                decline_reason TEXT,
                student_id UUID REFERENCES "{schemaName}".students(id),
                apply_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            -- Super Admin reviewers are not tenant users — drop FK if an older provision created it
            ALTER TABLE "{schemaName}".online_admissions
                DROP CONSTRAINT IF EXISTS online_admissions_reviewed_by_fkey;

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_online_admissions_status"
                ON "{schemaName}".online_admissions (status);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_online_admissions_class"
                ON "{schemaName}".online_admissions (class_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_online_admissions_reference"
                ON "{schemaName}".online_admissions (reference_no);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809020000_AddOnlineAdmissionModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureStudentImportModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureStudentImportModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".import_batches (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                file_name VARCHAR(255) NOT NULL,
                file_url VARCHAR(500),
                total_rows INT NOT NULL DEFAULT 0,
                success_count INT NOT NULL DEFAULT 0,
                failed_count INT NOT NULL DEFAULT 0,
                status VARCHAR(20) NOT NULL DEFAULT 'Processing',
                imported_by UUID NOT NULL,
                started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                completed_at TIMESTAMPTZ,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".import_batch_rows (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                batch_id UUID NOT NULL REFERENCES "{schemaName}".import_batches(id) ON DELETE CASCADE,
                row_number INT NOT NULL,
                raw_data JSONB NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'Pending',
                student_id UUID REFERENCES "{schemaName}".students(id),
                error_message TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_import_batch_rows_batch"
                ON "{schemaName}".import_batch_rows (batch_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_import_batch_rows_status"
                ON "{schemaName}".import_batch_rows (status);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809030000_AddStudentImportModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureStudentDeactivationFieldsAsync(schemaName, cancellationToken);
    }

    public async Task EnsureStudentDeactivationFieldsAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            ALTER TABLE "{schemaName}".students ADD COLUMN IF NOT EXISTS deactivate_reason TEXT;
            ALTER TABLE "{schemaName}".students ADD COLUMN IF NOT EXISTS deactivated_at TIMESTAMPTZ;
            ALTER TABLE "{schemaName}".students ADD COLUMN IF NOT EXISTS deactivated_by UUID;

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260808195033_AddStudentDeactivationFields', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureDeactivateReasonMasterAsync(schemaName, cancellationToken);
    }

    public async Task EnsureDeactivateReasonMasterAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".deactivate_reasons (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                reason VARCHAR(200) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_deactivate_reasons_reason"
                ON "{schemaName}".deactivate_reasons (LOWER(reason));

            ALTER TABLE "{schemaName}".students
                ADD COLUMN IF NOT EXISTS deactivate_reason_id UUID REFERENCES "{schemaName}".deactivate_reasons(id);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260808195514_AddDeactivateReasonMaster', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureGuardianParentFieldsAsync(schemaName, cancellationToken);
    }

    public async Task EnsureGuardianParentFieldsAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            ALTER TABLE "{schemaName}".guardians
                ALTER COLUMN student_id DROP NOT NULL;

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS reference_no VARCHAR(50);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS is_login_active BOOLEAN NOT NULL DEFAULT true;

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT true;

            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_guardians_reference_no"
                ON "{schemaName}".guardians (reference_no) WHERE reference_no IS NOT NULL;

            WITH numbered AS (
                SELECT id,
                       EXTRACT(YEAR FROM created_at)::int AS yr,
                       ROW_NUMBER() OVER (
                           PARTITION BY EXTRACT(YEAR FROM created_at)
                           ORDER BY created_at, id
                       ) AS rn
                FROM "{schemaName}".guardians
                WHERE reference_no IS NULL
            )
            UPDATE "{schemaName}".guardians g
            SET reference_no = n.yr::text || LPAD(n.rn::text, 3, '0')
            FROM numbered n
            WHERE g.id = n.id;

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260808200014_AddGuardianReferenceNo', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureGuardianSocialAndAlternativeFieldsAsync(schemaName, cancellationToken);
    }

    public async Task EnsureGuardianSocialAndAlternativeFieldsAsync(
        string schemaName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS alternative_parent_name VARCHAR(200);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS alternative_parent_relation VARCHAR(100);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS alternative_parent_mobile VARCHAR(20);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS facebook_url VARCHAR(500);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS twitter_url VARCHAR(500);

            ALTER TABLE "{schemaName}".guardians
                ADD COLUMN IF NOT EXISTS linkedin_url VARCHAR(500);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260808200303_AddGuardianSocialAndAlternativeFields', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureEmployeeModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureEmployeeModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".departments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".designations (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".employees (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL REFERENCES "{schemaName}".users(id),
                staff_id VARCHAR(50) NOT NULL UNIQUE,
                role VARCHAR(50) NOT NULL,
                designation_id UUID REFERENCES "{schemaName}".designations(id),
                department_id UUID REFERENCES "{schemaName}".departments(id),
                joining_date DATE NOT NULL,
                qualification TEXT,
                experience_details TEXT,
                total_experience VARCHAR(100),
                name VARCHAR(200) NOT NULL,
                gender VARCHAR(20),
                religion VARCHAR(100),
                blood_group VARCHAR(10),
                date_of_birth DATE,
                mobile_no VARCHAR(20) NOT NULL,
                email VARCHAR(255) NOT NULL,
                present_address TEXT,
                permanent_address TEXT,
                nid_number VARCHAR(100),
                profile_picture_url VARCHAR(500),
                facebook_url VARCHAR(500),
                twitter_url VARCHAR(500),
                linkedin_url VARCHAR(500),
                skip_bank_details BOOLEAN NOT NULL DEFAULT false,
                bank_name VARCHAR(200),
                holder_name VARCHAR(200),
                bank_branch VARCHAR(200),
                bank_address TEXT,
                ifsc_code VARCHAR(50),
                account_no VARCHAR(100),
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_employees_role"
                ON "{schemaName}".employees (role);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_employees_department"
                ON "{schemaName}".employees (department_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_employees_designation"
                ON "{schemaName}".employees (designation_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".employee_import_batches (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                file_name VARCHAR(255) NOT NULL,
                file_url VARCHAR(500),
                total_rows INT NOT NULL DEFAULT 0,
                success_count INT NOT NULL DEFAULT 0,
                failed_count INT NOT NULL DEFAULT 0,
                status VARCHAR(20) NOT NULL DEFAULT 'Processing',
                imported_by UUID NOT NULL,
                started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                completed_at TIMESTAMPTZ,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".employee_import_batch_rows (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                batch_id UUID NOT NULL REFERENCES "{schemaName}".employee_import_batches(id) ON DELETE CASCADE,
                row_number INT NOT NULL,
                raw_data JSONB NOT NULL DEFAULT jsonb_build_object(),
                status VARCHAR(20) NOT NULL DEFAULT 'Pending',
                employee_id UUID REFERENCES "{schemaName}".employees(id),
                error_message TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_emp_import_rows_batch"
                ON "{schemaName}".employee_import_batch_rows (batch_id);

            INSERT INTO "{schemaName}".departments (id, name, is_active)
            SELECT gen_random_uuid(), v.name, true
            FROM (VALUES
                ('MATHEMATICS'), ('ENGLISH'), ('BANGLA'), ('SCIENCE'),
                ('SOCIAL SCIENCE'), ('ICT'), ('PHYSICS')
            ) AS v(name)
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".departments d WHERE UPPER(d.name) = UPPER(v.name)
            );

            INSERT INTO "{schemaName}".designations (id, name, is_active)
            SELECT gen_random_uuid(), v.name, true
            FROM (VALUES
                ('HEAD MASTER'), ('ASSISTANT HEAD MASTER'),
                ('ASSISTANT TEACHER'), ('ADMIN')
            ) AS v(name)
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".designations d WHERE UPPER(d.name) = UPPER(v.name)
            );

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809072020_AddEmployeeModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsurePayrollModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsurePayrollModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".salary_templates (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                salary_grade VARCHAR(100) NOT NULL,
                basic_salary DECIMAL(12,2) NOT NULL,
                overtime_rate_per_hour DECIMAL(10,2),
                total_allowance DECIMAL(12,2) NOT NULL DEFAULT 0,
                total_deduction DECIMAL(12,2) NOT NULL DEFAULT 0,
                net_salary DECIMAL(12,2) NOT NULL DEFAULT 0,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".salary_allowances (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                template_id UUID NOT NULL REFERENCES "{schemaName}".salary_templates(id) ON DELETE CASCADE,
                name VARCHAR(200) NOT NULL,
                amount DECIMAL(12,2) NOT NULL DEFAULT 0,
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".salary_deductions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                template_id UUID NOT NULL REFERENCES "{schemaName}".salary_templates(id) ON DELETE CASCADE,
                name VARCHAR(200) NOT NULL,
                amount DECIMAL(12,2) NOT NULL DEFAULT 0,
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".employee_salary_assignments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id) ON DELETE CASCADE,
                template_id UUID NOT NULL REFERENCES "{schemaName}".salary_templates(id),
                assigned_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                assigned_by UUID REFERENCES "{schemaName}".users(id),
                is_active BOOLEAN NOT NULL DEFAULT true,
                UNIQUE(employee_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".salary_payments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id),
                template_id UUID NOT NULL REFERENCES "{schemaName}".salary_templates(id),
                payment_month VARCHAR(7) NOT NULL,
                basic_salary DECIMAL(12,2) NOT NULL,
                total_allowance DECIMAL(12,2) NOT NULL DEFAULT 0,
                total_deduction DECIMAL(12,2) NOT NULL DEFAULT 0,
                net_salary DECIMAL(12,2) NOT NULL,
                overtime_hours DECIMAL(6,2) DEFAULT 0,
                overtime_amount DECIMAL(12,2) DEFAULT 0,
                advance_deduction DECIMAL(12,2) DEFAULT 0,
                final_amount DECIMAL(12,2) NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'Unpaid',
                payment_date TIMESTAMPTZ,
                payment_method VARCHAR(50),
                payment_note TEXT,
                paid_by UUID REFERENCES "{schemaName}".users(id),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(employee_id, payment_month)
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_salary_payments_month"
                ON "{schemaName}".salary_payments (payment_month);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_salary_payments_status"
                ON "{schemaName}".salary_payments (status);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_salary_payments_employee"
                ON "{schemaName}".salary_payments (employee_id);

            INSERT INTO "{schemaName}".salary_templates
                (id, salary_grade, basic_salary, overtime_rate_per_hour, total_allowance, total_deduction, net_salary, is_active)
            SELECT gen_random_uuid(), 'Basic', 0, 0, 0, 0, 0, true
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".salary_templates t WHERE UPPER(t.salary_grade) = 'BASIC'
            );

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809072936_AddPayrollModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureAdvanceSalaryAndLeaveModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureAdvanceSalaryAndLeaveModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var roles = new[] { "Admin", "Teacher", "Accountant", "Librarian", "Receptionist", "Staff", "Demo" };
        var categories = new (string Name, int Days)[]
        {
            ("Casual Leave", 10),
            ("Sick Leave", 14),
            ("Annual Leave", 20)
        };
        var seedRows = string.Join(",\n", roles.SelectMany(role =>
            categories.Select(c => $"('{c.Name.Replace("'", "''")}', '{role}', {c.Days})")));

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".advance_salary_requests (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id),
                deduct_month VARCHAR(7) NOT NULL,
                amount DECIMAL(12,2) NOT NULL,
                reason TEXT,
                status VARCHAR(20) NOT NULL DEFAULT 'Pending',
                reviewed_by UUID REFERENCES "{schemaName}".users(id),
                reviewed_at TIMESTAMPTZ,
                reject_reason TEXT,
                applied_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_advance_salary_employee"
                ON "{schemaName}".advance_salary_requests (employee_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_advance_salary_month"
                ON "{schemaName}".advance_salary_requests (deduct_month);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_advance_salary_status"
                ON "{schemaName}".advance_salary_requests (status);

            CREATE TABLE IF NOT EXISTS "{schemaName}".leave_categories (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                role VARCHAR(50) NOT NULL,
                days INT NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".leave_requests (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id),
                leave_category_id UUID NOT NULL REFERENCES "{schemaName}".leave_categories(id),
                date_of_start DATE NOT NULL,
                date_of_end DATE NOT NULL,
                days INT NOT NULL,
                reason TEXT,
                attachment_url VARCHAR(500),
                comments TEXT,
                status VARCHAR(20) NOT NULL DEFAULT 'Pending',
                reviewed_by UUID REFERENCES "{schemaName}".users(id),
                reviewed_at TIMESTAMPTZ,
                apply_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_leave_requests_employee"
                ON "{schemaName}".leave_requests (employee_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_leave_requests_category"
                ON "{schemaName}".leave_requests (leave_category_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_leave_requests_status"
                ON "{schemaName}".leave_requests (status);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_leave_requests_dates"
                ON "{schemaName}".leave_requests (date_of_start, date_of_end);

            INSERT INTO "{schemaName}".leave_categories (id, name, role, days, is_active)
            SELECT gen_random_uuid(), v.name, v.role, v.days, true
            FROM (VALUES
                {seedRows}
            ) AS v(name, role, days)
            WHERE NOT EXISTS (
                SELECT 1 FROM "{schemaName}".leave_categories c
                WHERE UPPER(c.name) = UPPER(v.name) AND UPPER(c.role) = UPPER(v.role)
            );

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809073439_AddAdvanceSalaryAndLeaveModules', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureAwardModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureAwardModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".awards (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID REFERENCES "{schemaName}".employees(id) ON DELETE CASCADE,
                student_id UUID REFERENCES "{schemaName}".students(id) ON DELETE CASCADE,
                role VARCHAR(50) NOT NULL,
                award_name VARCHAR(200) NOT NULL,
                gift_item VARCHAR(200) NOT NULL,
                cash_price DECIMAL(12,2),
                award_reason VARCHAR(500) NOT NULL,
                given_date DATE NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                CONSTRAINT chk_award_recipient CHECK (
                    (employee_id IS NOT NULL AND student_id IS NULL) OR
                    (employee_id IS NULL AND student_id IS NOT NULL)
                )
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_awards_employee"
                ON "{schemaName}".awards (employee_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_awards_student"
                ON "{schemaName}".awards (student_id);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_awards_given_date"
                ON "{schemaName}".awards (given_date);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809073827_AddAwardModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        await EnsureAcademicModuleAsync(schemaName, cancellationToken);
    }

    public async Task EnsureAcademicModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        var sql = $"""
            ALTER TABLE "{schemaName}".sections ADD COLUMN IF NOT EXISTS capacity INT;
            ALTER TABLE "{schemaName}".sections ALTER COLUMN class_id DROP NOT NULL;
            ALTER TABLE "{schemaName}".sections DROP CONSTRAINT IF EXISTS sections_class_id_fkey;
            ALTER TABLE "{schemaName}".sections
                ADD CONSTRAINT sections_class_id_fkey FOREIGN KEY (class_id)
                    REFERENCES "{schemaName}".classes(id) ON DELETE SET NULL;

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_sections (
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id) ON DELETE CASCADE,
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id) ON DELETE CASCADE,
                CONSTRAINT "PK_{schemaName}_class_sections" PRIMARY KEY (class_id, section_id)
            );

            INSERT INTO "{schemaName}".class_sections (class_id, section_id)
            SELECT class_id, id FROM "{schemaName}".sections WHERE class_id IS NOT NULL
            ON CONFLICT DO NOTHING;

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_teacher_allocations (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id) ON DELETE CASCADE,
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id) ON DELETE CASCADE,
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id),
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(class_id, section_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".subjects (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                code VARCHAR(50) NOT NULL,
                author VARCHAR(200),
                subject_type VARCHAR(50) NOT NULL DEFAULT 'Theory',
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_subjects_code" ON "{schemaName}".subjects (code);

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_subject_assignments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id) ON DELETE CASCADE,
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id) ON DELETE CASCADE,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(class_id, section_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_subject_assignment_items (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                assignment_id UUID NOT NULL REFERENCES "{schemaName}".class_subject_assignments(id) ON DELETE CASCADE,
                subject_id UUID NOT NULL REFERENCES "{schemaName}".subjects(id),
                UNIQUE(assignment_id, subject_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_schedules (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id) ON DELETE CASCADE,
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id) ON DELETE CASCADE,
                day VARCHAR(20) NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(class_id, section_id, day)
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_class_schedules_class_section"
                ON "{schemaName}".class_schedules (class_id, section_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".class_schedule_periods (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                schedule_id UUID NOT NULL REFERENCES "{schemaName}".class_schedules(id) ON DELETE CASCADE,
                is_break BOOLEAN NOT NULL DEFAULT false,
                subject_id UUID REFERENCES "{schemaName}".subjects(id) ON DELETE SET NULL,
                employee_id UUID REFERENCES "{schemaName}".employees(id) ON DELETE SET NULL,
                starting_time TIME NOT NULL,
                ending_time TIME NOT NULL,
                class_room VARCHAR(100),
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_class_schedule_periods_employee"
                ON "{schemaName}".class_schedule_periods (employee_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".student_promotions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id) ON DELETE CASCADE,
                from_academic_year INT NOT NULL,
                from_class_id UUID REFERENCES "{schemaName}".classes(id) ON DELETE SET NULL,
                from_section_id UUID REFERENCES "{schemaName}".sections(id) ON DELETE SET NULL,
                from_roll VARCHAR(50),
                to_academic_year INT NOT NULL,
                to_class_id UUID REFERENCES "{schemaName}".classes(id) ON DELETE SET NULL,
                to_section_id UUID REFERENCES "{schemaName}".sections(id) ON DELETE SET NULL,
                to_roll VARCHAR(50),
                status VARCHAR(20) NOT NULL DEFAULT 'Promoted',
                current_due_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
                carry_forward_due BOOLEAN NOT NULL DEFAULT true,
                promoted_by UUID REFERENCES "{schemaName}".users(id) ON DELETE SET NULL,
                promoted_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_student_promotions_student"
                ON "{schemaName}".student_promotions (student_id);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809075909_AddAcademicModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task EnsureExamMasterModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureAcademicModuleAsync(schemaName, cancellationToken);

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_terms (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_exam_terms_name"
                ON "{schemaName}".exam_terms (name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_halls (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                hall_no VARCHAR(50) NOT NULL,
                no_of_seats INT NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_exam_halls_hall_no"
                ON "{schemaName}".exam_halls (hall_no);

            CREATE TABLE IF NOT EXISTS "{schemaName}".mark_distributions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(100) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_mark_distributions_name"
                ON "{schemaName}".mark_distributions (name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exams (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                exam_term_id UUID REFERENCES "{schemaName}".exam_terms(id) ON DELETE SET NULL,
                exam_type VARCHAR(100),
                remarks TEXT,
                is_published BOOLEAN NOT NULL DEFAULT false,
                is_result_published BOOLEAN NOT NULL DEFAULT false,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_exams_name"
                ON "{schemaName}".exams (name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_mark_distributions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                exam_id UUID NOT NULL REFERENCES "{schemaName}".exams(id) ON DELETE CASCADE,
                mark_distribution_id UUID NOT NULL REFERENCES "{schemaName}".mark_distributions(id)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_exam_mark_distributions_exam_dist"
                ON "{schemaName}".exam_mark_distributions (exam_id, mark_distribution_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_schedules (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                exam_id UUID NOT NULL REFERENCES "{schemaName}".exams(id) ON DELETE CASCADE,
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(exam_id, class_id, section_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_schedule_subjects (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                schedule_id UUID NOT NULL REFERENCES "{schemaName}".exam_schedules(id) ON DELETE CASCADE,
                subject_id UUID NOT NULL REFERENCES "{schemaName}".subjects(id),
                exam_date DATE NOT NULL,
                starting_time TIME NOT NULL,
                ending_time TIME NOT NULL,
                hall_id UUID REFERENCES "{schemaName}".exam_halls(id) ON DELETE SET NULL,
                written_full_mark INT,
                written_pass_mark INT,
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".mark_entries (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                exam_id UUID NOT NULL REFERENCES "{schemaName}".exams(id),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                subject_id UUID NOT NULL REFERENCES "{schemaName}".subjects(id),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                is_absent BOOLEAN NOT NULL DEFAULT false,
                written_mark NUMERIC(6,2),
                mcq_mark NUMERIC(6,2),
                total_mark NUMERIC(6,2),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(exam_id, class_id, section_id, subject_id, student_id)
            );
            CREATE INDEX IF NOT EXISTS idx_mark_entries_exam ON "{schemaName}".mark_entries(exam_id);
            CREATE INDEX IF NOT EXISTS idx_mark_entries_student ON "{schemaName}".mark_entries(student_id);
            CREATE INDEX IF NOT EXISTS idx_mark_entries_subject ON "{schemaName}".mark_entries(subject_id);

            INSERT INTO "{schemaName}".exam_terms (name) SELECT 'Mid Term' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".exam_terms WHERE name = 'Mid Term');
            INSERT INTO "{schemaName}".exam_terms (name) SELECT 'Half Yearly' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".exam_terms WHERE name = 'Half Yearly');
            INSERT INTO "{schemaName}".exam_terms (name) SELECT 'Annual Exam' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".exam_terms WHERE name = 'Annual Exam');
            INSERT INTO "{schemaName}".exam_terms (name) SELECT 'Pre-test' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".exam_terms WHERE name = 'Pre-test');

            INSERT INTO "{schemaName}".mark_distributions (name) SELECT 'WRITTEN' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".mark_distributions WHERE name = 'WRITTEN');
            INSERT INTO "{schemaName}".mark_distributions (name) SELECT 'MCQ' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".mark_distributions WHERE name = 'MCQ');

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809080441_AddExamMasterModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task DropSchemaAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await _masterDbContext.Database.ExecuteSqlRawAsync(
            $"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE",
            cancellationToken);
    }
}
