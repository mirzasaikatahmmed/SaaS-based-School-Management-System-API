# 12 — Employees

**Controllers:** `EmployeeController`, `DepartmentController`, `DesignationController`, `EmployeeLoginDeactivateController`

Staff module: departments, designations, employee CRUD (+ CSV import / photo), role-tabbed list, and login deactivate.

## Routes

| Area | Base path |
|------|-----------|
| Departments | `/api/departments` |
| Designations | `/api/designations` |
| Employees | `/api/employees` |
| Login deactivate | `/api/employee-login-deactivate` |

All require `Authorization` + `X-Tenant-ID`.

## Roles

| Role | Access |
|------|--------|
| Super Admin / School Admin | Full CRUD, import, login activate/deactivate |
| Teacher | `GET /api/employees/me` only |
| Others | No access |

## Employee endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/employees` | Paginated list (`?role=Teacher` tab filter) |
| GET | `/api/employees/{id}` | Detail |
| GET | `/api/employees/me` | Own profile (Teacher) |
| POST | `/api/employees` | Add (creates `users` + `employees`, auto `staff_id`) |
| PUT | `/api/employees/{id}` | Update |
| DELETE | `/api/employees/{id}` | Soft delete |
| POST | `/api/employees/{id}/photo` | Profile photo → MinIO `employees/{id}/profile.*` |
| POST | `/api/employees/{id}/signature` | Signature image → MinIO `employees/{id}/signature.*` |
| GET | `/api/employees/export` | csv / excel / pdf |
| POST | `/api/employees/import` | CSV bulk (partial success) |
| GET | `/api/employees/import/sample-csv` | Sample CSV |
| GET | `/api/employees/import/batches` | Import batches |
| GET | `/api/employees/import/batches/{id}` | Batch + rows |
| GET | `/api/employees/lookup/roles` | Role list |
| GET | `/api/employees/lookup/departments` | Active departments |
| GET | `/api/employees/lookup/designations` | Active designations |

## Login deactivate

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/employee-login-deactivate?role=` | **Role required** — login-inactive employees |
| PUT | `.../{id}/activate` | `users.active = true` |
| PUT | `.../{id}/deactivate` | `users.active = false` |
| POST | `.../bulk-activate` | Batch activate in one transaction |

## Rules

- **Staff ID:** `{A-Z}{6 digits}` (e.g. `R568131`), unique per tenant.
- **Bank:** if `SkipBankDetails = false` → BankName, HolderName, BankBranch, AccountNo required; if true → bank fields stored null.
- **Create:** atomic user + employee; username unique; email unique on employees.
- **Department / Designation:** name unique (case-insensitive); cannot delete while in use.
- **Seed on provision:** departments MATHEMATICS, ENGLISH, BANGLA, SCIENCE, SOCIAL SCIENCE, ICT, PHYSICS; designations HEAD MASTER, ASSISTANT HEAD MASTER, ASSISTANT TEACHER, ADMIN.
- **Photo / signature:** jpg/jpeg/png/webp, max 2MB; old object deleted before replace; 1h presigned URLs in responses.

## Workflow

1. Ensure departments / designations (seeded or add via masters).
2. Add employee (or CSV import) → appears under role tab.
3. Optional photo and signature upload.
4. Deactivate login → appears in Login Deactivate for that role → activate / bulk-activate.
