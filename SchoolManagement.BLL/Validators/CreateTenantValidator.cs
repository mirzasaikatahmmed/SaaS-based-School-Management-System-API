using FluentValidation;
using SchoolManagement.BLL.DTOs.Tenant;

namespace SchoolManagement.BLL.Validators;

public class CreateTenantValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with optional hyphens (e.g. greenwood-high).");

        RuleFor(x => x.Domain)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Domain));

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0).WithMessage("Max users must be greater than zero.");

        RuleFor(x => x.Admin)
            .NotNull().WithMessage("Initial school admin is required.");

        When(x => x.Admin is not null, () =>
        {
            RuleFor(x => x.Admin!.Email)
                .NotEmpty().EmailAddress();

            RuleFor(x => x.Admin!.Username)
                .NotEmpty().MinimumLength(3).MaximumLength(100);

            RuleFor(x => x.Admin!.Password)
                .NotEmpty().MinimumLength(8);

            RuleFor(x => x.Admin!.FirstName)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.Admin!.LastName)
                .NotEmpty().MaximumLength(100);
        });
    }
}

public class UpdateTenantSettingsValidator : AbstractValidator<UpdateTenantSettingsDto>
{
    public UpdateTenantSettingsValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Domain)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Domain));

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0)
            .When(x => x.MaxUsers.HasValue);
    }
}
