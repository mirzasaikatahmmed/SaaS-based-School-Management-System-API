namespace SchoolManagement.BLL.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SchoolManagementSystem";
    public string Audience { get; set; } = "SchoolManagementClients";
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

public class MinioSettings
{
    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin123";
    public bool UseSSL { get; set; } = false;
    /// <summary>Single shared bucket; each school uses a folder prefix {slug}/.</summary>
    public string BucketName { get; set; } = "school-mgmt";
}

public class SuperAdminSettings
{
    public string Email { get; set; } = "superadmin@schoolmgmt.com";
    public string Password { get; set; } = "SuperAdmin@123";
    public string Username { get; set; } = "superadmin";
    public string FirstName { get; set; } = "Super";
    public string LastName { get; set; } = "Admin";
}
