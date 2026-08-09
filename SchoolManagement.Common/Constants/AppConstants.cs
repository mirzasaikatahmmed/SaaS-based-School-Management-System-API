namespace SchoolManagement.Common.Constants;

public static class AppConstants
{
    public const string TenantHeaderName = "X-Tenant-ID";
    public const string SchemaPrefix = "tenant_";
    public const string BucketPrefix = "school-";
    public const string DefaultSubscriptionPlan = "basic";
    public const int DefaultMaxUsers = 100;
    /// <summary>Matches live ahskbera bcrypt cost ($2y$10$). New hashes use work factor 12.</summary>
    public const int BcryptWorkFactor = 12;
    public const int PresignedUrlExpirySeconds = 3600;
    public const decimal LibraryFinePerDay = 2m;

    /// <summary>
    /// Role names/prefixes aligned with ahskbera_main.roles.
    /// JWT / [Authorize] use Prefix values; Name is the display label.
    /// </summary>
    public static class Roles
    {
        public const string SuperAdmin = "superadmin";
        public const string Admin = "admin";
        public const string Teacher = "teacher";
        public const string Accountant = "accountant";
        public const string Librarian = "librarian";
        public const string Parent = "parent";
        public const string Student = "student";
        public const string Receptionist = "receptionist";
        public const string Staff = "staff";
        public const string Demo = "demo";

        // Back-compat aliases used in earlier foundation code
        public const string SchoolAdmin = Admin;

        public static readonly (int Id, string Name, string Prefix, bool IsSystem)[] Seed =
        [
            (1, "Super Admin", SuperAdmin, true),
            (2, "Admin", Admin, true),
            (3, "Teacher", Teacher, true),
            (4, "Accountant", Accountant, true),
            (5, "Librarian", Librarian, true),
            (6, "Parent", Parent, true),
            (7, "Student", Student, true),
            (8, "Receptionist", Receptionist, true),
            (9, "Staff", Staff, false),
            (10, "Demo", Demo, false)
        ];
    }

    public static class Claims
    {
        public const string UserId = "userId";
        public const string Email = "email";
        public const string TenantId = "tenantId";
        public const string SchemaName = "schemaName";
        public const string Roles = "roles";
        public const string IsSuperAdmin = "isSuperAdmin";
    }

    public static class StorageFolders
    {
        public const string Logo = "logo";
        public const string Avatars = "avatars";
        public const string Documents = "documents";
        public const string Assignments = "assignments";
        public const string Reports = "reports";
        public const string Students = "students";
        public const string Guardians = "guardians";
        public const string Employees = "employees";
        public const string LeaveAttachments = "leave-attachments";
        public const string Imports = "imports";
        public const string LibraryCovers = "library/covers";
        public const string Events = "events";
        public const string AccountingDeposits = "accounting/deposits";
        public const string AccountingExpenses = "accounting/expenses";
        public const string Messages = "messages";
        public const string SettingsLogos = "settings/logos";
    }
}
