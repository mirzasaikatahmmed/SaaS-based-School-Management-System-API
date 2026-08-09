using System.Text.RegularExpressions;

namespace SchoolManagement.BLL.Helpers;

public static class AccountingHelpers
{
    public static string Slugify(string value)
    {
        var lower = value.Trim().ToLowerInvariant();
        var hyphenated = Regex.Replace(lower, @"[^a-z0-9]+", "-").Trim('-');
        return Regex.Replace(hyphenated, "-{2,}", "-");
    }

    public static string GenerateTrxId()
        => $"TRX{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(0, 10000):D4}";
}
