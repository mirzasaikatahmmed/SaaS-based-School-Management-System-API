namespace SchoolManagement.Common.Constants;

/// <summary>
/// Static catalog of every permission-checkable feature in the UI. Roles/Permissions module
/// builds its matrix (view/add/edit/delete) from this list; <c>ViewOnly</c> features only expose
/// a "view" toggle (reports, dashboards, read-only lists).
/// </summary>
public static class AppFeatures
{
    public record FeatureDef(string Key, string Module, string Name, bool ViewOnly = false);

    public static readonly FeatureDef[] All =
    [
        // Dashboard
        new("Dashboard.Overview", "Dashboard", "Dashboard Overview", ViewOnly: true),
        new("Dashboard.Analytics", "Dashboard", "Analytics & Reports", ViewOnly: true),

        // Website (public site builder)
        new("Website.Pages", "Website", "Website Pages"),
        new("Website.Menu", "Website", "Website Menu"),
        new("Website.Sliders", "Website", "Sliders / Banners"),
        new("Website.Gallery", "Website", "Photo Gallery"),

        // Reception
        new("Reception.VisitorBook", "Reception", "Visitor Book"),
        new("Reception.CallLog", "Reception", "Phone Call Log"),
        new("Reception.PostalReceive", "Reception", "Postal Receive"),
        new("Reception.PostalDispatch", "Reception", "Postal Dispatch"),
        new("Reception.Enquiry", "Reception", "Admission Enquiry"),
        new("Reception.Complaint", "Reception", "Complaint"),

        // Inventory
        new("Inventory.ItemCategory", "Inventory", "Item Category"),
        new("Inventory.ItemStore", "Inventory", "Item Store"),
        new("Inventory.ItemSupplier", "Inventory", "Item Supplier"),
        new("Inventory.ItemStock", "Inventory", "Item Stock"),
        new("Inventory.ItemIssue", "Inventory", "Item Issue"),

        // Student
        new("Student.StudentCategory", "Student", "Student Category"),
        new("Student.AddStudent", "Student", "Add Student"),
        new("Student.StudentList", "Student", "Student Details / List"),
        new("Student.OnlineAdmission", "Student", "Online Admission"),
        new("Student.CsvImport", "Student", "CSV Student Import"),
        new("Student.DisabledStudents", "Student", "Disabled Students"),
        new("Student.StudentPromote", "Student", "Student Promotion"),
        new("Student.DeactivateReason", "Student", "Deactivate Reason"),
        new("Student.LoginDeactivate", "Student", "Student Login Deactivate"),

        // Student reports
        new("Reports.StudentLoginCredential", "Reports", "Login Credential Report"),
        new("Reports.StudentAdmission", "Reports", "Admission Report", ViewOnly: true),
        new("Reports.StudentClassSection", "Reports", "Class & Section Report", ViewOnly: true),
        new("Reports.StudentSibling", "Reports", "Sibling Report", ViewOnly: true),

        // Attendance reports
        new("Reports.AttendanceStudent", "Reports", "Student Attendance Report", ViewOnly: true),
        new("Reports.AttendanceStudentDaily", "Reports", "Student Daily Attendance Report", ViewOnly: true),
        new("Reports.AttendanceStudentOverview", "Reports", "Student Overview Attendance Report", ViewOnly: true),
        new("Reports.AttendanceSubjectWise", "Reports", "Subject Wise Attendance Report", ViewOnly: true),
        new("Reports.AttendanceSubjectWiseByDay", "Reports", "Subject Wise By Day Report", ViewOnly: true),
        new("Reports.AttendanceSubjectWiseByMonth", "Reports", "Subject Wise By Month Report", ViewOnly: true),
        new("Reports.AttendanceEmployee", "Reports", "Employee Attendance Report", ViewOnly: true),
        new("Reports.AttendanceExam", "Reports", "Exam Attendance Report", ViewOnly: true),
        new("Reports.AttendanceFingerprint", "Reports", "Fingerprint Attendance Logs", ViewOnly: true),
        new("Reports.Leave", "Reports", "Leave Reports", ViewOnly: true),
        new("Reports.PayrollSummary", "Reports", "Payroll Summary", ViewOnly: true),
        new("Reports.ExamReportCard", "Reports", "Exam Report Card", ViewOnly: true),
        new("Reports.ExamTabulation", "Reports", "Exam Tabulation Sheet", ViewOnly: true),
        new("Reports.ExamProgress", "Reports", "Exam Progress Reports", ViewOnly: true),

        // Parents
        new("Parents.ParentsList", "Parents", "Parents List"),
        new("Parents.LoginDeactivate", "Parents", "Parent Login Deactivate"),

        // Employee
        new("Employee.Department", "Employee", "Department"),
        new("Employee.Designation", "Employee", "Designation"),
        new("Employee.AddEmployee", "Employee", "Add Employee"),
        new("Employee.EmployeeList", "Employee", "Employee List"),
        new("Employee.CsvImport", "Employee", "CSV Employee Import"),
        new("Employee.LoginDeactivate", "Employee", "Employee Login Deactivate"),

        // Card management
        new("CardManagement.IdCard", "CardManagement", "ID Card"),
        new("CardManagement.AdmitCard", "CardManagement", "Admit Card"),

        // Certificate
        new("Certificate.CertificateTemplate", "Certificate", "Certificate Template"),
        new("Certificate.GenerateCertificate", "Certificate", "Generate Certificate"),

        // Human Resource
        new("HumanResource.SalaryTemplate", "HumanResource", "Salary Template"),
        new("HumanResource.AssignSalary", "HumanResource", "Assign Salary"),
        new("HumanResource.SalaryPayment", "HumanResource", "Salary Payment"),
        new("HumanResource.AdvanceSalary", "HumanResource", "Advance Salary"),
        new("HumanResource.LeaveCategory", "HumanResource", "Leave Category"),
        new("HumanResource.LeaveApplication", "HumanResource", "Leave Application"),
        new("HumanResource.Award", "HumanResource", "Award"),
        new("HumanResource.PayrollReport", "HumanResource", "Payroll Report", ViewOnly: true),

        // Academic
        new("Academic.ClassSection", "Academic", "Class & Section"),
        new("Academic.Subject", "Academic", "Subject"),
        new("Academic.AssignSubject", "Academic", "Assign Subject"),
        new("Academic.ClassTeacher", "Academic", "Class Teacher"),
        new("Academic.ClassSchedule", "Academic", "Class Schedule / Timetable"),

        // Live class
        new("LiveClass.LiveClass", "LiveClass", "Live Class"),

        // Attachments / notice book
        new("AttachmentsBook.NoticeBoard", "AttachmentsBook", "Notice Board"),
        new("AttachmentsBook.StudyMaterial", "AttachmentsBook", "Study Material"),

        // Homework
        new("Homework.Homework", "Homework", "Homework"),
        new("Homework.HomeworkEvaluation", "Homework", "Homework Evaluation"),

        // Exam master
        new("ExamMaster.ExamTerm", "ExamMaster", "Exam Term"),
        new("ExamMaster.ExamHall", "ExamMaster", "Exam Hall"),
        new("ExamMaster.MarkDistribution", "ExamMaster", "Mark Distribution"),
        new("ExamMaster.ExamSetup", "ExamMaster", "Exam Setup"),
        new("ExamMaster.ExamSchedule", "ExamMaster", "Exam Schedule"),
        new("ExamMaster.MarksRegister", "ExamMaster", "Marks Register / Entry"),
        new("ExamMaster.GradeRange", "ExamMaster", "Grade Range"),
        new("ExamMaster.ExamPosition", "ExamMaster", "Exam Position / Result"),

        // Online exam
        new("OnlineExam.QuestionBank", "OnlineExam", "Question Bank"),
        new("OnlineExam.OnlineExam", "OnlineExam", "Online Exam"),
        new("OnlineExam.OnlineExamResult", "OnlineExam", "Online Exam Result", ViewOnly: true),

        // Hostel
        new("Hostel.Hostel", "Hostel", "Hostel"),
        new("Hostel.HostelRoom", "Hostel", "Hostel Room"),
        new("Hostel.RoomAllocation", "Hostel", "Room Allocation"),

        // Transport
        new("Transport.TransportRoute", "Transport", "Transport Route"),
        new("Transport.Vehicle", "Transport", "Vehicle"),
        new("Transport.RouteAssign", "Transport", "Assign Route"),

        // Attendance
        new("Attendance.StudentAttendance", "Attendance", "Student Attendance"),
        new("Attendance.EmployeeAttendance", "Attendance", "Employee Attendance"),
        new("Attendance.ExamAttendance", "Attendance", "Exam Attendance"),
        new("Attendance.AttendanceReport", "Attendance", "Attendance Report", ViewOnly: true),

        // Library
        new("Library.BookCategory", "Library", "Book Category"),
        new("Library.Book", "Library", "Book"),
        new("Library.IssueReturn", "Library", "Issue / Return"),

        // Events
        new("Events.EventType", "Events", "Event Type"),
        new("Events.Event", "Events", "Event"),

        // Bulk SMS / Email
        new("BulkSmsEmail.SendSms", "BulkSmsEmail", "Send Bulk SMS"),
        new("BulkSmsEmail.SendEmail", "BulkSmsEmail", "Send Bulk Email"),
        new("BulkSmsEmail.SmsLog", "BulkSmsEmail", "SMS Log", ViewOnly: true),
        new("BulkSmsEmail.EmailLog", "BulkSmsEmail", "Email Log", ViewOnly: true),

        // Student accounting
        new("StudentAccounting.FeesType", "StudentAccounting", "Fees Type"),
        new("StudentAccounting.FeesGroup", "StudentAccounting", "Fees Group"),
        new("StudentAccounting.FeesAllocation", "StudentAccounting", "Fees Allocation"),
        new("StudentAccounting.FeesInvoice", "StudentAccounting", "Fees Invoice / Collect"),
        new("StudentAccounting.OfflinePayment", "StudentAccounting", "Offline Payment"),
        new("StudentAccounting.FeesReminder", "StudentAccounting", "Fees Reminder"),
        new("StudentAccounting.FeesReport", "StudentAccounting", "Fees Report", ViewOnly: true),

        // Office accounting
        new("OfficeAccounting.VoucherHead", "OfficeAccounting", "Voucher Head"),
        new("OfficeAccounting.Account", "OfficeAccounting", "Account"),
        new("OfficeAccounting.Deposit", "OfficeAccounting", "Deposit"),
        new("OfficeAccounting.Expense", "OfficeAccounting", "Expense"),
        new("OfficeAccounting.Transactions", "OfficeAccounting", "Transactions / Ledger", ViewOnly: true),

        // Settings
        new("Settings.RolesPermissions", "Settings", "Roles & Permissions"),
        new("Settings.AcademicSessions", "Settings", "Academic Sessions"),
        new("Settings.SchoolSettings", "Settings", "School Settings"),
        new("Settings.Cron", "Settings", "Cron Jobs"),
        new("Settings.DatabaseBackup", "Settings", "Database Backup"),
        new("Settings.LoginLog", "Settings", "User Login Log", ViewOnly: true),
        new("Settings.EmailSettings", "Settings", "Email Settings"),
        new("Settings.SmsSettings", "Settings", "SMS Settings"),

        // Alumni
        new("Alumni.AlumniList", "Alumni", "Alumni List"),
        new("Alumni.AlumniEvent", "Alumni", "Alumni Event"),

        // Multi class (bulk operations across classes)
        new("MultiClass.MultiClassStudent", "MultiClass", "Multi Class Student"),
        new("MultiClass.MultiClassFees", "MultiClass", "Multi Class Fees"),

        // Biometric
        new("Biometric.Devices", "Biometric", "Biometric Devices"),
        new("Biometric.UserMaps", "Biometric", "Biometric PIN Maps"),
        new("Biometric.PunchLogs", "Biometric", "Biometric Punch Logs", ViewOnly: true),

        // Messages
        new("Messages.Inbox", "Messages", "Inbox", ViewOnly: true),
        new("Messages.Compose", "Messages", "Compose Message"),
    ];

    public static readonly IReadOnlyDictionary<string, FeatureDef> ByKey =
        All.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);

    public static bool IsValidKey(string key) => ByKey.ContainsKey(key);
}
