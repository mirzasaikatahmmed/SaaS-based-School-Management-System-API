using FluentValidation;
using SchoolManagement.BLL.DTOs.Academic;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class CreateClassValidator : AbstractValidator<CreateClassDto>
{
    public CreateClassValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NumericName).GreaterThanOrEqualTo(0).When(x => x.NumericName.HasValue);
    }
}

public class UpdateClassValidator : AbstractValidator<UpdateClassDto>
{
    public UpdateClassValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NumericName).GreaterThanOrEqualTo(0).When(x => x.NumericName.HasValue);
    }
}

public class CreateSectionValidator : AbstractValidator<CreateSectionDto>
{
    public CreateSectionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}

public class UpdateSectionValidator : AbstractValidator<UpdateSectionDto>
{
    public UpdateSectionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
    }
}

public class UpsertClassTeacherValidator : AbstractValidator<UpsertClassTeacherDto>
{
    public UpsertClassTeacherValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public class CreateSubjectValidator : AbstractValidator<CreateSubjectDto>
{
    public CreateSubjectValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Author).MaximumLength(200);
        RuleFor(x => x.SubjectType).NotEmpty()
            .Must(t => SubjectTypes.All.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid subject type.");
    }
}

public class UpdateSubjectValidator : AbstractValidator<UpdateSubjectDto>
{
    public UpdateSubjectValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Author).MaximumLength(200);
        RuleFor(x => x.SubjectType).NotEmpty()
            .Must(t => SubjectTypes.All.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid subject type.");
    }
}

public class UpsertClassSubjectAssignmentValidator : AbstractValidator<UpsertClassSubjectAssignmentDto>
{
    public UpsertClassSubjectAssignmentValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.SubjectIds).NotEmpty().WithMessage("At least one subject must be provided.");
    }
}

public class SchedulePeriodValidator : AbstractValidator<SchedulePeriodDto>
{
    public SchedulePeriodValidator()
    {
        RuleFor(x => x.EndingTime).GreaterThan(x => x.StartingTime)
            .WithMessage("Period ending time must be after starting time.");
        RuleFor(x => x.SubjectId).NotEmpty().When(x => !x.IsBreak)
            .WithMessage("SubjectId is required for non-break periods.");
        RuleFor(x => x.ClassRoom).MaximumLength(100);
    }
}

public class UpsertClassScheduleValidator : AbstractValidator<UpsertClassScheduleDto>
{
    public UpsertClassScheduleValidator()
    {
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();
        RuleFor(x => x.Day).NotEmpty()
            .Must(d => WeekDays.All.Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid day of week.");
        RuleForEach(x => x.Periods).SetValidator(new SchedulePeriodValidator());
    }
}

public class ProcessPromotionItemValidator : AbstractValidator<ProcessPromotionItemDto>
{
    public ProcessPromotionItemValidator()
    {
        RuleFor(x => x.StudentId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => PromotionStatuses.All.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid promotion status.");
        RuleFor(x => x.ToClassId).NotEmpty()
            .When(x => x.Status.Equals(PromotionStatuses.Promoted, StringComparison.OrdinalIgnoreCase))
            .WithMessage("ToClassId is required for Promoted status.");
        RuleFor(x => x.ToSectionId).NotEmpty()
            .When(x => x.Status.Equals(PromotionStatuses.Promoted, StringComparison.OrdinalIgnoreCase))
            .WithMessage("ToSectionId is required for Promoted status.");
        RuleFor(x => x.ToAcademicYear).NotEmpty()
            .When(x => x.Status.Equals(PromotionStatuses.Promoted, StringComparison.OrdinalIgnoreCase) ||
                       x.Status.Equals(PromotionStatuses.Running, StringComparison.OrdinalIgnoreCase))
            .WithMessage("ToAcademicYear is required for Promoted or Running status.");
    }
}

public class ProcessPromotionValidator : AbstractValidator<ProcessPromotionDto>
{
    public ProcessPromotionValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("No students provided for promotion.");
        RuleForEach(x => x.Items).SetValidator(new ProcessPromotionItemValidator());
    }
}
