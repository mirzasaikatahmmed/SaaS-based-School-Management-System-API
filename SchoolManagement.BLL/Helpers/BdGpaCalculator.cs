using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Helpers;

/// <summary>
/// Bangladesh board GPA: main subjects average + additional subject contributes max(0, GP − 2).
/// Matches SSC transcript: GPA without additional, and GPA with "GP Above 2".
/// </summary>
public static class BdGpaCalculator
{
    public class SubjectGp
    {
        public Guid SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal GradePoint { get; set; }
        public bool IsContinuousAssessment { get; set; }
    }

    public class Result
    {
        public decimal GpaWithoutAdditional { get; set; }
        public decimal Gpa { get; set; }
        public decimal AdditionalGpAbove2 { get; set; }
        public int MainSubjectCount { get; set; }
        public decimal MainGpSum { get; set; }
        public Guid? AdditionalSubjectId { get; set; }
    }

    public static Result Calculate(IEnumerable<SubjectGp> subjects, Guid? additionalSubjectId)
    {
        var list = subjects.Where(s => !s.IsContinuousAssessment).ToList();
        SubjectGp? additional = null;
        if (additionalSubjectId.HasValue)
            additional = list.FirstOrDefault(s => s.SubjectId == additionalSubjectId.Value);

        var main = additional is null
            ? list
            : list.Where(s => s.SubjectId != additional.SubjectId).ToList();

        var mainSum = main.Sum(s => s.GradePoint);
        var n = main.Count;
        var without = n == 0 ? 0 : Math.Round(mainSum / n, 2);
        var above2 = additional is null
            ? 0
            : Math.Max(0, additional.GradePoint - BdGpaRules.AdditionalSubjectBaseGp);
        var with = n == 0 ? 0 : Math.Round((mainSum + above2) / n, 2);

        return new Result
        {
            GpaWithoutAdditional = without,
            Gpa = with,
            AdditionalGpAbove2 = above2,
            MainSubjectCount = n,
            MainGpSum = mainSum,
            AdditionalSubjectId = additional?.SubjectId
        };
    }
}
