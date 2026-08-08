using FluentValidation;
using SchoolManagement.BLL.DTOs.Parents;

namespace SchoolManagement.BLL.Validators;

public class AddParentValidator : AbstractValidator<AddParentDto>
{
    public AddParentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Relation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Occupation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.RetypePassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FacebookUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid Facebook URL");
        RuleFor(x => x.TwitterUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid Twitter URL");
        RuleFor(x => x.LinkedInUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid LinkedIn URL");
    }
}

public class UpdateParentValidator : AbstractValidator<UpdateParentDto>
{
    public UpdateParentValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MobileNo).MaximumLength(20).When(x => x.MobileNo is not null);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.FacebookUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid Facebook URL");
        RuleFor(x => x.TwitterUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid Twitter URL");
        RuleFor(x => x.LinkedInUrl)
            .Must(url => url == null || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("Invalid LinkedIn URL");
    }
}
