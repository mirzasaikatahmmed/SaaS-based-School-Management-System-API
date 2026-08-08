# 8 — Deactivate Reason Master

**Controller:** `DeactivateReasonController` · **Route:** `/api/deactivate-reasons`  
**Roles:** Super Admin, Admin

Reusable **reason labels** (Transfer, Dropout, Expelled, …) — not the list of deactivated students.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/deactivate-reasons` | List all (ordered by `created_at`) |
| POST | `/api/deactivate-reasons` | Create |
| PUT | `/api/deactivate-reasons/{id}` | Update |
| DELETE | `/api/deactivate-reasons/{id}` | Delete |

## Response fields

| Field | Source |
|-------|--------|
| Branch | `ITenantContext` school name |
| Reason | As entered (case preserved) |
| IsActive | Flag |
| CreatedAt | Timestamp |

## Rules

- Unique per tenant (case-insensitive).
- Cannot delete if linked to any student via `deactivate_reason_id` → `400: Reason is in use by X student(s)`.
- Table: `{schema}.deactivate_reasons`; FK on `students.deactivate_reason_id`.

## Workflow

1. Admin defines reasons first.
2. When deactivating a student record, link or match a master reason where applicable.
3. Student Login Deactivate list may show the reason label when set.
