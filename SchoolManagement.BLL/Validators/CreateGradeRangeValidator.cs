using FluentValidation;
using SchoolManagement.BLL.DTOs.Marks;

namespace SchoolManagement.BLL.Validators;

public class CreateGradeRangeValidator : AbstractValidator<CreateGradeRangeDto>
{
    public CreateGradeRangeValidator()
    {
        RuleFor(x => x.GradeName).NotEmpty().MaximumLength(20);
        RuleFor(x => x.GradePoint).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x).Must(x => x.MinPercentage <= x.MaxPercentage)
            .WithMessage("MinPercentage must be less than or equal to MaxPercentage.");
    }
}

public class UpdateGradeRangeValidator : AbstractValidator<UpdateGradeRangeDto>
{
    public UpdateGradeRangeValidator()
    {
        RuleFor(x => x.GradeName).NotEmpty().MaximumLength(20);
        RuleFor(x => x.GradePoint).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.MaxPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x).Must(x => x.MinPercentage <= x.MaxPercentage)
            .WithMessage("MinPercentage must be less than or equal to MaxPercentage.");
    }
}
