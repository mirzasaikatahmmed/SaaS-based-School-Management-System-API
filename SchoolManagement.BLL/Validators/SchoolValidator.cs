using FluentValidation;
using SchoolManagement.BLL.DTOs.School;

namespace SchoolManagement.BLL.Validators;

public class CreateSchoolValidator : AbstractValidator<CreateSchoolDto>
{
    private static readonly HashSet<string> AllowedSchoolTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Primary", "High School", "College", "University"
    };

    public CreateSchoolValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MaximumLength(100)
            .Matches(@"^[a-z0-9-]+$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only (e.g. greenwood-high).");

        RuleFor(x => x.Domain).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Domain));
        RuleFor(x => x.Phone).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Website).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Website));
        RuleFor(x => x.City).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.City));
        RuleFor(x => x.State).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.State));
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.Currency).MaximumLength(10);
        RuleFor(x => x.MaxUsers).GreaterThan(0);

        RuleFor(x => x.SchoolType)
            .Must(t => string.IsNullOrWhiteSpace(t) || AllowedSchoolTypes.Contains(t))
            .WithMessage("SchoolType must be one of: Primary, High School, College, University.");

        RuleFor(x => x.EstablishedYear)
            .InclusiveBetween(1800, DateTime.UtcNow.Year + 1)
            .When(x => x.EstablishedYear.HasValue);

        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.AdminFirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AdminLastName).NotEmpty().MaximumLength(100);
    }
}

public class UpdateSchoolValidator : AbstractValidator<UpdateSchoolDto>
{
    private static readonly HashSet<string> AllowedSchoolTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Primary", "High School", "College", "University"
    };

    public UpdateSchoolValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Name));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.MaxUsers).GreaterThan(0).When(x => x.MaxUsers.HasValue);
        RuleFor(x => x.SchoolType)
            .Must(t => string.IsNullOrWhiteSpace(t) || AllowedSchoolTypes.Contains(t))
            .When(x => !string.IsNullOrEmpty(x.SchoolType));
    }
}

public class SchoolSettingsValidator : AbstractValidator<SchoolSettingsDto>
{
    public SchoolSettingsValidator()
    {
        RuleFor(x => x.Features.MaxUsers).GreaterThan(0);
        RuleFor(x => x.Features.StorageQuotaGB).GreaterThan(0);
        RuleFor(x => x.Security.PasswordMinLength).GreaterThanOrEqualTo(6);
        RuleFor(x => x.Security.MaxLoginAttempts).GreaterThan(0);
    }
}
