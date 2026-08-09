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
    Task EnsureGradesAttendanceLibraryEventsModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureStudentAndOfficeAccountingModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureMessageAndSettingsModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureBiometricModuleAsync(string schemaName, CancellationToken cancellationToken = default);
    Task EnsureSettingsModuleAsync(string schemaName, CancellationToken cancellationToken = default);
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
                is_elective BOOLEAN NOT NULL DEFAULT false,
                elective_group VARCHAR(50),
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

        // EnsureAcademicModuleAsync references employees(id) (class_teacher_allocations, class_schedule_periods),
        // so the employee module must be provisioned first when reached via this chain
        // (Student/OfficeAccounting -> GradesAttendanceLibraryEvents -> ExamMaster -> Academic).
        await EnsureEmployeeModuleAsync(schemaName, cancellationToken);
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

    public async Task EnsureGradesAttendanceLibraryEventsModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureExamMasterModuleAsync(schemaName, cancellationToken);

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".grade_ranges (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                grade_name VARCHAR(10) NOT NULL,
                grade_point NUMERIC(4,2) NOT NULL,
                min_percentage NUMERIC(5,2) NOT NULL,
                max_percentage NUMERIC(5,2) NOT NULL,
                remarks VARCHAR(200),
                is_active BOOLEAN NOT NULL DEFAULT true,
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_grade_ranges_name"
                ON "{schemaName}".grade_ranges (grade_name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_positions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                exam_id UUID NOT NULL REFERENCES "{schemaName}".exams(id),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                academic_year INT NOT NULL,
                total_marks NUMERIC(8,2) NOT NULL DEFAULT 0,
                full_marks NUMERIC(8,2) NOT NULL DEFAULT 0,
                percentage NUMERIC(5,2) NOT NULL DEFAULT 0,
                gpa NUMERIC(4,2) NOT NULL DEFAULT 0,
                result VARCHAR(10) NOT NULL DEFAULT 'FAIL',
                position INT,
                principal_comments TEXT,
                teacher_comments TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(exam_id, class_id, section_id, student_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".student_attendance (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                attendance_date DATE NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'Present',
                remarks TEXT,
                created_by UUID REFERENCES "{schemaName}".users(id) ON DELETE SET NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(student_id, attendance_date)
            );
            CREATE INDEX IF NOT EXISTS idx_student_att_date ON "{schemaName}".student_attendance(attendance_date);

            CREATE TABLE IF NOT EXISTS "{schemaName}".employee_attendance (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                employee_id UUID NOT NULL REFERENCES "{schemaName}".employees(id),
                attendance_date DATE NOT NULL,
                status VARCHAR(20),
                remarks TEXT,
                created_by UUID REFERENCES "{schemaName}".users(id) ON DELETE SET NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(employee_id, attendance_date)
            );
            CREATE INDEX IF NOT EXISTS idx_employee_att_date ON "{schemaName}".employee_attendance(attendance_date);

            CREATE TABLE IF NOT EXISTS "{schemaName}".exam_attendance (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                exam_id UUID NOT NULL REFERENCES "{schemaName}".exams(id),
                subject_id UUID NOT NULL REFERENCES "{schemaName}".subjects(id),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                status VARCHAR(20) NOT NULL DEFAULT 'Present',
                remarks TEXT,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(exam_id, subject_id, student_id)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".book_categories (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_book_categories_name"
                ON "{schemaName}".book_categories (name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".books (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                title VARCHAR(500) NOT NULL,
                isbn_no VARCHAR(100),
                author VARCHAR(300),
                edition VARCHAR(100),
                publisher VARCHAR(300),
                purchase_date DATE,
                category_id UUID REFERENCES "{schemaName}".book_categories(id) ON DELETE SET NULL,
                description TEXT,
                price NUMERIC(10,2),
                cover_image_url VARCHAR(500),
                total_stock INT NOT NULL DEFAULT 0,
                issued_copies INT NOT NULL DEFAULT 0,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".book_issues (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                book_id UUID NOT NULL REFERENCES "{schemaName}".books(id),
                role VARCHAR(50) NOT NULL,
                student_id UUID REFERENCES "{schemaName}".students(id) ON DELETE SET NULL,
                employee_id UUID REFERENCES "{schemaName}".employees(id) ON DELETE SET NULL,
                user_name VARCHAR(200),
                date_of_issue DATE NOT NULL DEFAULT CURRENT_DATE,
                date_of_expiry DATE NOT NULL,
                return_date DATE,
                fine NUMERIC(10,2) DEFAULT 0,
                status VARCHAR(20) NOT NULL DEFAULT 'Issued',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS idx_book_issues_book ON "{schemaName}".book_issues(book_id);
            CREATE INDEX IF NOT EXISTS idx_book_issues_status ON "{schemaName}".book_issues(status);

            CREATE TABLE IF NOT EXISTS "{schemaName}".event_types (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                icon VARCHAR(100),
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_event_types_name"
                ON "{schemaName}".event_types (name);

            CREATE TABLE IF NOT EXISTS "{schemaName}".events (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                title VARCHAR(300) NOT NULL,
                event_type_id UUID REFERENCES "{schemaName}".event_types(id) ON DELETE SET NULL,
                is_holiday BOOLEAN NOT NULL DEFAULT false,
                audience VARCHAR(50) NOT NULL DEFAULT 'Everybody',
                date_of_start DATE NOT NULL,
                date_of_end DATE NOT NULL,
                description TEXT,
                image_url VARCHAR(500),
                show_website BOOLEAN NOT NULL DEFAULT false,
                is_published BOOLEAN NOT NULL DEFAULT false,
                created_by UUID REFERENCES "{schemaName}".users(id) ON DELETE SET NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS idx_events_dates ON "{schemaName}".events(date_of_start, date_of_end);

            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'A+', 5.00, 80, 100, 'Excellent!', 1 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'A+');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'A', 4.00, 70, 79, 'Very Good', 2 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'A');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'A-', 3.50, 60, 69, 'Good', 3 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'A-');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'B', 3.00, 50, 59, 'Average', 4 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'B');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'C', 2.00, 40, 49, 'Below Average', 5 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'C');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'D', 1.00, 33, 39, 'Pass', 6 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'D');
            INSERT INTO "{schemaName}".grade_ranges (grade_name, grade_point, min_percentage, max_percentage, remarks, sort_order)
            SELECT 'F', 0.00, 0, 32, 'Fail', 7 WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".grade_ranges WHERE grade_name = 'F');

            INSERT INTO "{schemaName}".event_types (name, icon)
            SELECT 'Holiday', 'bell' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".event_types WHERE name = 'Holiday');

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809175641_AddGradesAttendanceLibraryEvents', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }


    public async Task EnsureStudentAndOfficeAccountingModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureGradesAttendanceLibraryEventsModuleAsync(schemaName, cancellationToken);

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".offline_payment_types (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                instructions TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".offline_payments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                trx_id VARCHAR(100) NOT NULL UNIQUE,
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                payment_type_id UUID REFERENCES "{schemaName}".offline_payment_types(id),
                class_id UUID REFERENCES "{schemaName}".classes(id),
                section_id UUID REFERENCES "{schemaName}".sections(id),
                payment_date DATE NOT NULL,
                submit_date TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                amount NUMERIC(12,2) NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'Pending',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".fees_types (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                fee_code VARCHAR(100) NOT NULL,
                description TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_fees_types_fee_code"
                ON "{schemaName}".fees_types (fee_code);

            CREATE TABLE IF NOT EXISTS "{schemaName}".fees_groups (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                description TEXT,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".fees_group_items (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                group_id UUID NOT NULL REFERENCES "{schemaName}".fees_groups(id) ON DELETE CASCADE,
                fees_type_id UUID NOT NULL REFERENCES "{schemaName}".fees_types(id),
                due_date DATE NOT NULL,
                amount NUMERIC(12,2) NOT NULL,
                sort_order INT NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".fine_setups (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                group_id UUID NOT NULL REFERENCES "{schemaName}".fees_groups(id),
                fees_type_id UUID NOT NULL REFERENCES "{schemaName}".fees_types(id),
                fine_type VARCHAR(50) NOT NULL,
                fine_value NUMERIC(10,2) NOT NULL,
                late_fee_frequency VARCHAR(50),
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_{schemaName}_fine_setups_group_type"
                ON "{schemaName}".fine_setups (group_id, fees_type_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".fees_allocations (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                fees_group_id UUID NOT NULL REFERENCES "{schemaName}".fees_groups(id),
                academic_year INT NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(class_id, section_id, fees_group_id, academic_year)
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".student_fee_invoices (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID NOT NULL REFERENCES "{schemaName}".students(id),
                fees_allocation_id UUID NOT NULL REFERENCES "{schemaName}".fees_allocations(id),
                fees_group_id UUID NOT NULL REFERENCES "{schemaName}".fees_groups(id),
                class_id UUID NOT NULL REFERENCES "{schemaName}".classes(id),
                section_id UUID NOT NULL REFERENCES "{schemaName}".sections(id),
                total_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
                paid_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
                fine_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
                due_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
                status VARCHAR(20) NOT NULL DEFAULT 'Unpaid',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS idx_invoices_student ON "{schemaName}".student_fee_invoices(student_id);
            CREATE INDEX IF NOT EXISTS idx_invoices_status ON "{schemaName}".student_fee_invoices(status);

            CREATE TABLE IF NOT EXISTS "{schemaName}".fees_reminders (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                frequency VARCHAR(50) NOT NULL,
                days INT NOT NULL,
                message TEXT,
                dlt_template_id VARCHAR(200),
                notify_student BOOLEAN NOT NULL DEFAULT false,
                notify_guardian BOOLEAN NOT NULL DEFAULT false,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".voucher_heads (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(200) NOT NULL,
                type VARCHAR(20) NOT NULL,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".accounting_accounts (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                account_name VARCHAR(200) NOT NULL,
                account_number VARCHAR(100),
                description TEXT,
                opening_balance NUMERIC(14,2) NOT NULL DEFAULT 0,
                current_balance NUMERIC(14,2) NOT NULL DEFAULT 0,
                date DATE NOT NULL DEFAULT CURRENT_DATE,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{schemaName}".accounting_deposits (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                account_id UUID NOT NULL REFERENCES "{schemaName}".accounting_accounts(id),
                voucher_head_id UUID NOT NULL REFERENCES "{schemaName}".voucher_heads(id),
                ref_no VARCHAR(200),
                amount NUMERIC(14,2) NOT NULL,
                deposit_date DATE NOT NULL DEFAULT CURRENT_DATE,
                pay_via VARCHAR(50),
                description TEXT,
                attachment_url VARCHAR(500),
                created_by UUID REFERENCES "{schemaName}".users(id),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS idx_deposits_account ON "{schemaName}".accounting_deposits(account_id);

            CREATE TABLE IF NOT EXISTS "{schemaName}".accounting_expenses (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                account_id UUID NOT NULL REFERENCES "{schemaName}".accounting_accounts(id),
                voucher_head_id UUID NOT NULL REFERENCES "{schemaName}".voucher_heads(id),
                ref_no VARCHAR(200),
                amount NUMERIC(14,2) NOT NULL,
                expense_date DATE NOT NULL DEFAULT CURRENT_DATE,
                pay_via VARCHAR(50),
                description TEXT,
                attachment_url VARCHAR(500),
                created_by UUID REFERENCES "{schemaName}".users(id),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS idx_expenses_account ON "{schemaName}".accounting_expenses(account_id);

            INSERT INTO "{schemaName}".fees_types (name, fee_code) SELECT 'Monthly Fee', 'monthly-fee' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".fees_types WHERE fee_code = 'monthly-fee');
            INSERT INTO "{schemaName}".fees_types (name, fee_code) SELECT 'Exam Fee', 'exam-fee' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".fees_types WHERE fee_code = 'exam-fee');
            INSERT INTO "{schemaName}".fees_types (name, fee_code) SELECT 'Admission Fee', 'admission-fee' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".fees_types WHERE fee_code = 'admission-fee');

            INSERT INTO "{schemaName}".voucher_heads (name, type) SELECT 'School Fees', 'Income' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".voucher_heads WHERE name = 'School Fees' AND type = 'Income');
            INSERT INTO "{schemaName}".voucher_heads (name, type) SELECT 'Government Grant', 'Income' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".voucher_heads WHERE name = 'Government Grant' AND type = 'Income');
            INSERT INTO "{schemaName}".voucher_heads (name, type) SELECT 'Salary', 'Expense' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".voucher_heads WHERE name = 'Salary' AND type = 'Expense');
            INSERT INTO "{schemaName}".voucher_heads (name, type) SELECT 'Utilities', 'Expense' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".voucher_heads WHERE name = 'Utilities' AND type = 'Expense');
            INSERT INTO "{schemaName}".voucher_heads (name, type) SELECT 'Stationery', 'Expense' WHERE NOT EXISTS (SELECT 1 FROM "{schemaName}".voucher_heads WHERE name = 'Stationery' AND type = 'Expense');

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809182321_AddStudentAndOfficeAccounting', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task EnsureMessageAndSettingsModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureStudentAndOfficeAccountingModuleAsync(schemaName, cancellationToken);

        var sql = $$"""
            CREATE TABLE IF NOT EXISTS "{{schemaName}}".messages (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                sender_id UUID NOT NULL REFERENCES "{{schemaName}}".users(id),
                sender_name VARCHAR(200) NOT NULL,
                parent_message_id UUID REFERENCES "{{schemaName}}".messages(id),
                subject VARCHAR(500) NOT NULL,
                body TEXT NOT NULL,
                attachment_url VARCHAR(500),
                attachment_name VARCHAR(255),
                is_deleted_by_sender BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS "idx_{{schemaName}}_messages_sender"
                ON "{{schemaName}}".messages (sender_id);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".message_recipients (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                message_id UUID NOT NULL REFERENCES "{{schemaName}}".messages(id) ON DELETE CASCADE,
                recipient_id UUID NOT NULL REFERENCES "{{schemaName}}".users(id),
                recipient_name VARCHAR(200) NOT NULL,
                is_read BOOLEAN NOT NULL DEFAULT false,
                read_at TIMESTAMPTZ,
                is_important BOOLEAN NOT NULL DEFAULT false,
                is_deleted BOOLEAN NOT NULL DEFAULT false,
                deleted_at TIMESTAMPTZ,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS "idx_{{schemaName}}_message_recipients_recipient"
                ON "{{schemaName}}".message_recipients (recipient_id);
            CREATE INDEX IF NOT EXISTS "idx_{{schemaName}}_message_recipients_message"
                ON "{{schemaName}}".message_recipients (message_id);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".school_settings (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                school_name VARCHAR(200),
                school_code VARCHAR(50),
                address TEXT,
                phone VARCHAR(50),
                email VARCHAR(255),
                website VARCHAR(255),
                timezone VARCHAR(100) NOT NULL DEFAULT 'Asia/Dhaka',
                currency VARCHAR(10) NOT NULL DEFAULT 'BDT',
                currency_symbol VARCHAR(10) NOT NULL DEFAULT '৳',
                date_format VARCHAR(20) NOT NULL DEFAULT 'DD/MM/YYYY',
                language VARCHAR(20) NOT NULL DEFAULT 'en',
                allow_student_login BOOLEAN NOT NULL DEFAULT true,
                allow_guardian_login BOOLEAN NOT NULL DEFAULT true,
                show_fees_in_student_panel BOOLEAN NOT NULL DEFAULT true,
                show_attendance_in_student_panel BOOLEAN NOT NULL DEFAULT true,
                show_result_in_student_panel BOOLEAN NOT NULL DEFAULT true,
                student_panel_notice_message TEXT,
                system_logo_url VARCHAR(500),
                text_logo_url VARCHAR(500),
                printing_logo_url VARCHAR(500),
                report_card_logo_url VARCHAR(500),
                payment_gateways JSONB NOT NULL DEFAULT '__EMPTY_JSON_OBJECT__'::jsonb,
                active_gateways JSONB NOT NULL DEFAULT '[]'::jsonb,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            INSERT INTO "{{schemaName}}".school_settings (id)
            SELECT gen_random_uuid()
            WHERE NOT EXISTS (SELECT 1 FROM "{{schemaName}}".school_settings);

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809184708_AddMessageAndSettings', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        // ExecuteSqlRawAsync treats the SQL text as a composite format string, so a literal
        // '{}' (empty JSON object) must be escaped as '{{}}' — inserted post-interpolation
        // via a placeholder to avoid raw-string brace-escaping ambiguity.
        sql = sql.Replace("__EMPTY_JSON_OBJECT__", "{{}}");

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task EnsureBiometricModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureMessageAndSettingsModuleAsync(schemaName, cancellationToken);

        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{schemaName}".biometric_devices (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                serial_number VARCHAR(100) NOT NULL,
                name VARCHAR(200) NOT NULL,
                location VARCHAR(200),
                device_model VARCHAR(50) NOT NULL DEFAULT 'K40-H',
                exam_grace_minutes_before INT NOT NULL DEFAULT 30,
                exam_grace_minutes_after INT NOT NULL DEFAULT 30,
                is_active BOOLEAN NOT NULL DEFAULT true,
                last_seen_at TIMESTAMPTZ,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_biometric_devices_sn"
                ON "{schemaName}".biometric_devices (serial_number);

            CREATE TABLE IF NOT EXISTS "{schemaName}".biometric_user_maps (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                device_pin VARCHAR(50) NOT NULL,
                person_type VARCHAR(20) NOT NULL DEFAULT 'Student',
                student_id UUID REFERENCES "{schemaName}".students(id) ON DELETE CASCADE,
                employee_id UUID REFERENCES "{schemaName}".employees(id) ON DELETE CASCADE,
                is_active BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{schemaName}_biometric_user_maps_pin"
                ON "{schemaName}".biometric_user_maps (device_pin);

            CREATE TABLE IF NOT EXISTS "{schemaName}".biometric_punch_logs (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                device_id UUID REFERENCES "{schemaName}".biometric_devices(id) ON DELETE SET NULL,
                device_sn VARCHAR(100) NOT NULL,
                device_pin VARCHAR(50) NOT NULL,
                punch_time TIMESTAMPTZ NOT NULL,
                punch_kind VARCHAR(20) NOT NULL DEFAULT 'Unmapped',
                status_applied VARCHAR(20) NOT NULL DEFAULT 'Present',
                student_id UUID REFERENCES "{schemaName}".students(id) ON DELETE SET NULL,
                employee_id UUID REFERENCES "{schemaName}".employees(id) ON DELETE SET NULL,
                exam_id UUID,
                subject_id UUID,
                raw_line VARCHAR(500),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_biometric_punch_logs_time"
                ON "{schemaName}".biometric_punch_logs (punch_time);
            CREATE INDEX IF NOT EXISTS "idx_{schemaName}_biometric_punch_logs_pin"
                ON "{schemaName}".biometric_punch_logs (device_pin);

            INSERT INTO "{schemaName}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809192253_AddBiometricModule', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        await _masterDbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public async Task EnsureSettingsModuleAsync(string schemaName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            !System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^tenant_[a-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}", nameof(schemaName));
        }

        await EnsureBiometricModuleAsync(schemaName, cancellationToken);

        var sql = $$"""
            ALTER TABLE "{{schemaName}}".roles
                ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT true;

            ALTER TABLE "{{schemaName}}".users
                ADD COLUMN IF NOT EXISTS password_reveal_encrypted TEXT;

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".role_permissions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                role_id UUID NOT NULL REFERENCES "{{schemaName}}".roles(id) ON DELETE CASCADE,
                feature_key VARCHAR(150) NOT NULL,
                can_view BOOLEAN NOT NULL DEFAULT false,
                can_add BOOLEAN NOT NULL DEFAULT false,
                can_edit BOOLEAN NOT NULL DEFAULT false,
                can_delete BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{{schemaName}}_role_permissions_role_feature"
                ON "{{schemaName}}".role_permissions (role_id, feature_key);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".academic_sessions (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(50) NOT NULL,
                is_selected BOOLEAN NOT NULL DEFAULT false,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{{schemaName}}_academic_sessions_name"
                ON "{{schemaName}}".academic_sessions (name);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".database_backups (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                file_name VARCHAR(255) NOT NULL,
                object_key VARCHAR(500) NOT NULL,
                size_bytes BIGINT NOT NULL DEFAULT 0,
                note TEXT,
                created_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            ALTER TABLE "{{schemaName}}".school_settings
                ADD COLUMN IF NOT EXISTS attendance_type VARCHAR(20) NOT NULL DEFAULT 'DayWise',
                ADD COLUMN IF NOT EXISTS weekend_days VARCHAR(20) NOT NULL DEFAULT '5,6',
                ADD COLUMN IF NOT EXISTS default_deposit_account_id UUID,
                ADD COLUMN IF NOT EXISTS default_expense_account_id UUID,
                ADD COLUMN IF NOT EXISTS accounting_links_enabled BOOLEAN NOT NULL DEFAULT false,
                ADD COLUMN IF NOT EXISTS cron_secret_key VARCHAR(100);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".student_subject_attendance (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID NOT NULL REFERENCES "{{schemaName}}".students(id),
                class_id UUID NOT NULL REFERENCES "{{schemaName}}".classes(id),
                section_id UUID NOT NULL REFERENCES "{{schemaName}}".sections(id),
                subject_id UUID NOT NULL REFERENCES "{{schemaName}}".subjects(id),
                attendance_date DATE NOT NULL,
                status VARCHAR(20) NOT NULL DEFAULT 'Present',
                remarks TEXT,
                created_by UUID REFERENCES "{{schemaName}}".users(id) ON DELETE SET NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(student_id, subject_id, attendance_date)
            );
            CREATE INDEX IF NOT EXISTS idx_student_subject_att_date
                ON "{{schemaName}}".student_subject_attendance(attendance_date);

            ALTER TABLE "{{schemaName}}".class_subject_assignment_items
                ADD COLUMN IF NOT EXISTS is_elective BOOLEAN NOT NULL DEFAULT false,
                ADD COLUMN IF NOT EXISTS elective_group VARCHAR(50);

            ALTER TABLE "{{schemaName}}".subjects
                ADD COLUMN IF NOT EXISTS can_be_additional BOOLEAN NOT NULL DEFAULT false,
                ADD COLUMN IF NOT EXISTS is_continuous_assessment BOOLEAN NOT NULL DEFAULT false;

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".student_subject_enrollments (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                student_id UUID NOT NULL REFERENCES "{{schemaName}}".students(id) ON DELETE CASCADE,
                subject_id UUID NOT NULL REFERENCES "{{schemaName}}".subjects(id),
                additional_subject_id UUID REFERENCES "{{schemaName}}".subjects(id),
                class_id UUID NOT NULL REFERENCES "{{schemaName}}".classes(id),
                section_id UUID NOT NULL REFERENCES "{{schemaName}}".sections(id),
                academic_year INT NOT NULL,
                elective_group VARCHAR(50) NOT NULL DEFAULT '4th',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                UNIQUE(student_id, elective_group, academic_year),
                UNIQUE(student_id, subject_id, academic_year)
            );
            ALTER TABLE "{{schemaName}}".student_subject_enrollments
                ADD COLUMN IF NOT EXISTS additional_subject_id UUID REFERENCES "{{schemaName}}".subjects(id);
            CREATE INDEX IF NOT EXISTS "idx_{{schemaName}}_student_electives_class"
                ON "{{schemaName}}".student_subject_enrollments (class_id, section_id, academic_year, elective_group);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".email_settings (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                is_enabled BOOLEAN NOT NULL DEFAULT false,
                system_email VARCHAR(255),
                protocol VARCHAR(20) NOT NULL DEFAULT 'SMTP',
                smtp_host VARCHAR(255),
                smtp_port INT NOT NULL DEFAULT 587,
                smtp_username VARCHAR(255),
                smtp_password VARCHAR(1000),
                smtp_secure VARCHAR(20) NOT NULL DEFAULT 'TLS',
                smtp_auth BOOLEAN NOT NULL DEFAULT true,
                from_name VARCHAR(200),
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".email_templates (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                event_key VARCHAR(100) NOT NULL,
                name VARCHAR(200) NOT NULL,
                subject VARCHAR(500) NOT NULL,
                body_html TEXT NOT NULL,
                notify_enabled BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{{schemaName}}_email_templates_event"
                ON "{{schemaName}}".email_templates (event_key);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".sms_settings (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                is_enabled BOOLEAN NOT NULL DEFAULT false,
                activated_gateway VARCHAR(50) NOT NULL DEFAULT 'bulksmsbd',
                credentials_json JSONB NOT NULL DEFAULT '__EMPTY_JSON_OBJECT__'::jsonb,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".sms_templates (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                event_key VARCHAR(100) NOT NULL,
                name VARCHAR(200) NOT NULL,
                body TEXT NOT NULL,
                notify_student BOOLEAN NOT NULL DEFAULT false,
                notify_parent BOOLEAN NOT NULL DEFAULT true,
                dlt_template_id VARCHAR(100),
                notify_enabled BOOLEAN NOT NULL DEFAULT true,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{{schemaName}}_sms_templates_event"
                ON "{{schemaName}}".sms_templates (event_key);

            CREATE TABLE IF NOT EXISTS "{{schemaName}}".notification_dispatch_log (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                job_name VARCHAR(100) NOT NULL,
                entity_key VARCHAR(200) NOT NULL,
                run_date DATE NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "ux_{{schemaName}}_notification_dispatch_log"
                ON "{{schemaName}}".notification_dispatch_log (job_name, entity_key, run_date);

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809202445_AddSettingsModule', '10.0.0')
            ON CONFLICT DO NOTHING;

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809204404_AddPasswordRevealAndStudentReports', '10.0.0')
            ON CONFLICT DO NOTHING;

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809205248_AddAttendanceReports', '10.0.0')
            ON CONFLICT DO NOTHING;

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809210852_AddStudentElectives', '10.0.0')
            ON CONFLICT DO NOTHING;

            INSERT INTO "{{schemaName}}"."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20260809211622_AddAdditionalSubjectGpa', '10.0.0')
            ON CONFLICT DO NOTHING;
            """;

        sql = sql.Replace("__EMPTY_JSON_OBJECT__", "{{}}");
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
