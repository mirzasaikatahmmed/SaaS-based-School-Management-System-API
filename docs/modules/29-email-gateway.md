# 29 — Email Gateway (SMTP)

Per-tenant SMTP email configuration and event-triggered templates. Used for
account, leave, payslip, and other notification emails.

**Headers:** Bearer + `X-Tenant-ID`  
**Permission:** `Settings.EmailSettings` (`View` / `Edit`)  
**Updates:** `PATCH` (no PUT)  
**Base path:** `/api/settings/school/{tenantSlug}`

Related: [28-settings.md](./28-settings.md) · [30-sms-gateway.md](./30-sms-gateway.md)

---

## Overview

| Piece | Detail |
|-------|--------|
| Protocol | SMTP only (v1) |
| Password | Encrypted at rest (ASP.NET Data Protection) |
| Templates | Per `EventKey` with HTML body + subject |
| Placeholders | `{institute_name}`, `{name}`, `{login_username}`, `{password}`, `{user_role}`, `{login_url}` |
| Renderer | `INotificationTemplateService.Render(template, dataDict)` |

Tables: `email_settings` (single row), `email_templates` (unique `event_key`).

---

## Routes

| Method | Path | Notes |
|--------|------|-------|
| GET | `…/email-config` | Current SMTP config (`HasPassword` flag; password never returned) |
| PATCH | `…/email-config` | Save SMTP settings |
| POST | `…/email-config/test` | Send a test email `{ "to": "user@example.com" }` |
| GET | `…/email-triggers` | All event keys with saved values (or defaults) |
| PATCH | `…/email-triggers/{eventKey}` | Upsert subject / body / notify flag |

---

## SMTP config payload

```json
{
  "isEnabled": true,
  "systemEmail": "noreply@school.edu.bd",
  "protocol": "SMTP",
  "smtpHost": "smtp.example.com",
  "smtpPort": 587,
  "smtpUsername": "noreply@school.edu.bd",
  "smtpPassword": "••••••••",
  "smtpSecure": "TLS",
  "smtpAuth": true,
  "fromName": "Adarsha High School"
}
```

| Field | Values / notes |
|-------|----------------|
| `protocol` | `SMTP` |
| `smtpSecure` | `None` \| `SSL` \| `TLS` |
| `smtpAuth` | Use username/password when `true` |
| `smtpPassword` | Omit on PATCH to keep the existing encrypted password |

---

## Email trigger event keys

| EventKey | Default purpose |
|----------|-----------------|
| `AccountRegistered` | New account welcome |
| `ForgotPassword` | Password reset link |
| `ChangePassword` | Password changed notice |
| `NewMessageReceived` | Mailbox notification |
| `PayslipGenerated` | Payslip ready |
| `Award` | Award notification |
| `LeaveApprove` | Leave approved |
| `LeaveReject` | Leave rejected |
| `AdvanceSalaryReject` | Advance salary rejected |

### Update trigger example

```http
PATCH /api/settings/school/{slug}/email-triggers/ForgotPassword
Authorization: Bearer {token}
X-Tenant-ID: {slug}

{
  "notifyEnabled": true,
  "subject": "Password reset — {institute_name}",
  "bodyHtml": "<p>Hello {name},</p><p>Reset: {login_url}</p>"
}
```

---

## Test email

```http
POST /api/settings/school/{slug}/email-config/test
{ "to": "admin@school.edu.bd" }
```

Requires `smtpHost` + `systemEmail` configured. Uses `System.Net.Mail.SmtpClient`
with SSL enabled unless `smtpSecure` is `None`.

---

## Implementation notes

- Service: `EmailSettingsService`  
- Entities: `EmailSettings`, `EmailTemplate`, `NotificationEventKeys.EmailDefaults`  
- SuperAdmin / School Admin only (plus `[AuthorizePermission]`)  
- Path `{tenantSlug}` must match the school you manage; school admins also need matching `X-Tenant-ID`
