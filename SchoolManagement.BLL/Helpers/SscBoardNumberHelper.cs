using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Helpers;

/// <summary>SSC board roll/registration apply only to class 9 and 10.</summary>
public static class SscBoardNumberHelper
{
    public static bool IsSscEligibleClass(ClassEntity? clazz)
    {
        if (clazz is null) return false;
        if (clazz.NumericName is 9 or 10) return true;
        return IsSscEligibleClassName(clazz.Name);
    }

    public static bool IsSscEligibleClassName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var n = name.Trim().ToLowerInvariant().Replace(" ", "").Replace("-", "");
        return n is "9" or "ix" or "class9" or "classix" or "nine"
            or "10" or "x" or "class10" or "classx" or "ten";
    }

    public static string? NormalizeOptional(string? value)
    {
        if (value is null) return null;
        var trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
