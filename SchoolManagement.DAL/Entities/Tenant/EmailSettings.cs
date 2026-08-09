namespace SchoolManagement.DAL.Entities.Tenant;

/// <summary>Single-row per tenant SMTP configuration.</summary>
public class EmailSettings
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>System/from address used for outbound mail.</summary>
    public string? SystemEmail { get; set; }

    /// <summary>Transport protocol — currently only SMTP.</summary>
    public string Protocol { get; set; } = EmailProtocols.Smtp;

    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }

    /// <summary>Encrypted at rest via ASP.NET Data Protection.</summary>
    public string? SmtpPassword { get; set; }

    /// <summary>None | SSL | TLS</summary>
    public string SmtpSecure { get; set; } = SmtpSecureModes.Tls;

    public bool SmtpAuth { get; set; } = true;
    public string? FromName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class EmailProtocols
{
    public const string Smtp = "SMTP";
    public static readonly string[] All = [Smtp];
    public static bool IsValid(string? v) => All.Any(x => x.Equals(v?.Trim(), StringComparison.OrdinalIgnoreCase));
}

public static class SmtpSecureModes
{
    public const string None = "None";
    public const string Ssl = "SSL";
    public const string Tls = "TLS";
    public static readonly string[] All = [None, Ssl, Tls];
    public static bool IsValid(string? v) => All.Any(x => x.Equals(v?.Trim(), StringComparison.OrdinalIgnoreCase));
}

/// <summary>Per-event email template; placeholders use <c>{key}</c> syntax rendered by INotificationTemplateService.</summary>
public class EmailTemplate
{
    public Guid Id { get; set; }
    public string EventKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public bool NotifyEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Well-known email/SMS trigger event keys seeded on provisioning.</summary>
public static class NotificationEventKeys
{
    // Email
    public const string AccountRegistered = "AccountRegistered";
    public const string ForgotPassword = "ForgotPassword";
    public const string ChangePassword = "ChangePassword";
    public const string NewMessageReceived = "NewMessageReceived";
    public const string PayslipGenerated = "PayslipGenerated";
    public const string Award = "Award";
    public const string LeaveApprove = "LeaveApprove";
    public const string LeaveReject = "LeaveReject";
    public const string AdvanceSalaryReject = "AdvanceSalaryReject";

    // SMS
    public const string Admission = "Admission";
    public const string FeeCollection = "FeeCollection";
    public const string Attendance = "Attendance";
    public const string ExamAttendance = "ExamAttendance";
    public const string ExamResults = "ExamResults";
    public const string Homework = "Homework";
    public const string LiveClass = "LiveClass";
    public const string OnlineExamPublish = "OnlineExamPublish";
    public const string StudentBirthdayWishes = "StudentBirthdayWishes";
    public const string StaffBirthdayWishes = "StaffBirthdayWishes";
    public const string AlumniEvent = "AlumniEvent";
    public const string FeesReminder = "FeesReminder";

    public static readonly (string Key, string Name, string DefaultSubject, string DefaultBody)[] EmailDefaults =
    [
        (AccountRegistered, "Account Registered", "Welcome to {institute_name}", "<p>Hello {name},</p><p>Your account has been created. Username: {login_username}</p>"),
        (ForgotPassword, "Forgot Password", "Password reset — {institute_name}", "<p>Hello {name},</p><p>Use this link to reset your password: {login_url}</p>"),
        (ChangePassword, "Change Password", "Password changed — {institute_name}", "<p>Hello {name},</p><p>Your password was changed successfully.</p>"),
        (NewMessageReceived, "New Message Received", "New message at {institute_name}", "<p>Hello {name},</p><p>You have a new message.</p>"),
        (PayslipGenerated, "Payslip Generated", "Payslip ready — {institute_name}", "<p>Hello {name},</p><p>Your payslip is ready.</p>"),
        (Award, "Award", "Award notification — {institute_name}", "<p>Congratulations {name}!</p>"),
        (LeaveApprove, "Leave Approved", "Leave approved — {institute_name}", "<p>Hello {name},</p><p>Your leave request was approved.</p>"),
        (LeaveReject, "Leave Rejected", "Leave rejected — {institute_name}", "<p>Hello {name},</p><p>Your leave request was rejected.</p>"),
        (AdvanceSalaryReject, "Advance Salary Rejected", "Advance salary rejected — {institute_name}", "<p>Hello {name},</p><p>Your advance salary request was rejected.</p>")
    ];

    public static readonly (string Key, string Name, string DefaultBody)[] SmsDefaults =
    [
        (Admission, "Admission", "Dear {name}, admitted to {class}-{section}. Roll: {roll}, Reg: {register_no}. Date: {admission_date}"),
        (FeeCollection, "Fee Collection", "Dear parent, fee payment received for {name}."),
        (FeesReminder, "Fees Reminder", "Dear parent, fee reminder for {name}."),
        (Attendance, "Attendance", "Dear parent, {name} attendance update."),
        (ExamAttendance, "Exam Attendance", "Dear parent, {name} exam attendance recorded."),
        (ExamResults, "Exam Results", "Dear parent, exam results published for {name}."),
        (Homework, "Homework", "New homework assigned for {name}."),
        (LiveClass, "Live Class", "Live class starting soon for {name}."),
        (OnlineExamPublish, "Online Exam Publish", "Online exam published for {name}."),
        (StudentBirthdayWishes, "Student Birthday", "Happy Birthday {name}! — {institute_name}"),
        (StaffBirthdayWishes, "Staff Birthday", "Happy Birthday {name}! — {institute_name}"),
        (AlumniEvent, "Alumni Event", "Alumni event reminder from {institute_name}.")
    ];
}
