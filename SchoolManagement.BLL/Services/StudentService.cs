using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolManagement.BLL.DTOs.Student;
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

public class StudentService : IStudentService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxImageBytes = 2 * 1024 * 1024;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantSchemaProvisioner _schemaProvisioner;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ITenantSchemaProvisioner schemaProvisioner,
        IStorageService storageService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<StudentService> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _schemaProvisioner = schemaProvisioner;
        _storageService = storageService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<StudentListResponseDto> GetStudentsAsync(
        StudentSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanList();

        var userId = GetCurrentUserId();
        var roles = GetCurrentRoles();

        if (roles.Contains(AppConstants.Roles.Student) &&
            !roles.Contains(AppConstants.Roles.Admin) &&
            !roles.Contains(AppConstants.Roles.SuperAdmin) &&
            !roles.Contains(AppConstants.Roles.Teacher))
        {
            var own = await _unitOfWork.Students.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new ForbiddenException("You can only view your own profile.");
            var dto = await MapStudentAsync(own, 1, cancellationToken);
            return new StudentListResponseDto
            {
                Items = [dto],
                Page = 1,
                PageSize = 1,
                TotalCount = 1,
                TotalPages = 1
            };
        }

        if (roles.Contains(AppConstants.Roles.Parent) &&
            !roles.Contains(AppConstants.Roles.Admin) &&
            !roles.Contains(AppConstants.Roles.SuperAdmin) &&
            !roles.Contains(AppConstants.Roles.Teacher))
        {
            var wards = await _unitOfWork.Students.GetByGuardianUserIdAsync(userId, cancellationToken);
            var items = new List<StudentResponseDto>();
            for (var i = 0; i < wards.Count; i++)
                items.Add(await MapStudentAsync(wards[i], i + 1, cancellationToken));

            return new StudentListResponseDto
            {
                Items = items,
                Page = 1,
                PageSize = items.Count,
                TotalCount = items.Count,
                TotalPages = 1
            };
        }

        var (rows, total) = await _unitOfWork.Students.SearchAsync(filter, cancellationToken);
        var mapped = new List<StudentResponseDto>();
        var startSl = (Math.Max(filter.Page, 1) - 1) * Math.Max(filter.PageSize, 1);
        for (var i = 0; i < rows.Count; i++)
            mapped.Add(await MapStudentAsync(rows[i], startSl + i + 1, cancellationToken));

        return new StudentListResponseDto
        {
            Items = mapped,
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = filter.PageSize is < 1 or > 200 ? 20 : filter.PageSize,
            TotalCount = total,
            TotalPages = filter.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)Math.Max(filter.PageSize, 1))
        };
    }

    public async Task<StudentResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        EnsureCanReadStudent(student);
        return await MapStudentAsync(student, 1, cancellationToken);
    }

    public async Task<StudentResponseDto> CreateAdmissionAsync(
        CreateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        if (await _unitOfWork.Students.RegisterNoExistsAsync(dto.RegisterNo.Trim(), null, cancellationToken))
            throw new ConflictException($"Register number '{dto.RegisterNo}' already exists.");

        if (await _unitOfWork.Users.UsernameExistsAsync(dto.Username.Trim().ToLowerInvariant(), cancellationToken))
            throw new ConflictException($"Username '{dto.Username}' already exists.");

        var email = string.IsNullOrWhiteSpace(dto.Email)
            ? $"{dto.Username.Trim().ToLowerInvariant()}@students.local"
            : dto.Email.Trim().ToLowerInvariant();

        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException($"Email '{email}' already exists.");

        await ValidateAcademicLinksAsync(dto.ClassId, dto.SectionId, dto.CategoryId,
            dto.TransportRouteId, dto.HostelId, dto.RoomId, cancellationToken);

        var studentRole = await _unitOfWork.Users.GetRoleByNameAsync(AppConstants.Roles.Student, cancellationToken)
            ?? throw new AppException("Student role is not seeded in this tenant.", 500);
        var parentRole = await _unitOfWork.Users.GetRoleByNameAsync(AppConstants.Roles.Parent, cancellationToken)
            ?? throw new AppException("Parent/Guardian role is not seeded in this tenant.", 500);

        await _unitOfWork.BeginTenantTransactionAsync(cancellationToken);
        try
        {
            var studentUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = dto.Username.Trim().ToLowerInvariant(),
                Password = PasswordHelper.HashPassword(dto.Password),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName?.Trim() ?? string.Empty,
                Mobileno = dto.MobileNo,
                Active = true,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Users.AddAsync(studentUser, cancellationToken);
            await _unitOfWork.Users.AddUserRoleAsync(new UserRole
            {
                UserId = studentUser.Id,
                RoleId = studentRole.Id
            }, cancellationToken);

            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = studentUser.Id,
                RegisterNo = dto.RegisterNo.Trim(),
                Roll = dto.Roll?.Trim(),
                AcademicYear = dto.AcademicYear,
                AdmissionDate = dto.AdmissionDate == default ? DateTime.UtcNow.Date : dto.AdmissionDate.Date,
                ClassId = dto.ClassId,
                SectionId = dto.SectionId,
                CategoryId = dto.CategoryId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName?.Trim(),
                Gender = dto.Gender,
                BloodGroup = dto.BloodGroup,
                DateOfBirth = dto.DateOfBirth?.Date,
                MotherTongue = dto.MotherTongue,
                Religion = dto.Religion.Trim(),
                Caste = dto.Caste,
                MobileNo = dto.MobileNo.Trim(),
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
                City = dto.City,
                State = dto.State,
                PresentAddress = dto.PresentAddress,
                PermanentAddress = dto.PermanentAddress,
                FathersNidNumber = dto.FathersNidNumber,
                MothersNidNumber = dto.MothersNidNumber,
                BirthRegistrationNumber = dto.BirthRegistrationNumber,
                TransportRouteId = dto.TransportRouteId,
                VehicleNo = dto.TransportRouteId.HasValue ? dto.VehicleNo : null,
                HostelId = dto.HostelId,
                RoomId = dto.RoomId,
                PreviousSchoolName = dto.PreviousSchoolName,
                PreviousSchoolQualification = dto.PreviousSchoolQualification,
                Remarks = dto.Remarks,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Students.AddAsync(student, cancellationToken);

            if (dto.GuardianAlreadyExist)
            {
                var existing = await _unitOfWork.Guardians.GetByIdAsync(dto.ExistingGuardianId!.Value, cancellationToken)
                    ?? throw new NotFoundException($"Guardian '{dto.ExistingGuardianId}' not found.");

                await _unitOfWork.Guardians.AddAsync(new Guardian
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    UserId = existing.UserId,
                    Name = existing.Name,
                    Relation = existing.Relation,
                    FatherName = existing.FatherName,
                    MotherName = existing.MotherName,
                    Occupation = existing.Occupation,
                    Income = existing.Income,
                    Education = existing.Education,
                    City = existing.City,
                    State = existing.State,
                    MobileNo = existing.MobileNo,
                    Email = existing.Email,
                    Address = existing.Address,
                    ProfilePictureUrl = existing.ProfilePictureUrl,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }, cancellationToken);
            }
            else if (dto.Guardian is not null)
            {
                Guid? guardianUserId = null;
                var g = dto.Guardian;
                if (!string.IsNullOrWhiteSpace(g.Username) && !string.IsNullOrWhiteSpace(g.Password))
                {
                    var gUsername = g.Username.Trim().ToLowerInvariant();
                    if (await _unitOfWork.Users.UsernameExistsAsync(gUsername, cancellationToken))
                        throw new ConflictException($"Guardian username '{gUsername}' already exists.");

                    var gEmail = string.IsNullOrWhiteSpace(g.Email)
                        ? $"{gUsername}@guardians.local"
                        : g.Email.Trim().ToLowerInvariant();

                    if (await _unitOfWork.Users.EmailExistsAsync(gEmail, cancellationToken))
                        throw new ConflictException($"Guardian email '{gEmail}' already exists.");

                    var nameParts = g.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    var guardianUser = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = gEmail,
                        Username = gUsername,
                        Password = PasswordHelper.HashPassword(g.Password),
                        FirstName = nameParts[0],
                        LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
                        Mobileno = g.MobileNo,
                        Active = true,
                        IsEmailVerified = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Users.AddAsync(guardianUser, cancellationToken);
                    await _unitOfWork.Users.AddUserRoleAsync(new UserRole
                    {
                        UserId = guardianUser.Id,
                        RoleId = parentRole.Id
                    }, cancellationToken);
                    guardianUserId = guardianUser.Id;
                }

                await _unitOfWork.Guardians.AddAsync(new Guardian
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    UserId = guardianUserId,
                    Name = g.Name.Trim(),
                    Relation = g.Relation.Trim(),
                    FatherName = g.FatherName,
                    MotherName = g.MotherName,
                    Occupation = g.Occupation,
                    Income = g.Income,
                    Education = g.Education,
                    City = g.City,
                    State = g.State,
                    MobileNo = g.MobileNo.Trim(),
                    Email = string.IsNullOrWhiteSpace(g.Email) ? null : g.Email.Trim().ToLowerInvariant(),
                    Address = g.Address,
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
            await _unitOfWork.CommitTenantTransactionAsync(cancellationToken);

            _logger.LogInformation("Admission created for {RegisterNo} in tenant {Slug}",
                student.RegisterNo, _tenantContext.TenantSlug);

            var created = await _unitOfWork.Students.GetByIdWithDetailsAsync(student.Id, cancellationToken)
                ?? student;
            return await MapStudentAsync(created, 1, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTenantTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StudentResponseDto> UpdateAdmissionAsync(
        Guid id,
        UpdateAdmissionDto dto,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        var classId = dto.ClassId ?? student.ClassId ?? Guid.Empty;
        var sectionId = dto.SectionId ?? student.SectionId ?? Guid.Empty;
        if (classId != Guid.Empty && sectionId != Guid.Empty)
        {
            await ValidateAcademicLinksAsync(
                classId,
                sectionId,
                dto.CategoryId ?? student.CategoryId,
                dto.TransportRouteId ?? student.TransportRouteId,
                dto.HostelId ?? student.HostelId,
                dto.RoomId ?? student.RoomId,
                cancellationToken);
        }

        if (dto.Roll is not null) student.Roll = dto.Roll;
        if (dto.AdmissionDate.HasValue) student.AdmissionDate = dto.AdmissionDate.Value.Date;
        if (dto.ClassId.HasValue) student.ClassId = dto.ClassId;
        if (dto.SectionId.HasValue) student.SectionId = dto.SectionId;
        if (dto.CategoryId.HasValue) student.CategoryId = dto.CategoryId;
        if (dto.FirstName is not null) student.FirstName = dto.FirstName.Trim();
        if (dto.LastName is not null) student.LastName = dto.LastName;
        if (dto.Gender is not null) student.Gender = dto.Gender;
        if (dto.BloodGroup is not null) student.BloodGroup = dto.BloodGroup;
        if (dto.DateOfBirth.HasValue) student.DateOfBirth = dto.DateOfBirth.Value.Date;
        if (dto.MotherTongue is not null) student.MotherTongue = dto.MotherTongue;
        if (dto.Religion is not null) student.Religion = dto.Religion;
        if (dto.Caste is not null) student.Caste = dto.Caste;
        if (dto.MobileNo is not null) student.MobileNo = dto.MobileNo;
        if (dto.Email is not null) student.Email = dto.Email;
        if (dto.City is not null) student.City = dto.City;
        if (dto.State is not null) student.State = dto.State;
        if (dto.PresentAddress is not null) student.PresentAddress = dto.PresentAddress;
        if (dto.PermanentAddress is not null) student.PermanentAddress = dto.PermanentAddress;
        if (dto.FathersNidNumber is not null) student.FathersNidNumber = dto.FathersNidNumber;
        if (dto.MothersNidNumber is not null) student.MothersNidNumber = dto.MothersNidNumber;
        if (dto.BirthRegistrationNumber is not null) student.BirthRegistrationNumber = dto.BirthRegistrationNumber;
        if (dto.TransportRouteId.HasValue) student.TransportRouteId = dto.TransportRouteId;
        if (dto.VehicleNo is not null) student.VehicleNo = student.TransportRouteId.HasValue ? dto.VehicleNo : null;
        if (dto.HostelId.HasValue) student.HostelId = dto.HostelId;
        if (dto.RoomId.HasValue) student.RoomId = dto.RoomId;
        if (dto.PreviousSchoolName is not null) student.PreviousSchoolName = dto.PreviousSchoolName;
        if (dto.PreviousSchoolQualification is not null) student.PreviousSchoolQualification = dto.PreviousSchoolQualification;
        if (dto.Remarks is not null) student.Remarks = dto.Remarks;

        student.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);

        if (dto.Guardian is not null)
        {
            var primary = student.Guardians.FirstOrDefault(g => g.IsPrimary) ?? student.Guardians.FirstOrDefault();
            if (primary is not null)
            {
                var g = dto.Guardian;
                if (!string.IsNullOrWhiteSpace(g.Name)) primary.Name = g.Name.Trim();
                if (!string.IsNullOrWhiteSpace(g.Relation)) primary.Relation = g.Relation.Trim();
                if (g.FatherName is not null) primary.FatherName = g.FatherName;
                if (g.MotherName is not null) primary.MotherName = g.MotherName;
                if (g.Occupation is not null) primary.Occupation = g.Occupation;
                if (g.Income.HasValue) primary.Income = g.Income;
                if (g.Education is not null) primary.Education = g.Education;
                if (g.City is not null) primary.City = g.City;
                if (g.State is not null) primary.State = g.State;
                if (!string.IsNullOrWhiteSpace(g.MobileNo)) primary.MobileNo = g.MobileNo.Trim();
                if (g.Email is not null) primary.Email = g.Email;
                if (g.Address is not null) primary.Address = g.Address;
                primary.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Guardians.UpdateAsync(primary, cancellationToken);
            }
        }

        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
        var updated = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken) ?? student;
        return await MapStudentAsync(updated, 1, cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();

        var student = await _unitOfWork.Students.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(student.UserId, cancellationToken);
        if (user is not null)
        {
            user.Active = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);
    }

    public async Task<StudentResponseDto> UploadProfilePictureAsync(
        Guid id,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();
        ValidateImage(fileName, contentType, fileStream);

        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Student '{id}' not found.");

        var slug = RequireTenantSlug();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var objectKey = $"{AppConstants.StorageFolders.Students}/{student.Id}/profile{ext}";

        if (!string.IsNullOrWhiteSpace(student.ProfilePictureUrl))
        {
            try { await _storageService.DeleteFileAsync(slug, student.ProfilePictureUrl, cancellationToken); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old student photo"); }
        }

        var stored = await _storageService.UploadObjectAsync(slug, objectKey, fileStream, contentType, cancellationToken);
        student.ProfilePictureUrl = stored;
        student.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Students.UpdateAsync(student, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        return await MapStudentAsync(student, 1, cancellationToken);
    }

    public async Task<StudentResponseDto> UploadGuardianPictureAsync(
        Guid studentId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        EnsureCanManage();
        ValidateImage(fileName, contentType, fileStream);

        var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(studentId, cancellationToken)
            ?? throw new NotFoundException($"Student '{studentId}' not found.");

        var guardian = student.Guardians.FirstOrDefault(g => g.IsPrimary)
            ?? student.Guardians.FirstOrDefault()
            ?? throw new NotFoundException("No guardian found for this student.");

        var slug = RequireTenantSlug();
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var objectKey = $"{AppConstants.StorageFolders.Guardians}/{guardian.Id}/profile{ext}";

        if (!string.IsNullOrWhiteSpace(guardian.ProfilePictureUrl))
        {
            try { await _storageService.DeleteFileAsync(slug, guardian.ProfilePictureUrl, cancellationToken); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old guardian photo"); }
        }

        var stored = await _storageService.UploadObjectAsync(slug, objectKey, fileStream, contentType, cancellationToken);
        guardian.ProfilePictureUrl = stored;
        guardian.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Guardians.UpdateAsync(guardian, cancellationToken);
        await _unitOfWork.SaveTenantChangesAsync(cancellationToken);

        var refreshed = await _unitOfWork.Students.GetByIdWithDetailsAsync(studentId, cancellationToken) ?? student;
        return await MapStudentAsync(refreshed, 1, cancellationToken);
    }

    public Task<IReadOnlyList<int>> GetAcademicYearsAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        IReadOnlyList<int> years = Enumerable.Range(year - 2, 5).ToList();
        return Task.FromResult(years);
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetClassesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetClassesAsync(cancellationToken);
        return items.Select(c => new AdmissionLookupItemDto
        {
            Id = c.Id,
            Name = c.Name,
            NumericName = c.NumericName
        }).ToList();
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetSectionsAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetSectionsByClassIdAsync(classId, cancellationToken);
        return items.Select(s => new AdmissionLookupItemDto
        {
            Id = s.Id,
            Name = s.Name,
            ParentId = s.ClassId
        }).ToList();
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetCategoriesAsync(cancellationToken);
        return items.Select(c => new AdmissionLookupItemDto { Id = c.Id, Name = c.Name }).ToList();
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetTransportRoutesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetTransportRoutesAsync(cancellationToken);
        return items.Select(r => new AdmissionLookupItemDto { Id = r.Id, Name = r.Name }).ToList();
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetHostelsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetHostelsAsync(cancellationToken);
        return items.Select(h => new AdmissionLookupItemDto { Id = h.Id, Name = h.Name }).ToList();
    }

    public async Task<IReadOnlyList<AdmissionLookupItemDto>> GetHostelRoomsAsync(
        Guid hostelId,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var items = await _unitOfWork.AdmissionLookups.GetHostelRoomsAsync(hostelId, cancellationToken);
        return items.Select(r => new AdmissionLookupItemDto
        {
            Id = r.Id,
            Name = r.Name,
            ParentId = r.HostelId
        }).ToList();
    }

    public async Task<NextRegisterNoDto> GetNextRegisterNoAsync(
        int? academicYear = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken);
        var year = academicYear ?? DateTime.UtcNow.Year;
        var count = await _unitOfWork.Students.CountByAcademicYearAsync(year, cancellationToken);
        var sequence = count + 1;
        return new NextRegisterNoDto
        {
            AcademicYear = year,
            Sequence = sequence,
            RegisterNo = $"{year}-{sequence:D5}"
        };
    }

    private async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_tenantContext.SchemaName))
            throw new AppException("X-Tenant-ID header is required for admission operations.", 400);

        await _schemaProvisioner.EnsureAdmissionModuleAsync(_tenantContext.SchemaName, cancellationToken);
    }

    private string RequireTenantSlug()
    {
        return _tenantContext.TenantSlug
            ?? throw new AppException("Tenant slug is not resolved.", 400);
    }

    private async Task ValidateAcademicLinksAsync(
        Guid classId,
        Guid sectionId,
        Guid? categoryId,
        Guid? transportRouteId,
        Guid? hostelId,
        Guid? roomId,
        CancellationToken cancellationToken)
    {
        var clazz = await _unitOfWork.AdmissionLookups.GetClassByIdAsync(classId, cancellationToken)
            ?? throw new AppException("Invalid ClassId.", 400);

        var section = await _unitOfWork.AdmissionLookups.GetSectionByIdAsync(sectionId, cancellationToken)
            ?? throw new AppException("Invalid SectionId.", 400);

        if (section.ClassId != clazz.Id)
            throw new AppException("SectionId does not belong to the selected ClassId.", 400);

        if (categoryId.HasValue)
        {
            _ = await _unitOfWork.AdmissionLookups.GetCategoryByIdAsync(categoryId.Value, cancellationToken)
                ?? throw new AppException("Invalid CategoryId.", 400);
        }

        if (transportRouteId.HasValue)
        {
            _ = await _unitOfWork.AdmissionLookups.GetTransportRouteByIdAsync(transportRouteId.Value, cancellationToken)
                ?? throw new AppException("Invalid TransportRouteId.", 400);
        }

        if (hostelId.HasValue)
        {
            _ = await _unitOfWork.AdmissionLookups.GetHostelByIdAsync(hostelId.Value, cancellationToken)
                ?? throw new AppException("Invalid HostelId.", 400);
        }

        if (roomId.HasValue)
        {
            var room = await _unitOfWork.AdmissionLookups.GetHostelRoomByIdAsync(roomId.Value, cancellationToken)
                ?? throw new AppException("Invalid RoomId.", 400);

            if (!hostelId.HasValue || room.HostelId != hostelId.Value)
                throw new AppException("RoomId does not belong to the selected HostelId.", 400);
        }
    }

    private void EnsureCanManage()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) || roles.Contains(AppConstants.Roles.Admin))
            return;
        throw new ForbiddenException("Only Super Admin or School Admin can manage admissions.");
    }

    private void EnsureCanList()
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher) ||
            roles.Contains(AppConstants.Roles.Parent) ||
            roles.Contains(AppConstants.Roles.Student))
            return;
        throw new ForbiddenException("You do not have access to student admissions.");
    }

    private void EnsureCanReadStudent(Student student)
    {
        var roles = GetCurrentRoles();
        if (roles.Contains(AppConstants.Roles.SuperAdmin) ||
            roles.Contains(AppConstants.Roles.Admin) ||
            roles.Contains(AppConstants.Roles.Teacher))
            return;

        var userId = GetCurrentUserId();
        if (roles.Contains(AppConstants.Roles.Student) && student.UserId == userId)
            return;

        if (roles.Contains(AppConstants.Roles.Parent) &&
            student.Guardians.Any(g => g.UserId == userId))
            return;

        throw new ForbiddenException("You do not have access to this student profile.");
    }

    private Guid GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.Claims.UserId)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

        if (claim is null || !Guid.TryParse(claim.Value, out var id))
            throw new UnauthorizedException();

        return id;
    }

    private HashSet<string> GetCurrentRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roles = user.FindAll("role")
            .Concat(user.FindAll(ClaimTypes.Role))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rolesCsv = user.FindFirst(AppConstants.Claims.Roles)?.Value;
        if (!string.IsNullOrWhiteSpace(rolesCsv))
        {
            foreach (var r in rolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                roles.Add(r);
        }

        return roles;
    }

    private static void ValidateImage(string fileName, string contentType, Stream stream)
    {
        var ext = Path.GetExtension(fileName);
        if (!AllowedImageExtensions.Contains(ext))
            throw new AppException("Only jpg, jpeg, png, and webp images are allowed.", 400);

        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new AppException("Invalid image content type.", 400);

        if (stream.CanSeek && stream.Length > MaxImageBytes)
            throw new AppException("Image must be 2MB or smaller.", 400);
    }

    private async Task<StudentResponseDto> MapStudentAsync(Student s, int sl, CancellationToken cancellationToken)
    {
        var slug = _tenantContext.TenantSlug;
        string? photo = s.ProfilePictureUrl;
        if (!string.IsNullOrWhiteSpace(photo) && !string.IsNullOrWhiteSpace(slug))
        {
            try { photo = await _storageService.GetPresignedUrlAsync(slug, photo, cancellationToken); }
            catch { /* keep key */ }
        }

        var guardians = new List<GuardianDto>();
        foreach (var g in s.Guardians.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.CreatedAt))
        {
            string? gPhoto = g.ProfilePictureUrl;
            if (!string.IsNullOrWhiteSpace(gPhoto) && !string.IsNullOrWhiteSpace(slug))
            {
                try { gPhoto = await _storageService.GetPresignedUrlAsync(slug, gPhoto, cancellationToken); }
                catch { /* keep key */ }
            }

            guardians.Add(new GuardianDto
            {
                Id = g.Id,
                Name = g.Name,
                Relation = g.Relation,
                FatherName = g.FatherName,
                MotherName = g.MotherName,
                Occupation = g.Occupation,
                Income = g.Income,
                Education = g.Education,
                City = g.City,
                State = g.State,
                MobileNo = g.MobileNo,
                Email = g.Email,
                Address = g.Address,
                ProfilePictureUrl = gPhoto,
                IsPrimary = g.IsPrimary
            });
        }

        return new StudentResponseDto
        {
            Id = s.Id,
            Sl = sl,
            UserId = s.UserId,
            RegisterNo = s.RegisterNo,
            Roll = s.Roll,
            AcademicYear = s.AcademicYear,
            AdmissionDate = s.AdmissionDate,
            ClassId = s.ClassId,
            ClassName = s.Class?.Name,
            SectionId = s.SectionId,
            SectionName = s.Section?.Name,
            CategoryId = s.CategoryId,
            CategoryName = s.Category?.Name,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Gender = s.Gender,
            BloodGroup = s.BloodGroup,
            DateOfBirth = s.DateOfBirth,
            MotherTongue = s.MotherTongue,
            Religion = s.Religion,
            Caste = s.Caste,
            MobileNo = s.MobileNo,
            Email = s.Email,
            City = s.City,
            State = s.State,
            PresentAddress = s.PresentAddress,
            PermanentAddress = s.PermanentAddress,
            ProfilePictureUrl = photo,
            FathersNidNumber = s.FathersNidNumber,
            MothersNidNumber = s.MothersNidNumber,
            BirthRegistrationNumber = s.BirthRegistrationNumber,
            TransportRouteId = s.TransportRouteId,
            TransportRouteName = s.TransportRoute?.Name,
            VehicleNo = s.VehicleNo,
            HostelId = s.HostelId,
            HostelName = s.Hostel?.Name,
            RoomId = s.RoomId,
            RoomName = s.Room?.Name,
            PreviousSchoolName = s.PreviousSchoolName,
            PreviousSchoolQualification = s.PreviousSchoolQualification,
            Remarks = s.Remarks,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            Guardians = guardians
        };
    }
}
