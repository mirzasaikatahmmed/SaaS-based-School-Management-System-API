# 2 — Tenants & Schools

**Controllers:** `TenantController` (`/api/tenants`), `SchoolController` (`/api/schools`)

Provisions a school as a SaaS tenant: PostgreSQL schema `tenant_{slug}` + MinIO bucket `school-{slug}` + seeded school admin.

## Tenants — `/api/tenants`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| POST | `/api/tenants` | Super Admin | Create tenant (schema + bucket + admin) |
| GET | `/api/tenants` | Super Admin | List tenants |
| GET | `/api/tenants/{slug}` | Auth | Get tenant |
| PUT | `/api/tenants/{slug}/settings` | Super Admin, Admin | Update settings |
| DELETE | `/api/tenants/{slug}` | Super Admin | Remove tenant |

## Schools — `/api/schools`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/schools` | Super Admin | Paginated list (+ export) |
| GET | `/api/schools/{slug}` | Auth | School detail |
| POST | `/api/schools` | Super Admin | Create school |
| PUT | `/api/schools/{slug}` | Super Admin, Admin | Update |
| DELETE | `/api/schools/{slug}` | Super Admin | Soft/hard remove |
| PUT | `/api/schools/{slug}/activate` | Super Admin | Re-activate |
| POST | `/api/schools/{slug}/logo` | Super Admin, Admin | Upload logo → MinIO |
| GET/PUT | `/api/schools/{slug}/settings` | Super Admin, Admin | Branding / features / security |
| GET | `/api/schools/{slug}/stats` | Super Admin, Admin | Users, storage, subscription |

## Workflow

1. Super Admin creates school/tenant.
2. Platform runs DDL provisioner + creates MinIO bucket.
3. School admin credentials are seeded; admin logs in with `X-Tenant-ID`.

## Notes

- Provisioning order: schema/bucket **before** seeding admin (admin seed needs the schema).
- Tenant registry lives in `public.tenants`.
