# SaaS School Management System

Multi-tenant school management API built with **.NET 10** — admissions, students, parents, auth, and file storage. Each school gets an isolated PostgreSQL schema and MinIO bucket.

**Docker image:** [`mirzasaikatahmmed/saas-based-school-management-system-api`](https://hub.docker.com/r/mirzasaikatahmmed/saas-based-school-management-system-api)

---

## Features

| Area | Capabilities |
|------|----------------|
| **Auth & tenancy** | JWT login/refresh, schema-per-tenant (`tenant_{slug}`), per-school MinIO buckets |
| **Schools** | Super Admin school provisioning, branding, stats, export |
| **Admission** | Create/update students, guardians, lookups (class/section/category/hostel/transport) |
| **Online admission** | Public apply/track; admin approve/decline/payment/export |
| **CSV import** | Bulk student import with partial success and failed-row export |
| **Student categories** | Tenant-scoped category CRUD |
| **Student list** | Filter, search, bulk soft-delete, export, login toggle, deactivate/reactivate |
| **Deactivate reasons** | Master reason labels for student deactivation |
| **Login deactivate** | Students / parents with deactivated login; bulk re-activate |
| **Parents** | Guardian directory, standalone add, photo upload, social/alternative fields |

---

## Architecture

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Presentation | `SchoolManagement.API` | Controllers, middleware, filters, DI |
| Business | `SchoolManagement.BLL` | Services, DTOs, validators |
| Data | `SchoolManagement.DAL` | EF Core, repositories, unit of work, schema provisioner |
| Shared | `SchoolManagement.Common` | Constants, wrappers |

**Multi-tenancy:** school registry in `public.tenants`; tenant data in `tenant_{slug}`; files in `school-{slug}` MinIO bucket. Send `X-Tenant-ID: {slug}` on school-scoped requests.

Column naming follows the live ahskbera dump (`password`, `mobileno`, `photo`, `active`, role `prefix`). See [`docs/AHSKBERA_SCHEMA_MAPPING.md`](docs/AHSKBERA_SCHEMA_MAPPING.md).

---

## Quick start

### Option A — Docker Compose (recommended)

```bash
cd SchoolManagement
make pull    # or: make build
make up
```

| Service | URL |
|---------|-----|
| API / Swagger | http://localhost:5000/swagger |
| MinIO console | http://localhost:9001 (`minioadmin` / `minioadmin123`) |
| PostgreSQL | `localhost:5432` (`schooladmin` / `schoolpassword`) |

### Option B — Local API + Docker deps

```bash
cd SchoolManagement
docker compose up -d postgres minio
dotnet run --project SchoolManagement.API
```

### Makefile

| Command | Description |
|---------|-------------|
| `make build` | Build API image locally |
| `make up` | Start postgres + minio + api |
| `make down` | Stop stack |
| `make push` | Push image to Docker Hub |
| `make pull` | Pull image from Docker Hub |
| `make logs` | Follow API logs |
| `make restart` | Rebuild image and recreate API |

```bash
make build TAG=v1.0
make push TAG=v1.0
```

---

## Seeded super admin

| Field | Value |
|-------|-------|
| Email | `superadmin@schoolmgmt.com` |
| Password | `SuperAdmin@123` |

---

## Auth flow

1. `POST /api/auth/login` as super admin (**no** `X-Tenant-ID`)
2. Create a school via tenants/schools API → provisions schema + bucket + school admin
3. School users: `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`

**Roles:** `superadmin`, `admin`, `teacher`, `accountant`, `librarian`, `parent`, `student`, `receptionist`, `staff`

- Custom JWT (no ASP.NET Identity); BCrypt with `$2y$` verify support  
- Access token ~15 min · Refresh token ~7 days (per tenant)

---

## Main API routes

| Prefix | Module |
|--------|--------|
| `/api/auth` | Login, refresh, logout |
| `/api/tenants`, `/api/schools` | Tenant / school management |
| `/api/admission` | Student admission CRUD |
| `/api/online-admissions` | Online applications |
| `/api/student-import` | CSV import |
| `/api/student-categories` | Categories |
| `/api/student-list` | Directory, deactivate, export |
| `/api/deactivate-reasons` | Deactivation reason master |
| `/api/login-deactivate` | Student login deactivate |
| `/api/parents` | Parents / guardians |
| `/api/parent-login-deactivate` | Parent login deactivate |

Interactive docs: **Swagger** at `/swagger` when the API is running.

Per-module workflow docs: [`docs/MODULES.md`](docs/MODULES.md)  
Full system workflow (current + how to extend): [`docs/SYSTEM_WORKFLOW.md`](docs/SYSTEM_WORKFLOW.md)

---

## Tech stack

- .NET 10 / ASP.NET Core  
- PostgreSQL 16 (EF Core + Npgsql)  
- MinIO (presigned URLs)  
- FluentValidation · Docker / Compose  

---

## License

Use and modify as needed for your deployment.
