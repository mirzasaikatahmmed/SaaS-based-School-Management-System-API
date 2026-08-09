using FluentValidation;
using SchoolManagement.BLL.DTOs.Library;

namespace SchoolManagement.BLL.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}

public class UpdateBookValidator : AbstractValidator<UpdateBookDto>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}

public class CreateBookCategoryValidator : AbstractValidator<CreateBookCategoryDto>
{
    public CreateBookCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class IssueBookValidator : AbstractValidator<IssueBookDto>
{
    public IssueBookValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty();
        RuleFor(x => x.DateOfExpiry).NotEmpty();
    }
}
