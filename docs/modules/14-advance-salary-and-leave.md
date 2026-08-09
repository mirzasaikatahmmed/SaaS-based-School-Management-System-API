# 14 — Advance Salary & Leave (Human Resource)

**Controllers:** `AdvanceSalaryController`, `LeaveCategoryController`, `LeaveController`

Shared pattern: Category/Setup → My Application (employee) → Manage Application (admin).

## Advance Salary — `/api/advance-salary`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/my` | Any employee | Own requests |
| POST | `/my` | Any employee | Self-apply (employee from JWT) |
| GET | `/` | Admin+, Accountant | Manage list |
| POST | `/` | Admin+, Accountant | Create for employee |
| PUT | `/{id}/approve` | Admin+, Accountant | Approve |
| PUT | `/{id}/reject` | Admin+, Accountant | Reject (+ reason) |
| DELETE | `/{id}` | Admin+, Accountant | Delete (not if Approved) |
| GET | `/export` | Admin+, Accountant | csv / excel / pdf |
| GET | `/lookup/employees?role=` | Admin+, Accountant | Applicant dropdown |

**Rules:** one pending request per employee per deduct month (`YYYY-MM`); approved amounts auto-fill `salary_payments.advance_deduction` when that month is paid.

## Leave categories — `/api/leave-categories`

CRUD + `/lookup?role=` (Admin / Super Admin). Name+Role unique. Cannot delete if in use.

**Seed:** Casual (10), Sick (14), Annual (20) for every employee role.

## Leave — `/api/leave`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET/POST | `/my` | Employee | Own list / apply |
| DELETE | `/my/{id}` | Employee | Cancel pending only |
| GET/POST | `/` | Admin+ | Manage list / admin create |
| PUT | `/{id}/approve` \| `/reject` | Admin+ | Review |
| DELETE | `/{id}` | Admin+ | Delete |
| GET | `/export` | Admin+ | Export |
| POST | `/{id}/attachment` | Admin+ | MinIO ≤5MB |
| GET | `/lookup/leave-types` | Employee | Categories for own role |
| GET | `/lookup/employees?role=` | Admin+ | Applicant dropdown |

**Rules:** days = end − start + 1; quota = category.days − (approved+pending in year); attachment path `leave-attachments/{leaveId}/{filename}`.
