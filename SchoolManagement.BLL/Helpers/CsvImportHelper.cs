using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using SchoolManagement.BLL.DTOs.Import;
using SchoolManagement.BLL.Exceptions;

namespace SchoolManagement.BLL.Helpers;

public static class CsvImportHelper
{
    public static readonly string[] ExpectedHeaders =
    [
        "RegisterNo", "Roll", "AcademicYear", "AdmissionDate", "FirstName", "LastName",
        "Gender", "DateOfBirth", "BloodGroup", "Religion", "Caste", "MotherTongue",
        "MobileNo", "Email", "City", "State", "PresentAddress", "PermanentAddress",
        "FathersNidNumber", "MothersNidNumber", "BirthRegistrationNumber",
        "CategoryId", "Username", "Password",
        "GuardianName", "GuardianRelation", "GuardianMobile", "GuardianEmail",
        "FatherName", "MotherName", "GuardianOccupation", "GuardianIncome",
        "GuardianEducation", "GuardianAddress",         "GuardianUsername", "GuardianPassword",
        "TransportRoute", "HostelName", "RoomName",
        "PreviousSchoolName", "PreviousSchoolQualification", "Remarks",
        "SscRoll", "SscRegistrationNo"
    ];

    public const long MaxFileBytes = 5 * 1024 * 1024;

    public static void ValidateFile(string fileName, long fileLength)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Only .csv files are allowed.", 400);

        if (fileLength <= 0)
            throw new AppException("CSV file is empty.", 400);

