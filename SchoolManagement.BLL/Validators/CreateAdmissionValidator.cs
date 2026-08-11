using FluentValidation;
using SchoolManagement.BLL.DTOs.Student;

namespace SchoolManagement.BLL.Validators;

public class CreateAdmissionValidator : AbstractValidator<CreateAdmissionDto>
{
    private static readonly HashSet<string> Genders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male", "Female", "Other"
    };

    private static readonly HashSet<string> BloodGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"
    };

    public CreateAdmissionValidator()
    {
        RuleFor(x => x.AcademicYear)
            .InclusiveBetween(2000, DateTime.UtcNow.Year + 5);

        RuleFor(x => x.RegisterNo)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.ClassId).NotEmpty();
        RuleFor(x => x.SectionId).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName is not null);
        RuleFor(x => x.Religion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MobileNo).NotEmpty().MaximumLength(20);

        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrWhiteSpace(g) || Genders.Contains(g))
            .WithMessage("Gender must be Male, Female, or Other.");

        RuleFor(x => x.BloodGroup)
            .Must(b => string.IsNullOrWhiteSpace(b) || BloodGroups.Contains(b))
            .WithMessage("BloodGroup must be a valid type (A+, A-, B+, B-, O+, O-, AB+, AB-).");

        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Optional — only meaningful for class 9/10; never required
        RuleFor(x => x.SscRoll).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.SscRoll));
        RuleFor(x => x.SscRegistrationNo).MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SscRegistrationNo));

        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.RetypePassword)
            .Equal(x => x.Password)
            .WithMessage("Password and RetypePassword must match.");

        When(x => x.GuardianAlreadyExist, () =>
        {
            RuleFor(x => x.ExistingGuardianId)
                .NotEmpty()
                .WithMessage("ExistingGuardianId is required when GuardianAlreadyExist is true.");
        });

        When(x => !x.GuardianAlreadyExist, () =>
        {
            RuleFor(x => x.Guardian).NotNull().WithMessage("Guardian details are required.");
            When(x => x.Guardian is not null, () =>
            {
                RuleFor(x => x.Guardian!.Name).NotEmpty().MaximumLength(200);
                RuleFor(x => x.Guardian!.Relation).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Guardian!.MobileNo).NotEmpty().MaximumLength(20);
                RuleFor(x => x.Guardian!.Email).EmailAddress()
                    .When(x => !string.IsNullOrWhiteSpace(x.Guardian!.Email));

                When(x => !string.IsNullOrWhiteSpace(x.Guardian!.Password) ||
                          !string.IsNullOrWhiteSpace(x.Guardian!.Username), () =>
                {
                    RuleFor(x => x.Guardian!.Username).NotEmpty();
                    RuleFor(x => x.Guardian!.Password).NotEmpty().MinimumLength(8);
                    RuleFor(x => x.Guardian!.RetypePassword)
                        .Equal(x => x.Guardian!.Password)
                        .WithMessage("Guardian Password and RetypePassword must match.");
                });
            });
        });

        When(x => x.RoomId.HasValue, () =>
        {
            RuleFor(x => x.HostelId)
                .NotEmpty()
                .WithMessage("HostelId is required when RoomId is provided.");
        });
    }
}

public class UpdateAdmissionValidator : AbstractValidator<UpdateAdmissionDto>
{
    private static readonly HashSet<string> Genders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Male", "Female", "Other"
    };

    private static readonly HashSet<string> BloodGroups = new(StringComparer.OrdinalIgnoreCase)
    {
        "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"
    };

    public UpdateAdmissionValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => x.FirstName is not null);
        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrWhiteSpace(g) || Genders.Contains(g))
            .When(x => x.Gender is not null);
        RuleFor(x => x.BloodGroup)
            .Must(b => string.IsNullOrWhiteSpace(b) || BloodGroups.Contains(b))
            .When(x => x.BloodGroup is not null);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));

        // Optional — only meaningful for class 9/10; never required
        RuleFor(x => x.SscRoll).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.SscRoll));
        RuleFor(x => x.SscRegistrationNo).MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SscRegistrationNo));
    }
}
