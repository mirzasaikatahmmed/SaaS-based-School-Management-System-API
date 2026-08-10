using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SchoolManagement.API.Filters;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.BLL.Services;
using SchoolManagement.BLL.Settings;
using SchoolManagement.BLL.Validators;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Repositories.Implementations;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<MinioSettings>(configuration.GetSection("MinIO"));
        services.Configure<SuperAdminSettings>(configuration.GetSection("SuperAdmin"));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
        services.AddScoped<ITenantSchemaProvisioner, TenantSchemaProvisioner>();

        services.AddDbContext<MasterDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("MasterDb"), npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                npgsql.MigrationsAssembly(typeof(MasterDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<JwtHelper>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IOnlineAdmissionService, OnlineAdmissionService>();
        services.AddScoped<IStudentImportService, StudentImportService>();
        services.AddScoped<IStudentCategoryService, StudentCategoryService>();
        services.AddScoped<IStudentListService, StudentListService>();
        services.AddScoped<IDeactivateReasonService, DeactivateReasonService>();
        services.AddScoped<ILoginDeactivateService, LoginDeactivateService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<IParentLoginDeactivateService, ParentLoginDeactivateService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeImportService, EmployeeImportService>();
        services.AddScoped<ISalaryTemplateService, SalaryTemplateService>();
        services.AddScoped<ISalaryAssignService, SalaryAssignService>();
        services.AddScoped<ISalaryPaymentService, SalaryPaymentService>();
        services.AddScoped<IAdvanceSalaryService, AdvanceSalaryService>();
        services.AddScoped<ILeaveCategoryService, LeaveCategoryService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<IAwardService, AwardService>();
        services.AddScoped<IClassControlService, ClassControlService>();
        services.AddScoped<ISectionControlService, SectionControlService>();
        services.AddScoped<IClassTeacherService, ClassTeacherService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IClassSubjectAssignmentService, ClassSubjectAssignmentService>();
        services.AddScoped<IStudentElectiveService, StudentElectiveService>();
        services.AddScoped<IClassScheduleService, ClassScheduleService>();
        services.AddScoped<IStudentPromotionService, StudentPromotionService>();
        services.AddScoped<IExamTermService, ExamTermService>();
        services.AddScoped<IExamHallService, ExamHallService>();
        services.AddScoped<IMarkDistributionService, MarkDistributionService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IExamScheduleService, ExamScheduleService>();
        services.AddScoped<IMarkEntryService, MarkEntryService>();
        services.AddScoped<IGradeRangeService, GradeRangeService>();
        services.AddScoped<IExamPositionService, ExamPositionService>();
        services.AddScoped<IStudentAttendanceService, StudentAttendanceService>();
        services.AddScoped<ISubjectAttendanceService, SubjectAttendanceService>();
        services.AddScoped<IEmployeeAttendanceService, EmployeeAttendanceService>();
        services.AddScoped<IExamAttendanceService, ExamAttendanceService>();
        services.AddScoped<IAttendanceReportService, AttendanceReportService>();
        services.AddScoped<IHrReportService, HrReportService>();
        services.AddScoped<IExaminationReportService, ExaminationReportService>();
        services.AddScoped<IBookCategoryService, BookCategoryService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IBookIssueService, BookIssueService>();
        services.AddScoped<IEventTypeService, EventTypeService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IPublicWebsiteService, PublicWebsiteService>();
        services.AddScoped<ISscBoardResultService, SscBoardResultService>();
        services.AddHttpClient(SscBoardResultService.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://eduboardresults.gov.bd/");
            client.Timeout = TimeSpan.FromSeconds(45);
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent",
                "Mozilla/5.0 (Linux; Android 15; Pixel 9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Mobile Safari/537.36");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://eduboardresults.gov.bd");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://eduboardresults.gov.bd/");
        });
        services.AddScoped<IFeesTypeService, FeesTypeService>();
        services.AddScoped<IFeesGroupService, FeesGroupService>();
        services.AddScoped<IFeesAllocationService, FeesAllocationService>();
        services.AddScoped<IFineSetupService, FineSetupService>();
        services.AddScoped<IFeesReminderService, FeesReminderService>();
        services.AddScoped<IStudentFeeInvoiceService, StudentFeeInvoiceService>();
        services.AddScoped<IOfflinePaymentService, OfflinePaymentService>();
        services.AddScoped<IOfflinePaymentTypeService, OfflinePaymentTypeService>();
        services.AddScoped<IAccountingAccountService, AccountingAccountService>();
        services.AddScoped<IAccountingDepositService, AccountingDepositService>();
        services.AddScoped<IAccountingExpenseService, AccountingExpenseService>();
        services.AddScoped<IVoucherHeadService, VoucherHeadService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IGlobalSettingsService, GlobalSettingsService>();
        services.AddScoped<ISchoolSettingsService, SchoolSettingsService>();
        services.AddScoped<IBiometricPunchProcessor, BiometricPunchProcessor>();
        services.AddScoped<IBiometricDeviceService, BiometricDeviceService>();
        services.AddScoped<IBiometricUserMapService, BiometricUserMapService>();
        services.AddScoped<IBiometricPunchService, BiometricPunchService>();
        services.AddScoped<IZkTecoAdmsService, ZkTecoAdmsService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAcademicSessionService, AcademicSessionService>();
        services.AddScoped<ICronSettingsService, CronSettingsService>();
        services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
        services.AddScoped<IUserLoginLogService, UserLoginLogService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IEmailSettingsAppService, EmailSettingsService>();
        services.AddScoped<ISmsSettingsAppService, SmsSettingsService>();
        services.AddScoped<ISmsSenderFactory, SchoolManagement.BLL.Services.Sms.SmsSenderFactory>();
        services.AddHttpClient(SchoolManagement.BLL.Services.Sms.BulkSmsBdSmsSender.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
        });
        services.AddScoped<ISchoolSettingsExtrasService, SchoolSettingsExtrasService>();
        services.AddScoped<IStudentReportService, StudentReportService>();
        services.AddScoped<IPasswordRevealService, PasswordRevealService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddDataProtection();


        services.AddValidatorsFromAssemblyContaining<LoginValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateSchoolValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateAdmissionValidator>();
        services.AddValidatorsFromAssemblyContaining<SubmitOnlineAdmissionValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateStudentCategoryValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateDeactivateReasonValidator>();
        services.AddValidatorsFromAssemblyContaining<AddParentValidator>();
        services.AddValidatorsFromAssemblyContaining<AddEmployeeValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateSalaryTemplateValidator>();
        services.AddValidatorsFromAssemblyContaining<ProcessPaymentValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateAdvanceSalaryValidator>();
        services.AddValidatorsFromAssemblyContaining<GiveAwardValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateClassValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateExamValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateGradeRangeValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateEventValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateRoleValidator>();
        services.AddScoped<ValidationFilter>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings configuration is missing.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RoleClaimType = "role",
                    NameClaimType = AppConstants.Claims.UserId
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "School Management System API",
                Version = "v1",
                Description = "SaaS School Management System — Authentication & Multi-Tenancy"
            });

            options.DocumentFilter<EndpointCountDocumentFilter>();

            // Avoid collisions when DTO class names repeat across namespaces
            // (e.g. Student.StudentListResponseDto vs StudentList.StudentListResponseDto).
            options.CustomSchemaIds(type =>
            {
                if (type.IsGenericType)
                {
                    var name = type.GetGenericTypeDefinition().FullName;
                    if (name is not null)
                    {
                        var tick = name.IndexOf('`');
                        if (tick > 0) name = name[..tick];
                    }

                    var args = string.Join(",", type.GetGenericArguments().Select(SchemaId));
                    return $"{name?.Replace('+', '.')}<{args}>";
                }

                return SchemaId(type);
            });

            static string SchemaId(Type t) =>
                (t.FullName ?? t.Name).Replace('+', '.');

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT access token"
            });

            options.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
            {
                Name = "X-Tenant-ID",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Tenant slug (e.g. greenwood)"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Tenant"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
