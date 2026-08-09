using FluentValidation;
using SchoolManagement.BLL.DTOs.Events;

namespace SchoolManagement.BLL.Validators;

public class CreateEventValidator : AbstractValidator<CreateEventDto>
{
    public CreateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Audience).NotEmpty();
        RuleFor(x => x.DateOfStart).NotEmpty();
        RuleFor(x => x.DateOfEnd).NotEmpty();
        RuleFor(x => x).Must(x => x.DateOfEnd.Date >= x.DateOfStart.Date)
            .WithMessage("DateOfEnd must be on or after DateOfStart.");
    }
}

public class UpdateEventValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Audience).NotEmpty();
        RuleFor(x => x.DateOfStart).NotEmpty();
        RuleFor(x => x.DateOfEnd).NotEmpty();
        RuleFor(x => x).Must(x => x.DateOfEnd.Date >= x.DateOfStart.Date)
            .WithMessage("DateOfEnd must be on or after DateOfStart.");
    }
}

public class CreateEventTypeValidator : AbstractValidator<CreateEventTypeDto>
{
    public CreateEventTypeValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
