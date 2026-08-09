using FluentValidation;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.DAL.Entities.Tenant;
namespace SchoolManagement.BLL.Validators;
public class AddEmployeeValidator : AbstractValidator<AddEmployeeDto>
{
 public AddEmployeeValidator()
 {
  RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);
  RuleFor(x=>x.Email).NotEmpty().EmailAddress();
  RuleFor(x=>x.MobileNo).NotEmpty().MaximumLength(20);
  RuleFor(x=>x.Username).NotEmpty().MaximumLength(100);
  RuleFor(x=>x.PresentAddress).NotEmpty();
  RuleFor(x=>x.Qualification).NotEmpty();
  RuleFor(x=>x.JoiningDate).NotEmpty();
  RuleFor(x=>x.DepartmentId).NotEmpty().WithMessage("DepartmentId is required.");
  RuleFor(x=>x.DesignationId).NotEmpty().WithMessage("DesignationId is required.");
  RuleFor(x=>x.Role).Must(x=>EmployeeRoles.All.Contains(x,StringComparer.OrdinalIgnoreCase)).WithMessage("Invalid employee role.");
  RuleFor(x=>x.Password).NotEmpty().MinimumLength(6);
  RuleFor(x=>x.RetypePassword).Equal(x=>x.Password).WithMessage("Passwords do not match");
  RuleFor(x=>x.BankName).NotEmpty().When(x=>!x.SkipBankDetails);
  RuleFor(x=>x.HolderName).NotEmpty().When(x=>!x.SkipBankDetails);
  RuleFor(x=>x.BankBranch).NotEmpty().When(x=>!x.SkipBankDetails);
  RuleFor(x=>x.AccountNo).NotEmpty().When(x=>!x.SkipBankDetails);
  RuleFor(x=>x.FacebookUrl).Must(Url).WithMessage("Invalid Facebook URL");
  RuleFor(x=>x.TwitterUrl).Must(Url).WithMessage("Invalid Twitter URL");
  RuleFor(x=>x.LinkedInUrl).Must(Url).WithMessage("Invalid LinkedIn URL");
 }
 private static bool Url(string? x)=>string.IsNullOrWhiteSpace(x)||Uri.IsWellFormedUriString(x,UriKind.Absolute);
}
public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>{public UpdateEmployeeValidator(){RuleFor(x=>x.Name).NotEmpty().MaximumLength(200);RuleFor(x=>x.Email).NotEmpty().EmailAddress();RuleFor(x=>x.MobileNo).NotEmpty().MaximumLength(20);RuleFor(x=>x.Role).Must(x=>EmployeeRoles.All.Contains(x,StringComparer.OrdinalIgnoreCase));RuleFor(x=>x.BankName).NotEmpty().When(x=>!x.SkipBankDetails);RuleFor(x=>x.HolderName).NotEmpty().When(x=>!x.SkipBankDetails);RuleFor(x=>x.BankBranch).NotEmpty().When(x=>!x.SkipBankDetails);RuleFor(x=>x.AccountNo).NotEmpty().When(x=>!x.SkipBankDetails);}}
