using FluentValidation;
using SchoolManagement.BLL.DTOs.StudentDetails;

namespace SchoolManagement.BLL.Validators;

public class CreateDeactivateReasonValidator : AbstractValidator<CreateDeactivateReasonDto>
{
    public CreateDeactivateReasonValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200);
    }
}

public class UpdateDeactivateReasonValidator : AbstractValidator<UpdateDeactivateReasonDto>
{
    public UpdateDeactivateReasonValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200);
    }
}

public class BulkAuthenticationActivateValidator : AbstractValidator<BulkAuthenticationActivateDto>
{
    public BulkAuthenticationActivateValidator()
    {
        RuleFor(x => x.StudentIds)
            .NotEmpty().WithMessage("At least one student id is required.");
    }
}
