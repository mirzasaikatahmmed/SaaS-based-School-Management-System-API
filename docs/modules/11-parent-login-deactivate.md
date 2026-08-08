# 11 — Parent Login Deactivate

**Controller:** `ParentLoginDeactivateController` · **Route:** `/api/parent-login-deactivate`  
**Roles:** Super Admin, Admin

Parents whose **login** is deactivated (`users.active = false`). Guardian record stays intact.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/parent-login-deactivate` | List (Search only; `?export=` for file) |
| PUT | `/api/parent-login-deactivate/{id}/activate` | Enable login |
| PUT | `/api/parent-login-deactivate/{id}/deactivate` | Disable login |
| POST | `/api/parent-login-deactivate/bulk-activate` | Bulk enable → `{ activated, failed }` |

## List columns

Checkbox | Guardian Name | Occupation | Mobile No (real phone) | Email | Action

## Rules

- No Class/Section filter (simpler than student login deactivate).
- Activate/deactivate updates `users.active` and syncs `guardians.is_login_active`.
- Bulk activate validates all IDs belong to the tenant.

## Workflow

1. Deactivate a parent login (from this API or related toggle).
2. Parent appears in this list.
3. Use row action or bulk **Authentication Activate** → removed from list.