        if (fileLength > MaxFileBytes)
            throw new AppException("CSV file must be 5MB or smaller.", 400);
    }

    public static List<StudentImportRowDto> ParseRows(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        };

        using var csv = new CsvReader(reader, config);
        if (!csv.Read() || !csv.ReadHeader())
            throw new AppException("CSV file has no header row.", 400);

        var headers = csv.HeaderRecord ?? Array.Empty<string>();
        if (headers.Length != ExpectedHeaders.Length ||
            !headers.Select(h => h.Trim()).SequenceEqual(ExpectedHeaders, StringComparer.Ordinal))
        {
            throw new AppException(
                "CSV headers do not match the expected template. Download the sample CSV and use those exact column names.",
                400);
        }

        var rows = new List<StudentImportRowDto>();
        var rowNumber = 1; // header is row 1; first data row is 2
        while (csv.Read())
        {
            rowNumber++;
            var raw = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in ExpectedHeaders)
                raw[header] = csv.GetField(header)?.Trim() ?? string.Empty;

            // Skip completely empty rows
            if (raw.Values.All(string.IsNullOrWhiteSpace))
                continue;

            rows.Add(new StudentImportRowDto
            {
                RowNumber = rowNumber,
                RawData = raw,
                RegisterNo = Get(raw, "RegisterNo"),
                Roll = Get(raw, "Roll"),
                AcademicYear = Get(raw, "AcademicYear"),
                AdmissionDate = Get(raw, "AdmissionDate"),
                FirstName = Get(raw, "FirstName"),
                LastName = Get(raw, "LastName"),
                Gender = Get(raw, "Gender"),
                DateOfBirth = Get(raw, "DateOfBirth"),
                BloodGroup = Get(raw, "BloodGroup"),
                Religion = Get(raw, "Religion"),
                Caste = Get(raw, "Caste"),
                MotherTongue = Get(raw, "MotherTongue"),
                MobileNo = Get(raw, "MobileNo"),
                Email = Get(raw, "Email"),
                City = Get(raw, "City"),
                State = Get(raw, "State"),
                PresentAddress = Get(raw, "PresentAddress"),
                PermanentAddress = Get(raw, "PermanentAddress"),
                FathersNidNumber = Get(raw, "FathersNidNumber"),
                MothersNidNumber = Get(raw, "MothersNidNumber"),
                BirthRegistrationNumber = Get(raw, "BirthRegistrationNumber"),
                CategoryId = Get(raw, "CategoryId"),
                Username = Get(raw, "Username"),
                Password = Get(raw, "Password"),
                GuardianName = Get(raw, "GuardianName"),
                GuardianRelation = Get(raw, "GuardianRelation"),
                GuardianMobile = Get(raw, "GuardianMobile"),
                GuardianEmail = Get(raw, "GuardianEmail"),
                FatherName = Get(raw, "FatherName"),
                MotherName = Get(raw, "MotherName"),
                GuardianOccupation = Get(raw, "GuardianOccupation"),
                GuardianIncome = Get(raw, "GuardianIncome"),
                GuardianEducation = Get(raw, "GuardianEducation"),
                GuardianAddress = Get(raw, "GuardianAddress"),
                GuardianUsername = Get(raw, "GuardianUsername"),
                GuardianPassword = Get(raw, "GuardianPassword"),
                TransportRoute = Get(raw, "TransportRoute"),
                HostelName = Get(raw, "HostelName"),
                RoomName = Get(raw, "RoomName"),
                PreviousSchoolName = Get(raw, "PreviousSchoolName"),
                PreviousSchoolQualification = Get(raw, "PreviousSchoolQualification"),
                Remarks = Get(raw, "Remarks"),
                SscRoll = Get(raw, "SscRoll"),
                SscRegistrationNo = Get(raw, "SscRegistrationNo"),
            });
        }

        if (rows.Count == 0)
            throw new AppException("CSV file contains no data rows.", 400);

        return rows;
    }

    public static byte[] BuildSampleCsv()
    {
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(BuildSampleBody())).ToArray();
    }

    private static string BuildSampleBody()
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', ExpectedHeaders));

        // Row 1 — password blank (auto-generated); guardian created from columns
        var row1 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RegisterNo"] = "2026-00001",
            ["Roll"] = "1",
            ["AcademicYear"] = "2026",
            ["AdmissionDate"] = "2026-08-09",
            ["FirstName"] = "John",
            ["LastName"] = "Doe",
            ["Gender"] = "Male",
            ["DateOfBirth"] = "2010-05-15",
            ["BloodGroup"] = "A+",
            ["Religion"] = "Islam",
            ["Caste"] = "",
            ["MotherTongue"] = "Bengali",
            ["MobileNo"] = "01711000001",
            ["Email"] = "john@email.com",
            ["City"] = "Dhaka",
            ["State"] = "Dhaka",
            ["PresentAddress"] = "123 Main St",
            ["PermanentAddress"] = "123 Main St",
            ["FathersNidNumber"] = "",
            ["MothersNidNumber"] = "",
            ["BirthRegistrationNumber"] = "123456789",
            ["CategoryId"] = "",
            ["Username"] = "john.doe",
            ["Password"] = "",
            ["GuardianName"] = "Abdul Doe",
            ["GuardianRelation"] = "Father",
            ["GuardianMobile"] = "01711000002",
            ["GuardianEmail"] = "",
            ["FatherName"] = "Abdul Doe",
            ["MotherName"] = "Fatema Doe",
            ["GuardianOccupation"] = "Business",
            ["GuardianIncome"] = "50000",
            ["GuardianEducation"] = "HSC",
            ["GuardianAddress"] = "123 Main St",
            ["GuardianUsername"] = "",
            ["GuardianPassword"] = "",
            ["TransportRoute"] = "",
            ["HostelName"] = "",
            ["RoomName"] = "",
            ["PreviousSchoolName"] = "",
            ["PreviousSchoolQualification"] = "PSC",
            ["Remarks"] = "",
            ["SscRoll"] = "",
            ["SscRegistrationNo"] = ""
        };

        // Row 2 — guardian reuse via GuardianUsername
        var row2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RegisterNo"] = "2026-00002",
            ["Roll"] = "2",
            ["AcademicYear"] = "2026",
            ["AdmissionDate"] = "2026-08-09",
            ["FirstName"] = "Jane",
            ["LastName"] = "Smith",
            ["Gender"] = "Female",
            ["DateOfBirth"] = "2011-03-20",
            ["BloodGroup"] = "B+",
            ["Religion"] = "Hindu",
            ["Caste"] = "",
            ["MotherTongue"] = "Bengali",
            ["MobileNo"] = "01711000003",
            ["Email"] = "",
            ["City"] = "",
            ["State"] = "",
            ["PresentAddress"] = "",
            ["PermanentAddress"] = "",
            ["FathersNidNumber"] = "",
            ["MothersNidNumber"] = "",
            ["BirthRegistrationNumber"] = "",
            ["CategoryId"] = "",
            ["Username"] = "jane.smith",
            ["Password"] = "Pass@1234",
            ["GuardianName"] = "",
            ["GuardianRelation"] = "",
            ["GuardianMobile"] = "",
            ["GuardianEmail"] = "",
            ["FatherName"] = "",
            ["MotherName"] = "",
            ["GuardianOccupation"] = "",
            ["GuardianIncome"] = "",
            ["GuardianEducation"] = "",
            ["GuardianAddress"] = "",
            ["GuardianUsername"] = "guardian.existing",
            ["GuardianPassword"] = "",
            ["TransportRoute"] = "",
            ["HostelName"] = "",
            ["RoomName"] = "",
            ["PreviousSchoolName"] = "",
            ["PreviousSchoolQualification"] = "JSC",
            ["Remarks"] = "Good student",
            ["SscRoll"] = "",
            ["SscRegistrationNo"] = ""
        };

        sb.AppendLine(string.Join(',', ExpectedHeaders.Select(h => CsvEscape(row1[h]))));
        sb.AppendLine(string.Join(',', ExpectedHeaders.Select(h => CsvEscape(row2[h]))));
        return sb.ToString();
    }

    public static byte[] BuildFailedRowsCsv(IEnumerable<ImportRowResultDto> failedRows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', ExpectedHeaders.Concat(["ErrorMessage"])));
        foreach (var row in failedRows)
        {
            var cells = ExpectedHeaders.Select(h =>
                CsvEscape(row.RawData.TryGetValue(h, out var v) ? v : string.Empty));
            sb.AppendLine(string.Join(',', cells.Concat([CsvEscape(row.ErrorMessage ?? "")])));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public static string GenerateUsername(string firstName, string? lastName, string registerNo)
    {
        var first = Sanitize(firstName);
        var last = Sanitize(lastName ?? string.Empty);
        var baseName = string.IsNullOrEmpty(last) ? first : $"{first}.{last}";
        return $"{baseName}.{registerNo}".ToLowerInvariant();
    }

    public static string GenerateDefaultPassword(string registerNo) => $"Student@{registerNo}";

    private static string Sanitize(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .ToArray();
        return chars.Length == 0 ? "user" : new string(chars);
    }

    private static string? Get(Dictionary<string, string> raw, string key)
        => raw.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;

    private static string CsvEscape(string? value)
    {
        value ??= string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
