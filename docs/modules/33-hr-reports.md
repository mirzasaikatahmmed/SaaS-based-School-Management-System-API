# 33 — HR Reports (Leave Reports, Payroll Summary)

Portal screens under **Reports → Human Resource**.

**Headers:** Bearer + `X-Tenant-ID`  
**Base path:** `/api/reports/hr`  
**Roles:** Super Admin, School Admin, Accountant

---

## Leave Reports

Matches portal **Leave List**: Role + date range → applicants and leave windows.

| Method | Path | Permission |
|--------|------|------------|
| GET | `/leave?role=&fromDate=&toDate=&status=&search=&page=&pageSize=&export=csv` | `Reports.Leave` View |

Date filter uses **overlap** with leave period (`dateOfStart`…`dateOfEnd`).

Columns: `sl`, `role`, `applicant`, `leaveCategory`, `dateOfStart`, `dateOfEnd`, `days`, `applyDate`, `status`.

---

## Payroll Summary

Matches portal **Payroll Summary** (`/payroll/salary_statement`): month → salary sheet with footer totals.

| Method | Path | Permission |
|--------|------|------------|
| GET | `/payroll-summary?month=YYYY-MM&role=&search=&page=&pageSize=&export=csv` | `Reports.PayrollSummary` View |

| Column | Source |
|--------|--------|
| `salary` | Payment basic (or template basic if unpaid) |
| `allowance` | Total allowance |
| `deduction` | Total deduction |
| `netSalary` | Final amount if paid, else template net |
| `payVia` | Payment method (null until paid) |
| `status` | Paid / Unpaid / No Grade Assigned |

Response includes page totals: `totalSalary`, `totalAllowance`, `totalDeduction`, `totalNetSalary`.

Existing pay/generate flow remains under `/api/payroll/salary-payment`.
