using FluentValidation;
using SchoolManagement.BLL.DTOs.OnlineAdmission;

namespace SchoolManagement.BLL.Validators;

public class SubmitOnlineAdmissionValidator : AbstractValidator<SubmitOnlineAdmissionDto>
{
    private static readonly HashSet<string> Genders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male", "Female", "Other"
    };

    public SubmitOnlineAdmissionValidator()
    {
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AcademicYear).InclusiveBetween(2000, DateTime.UtcNow.Year + 5);
        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.MobileNo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.GuardianEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.GuardianEmail));
        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrWhiteSpace(g) || Genders.Contains(g))
            .WithMessage("Gender must be Male, Female, or Other.");
    }
}

public class ApproveOnlineAdmissionValidator : AbstractValidator<ApproveAdmissionDto>
{
    public ApproveOnlineAdmissionValidator()
    {
        RuleFor(x => x.AdminUsername).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(8);
    }
}

public class UpdatePaymentStatusValidator : AbstractValidator<UpdatePaymentStatusDto>
{
    public UpdatePaymentStatusValidator()
    {
        RuleFor(x => x.PaymentStatus)
            .NotEmpty()
            .Must(s => s is "Paid" or "Unpaid")
            .WithMessage("PaymentStatus must be Paid or Unpaid.");
    }
}
