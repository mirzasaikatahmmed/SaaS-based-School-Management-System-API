# 6 — Student Categories

**Controller:** `StudentCategoryController` · **Route:** `/api/student-categories`

Tenant-scoped category master used on admission (e.g. COMMON, GENERAL).

## Endpoints

| Method | Path | Roles | Description |
|--------|------|-------|-------------|
| GET | `/api/student-categories` | Super Admin, Admin, Teacher | List (includes Branch = school name) |
| POST | `/api/student-categories` | Super Admin, Admin | Create |
| PUT | `/api/student-categories/{id}` | Super Admin, Admin | Update |
| DELETE | `/api/student-categories/{id}` | Super Admin, Admin | Delete |

## Rules

- Names stored **uppercase**; unique per tenant (case-insensitive).
- Cannot delete if any student still uses the category → `400` with in-use count.
- Seeded defaults: COMMON, GENERAL, FREEDOM FIGHTER, TRIBAL, SPECIAL NEEDS.

## Workflow

1. Admin creates categories before or during admission season.
2. Admission / import / online approve assign `categoryId`.
