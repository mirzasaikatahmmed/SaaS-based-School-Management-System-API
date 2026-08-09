# 22 — Student Accounting

Fees types/groups, fine setup, allocation, invoices, offline payments, reminders.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** `PATCH` (no PUT)

**Migration:** `20260809182321_AddStudentAndOfficeAccounting`  
**Provisioner:** `EnsureStudentAndOfficeAccountingModuleAsync`  
**Seed fees types:** Monthly Fee, Exam Fee, Admission Fee

## Routes (`/api/student-accounting/...`)

| Area | Prefix | Notes |
|------|--------|-------|
| Payment types | `/payment-types` | CRUD |
| Offline payments | `/offline-payments` | Submit; PATCH approve/reject; TrxId auto |
| Fees types | `/fees-types` | Unique fee code; lookup |
| Fees groups | `/fees-groups` | Items replace-on-update; lookup |
| Fine setups | `/fine-setups` | One fine per group+type |
| Allocation | `/fees-allocation` | Auto-generates invoices for class students |
| Invoices | `/invoices` | List, generate, PATCH pay, CSV export |
| Due invoices | `/due-invoices` | Unpaid/partial overdue |
| Reminders | `/reminders` | Frequency + notify student/guardian |

## Auth

Admin / Super Admin / Accountant → full  
Student / Parent → submit offline payment; view own invoices
