# Graph Report - SchoolManagement  (2026-08-09)

## Corpus Check
- 96 files · ~28,823 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 936 nodes · 2488 edges · 27 communities (26 shown, 1 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 137 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Tenant
- TenantDbContext
- SchoolManagement.DAL.Repositories.Interfaces
- UnitOfWork
- UserProfileDto
- SchoolManagement.BLL
- TenantResponseDto
- InitialMaster
- StorageService
- ApiResponse
- School Management System — Foundation (Authentication + Multi-Tenancy)
- AbstractValidator
- http
- SchoolService
- AppException
- .HandleExceptionAsync
- AppConstants
- .SeedSuperAdminAsync
- MasterDbContextFactory
- ServiceCollectionExtensions
- IAdmissionLookupRepository
- UserRoleEnum.cs
- .OnActionExecutionAsync
- StudentService
- AdmissionLookupItemDto
- Student
- Guardian

## God Nodes (most connected - your core abstractions)
1. `ApiResponse` - 40 edges
2. `Tenant` - 36 edges
3. `StudentService` - 35 edges
4. `Student` - 31 edges
5. `TenantDbContext` - 28 edges
6. `SchoolService` - 27 edges
7. `User` - 25 edges
8. `SchoolManagement.DAL.Repositories.Interfaces` - 25 edges
9. `SchoolManagement.DAL.Context` - 24 edges
10. `SchoolManagement.DAL.Entities.Tenant` - 23 edges

## Surprising Connections (you probably didn't know these)
- `AdmissionController` --references--> `IStudentService`  [EXTRACTED]
  SchoolManagement.API/Controllers/AdmissionController.cs → SchoolManagement.BLL/Interfaces/IStudentService.cs
- `SchoolController` --references--> `ISchoolService`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.BLL/Interfaces/ISchoolService.cs
- `SchoolController` --references--> `ITenantContext`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.DAL/TenantContext/ITenantContext.cs
- `TenantController` --references--> `ITenantService`  [EXTRACTED]
  SchoolManagement.API/Controllers/TenantController.cs → SchoolManagement.BLL/Interfaces/ITenantService.cs
- `AuthService` --references--> `ITenantRepository`  [EXTRACTED]
  SchoolManagement.BLL/Services/AuthService.cs → SchoolManagement.DAL/Repositories/Interfaces/ITenantRepository.cs

## Import Cycles
- None detected.

## Communities (27 total, 1 thin omitted)

### Community 0 - "Tenant"
Cohesion: 0.07
Nodes (31): DbContext, DbSet, ModelBuilder, MasterDbContext, SchoolEntity, DateTime, Guid, BrandingSettings (+23 more)

### Community 1 - "TenantDbContext"
Cohesion: 0.10
Nodes (22): DbSet, ModelBuilder, string, TenantDbContext, DateTime, Guid, ICollection, LoginLog (+14 more)

### Community 2 - "SchoolManagement.DAL.Repositories.Interfaces"
Cohesion: 0.08
Nodes (27): SchoolManagement.BLL.Interfaces, SchoolManagement.DAL.UnitOfWork, SchoolManagement.BLL.DTOs.Tenant, SchoolManagement.DAL.Entities.Master, SchoolManagement.Common.Constants, SchoolManagement.Common.Wrappers, SchoolManagement.BLL.Settings, SchoolManagement.BLL.DTOs.School (+19 more)

### Community 3 - "UnitOfWork"
Cohesion: 0.09
Nodes (17): bool, IDbContextTransaction, HashSet, HttpContext, ILogger, RequestDelegate, Task, TenantResolutionMiddleware (+9 more)

### Community 4 - "UserProfileDto"
Cohesion: 0.07
Nodes (45): AllowAnonymous, ExpiresAt, IEnumerable, IsSuperAdmin, ActionResult, Authorize, CancellationToken, Guid (+37 more)

### Community 5 - "SchoolManagement.BLL"
Cohesion: 0.06
Nodes (36): BCrypt.Net-Next (4.2.0), FluentValidation (12.1.1), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.Http.Abstractions (2.3.11), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Tools (10.0.10), Microsoft.Extensions.Configuration.Abstractions (10.0.10) (+28 more)

### Community 6 - "TenantResponseDto"
Cohesion: 0.09
Nodes (28): IAsyncDisposable, IServiceScopeFactory, DateTime, Guid, BrandingSettingsDto, CreateTenantAdminDto, CreateTenantDto, FeatureSettingsDto (+20 more)

### Community 7 - "InitialMaster"
Cohesion: 0.06
Nodes (20): SchoolManagement.DAL.Migrations.Master, SchoolManagement.DAL.Migrations.Tenant, Migration, ModelSnapshot, MigrationBuilder, ModelBuilder, InitialMaster, MigrationBuilder (+12 more)

### Community 8 - "StorageService"
Cohesion: 0.35
Nodes (6): IMinioClient, CancellationToken, ILogger, Stream, Task, StorageService

### Community 9 - "ApiResponse"
Cohesion: 0.10
Nodes (38): ControllerBase, IActionResult, ActionResult, Authorize, CancellationToken, Guid, HttpDelete, HttpGet (+30 more)

### Community 10 - "School Management System — Foundation (Authentication + Multi-Tenancy)"
Cohesion: 0.14
Nodes (12): ahskbera_main.sql → SaaS Schema Format Mapping, Auth mapping, Conventions retained from ahskbera, Isolation model difference, Role IDs / prefixes (from `ahskbera_main.roles`), Architecture, Auth flow, Notes (+4 more)

### Community 11 - "AbstractValidator"
Cohesion: 0.09
Nodes (23): AbstractValidator, SchoolManagement.BLL.Validators, DateTime, Guid, CreateAdmissionDto, Guid, GuardianDto, DateTime (+15 more)

### Community 12 - "http"
Cohesion: 0.18
Nodes (10): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, profiles (+2 more)

### Community 13 - "SchoolService"
Cohesion: 0.07
Nodes (41): BrandingSettings, DateTime, FeatureSettings, Guid, IReadOnlyList, SecuritySettings, BrandingSettings, CreateSchoolDto (+33 more)

### Community 14 - "AppException"
Cohesion: 0.39
Nodes (7): Exception, List, AppException, ConflictException, ForbiddenException, NotFoundException, UnauthorizedException

### Community 15 - ".HandleExceptionAsync"
Cohesion: 0.36
Nodes (6): Exception, HttpContext, ILogger, RequestDelegate, Task, ExceptionHandlingMiddleware

### Community 16 - "AppConstants"
Cohesion: 0.48
Nodes (6): int, string, AppConstants, Claims, Roles, StorageFolders

### Community 17 - ".SeedSuperAdminAsync"
Cohesion: 0.38
Nodes (5): IServiceProvider, ILogger, Task, StartupExtensions, WebApplication

### Community 18 - "MasterDbContextFactory"
Cohesion: 0.33
Nodes (4): IConfigurationRoot, IDesignTimeDbContextFactory, MasterDbContextFactory, TenantDesignTimeDbContextFactory

### Community 19 - "ServiceCollectionExtensions"
Cohesion: 0.53
Nodes (3): IServiceCollection, IConfiguration, ServiceCollectionExtensions

### Community 20 - "IAdmissionLookupRepository"
Cohesion: 0.07
Nodes (36): SchoolManagement.BLL.Mappings, AdmissionMappings, DateTime, Guid, ICollection, ClassEntity, DateTime, Guid (+28 more)

### Community 22 - ".OnActionExecutionAsync"
Cohesion: 0.33
Nodes (5): ActionExecutingContext, ActionExecutionDelegate, IAsyncActionFilter, Task, ValidationFilter

### Community 23 - "StudentService"
Cohesion: 0.11
Nodes (20): IHttpContextAccessor, long, IReadOnlyList, StudentListResponseDto, DateTime, Guid, IReadOnlyList, StudentResponseDto (+12 more)

### Community 24 - "AdmissionLookupItemDto"
Cohesion: 0.17
Nodes (16): ActionResult, CancellationToken, Guid, HttpGet, IReadOnlyList, Task, AdmissionLookupController, Guid (+8 more)

### Community 25 - "Student"
Cohesion: 0.13
Nodes (19): DateTime, Guid, ICollection, Student, CancellationToken, Guid, IReadOnlyList, Items (+11 more)

### Community 26 - "Guardian"
Cohesion: 0.19
Nodes (13): DateTime, Guid, Guardian, CancellationToken, Guid, IReadOnlyList, Task, GuardianRepository (+5 more)

## Knowledge Gaps
- **55 isolated node(s):** `$schema`, `commandName`, `dotnetRunMessages`, `launchBrowser`, `launchUrl` (+50 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SchoolManagement.DAL.Context` connect `SchoolManagement.DAL.Repositories.Interfaces` to `MasterDbContextFactory`, `TenantResponseDto`, `InitialMaster`?**
  _High betweenness centrality (0.109) - this node is a cross-community bridge._
- **Why does `TenantDbContext` connect `TenantDbContext` to `Tenant`, `SchoolManagement.DAL.Repositories.Interfaces`, `UnitOfWork`, `SchoolService`, `MasterDbContextFactory`, `IAdmissionLookupRepository`, `Student`, `Guardian`?**
  _High betweenness centrality (0.105) - this node is a cross-community bridge._
- **Why does `StudentService` connect `StudentService` to `AdmissionLookupItemDto`, `SchoolManagement.DAL.Repositories.Interfaces`, `UnitOfWork`, `TenantResponseDto`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **What connects `$schema`, `commandName`, `dotnetRunMessages` to the rest of the system?**
  _55 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.07067603160667252 - nodes in this community are weakly interconnected._
- **Should `TenantDbContext` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `SchoolManagement.DAL.Repositories.Interfaces` be split into smaller, more focused modules?**
  _Cohesion score 0.07715260017050299 - nodes in this community are weakly interconnected._