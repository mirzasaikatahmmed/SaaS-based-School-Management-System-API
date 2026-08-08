# ahskbera_main.sql → SaaS Schema Format Mapping

Live dump: `ahskbera_main.sql` (MySQL 8 / InnoDB / utf8mb3, ~179 tables).  
SaaS foundation keeps **PostgreSQL schema-per-tenant** for isolation, but **column naming, roles, and auth semantics** follow the live dump.

## Conventions retained from ahskbera

| Convention | Live (MySQL) | SaaS (PostgreSQL) |
|---|---|---|
| Naming | `snake_case` | `snake_case` |
| Password column | `login_credential.password` | `users.password` |
| Phone | `mobileno` | `mobileno` |
| Avatar | `photo` | `photo` |
| Active flag | `active` tinyint(1) | `active` boolean |
| Last login | `last_login` | `last_login` |
| Timestamps | `created_at` timestamp, `updated_at` datetime NULL | `created_at`, `updated_at` (nullable) |
| Roles | `roles(name, prefix, is_system)` | same columns |
| Login audit | `login_log` | `login_log` |
| BCrypt | `$2y$10$...` (PHP) | verifies `$2y$` + hashes with work factor 12 |

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

## Isolation model difference

| | Live | SaaS |
|---|---|---|
| Tenancy | Shared DB + `branch_id` | Schema-per-tenant (`tenant_{slug}`) |
| Primary keys | `int AUTO_INCREMENT` | `UUID` (safer for distributed SaaS) |
| Engine | MySQL InnoDB | PostgreSQL 16 |

`branch` ≈ SaaS `public.tenants` (school registry). Future modules (student, staff, attendance, fees…) should reuse live table/column names inside each tenant schema.

## Auth mapping

```
ahskbera.login_credential  →  tenant.users  (+ user_roles)
ahskbera.roles             →  tenant.roles
ahskbera.login_log         →  tenant.login_log
ahskbera.staff / student   →  (later modules; not in auth foundation)
```

Login accepts email; live usernames (register_no / mobileno) can be added as alternate login identifiers later.
