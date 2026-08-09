# 25 — Global Settings

Platform-wide settings in `public.global_settings` (single row).

**Auth:** Super Admin only (no `X-Tenant-ID` required)  
**Updates:** `PATCH` (no PUT)

**Master migration:** `20260809184728_AddGlobalSettings`  
**Seed:** one default row on API startup

## Routes (`/api/settings/global`)

| Method | Path | Notes |
|--------|------|-------|
| GET | `/` | Full global settings |
| PATCH | `/` | General settings tab |
| PATCH | `/upload-file` | Image/file extension + size limits |

Defaults include currency BDT/৳, timezone Asia/Dhaka, academic session, social URLs, upload limits (2048 KB).
