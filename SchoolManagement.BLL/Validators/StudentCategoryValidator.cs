using FluentValidation;
using SchoolManagement.BLL.DTOs.StudentCategory;

namespace SchoolManagement.BLL.Validators;

public class CreateStudentCategoryValidator : AbstractValidator<CreateStudentCategoryDto>
{
    public CreateStudentCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);
    }
}

public class UpdateStudentCategoryValidator : AbstractValidator<UpdateStudentCategoryDto>
{
    public UpdateStudentCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);
    }
}
