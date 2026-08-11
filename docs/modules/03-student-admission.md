# 3 — Student Admission

**Controllers:** `AdmissionController` (`/api/admission`), `AdmissionLookupController` (`/api/admission/lookup`)

Creates students (and optional guardian user accounts) inside the current tenant. Atomic create: student user + student row + guardian(s).

**SSC board numbers (optional):** For **class 9 or 10** only, you *may* set `sscRoll` and/or `sscRegistrationNo` on create/update. Both fields are optional — leave blank if not available yet. Other classes reject non-empty values; moving a student out of 9/10 clears them.

## Lookups — `/api/admission/lookup`

| Method | Path | Description |
|--------|------|-------------|
| GET | `.../academic-years` | Years |
| GET | `.../classes` | Classes |
| GET | `.../sections/{classId}` | Sections for class |
| GET | `.../categories` | Student categories |
| GET | `.../transport-routes` | Transport |
| GET | `.../hostels` | Hostels |
| GET | `.../hostel-rooms/{hostelId}` | Rooms |
| GET | `.../next-register-no` | Suggested register number |

Roles: Super Admin, Admin, Teacher, Receptionist.

## Admission — `/api/admission`

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/admission` | Admin+, Teacher, Parent, Student | List / filter students |
| GET | `/api/admission/{id}` | Same | Detail (scoped for parent/student) |
| POST | `/api/admission` | Super Admin, Admin | Create admission |
| PUT | `/api/admission/{id}` | Super Admin, Admin | Update |
| DELETE | `/api/admission/{id}` | Super Admin, Admin | Soft delete (+ deactivate login) |
| POST | `/api/admission/{id}/profile-picture` | Super Admin, Admin | Student photo → MinIO |
| POST | `/api/admission/{id}/guardian-picture` | Super Admin, Admin | Guardian photo → MinIO |

## Workflow

1. Load lookups (class → section → category).
2. `POST /api/admission` with academic + personal + optional guardian login fields.
3. Upload photos if needed.
4. Student appears in Student List / Parents (via guardian rows).

## Notes

- Creates tenant `users` with role `student` / `parent` when credentials provided.
- Guardians get auto `reference_no` when saved (Parents module).
