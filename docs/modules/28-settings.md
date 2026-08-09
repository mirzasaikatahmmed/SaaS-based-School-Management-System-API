# 28 — Settings Module (Roles, Sessions, Cron, Backup, Login Log, Email/SMS)

Full Settings area: role permission matrix, academic sessions, cron secret jobs,
database backup/restore, user login log, and school-settings sub-pages
(attendance type, accounting links, email, SMS).

**Headers:** Bearer + `X-Tenant-ID` (except anonymous `/cron_api/*`)
**Updates:** `PATCH` (no PUT)
**Auth filter:** `[AuthorizePermission("FeatureKey", "View"|"Add"|"Edit"|"Delete")]`
**Provisioner:** `EnsureSettingsModuleAsync` (chains from `EnsureBiometricModuleAsync`)

Manual DTO mapping (no AutoMapper). System roles are non-deletable; Admin with an
empty permission matrix is treated as full access until first save.

## Migrations

| Context | Migration |
|---------|-----------|
| Tenant | `20260809202445_AddSettingsModule` |
| Master | `20260809202459_AddCronSecretRegistry` |

## 1. Roles & permissions — `/api/roles`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | List roles (`IsSystemRole`, `IsActive`) |
| POST | `/` | Create custom role |
| PATCH | `/{id}` | Update name / description / active |
| DELETE | `/{id}` | Blocked for system roles or roles with users |
| GET | `/{id}/permissions` | Full feature matrix (defaults false) |
| PATCH | `/{id}/permissions` | Bulk upsert in one transaction |

Feature catalog: `AppFeatures.All` in Common (Dashboard → Messages).

## 2. Academic sessions — `/api/sessions`

| Method | Path |
|--------|------|
| GET | `/` |
| GET | `/current` |
| POST | `/` |
| PATCH | `/{id}` |
| DELETE | `/{id}` (conflict if students exist for that year name) |

Only one session may have `IsSelected = true` (cleared in the same transaction).

## 3. Cron — `/api/settings/cron` + `/cron_api/*`

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/settings/cron` | JWT + permission |
| POST | `/api/settings/cron/regenerate-key` | JWT + permission |
| GET | `/cron_api/send_smsemail_command/{secretKey}` | Secret key only |
| GET | `/cron_api/homework_command/{secretKey}` | Secret key only |
| GET | `/cron_api/fees_reminder_command/{secretKey}` | Secret key only |

Cron responses are plain JSON (`CronJobResultDto`), not `ApiResponse<T>`.
Master `cron_secret_registry` resolves tenant schema without `X-Tenant-ID`.
Fees reminder is idempotent via `notification_dispatch_log` (job + entity + date).

## 4. Database backup — `/api/settings/backup`

| Method | Path |
|--------|------|
| GET | `/` (paged) |
| POST | `/create` (`pg_dump` → zip → MinIO `db-backups/`) |
| GET | `/{id}/download` (presigned URL) |
| DELETE | `/{id}` |
| POST | `/restore` (multipart `.sql`/`.zip`; safety backup first) |

## 5. User login log — `/api/settings/login-log`

| Method | Path |
|--------|------|
| GET | `/?type=staff\|student\|parent&search=&page=&pageSize=&export=csv` |
| DELETE | `/clear` |

Written automatically on successful tenant login (`Browser` / `Platform` from User-Agent).

## 6–9. School settings sub-pages — `/api/settings/school/{tenantSlug}/…`

| Path | Purpose |
|------|---------|
| `…/attendance-type` | GET/PATCH `DayWise` \| `SubjectWise` (surfaced on student attendance GET) |
| `…/accounting-links` | Default deposit/expense account FKs + enabled flag |
| `…/email-config` | SMTP config (password encrypted via Data Protection) — see [29-email-gateway.md](./29-email-gateway.md) |
| `…/email-config/test` | Send test email |
| `…/email-triggers` | List / PATCH `{eventKey}` |
| `…/sms-config` | Activated gateway + credentials JSON (`api_key`, `senderid` for BulkSMSBD) — see [30-sms-gateway.md](./30-sms-gateway.md) |
| `…/sms-config/{gateway}` | Gateway-specific credentials |
| `…/sms-config/test` | POST test SMS (`{ to, message? }`) via BulkSMSBD `/api/smsapi` |
| `…/sms-config/balance` | GET credit balance via BulkSMSBD `/api/getBalanceApi` |
| `…/sms-triggers` | List / PATCH `{eventKey}` |

**Default SMS gateway:** `bulksmsbd` (BulkSMSBD.net). Single send uses GET `http://bulksmsbd.net/api/smsapi`; multi-send uses POST `/api/smsapimany`. Response code `202` = success.

Shared: `INotificationTemplateService.Render(template, data)` for `{placeholder}` merge codes.
`ISmsSenderFactory` resolves `BulkSmsBdSmsSender` for `bulksmsbd`; other gateways remain logging stubs.
