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
