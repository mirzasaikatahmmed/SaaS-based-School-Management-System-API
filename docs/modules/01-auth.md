# 1 — Authentication

**Controller:** `AuthController` · **Route:** `/api/auth`

Custom JWT auth (no ASP.NET Identity). Passwords hashed with BCrypt; `$2y$` (PHP) hashes are verified for compatibility.

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/login` | Public | Login (Super Admin: no tenant header; school users: send `X-Tenant-ID`) |
| POST | `/api/auth/register` | Varies | Register (tenant-scoped where applicable) |
| POST | `/api/auth/refresh-token` | Public | Rotate access token |
| POST | `/api/auth/revoke-token` | Bearer | Revoke refresh token |
| GET | `/api/auth/me` | Bearer | Current user profile |
| PATCH | `/api/auth/me` | Bearer | Update current profile (school users only) |

## Workflow

1. Super Admin logs in without `X-Tenant-ID`.
2. After a school exists, school users login with `X-Tenant-ID: {slug}`.
3. Use access token on subsequent calls; refresh when expired.

## Notes

- Access token ~15 minutes; refresh ~7 days.
- School-user refresh tokens are stored per tenant schema. Super Admin refresh tokens are stored on `public.super_admins` and use the `sa:{id}:{token}` format. Refresh does **not** need `X-Tenant-ID` for Super Admin.
- `POST /api/auth/register` is tenant-scoped, requires `AllowSelfRegistration`, and may only create **student** or **parent** accounts.
- Role claim uses ahskbera **prefix** values (`admin`, `teacher`, `parent`, …).
- Updates use **PATCH** (no PUT).
