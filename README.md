# School Management System — Foundation (Authentication + Multi-Tenancy)

SaaS school platform built with .NET 10, 3-layer architecture, schema-per-tenant PostgreSQL isolation, and per-tenant MinIO buckets.

**Schema naming follows the live `ahskbera_main.sql` dump** (snake_case columns, `password` / `mobileno` / `photo` / `active` / `last_login`, role `prefix` values, `login_log`). See [`docs/AHSKBERA_SCHEMA_MAPPING.md`](docs/AHSKBERA_SCHEMA_MAPPING.md).

## Architecture

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Presentation | `SchoolManagement.API` | Controllers, middleware, filters, DI |
| Business | `SchoolManagement.BLL` | Auth/Tenant/Storage services, DTOs, validators |
| Data | `SchoolManagement.DAL` | EF Core contexts, repositories, unit of work |
| Shared | `SchoolManagement.Common` | Constants, enums, `ApiResponse` wrapper |

**Multi-tenancy:** each school gets `tenant_{slug}` PostgreSQL schema + `school-{slug}` MinIO bucket. Tenant registry lives in `public.tenants`.

## Quick start

```bash
cd SchoolManagement
docker compose up -d postgres minio
dotnet run --project SchoolManagement.API
```

- API / Swagger: http://localhost:5000/swagger  
- MinIO console: http://localhost:9001 (`minioadmin` / `minioadmin123`)  

### Seeded super admin

| Field | Value |
|-------|-------|
| Email | `superadmin@schoolmgmt.com` |
| Password | `SuperAdmin@123` |

## Roles (ahskbera prefixes)

`superadmin`, `admin`, `teacher`, `accountant`, `librarian`, `parent`, `student`, `receptionist`, `staff`

## Auth flow

1. `POST /api/auth/login` as super admin (no `X-Tenant-ID`)
2. `POST /api/tenants` with Bearer token → creates schema + MinIO bucket + school admin
3. School users: send header `X-Tenant-ID: {slug}`

## Notes

- Custom JWT auth; BCrypt with `$2y$` (PHP) verify support
- Access token 15 min · Refresh token 7 days (per tenant schema)
- Knowledge graph: `graphify update SchoolManagement --force` from repo root
