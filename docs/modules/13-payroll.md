# 13 — Payroll (Human Resource)

**Controllers:** `SalaryTemplateController`, `SalaryAssignController`, `SalaryPaymentController`

Flow: create salary grade template → assign to employees → process monthly payment.

## Routes

| Area | Base path |
|------|-----------|
| Salary templates | `/api/payroll/salary-templates` |
| Salary assign | `/api/payroll/salary-assign` |
| Salary payment | `/api/payroll/salary-payment` |
| My salary | `/api/payroll/my-salary` |

All require `Authorization` + `X-Tenant-ID`.

## Roles

| Role | Access |
|------|--------|
| Super Admin / School Admin / Accountant | Full payroll CRUD, assign, pay, export |
| Any employee (authenticated) | `GET /api/payroll/my-salary` and `.../my-salary/{month}` |

## Salary templates

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/payroll/salary-templates` | List |
| GET | `/api/payroll/salary-templates/lookup` | Dropdown (id + grade) |
| GET | `/api/payroll/salary-templates/{id}` | Detail + allowance/deduction rows |
| POST | `/api/payroll/salary-templates` | Create |
| PUT | `/api/payroll/salary-templates/{id}` | Update (replace rows) |
| DELETE | `/api/payroll/salary-templates/{id}` | Delete if not assigned |

**Net salary (server):** `Basic + ΣAllowances − ΣDeductions`

## Salary assign

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/payroll/salary-assign?role=` | Employees + current grade (`role` required) |
| PUT | `/api/payroll/salary-assign/{employeeId}` | Upsert grade |
| POST | `/api/payroll/salary-assign/bulk` | Bulk upsert |

## Salary payment

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/payroll/salary-payment?role=&paymentMonth=` | Status list (`YYYY-MM`) |
| POST | `/api/payroll/salary-payment/{employeeId}/pay` | Mark Paid (snapshot template) |
| PUT | `/api/payroll/salary-payment/{id}/update` | Update overtime/advance/note |
| GET | `/api/payroll/salary-payment/{id}` | Detail |
| GET | `/api/payroll/salary-payment/export` | csv / excel / pdf |
| GET | `/api/payroll/my-salary` | Own template |
| GET | `/api/payroll/my-salary/{month}` | Own payment for month |

**Final amount:** `net_salary + (overtime_hours × overtime_rate) − advance_deduction`

## Rules

- Grade name unique (case-insensitive); cannot delete template in use.
- One assignment per employee (upsert).
- One payment per employee per month; duplicate pay → 400.
- No grade → list status `No Grade Assigned` (pay disabled).
- Seed: grade `Basic` with basic salary `0`.

## Workflow

1. Create / edit salary template with dynamic allowances & deductions.
2. Assign grade (or bulk) by Role (+ optional Designation).
3. Filter payment by Role + Month → Pay → appears as Paid with date.
