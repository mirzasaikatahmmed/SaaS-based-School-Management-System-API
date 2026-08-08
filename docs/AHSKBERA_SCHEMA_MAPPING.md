# ahskbera_main.sql → SaaS schema mapping

Live dump: [`ahskbera_main.sql`](../ahskbera_main.sql) (MySQL 8 / InnoDB, ~179 tables).  
This SaaS API keeps **PostgreSQL schema-per-tenant** for isolation, but **auth column naming, roles, and login semantics** follow the live dump.

> For product workflows see [`SYSTEM_WORKFLOW.md`](./SYSTEM_WORKFLOW.md).  
> This file is only about **naming / auth alignment** with ahskbera — not the full feature catalog.

---

## Conventions retained from ahskbera

| Convention | Live (MySQL) | SaaS (PostgreSQL) |
|---|---|---|
| Naming | `snake_case` | `snake_case` |
| Password column | `login_credential.password` | `users.password` |
| Phone | `mobileno` | `users.mobileno` |
| Avatar | `photo` | `users.photo` |
| Active flag | `active` tinyint(1) | `users.active` boolean |
| Last login | `last_login` | `users.last_login` |
| Timestamps | `created_at` / `updated_at` | same (timestamptz) |
| Roles | `roles(name, prefix, is_system)` | same |
| Login audit | `login_log` | `login_log` |
| BCrypt | `$2y$10$...` (PHP) | verifies `$2y$`; new hashes use work factor 12 |

---

## Role IDs / prefixes (from `ahskbera_main.roles`)

| id | name | prefix |
|---|---|---|
| 1 | Super Admin | `superadmin` |
| 2 | Admin | `admin` |
| 3 | Teacher | `teacher` |
| 4 | Accountant | `accountant` |
| 5 | Librarian | `librarian` |
| 6 | Parent | `parent` |
| 7 | Student | `student` |
| 8 | Receptionist | `receptionist` |
| 9 | Staff | `staff` |

JWT `[Authorize(Roles=...)]` uses **prefix** values.

---

## Isolation model difference

| | Live | SaaS |
|---|---|---|
| Tenancy | Shared DB + `branch_id` | Schema-per-tenant (`tenant_{slug}`) |
| Primary keys | `int AUTO_INCREMENT` | `UUID` |
| Engine | MySQL InnoDB | PostgreSQL 16 |
| Files | Local / mixed | MinIO bucket `school-{slug}` |

`branch` ≈ SaaS `public.tenants` (school registry).

---

## Auth mapping

```
ahskbera.login_credential  →  tenant.users  (+ user_roles)
ahskbera.roles             →  tenant.roles
ahskbera.login_log         →  tenant.login_log
```

Login accepts email/username in the tenant. Super Admin lives in the master DB (not tenant `users`).

---

## Domain tables (implemented in SaaS — inspired by ahskbera, not 1:1 copies)

| Domain | Tenant tables (high level) | Notes vs ahskbera |
|--------|----------------------------|-------------------|
| Academics masters | `classes`, `sections`, `student_categories` | Seeded lookups |
| Students | `students` | UUID PKs; `register_no`, soft `is_active`, deactivate fields |
| Guardians / parents | `guardians` | Linked to students + optional `users`; `reference_no`, social/alternative fields |
| Online admit | `online_admissions` | Public apply → approve creates student |
| Import | `import_batches`, `import_batch_rows` | CSV pipeline |
| Deactivate labels | `deactivate_reasons` | Master reasons; FK on students |
| Lookups | `transport_routes`, `hostels`, `hostel_rooms` | Seed / admission support |

Student/guardian **user** accounts still use ahskbera-style `users` columns (`password`, `mobileno`, `photo`, `active`).

---

## What this doc is not

- Not a full MySQL→PostgreSQL migration of all ~179 ahskbera tables.
- Not the product workflow guide (use [`SYSTEM_WORKFLOW.md`](./SYSTEM_WORKFLOW.md) + [`MODULES.md`](./MODULES.md)).
- Future fees / attendance / exams should prefer ahskbera names where practical, adapted to UUID + schema-per-tenant.

---

## When to update this file

- Auth/user/role column renames
- New role prefixes
- Mapping a new ahskbera table into a tenant schema  
Do **not** duplicate feature docs here — link module docs instead.
