using FluentValidation;
using SchoolManagement.BLL.DTOs.Employee;
namespace SchoolManagement.BLL.Validators;
public class DepartmentValidator : AbstractValidator<CreateDepartmentDto> { public DepartmentValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);} }
public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentDto> { public UpdateDepartmentValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);} }
