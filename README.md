# SaaS School Management System

Multi-tenant school management API built with **.NET 10**. Each school gets an isolated PostgreSQL schema (`tenant_{slug}`) and MinIO bucket (`school-{slug}`).

**Docker image:** [`mirzasaikatahmmed/saas-based-school-management-system-api`](https://hub.docker.com/r/mirzasaikatahmmed/saas-based-school-management-system-api)

---

## Features

| Area | Capabilities |
|------|----------------|
| **Auth & tenancy** | JWT login/refresh, schema-per-tenant, per-school MinIO buckets |
| **Schools** | Super Admin provisioning, branding, settings, stats, export |
| **Admission** | Create/update students & guardians; class/section/category lookups |
| **Online admission** | Public apply/track; admin approve/decline/payment/export |
| **CSV import** | Bulk student import with partial success and failed-row export |
| **Student categories** | Tenant-scoped category CRUD |
| **Student list** | Filter, search, soft-delete, export, login toggle, deactivate/reactivate |
| **Deactivate reasons** | Master reason labels for student deactivation |
| **Login deactivate** | Student / parent login enable/disable (bulk supported) |
| **Parents** | Guardian directory, standalone add, photo upload |
| **Employees** | Departments, designations, staff CRUD/import, login deactivate |
| **Payroll** | Salary templates, assign grades, monthly payment, my-salary |
| **Advance salary & leave** | Advance requests; leave categories & applications |
| **Awards** | Give awards to employees or students |
| **Academic** | Classes, sections, subjects, class teachers, timetables, promotion |
| **Exam Master** | Terms, halls, mark distributions, exam setup, schedules, mark entries |
| **Grades & positions** | Grade ranges (A+–F); auto-generate ranks/GPA/PASS-FAIL from mark entries |
| **Attendance** | Student, employee (by role), and exam-subject attendance + reports |
| **Library** | Categories, books + cover upload, issue/return, fine, my issues |
| **Events** | Event types (icons), events, publish/website toggles, public list |
| **Student accounting** | Fees types/groups, fine, allocation, invoices, offline payments, reminders |
| **Office accounting** | Voucher heads, accounts, deposits/expenses + attachments, transactions (Dr/Cr/running balance) |
| **Messages** | Inbox, sent, important, trash, reply, attachments, unread count |
| **Global settings** | Platform-wide institute, currency, timezone, upload limits (Super Admin) |
| **School settings** | Per-school general/panel/payment gateways + logo uploads |
| **Biometric attendance** | ZKTeco K40-H multi-device ADMS push protocol; auto student/employee/exam attendance from fingerprint punches |
| **Settings module** | Roles/permissions matrix, sessions, cron jobs, DB backup, login log, attendance type, accounting links |
| **Email gateway** | Per-school SMTP (TLS/SSL), encrypted password, event templates + test send |
| **SMS gateway** | BulkSMSBD.net (default): send / send-many / balance, event templates + test SMS |
| **Student reports** | Login credentials (student + parent passwords), admission, class/section, siblings |

---

## Architecture

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Presentation | `SchoolManagement.API` | Controllers, middleware, filters, DI, Swagger |
| Business | `SchoolManagement.BLL` | Services, DTOs, FluentValidation |
| Data | `SchoolManagement.DAL` | EF Core, repositories, unit of work, schema provisioner |
| Shared | `SchoolManagement.Common` | Constants, wrappers |

**Multi-tenancy:** school registry in `public.tenants`; tenant data in `tenant_{slug}`; files in `school-{slug}`. Send `X-Tenant-ID: {slug}` on school-scoped requests.

Column naming follows the live ahskbera dump (`password`, `mobileno`, `photo`, `active`, role `prefix`). See [`docs/AHSKBERA_SCHEMA_MAPPING.md`](docs/AHSKBERA_SCHEMA_MAPPING.md).

**HTTP convention:** updates use **`PATCH`** (no `PUT` endpoints).

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

> Docker Compose uses the Hub image. For the latest local code (Exam Master, PATCH, Swagger counter), run **Option B** or `make build` / `make restart` after updating the Dockerfile build path.

### Option B — Local API + Docker deps

```bash
cd SchoolManagement
docker compose up -d postgres minio
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5000 \
  dotnet run --project SchoolManagement.API --no-launch-profile
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
| `/api/auth` | Login, refresh, logout, profile |
| `/api/tenants`, `/api/schools` | Tenant / school management |
| `/api/admission` | Student admission |
| `/api/online-admissions` | Online applications |
| `/api/student-import` | CSV import |
| `/api/student-categories` | Categories |
| `/api/student-list` | Directory, deactivate, export |
| `/api/deactivate-reasons` | Deactivation reason master |
| `/api/login-deactivate` | Student login deactivate |
| `/api/parents` | Parents / guardians |
| `/api/parent-login-deactivate` | Parent login deactivate |
| `/api/departments`, `/api/designations`, `/api/employees` | Staff HR masters |
| `/api/payroll` | Salary templates, assign, payment |
| `/api/advance-salary`, `/api/leave` | Advance salary & leave |
| `/api/awards` | Awards |
| `/api/academic` | Classes, sections, subjects, schedules, promotion |
| `/api/exam` | Terms, halls, distributions, exams, schedules, mark entries |
| `/api/marks` | Grade ranges, exam positions |
| `/api/attendance` | Student / employee / exam attendance |
| `/api/library` | Categories, books, issues |
| `/api/events` | Event types, events (+ `/public`) |
| `/api/student-accounting` | Fees, invoices, offline payments, reminders |
| `/api/office-accounting` | Accounts, deposits, expenses, transactions |
| `/api/messages` | Mailbox (inbox/sent/important/trash) |
| `/api/settings/global` | Platform global settings (Super Admin) |
| `/api/settings/school` | Per-school settings, logos, attendance type, accounting links, **email/SMS gateways** |
| `/api/roles`, `/api/sessions` | Role permission matrix; academic sessions |
| `/api/settings/cron`, `/cron_api` | Cron secret + scheduled SMS/email/fees/homework jobs |
| `/api/settings/backup` | Tenant schema backup / restore (MinIO) |
| `/api/settings/login-log` | Staff / student / parent login audit |
| `/api/reports/students` | Login credentials (+ passwords), admission, class/section, sibling reports |
| `/api/reports/attendance` | Student / subject / employee / exam / fingerprint attendance reports |
| `/api/reports/hr` | Leave reports + payroll summary |
| `/api/academic/student-electives` | Per-student 4th / optional subject (Higher Math vs Agriculture, etc.) |
| `/api/reports/examination` | Report card, tabulation sheet, progress reports |
| `/api/attendance/subject` | Subject-wise attendance capture (for subject reports) |
| `/api/biometric` | Devices, PIN↔person maps, punch logs (Admin) |
| `/iclock` | ZKTeco ADMS device push endpoint (anonymous, plain text) |

Interactive docs: **Swagger** at http://localhost:5000/swagger (Development).  
Swagger title/banner shows a live **endpoint counter** (GET / POST / PATCH / DELETE breakdown).

Per-module docs: [`docs/MODULES.md`](docs/MODULES.md) (modules **1–35**)  
- Email gateway: [`docs/modules/29-email-gateway.md`](docs/modules/29-email-gateway.md)  
- SMS gateway (BulkSMSBD): [`docs/modules/30-sms-gateway.md`](docs/modules/30-sms-gateway.md)  
- Student reports: [`docs/modules/31-student-reports.md`](docs/modules/31-student-reports.md)  
- Attendance reports: [`docs/modules/32-attendance-reports.md`](docs/modules/32-attendance-reports.md)  
- HR reports: [`docs/modules/33-hr-reports.md`](docs/modules/33-hr-reports.md)  
- Examination reports: [`docs/modules/34-examination-reports.md`](docs/modules/34-examination-reports.md)  
- Student electives: [`docs/modules/35-student-electives.md`](docs/modules/35-student-electives.md)  
- Public website (frontend endpoints): [`docs/modules/36-public-website.md`](docs/modules/36-public-website.md)  
System workflow: [`docs/SYSTEM_WORKFLOW.md`](docs/SYSTEM_WORKFLOW.md)

---

## Tech stack

- .NET 10 / ASP.NET Core  
- PostgreSQL 16 (EF Core + Npgsql)  
- MinIO (presigned URLs)  
- FluentValidation · Swashbuckle · Docker / Compose  

---

## License

**Proprietary — All Rights Reserved.**  
You may **not** use this software for free or for any purpose without prior written permission. See [LICENSE](./LICENSE) and [SECURITY.md](./SECURITY.md).

To request permission: contact the repository owner on GitHub.
