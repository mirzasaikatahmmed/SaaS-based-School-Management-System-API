using FluentValidation;
using SchoolManagement.BLL.DTOs.Award;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class GiveAwardValidator : AbstractValidator<GiveAwardDto>
{
    public GiveAwardValidator()
    {
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => AwardRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid award role.");
        RuleFor(x => x.AwardName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GiftItem).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AwardReason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CashPrice).GreaterThanOrEqualTo(0).When(x => x.CashPrice.HasValue);
        RuleFor(x => x).Must(x =>
            (AwardRoles.IsStudent(x.Role) && x.StudentId.HasValue && !x.EmployeeId.HasValue) ||
            (!AwardRoles.IsStudent(x.Role) && x.EmployeeId.HasValue && !x.StudentId.HasValue))
            .WithMessage("Provide EmployeeId for staff roles or StudentId for Student role");
    }
}

public class UpdateAwardValidator : AbstractValidator<UpdateAwardDto>
{
    public UpdateAwardValidator()
    {
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => AwardRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase));
        RuleFor(x => x.AwardName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GiftItem).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AwardReason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CashPrice).GreaterThanOrEqualTo(0).When(x => x.CashPrice.HasValue);
        RuleFor(x => x.GivenDate).NotEmpty();
        RuleFor(x => x).Must(x =>
            (AwardRoles.IsStudent(x.Role) && x.StudentId.HasValue && !x.EmployeeId.HasValue) ||
            (!AwardRoles.IsStudent(x.Role) && x.EmployeeId.HasValue && !x.StudentId.HasValue))
            .WithMessage("Provide EmployeeId for staff roles or StudentId for Student role");
    }
}
