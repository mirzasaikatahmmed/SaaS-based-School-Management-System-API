using FluentValidation;
using SchoolManagement.BLL.DTOs.Payroll;

namespace SchoolManagement.BLL.Validators;

public class ProcessPaymentValidator : AbstractValidator<ProcessPaymentDto>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.PaymentMonth)
            .NotEmpty()
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
            .WithMessage("PaymentMonth must be in YYYY-MM format.");
        RuleFor(x => x.OvertimeHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AdvanceDeduction).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentMethod).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.PaymentMethod));
    }
}
