using FluentValidation;
using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Validators;

public class CreateExamScheduleValidator : AbstractValidator<CreateExamScheduleDto>
{
    public CreateExamScheduleValidator()
    {
        RuleFor(x => x.ExamId).NotEmpty();
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleForEach(x => x.Subjects).ChildRules(s =>
        {
            s.RuleFor(x => x.SubjectId).NotEmpty();
            s.RuleFor(x => x.ExamDate).NotEmpty();
            s.RuleFor(x => x).Must(x => x.EndingTime > x.StartingTime)
                .WithMessage("Subject ending time must be after starting time.");
        });
        RuleFor(x => x).Must(x =>
            x.Subjects.Count > 0 ||
            (x.StartingDate.HasValue && x.StartingTime.HasValue && x.ExamDurationMinutes.HasValue))
            .WithMessage("Provide subject rows or StartingDate, StartingTime, and ExamDurationMinutes.");
        RuleFor(x => x.ExamDurationMinutes).GreaterThan(0)
            .When(x => x.Subjects.Count == 0 && x.ExamDurationMinutes.HasValue);
    }
}
