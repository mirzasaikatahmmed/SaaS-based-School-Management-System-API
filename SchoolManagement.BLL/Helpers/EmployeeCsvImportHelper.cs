using System.Text;
using SchoolManagement.BLL.Exceptions;

namespace SchoolManagement.BLL.Helpers;

public static class EmployeeCsvImportHelper
{
    public static readonly string[] Headers =
    [
        "Name", "Role", "JoiningDate", "DesignationName", "DepartmentName", "Qualification",
        "ExperienceDetails", "TotalExperience", "Gender", "Religion", "BloodGroup", "DateOfBirth",
        "MobileNo", "Email", "PresentAddress", "PermanentAddress", "NidNumber", "Username", "Password",
        "FacebookUrl", "TwitterUrl", "LinkedInUrl", "SkipBankDetails", "BankName", "HolderName",
        "BankBranch", "BankAddress", "IfscCode", "AccountNo"
    ];

    public static byte[] BuildSampleCsv() =>
        Encoding.UTF8.GetBytes(string.Join(',', Headers) +
            "\nJane Doe,Teacher,2026-01-01,ASSISTANT TEACHER,MATHEMATICS,B.Sc,,,,,Female,,,,01700000000,jane@example.com,Dhaka,,,jane.doe,Password123,,,,true,,,,,\n");

    public static List<Dictionary<string, string>> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var first = reader.ReadLine();
        if (first is null) throw new AppException("CSV is empty.", 400);

        var headers = Split(first);
        if (!headers.Contains("Name", StringComparer.OrdinalIgnoreCase) ||
            !headers.Contains("Email", StringComparer.OrdinalIgnoreCase))
            throw new AppException("CSV must contain Name and Email headers.", 400);

        var rows = new List<Dictionary<string, string>>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = Split(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Count; i++)
                row[headers[i]] = i < values.Count ? values[i] : string.Empty;
            rows.Add(row);
        }

        return rows;
    }

    private static List<string> Split(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else quoted = !quoted;
            }
            else if (line[i] == ',' && !quoted)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else sb.Append(line[i]);
        }

        result.Add(sb.ToString().Trim());
        return result;
    }
}
