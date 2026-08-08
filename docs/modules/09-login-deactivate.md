# 9 — Student Login Deactivate

**Controller:** `LoginDeactivateController` · **Route:** `/api/login-deactivate`  
**Roles:** Super Admin, Admin

Filtered view of students whose **login** is off (`users.active = false`). Student record (`students.is_active`) is unchanged.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/login-deactivate` | List (Class/Section/Search; `?export=` for file) |
| PUT | `/api/login-deactivate/{studentId}/activate` | Enable login |
| PUT | `/api/login-deactivate/{studentId}/deactivate` | Disable login |
| POST | `/api/login-deactivate/bulk-activate` | Activate selected → `{ activated, failed }` |

## List columns

Photo, Name, Register No, Roll, Guardian Name, Class, Deactivate Reason (if any), Email, Mobile, IsLoginActive.

## Rules

- List **only** login-deactivated students.
- Bulk activate requires all IDs in current tenant (`403` otherwise).
- Export respects filters.

## Workflow

1. Toggle login off from Student List (or deactivate endpoint here).
2. Student appears on this page.
3. Single activate or bulk **Authentication Activate** → disappears from list.
