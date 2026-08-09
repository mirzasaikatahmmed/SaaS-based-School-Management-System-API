using FluentValidation;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class LeaveCategoryValidator : AbstractValidator<CreateLeaveCategoryDto>
{
    public LeaveCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => EmployeeRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid employee role.");
        RuleFor(x => x.Days).GreaterThan(0);
    }
}

public class UpdateLeaveCategoryValidator : AbstractValidator<UpdateLeaveCategoryDto>
{
    public UpdateLeaveCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => EmployeeRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.Days).GreaterThan(0);
    }
}
