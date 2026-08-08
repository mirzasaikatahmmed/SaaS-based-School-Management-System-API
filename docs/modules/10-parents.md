# 10 — Parents

**Controller:** `ParentController` · **Route:** `/api/parents`

Management layer over existing `guardians` (created at admission or standalone).

## Endpoints

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/parents` | Super Admin, Admin, Teacher | Paginated list |
| GET | `/api/parents/me` | Parent | Own profile + linked students |
| GET | `/api/parents/export` | Super Admin, Admin | csv / excel / pdf |
| GET | `/api/parents/{id}` | Admin+, Teacher, Parent | Detail |
| POST | `/api/parents` | Super Admin, Admin | Standalone add + user account |
| PUT | `/api/parents/{id}` | Super Admin, Admin | Update |
| DELETE | `/api/parents/{id}` | Super Admin, Admin | Soft delete |
| POST | `/api/parents/{id}/photo` | Super Admin, Admin | Photo → MinIO `guardians/{id}/profile.*` |

## List columns

Sl, Branch, Guardian Name, Occupation, **Reference No** (UI label often “Mobile No”: `2026001`), Email, Login active.

## Add parent payload (highlights)

- Required: Name, Relation, Occupation, MobileNo, Username, Password, RetypePassword
- Optional: Father/Mother, income, education, address
- Alternative parent: name / relation / mobile
- Social: FacebookUrl, TwitterUrl, LinkedInUrl
- Optional `StudentIds` to link wards

## Rules

- Auto `reference_no` = `{year}{seq}` (e.g. `2026001`), unique, never reused.
- Creates `users` + role `parent`.
- Soft delete sets guardian `is_active = false` and user inactive.
- Cannot delete if sole active guardian for any linked student.
- Photo: jpg/jpeg/png/webp, max 2MB; old object deleted before replace.

## Workflow

1. Guardians from admission already appear in the list (refs backfilled).
2. Admin adds standalone parent → login credentials + optional student links.
3. Detail shows wards; parent uses `/api/parents/me`.
