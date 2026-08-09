# 24 — Messages / Mailbox

Internal messaging: inbox, sent, important, trash, reply, attachments.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}`  
**Updates:** `PATCH` (no PUT)

**Migration:** `20260809184708_AddMessageAndSettings`  
**Provisioner:** `EnsureMessageAndSettingsModuleAsync`

## Routes (`/api/messages`)

| Method | Path | Notes |
|--------|------|-------|
| GET | `/inbox` | Received, unread first |
| GET | `/sent` | Sent by current user |
| GET | `/important` | Starred |
| GET | `/trash` | Soft-deleted |
| GET | `/{id}` | Detail; marks read |
| POST | `/compose` | Send to one recipient |
| POST | `/{id}/reply` | Subject prefixed `Re:` |
| POST | `/{id}/attachment` | MinIO `messages/{id}/...` |
| PATCH | `/{id}/important` | Toggle star |
| DELETE | `/{id}` | Soft delete → trash |
| POST | `/{id}/restore` | From trash |
| DELETE | `/{id}/permanent` | Hard delete (must be in trash) |
| GET | `/unread-count` | Badge count |
| GET | `/lookup/recipients` | By role (+ class for students) |

## Auth

Any authenticated user — own messages only
