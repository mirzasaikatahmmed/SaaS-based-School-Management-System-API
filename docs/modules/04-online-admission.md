# 4 — Online Admission

**Controller:** `OnlineAdmissionController` · **Route:** `/api/online-admission`

Public application funnel + admin review. Approving an application creates a real student via the admission pipeline.

## Public (no JWT)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/online-admission/apply` | Submit application (`X-Tenant-ID` or school slug as required by API) |
| GET | `/api/online-admission/track/{referenceNo}` | Track status |
| GET | `/api/online-admission/lookup/classes/{tenantSlug}` | Classes for apply form |

## Admin / Teacher

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/online-admission` | Super Admin, Admin, Teacher | Filtered list (+ inline export query) |
| GET | `/api/online-admission/export` | Super Admin, Admin | csv / excel / pdf |
| GET | `/api/online-admission/{id}` | Super Admin, Admin, Teacher | Detail |
| GET | `/api/online-admission/{id}/print` | Super Admin, Admin, Teacher | Print payload |
| PUT | `/api/online-admission/{id}/approve` | Super Admin, Admin | Approve → create student |
| PUT | `/api/online-admission/{id}/decline` | Super Admin, Admin | Decline with reason |
| PUT | `/api/online-admission/{id}/payment` | Super Admin, Admin | Record payment |
| DELETE | `/api/online-admission/{id}` | Super Admin, Admin | Delete (not if approved) |

## Workflow

1. Applicant applies and receives a reference number.
2. Admin reviews list → approve / decline / payment.
3. On approve, student + users are created in the tenant schema.
4. Student shows up under Student List.

## Notes

- `reviewed_by` may be Super Admin (stored without FK to tenant `users`).
- Export respects current filters.
