# 21 — Events

Event types (with icon) and school events with publish / website / public list.

**Headers:** `Authorization: Bearer {token}` + `X-Tenant-ID: {slug}` (except public)  
**Updates:** `PATCH` (no PUT)

**Seed:** event type `Holiday` (icon `bell`)

## Types — `/api/events/types`

CRUD; icon string (`bell`, `star`, `users`, …); cannot delete if events use it.

## Events — `/api/events`

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | Admin list |
| GET | `/public` | **AllowAnonymous** — published only |
| GET | `/{id}` | Detail |
| POST | `/` | Create (`DateOfEnd >= DateOfStart`) |
| PATCH | `/{id}` | Update |
| DELETE | `/{id}` | Soft/hard delete per service |
| POST | `/{id}/image` | MinIO `events/{eventId}/...` |
| PATCH | `/{id}/publish` | Toggle `isPublished` |
| PATCH | `/{id}/show-website` | Toggle `showWebsite` |

**Audience:** Everybody | Students | Teachers | Parents | Staff

## Auth

| Role | Access |
|------|--------|
| Super Admin / Admin | Full |
| Public | `GET /api/events/public` |
