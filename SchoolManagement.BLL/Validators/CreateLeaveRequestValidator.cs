using FluentValidation;
using SchoolManagement.BLL.DTOs.Leave;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveCategoryId).NotEmpty();
        RuleFor(x => x.DateOfStart).NotEmpty();
        RuleFor(x => x.DateOfEnd).NotEmpty()
            .Must((dto, end) => end.Date >= dto.DateOfStart.Date)
            .WithMessage("DateOfEnd must be greater than or equal to DateOfStart.");
    }
}

public class AdminCreateLeaveRequestValidator : AbstractValidator<AdminCreateLeaveRequestDto>
{
    public AdminCreateLeaveRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => EmployeeRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.LeaveCategoryId).NotEmpty();
        RuleFor(x => x.DateOfStart).NotEmpty();
        RuleFor(x => x.DateOfEnd).NotEmpty()
            .Must((dto, end) => end.Date >= dto.DateOfStart.Date)
            .WithMessage("DateOfEnd must be greater than or equal to DateOfStart.");
    }
}
