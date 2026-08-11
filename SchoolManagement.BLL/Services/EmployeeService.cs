using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using SchoolManagement.BLL.DTOs.Employee;
using SchoolManagement.BLL.Exceptions;
using SchoolManagement.BLL.Helpers;
using SchoolManagement.BLL.Interfaces;
using SchoolManagement.Common.Constants;
using SchoolManagement.DAL.Context;
using SchoolManagement.DAL.Entities.Tenant;
using SchoolManagement.DAL.Repositories.Interfaces;
using SchoolManagement.DAL.TenantContext;
using SchoolManagement.DAL.UnitOfWork;

namespace SchoolManagement.BLL.Services;
public class EmployeeService(IUnitOfWork uow, ITenantContext tenant, ITenantSchemaProvisioner provisioner, IStorageService storage, IHttpContextAccessor http) : IEmployeeService
{
    private static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly Dictionary<string,string> RolePrefixes = new(StringComparer.OrdinalIgnoreCase) { [EmployeeRoles.Admin]=AppConstants.Roles.Admin,[EmployeeRoles.Teacher]=AppConstants.Roles.Teacher,[EmployeeRoles.Accountant]=AppConstants.Roles.Accountant,[EmployeeRoles.Librarian]=AppConstants.Roles.Librarian,[EmployeeRoles.Receptionist]=AppConstants.Roles.Receptionist,[EmployeeRoles.Staff]=AppConstants.Roles.Staff,[EmployeeRoles.Demo]=AppConstants.Roles.Demo };

