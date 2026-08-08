# Graph Report - SchoolManagement  (2026-08-09)

## Corpus Check
- 111 files · ~35,608 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1097 nodes · 3025 edges · 27 communities (26 shown, 1 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 171 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `3fe6359a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- Tenant
- TenantDbContext
- SchoolManagement.DAL.Entities.Tenant
- UnitOfWork
- .SaveTenantChangesAsync
- SchoolManagement.BLL
- IUnitOfWork
- SchoolManagement.DAL.Migrations.Tenant
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
- OnlineAdmissionService
- ServiceCollectionExtensions
- IAdmissionLookupRepository
- UserRoleEnum.cs
- .OnActionExecutionAsync
- StudentService
- OnlineAdmission
- Student
- Guardian

## God Nodes (most connected - your core abstractions)
1. `ApiResponse` - 49 edges
2. `Tenant` - 38 edges
3. `StudentService` - 36 edges
4. `OnlineAdmissionService` - 35 edges
5. `Student` - 34 edges
6. `TenantDbContext` - 30 edges
7. `SchoolManagement.DAL.Entities.Tenant` - 28 edges
8. `SchoolManagement.DAL.Repositories.Interfaces` - 28 edges
9. `SchoolService` - 27 edges
10. `SchoolManagement.DAL.Context` - 27 edges

## Surprising Connections (you probably didn't know these)
- `AdmissionController` --references--> `IStudentService`  [EXTRACTED]
  SchoolManagement.API/Controllers/AdmissionController.cs → SchoolManagement.BLL/Interfaces/IStudentService.cs
- `AdmissionLookupController` --references--> `IStudentService`  [EXTRACTED]
  SchoolManagement.API/Controllers/AdmissionLookupController.cs → SchoolManagement.BLL/Interfaces/IStudentService.cs
- `SchoolController` --references--> `ISchoolService`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.BLL/Interfaces/ISchoolService.cs
- `SchoolController` --references--> `ITenantContext`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.DAL/TenantContext/ITenantContext.cs
- `TenantController` --references--> `ITenantService`  [EXTRACTED]
  SchoolManagement.API/Controllers/TenantController.cs → SchoolManagement.BLL/Interfaces/ITenantService.cs

## Import Cycles
- None detected.

## Communities (27 total, 1 thin omitted)

### Community 0 - "Tenant"
Cohesion: 0.06
Nodes (35): DbContext, IConfigurationRoot, IDesignTimeDbContextFactory, MasterDbContextFactory, TenantDesignTimeDbContextFactory, DbSet, ModelBuilder, MasterDbContext (+27 more)

### Community 1 - "TenantDbContext"
Cohesion: 0.10
Nodes (22): DbSet, ModelBuilder, string, TenantDbContext, DateTime, Guid, ICollection, LoginLog (+14 more)

### Community 2 - "SchoolManagement.DAL.Entities.Tenant"
Cohesion: 0.06
Nodes (34): SchoolManagement.BLL.Interfaces, SchoolManagement.DAL.UnitOfWork, SchoolManagement.BLL.Mappings, SchoolManagement.BLL.DTOs.Tenant, SchoolManagement.DAL.Entities.Master, SchoolManagement.Common.Constants, SchoolManagement.Common.Wrappers, SchoolManagement.BLL.Settings (+26 more)

### Community 3 - "UnitOfWork"
Cohesion: 0.09
Nodes (18): bool, IDbContextTransaction, HashSet, HttpContext, ILogger, RequestDelegate, Task, TenantResolutionMiddleware (+10 more)

### Community 4 - ".SaveTenantChangesAsync"
Cohesion: 0.07
Nodes (45): ExpiresAt, IEnumerable, IsSuperAdmin, ActionResult, AllowAnonymous, Authorize, CancellationToken, Guid (+37 more)

### Community 5 - "SchoolManagement.BLL"
Cohesion: 0.06
Nodes (36): BCrypt.Net-Next (4.2.0), FluentValidation (12.1.1), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.Http.Abstractions (2.3.11), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Tools (10.0.10), Microsoft.Extensions.Configuration.Abstractions (10.0.10) (+28 more)

### Community 6 - "IUnitOfWork"
Cohesion: 0.08
Nodes (28): IAsyncDisposable, IServiceScopeFactory, DateTime, Guid, BrandingSettingsDto, CreateTenantAdminDto, CreateTenantDto, FeatureSettingsDto (+20 more)

### Community 7 - "SchoolManagement.DAL.Migrations.Tenant"
Cohesion: 0.06
Nodes (18): SchoolManagement.DAL.Migrations.Master, SchoolManagement.DAL.Migrations.Tenant, Migration, MigrationBuilder, ModelBuilder, InitialMaster, MigrationBuilder, ModelBuilder (+10 more)

### Community 8 - "StorageService"
Cohesion: 0.35
Nodes (6): IMinioClient, CancellationToken, ILogger, Stream, Task, StorageService

### Community 9 - "ApiResponse"
Cohesion: 0.09
Nodes (45): ControllerBase, ActionResult, Authorize, CancellationToken, Guid, HttpDelete, HttpGet, HttpPost (+37 more)

### Community 10 - "School Management System — Foundation (Authentication + Multi-Tenancy)"
Cohesion: 0.14
Nodes (12): ahskbera_main.sql → SaaS Schema Format Mapping, Auth mapping, Conventions retained from ahskbera, Isolation model difference, Role IDs / prefixes (from `ahskbera_main.roles`), Architecture, Auth flow, Notes (+4 more)

### Community 11 - "AbstractValidator"
Cohesion: 0.14
Nodes (19): AbstractValidator, SchoolManagement.BLL.Validators, HashSet, CreateAdmissionValidator, UpdateAdmissionValidator, CreateTenantValidator, UpdateTenantSettingsValidator, LoginValidator (+11 more)

### Community 12 - "http"
Cohesion: 0.18
Nodes (10): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, profiles (+2 more)

### Community 13 - "SchoolService"
Cohesion: 0.06
Nodes (44): BrandingSettings, DateTime, FeatureSettings, Guid, IReadOnlyList, SecuritySettings, BrandingSettings, CreateSchoolDto (+36 more)

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

### Community 18 - "OnlineAdmissionService"
Cohesion: 0.06
Nodes (49): Random, ActionResult, AllowAnonymous, Authorize, CancellationToken, Guid, HttpDelete, HttpGet (+41 more)

### Community 19 - "ServiceCollectionExtensions"
Cohesion: 0.53
Nodes (3): IServiceCollection, IConfiguration, ServiceCollectionExtensions

### Community 20 - "IAdmissionLookupRepository"
Cohesion: 0.07
Nodes (35): AdmissionMappings, DateTime, Guid, ICollection, ClassEntity, DateTime, Guid, ICollection (+27 more)

### Community 22 - ".OnActionExecutionAsync"
Cohesion: 0.33
Nodes (5): ActionExecutingContext, ActionExecutionDelegate, IAsyncActionFilter, Task, ValidationFilter

### Community 23 - "StudentService"
Cohesion: 0.07
Nodes (33): long, Guid, AdmissionLookupItemDto, NextRegisterNoDto, DateTime, Guid, CreateAdmissionDto, Guid (+25 more)

### Community 24 - "OnlineAdmission"
Cohesion: 0.12
Nodes (21): DateTime, Guid, string, OnlineAdmission, OnlineAdmissionPaymentStatuses, OnlineAdmissionStatuses, CancellationToken, Guid (+13 more)

### Community 25 - "Student"
Cohesion: 0.13
Nodes (19): DateTime, Guid, ICollection, Student, CancellationToken, Guid, IReadOnlyList, Items (+11 more)

### Community 26 - "Guardian"
Cohesion: 0.19
Nodes (13): DateTime, Guid, Guardian, CancellationToken, Guid, IReadOnlyList, Task, GuardianRepository (+5 more)

## Knowledge Gaps
- **54 isolated node(s):** `$schema`, `commandName`, `dotnetRunMessages`, `launchBrowser`, `launchUrl` (+49 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SchoolManagement.DAL.Context` connect `SchoolManagement.DAL.Entities.Tenant` to `Tenant`, `UnitOfWork`, `IUnitOfWork`, `SchoolManagement.DAL.Migrations.Tenant`?**
  _High betweenness centrality (0.130) - this node is a cross-community bridge._
- **Why does `OnlineAdmissionService` connect `OnlineAdmissionService` to `SchoolManagement.DAL.Entities.Tenant`, `UnitOfWork`, `IUnitOfWork`, `SchoolService`, `StudentService`?**
  _High betweenness centrality (0.103) - this node is a cross-community bridge._
- **Why does `TenantDbContext` connect `TenantDbContext` to `Tenant`, `SchoolManagement.DAL.Entities.Tenant`, `UnitOfWork`, `SchoolService`, `IAdmissionLookupRepository`, `OnlineAdmission`, `Student`, `Guardian`?**
  _High betweenness centrality (0.094) - this node is a cross-community bridge._
- **What connects `$schema`, `commandName`, `dotnetRunMessages` to the rest of the system?**
  _54 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.05980861244019139 - nodes in this community are weakly interconnected._
- **Should `TenantDbContext` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `SchoolManagement.DAL.Entities.Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.06368330464716007 - nodes in this community are weakly interconnected._