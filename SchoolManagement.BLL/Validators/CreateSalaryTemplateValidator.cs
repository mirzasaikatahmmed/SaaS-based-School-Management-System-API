using FluentValidation;
using SchoolManagement.BLL.DTOs.Payroll;

namespace SchoolManagement.BLL.Validators;

public class CreateSalaryTemplateValidator : AbstractValidator<CreateSalaryTemplateDto>
{
    public CreateSalaryTemplateValidator()
    {
        RuleFor(x => x.SalaryGrade).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeRatePerHour).GreaterThanOrEqualTo(0).When(x => x.OvertimeRatePerHour.HasValue);
        RuleForEach(x => x.Allowances).ChildRules(a =>
        {
            a.RuleFor(r => r.Name).NotEmpty().MaximumLength(200);
            a.RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.Deductions).ChildRules(d =>
        {
            d.RuleFor(r => r.Name).NotEmpty().MaximumLength(200);
            d.RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateSalaryTemplateValidator : AbstractValidator<UpdateSalaryTemplateDto>
{
    public UpdateSalaryTemplateValidator()
    {
        RuleFor(x => x.SalaryGrade).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BasicSalary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OvertimeRatePerHour).GreaterThanOrEqualTo(0).When(x => x.OvertimeRatePerHour.HasValue);
        RuleForEach(x => x.Allowances).ChildRules(a =>
        {
            a.RuleFor(r => r.Name).NotEmpty().MaximumLength(200);
            a.RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
        });
        RuleForEach(x => x.Deductions).ChildRules(d =>
        {
            d.RuleFor(r => r.Name).NotEmpty().MaximumLength(200);
            d.RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
        });
    }
}
