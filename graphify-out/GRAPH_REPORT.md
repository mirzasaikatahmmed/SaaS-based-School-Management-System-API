# Graph Report - SchoolManagement  (2026-08-09)

## Corpus Check
- 125 files · ~42,431 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1254 nodes · 3418 edges · 49 communities (48 shown, 1 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 190 edges (avg confidence: 0.8)
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
- TenantResponseDto
- InitialMaster
- StorageService
- ApiResponse
- School Management System — Foundation (Authentication + Multi-Tenancy)
- ImportBatch
- http
- SchoolResponseDto
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
- StudentImportService
- SchoolController
- SchoolService
- ITenantSchemaProvisioner
- .Import
- CsvImportHelper
- IStorageService
- ISchoolRepository
- IStudentImportService
- SchoolManagement.DAL.Migrations.Tenant
- IUnitOfWork
- Migration
- MasterDbContextModelSnapshot.cs
- StudentImportRowDto
- .ExportAsync
- ImportBatchResponseDto
- ImportResultDto
- SchoolSettingsDto
- AddAdmissionModule
- AddStudentImportModule
- ExtendTenantsWithSchoolFields
- ITenantDbContextFactory

## God Nodes (most connected - your core abstractions)
1. `ApiResponse` - 52 edges
2. `Tenant` - 38 edges
3. `StudentService` - 37 edges
4. `Student` - 37 edges
5. `OnlineAdmissionService` - 35 edges
6. `TenantDbContext` - 33 edges
7. `SchoolManagement.DAL.Entities.Tenant` - 33 edges
8. `SchoolManagement.DAL.Context` - 30 edges
9. `SchoolManagement.DAL.Repositories.Interfaces` - 30 edges
10. `SchoolService` - 27 edges

## Surprising Connections (you probably didn't know these)
- `AdmissionController` --references--> `IStudentService`  [EXTRACTED]
  SchoolManagement.API/Controllers/AdmissionController.cs → SchoolManagement.BLL/Interfaces/IStudentService.cs
- `AdmissionLookupController` --references--> `IStudentService`  [EXTRACTED]
  SchoolManagement.API/Controllers/AdmissionLookupController.cs → SchoolManagement.BLL/Interfaces/IStudentService.cs
- `OnlineAdmissionController` --references--> `IOnlineAdmissionService`  [EXTRACTED]
  SchoolManagement.API/Controllers/OnlineAdmissionController.cs → SchoolManagement.BLL/Interfaces/IOnlineAdmissionService.cs
- `SchoolController` --references--> `ISchoolService`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.BLL/Interfaces/ISchoolService.cs
- `SchoolController` --references--> `ITenantContext`  [EXTRACTED]
  SchoolManagement.API/Controllers/SchoolController.cs → SchoolManagement.DAL/TenantContext/ITenantContext.cs

## Import Cycles
- None detected.

## Communities (49 total, 1 thin omitted)

### Community 0 - "Tenant"
Cohesion: 0.07
Nodes (29): IConfigurationRoot, IDesignTimeDbContextFactory, MasterDbContextFactory, TenantDesignTimeDbContextFactory, DbSet, ModelBuilder, MasterDbContext, SchoolEntity (+21 more)

### Community 1 - "TenantDbContext"
Cohesion: 0.10
Nodes (23): DbContext, DbSet, ModelBuilder, string, TenantDbContext, DateTime, Guid, ICollection (+15 more)

### Community 2 - "SchoolManagement.DAL.Entities.Tenant"
Cohesion: 0.05
Nodes (47): AbstractValidator, SchoolManagement.BLL.Interfaces, SchoolManagement.DAL.UnitOfWork, SchoolManagement.BLL.Mappings, SchoolManagement.BLL.DTOs.Tenant, SchoolManagement.DAL.Entities.Master, SchoolManagement.BLL.DTOs.Import, SchoolManagement.Common.Constants (+39 more)

### Community 3 - "UnitOfWork"
Cohesion: 0.10
Nodes (15): bool, IDbContextTransaction, HashSet, HttpContext, ILogger, RequestDelegate, Task, TenantResolutionMiddleware (+7 more)

### Community 4 - ".SaveTenantChangesAsync"
Cohesion: 0.06
Nodes (45): IsSuperAdmin, ActionResult, AllowAnonymous, Authorize, CancellationToken, Guid, HttpGet, HttpPost (+37 more)

### Community 5 - "SchoolManagement.BLL"
Cohesion: 0.06
Nodes (37): BCrypt.Net-Next (4.2.0), CsvHelper (33.1.0), FluentValidation (12.1.1), FluentValidation.AspNetCore (11.3.1), Microsoft.AspNetCore.Authentication.JwtBearer (10.0.10), Microsoft.AspNetCore.Http.Abstractions (2.3.11), Microsoft.EntityFrameworkCore.Relational (10.0.10), Microsoft.EntityFrameworkCore.Tools (10.0.10) (+29 more)

### Community 6 - "TenantResponseDto"
Cohesion: 0.10
Nodes (30): IServiceScopeFactory, ActionResult, Authorize, CancellationToken, HttpDelete, HttpGet, HttpPost, HttpPut (+22 more)

### Community 7 - "InitialMaster"
Cohesion: 0.22
Nodes (4): SchoolManagement.DAL.Migrations.Master, MigrationBuilder, ModelBuilder, InitialMaster

### Community 8 - "StorageService"
Cohesion: 0.15
Nodes (15): ExpiresAt, IMinioClient, DateTime, Guid, IEnumerable, JwtHelper, CancellationToken, ILogger (+7 more)

### Community 9 - "ApiResponse"
Cohesion: 0.11
Nodes (36): ControllerBase, ActionResult, Authorize, CancellationToken, Guid, HttpDelete, HttpGet, HttpPost (+28 more)

### Community 10 - "School Management System — Foundation (Authentication + Multi-Tenancy)"
Cohesion: 0.14
Nodes (12): ahskbera_main.sql → SaaS Schema Format Mapping, Auth mapping, Conventions retained from ahskbera, Isolation model difference, Role IDs / prefixes (from `ahskbera_main.roles`), Architecture, Auth flow, Notes (+4 more)

### Community 11 - "ImportBatch"
Cohesion: 0.10
Nodes (25): DateTime, Guid, ICollection, string, ImportBatch, ImportBatchStatuses, DateTime, Guid (+17 more)

### Community 12 - "http"
Cohesion: 0.18
Nodes (10): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, launchUrl, profiles (+2 more)

### Community 13 - "SchoolResponseDto"
Cohesion: 0.16
Nodes (16): DateTime, Guid, IReadOnlyList, CreateSchoolDto, SchoolListResponseDto, SchoolResponseDto, SchoolStatsDto, UpdateSchoolDto (+8 more)

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
Cohesion: 0.07
Nodes (36): Random, Guid, ApproveAdmissionDto, DeclineAdmissionDto, Guid, OnlineAdmissionFilterDto, DateTime, Guid (+28 more)

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
Nodes (33): Guid, AdmissionLookupItemDto, NextRegisterNoDto, DateTime, Guid, CreateAdmissionDto, Guid, GuardianDto (+25 more)

### Community 24 - "OnlineAdmission"
Cohesion: 0.12
Nodes (21): DateTime, Guid, string, OnlineAdmission, OnlineAdmissionPaymentStatuses, OnlineAdmissionStatuses, CancellationToken, Guid (+13 more)

### Community 25 - "Student"
Cohesion: 0.12
Nodes (19): DateTime, Guid, ICollection, Student, CancellationToken, Guid, IReadOnlyList, Items (+11 more)

### Community 26 - "Guardian"
Cohesion: 0.19
Nodes (13): DateTime, Guid, Guardian, CancellationToken, Guid, IReadOnlyList, Task, GuardianRepository (+5 more)

### Community 27 - "StudentImportService"
Cohesion: 0.21
Nodes (11): CancellationToken, Content, Dictionary, FileName, Guid, HashSet, IHttpContextAccessor, ILogger (+3 more)

### Community 28 - "SchoolController"
Cohesion: 0.27
Nodes (12): ActionResult, Authorize, CancellationToken, HttpDelete, HttpGet, HttpPost, HttpPut, IActionResult (+4 more)

### Community 29 - "SchoolService"
Cohesion: 0.27
Nodes (6): CancellationToken, DateTime, ILogger, Stream, Task, SchoolService

### Community 30 - "ITenantSchemaProvisioner"
Cohesion: 0.33
Nodes (4): CancellationToken, Task, ITenantSchemaProvisioner, TenantSchemaProvisioner

### Community 31 - ".Import"
Cohesion: 0.27
Nodes (10): ActionResult, CancellationToken, Guid, HttpGet, HttpPost, IActionResult, IFormFile, RequestSizeLimit (+2 more)

### Community 32 - "CsvImportHelper"
Cohesion: 0.21
Nodes (4): IEnumerable, long, string, CsvImportHelper

### Community 33 - "IStorageService"
Cohesion: 0.35
Nodes (4): CancellationToken, Stream, Task, IStorageService

### Community 34 - "ISchoolRepository"
Cohesion: 0.28
Nodes (7): CancellationToken, Guid, IReadOnlyList, Items, Task, TotalCount, ISchoolRepository

### Community 35 - "IStudentImportService"
Cohesion: 0.29
Nodes (7): CancellationToken, Content, FileName, Guid, Stream, Task, IStudentImportService

### Community 36 - "SchoolManagement.DAL.Migrations.Tenant"
Cohesion: 0.22
Nodes (4): SchoolManagement.DAL.Migrations.Tenant, MigrationBuilder, ModelBuilder, InitialTenant_AhskberaFormat

### Community 37 - "IUnitOfWork"
Cohesion: 0.39
Nodes (4): IAsyncDisposable, CancellationToken, Task, IUnitOfWork

### Community 38 - "Migration"
Cohesion: 0.25
Nodes (4): Migration, MigrationBuilder, ModelBuilder, AddOnlineAdmissionModule

### Community 39 - "MasterDbContextModelSnapshot.cs"
Cohesion: 0.22
Nodes (5): ModelSnapshot, ModelBuilder, MasterDbContextModelSnapshot, ModelBuilder, TenantDbContextModelSnapshot

### Community 40 - "StudentImportRowDto"
Cohesion: 0.25
Nodes (5): Dictionary, StudentImportRowDto, Dictionary, List, Stream

### Community 41 - ".ExportAsync"
Cohesion: 0.50
Nodes (4): Content, ContentType, FileName, IReadOnlyList

### Community 42 - "ImportBatchResponseDto"
Cohesion: 0.33
Nodes (6): DateTime, Guid, IReadOnlyList, List, ImportBatchListResponseDto, ImportBatchResponseDto

### Community 43 - "ImportResultDto"
Cohesion: 0.29
Nodes (5): Guid, List, ImportResultDto, Dictionary, ImportRowResultDto

### Community 44 - "SchoolSettingsDto"
Cohesion: 0.29
Nodes (7): BrandingSettings, FeatureSettings, SecuritySettings, BrandingSettings, FeatureSettings, SchoolSettingsDto, SecuritySettings

### Community 45 - "AddAdmissionModule"
Cohesion: 0.33
Nodes (3): MigrationBuilder, ModelBuilder, AddAdmissionModule

### Community 46 - "AddStudentImportModule"
Cohesion: 0.33
Nodes (3): MigrationBuilder, ModelBuilder, AddStudentImportModule

### Community 47 - "ExtendTenantsWithSchoolFields"
Cohesion: 0.40
Nodes (3): MigrationBuilder, ModelBuilder, ExtendTenantsWithSchoolFields

### Community 48 - "ITenantDbContextFactory"
Cohesion: 0.50
Nodes (3): IConfiguration, ITenantDbContextFactory, TenantDbContextFactory

## Knowledge Gaps
- **55 isolated node(s):** `$schema`, `commandName`, `dotnetRunMessages`, `launchBrowser`, `launchUrl` (+50 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **1 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TenantDbContext` connect `TenantDbContext` to `Tenant`, `SchoolManagement.DAL.Entities.Tenant`, `UnitOfWork`, `ImportBatch`, `ITenantDbContextFactory`, `IAdmissionLookupRepository`, `OnlineAdmission`, `Student`, `Guardian`, `SchoolService`?**
  _High betweenness centrality (0.117) - this node is a cross-community bridge._
- **Why does `SchoolManagement.DAL.Context` connect `SchoolManagement.DAL.Entities.Tenant` to `Tenant`, `SchoolManagement.DAL.Migrations.Tenant`, `Migration`, `MasterDbContextModelSnapshot.cs`, `InitialMaster`, `AddStudentImportModule`, `ITenantDbContextFactory`, `ITenantSchemaProvisioner`?**
  _High betweenness centrality (0.096) - this node is a cross-community bridge._
- **Why does `ITenantContext` connect `UnitOfWork` to `TenantDbContext`, `.SaveTenantChangesAsync`, `ITenantDbContextFactory`, `OnlineAdmissionService`, `StudentService`, `StudentImportService`, `SchoolController`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **What connects `$schema`, `commandName`, `dotnetRunMessages` to the rest of the system?**
  _55 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.07247223845704266 - nodes in this community are weakly interconnected._
- **Should `TenantDbContext` be split into smaller, more focused modules?**
  _Cohesion score 0.09726775956284153 - nodes in this community are weakly interconnected._
- **Should `SchoolManagement.DAL.Entities.Tenant` be split into smaller, more focused modules?**
  _Cohesion score 0.05128205128205128 - nodes in this community are weakly interconnected._