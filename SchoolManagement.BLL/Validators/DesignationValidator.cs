using FluentValidation;
using SchoolManagement.BLL.DTOs.Employee;
namespace SchoolManagement.BLL.Validators;
public class DesignationValidator : AbstractValidator<CreateDesignationDto> { public DesignationValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);} }
public class UpdateDesignationValidator : AbstractValidator<UpdateDesignationDto> { public UpdateDesignationValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);} }