    public async Task<EmployeeListResponseDto> GetListAsync(EmployeeListFilterDto filter, CancellationToken ct=default)
    {
        await Ready(ct); Manage();
        var page=filter.Page<1?1:filter.Page; var size=filter.PageSize is <1 or >200?20:filter.PageSize;
        var (items,total)=await uow.Employees.SearchAsync(new EmployeeSearchFilter{Role=filter.Role,DepartmentId=filter.DepartmentId,DesignationId=filter.DesignationId,Search=filter.Search,SortBy=filter.SortBy,SortDir=filter.SortDir,Page=page,PageSize=size,IsActive=true},ct);
        var data=new List<EmployeeListItemDto>();
        for(var i=0;i<items.Count;i++) data.Add(await MapList(items[i],(page-1)*size+i+1,ct));
        return new EmployeeListResponseDto{Data=data,TotalCount=total,Page=page,PageSize=size,TotalPages=total==0?0:(int)Math.Ceiling(total/(double)size)};
    }
    public async Task<EmployeeDetailDto> GetByIdAsync(Guid id,CancellationToken ct=default){await Ready(ct); Manage();return await Detail(id,ct);}
    public async Task<EmployeeDetailDto> GetMeAsync(CancellationToken ct=default){await Ready(ct); if(!Roles().Contains(AppConstants.Roles.Teacher))throw new ForbiddenException("Only teachers can access this endpoint."); var x=await uow.Employees.GetByUserIdAsync(CurrentUser(),ct)??throw new NotFoundException("Employee profile not found for current user."); return await MapDetail(x,ct);}
    public async Task<EmployeeDetailDto> CreateAsync(AddEmployeeDto dto,CancellationToken ct=default)
    {
        await Ready(ct); Manage(); Validate(dto);
        if(!string.Equals(dto.Password,dto.RetypePassword,StringComparison.Ordinal))throw new AppException("Password and Retype Password must match.",400);
        var username=dto.Username.Trim().ToLowerInvariant(); var email=dto.Email.Trim().ToLowerInvariant();
        if(await uow.Users.UsernameExistsAsync(username,ct))throw new ConflictException($"Username '{username}' already exists.");
        if(await uow.Users.EmailExistsAsync(email,ct)||await uow.Employees.EmailExistsAsync(email,null,ct))throw new ConflictException($"Email '{email}' already exists.");
        await ValidateLookups(dto.DepartmentId,dto.DesignationId,ct);
        var role=RolePrefix(dto.Role); var dbRole=await uow.Users.GetRoleByNameAsync(role,ct)??throw new AppException($"Role '{dto.Role}' is not seeded in this tenant.",500);
        await uow.BeginTenantTransactionAsync(ct);
        try {
            var names=dto.Name.Trim().Split(' ',2,StringSplitOptions.RemoveEmptyEntries);
            var user=new User{Id=Guid.NewGuid(),Username=username,Email=email,Password=PasswordHelper.HashPassword(dto.Password),FirstName=names[0],LastName=names.Length>1?names[1]:string.Empty,Mobileno=dto.MobileNo.Trim(),Active=true,CreatedAt=DateTime.UtcNow,UpdatedAt=DateTime.UtcNow};
            await uow.Users.AddAsync(user,ct); await uow.Users.AddUserRoleAsync(new UserRole{UserId=user.Id,RoleId=dbRole.Id},ct);
            var employee=From(dto,user.Id,await NewStaffId(ct)); await uow.Employees.AddAsync(employee,ct);
            await uow.SaveTenantChangesAsync(ct); await uow.CommitTenantTransactionAsync(ct); return await Detail(employee.Id,ct);
        } catch { await uow.RollbackTenantTransactionAsync(ct); throw; }
    }
    public async Task<EmployeeDetailDto> UpdateAsync(Guid id,UpdateEmployeeDto dto,CancellationToken ct=default)
    {
        await Ready(ct); Manage(); Validate(dto); var e=await uow.Employees.GetByIdWithDetailsAsync(id,ct)??throw new NotFoundException($"Employee '{id}' not found.");
        if(await uow.Employees.EmailExistsAsync(dto.Email,id,ct))throw new ConflictException($"Email '{dto.Email}' already exists."); await ValidateLookups(dto.DepartmentId,dto.DesignationId,ct);
        e.Role=CanonicalRole(dto.Role); e.DepartmentId=dto.DepartmentId;e.DesignationId=dto.DesignationId;e.JoiningDate=dto.JoiningDate;e.Qualification=dto.Qualification;e.ExperienceDetails=dto.ExperienceDetails;e.TotalExperience=dto.TotalExperience;e.Name=dto.Name.Trim();e.Gender=dto.Gender;e.Religion=dto.Religion;e.BloodGroup=dto.BloodGroup;e.DateOfBirth=dto.DateOfBirth;e.MobileNo=dto.MobileNo.Trim();e.Email=dto.Email.Trim().ToLowerInvariant();e.PresentAddress=dto.PresentAddress;e.PermanentAddress=dto.PermanentAddress;e.NidNumber=dto.NidNumber;e.FacebookUrl=dto.FacebookUrl;e.TwitterUrl=dto.TwitterUrl;e.LinkedInUrl=dto.LinkedInUrl;e.SkipBankDetails=dto.SkipBankDetails;
        e.BankName=dto.SkipBankDetails?null:dto.BankName;e.HolderName=dto.SkipBankDetails?null:dto.HolderName;e.BankBranch=dto.SkipBankDetails?null:dto.BankBranch;e.BankAddress=dto.SkipBankDetails?null:dto.BankAddress;e.IfscCode=dto.SkipBankDetails?null:dto.IfscCode;e.AccountNo=dto.SkipBankDetails?null:dto.AccountNo;e.UpdatedAt=DateTime.UtcNow;
        if(e.User is not null){e.User.Email=e.Email;e.User.Mobileno=e.MobileNo;var names=e.Name.Split(' ',2,StringSplitOptions.RemoveEmptyEntries);e.User.FirstName=names[0];e.User.LastName=names.Length>1?names[1]:string.Empty;e.User.UpdatedAt=DateTime.UtcNow;await uow.Users.UpdateAsync(e.User,ct);}
        await uow.Employees.UpdateAsync(e,ct);await uow.SaveTenantChangesAsync(ct);return await Detail(id,ct);
    }
    public async Task SoftDeleteAsync(Guid id,CancellationToken ct=default){await Ready(ct);Manage();var e=await uow.Employees.GetByIdWithDetailsAsync(id,ct)??throw new NotFoundException($"Employee '{id}' not found.");e.IsActive=false;e.UpdatedAt=DateTime.UtcNow;await uow.Employees.UpdateAsync(e,ct);var user=await uow.Users.GetByIdAsync(e.UserId,ct);if(user is not null){user.Active=false;user.UpdatedAt=DateTime.UtcNow;await uow.Users.UpdateAsync(user,ct);}await uow.SaveTenantChangesAsync(ct);}
    public async Task<EmployeeDetailDto> UploadPhotoAsync(Guid id,Stream stream,string name,string type,CancellationToken ct=default){await Ready(ct);Manage();if(!Images.Contains(Path.GetExtension(name))||!type.StartsWith("image/",StringComparison.OrdinalIgnoreCase)||(stream.CanSeek&&stream.Length>2*1024*1024))throw new AppException("Only jpg, jpeg, png, and webp images up to 2MB are allowed.",400);var e=await uow.Employees.GetByIdWithDetailsAsync(id,ct)??throw new NotFoundException($"Employee '{id}' not found.");var slug=tenant.TenantSlug??throw new AppException("Tenant slug is not resolved.",400);var key=$"{AppConstants.StorageFolders.Employees}/{id}/profile{Path.GetExtension(name).ToLowerInvariant()}";if(!string.IsNullOrWhiteSpace(e.ProfilePictureUrl))try{await storage.DeleteFileAsync(slug,e.ProfilePictureUrl,ct);}catch{}await storage.UploadObjectAsync(slug,key,stream,type,ct);e.ProfilePictureUrl=key;e.UpdatedAt=DateTime.UtcNow;await uow.Employees.UpdateAsync(e,ct);await uow.SaveTenantChangesAsync(ct);return await MapDetail(e,ct);}
    public async Task<EmployeeDetailDto> UploadSignatureAsync(Guid id,Stream stream,string name,string type,CancellationToken ct=default)
    {
        await Ready(ct); Manage();
        if (!Images.Contains(Path.GetExtension(name)) || !type.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || (stream.CanSeek && stream.Length > 2 * 1024 * 1024))
            throw new AppException("Only jpg, jpeg, png, and webp images up to 2MB are allowed.", 400);
        var e = await uow.Employees.GetByIdWithDetailsAsync(id, ct) ?? throw new NotFoundException($"Employee '{id}' not found.");
        var slug = tenant.TenantSlug ?? throw new AppException("Tenant slug is not resolved.", 400);
        var key = $"{AppConstants.StorageFolders.Employees}/{id}/signature{Path.GetExtension(name).ToLowerInvariant()}";
        if (!string.IsNullOrWhiteSpace(e.SignatureUrl))
            try { await storage.DeleteFileAsync(slug, e.SignatureUrl, ct); } catch { }
        await storage.UploadObjectAsync(slug, key, stream, type, ct);
        e.SignatureUrl = key;
        e.UpdatedAt = DateTime.UtcNow;
        await uow.Employees.UpdateAsync(e, ct);
        await uow.SaveTenantChangesAsync(ct);
        return await MapDetail(e, ct);
    }
    public async Task<(byte[] Content,string ContentType,string FileName)> ExportAsync(EmployeeListFilterDto filter,CancellationToken ct=default){var data=await GetListAsync(new EmployeeListFilterDto{Role=filter.Role,DepartmentId=filter.DepartmentId,DesignationId=filter.DesignationId,Search=filter.Search,SortBy=filter.SortBy,SortDir=filter.SortDir,Page=1,PageSize=200},ct);var sb=new StringBuilder("StaffId,Name,Role,Department,Designation,Email,MobileNo,IsLoginActive\n");foreach(var x in data.Data)sb.AppendLine($"{Csv(x.StaffId)},{Csv(x.Name)},{Csv(x.Role)},{Csv(x.Department)},{Csv(x.Designation)},{Csv(x.Email)},{Csv(x.MobileNo)},{x.IsLoginActive}");var bytes=Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();var fmt=(filter.Export??"csv").ToLowerInvariant();return fmt switch{"csv"=>(bytes,"text/csv",$"employees-{DateTime.UtcNow:yyyyMMdd}.csv"),"excel"=>(bytes,"application/vnd.ms-excel",$"employees-{DateTime.UtcNow:yyyyMMdd}.xls"),"pdf"=>(bytes,"application/pdf",$"employees-{DateTime.UtcNow:yyyyMMdd}.pdf"),_=>throw new AppException("Unsupported export format. Use csv, excel, or pdf.",400)};}
    public async Task<IReadOnlyList<string>> GetRolesLookupAsync(CancellationToken ct=default){await Ready(ct);Manage();return EmployeeRoles.All;}
    public async Task<IReadOnlyList<DepartmentResponseDto>> GetDepartmentsLookupAsync(CancellationToken ct=default){await Ready(ct);Manage();return (await uow.Departments.GetAllAsync(ct)).Where(x=>x.IsActive).Select(x=>new DepartmentResponseDto{Id=x.Id,Name=x.Name,IsActive=x.IsActive,CreatedAt=x.CreatedAt,Branch=tenant.TenantName??string.Empty}).ToList();}
    public async Task<IReadOnlyList<DesignationResponseDto>> GetDesignationsLookupAsync(CancellationToken ct=default){await Ready(ct);Manage();return (await uow.Designations.GetAllAsync(ct)).Where(x=>x.IsActive).Select(x=>new DesignationResponseDto{Id=x.Id,Name=x.Name,IsActive=x.IsActive,CreatedAt=x.CreatedAt,Branch=tenant.TenantName??string.Empty}).ToList();}
    public async Task<EmployeeLoginDeactivateListResponseDto> GetLoginDeactivateListAsync(EmployeeLoginDeactivateFilterDto f,CancellationToken ct=default)
    {
        await Ready(ct); Manage();
        if (string.IsNullOrWhiteSpace(f.Role)) throw new AppException("Role is required.", 400);
        var page = f.Page < 1 ? 1 : f.Page;
        var size = f.PageSize is < 1 or > 200 ? 20 : f.PageSize;
        var (items, total) = await uow.Employees.SearchAsync(new EmployeeSearchFilter { Role = f.Role, Search = f.Search, SortBy = f.SortBy, SortDir = f.SortDir, IsActive = true, IsLoginActive = false, Page = page, PageSize = size }, ct);
        var data = new List<EmployeeLoginDeactivateItemDto>();
        foreach (var x in items)
        {
            data.Add(new EmployeeLoginDeactivateItemDto
            {
                Id = x.Id,
                EmployeeId = x.Id,
                PhotoUrl = await Presign(x.ProfilePictureUrl, ct),
                Branch = tenant.TenantName ?? string.Empty,
                Name = x.Name,
                Designation = x.Designation?.Name,
                Department = x.Department?.Name,
                Email = x.Email,
                MobileNo = x.MobileNo,
                Role = x.Role,
                IsLoginActive = x.User?.Active ?? false
            });
        }
        return new EmployeeLoginDeactivateListResponseDto { Data = data, TotalCount = total, Page = page, PageSize = size, TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)size) };
    }
    public async Task ActivateLoginAsync(Guid id,CancellationToken ct=default)=>await SetLogin(id,true,ct);
    public async Task DeactivateLoginAsync(Guid id,CancellationToken ct=default)=>await SetLogin(id,false,ct);
    public async Task<BulkEmployeeActivateResultDto> BulkActivateLoginAsync(BulkEmployeeActivateDto dto,CancellationToken ct=default)
    {
        await Ready(ct); Manage();
        var r = new BulkEmployeeActivateResultDto();
        await uow.BeginTenantTransactionAsync(ct);
        try
        {
            foreach (var id in dto.EmployeeIds.Distinct())
            {
                try
                {
                    var e = await uow.Employees.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Employee '{id}' not found.");
                    var user = await uow.Users.GetByIdAsync(e.UserId, ct) ?? throw new NotFoundException("Employee user not found.");
                    user.Active = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await uow.Users.UpdateAsync(user, ct);
                    r.Activated++;
                }
                catch (NotFoundException) { r.Failed++; }
            }
            await uow.SaveTenantChangesAsync(ct);
            await uow.CommitTenantTransactionAsync(ct);
            return r;
        }
        catch { await uow.RollbackTenantTransactionAsync(ct); throw; }
    }
    private async Task SetLogin(Guid id,bool active,CancellationToken ct){await Ready(ct);Manage();var e=await uow.Employees.GetByIdAsync(id,ct)??throw new NotFoundException($"Employee '{id}' not found.");var user=await uow.Users.GetByIdAsync(e.UserId,ct)??throw new NotFoundException("Employee user not found.");user.Active=active;user.UpdatedAt=DateTime.UtcNow;await uow.Users.UpdateAsync(user,ct);await uow.SaveTenantChangesAsync(ct);}
    private Employee From(AddEmployeeDto d,Guid userId,string staff)
    {
        var skip = d.SkipBankDetails;
        return new()
        {
            Id = Guid.NewGuid(), UserId = userId, StaffId = staff, Role = CanonicalRole(d.Role),
            DesignationId = d.DesignationId, DepartmentId = d.DepartmentId, JoiningDate = d.JoiningDate,
            Qualification = d.Qualification, ExperienceDetails = d.ExperienceDetails, TotalExperience = d.TotalExperience,
            Name = d.Name.Trim(), Gender = d.Gender, Religion = d.Religion, BloodGroup = d.BloodGroup, DateOfBirth = d.DateOfBirth,
            MobileNo = d.MobileNo.Trim(), Email = d.Email.Trim().ToLowerInvariant(), PresentAddress = d.PresentAddress,
            PermanentAddress = d.PermanentAddress, NidNumber = d.NidNumber, FacebookUrl = d.FacebookUrl,
            TwitterUrl = d.TwitterUrl, LinkedInUrl = d.LinkedInUrl, SkipBankDetails = skip,
            BankName = skip ? null : d.BankName, HolderName = skip ? null : d.HolderName,
            BankBranch = skip ? null : d.BankBranch, BankAddress = skip ? null : d.BankAddress,
            IfscCode = skip ? null : d.IfscCode, AccountNo = skip ? null : d.AccountNo,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
    }
    private async Task<string> NewStaffId(CancellationToken ct){for(var i=0;i<20;i++){var id=$"{(char)Random.Shared.Next('A','Z'+1)}{Random.Shared.Next(0,1_000_000):D6}";if(!await uow.Employees.StaffIdExistsAsync(id,ct))return id;}throw new AppException("Unable to generate a unique staff ID.",500);}
    private async Task ValidateLookups(Guid? dept,Guid? designation,CancellationToken ct){if(dept.HasValue&&await uow.Departments.GetByIdAsync(dept.Value,ct)is null)throw new AppException("Invalid DepartmentId.",400);if(designation.HasValue&&await uow.Designations.GetByIdAsync(designation.Value,ct)is null)throw new AppException("Invalid DesignationId.",400);}
    private static string CanonicalRole(string role)=>EmployeeRoles.All.FirstOrDefault(x=>x.Equals(role.Trim(),StringComparison.OrdinalIgnoreCase))??throw new AppException("Invalid employee role.",400);
    private static string RolePrefix(string role)=>RolePrefixes[CanonicalRole(role)];
    private static void Validate(AddEmployeeDto d){if(string.IsNullOrWhiteSpace(d.Name)||string.IsNullOrWhiteSpace(d.Email)||string.IsNullOrWhiteSpace(d.MobileNo)||string.IsNullOrWhiteSpace(d.Username))throw new AppException("Name, email, mobile number, and username are required.",400);CanonicalRole(d.Role);if(!d.SkipBankDetails&&(string.IsNullOrWhiteSpace(d.BankName)||string.IsNullOrWhiteSpace(d.HolderName)||string.IsNullOrWhiteSpace(d.BankBranch)||string.IsNullOrWhiteSpace(d.AccountNo)))throw new AppException("Bank name, holder name, bank branch, and account number are required.",400);}
    private static void Validate(UpdateEmployeeDto d){if(string.IsNullOrWhiteSpace(d.Name)||string.IsNullOrWhiteSpace(d.Email)||string.IsNullOrWhiteSpace(d.MobileNo))throw new AppException("Name, email, and mobile number are required.",400);CanonicalRole(d.Role);if(!d.SkipBankDetails&&(string.IsNullOrWhiteSpace(d.BankName)||string.IsNullOrWhiteSpace(d.HolderName)||string.IsNullOrWhiteSpace(d.BankBranch)||string.IsNullOrWhiteSpace(d.AccountNo)))throw new AppException("Bank name, holder name, bank branch, and account number are required.",400);}
    private async Task<EmployeeListItemDto> MapList(Employee e,int sl,CancellationToken ct)=>new(){Id=e.Id,Sl=sl,PhotoUrl=await Presign(e.ProfilePictureUrl,ct),SignatureUrl=await Presign(e.SignatureUrl,ct),Branch=tenant.TenantName??string.Empty,StaffId=e.StaffId,Name=e.Name,Role=e.Role,Department=e.Department?.Name,Designation=e.Designation?.Name,Email=e.Email,MobileNo=e.MobileNo,IsActive=e.IsActive,IsLoginActive=e.User?.Active??false};
    private async Task<EmployeeDetailDto> Detail(Guid id,CancellationToken ct)=>await MapDetail(await uow.Employees.GetByIdWithDetailsAsync(id,ct)??throw new NotFoundException($"Employee '{id}' not found."),ct);
    private async Task<EmployeeDetailDto> MapDetail(Employee e,CancellationToken ct)=>new(){Id=e.Id,UserId=e.UserId,Branch=tenant.TenantName??string.Empty,StaffId=e.StaffId,Name=e.Name,Role=e.Role,Department=e.Department?.Name,Designation=e.Designation?.Name,Email=e.Email,MobileNo=e.MobileNo,IsActive=e.IsActive,IsLoginActive=e.User?.Active??false,JoiningDate=e.JoiningDate,Qualification=e.Qualification,ExperienceDetails=e.ExperienceDetails,TotalExperience=e.TotalExperience,Gender=e.Gender,Religion=e.Religion,BloodGroup=e.BloodGroup,DateOfBirth=e.DateOfBirth,PresentAddress=e.PresentAddress,PermanentAddress=e.PermanentAddress,NidNumber=e.NidNumber,PhotoUrl=await Presign(e.ProfilePictureUrl,ct),SignatureUrl=await Presign(e.SignatureUrl,ct),Username=e.User?.Username,FacebookUrl=e.FacebookUrl,TwitterUrl=e.TwitterUrl,LinkedInUrl=e.LinkedInUrl,SkipBankDetails=e.SkipBankDetails,BankName=e.BankName,HolderName=e.HolderName,BankBranch=e.BankBranch,BankAddress=e.BankAddress,IfscCode=e.IfscCode,AccountNo=e.AccountNo,CreatedAt=e.CreatedAt};
    private async Task<string?> Presign(string? key,CancellationToken ct){if(string.IsNullOrWhiteSpace(key)||string.IsNullOrWhiteSpace(tenant.TenantSlug))return key;try{return await storage.GetPresignedUrlAsync(tenant.TenantSlug,key,ct);}catch{return key;} }
    private async Task Ready(CancellationToken ct){if(string.IsNullOrEmpty(tenant.SchemaName))throw new AppException("X-Tenant-ID header is required.",400);await provisioner.EnsureEmployeeModuleAsync(tenant.SchemaName!,ct);}
    private HashSet<string> Roles(){var p=http.HttpContext?.User;if(p is null)return [];return p.FindAll("role").Concat(p.FindAll(ClaimTypes.Role)).Select(x=>x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);}
    private void Manage(){var r=Roles();if(!r.Contains(AppConstants.Roles.Admin)&&!r.Contains(AppConstants.Roles.SuperAdmin))throw new ForbiddenException("Only Super Admin or School Admin can manage employees.");}
    private Guid CurrentUser(){var c=http.HttpContext?.User.FindFirst(AppConstants.Claims.UserId)??http.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);if(c is null||!Guid.TryParse(c.Value,out var id))throw new UnauthorizedException();return id;}
    private static string Csv(string? v)=>string.IsNullOrEmpty(v)?"":$"\"{v.Replace("\"","\"\"")}\"";
}
