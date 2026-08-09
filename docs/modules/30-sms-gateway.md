# 30 — SMS Gateway (BulkSMSBD.net)

Per-tenant SMS gateway configuration and event-triggered templates. **Default
gateway is BulkSMSBD.net** (`bulksmsbd`).

**Headers:** Bearer + `X-Tenant-ID`  
**Permission:** `Settings.SmsSettings` (`View` / `Edit`)  
**Updates:** `PATCH` (no PUT)  
**Base path:** `/api/settings/school/{tenantSlug}`

Related: [28-settings.md](./28-settings.md) · [29-email-gateway.md](./29-email-gateway.md)

---

## Overview

| Piece | Detail |
|-------|--------|
| Default gateway | `bulksmsbd` (BulkSMSBD.net) |
| Credentials | JSON: `api_key`, `senderid` |
| Single send | `GET http://bulksmsbd.net/api/smsapi` |
| Many send | `POST http://bulksmsbd.net/api/smsapimany` |
| Balance | `GET http://bulksmsbd.net/api/getBalanceApi` |
| Success code | `202` — SMS Submitted Successfully |
| Renderer | Same `INotificationTemplateService` as email (`{placeholder}`) |

Tables: `sms_settings` (single row), `sms_templates` (unique `event_key`).  
Implementation: `BulkSmsBdSmsSender` via `ISmsSenderFactory`.

Other gateway keys (`twilio`, `textlocal`, …) remain selectable but use a logging
stub until implemented.

---

## Routes

| Method | Path | Notes |
|--------|------|-------|
| GET | `…/sms-config` | Active gateway + credentials map + available gateways |
| PATCH | `…/sms-config` | Enable/disable, set gateway + credentials |
| PATCH | `…/sms-config/{gateway}` | Update credentials for one gateway (also sets it active) |
| POST | `…/sms-config/test` | Send test SMS `{ "to", "message?" }` |
| GET | `…/sms-config/balance` | Credit balance (BulkSMSBD only) |
| GET | `…/sms-triggers` | All event keys with saved values (or defaults) |
| PATCH | `…/sms-triggers/{eventKey}` | Upsert body / notify flags / DLT id |

---

## Configure BulkSMSBD

```http
PATCH /api/settings/school/{slug}/sms-config
Authorization: Bearer {token}
X-Tenant-ID: {slug}

{
  "isEnabled": true,
  "activatedGateway": "bulksmsbd",
  "credentials": {
    "api_key": "YOUR_API_KEY",
    "senderid": "8809617625860"
  }
}
```

Credential keys accepted (case-insensitive): `api_key` / `ApiKey`, `senderid` / `SenderId`.

Or gateway-specific:

```http
PATCH /api/settings/school/{slug}/sms-config/bulksmsbd
{
  "api_key": "YOUR_API_KEY",
  "senderid": "8809617625860"
}
```

Store keys only in SMS settings — never commit them to source control.

---

## BulkSMSBD API mapping

### Single SMS (`/api/smsapi`)

Query parameters:

| Param | Required | Notes |
|-------|----------|-------|
| `api_key` | Yes | From credentials |
| `senderid` | Yes | Approved sender ID |
| `number` | Yes | One or more numbers, comma-separated; BD numbers normalized to `8801…` |
| `message` | Yes | URL-encoded; avoid single quotes `'` |
| `type` | No | Sent as `text` |

### Many SMS (`/api/smsapimany`)

```json
{
  "api_key": "…",
  "senderid": "…",
  "messages": [
    { "to": "01711111111", "message": "My Text 1" },
    { "to": "01811111111", "message": "My Text 2" }
  ]
}
```

### Balance (`/api/getBalanceApi`)

```
GET http://bulksmsbd.net/api/getBalanceApi?api_key=…
```

### Response codes (subset)

| Code | Meaning |
|------|---------|
| 202 | SMS Submitted Successfully |
| 1001 | Invalid Number |
| 1002 | Sender ID incorrect / disabled |
| 1003 | Required fields missing |
| 1005 | Internal Error |
| 1006 | Balance Validity Not Available |
| 1007 | Balance Insufficient |
| 1011 | User Id not found |
| 1031 | Account not verified |
| 1032 | IP not whitelisted |

Full list is mapped in `BulkSmsBdSmsSender.ExplainCode`.

---

## Test & balance

```http
POST /api/settings/school/{slug}/sms-config/test
{
  "to": "017XXXXXXXX",
  "message": "Test SMS from School Management"
}

GET /api/settings/school/{slug}/sms-config/balance
```

Test requires `isEnabled: true` and valid credentials. Numbers like `017…` /
`01…` are normalized to `88017…` when possible.

---

## SMS trigger event keys

| EventKey | Typical use | Admission placeholders |
|----------|-------------|------------------------|
| `Admission` | New admission | `{name}`, `{class}`, `{section}`, `{admission_date}`, `{roll}`, `{register_no}` |
| `FeeCollection` | Fee paid | |
| `FeesReminder` | Due fees | |
| `Attendance` | Attendance alert | |
| `ExamAttendance` | Exam attendance | |
| `ExamResults` | Results published | |
| `Homework` | Homework assigned | |
| `LiveClass` | Live class starting | |
| `OnlineExamPublish` | Online exam live | |
| `StudentBirthdayWishes` | Student birthday | |
| `StaffBirthdayWishes` | Staff birthday | |
| `AlumniEvent` | Alumni event | |

### Trigger fields

| Field | Notes |
|-------|-------|
| `notifyEnabled` | Master on/off for the event |
| `notifyStudent` / `notifyParent` | Audience flags |
| `dltTemplateId` | Optional (Indian DLT gateways) |
| `body` | Plain text, max **918** characters |

Also available globally: `{institute_name}` via the shared template renderer.

### Update trigger example

```http
PATCH /api/settings/school/{slug}/sms-triggers/Admission
{
  "notifyEnabled": true,
  "notifyStudent": false,
  "notifyParent": true,
  "body": "Dear {name}, admitted to {class}-{section}. Roll: {roll}, Reg: {register_no}."
}
```

---

## Number normalization

| Input | Normalized |
|-------|------------|
| `017XXXXXXXX` (11 digits) | `88017XXXXXXXX` |
| `17XXXXXXXX` (10 digits) | `88017XXXXXXXX` |
| `88017XXXXXXXX` | unchanged |

---

## Implementation notes

- Service: `SmsSettingsService`  
- Sender: `SchoolManagement.BLL.Services.Sms.BulkSmsBdSmsSender`  
- Factory: `ISmsSenderFactory` → `BulkSmsBdSmsSender` when gateway is `bulksmsbd`  
- Named `HttpClient`: `BulkSmsBd` (30s timeout)  
- Cron job `/cron_api/send_smsemail_command/{secretKey}` will use the same factory when the SMS/email queue is wired
