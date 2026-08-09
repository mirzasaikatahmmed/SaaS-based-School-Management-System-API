using FluentValidation;
using SchoolManagement.BLL.DTOs.ExamMaster;

namespace SchoolManagement.BLL.Validators;

public class CreateExamValidator : AbstractValidator<CreateExamDto>
{
    public CreateExamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExamType).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.ExamType));
        RuleFor(x => x.Remarks).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Remarks));
        RuleFor(x => x.MarkDistributionIds).NotNull();
    }
}

public class UpdateExamValidator : AbstractValidator<UpdateExamDto>
{
    public UpdateExamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ExamType).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.ExamType));
        RuleFor(x => x.Remarks).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Remarks));
        RuleFor(x => x.MarkDistributionIds).NotNull();
    }
}
