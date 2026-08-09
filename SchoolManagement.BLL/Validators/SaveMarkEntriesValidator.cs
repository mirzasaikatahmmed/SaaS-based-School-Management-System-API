using FluentValidation;
using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Validators;

public class SaveMarkEntriesValidator : AbstractValidator<SaveMarkEntriesDto>
{
    public SaveMarkEntriesValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.Marks).NotNull().NotEmpty();
        RuleForEach(x => x.Marks).ChildRules(m =>
        {
            m.RuleFor(x => x.StudentId).NotEmpty();
            m.RuleFor(x => x.WrittenMark).GreaterThanOrEqualTo(0).When(x => !x.IsAbsent && x.WrittenMark.HasValue);
            m.RuleFor(x => x.McqMark).GreaterThanOrEqualTo(0).When(x => !x.IsAbsent && x.McqMark.HasValue);
        });
    }
}

public class MarkEntryFilterValidator : AbstractValidator<MarkEntryFilterDto>
{
    public MarkEntryFilterValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.SubjectId).NotEmpty();
    }
}
