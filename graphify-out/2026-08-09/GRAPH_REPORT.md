# Graph Report - SchoolManagement  (2026-08-09)

## Corpus Check
- 68 files · ~17,849 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 655 nodes · 1582 edges · 23 communities (22 shown, 1 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 94 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- Tenant
- User
- SchoolManagement.DAL.Context
- UnitOfWork
- UserProfileDto
- SchoolManagement.BLL
- .GetAll
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
- MasterDbContext
- ServiceCollectionExtensions
- SchoolRepository
- UserRoleEnum.cs
- .OnActionExecutionAsync

## God Nodes (most connected - your core abstractions)
1. `Tenant` - 36 edges
2. `SchoolService` - 27 edges
3. `ApiResponse` - 25 edges
4. `User` - 23 edges
5. `IUserRepository` - 22 edges
6. `UserRepository` - 21 edges
7. `SchoolManagement.DAL.Context` - 19 edges
8. `ITenantRepository` - 19 edges
9. `SchoolResponseDto` - 17 edges
10. `TenantResponseDto` - 17 edges

## Surprising Connections (you probably didn't know these)
- `SchoolController` --references--> `ITenantContext`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.DAL/TenantContext/ITenantContext.cs
- `AuthService` --references--> `ITenantRepository`  [EXTRACTED]
  SchoolManagement.BLL/Services/AuthService.cs → SchoolManagement.DAL/Repositories/Interfaces/ITenantRepository.cs
- `AuthService` --references--> `ITenantContext`  [EXTRACTED]
  SchoolManagement.BLL/Services/AuthService.cs → SchoolManagement.DAL/TenantContext/ITenantContext.cs
- `AuthService` --references--> `IUnitOfWork`  [EXTRACTED]
  SchoolManagement.BLL/Services/AuthService.cs → SchoolManagement.DAL/UnitOfWork/IUnitOfWork.cs
- `SchoolService` --references--> `MasterDbContext`  [EXTRACTED]
  SchoolManagement.BLL/Services/SchoolService.cs → SchoolManagement.DAL/Context/MasterDbContext.cs

## Import Cycles
- None detected.

## Communities (23 total, 1 thin omitted)

### Community 0 - "Tenant"
Cohesion: 0.08
Nodes (30): IAsyncDisposable, IServiceScopeFactory, DateTime, Guid, TenantResponseDto, CancellationToken, ILogger, IReadOnlyList (+22 more)

### Community 1 - "User"
Cohesion: 0.10
Nodes (22): ICollection, DbSet, ModelBuilder, string, TenantDbContext, DateTime, Guid, LoginLog (+14 more)

### Community 2 - "SchoolManagement.DAL.Context"
Cohesion: 0.08
Nodes (27): SchoolManagement.BLL.Interfaces, SchoolManagement.DAL.UnitOfWork, SchoolManagement.BLL.DTOs.Tenant, SchoolManagement.DAL.Entities.Master, SchoolManagement.Common.Constants, SchoolManagement.Common.Wrappers, SchoolManagement.BLL.Settings, SchoolManagement.BLL.DTOs.School (+19 more)

### Community 3 - "UnitOfWork"
Cohesion: 0.09
Nodes (16): bool, HashSet, HttpContext, ILogger, RequestDelegate, Task, TenantResolutionMiddleware, IConfiguration (+8 more)

### Community 4 - "UserProfileDto"
Cohesion: 0.09
Nodes (35): AllowAnonymous, IsSuperAdmin, ActionResult, Authorize, CancellationToken, Guid, HttpGet, HttpPost (+27 more)

### Community 5 - "SchoolManagement.BLL"
Cohesion: 0.06
Nodes (36): BCrypt.Net-Next (4.2.0), FluentValidation (12.1.1), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.Http.Abstractions (2.3.11), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Tools (10.0.10), Microsoft.Extensions.Configuration.Abstractions (10.0.10) (+28 more)

### Community 6 - ".GetAll"
Cohesion: 0.13
Nodes (22): ControllerBase, ActionResult, Authorize, CancellationToken, HttpDelete, HttpGet, HttpPost, HttpPut (+14 more)

### Community 7 - "InitialMaster"
Cohesion: 0.07
Nodes (17): SchoolManagement.DAL.Migrations.Master, SchoolManagement.DAL.Migrations.Tenant, Migration, ModelSnapshot, MigrationBuilder, ModelBuilder, InitialMaster, MigrationBuilder (+9 more)

### Community 8 - "StorageService"
Cohesion: 0.15
Nodes (15): ExpiresAt, IEnumerable, IMinioClient, DateTime, Guid, JwtHelper, CancellationToken, ILogger (+7 more)

### Community 9 - "ApiResponse"
Cohesion: 0.09
Nodes (34): IActionResult, IFormFile, RequestSizeLimit, ActionResult, Authorize, CancellationToken, HttpDelete, HttpGet (+26 more)

### Community 10 - "School Management System — Foundation (Authentication + Multi-Tenancy)"
Cohesion: 0.14
Nodes (12): ahskbera_main.sql → SaaS Schema Format Mapping, Auth mapping, Conventions retained from ahskbera, Isolation model difference, Role IDs / prefixes (from `ahskbera_main.roles`), Architecture, Auth flow, Notes (+4 more)

### Community 11 - "AbstractValidator"
Cohesion: 0.19
Nodes (14): AbstractValidator, SchoolManagement.BLL.Validators, CreateTenantValidator, UpdateTenantSettingsValidator, HashSet, LoginValidator, RefreshTokenValidator, RegisterValidator (+6 more)

### Community 12 - "http"
Cohesion: 0.18
Nodes (10): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, profiles (+2 more)

### Community 13 - "SchoolService"
Cohesion: 0.09
Nodes (26): Guid, IReadOnlyList, SchoolListResponseDto, SchoolResponseDto, CancellationToken, Stream, Task, IStorageService (+18 more)

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
Cohesion: 0.24
Nodes (6): IServiceProvider, ILogger, Task, StartupExtensions, PasswordHelper, WebApplication

### Community 18 - "MasterDbContext"
Cohesion: 0.13
Nodes (12): DbContext, IConfigurationRoot, IDesignTimeDbContextFactory, MasterDbContextFactory, TenantDesignTimeDbContextFactory, DbSet, ModelBuilder, MasterDbContext (+4 more)

### Community 19 - "ServiceCollectionExtensions"
Cohesion: 0.53
Nodes (3): IServiceCollection, IConfiguration, ServiceCollectionExtensions

### Community 20 - "SchoolRepository"
Cohesion: 0.27
Nodes (7): CancellationToken, Guid, IReadOnlyList, Items, Task, TotalCount, SchoolRepository

### Community 22 - ".OnActionExecutionAsync"
Cohesion: 0.33
Nodes (5): ActionExecutingContext, ActionExecutionDelegate, IAsyncActionFilter, Task, ValidationFilter

## Knowledge Gaps
- **54 isolated node(s):** `$schema`, `commandName`, `dotnetRunMessages`, `launchBrowser`, `launchUrl` (+49 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SchoolManagement.DAL.Context` connect `SchoolManagement.DAL.Context` to `MasterDbContext`, `InitialMaster`?**
  _High betweenness centrality (0.123) - this node is a cross-community bridge._
- **Why does `SchoolService` connect `SchoolService` to `ApiResponse`, `SchoolManagement.DAL.Context`, `MasterDbContext`?**
  _High betweenness centrality (0.093) - this node is a cross-community bridge._
- **Why does `Tenant` connect `Tenant` to `MasterDbContext`, `SchoolRepository`, `SchoolService`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **What connects `$schema`, `commandName`, `dotnetRunMessages` to the rest of the system?**
  _54 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.08005427408412483 - nodes in this community are weakly interconnected._
- **Should `User` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `SchoolManagement.DAL.Context` be split into smaller, more focused modules?**
  _Cohesion score 0.07909604519774012 - nodes in this community are weakly interconnected._