using FluentValidation;
using SchoolManagement.BLL.DTOs.Auth;
using SchoolManagement.Common.Constants;

namespace SchoolManagement.BLL.Validators;

public class LoginValidator : AbstractValidator<LoginRequestDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class RegisterValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Username may only contain letters, numbers, dots, underscores, and hyphens.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(255);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(255);

        RuleFor(x => x.Mobileno)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Mobileno));

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => AllowedRoles.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage($"Invalid role. Allowed: {string.Join(", ", AllowedRoles)}");
    }

    private static readonly HashSet<string> AllowedRoles =
    [
        AppConstants.Roles.Admin,
        AppConstants.Roles.Teacher,
        AppConstants.Roles.Accountant,
        AppConstants.Roles.Librarian,
        AppConstants.Roles.Parent,
        AppConstants.Roles.Student,
        AppConstants.Roles.Receptionist,
        AppConstants.Roles.Staff
    ];
}

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RevokeTokenValidator : AbstractValidator<RevokeTokenRequestDto>
{
    public RevokeTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Mobileno)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Mobileno));
    }
}
