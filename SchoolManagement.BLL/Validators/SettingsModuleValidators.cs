using FluentValidation;
using SchoolManagement.BLL.DTOs.Settings;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Entities.Tenant;

namespace SchoolManagement.BLL.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name).MaximumLength(50).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(255);
    }
}

public class UpdateRolePermissionsValidator : AbstractValidator<UpdateRolePermissionsDto>
{
    public UpdateRolePermissionsValidator()
    {
        RuleFor(x => x.Permissions).NotNull();
        RuleForEach(x => x.Permissions).ChildRules(p =>
        {
            p.RuleFor(i => i.FeatureKey).NotEmpty().MaximumLength(150)
                .Must(AppFeatures.IsValidKey).WithMessage("Unknown feature key.");
        });
    }
}

public class CreateAcademicSessionValidator : AbstractValidator<CreateAcademicSessionDto>
{
    public CreateAcademicSessionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
    }
}

public class UpdateAcademicSessionValidator : AbstractValidator<UpdateAcademicSessionDto>
{
    public UpdateAcademicSessionValidator()
    {
        RuleFor(x => x.Name).MaximumLength(50).When(x => x.Name is not null);
    }
}

public class UpdateAttendanceTypeValidator : AbstractValidator<UpdateAttendanceTypeDto>
{
    public UpdateAttendanceTypeValidator()
    {
        RuleFor(x => x.AttendanceType).NotEmpty().Must(AttendanceTypes.IsValid)
            .WithMessage("AttendanceType must be DayWise or SubjectWise.");
    }
}

public class UpdateEmailConfigValidator : AbstractValidator<UpdateEmailConfigDto>
{
    public UpdateEmailConfigValidator()
    {
        RuleFor(x => x.Protocol).Must(EmailProtocols.IsValid);
        RuleFor(x => x.SmtpSecure).Must(SmtpSecureModes.IsValid);
        RuleFor(x => x.SmtpPort).InclusiveBetween(1, 65535);
        RuleFor(x => x.SystemEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.SystemEmail));
    }
}

public class TestEmailValidator : AbstractValidator<TestEmailDto>
{
    public TestEmailValidator() => RuleFor(x => x.To).NotEmpty().EmailAddress();
}

public class TestSmsValidator : AbstractValidator<TestSmsDto>
{
    public TestSmsValidator()
    {
        RuleFor(x => x.To).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Message).MaximumLength(918).When(x => x.Message is not null);
    }
}

public class UpdateEmailTriggerValidator : AbstractValidator<UpdateEmailTriggerDto>
{
    public UpdateEmailTriggerValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BodyHtml).NotEmpty();
    }
}

public class UpdateSmsConfigValidator : AbstractValidator<UpdateSmsConfigDto>
{
    public UpdateSmsConfigValidator()
    {
        RuleFor(x => x.ActivatedGateway).Must(SmsGateways.IsValid);
    }
}

public class UpdateSmsTriggerValidator : AbstractValidator<UpdateSmsTriggerDto>
{
    public UpdateSmsTriggerValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(918);
    }
}
