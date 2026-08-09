using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;
public class EmployeeImportService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IEmployeeService employees, IHttpContextAccessor http) : IEmployeeImportService
{
    public byte[] GetSampleCsv()=>EmployeeCsvImportHelper.BuildSampleCsv();
    public async Task<EmployeeImportBatchDto> ImportAsync(Stream stream,string fileName,CancellationToken ct=default)
    {
        await Ready(ct); Manage(); var rows=EmployeeCsvImportHelper.Parse(stream); var batch=new EmployeeImportBatch{Id=Guid.NewGuid(),FileName=Path.GetFileName(fileName),TotalRows=rows.Count,ImportedBy=CurrentUser(),StartedAt=DateTime.UtcNow,CreatedAt=DateTime.UtcNow,Status=ImportBatchStatuses.Processing};await uow.Employees.AddImportBatchAsync(batch,ct);await uow.SaveTenantChangesAsync(ct);
        var success=0;var failed=0;
        for(var i=0;i<rows.Count;i++){var raw=rows[i];try{var dto=await ToDto(raw,ct);var employee=await employees.CreateAsync(dto,ct);await uow.Employees.AddImportBatchRowAsync(new EmployeeImportBatchRow{Id=Guid.NewGuid(),BatchId=batch.Id,RowNumber=i+2,RawData=JsonSerializer.Serialize(raw),Status="Success",EmployeeId=employee.Id,CreatedAt=DateTime.UtcNow},ct);await uow.SaveTenantChangesAsync(ct);success++;}catch(Exception ex){uow.ClearTenantChangeTracker();failed++;await uow.Employees.AddImportBatchRowAsync(new EmployeeImportBatchRow{Id=Guid.NewGuid(),BatchId=batch.Id,RowNumber=i+2,RawData=JsonSerializer.Serialize(raw),Status="Failed",ErrorMessage=ex is AppException a?a.Message:ex.Message,CreatedAt=DateTime.UtcNow},ct);await uow.SaveTenantChangesAsync(ct);}}
        batch.SuccessCount=success;batch.FailedCount=failed;batch.CompletedAt=DateTime.UtcNow;batch.Status=failed==0?ImportBatchStatuses.Completed:success==0?ImportBatchStatuses.Failed:ImportBatchStatuses.CompletedWithErrors;await uow.Employees.UpdateImportBatchAsync(batch,ct);await uow.SaveTenantChangesAsync(ct);return Map(batch,false);
    }
    public async Task<IReadOnlyList<EmployeeImportBatchDto>> GetBatchesAsync(CancellationToken ct=default){await Ready(ct);Manage();return (await uow.Employees.GetImportBatchesAsync(ct)).Select(x=>Map(x,false)).ToList();}
    public async Task<EmployeeImportBatchDto> GetBatchByIdAsync(Guid id,CancellationToken ct=default){await Ready(ct);Manage();return Map(await uow.Employees.GetImportBatchByIdAsync(id,ct)??throw new NotFoundException($"Employee import batch '{id}' not found."),true);}
    private async Task<AddEmployeeDto> ToDto(Dictionary<string,string> r,CancellationToken ct)
    {
        string V(string k) => r.TryGetValue(k, out var x) ? x : string.Empty;
        string First(params string[] keys) { foreach (var k in keys) { var v = V(k); if (!string.IsNullOrWhiteSpace(v)) return v; } return string.Empty; }
        var dept = await LookupDepartment(First("DepartmentName", "Department"), ct);
        var designation = await LookupDesignation(First("DesignationName", "Designation"), ct);
        var password = string.IsNullOrWhiteSpace(V("Password")) ? "Password123" : V("Password");
        return new AddEmployeeDto
        {
            Role = V("Role"), Name = V("Name"), Email = V("Email"), MobileNo = V("MobileNo"), Username = V("Username"),
            Password = password, RetypePassword = password,
            JoiningDate = DateTime.TryParse(V("JoiningDate"), out var joining) ? joining : DateTime.UtcNow,
            DepartmentId = dept, DesignationId = designation, Qualification = V("Qualification"),
            ExperienceDetails = V("ExperienceDetails"), TotalExperience = V("TotalExperience"), Gender = V("Gender"),
            Religion = V("Religion"), BloodGroup = V("BloodGroup"),
            DateOfBirth = DateTime.TryParse(V("DateOfBirth"), out var dob) ? dob : null,
            PresentAddress = V("PresentAddress"), PermanentAddress = V("PermanentAddress"), NidNumber = V("NidNumber"),
            SkipBankDetails = bool.TryParse(V("SkipBankDetails"), out var skip) && skip,
            BankName = V("BankName"), HolderName = V("HolderName"), BankBranch = V("BankBranch"),
            BankAddress = V("BankAddress"), IfscCode = V("IfscCode"), AccountNo = V("AccountNo"),
            FacebookUrl = V("FacebookUrl"), TwitterUrl = V("TwitterUrl"), LinkedInUrl = V("LinkedInUrl")
        };
    }
    private async Task<Guid?> LookupDepartment(string name,CancellationToken ct){if(string.IsNullOrWhiteSpace(name))return null;var x=await uow.Departments.GetByNameAsync(name.Trim(),ct);if(x is not null)return x.Id;x=new Department{Id=Guid.NewGuid(),Name=name.Trim().ToUpperInvariant(),IsActive=true,CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow};await uow.Departments.AddAsync(x,ct);return x.Id;}
    private async Task<Guid?> LookupDesignation(string name,CancellationToken ct){if(string.IsNullOrWhiteSpace(name))return null;var x=await uow.Designations.GetByNameAsync(name.Trim(),ct);if(x is not null)return x.Id;x=new Designation{Id=Guid.NewGuid(),Name=name.Trim().ToUpperInvariant(),IsActive=true,CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow};await uow.Designations.AddAsync(x,ct);return x.Id;}
    private static EmployeeImportBatchDto Map(EmployeeImportBatch x,bool detail)=>new(){Id=x.Id,FileName=x.FileName,TotalRows=x.TotalRows,SuccessCount=x.SuccessCount,FailedCount=x.FailedCount,Status=x.Status,StartedAt=x.StartedAt,CompletedAt=x.CompletedAt,Rows=detail?x.Rows.Select(r=>new EmployeeImportBatchRowDto{RowNumber=r.RowNumber,Status=r.Status,EmployeeId=r.EmployeeId,ErrorMessage=r.ErrorMessage,RawData=r.RawData}).ToList():null};
    private async Task Ready(CancellationToken ct){if(string.IsNullOrEmpty(tenant.SchemaName))throw new AppException("X-Tenant-ID header is required.",400);await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!,ct);}
    private void Manage(){var p=http.HttpContext?.User;var roles=p?.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase)??[];if(!roles.Contains(AppConstants.Roles.Admin)&&!roles.Contains(AppConstants.Roles.SuperAdmin))throw new ForbiddenException("Only Super Admin or School Admin can import employees.");}
    private Guid CurrentUser(){var c=http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)??http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);if(c is null||!Guid.TryParse(c.Value,out var id))throw new UnauthorizedException();return id;}
}
