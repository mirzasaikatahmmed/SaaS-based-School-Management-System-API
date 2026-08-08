# 7 — Student List (Student Details)

**Controller:** `StudentListController` · **Route:** `/api/student-list`

Main student directory under Student Details: filter, search, export, bulk delete, login toggle, record deactivate/reactivate.

## Endpoints

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/student-list` | Super Admin, Admin, Teacher | Paginated list (**ClassId required**) |
| GET | `/api/student-list/me` | Student | Own detail |
| GET | `/api/student-list/export` | Super Admin, Admin | csv / excel / pdf |
| GET | `/api/student-list/login-deactivate` | Super Admin, Admin | Legacy list helper (prefer `/api/login-deactivate`) |
| GET | `/api/student-list/deactivate-reasons` | Super Admin, Admin | Currently deactivated *students* (records) |
| POST | `/api/student-list/bulk-delete` | Super Admin, Admin | Soft delete many |
| GET | `/api/student-list/{id}` | Admin+, Teacher, Parent, Student | Full detail |
| PUT | `/api/student-list/{id}` | Super Admin, Admin | Update (`UpdateAdmissionDto`) |
| DELETE | `/api/student-list/{id}` | Super Admin, Admin | Soft delete |
| PUT | `/api/student-list/{id}/toggle-login` | Super Admin, Admin | Toggle `users.active` |
| POST | `/api/student-list/{id}/deactivate` | Super Admin, Admin | Soft deactivate + reason text |
| PUT | `/api/student-list/{id}/activate` | Super Admin, Admin | Re-activate record |

## List columns (conceptual)

Photo, Name, Class, Section, Register No, Roll, DOB, Age (calculated), Gender, Father, Mother, Guardian mobile, Active / Login active.

## Rules

- Default academic year = current year; `SectionId` null = all sections.
- Without `ClassId` → empty list + message *"Class is required for filtering."*
- Soft-deleted / deactivated students (`is_active = false`) excluded from normal list.
- Bulk delete: all IDs must belong to tenant or `403`; returns `{ deleted, failed }`.
- Photos: MinIO presigned URLs (~1 hour).

## Workflow

1. Select Class (+ optional Section / search).
2. View / edit / delete row actions.
3. Bulk select → bulk delete.
4. Deactivate with reason → student leaves normal list; appears under deactivate-reasons list until re-activated.
