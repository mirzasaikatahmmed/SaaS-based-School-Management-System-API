using FluentValidation;
using SchoolManagement.BLL.DTOs.AdvanceSalary;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class CreateAdvanceSalaryValidator : AbstractValidator<CreateAdvanceSalaryDto>
{
    public CreateAdvanceSalaryValidator()
    {
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => EmployeeRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid employee role.");
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.DeductMonth).NotEmpty()
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
            .WithMessage("DeductMonth must be in YYYY-MM format.");
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CreateMyAdvanceSalaryValidator : AbstractValidator<CreateMyAdvanceSalaryDto>
{
    public CreateMyAdvanceSalaryValidator()
    {
        RuleFor(x => x.DeductMonth).NotEmpty()
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
            .WithMessage("DeductMonth must be in YYYY-MM format.");
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
